using EnergyHack.Validators.Errors;

namespace EnergyHack.TransformerCheckers
{
    internal class IsmallError : IError
    {
        public string Description => "Расчетный ударные ток должен быть не больше тока динамической стойкости";
    }
}