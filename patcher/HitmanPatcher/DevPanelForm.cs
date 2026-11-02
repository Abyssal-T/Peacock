using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace HitmanPatcher
{
    /// <summary>
    /// Dev Panel: pick exactly which products the server should treat as "owned",
    /// to test combinations against Peacock's real ownership logic.
    /// Built in code (no designer file). The ticked products are written to
    /// devEntitlements.txt next to the patcher by Settings.SaveToFile().
    /// </summary>
    public class DevPanelForm : Form
    {
        private readonly CheckBox checkBoxOverride = new CheckBox();
        private readonly CheckedListBox listProducts = new CheckedListBox();
        private readonly Button buttonSelectAll = new Button();
        private readonly Button buttonDeselectAll = new Button();
        private readonly Button buttonSave = new Button();
        private readonly Button buttonCancel = new Button();
        private readonly Label labelInfo = new Label();

        public bool overrideEntitlements => checkBoxOverride.Checked;

        public List<string> selectedProducts
        {
            get
            {
                List<string> result = new List<string>();
                foreach (object item in listProducts.CheckedItems)
                {
                    result.Add(((ProductItem)item).Key);
                }
                return result;
            }
        }

        private class ProductItem
        {
            public string Key;
            public string Label;
            public override string ToString() => Label;
        }

        public DevPanelForm(bool overrideOn, List<string> selected, bool darkMode)
        {
            Text = "Dev Panel - Test owned products";
            ClientSize = new Size(420, 500);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.CenterParent;
            ShowIcon = false;

            checkBoxOverride.Text = "Override owned products (testing)";
            checkBoxOverride.AutoSize = true;
            checkBoxOverride.Location = new Point(12, 10);
            checkBoxOverride.CheckedChanged += (s, e) => UpdateEnabledState();

            labelInfo.Text = "When on, Peacock ignores what you really own and uses only the ticked products. Restart Peacock after saving.";
            labelInfo.Size = new Size(396, 32);
            labelInfo.Location = new Point(12, 34);

            buttonSelectAll.Text = "Select all";
            buttonSelectAll.Size = new Size(120, 28);
            buttonSelectAll.Location = new Point(12, 72);
            buttonSelectAll.Click += (s, e) => SetAll(true);

            buttonDeselectAll.Text = "Deselect all";
            buttonDeselectAll.Size = new Size(120, 28);
            buttonDeselectAll.Location = new Point(140, 72);
            buttonDeselectAll.Click += (s, e) => SetAll(false);

            listProducts.Location = new Point(12, 108);
            listProducts.Size = new Size(396, 340);
            listProducts.CheckOnClick = true;
            listProducts.HorizontalScrollbar = true;

            foreach (string[] product in DevProducts.All)
            {
                bool isChecked = selected != null && selected.Contains(product[0]);
                listProducts.Items.Add(new ProductItem { Key = product[0], Label = product[1] }, isChecked);
            }

            buttonSave.Text = "Save";
            buttonSave.Size = new Size(120, 30);
            buttonSave.Location = new Point(12, 460);
            buttonSave.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

            buttonCancel.Text = "Cancel";
            buttonCancel.Size = new Size(120, 30);
            buttonCancel.Location = new Point(140, 460);
            buttonCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(checkBoxOverride);
            Controls.Add(labelInfo);
            Controls.Add(buttonSelectAll);
            Controls.Add(buttonDeselectAll);
            Controls.Add(listProducts);
            Controls.Add(buttonSave);
            Controls.Add(buttonCancel);

            checkBoxOverride.Checked = overrideOn;
            UpdateEnabledState();
            ApplyTheme(darkMode);
        }

        private void SetAll(bool value)
        {
            for (int i = 0; i < listProducts.Items.Count; i++)
            {
                listProducts.SetItemChecked(i, value);
            }
        }

        private void UpdateEnabledState()
        {
            listProducts.Enabled = checkBoxOverride.Checked;
            buttonSelectAll.Enabled = checkBoxOverride.Checked;
            buttonDeselectAll.Enabled = checkBoxOverride.Checked;
        }

        private void ApplyTheme(bool dark)
        {
            Color text = dark ? Color.White : SystemColors.ControlText;
            Color button = dark ? Color.FromArgb(19, 21, 22) : SystemColors.Control;

            BackColor = dark ? Color.FromArgb(24, 26, 27) : SystemColors.Control;
            checkBoxOverride.ForeColor = text;
            labelInfo.ForeColor = text;
            listProducts.BackColor = dark ? Color.FromArgb(24, 26, 27) : SystemColors.Window;
            listProducts.ForeColor = text;

            foreach (Button b in new[] { buttonSelectAll, buttonDeselectAll, buttonSave, buttonCancel })
            {
                b.FlatStyle = dark ? FlatStyle.Flat : FlatStyle.Standard;
                b.BackColor = button;
                b.ForeColor = text;
            }
        }
    }
}
