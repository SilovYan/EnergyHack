using EnergyHack.Validators.Errors;

namespace EnergyHack.TransformerCheckers
{
    internal class BkError : IError
    {
        public string Description => "Тепловой импульс слишком большой";
    }
}