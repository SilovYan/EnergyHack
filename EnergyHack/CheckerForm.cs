using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraLayout;
using EnergyHack.TransformerCheckers;
using EnergyHack.Validators.Errors;

namespace EnergyHack
{
    public partial class CheckerForm : Form
    {
        private ICollection<Control> _controlsForValidate;
        private readonly CurrentTransformerChecker _currentTransformerChecker = new CurrentTransformerChecker();
        private VoltageTransformerChecker _voltageTransformerChecker = new VoltageTransformerChecker();

        public ILayoutControl Current => TTControl;
        public ILayoutControl Voltage => TNControl;
        public CheckerForm()
        {
            InitializeComponent();
            Fill();

            UNomNetworkComboBoxEdit.SelectedIndexChanged += UNomNetworkComboBoxEdit_SelectedIndexChanged;
            UNomTTComboBoxEdit.SelectedIndexChanged += UNomTTComboBoxEdit_SelectedIndexChanged;

            I1NomComboBoxEdit.SelectedIndexChanged += I1NomComboBoxEdit_SelectedIndexChanged;
            IRabMaxComboBoxEdit.TextChanged += IRabMaxComboBoxEdit_TextChanged;
            _currentTransformerChecker.ErrorsChanged += _currentTransformerChecker_ErrorsChanged;

            _controlsForValidate = new List<Control>
            {
                IRabMaxComboBoxEdit,
                I1NomComboBoxEdit,
                UNomTTComboBoxEdit,
                UNomNetworkComboBoxEdit
            };
        }

        private void ValidateCurrentTransformerControls()
        {
            foreach (var control in _controlsForValidate)
                TryGetValue(control, CurrentTransformerErrorProvider, out _);
        }

        private void _currentTransformerChecker_ErrorsChanged(object sender, ICollection<IError> errors)
        {
            CurrentTransformerErrorProvider.ClearErrors();

            ValidateCurrentTransformerControls();

            var amperageError = errors?.FirstOrDefault(e => e is AmperageError);
            if (amperageError != null)
            {
                CurrentTransformerErrorProvider.SetError(I1NomComboBoxEdit, amperageError.Description);
            }

            var voltageError = errors?.FirstOrDefault(e => e is VoltageError);
            if (voltageError != null)
            {
                CurrentTransformerErrorProvider.SetError(UNomTTComboBoxEdit, voltageError.Description);
            }
        }

        private void Fill()
        {
            var untt = new [] { 0.66, 3, 6, 10, 15, 20, 24, 27, 35, 110, 150, 220, 330, 500, 750 };
            var unnetwork = new [] { 0.66, 3, 6, 10, 15, 20, 24, 27, 35, 110, 150, 220, 330, 500, 750 };
            var ittnom = new[]
            {
                1, 5, 10, 15, 20, 30, 40, 50, 75, 80, 100, 150, 200, 300, 400, 500, 600, 750, 800, 1000, 1200, 1500,
                1600, 2000, 3000, 4000, 5000, 6000, 8000, 10000, 12000, 14000, 16000, 18000, 20000, 25000, 28000, 30000,
                32000, 35000, 40000
            };
            UNomTTComboBoxEdit.Properties.Items.AddRange(untt);
            UNomTTComboBoxEdit.SelectedIndex = 0;
            UNomNetworkComboBoxEdit.Properties.Items.AddRange(unnetwork);
            UNomNetworkComboBoxEdit.SelectedIndex = 0;
            I1NomComboBoxEdit.Properties.Items.AddRange(ittnom);
            I1NomComboBoxEdit.SelectedIndex = 0;
        }

        private void IRabMaxComboBoxEdit_TextChanged(object sender, EventArgs e)
        {
            if (TryGetValue(IRabMaxComboBoxEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.IRabMax = value;
            }
        }

        private void I1NomComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(TryGetValue(I1NomComboBoxEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.I1nom = value;
            }
        }

        private void UNomTTComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TryGetValue(UNomTTComboBoxEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.UNomTT = value;
            }
        }

        private void UNomNetworkComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TryGetValue(UNomNetworkComboBoxEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.UNetwork = value;
            }
        }

        private static bool TryGetValue(Control control, DXErrorProvider provider, out double value)
        {
            provider.SetError(control, null);
            try
            {
                value = Convert.ToDouble(control.Text);
                return true;
            }
            catch
            {
                provider.SetError(control, "Должно быть числом");
            }
            value = default(double);
            return false;
        }

        private void AccountingModeCheckEdit_CheckedChanged(object sender, EventArgs e)
        {
            AccountingModeControl.Enabled = AccountingModeCheckEdit.Checked;
            if (AccountingModeCheckEdit.Checked)
            {
                _currentTransformerChecker.AccountingPart = new CurrentTransformerAccountingMode();
                FillAccountingPart();
            }
            else
            {
                _currentTransformerChecker.AccountingPart = null;
                ClearAccountingPart();
            }
        }

        private void FillAccountingPart()
        {
            throw new NotImplementedException();
        }

        private void ClearAccountingPart()
        {
            throw new NotImplementedException();
        }

        private void DefenceModeCheckEdit_CheckedChanged(object sender, EventArgs e)
        {
            DefenceModeLayout.Enabled = DefenceModeCheckEdit.Checked;
        }

        private void I2NomComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TryGetValue(I2NomComboBoxEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.AccountingPart.I2Nom = value;
            }
        }

        private void AccuracyClassTypeСomboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO: тут не всё так просто
        }

        private void AccuracyClassComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
             _currentTransformerChecker.AccountingPart.Accuracy = AccuracyClassComboBoxEdit.SelectedItem.ToString();
        }

        private void S2NomComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TryGetValue(S2NomComboBoxEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.AccountingPart.S2Nom = value;
            }
        }

        private void CurrentLengthTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(CurrentLengthTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.AccountingPart.CurrentLength = value;
            }
        }

        private void sComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TryGetValue(sComboBoxEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.AccountingPart.Spr = value;
            }
        }

        private void RkTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(RkTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.AccountingPart.Rk = value;
            }
        }

        private void SaddTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(SaddTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.AccountingPart.Sadd = value;
            }
        }
    }
}
