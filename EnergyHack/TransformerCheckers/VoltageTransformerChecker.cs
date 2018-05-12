using System.Collections.Generic;
using EnergyHack.Validators;
using EnergyHack.Validators.Errors;

namespace EnergyHack.TransformerCheckers
{
    public class VoltageTransformerChecker : ITransformerChecker
    {
        public IEnumerable<IError> Errors { get; }

        public void Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}