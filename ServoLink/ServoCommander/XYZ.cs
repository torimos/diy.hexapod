namespace ServoCommander
{
    partial class Program
    {
        struct XYZ
        {
            public double X;
            public double Y;
            public double Z;
            public XYZ(double x, double y, double z)
            {
                X = x; Y = y; Z = z;
            }

            public override string ToString()
            {
                return $"{X}:{Y}:{Z}";
            }
        }
    }
}
