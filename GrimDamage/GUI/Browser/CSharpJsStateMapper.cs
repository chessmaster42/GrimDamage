﻿using GrimDamage.Parser.Service;
using GrimDamage.Statistics.Service;
using log4net;
using Newtonsoft.Json;

namespace GrimDamage.GUI.Browser
{
    class CSharpJsStateMapper {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CSharpJsStateMapper));
        private readonly CefBrowserHandler _browser;
        private readonly StatisticsService _statisticsService;
        private readonly GeneralStateService _generalStateService;
        private readonly JsonSerializerSettings _settings;
        private readonly PositionTrackerService _positionTrackerService;

        public CSharpJsStateMapper(CefBrowserHandler browser, StatisticsService statisticsService, GeneralStateService generalStateService, PositionTrackerService positionTrackerService) {
            _browser = browser;
            _statisticsService = statisticsService;
            _generalStateService = generalStateService;
            _positionTrackerService = positionTrackerService;
            _settings = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Culture = System.Globalization.CultureInfo.InvariantCulture,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
        }

        private string Serialize(object value) {
            return JsonConvert.SerializeObject(value, _settings);
        }

        public void RequestData(DataRequestType data, long start, long end, int entityId, string callback) {
            switch (data) {
                case DataRequestType.States:
                    TransferStates(start, callback);
                    break;

                case DataRequestType.FetchEntities:
                    TransferEntities(callback);
                    break;

                case DataRequestType.FetchLocations:
                    TransferLocations(start, end, callback);
                    break;

                case DataRequestType.DetailedDamageTaken:
                    if (entityId > 0) {
                        TransferDetailedDamageTaken(entityId, start, end, callback);
                    }
                    else {
                        Logger.Warn($"Data request for {data} was not handled due to the entityId being <0.");
                    }
                    break;

                case DataRequestType.FetchResists:
                    if (entityId > 0) {
                        TransferResists(entityId, start, end, callback);
                    }
                    else {
                        Logger.Warn($"Data request for {data} was not handled due to the entityId being <0.");
                    }
                    break;

                case DataRequestType.DetailedDamageDealt:
                    if (entityId > 0) {
                        TransferDetailedDamageDealt(entityId, start, end, callback);
                    }
                    else {
                        Logger.Warn($"Data request for {data} was not handled due to the entityId being <0.");
                    }
                    break;

                case DataRequestType.SimpleDamageDealt:
                    if (entityId > 0) {
                        TransferSimpleDamageDealt(entityId, start, end, callback);
                    }
                    else {
                        Logger.Warn($"Data request for {data} was not handled due to the entityId being <0.");
                    }
                    break;

                case DataRequestType.SimpleDamageTaken:
                    if (entityId > 0) {
                        TransferSimpleDamageTaken(entityId, start, end, callback);
                    }
                    else {
                        Logger.Warn($"Data request for {data} was not handled due to the entityId being <0.");
                    }
                    break;

                case DataRequestType.EntityHealth:
                    if (entityId > 0) {
                        TransferHealth(entityId, start, end, callback);
                    }
                    else {
                        Logger.Warn($"Data request for {data} was not handled due to the entityId being <0.");
                    }
                    break;

                    case DataRequestType.FetchPetDamageDealt:
                        TransferPetDamageDealt(start, end, callback);
                    break;

                default:
                    Logger.Warn($"Data request for {data} was not handled, unknown type.");
                    break;
            }
        }
        
        private void TransferHealth(int entityId, long start, long end, string callback) {
            _browser.JsCallback(callback, Serialize(_statisticsService.GetEntityHealth(entityId, start, end)));
        }

        private void TransferSimpleDamageDealt(int entityId, long start, long end, string callback) {
            _browser.JsCallback(callback, Serialize(_statisticsService.GetSimpleDamageDealt(entityId, start, end)));
        }

        private void TransferPetDamageDealt(long start, long end, string callback) {
            _browser.JsCallback(callback, Serialize(_statisticsService.GetSimpleDamageDealtForPets(start, end)));
        }
        
        private void TransferSimpleDamageTaken(int entityId, long start, long end, string callback) {
            _browser.JsCallback(callback, Serialize(_statisticsService.GetSimpleDamageTaken(entityId, start, end)));
        }

        private void TransferDetailedDamageDealt(int entityId, long start, long end, string callback) {
            _browser.JsCallback(callback, Serialize(_statisticsService.GetDetailedLatestDamageDealt(entityId, start, end)));
        }
        
        private void TransferResists(int entityId, long start, long end, string callback) {
            _browser.JsCallback(callback, Serialize(_statisticsService.GetResists(entityId, start, end)));
        }
        private void TransferDetailedDamageTaken(int entityId, long start, long end, string callback) {
            _browser.JsCallback(callback, Serialize(_statisticsService.GetDetailedLatestDamageTaken(entityId, start, end)));
        }
        
        private void TransferLocations(long start, long end, string callback) {
            _browser.JsCallback(callback, Serialize(_positionTrackerService.GetLocations(start, end)));
        }
        private void TransferEntities(string callback) {
            _browser.JsCallback(callback, Serialize(_statisticsService.GetEntities()));
        }
        
        private void TransferStates(long timestamp, string callback) {
            _browser.JsCallback(callback, Serialize(_generalStateService.GetStates(timestamp)));
        }
    }
}
