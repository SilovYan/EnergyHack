using System.Collections.Generic;
using EnergyHack.Validators;
using EnergyHack.Validators.Errors;

namespace EnergyHack.TransformerCheckers
{
    public interface ITransformerChecker
    {
        IEnumerable<IError> Errors { get; }

        void Validate();
    }
}