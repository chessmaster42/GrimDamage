using GrimDamage.Settings;
using System;
using System.Windows.Forms;

namespace GrimDamage.GUI.Forms
{
    public partial class DebugSettings : Form
    {
        private readonly AppSettings _appSettings;

        public DebugSettings(AppSettings appSettings)
        {
            InitializeComponent();
            _appSettings = appSettings;
        }

        private void DebugSettings_Load(object sender, EventArgs eventArgs)
        {
            Dock = DockStyle.Fill;
            cbLogStateChanges.Checked = _appSettings.LogStateChanges;
            cbLogAllLogStatements.Checked = _appSettings.LogAllEvents;
            cbLogUnknownLogStatements.Checked = _appSettings.LogUnknownEvents;
            cbEnableInvestigativeLogging.Checked = _appSettings.LogProcessedMessages;
            cbLogPlayerMovement.Checked = _appSettings.LogPlayerMovement;
            cbLogPlayerDetection.Checked = _appSettings.LogPlayerDetection;
            cbLogHitpointChanges.Checked = _appSettings.LogEntityHitpointEvent;
            cbLogResistsOnAttack.Checked = _appSettings.LogResistEntries;
        }

        private void cbEnableInvestigativeLogging_CheckedChanged(object sender, EventArgs eventArgs)
        {
            _appSettings.LogProcessedMessages = (sender as CheckBox)?.Checked ?? false;
        }

        private void cbLogStateChanges_CheckedChanged(object sender, EventArgs eventArgs)
        {
            _appSettings.LogStateChanges = (sender as CheckBox)?.Checked ?? false;
        }

        private void cbLogAllLogStatements_CheckedChanged(object sender, EventArgs eventArgs)
        {
            _appSettings.LogAllEvents = (sender as CheckBox)?.Checked ?? false;
        }

        private void cbLogUnknownLogStatements_CheckedChanged(object sender, EventArgs eventArgs)
        {
            _appSettings.LogUnknownEvents = (sender as CheckBox)?.Checked ?? false;
        }

        private void cbLogPlayerMovement_CheckedChanged(object sender, EventArgs eventArgs)
        {
            _appSettings.LogPlayerMovement = (sender as CheckBox)?.Checked ?? false;
        }

        private void cbLogPlayerDetection_CheckedChanged(object sender, EventArgs eventArgs)
        {
            _appSettings.LogPlayerDetection = (sender as CheckBox)?.Checked ?? false;
        }

        private void cbLogHitpointChanges_CheckedChanged(object sender, EventArgs eventArgs)
        {
            _appSettings.LogEntityHitpointEvent = (sender as CheckBox)?.Checked ?? false;
        }

        private void cbLogResistsOnAttack_CheckedChanged(object sender, EventArgs eventArgs)
        {
            _appSettings.LogResistEntries = (sender as CheckBox)?.Checked ?? false;
        }
    }
}
