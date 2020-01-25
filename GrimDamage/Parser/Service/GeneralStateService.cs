﻿using GrimDamage.Parser.Model;
using GrimDamage.Settings;
using GrimDamage.Statistics.dto;
using GrimDamage.Utility;
using log4net;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GrimDamage.Parser.Service
{
    class GeneralStateService {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GeneralStateService));
        private ConcurrentBag<GrimDawnStateEventJson> _states = new ConcurrentBag<GrimDawnStateEventJson>();
        private readonly AppSettings _appSettings;

        public GeneralStateService(AppSettings settings) {
            _appSettings = settings;
        }

        public void PushState(GrimState state) {
            _states.Add(new GrimDawnStateEventJson {
                Event = state.ToString(),
                Timestamp = Timestamp.UtcMillisecondsNow
            });
            if (_appSettings.LogStateChanges) {
                Logger.Debug($"GD State has been set to \"{state}\"");
            }
        }

        public List<GrimDawnStateEventJson> GetStates(long timestamp) {
            return _states.Where(m => m.Timestamp > timestamp).OrderByDescending(m => m.Timestamp).ToList();
        }
    }
}
