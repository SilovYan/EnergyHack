using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EnergyHack.Validators;
using EnergyHack.Validators.Errors;

namespace EnergyHack.TransformerCheckers
{
    public class VoltageTransformerChecker : ITransformerChecker
    {
        public event ErrorsChangedHandler ErrorsChanged;

        public double? UnTT { get; set; }
        public double? UnNetwork { get; set; }
        public double? S2Nom { get; set; }
        public double? Stn { get; set; }
        public double? DUlim { get; set; }
        public double? CurrentLength { get; set; }
        public double? CurrentS { get; set; }

        public double? Rpr => CurrentLength / (57 * CurrentS);
        public double? Inagr => Math.Sqrt(3) * Stn / 100;
        public double? DU => Math.Sqrt(3) * Inagr * Rpr;
        public double? Kload => Stn / S2Nom;
        public double? Sadd => 0.5 * S2Nom - Stn;
        public double? Radd => 10000/Sadd;

        private static readonly Dictionary<double,double[]> Map = new Dictionary<double, double[]>();

        static VoltageTransformerChecker()
        {
            Map.Add(3, new[] {3, 3.15, 3.3});
            Map.Add(6, new[] { 5, 6, 6.3, 6.6, 6.9 });
            Map.Add(10, new[] { 10, 10.5, 11 });
            Map.Add(15, new[] { 13.8, 15, 15.75, 16 });
            Map.Add(20, new double[] { 18, 20, 22 });
            Map.Add(24, new double[] { 24 });
            Map.Add(27, new[] { 27, 27.5 });
            Map.Add(35, new double[] { 33, 35, 36 });
            Map.Add(110, new double[] {110 });
            Map.Add(150, new double[] { 150 });
            Map.Add(220, new double[] { 220 });
            Map.Add(330, new double[] { 330 });
            Map.Add(500, new double[] { 500 });
            Map.Add(750, new double[] { 750});
        }

        private ICollection<IError> _errors;

        public void Validate()
        {
            var errors = new List<IError>();

            Validate(errors, ValidateCompability);
            Validate(errors, ValidateKload);
            Validate(errors, ValidateDU);

            if (_errors.Equal(errors)) return;

            _errors = errors.IsNullOrEmpty() ? null : errors;
            ErrorsChanged?.Invoke(this, _errors);
        }

        private IError ValidateDU()
        {
            if (DU <= DUlim)
                return null;
            return new DUError();
        }

        private IError ValidateKload()
        {
            if (0.4 <= Kload && Kload<=0.6)
                return null;
            if (Kload < 0.4)
                return new KloadTooSmallError();
            return new KloadTooBigError();
        }

        private IError ValidateCompability()
        {
            if (UnNetwork.HasValue && Map[UnNetwork.Value].Any(t => Equals(t, UnTT)))
                return null;
            return new CompabilityError();
        }

        private static void Validate(ICollection<IError> errors, Validator validator)
        {
            var error = validator();
            if (error != null)
                errors.Add(error);
        }
    }

    internal class KloadTooBigError : IError
    {
        public string Description => "Необходимо увеличить мощность обмотки ТН";
    }

    internal class KloadTooSmallError : IError
    {
        public string Description => "Необходимо добавить догрузочное сопротивление";
    }

    internal class DUError : IError
    {
        public string Description => "Напряжение больше допустимого";
    }

    internal class CompabilityError : IError
    {
        public string Description => "Указаны не совместимые напряжения";
    }
}