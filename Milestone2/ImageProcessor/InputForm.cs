using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

// Set at design time:
//      Form.AcceptButton = okButton
//      Form.CancelButton = cancelButton
//      captionLabel.Modifiers = Internal
//      valueTextBox.Modifiers = Internal
//      okButton.DialogResult = OK
//      cancelButton.DialogResult = Cancel

namespace ImageProcessing
{
    public partial class InputForm : Form
    {
        public InputForm()
        {
            InitializeComponent();
        }

        internal static string GetString(
            string title, string prompt, string defaultValue)
        {
            // Prepare the dialog.
            InputForm dlg = new InputForm();
            dlg.Text = title;
            dlg.captionLabel.Text = prompt;
            dlg.valueTextBox.Text = defaultValue;

            // Display the dialog.
            if (dlg.ShowDialog() == DialogResult.Cancel)
                return null;

            return dlg.valueTextBox.Text;
        }

        internal static float GetFloat(
            string title, string prompt, string defaultValue,
            float min, float max, string errString)
        {
            // Prepare the dialog.
            InputForm dlg = new InputForm();
            dlg.Text = title;
            dlg.captionLabel.Text = prompt;
            dlg.valueTextBox.Text = defaultValue;

            // Display the dialog.
            if (dlg.ShowDialog() == DialogResult.Cancel)
                return float.NaN;

            // Parse the value.
            float result;
            if (float.TryParse(dlg.valueTextBox.Text, out result)) return result;

            MessageBox.Show(errString);
            return float.NaN;
        }
        
        internal static int GetInt(
            string title, string prompt, string defaultValue,
            int min, int max, string errString)
        {
            // Prepare the dialog.
            InputForm dlg = new InputForm();
            dlg.Text = title;
            dlg.captionLabel.Text = prompt;
            dlg.valueTextBox.Text = defaultValue;

            // Display the dialog.
            if (dlg.ShowDialog() == DialogResult.Cancel)
                return int.MinValue;

            // Parse the value.
            int result;
            if (int.TryParse(dlg.valueTextBox.Text, out result)) return result;

            MessageBox.Show(errString);
            return int.MinValue;
        }
    }
}
