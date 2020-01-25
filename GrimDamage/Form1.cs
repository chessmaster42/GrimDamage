using GrimDamage.GD.Processors;
using GrimDamage.GUI.Browser;
using GrimDamage.GUI.Browser.dto;
using GrimDamage.GUI.Forms;
using GrimDamage.Parser.Service;
using GrimDamage.Settings;
using GrimDamage.Statistics.Service;
using log4net;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace GrimDamage
{
    public partial class Form1 : Form {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Form1));
        private FormWindowState _previousWindowState = FormWindowState.Normal;
        private System.Timers.Timer _timerReportUsage;

        private readonly DamageParsingService _damageParsingService = new DamageParsingService();
        private readonly CefBrowserHandler _browser;
        private readonly MessageProcessorCore _messageProcessorCore;
        private readonly PositionTrackerService _positionTrackerService = new PositionTrackerService();
        private readonly AppSettings _appSettings;

        // ReSharper disable once UnusedParameter.Local
        public Form1(CefBrowserHandler browser, AppSettings appSettings, bool showDevtools) {
            InitializeComponent();
            _browser = browser;
            _appSettings = appSettings;

            GeneralStateService generalStateService = new GeneralStateService(_appSettings);
            StatisticsService statisticsService = new StatisticsService(_damageParsingService);

            _messageProcessorCore = new MessageProcessorCore(_damageParsingService, _positionTrackerService, generalStateService, _appSettings);
            _browser.JsPojo.OnSave += JsPojoOnOnSave;
            _browser.JsPojo.OnSetLightMode += (sender, args) => {
                bool isDarkMode = (args as LightModeArgument)?.IsDarkMode ?? false;
                Properties.Settings.Default.DarkModeEnabled = isDarkMode;
                Properties.Settings.Default.Save();
            };
            _browser.JsPojo.OnLog += (sender, args) => {
                string data = (args as SaveParseArgument)?.Data;
                Logger.Warn(data);
            };

            CSharpJsStateMapper cSharpJsStateMapper = new CSharpJsStateMapper(_browser, statisticsService, generalStateService, _positionTrackerService);
            _browser.JsPojo.OnRequestData += (sender, rawArgs) => {
                RequestDataArgument args = rawArgs as RequestDataArgument;
                if (args == null)
                {
                    Logger.Error("Failed to parse data request args");
                    return;
                }
                if (long.TryParse(args.TimestampStart, out long start)) {
                    if (long.TryParse(args.TimestampEnd, out long end)) {
                        cSharpJsStateMapper.RequestData(args.Type, start, end, args.EntityId, args.Callback);
                    }
                    else {
                        Logger.Warn($"Could not parse timestamp {args.TimestampEnd} received for {args.Type}");
                    }
                }
                else {
                    Logger.Warn($"Could not parse timestamp {args.TimestampStart} received for {args.Type}");
                }
            };

#if !DEBUG
            webViewPanel.Parent.Controls.Remove(webViewPanel);
            Controls.Clear();
            if (showDevtools) {
                Controls.Add(btnShowDevtools);
            }
            Controls.Add(webViewPanel);

            bool itemAssistantInstalled = Directory.Exists(GlobalSettings.ItemAssistantFolder) || new Random().Next(10) < 8;
            if (itemAssistantInstalled) {
                webViewPanel.Location = new Point { X = 0, Y = 0 };
                webViewPanel.Width = ClientSize.Width;
                webViewPanel.Height = ClientSize.Height;
            }
            else {
                var labels = new[] {
                    "Is your stash full? Try Item Assistant!",
                    "Need a larger stash? Try Item Assistant!",
                    "Having trouble finding space for all your loot? Try Item Assistant!",
                    "Having trouble finding space for all your items? Try Item Assistant!",
                    "Need extra item storage? Try Item Assistant!",
                };
                var idx = new Random().Next(0, labels.Length);
                linkItemAssistant.Text = labels[idx];


                const int margin = 5;
                webViewPanel.Location = new Point { X = 0, Y = linkItemAssistant.Height + margin*2 };
                webViewPanel.Width = ClientSize.Width;
                webViewPanel.Height = ClientSize.Height - linkItemAssistant.Height - margin * 2;
                Controls.Add(linkItemAssistant);

            }
#else
            linkItemAssistant.Hide();
#endif

            _timerReportUsage = new System.Timers.Timer();
            _timerReportUsage.Start();
            _timerReportUsage.Elapsed += (a1, a2) => {
                if (Thread.CurrentThread.Name == null)
                    Thread.CurrentThread.Name = "CleanupThread";

                _damageParsingService.Cleanup();
            };
            _timerReportUsage.Interval = 60 * 1000 * 5;
            _timerReportUsage.AutoReset = true;
            _timerReportUsage.Start();
        }

        private void JsPojoOnOnSave(object sender, EventArgs eventArgs) {

            if (InvokeRequired) {
                Invoke((MethodInvoker) delegate { JsPojoOnOnSave(sender, eventArgs); });
            }
            else {
                SaveParseArgument args = eventArgs as SaveParseArgument;
                
                var ofd = new SaveFileDialog {
                    InitialDirectory = GlobalSettings.SavedParsePath,
                    Filter = "Damage logs (*.dmg)|*.dmg|All files (*.*)|*.*"
                };

                if (ofd.ShowDialog() == DialogResult.OK) {
                    File.WriteAllText(ofd.FileName, args?.Data);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs eventArgs) {
            Logger.Debug("Starting..");
            if (Thread.CurrentThread.Name == null) {
                Thread.CurrentThread.Name = "UI";
            }

            Closing += Form1_Closing;

            {
                WebView webView = new WebView(_browser)
                {
                    TopLevel = false
                };
                webViewPanel.Controls.Add(webView);
                webView.Show();
            }
            {
                DebugSettings debugView = new DebugSettings(_appSettings)
                {
                    TopLevel = false
                };
                panelDebugView.Controls.Add(debugView);
                debugView.Show();
            }

            _messageProcessorCore.OnHookActivation += (_, __) => {
                labelHookStatus.Text = "Hook activated";
                labelHookStatus.ForeColor = Color.Green;
            };

            FormClosing += OnFormClosing;
            SizeChanged += Form1_SizeChanged;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs formClosingEventArgs) {
            SizeChanged -= Form1_SizeChanged;
            _timerReportUsage?.Stop();
            _timerReportUsage?.Dispose();
            _timerReportUsage = null;
        }

        private void Form1_Closing(object sender, CancelEventArgs eventArgs) {
            _messageProcessorCore?.Dispose();
        }

        private void btnShowDevtools_Click(object sender, EventArgs eventArgs) {
            _browser.ShowDevTools();
        }

        private void btnLoadSave_Click(object sender, EventArgs eventArgs) {
            var ofd = new OpenFileDialog {
                InitialDirectory = GlobalSettings.SavedParsePath,
                Filter = "Damage logs (*.dmg)|*.dmg|All files (*.*)|*.*"
            };

            if (ofd.ShowDialog() == DialogResult.OK) {
                if (File.Exists(ofd.FileName)) {
                    string data = File.ReadAllText(ofd.FileName);
                    _browser.TransferSave(data);
                }
                else {
                    MessageBox.Show(
                        "Could not find the file you requested",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs eventArgs) {
            try {
                if (WindowState == FormWindowState.Minimized) {
                    Hide();
                    notifyIcon1.Visible = true;
                }
                else {
                    notifyIcon1.Visible = false;
                    _previousWindowState = WindowState;
                }
                
            }
            catch (Exception ex) {
                Logger.Warn(ex.Message);
                Logger.Warn(ex.StackTrace);
            }
        }

        private void trayContextMenuStrip_Opening(object sender, CancelEventArgs eventArgs) {
            eventArgs.Cancel = false;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs eventArgs) {

            Visible = true;
            notifyIcon1.Visible = false;
            WindowState = _previousWindowState;
        }
    }
}
