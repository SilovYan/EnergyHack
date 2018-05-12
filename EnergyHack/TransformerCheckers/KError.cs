using EnergyHack.Validators.Errors;

namespace EnergyHack.TransformerCheckers
{
    internal class KError : IError
    {
        public string Description => "Рекомендуется уставновить догрузочное сопротивнение c мощностью не менее Sadd";
    }
}