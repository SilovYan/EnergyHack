using EnergyHack.Validators.Errors;

namespace EnergyHack.TransformerCheckers
{
    internal class RzaError : IError
    {
        public string Description => "Увеличьте сечение или мощность обомток ТТ";
    }
}