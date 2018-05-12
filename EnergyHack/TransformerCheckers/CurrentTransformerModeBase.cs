namespace EnergyHack.TransformerCheckers
{
    public enum CurrentType
    {
        Aluminium = 345,
        Copper = 570
    }
    public abstract class CurrentTransformerModeBase
    {
        public double I2Nom { get; set; }
        public double S2Nom { get; set; }
        public CurrentType CurrentType { get; set; }
        public double CurrentLength { get; set; }
        public double CurrentS { get; set; }
        public double Rpr => CurrentLength / ((double) CurrentType / 10 * CurrentS);
    }
}