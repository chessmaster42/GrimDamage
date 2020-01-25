using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace DllInjector
{
    /// <summary>
    /// 
    /// </summary>
    public class InjectionHelper : IDisposable
    {
        private class RunArguments
        {
            public string WindowName;
            public string ClassName;
            public string DllName;
        }

        private static readonly ILog Logger = LogManager.GetLogger(typeof(InjectionHelper));

        private const int InjectionError = 0;
        private const int NoProcessFoundOnStartup = 1;
        private const int NoProcessFound = 2;

        private readonly HashSet<uint> _previouslyInjected = new HashSet<uint>();
        private readonly HashSet<uint> _dontLog = new HashSet<uint>();
        private readonly Dictionary<uint, IntPtr> _pidModuleHandleMap = new Dictionary<uint, IntPtr>();

        private readonly ProgressChangedEventHandler _registeredProgressCallback;
        private readonly bool _unloadOnExit;
        private readonly RunArguments _exitArguments;

        private BackgroundWorker _backgroundWorker;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="backgroundWorker"></param>
        /// <param name="progressChanged">Callback for errors and notifications</param>
        /// <param name="unloadOnExit">Whatever this DLL should get unloaded on exit</param>
        /// <param name="windowName">Name of the window you wish to inject to</param>
        /// <param name="className">Name of the class you wish to inject to (IFF window name is empty/null)</param>
        /// <param name="dll">Name of the DLL you wish to inject</param>
        public InjectionHelper(BackgroundWorker backgroundWorker, ProgressChangedEventHandler progressChanged, bool unloadOnExit, string windowName, string className, string dll)
        {
            if (string.IsNullOrEmpty(windowName) && string.IsNullOrEmpty(className))
            {
                throw new ArgumentException("Either window or class name must be specified");
            }
            if (string.IsNullOrEmpty(dll))
            {
                throw new ArgumentException("DLL name must be specified");
            }

            _backgroundWorker = backgroundWorker;
            _registeredProgressCallback = progressChanged;
            _unloadOnExit = unloadOnExit;
            _exitArguments = new RunArguments
            {
                WindowName = windowName,
                ClassName = className,
                DllName = dll
            };

            _backgroundWorker.DoWork += DoWork;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.ProgressChanged += progressChanged;
            _backgroundWorker.RunWorkerAsync(_exitArguments);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (_backgroundWorker != null)
            {
                _backgroundWorker.ProgressChanged -= _registeredProgressCallback;
                _backgroundWorker.CancelAsync();
                _backgroundWorker = null;
            }

            if (_unloadOnExit)
            {
                // Unload the DLL from any still running instance
                HashSet<uint> pids = FindProcesses(_exitArguments);
                foreach (uint pid in _pidModuleHandleMap.Keys)
                {
                    if (!pids.Contains(pid))
                    {
                        continue;
                    }

                    Logger.Info(DllInjector.UnloadDll(pid, _pidModuleHandleMap[pid]) ? $"Unloaded module from pid {pid}" : $"Failed to unload module from pid {pid}");
                }
            }
            else
            {
                Logger.Info("Exiting without unloading DLL (as per configuration)");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void DoWork(object sender, DoWorkEventArgs eventArgs)
        {
            try
            {
                BackgroundWorker worker = sender as BackgroundWorker;

                if (Thread.CurrentThread.Name == null)
                {
                    Thread.CurrentThread.Name = "InjectionHelper";
                }

                while (worker != null && !worker.CancellationPending)
                {
                    Process(worker, eventArgs.Argument as RunArguments);
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                Logger.Fatal(e.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private HashSet<uint> FindProcesses(RunArguments args)
        {
            if (!string.IsNullOrEmpty(args.WindowName))
            {
                return DllInjector.FindProcessForWindow(args.WindowName);
            }

            if (!string.IsNullOrEmpty(args.ClassName))
            {
                throw new NotSupportedException("Class name provided instead of window name, not yet implemented.");
            }

            return new HashSet<uint>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="arguments"></param>
        private void Process(BackgroundWorker worker, RunArguments arguments)
        {
            Thread.Sleep(1200);

            HashSet<uint> pids = FindProcesses(arguments);
                        
            if (pids.Count == 0 && _previouslyInjected.Count == 0)
                worker.ReportProgress(NoProcessFoundOnStartup, null);
            else if (pids.Count == 0)
                worker.ReportProgress(NoProcessFound, null);

            string dll = Path.Combine(Directory.GetCurrentDirectory(), arguments.DllName);
            if (!File.Exists(dll))
            {
                Logger.Fatal($"Could not find {arguments.DllName} at \"{dll}\"");
                return;
            }

            foreach (uint pid in pids)
            {
                if (_previouslyInjected.Contains(pid))
                {
                    continue;
                }

                IntPtr remoteModuleHandle = DllInjector.NewInject(pid, dll);
                if (remoteModuleHandle == IntPtr.Zero)
                {
                    if (!_dontLog.Contains(pid))
                    {
                        Logger.Warn($"Could not inject dll into process {pid}, if this is a recurring issue, try running as administrator.");
                        worker.ReportProgress(InjectionError, $"Could not inject dll into process {pid}");
                    }

                    continue;
                }

                if (!_dontLog.Contains(pid))
                    Logger.Info("Injected dll into process " + pid);

                if (!InjectionVerifier.VerifyInjection(pid, arguments.DllName))
                {
                    if (!_dontLog.Contains(pid))
                    {
                        Logger.Warn("InjectionVerifier reports injection failed.");
                        worker.ReportProgress(InjectionError, $"InjectionVerifier reports injection failed into PID {pid}, try running as administrator.");
                    }

                    _dontLog.Add(pid);

                    continue;
                }

                Logger.Info("InjectionVerifier reports injection succeeded.");
                _previouslyInjected.Add(pid);
                _pidModuleHandleMap[pid] = remoteModuleHandle;
            }
        }
    }
}
