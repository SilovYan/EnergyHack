namespace EnergyHack.Validators.Errors
{
    public class VoltageError : IError
    {
        public string Description => "Номинальное напряжение должно быть не меньше напряжения сети";
    }
}