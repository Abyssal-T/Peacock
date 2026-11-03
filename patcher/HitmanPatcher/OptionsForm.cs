using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace HitmanPatcher
{
    public partial class OptionsForm : Form
    {
        private string customDomain;
        private bool startInTray, minimizeToTray;
        private List<string> trayDomains;
        private bool devOverrideEntitlements;
        private List<string> devSelectedProducts;
        private string devKeyHex;

        private void toggleTheme(bool darkModeEnabled)
        {
            // Change main form background
            BackColor = darkModeEnabled ? Color.FromArgb(24, 26, 27) : SystemColors.Control;

            // Change the group box colours
            groupBox1.BackColor = darkModeEnabled ? Color.FromArgb(24, 26, 27) : SystemColors.Control;
            groupBox1.ForeColor = darkModeEnabled ? Color.White : SystemColors.ControlText;

            // Change button colours and style
            buttonSave.FlatStyle = darkModeEnabled ? FlatStyle.Flat : FlatStyle.Standard;
            buttonSave.BackColor = darkModeEnabled ? Color.FromArgb(19, 21, 22) : SystemColors.Control;
            buttonSave.ForeColor = darkModeEnabled ? Color.White : SystemColors.ControlText;

            buttonCancel.FlatStyle = darkModeEnabled ? FlatStyle.Flat : FlatStyle.Standard;
            buttonCancel.BackColor = darkModeEnabled ? Color.FromArgb(19, 21, 22) : SystemColors.Control;
            buttonCancel.ForeColor = darkModeEnabled ? Color.White : SystemColors.ControlText;

            buttonTrayOptions.FlatStyle = darkModeEnabled ? FlatStyle.Flat : FlatStyle.Standard;
            buttonTrayOptions.BackColor = darkModeEnabled ? Color.FromArgb(19, 21, 22) : SystemColors.Control;
            buttonTrayOptions.ForeColor = darkModeEnabled ? Color.White : SystemColors.ControlText;

            buttonDevPanel.FlatStyle = darkModeEnabled ? FlatStyle.Flat : FlatStyle.Standard;
            buttonDevPanel.BackColor = darkModeEnabled ? Color.FromArgb(19, 21, 22) : SystemColors.Control;
            buttonDevPanel.ForeColor = darkModeEnabled ? Color.White : SystemColors.ControlText;
        }

        public OptionsForm(Settings currentSettings)
        {
            InitializeComponent();
            Settings = currentSettings;
            startInTray = Settings.startInTray;
            minimizeToTray = Settings.minimizeToTray;
            trayDomains = Settings.trayDomains;
            toggleTheme(Settings.darkModeEnabled);
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public Settings Settings
        {
            get =>
                new Settings()
                {
                    patchOptions = new MemoryPatcher.Options()
                    {
                        CustomConfigDomain = customDomain,
                        UseHttp = checkBoxHttp.Checked,
                        EnableDynamicResources = true,
                        DisableForceOfflineOnFailedDynamicResources = dynResources.Checked,
                        DisableCertPinning = true,
                        SetCustomConfigDomain = true,
                        AlwaysSendAuthHeader = true
                    },
                    darkModeEnabled = darkModeBox.Checked,
                    startInTray = startInTray,
                    minimizeToTray = minimizeToTray,
                    trayDomains = trayDomains,
                    devOverrideEntitlements = devOverrideEntitlements,
                    devSelectedProducts = devSelectedProducts,
                    devKeyHex = devKeyHex
                };
            private set
            {
                customDomain = value.patchOptions.CustomConfigDomain;
                checkBoxHttp.Checked = value.patchOptions.UseHttp;
                dynResources.Checked = value.patchOptions.DisableForceOfflineOnFailedDynamicResources;
                darkModeBox.Checked = value.darkModeEnabled;
                startInTray = value.startInTray;
                minimizeToTray = value.minimizeToTray;
                trayDomains = value.trayDomains;
                devOverrideEntitlements = value.devOverrideEntitlements;
                devSelectedProducts = value.devSelectedProducts;
                devKeyHex = value.devKeyHex;
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonDevPanel_Click(object sender, EventArgs e)
        {
            if (devKeyHex == null)
            {
                using (PasswordPrompt prompt = new PasswordPrompt(darkModeBox.Checked))
                {
                    if (prompt.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    string key = DevAuth.TryDeriveKey(prompt.Password);

                    if (key == null)
                    {
                        MessageBox.Show(this, "Wrong password.", "Dev Panel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    devKeyHex = key;
                }
            }

            DevPanelForm devPanel = new DevPanelForm(devOverrideEntitlements, devSelectedProducts, darkModeBox.Checked);
            if (devPanel.ShowDialog() == DialogResult.OK)
            {
                devOverrideEntitlements = devPanel.overrideEntitlements;
                devSelectedProducts = devPanel.selectedProducts;
            }
        }

        private void buttonTrayOptions_Click(object sender, EventArgs e)
        {
            TrayOptionsForm trayOptionsForm = new TrayOptionsForm(startInTray, minimizeToTray, trayDomains, Settings.darkModeEnabled);
            DialogResult result = trayOptionsForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                startInTray = trayOptionsForm.startInTray;
                minimizeToTray = trayOptionsForm.minimizeToTray;
                trayDomains = trayOptionsForm.trayDomains;
            }
        }
    }
}
