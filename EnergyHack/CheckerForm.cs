using System;
using System.Collections.Concurrent;
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
        private const string Comercial = "Для коммерческого учета";
        private const string Technical = "Для технического учета";
        private const string Indicating = "Для указывающих амперметров";
        private readonly ICollection<Control> _controlsForValidate;
        private readonly CurrentTransformerChecker _currentTransformerChecker = new CurrentTransformerChecker();
        private VoltageTransformerChecker _voltageTransformerChecker = new VoltageTransformerChecker();

        private readonly ConcurrentDictionary<string, string[]> _typesToListsMap = new ConcurrentDictionary<string, string[]>();
        private readonly ConcurrentDictionary<byte, double[]> _currentMap = new ConcurrentDictionary<byte, double[]>();

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
            _typesToListsMap.TryAdd(Comercial, new[] { "0.2S", "0.5S" });
            _typesToListsMap.TryAdd(Technical, new[] { "0.1", "0.2", "0.5", "1" });
            _typesToListsMap.TryAdd(Indicating, new[] { "0.1", "0.2", "0.5", "1", "3" });

            _currentMap.TryAdd(0, new[] {1, 1.5, 2.5, 4, 6, 10});
            _currentMap.TryAdd(1, new[] {2.5, 4, 6, 10});
        }

        #region Changed

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

        private void IRabMaxComboBoxEdit_TextChanged(object sender, EventArgs e)
        {
            if (TryGetValue(IRabMaxComboBoxEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.IRabMax = value;
            }
        }

        private void I1NomComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TryGetValue(I1NomComboBoxEdit, CurrentTransformerErrorProvider, out var value))
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

        private void DefenceModeCheckEdit_CheckedChanged(object sender, EventArgs e)
        {
            DefenceModeLayout.Enabled = DefenceModeCheckEdit.Checked;
        }

        private void I2NomComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TryGetValue(I2NomComboBoxEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.I2Nom = value;
            }
        }

        private void AccuracyClassTypeСomboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO: тут не всё так просто
            var selected = AccuracyClassTypeСomboBoxEdit.SelectedItem.ToString();
            FillAccuracy(selected);
            switch (selected)
            {
                case Comercial:
                    _currentTransformerChecker.AccountingPart.AccuracyClass = AccuracyClass.Comercial;
                    break;
                case Technical:
                    _currentTransformerChecker.AccountingPart.AccuracyClass = AccuracyClass.Technical;
                    break;
                case Indicating:
                    _currentTransformerChecker.AccountingPart.AccuracyClass = AccuracyClass.Indicating;
                    break;
            }
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

        private void CurrentTypeRadioGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillCurrentS((byte)CurrentTypeRadioGroup.SelectedIndex);
        }

        #endregion

        #region Fillers

        private void Fill()
        {
            var untt = new[] { 0.66, 3, 6, 10, 15, 20, 24, 27, 35, 110, 150, 220, 330, 500, 750 };
            var unnetwork = new[] { 0.66, 3, 6, 10, 15, 20, 24, 27, 35, 110, 150, 220, 330, 500, 750 };
            var i1ttnom = new[]
            {
                1, 5, 10, 15, 20, 30, 40, 50, 75, 80, 100, 150, 200, 300, 400, 500, 600, 750, 800, 1000, 1200, 1500,
                1600, 2000, 3000, 4000, 5000, 6000, 8000, 10000, 12000, 14000, 16000, 18000, 20000, 25000, 28000, 30000,
                32000, 35000, 40000
            };
            var i2ttnom = new[] { 1, 5 };

            UNomTTComboBoxEdit.ClearAndFill(untt);
            UNomNetworkComboBoxEdit.ClearAndFill(unnetwork);
            I1NomComboBoxEdit.ClearAndFill(i1ttnom);
            I2NomComboBoxEdit.ClearAndFill(i2ttnom);
        }

        private void FillAccountingPart()
        {
            var s2Nom = new[] { 0.5, 1, 2, 2.5, 5, 3, 10, 15, 20, 25, 30, 40, 50, 60, 75, 100 };
            S2NomComboBoxEdit.ClearAndFill(s2Nom);
            AccuracyClassTypeСomboBoxEdit.ClearAndFill(_typesToListsMap.Keys.ToList());
        }

        private void FillAccuracy(string type)
        {
            if (_typesToListsMap != null && _typesToListsMap.ContainsKey(type))
            {
                AccuracyClassComboBoxEdit.ClearAndFill(_typesToListsMap[type].ToList());
            }
        }

        private void ClearAccountingPart()
        {
            // TODO: не совсем понятно что тут делать
        }
        
        private void FillCurrentS(byte selectedIndex)
        {
            sComboBoxEdit.ClearAndFill(_currentMap[selectedIndex]);
        }

        #endregion

        private void ValidateCurrentTransformerControls()
        {
            foreach (var control in _controlsForValidate)
                TryGetValue(control, CurrentTransformerErrorProvider, out _);
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
    }
}
