﻿using CefSharp;
using CefSharp.WinForms;
using log4net;
using System;
using System.IO;
using System.Windows.Forms;

namespace GrimDamage.GUI.Browser
{
    public class CefBrowserHandler : IDisposable {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CefBrowserHandler));
        private ChromiumWebBrowser _browser;

        public WebViewJsPojo JsPojo { get; private set; }
        public WebViewJsInteractor JsInteractor { get; private set; }

        public Control BrowserControl => _browser;

        private object _lockObj = new object();

        ~CefBrowserHandler() {
            Dispose();
        }

        public void Dispose() {
            lock (_lockObj) {
                if (_browser != null) {
                    CefSharpSettings.WcfTimeout = TimeSpan.Zero;
                    _browser.Dispose();

                    Cef.Shutdown();
                    _browser = null;
                }
            }
        }

        public void ShowDevTools() {
            if (_browser.IsBrowserInitialized) {
                _browser.ShowDevTools();
            }
            else {
                MessageBox.Show("Chill the fuck out\nChromium is still initializing.", "Chill, man",
                    MessageBoxButtons.OK);
            }
        }

        public void NotifyUpdate() {
            if (_browser.IsBrowserInitialized)
                _browser.ExecuteScriptAsync("_itemsReceived();");
        }

        public void JsCallback(string method, string json) {
            if (_browser.IsBrowserInitialized)
                _browser.ExecuteScriptAsync($"{method}({json});");
        }
        
        public void TransferSave(string data) {
            if (_browser.IsBrowserInitialized) {
                _browser.ExecuteScriptAsync($"_saveReceived({data});");
            }
        }

        public void InitializeChromium(
            string startPage,
            WebViewJsPojo bindeable, 
            EventHandler isBrowserInitializedChanged
        ) {
            JsPojo = bindeable;
            JsInteractor = new WebViewJsInteractor(bindeable);

            try {
                Logger.Info("Creating Chromium instance..");

                CefSharpSettings.LegacyJavascriptBindingEnabled = true;

                Cef.EnableHighDPISupport();
                Cef.Initialize(new CefSettings());

                _browser = new ChromiumWebBrowser(startPage);
                
                _browser.RegisterJsObject("data", bindeable);
                _browser.RequestHandler = new DisableLinksRequestHandler();

                if (isBrowserInitializedChanged != null)
                    _browser.IsBrowserInitializedChanged += isBrowserInitializedChanged;

                //browser.RequestHandler = new TransferUrlHijack { TransferMethod = transferItem };
                Logger.Info("Chromium created..");
            }
            catch (FileNotFoundException ex) {
                MessageBox.Show("Error \"File Not Found\" loading Chromium, did you forget to install Visual C++ runtimes?\n\nvc_redist86 in the IA folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.Warn(ex.Message);
                Logger.Warn(ex.StackTrace);
                throw;
            }
            catch (IOException ex) {
                MessageBox.Show("Error loading Chromium, did you forget to install Visual C++ runtimes?\n\nvc_redist86 in the IA folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.Warn(ex.Message);
                Logger.Warn(ex.StackTrace);
                throw;
            }
            catch (Exception ex) {
                MessageBox.Show("Unknown error loading Chromium, please see log file for more information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.Warn(ex.Message);
                Logger.Warn(ex.StackTrace);
                throw;
            }
        }
    }
}
