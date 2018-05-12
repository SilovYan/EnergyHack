using System;
using System.Collections.Generic;
using EnergyHack.Validators;
using EnergyHack.Validators.Errors;

namespace EnergyHack.TransformerCheckers
{
    public delegate void ErrorsChangedHandler(object sender, ICollection<IError> errors);
    public interface ITransformerChecker
    {
        event ErrorsChangedHandler ErrorsChanged;

        void Validate();
    }
}