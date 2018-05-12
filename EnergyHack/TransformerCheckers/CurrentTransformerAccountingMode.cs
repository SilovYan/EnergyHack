﻿namespace EnergyHack.TransformerCheckers
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
        public string Accuracy { get; set; }
        public double Spr { get; set; }
        public bool RkVisible => SaddVisible;
        public double Rk { get; set; } = 0;
        public double Sadd { get; set; }
        public bool SaddVisible => !string.IsNullOrEmpty(Accuracy) && Accuracy.StartsWith("3") ? K < 0.5 : K < 0.25;

        public double S2 => Spr + (I2Nom * I2Nom) * (Rpr + Rk);
        public double K => (S2 + Sadd) / S2Nom;

        public void SetSadd()
        {
            Sadd = 0.5 * S2Nom - S2;
        }
    }
}