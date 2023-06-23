namespace BeaconAPI.classes.DataBase
{
    public class DLAvgDevSt
    {
        public string section { get; set; } = "";
        public string device { get; set; } = "";
        public string type { get; set; } = "";
        public string name { get; set; } = "";
        public double avg { get; set; } = 0;
        public double devst { get; set; } = 0;
    }
}
