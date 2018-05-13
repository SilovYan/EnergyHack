using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Utils;
using EnergyHack.TransformerCheckers;
using EnergyHack.Validators.Errors;

namespace EnergyHack
{
    public partial class CheckerForm : Form
    {
        private const string Comercial = "Для коммерческого учета";
        private const string Technical = "Для технического учета";
        private const string Indicating = "Для измерений";
        private readonly ICollection<Control> _controlsForValidate;
        private readonly CurrentTransformerChecker _currentTransformerChecker;
        private VoltageTransformerChecker _voltageTransformerChecker = new VoltageTransformerChecker();

        private readonly ConcurrentDictionary<string, string[]> _typesToListsMap = new ConcurrentDictionary<string, string[]>();
        private readonly ConcurrentDictionary<byte, double[]> _currentMap = new ConcurrentDictionary<byte, double[]>();

        public CheckerForm(CurrentTransformerChecker currentTransformerChecker)
        {
            _currentTransformerChecker = currentTransformerChecker;
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
                UNomNetworkComboBoxEdit,
            };
            _typesToListsMap.TryAdd(Comercial, new[] { "0.2S", "0.5S" });
            _typesToListsMap.TryAdd(Technical, new[] { "0.1", "0.2", "0.5", "1" });
            _typesToListsMap.TryAdd(Indicating, new[] { "0.1", "0.2", "0.5", "1", "3" });

            _currentMap.TryAdd(0, new[] {1, 1.5, 2.5, 4, 6});
            _currentMap.TryAdd(1, new[] {2.5, 4, 6});
            UpdateBaseModelAfterInitialize();
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

            var kError = errors?.FirstOrDefault(e => e is KError);
            if (kError != null)
            {
                CurrentTransformerErrorProvider.SetError(SaddTextEdit, kError.Description);
            }

            var sectionError = errors?.FirstOrDefault(e => e is SectionError);
            if (sectionError != null)
            {
                CurrentTransformerErrorProvider.SetError(sComboBoxEdit, sectionError.Description);
            }
            
            var kSecurityError = errors?.FirstOrDefault(e => e is KSecurityError);
            if (kSecurityError != null)
            {
                CurrentTransformerErrorProvider.SetError(kSecurityEquipmentTextEdit, kSecurityError.Description);
            }

            var rzaError = errors?.FirstOrDefault(e => e is RzaError);
            if (rzaError != null)
            {
                CurrentTransformerErrorProvider.SetError(RZAsComboBoxEdit, rzaError.Description);
            }

            var ismallError = errors?.FirstOrDefault(e => e is IsmallError);
            if (ismallError != null)
            {
                CurrentTransformerErrorProvider.SetError(iуTextEdit, ismallError.Description);
            }

            var bkError = errors?.FirstOrDefault(e => e is BkError);
            if (bkError != null)
            {
                CurrentTransformerErrorProvider.SetError(BkTextEdit, bkError.Description);
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
                // TODO: магия =)
                _currentTransformerChecker.I2Nom = _currentTransformerChecker.I2Nom;
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
            if (DefenceModeCheckEdit.Checked)
            {
                _currentTransformerChecker.RzaPart = new CurentTransformerRzaMode();
                // TODO: магия =)
                _currentTransformerChecker.I1nom = _currentTransformerChecker.I1nom;
                _currentTransformerChecker.I2Nom = _currentTransformerChecker.I2Nom;
                FillRzaPart();
            }
            else
            {
                _currentTransformerChecker.RzaPart = null;
                ClearRzaPart();
            }
        }

        private void I2NomComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TryGetValue(I2NomComboBoxEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.I2Nom = value;
            }
            UpdateVisibleRkAndSadd();
        }

        private void AccuracyClassTypeСomboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO: тут не всё так просто
            var selected = AccuracyClassTypeСomboBoxEdit.SelectedItem?.ToString();
            if (selected != null)
            {
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
            UpdateVisibleRkAndSadd();
            UpdateVisibleKSecurity();
        }

