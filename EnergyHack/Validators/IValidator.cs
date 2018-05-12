using EnergyHack.Validators.Errors;

namespace EnergyHack.Validators
{
    public interface IValidator
    {
        IError Validate();
    }
}