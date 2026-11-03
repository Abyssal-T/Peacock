using System;
using System.Drawing;
using System.Windows.Forms;

namespace HitmanPatcher
{
    /// <summary>
    /// Minimal modal password prompt for the Dev Panel. Built in code (no designer file).
    /// </summary>
    public class PasswordPrompt : Form
    {
        private readonly TextBox textBoxPassword = new TextBox();
        private readonly Label labelInfo = new Label();
        private readonly Button buttonOk = new Button();
        private readonly Button buttonCancel = new Button();

        public string Password => textBoxPassword.Text;

        public PasswordPrompt(bool darkMode)
        {
            Text = "Dev Panel - Password";
            ClientSize = new Size(320, 110);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.CenterParent;
            ShowIcon = false;

            labelInfo.Text = "Enter the Dev Panel password:";
            labelInfo.AutoSize = true;
            labelInfo.Location = new Point(12, 12);

            textBoxPassword.UseSystemPasswordChar = true;
            textBoxPassword.Size = new Size(296, 23);
            textBoxPassword.Location = new Point(12, 36);

            buttonOk.Text = "Unlock";
            buttonOk.Size = new Size(100, 28);
            buttonOk.Location = new Point(12, 70);
            buttonOk.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

            buttonCancel.Text = "Cancel";
            buttonCancel.Size = new Size(100, 28);
            buttonCancel.Location = new Point(120, 70);
            buttonCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            AcceptButton = buttonOk;
            CancelButton = buttonCancel;

            Controls.Add(labelInfo);
            Controls.Add(textBoxPassword);
            Controls.Add(buttonOk);
            Controls.Add(buttonCancel);

            Color text = darkMode ? Color.White : SystemColors.ControlText;
            BackColor = darkMode ? Color.FromArgb(24, 26, 27) : SystemColors.Control;
            labelInfo.ForeColor = text;
            textBoxPassword.BackColor = darkMode ? Color.FromArgb(19, 21, 22) : SystemColors.Window;
            textBoxPassword.ForeColor = text;

            foreach (Button b in new[] { buttonOk, buttonCancel })
            {
                b.FlatStyle = darkMode ? FlatStyle.Flat : FlatStyle.Standard;
                b.BackColor = darkMode ? Color.FromArgb(19, 21, 22) : SystemColors.Control;
                b.ForeColor = text;
            }
        }
    }
}
