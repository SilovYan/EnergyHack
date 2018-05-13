using System;
using System.Collections.Generic;
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


        public void Validate()
        {
            ErrorsChanged?.Invoke(this,null);
        }
    }
}