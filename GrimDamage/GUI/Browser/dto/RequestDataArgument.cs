using System;

namespace GrimDamage.GUI.Browser.dto
{
    class RequestDataArgument : EventArgs {
        public DataRequestType Type { get; set; }
        public string TimestampStart { get; set; }
        public string TimestampEnd { get; set; }
        public string Callback { get; set; }

        public int EntityId { get; set; }
    }
}
