using EnergyHack.Validators.Errors;

namespace EnergyHack.TransformerCheckers
{
    internal class SectionError : IError
    {
        public string Description => "надо более мощный ТТ, увеличить сечение проводника";
    }
}