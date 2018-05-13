using System.Collections.Generic;
using EnergyHack.Validators.Errors;
// ReSharper disable InconsistentNaming

namespace EnergyHack.TransformerCheckers
{
    public class CurrentTransformerChecker : ITransformerChecker
    {
        private delegate IError Validator();
        private double? _UNomTT;
        private double? _UNetwork;

        private double? _I1nom;
        private double? _I2nom;
        private double? _IRabMax;

        private ICollection<IError> _errors;

        public event ErrorsChangedHandler ErrorsChanged;

        public CurrentTransformerAccountingMode AccountingPart { get; set; }
        public CurentTransformerRzaMode RzaPart { get; set; }

        public double? UNomTT
        {
            get => _UNomTT;
            set
            {
                _UNomTT = value;
                Validate();
            }
        }
        public double? UNetwork
        {
            get => _UNetwork;
            set
            {
                _UNetwork = value;
                Validate();
            }
        }

        public double? I1nom
        {
            get => _I1nom;
            set
            {
                _I1nom = value;
                if (RzaPart != null)
                    RzaPart.I1Nom = value;
                Validate();
            }
        }
        public double? IRabMax
        {
            get => _IRabMax;
            set
            {
                _IRabMax = value;
                Validate();
            }
        }

        public double? I2Nom
        {
            get => _I2nom;
            set
            {
                _I2nom = value;
                if (AccountingPart != null)
                    AccountingPart.I2Nom = value;

                if (RzaPart != null)
                    RzaPart.I2Nom = value;
            }
        }

        public double? Iy { get; set; }
        public double? Iprs { get; set; }
        public double? Bk { get; set; }
        public double? Iter { get; set; }
        public double? TTer;

        public void Validate()
        {
            var errors = new List<IError>();

            Validate(errors, ValidateU);
            Validate(errors, ValidateI);
            Validate(errors, ValidateK);
            Validate(errors, ValidateKSecurity);
            Validate(errors, ValidateRza);
            Validate(errors, ValidateIsmall);
            Validate(errors, ValidateBk);

            if (_errors.Equal(errors)) return;

            _errors = errors.IsNullOrEmpty() ? null : errors;
            ErrorsChanged?.Invoke(this, _errors);
        }

        private IError ValidateBk()
        {
            return Bk > TTer * Iter * Iter ? new BkError() : null;
        }

        private IError ValidateIsmall()
        {
            return Iy > Iprs ? new IsmallError() : null;
        }


        private static void Validate(ICollection<IError> errors, Validator validator)
        {
            var error = validator();
            if (error != null)
                errors.Add(error);
        }

        private IError ValidateU()
        {
            return UNomTT is null || UNetwork is null || UNomTT < UNetwork ? new VoltageError() : null;
        }

        private IError ValidateI()
        {
            return I1nom is null || IRabMax is null || I1nom < IRabMax ? new AmperageError() : null;
        }

        private IError ValidateK()
        {
            if (AccountingPart == null)
                return null;
            if (AccountingPart.K > 1)
                return new SectionError();
            return new KError();
        }

        private IError ValidateKSecurity()
        {
            return AccountingPart != null && 
                   AccountingPart.HasKSecurity &&
                   AccountingPart.KSecurity > AccountingPart.KSecurityEquipment
                ? new KSecurityError()
                : null;
        }

        private IError ValidateRza()
        {
            return RzaPart?.K10 != null &&
                   RzaPart?.K10dop != null &&
                   RzaPart.K10dop < RzaPart.K10
                ? new RzaError()
                : null;
        }
    }
}