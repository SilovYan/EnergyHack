namespace EnergyHack.TransformerCheckers
{
    public enum AccuracyClass
    {
        Comercial,
        Technical,
        Indicating
    }

    public class CurrentTransformerAccountingMode : CurrentTransformerModeBase
    {
        public AccuracyClass AccuracyClass { get; set; }
        public double Spr { get; set; }
        public double Rk { get; set; } = 0;
        public double Sadd { get; set; } = 0;

        public double S2 => Spr + (I2Nom * I2Nom) * (Rpr + Rk);
        public double K => (S2 + Sadd) / S2Nom;

    }
}