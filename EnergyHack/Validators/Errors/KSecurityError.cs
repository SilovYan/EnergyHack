using EnergyHack.Validators.Errors;

namespace EnergyHack.TransformerCheckers
{
    internal class KSecurityError : IError
    {
        public string Description =>
            "Коэф. безопасности для ТТ должен быть меньше или равен коэф. безопасности для оборудования";
    }
}