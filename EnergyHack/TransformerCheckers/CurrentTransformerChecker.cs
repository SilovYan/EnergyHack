using System.Collections.Generic;
using EnergyHack.Validators.Errors;

namespace EnergyHack.TransformerCheckers
{
    public class CurrentTransformerChecker : ITransformerChecker
    {
        private delegate IError Validator();
        private double _UNomTT;
        private double _UNetwork;

        private double _I1nom;
        private double _IRabMax;

        private IList<IError> _errors;

        public IEnumerable<IError> Errors => _errors;

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
            _errors = new List<IError>();

            Validate(ValidateU);
            Validate(ValidateI);

            _errors = Errors.IsNullOrEmpty() ? null : _errors;
        }

        private void Validate(Validator validator)
        {
            var error = validator();
            if (error != null)
                _errors.Add(error);
        }

        public IError ValidateU()
        {
            return UNomTT < UNetwork ? new VoltageError() : null;
        }

        public IError ValidateI()
        {
            return I1nom < IRabMax ? new AmperageError() : null;
        }
    }
}