using CefSharp;
using CefSharp.Handler;
using System.Diagnostics;

namespace GrimDamage.GUI.Browser
{
    // https://github.com/cefsharp/CefSharp/blob/master/CefSharp.Example/Handlers/ExampleRequestHandler.cs
    class DisableLinksRequestHandler : RequestHandler
    {
        protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            if (request.Url.StartsWith("discord://"))
            {
                Process.Start("https://discord.gg/PJ87Ewa");
                return true;
            }
            if (request.Url.StartsWith("relics://"))
            {
                Process.Start("http://items.dreamcrash.org/ComponentAssembler?record=d106_relic.dbr ");
                return true;
            }
            return request.Url.StartsWith("http:");
        }
    }
}
