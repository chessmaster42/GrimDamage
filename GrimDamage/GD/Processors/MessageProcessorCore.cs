using DllInjector;
using GrimDamage.GD.Dto;
using GrimDamage.Parser.Service;
using GrimDamage.Settings;
using GrimDamage.Statistics.Service;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace GrimDamage.GD.Processors
{
    class MessageProcessorCore : IDisposable
    {
        public delegate void HookActivationCallback(object sender, EventArgs e);

        public event HookActivationCallback OnHookActivation;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageProcessorCore));

        // ReSharper disable once NotAccessedField.Local
        private readonly RegisterWindow _window;  // Must be class field to maintain reference
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ProgressChangedEventHandler _injectorCallbackDelegate;  // Must be class field to maintain delegate reference
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<WindowMessage> _registerWindowDelegate;  // Must be class field to maintain delegate reference
        private readonly List<IMessageProcessor> _processors;
        private readonly AppSettings _appSettings;

        private bool _isFirstMessage = true;
        private InjectionHelper _injector;

        public MessageProcessorCore(
            DamageParsingService damageParsingService, 
            PositionTrackerService positionTrackerService,
            GeneralStateService generalStateService,
            AppSettings appSettings
        ) {
            _processors = new List<IMessageProcessor>
            {
                new GdLogMessageProcessor(appSettings, damageParsingService),
                new PlayerPositionTrackerProcessor(positionTrackerService, appSettings),
                new GdGameEventProcessor(generalStateService),
                new PlayerDetectionProcessor(damageParsingService, appSettings),
                new DetectPlayerHitpointsProcessor(damageParsingService, appSettings),
                new PlayerResistMonitor(damageParsingService, appSettings)
            };

            _registerWindowDelegate = CustomWndProc;
            _injectorCallbackDelegate = InjectorCallback;
            _window = new RegisterWindow("GDDamageWindowClass", _registerWindowDelegate);
            _injector = new InjectionHelper(new BackgroundWorker(), _injectorCallbackDelegate, false, "Grim Dawn", string.Empty, "Hook.dll");
            _appSettings = appSettings;
        }

        public void Dispose()
        {
            _injector?.Dispose();
            _injector = null;
        }

        private void CustomWndProc(WindowMessage message)
        {
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "Core";
            }

            if (_isFirstMessage)
            {
                Logger.Debug("Window message received");
                _isFirstMessage = false;
                OnHookActivation?.Invoke(null, null);
            }

            MessageType type = (MessageType) message.Type;
            foreach (IMessageProcessor processor in _processors)
            {
                if (processor.Process(type, message.Data))
                {
                    if (_appSettings.LogProcessedMessages)
                    {
                        Logger.Debug($"Processor {processor.GetType()} handled message type {type}");
                    }
                    return;
                }
            }

            // Message was not handled by a processor so try to handle it ourselves
            if (!HandleUnprocessedMessage(message))
            {
                Logger.Warn($"Got an unknown message of type {message.Type}");
            }
        }

        private void InjectorCallback(object sender, ProgressChangedEventArgs e)
        {
            //Logger.Debug("Injector callback");
        }

        private bool HandleUnprocessedMessage(WindowMessage message)
        {
            //Logger.Debug($"Window message type {message.Type} was not handled by a processor");

            if (message.Type == 929001)
            {
                int entityId = IOHelper.GetInt(message.Data, 0);
                float fire = IOHelper.GetFloat(message.Data, 4);
                float cold = IOHelper.GetFloat(message.Data, 8);
                float lightning = IOHelper.GetFloat(message.Data, 12);
                float poison = IOHelper.GetFloat(message.Data, 16);
                float pierce = IOHelper.GetFloat(message.Data, 20);
                float bleed = IOHelper.GetFloat(message.Data, 24);
                float vitality = IOHelper.GetFloat(message.Data, 28);
                float chaos = IOHelper.GetFloat(message.Data, 32);
                float aether = IOHelper.GetFloat(message.Data, 36);
                Logger.Debug($"CombatAttributeAccumulator::ProcessDefenseMethod() called for {entityId} with Fire:{fire}, Cold:{cold}, Lightning:{lightning}, Poison:{poison}, Pierce:{pierce}, Bleed:{bleed}, Vitality:{vitality}, Chaos:{chaos}, Aether:{aether}");
                return true;
            }

            if (message.Type == 929002)
            {
                Logger.Debug("CharacterBio::GetAllDefenseAttributes()");
                return true;
            }

            if (message.Type == 929003)
            {
                //Logger.Debug("ItemEquipment::GetAllDefenseAttributes()");
                return true;
            }

            if (message.Type == 929022)
            {
                //Logger.Debug("CharAttributeAccumulator::ExecuteDefense()");
                return true;
            }

            if (message.Type == 929222)
            {
                int ptr = IOHelper.GetInt(message.Data, 0);
                Logger.Fatal($"DECONSTRUCTION! {ptr}");
                return true;
            }

            if (message.Type == 999001)
            {
                Logger.Debug("Character::TakeAttack()");
                return true;
            }

            if (message.Type == 999002)
            {
                int entityId = IOHelper.GetInt(message.Data, 0);
                float fire = IOHelper.GetFloat(message.Data, 4);
                float cold = IOHelper.GetFloat(message.Data, 8);
                float lightning = IOHelper.GetFloat(message.Data, 12);
                float poison = IOHelper.GetFloat(message.Data, 16);
                float pierce = IOHelper.GetFloat(message.Data, 20);
                float bleed = IOHelper.GetFloat(message.Data, 24);
                float vitality = IOHelper.GetFloat(message.Data, 28);
                float chaos = IOHelper.GetFloat(message.Data, 32);
                float aether = IOHelper.GetFloat(message.Data, 36);
                Logger.Debug($"Character::GetAllDefenseAttributes called for {entityId} with Fire:{fire}, Cold:{cold}, Lightning:{lightning}, Poison:{poison}, Pierce:{pierce}, Bleed:{bleed}, Vitality:{vitality}, Chaos:{chaos}, Aether:{aether}");
                return true;
            }

            if (message.Type == 999003)
            {
                int entityId = IOHelper.GetInt(message.Data, 0);
                float fire = IOHelper.GetFloat(message.Data, 4);
                float cold = IOHelper.GetFloat(message.Data, 8);
                float lightning = IOHelper.GetFloat(message.Data, 12);
                float poison = IOHelper.GetFloat(message.Data, 16);
                float pierce = IOHelper.GetFloat(message.Data, 20);
                float bleed = IOHelper.GetFloat(message.Data, 24);
                float vitality = IOHelper.GetFloat(message.Data, 28);
                float chaos = IOHelper.GetFloat(message.Data, 32);
                float aether = IOHelper.GetFloat(message.Data, 36);
                Logger.Debug($"SkillManager::GetDefenseAttributes called for {entityId} with Fire:{fire}, Cold:{cold}, Lightning:{lightning}, Poison:{poison}, Pierce:{pierce}, Bleed:{bleed}, Vitality:{vitality}, Chaos:{chaos}, Aether:{aether}");
                return true;
            }

            if (message.Type == 999111)
            {
                Logger.Debug("CM:TakeAttack()");
                return true;
            }

            if (message.Type == 10101012)
            {
                // This produces exact matches to "^y    Damage 0,474187463521957 to Defender 0x101028 (Vitality)" // Just not the defender
                // ^bShield: Reduced (75,5949020385742) Damage by (5,33183132285327E-315%) percent, remaining damage (12)
                // Lists the 75, but not the 12
                float f = IOHelper.GetFloat(message.Data, 0);
                int playStatsDamageType = IOHelper.GetInt(message.Data, 4);
                int combatAttributeType = IOHelper.GetInt(message.Data, 8);
                Logger.Debug($"ApplyDamage({f}, {playStatsDamageType}, {combatAttributeType})");
                return true;
            }

            return false;
        }
    }
}
