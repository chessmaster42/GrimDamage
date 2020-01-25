using GrimDamage.GUI.Browser;
using System;
using System.Windows.Forms;

namespace GrimDamage.GUI.Forms
{
    public partial class WebView : Form {
        private readonly CefBrowserHandler _browser;
        public WebView(CefBrowserHandler browser) {
            InitializeComponent();
            _browser = browser;
        }

        private void WebView_Load(object sender, EventArgs e) {
            Dock = DockStyle.Fill;
            Controls.Add(_browser.BrowserControl);
        }
    }
}
