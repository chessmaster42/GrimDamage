using GrimDamage.Parser.Model;
using GrimDamage.Statistics.dto;
using GrimDamage.Tracking.Model;
using GrimDamage.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GrimDamage.GUI.Browser
{
    public class WebViewJsInteractor
    {
        private readonly JsonSerializerSettings _settings;

        public WebViewJsInteractor(WebViewJsPojo js)
        {
            _settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Culture = System.Globalization.CultureInfo.InvariantCulture,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            List<string> documentation = new List<string>();
            documentation.Add("Example values for state changes:");
            documentation.Add(
                Serialize(new List<GrimDawnStateEventJson> { new GrimDawnStateEventJson { Event = GrimState.Dead.ToString(), Timestamp = Timestamp.UtcMillisecondsNow } }
            ));
            documentation.Add("");

            documentation.Add("The possible values for state are:");
            documentation.Add(Serialize(Enum.GetValues(typeof(GrimState)).Cast<GrimState>().Select(m => m.ToString())));
            documentation.Add("");


            documentation.Add("The possible values for entity type are:");
            documentation.Add(Serialize(Enum.GetValues(typeof(EntityType)).Cast<EntityType>().Select(m => m.ToString())));
            documentation.Add("");


            documentation.Add("The possible values for damage type are:");
            documentation.Add(Serialize(Enum.GetValues(typeof(DamageType)).Cast<DamageType>().Select(m => m.ToString())));
            documentation.Add("");


            documentation.Add("\r\n\r\nThe following methods are exposed:");
            MethodInfo[] methodInfos = typeof(WebViewJsPojo).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methodInfos) {
                var parameterDescriptions = string.Join
                (", ", method.GetParameters()
                    .Select(x => x.ParameterType + " " + x.Name)
                    .ToArray());

                if (!method.Name.Contains('_') && !"ToString".Equals(method.Name) && !"Equals".Equals(method.Name) && !"GetHashCode".Equals(method.Name) && !"GetType".Equals(method.Name)) {
                    documentation.Add($"{method.ReturnType} {method.Name}({parameterDescriptions})");
                }
            }

            documentation.Add("\r\n=== END OF DOCUMENTATION ===");

            js.api = string.Join("\r\n\t", documentation);
        }

        private string Serialize(object o) {
            return JsonConvert.SerializeObject(o, _settings);
        }
    }
}
