﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrimDamage.Parser.Model;
using GrimDamage.Settings;
using log4net;

namespace GrimDamage.Parser.Service {
    class GeneralStateService {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GeneralStateService));
        private List<GrimState> _states = new List<GrimState>();
        private readonly AppSettings _appSettings;

        public GeneralStateService(AppSettings settings) {
            _appSettings = settings;
        }

        public void PushState(GrimState state) {
            _states.Add(state);
            if (_appSettings.LogStateChanges) {
                Logger.Debug($"GD State has been set to \"{state}\"");
            }
        }

        public List<GrimState> GetAndClearStates() {
            var states = _states;
            _states = new List<GrimState>();
            return states;
        }
    }
}
