namespace BeaconAPI.classes.DataBase
{
    public class DLCentroid
    {
        public string section { get; set; } = "";
        public string device { get; set; } = "";
        public string index { get; set; } = "";

        public long GMAddress1 { get; set; } = 0;
        public long GMAddress2 { get; set; } = 0;

        public int cluster { get; set; } = 0;

        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;

    }
}
