﻿using GrimDamage.GD.Dto;
using GrimDamage.Parser.Service;
using GrimDamage.Settings;
using log4net;
using System.Text;

namespace GrimDamage.GD.Processors
{
    public class DetectPlayerHitpointsProcessor : IMessageProcessor {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DetectPlayerHitpointsProcessor));
        private readonly DamageParsingService _damageParsingService;
        private readonly AppSettings _appSettings;

        public DetectPlayerHitpointsProcessor(DamageParsingService damageParsingService, AppSettings appSettings) {
            _damageParsingService = damageParsingService;
            _appSettings = appSettings;
        }

        public bool Process(MessageType type, byte[] data) {
            switch (type) {
                case MessageType.PlayerHealthOffsetDetected:
                    Logger.Info("Player health offset has been successfully detected");
                    return true;

                case MessageType.ErrorDetectingPlayerHealthOffset: {
                    Logger.Warn("Player health offset could not be detect, health graphs will be unavailable");

                    StringBuilder hex = new StringBuilder(data.Length * 2);
                    foreach (byte b in data)
                        hex.AppendFormat("{0:x2} ", b);
                    Logger.Debug(hex.ToString());

                    return true;
                }

                case MessageType.ErrorDetectingPrimaryPlayerIdOffset: 
                    {
                    Logger.Warn("Player id offset could not be detect, player id may be unavailable");

                    StringBuilder hex = new StringBuilder(data.Length * 2);
                    foreach (byte b in data)
                        hex.AppendFormat("{0:x2} ", b);
                    Logger.Debug(hex.ToString());

                    return true;
                }

                case MessageType.HitpointMonitor: 
                    {
                        int entity = IOHelper.GetInt(data, 0);
                        float hp = IOHelper.GetFloat(data, 4);
                        bool isPrimary = data[8] != 0;

                        if (_appSettings.LogEntityHitpointEvent) {
                            Logger.Info($"Entity {entity} has {hp} hitpoints, isPrimary: {isPrimary}.");
                        }

                        if (isPrimary) {
                            _damageParsingService.SetPlayerInfo(entity, true);
                        }

                        _damageParsingService.SetHealth(entity, hp);
                        return true;
                    }
            }

            return false;
        }
    }
}
