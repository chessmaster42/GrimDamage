using GrimDamage.GUI.Browser.dto;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace GrimDamage.GUI.Browser
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class WebViewJsPojo
    {
        public event EventHandler OnSave;
        public event EventHandler OnLog;
        public event EventHandler OnRequestData;
        public event EventHandler OnSetLightMode;
        
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string api { get; set; }

        public string version
        {
            get
            {

                var v = Assembly.GetExecutingAssembly().GetName().Version;

                return $"Running version {v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            }
        }

        public void save(string json)
        {
            OnSave?.Invoke(this, new SaveParseArgument
            {
                Data = json
            });
        }

        public void log(string json)
        {
            OnLog?.Invoke(this, new SaveParseArgument
            {
                Data = json
            });
        }

        public void setLightMode(string mode)
        {
            OnSetLightMode?.Invoke(this, new LightModeArgument
            {
                IsDarkMode = mode.ToLower() == "dark"
            });
        }

        public void requestData(int type, string start, string end, int id, string callback)
        {
            OnRequestData?.Invoke(this, new RequestDataArgument
            {
                Type = (DataRequestType)type,
                TimestampStart = start,
                TimestampEnd = end,
                Callback = callback,
                EntityId = id
            });
        }
    }
}
