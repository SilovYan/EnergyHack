namespace EnergyHack.Validators.Errors
{
    public class AmperageError : IError
    {
        public string Description => "Максимальный рабочий ток должен быть меньше первичного номинального";
    }
}