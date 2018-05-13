using System;

namespace EnergyHack.TransformerCheckers
{
    public class CurentTransformerRzaMode : CurrentTransformerModeBase
    {
        public double? I1Nom { get; set; }
        public double? Kn { get; set; }
        public double? Zn => Sn / (I2Nom * I2Nom);
        public double? Sn { get; set; }
        public double? Zr { get; set; }
        public double? Ikz { get; set; }
        public double? Rk { get; set; }

        public double? Z2 => S2Nom / (I2Nom * I2Nom);

        public double? Znfact => Zr + Rpr + Rk;

        public double? K10 => Ikz / I1Nom;

        public double? K10dop => Kn * (Z2 + Zn) / (Z2 + Znfact);
    }
}