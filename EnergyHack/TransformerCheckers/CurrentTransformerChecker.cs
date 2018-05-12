using System;
using System.Collections.Generic;
using EnergyHack.Validators.Errors;
// ReSharper disable InconsistentNaming

namespace EnergyHack.TransformerCheckers
{
    public class CurrentTransformerChecker : ITransformerChecker
    {
        private delegate IError Validator();
        private double _UNomTT;
        private double _UNetwork;

        private double _I1nom;
        private double _IRabMax;

        private ICollection<IError> _errors;

        public event ErrorsChangedHandler ErrorsChanged;

        public double UNomTT
        {
            get => _UNomTT;
            set
            {
                _UNomTT = value;
                Validate();
            }
        }
        public double UNetwork
        {
            get => _UNetwork;
            set
            {
                _UNetwork = value;
                Validate();
            }
        }

        public double I1nom
        {
            get => _I1nom;
            set
            {
                _I1nom = value;
                Validate();
            }
        }
        public double IRabMax
        {
            get => _IRabMax;
            set
            {
                _IRabMax = value;
                Validate();
            }
        }

        public void Validate()
        {
            var errors = new List<IError>();

            Validate(errors, ValidateU);
            Validate(errors, ValidateI);

            if (_errors.Equal(errors)) return;

            _errors = errors.IsNullOrEmpty() ? null : errors;
            ErrorsChanged?.Invoke(this, _errors);
        }

        private static void Validate(ICollection<IError> errors, Validator validator)
        {
            var error = validator();
            if (error != null)
                errors.Add(error);
        }

        private IError ValidateU()
        {
            return UNomTT < UNetwork ? new VoltageError() : null;
        }

        private IError ValidateI()
        {
            return I1nom < IRabMax ? new AmperageError() : null;
        }
    }
}