        private void AccuracyClassComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentTransformerChecker.AccountingPart.Accuracy = AccuracyClassComboBoxEdit.SelectedItem?.ToString();
            UpdateVisibleRkAndSadd();
        }

        private void S2NomComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TryGetValue(S2NomComboBoxEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.AccountingPart.S2Nom = value;
            }
            UpdateVisibleRkAndSadd();
        }

        private void CurrentLengthTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(CurrentLengthTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.AccountingPart.CurrentLength = value;
            }
            UpdateVisibleRkAndSadd();
        }

        private void sComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TryGetValue(sComboBoxEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.AccountingPart.CurrentS = value;
            }
            UpdateVisibleRkAndSadd();
        }

        private void RkTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(RkTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.AccountingPart.Rk = value;
            }
            _currentTransformerChecker.Validate();
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
            var index = (byte) CurrentTypeRadioGroup.SelectedIndex;
            if (index == 0)
                _currentTransformerChecker.AccountingPart.CurrentType =
                    CurrentType.Aluminium;
            if(index == 1)
                _currentTransformerChecker.AccountingPart.CurrentType =
                    CurrentType.Copper;
            FillCurrentS(index);
            UpdateVisibleRkAndSadd();
        }

        private void SprTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(SprTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.AccountingPart.Spr = value;
            }
            UpdateVisibleRkAndSadd();
        }

        private void kSecurityComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TryGetValue(kSecurityComboBoxEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.AccountingPart.KSecurity = value;
            }
            _currentTransformerChecker.Validate();
        }

        private void kSecurityEquipmentTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(kSecurityEquipmentTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.AccountingPart.KSecurityEquipment = value;
            }
            _currentTransformerChecker.Validate();
        }

        private void DefenceModeCheckEdit_EditValueChanged(object sender, EventArgs e)
        {
            DefenceModeLayout.Enabled = DefenceModeCheckEdit.Checked;
        }

        private void RzaS2NomComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TryGetValue(RzaS2NomComboBoxEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.RzaPart.S2Nom = value;
            }
            _currentTransformerChecker.Validate();
        }

        private void RzaKnTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(RzaKnTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.RzaPart.Kn = value;
            }
            _currentTransformerChecker.Validate();
        }

        private void RzaZnTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(RzaZnTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.RzaPart.Zn = value;
            }
            _currentTransformerChecker.Validate();
        }

        private void RzaZrTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(RzaZrTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.RzaPart.Zr = value;
            }
            _currentTransformerChecker.Validate();
        }

        private void RzaIkzTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(RzaIkzTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.RzaPart.Ikz = value;
            }
            _currentTransformerChecker.Validate();
        }

        private void RzaCurrentTypeRadioGroup_EditValueChanged(object sender, EventArgs e)
        {
            var index = (byte)RzaCurrentTypeRadioGroup.SelectedIndex;
            if (index == 0)
                _currentTransformerChecker.RzaPart.CurrentType =
                    CurrentType.Aluminium;
            if (index == 1)
                _currentTransformerChecker.RzaPart.CurrentType =
                    CurrentType.Copper;
            FillRzaCurrentS(index);
            _currentTransformerChecker.Validate();
        }

        private void RzaCurrentLengthTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(RzaCurrentLengthTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.RzaPart.CurrentLength = value;
            }
            _currentTransformerChecker.Validate();
        }

        private void RZAsComboBoxEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(RZAsComboBoxEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.RzaPart.CurrentS = value;
            }
            _currentTransformerChecker.Validate();
        }

        private void RzaRkTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(RzaRkTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.RzaPart.Rk = value;
            }
            _currentTransformerChecker.Validate();
        }

        private void iуTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(iуTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.Iy = value;
            }
            _currentTransformerChecker.Validate();
        }

        private void iprsTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(iprsTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.Iprs = value;
            }
            _currentTransformerChecker.Validate();
        }

        private void BkTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (TryGetValue(BkTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.Bk = value;
            }
            _currentTransformerChecker.Validate();
        }

        private void IterTextEdit_EditValueChanged(object sender, EventArgs e)
        {

            if (TryGetValue(IterTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.Iter = value;
            }
            _currentTransformerChecker.Validate();
        }

        private void TterTextEdit_EditValueChanged(object sender, EventArgs e)
        {

            if (TryGetValue(TterTextEdit, CurrentTransformerErrorProvider, out var value))
            {
                _currentTransformerChecker.TTer = value;
            }
            _currentTransformerChecker.Validate();
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
            var kBez = new[] {5, 10};
            S2NomComboBoxEdit.ClearAndFill(s2Nom);
            AccuracyClassTypeСomboBoxEdit.ClearAndFill(_typesToListsMap.Keys.ToList());
            kSecurityComboBoxEdit.ClearAndFill(kBez);

            UpdateVisibleRkAndSadd();
        }

        private void FillRzaPart()
        {
            var s2Nom = new[] { 0.5, 1, 2, 2.5, 3, 5, 10, 15, 20, 25, 30, 40, 50, 60, 75, 100 };
            RzaS2NomComboBoxEdit.ClearAndFill(s2Nom);
            UpdateRzaPartOfModel();
        }

        private void UpdateVisibleRkAndSadd()
        {
            var SaddVisible = _currentTransformerChecker.AccountingPart?.SaddVisible ?? false;

            if (SaddVisible && !SaddLayoutItem.Visible)
            {
                _currentTransformerChecker.AccountingPart.SetSadd();
                MessageBox.Show(
                    @"Рекомендуется уставновить догрузочное сопротивнение c мощностью не менее Sadd");
                SaddTextEdit.Text = _currentTransformerChecker.AccountingPart.Sadd.ToString();
            }
            if (!SaddVisible && SaddLayoutItem.Visible)
            {
                if (_currentTransformerChecker.AccountingPart != null)
                {
                    _currentTransformerChecker.AccountingPart.Sadd = 0;
                }
            }

            SaddLayoutItem.Visibility = SaddVisible ? LayoutVisibility.Always : LayoutVisibility.Never;
            _currentTransformerChecker.Validate();
        }
        
        private void UpdateVisibleKSecurity()
        {
            var visibility = _currentTransformerChecker.AccountingPart.HasKSecurity
                ? LayoutVisibility.Always
                : LayoutVisibility.Never;
            layoutControlItem22.Visibility = visibility;
            layoutControlItem23.Visibility = visibility;
            layoutControlItem24.Visibility = visibility;
            layoutControlItem25.Visibility = visibility;
        }

        private void FillAccuracy(string type)
        {
            if (_typesToListsMap != null && _typesToListsMap.ContainsKey(type))
            {
                AccuracyClassComboBoxEdit.ClearAndFill(_typesToListsMap[type].ToList());
            }
            UpdateAccountingPartOfModel();
        }

        private void ClearAccountingPart()
        {
            // TODO: не совсем понятно что тут делать
        }
        private void ClearRzaPart()
        {
            // TODO: не совсем понятно что тут делать
        }

        private void FillCurrentS(byte selectedIndex)
        {
            if(_currentMap.ContainsKey(selectedIndex))
                sComboBoxEdit.ClearAndFill(_currentMap[selectedIndex]);
        }

        private void FillRzaCurrentS(byte selectedIndex)
        {
            if (_currentMap.ContainsKey(selectedIndex))
                RZAsComboBoxEdit.ClearAndFill(_currentMap[selectedIndex]);
        }

        private void UpdateBaseModelAfterInitialize()
        {
            UNomTTComboBoxEdit_SelectedIndexChanged(null, null);
            UNomNetworkComboBoxEdit_SelectedIndexChanged(null, null);
            I1NomComboBoxEdit_SelectedIndexChanged(null, null);
            IRabMaxComboBoxEdit_TextChanged(null, null);
            I2NomComboBoxEdit_SelectedIndexChanged(null, null);
            UNomTTComboBoxEdit_SelectedIndexChanged(null, null);
            UNomTTComboBoxEdit_SelectedIndexChanged(null, null);
        }

        private void UpdateAccountingPartOfModel()
        {
            AccuracyClassComboBoxEdit_SelectedIndexChanged(null, null);
            S2NomComboBoxEdit_SelectedIndexChanged(null, null);
            CurrentTypeRadioGroup_SelectedIndexChanged(null, null);
            CurrentLengthTextEdit_EditValueChanged(null, null);
            SprTextEdit_EditValueChanged(null, null);
            CurrentTypeRadioGroup_SelectedIndexChanged(null, null);
            UpdateKSecurityPartOfModel();
        }

        private void UpdateKSecurityPartOfModel()
        {
            kSecurityComboBoxEdit_SelectedIndexChanged(null, null);
            kSecurityEquipmentTextEdit_EditValueChanged(null, null);
        }

        private void UpdateRzaPartOfModel()
        {
            RzaS2NomComboBoxEdit_SelectedIndexChanged(null,null);
            RzaKnTextEdit_EditValueChanged(null,null);
            RzaZnTextEdit_EditValueChanged(null,null);
            RzaZrTextEdit_EditValueChanged(null,null);
            RzaIkzTextEdit_EditValueChanged(null,null);
            RzaCurrentTypeRadioGroup_EditValueChanged(null,null);
            RzaCurrentLengthTextEdit_EditValueChanged(null,null);
            RZAsComboBoxEdit_EditValueChanged(null,null);
            RzaRkTextEdit_EditValueChanged(null,null);
        }

        #endregion

        private void ValidateCurrentTransformerControls()
        {
            foreach (var control in _controlsForValidate)
                TryGetValue(control, CurrentTransformerErrorProvider, out _);

            if (_currentTransformerChecker.AccountingPart != null)
            {
                TryGetValue(SaddTextEdit, CurrentTransformerErrorProvider, out _);
                TryGetValue(RkTextEdit, CurrentTransformerErrorProvider, out _);
                TryGetValue(CurrentLengthTextEdit, CurrentTransformerErrorProvider, out _);
                TryGetValue(SprTextEdit, CurrentTransformerErrorProvider, out _);
            }
            if (_currentTransformerChecker.RzaPart != null)
            {
                TryGetValue(RzaKnTextEdit, CurrentTransformerErrorProvider, out _);
                TryGetValue(RzaZnTextEdit, CurrentTransformerErrorProvider, out _);
                TryGetValue(RzaZrTextEdit, CurrentTransformerErrorProvider, out _);
                TryGetValue(RzaCurrentLengthTextEdit, CurrentTransformerErrorProvider, out _);
                TryGetValue(RzaIkzTextEdit, CurrentTransformerErrorProvider, out _);
            }
            TryGetValue(iуTextEdit, CurrentTransformerErrorProvider, out _);
            TryGetValue(iprsTextEdit, CurrentTransformerErrorProvider, out _);
            TryGetValue(BkTextEdit, CurrentTransformerErrorProvider, out _);
            TryGetValue(IterTextEdit, CurrentTransformerErrorProvider, out _);
            TryGetValue(TterTextEdit, CurrentTransformerErrorProvider, out _);
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

        private void VoltUnTTComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void VoltUnNetworkComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void VoltS2NomComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void VoltDestinitionComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void VoltAccuracyClassComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void VoltAimComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void VoltMaxSTextEdit_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void VoltRecomentRTextEdit_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void VoltCurrentLengthTextEdit_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void VoltsCurrentTextEdit_EditValueChanged(object sender, EventArgs e)
        {

        }
    }
}
