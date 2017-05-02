namespace ServoCommander
{
    public struct XYZ
    {
        public double x;
        public double y;
        public double z;
        public XYZ(double x, double y, double z)
        {
            this.x = x; this.y = y; this.z = z;
        }

        public override string ToString()
        {
            return $"{x,4}:{y,4}:{z,4}";
        }
    }

    public struct CoxaFemurTibia
    {
        public double Coxa;
        public double Femur;
        public double Tibia;
        public CoxaFemurTibia(double coxa, double femur, double tibia)
        {
            Coxa = coxa; Femur = femur; Tibia = tibia;
        }

        public string ToString(string fmt)
        {
            return $"{Coxa.ToString(fmt),6}:{Femur.ToString(fmt),6}:{Tibia.ToString(fmt),6}";
        }

        public override string ToString()
        {
            return ToString("N1");
        }
    }
}
