namespace BeaconAPI.classes
{
    public class PlantApiResponse
    {
        public string name { get; set; } = "";
        public string plant { get; set; } = "";
        public string sse { get; set; } = "";
        public string type { get; set; } = "";
    }


    public class DeviceApiResponse
    {        
        public string sse { get; set; } = "";
        public string plant { get; set; } = "";
        public string subsystem { get; set; } = "";
        public string section { get; set; } = "";                           
        public string device { get; set; } = "";

        public string deviceType { get; set; } = "";
        public string up { get; set; } = "";
        public string asset { get; set; } = "";
        public string ecName { get; set; } = "";
        public double nominalPower { get; set; } = 0;
    }


    public class MeasureApiResponse
    {
        public string sse { get; set; } = "";
        public string plant { get; set; } = "";
        public string subsystem { get; set; } = "";
        public string section { get; set; } = "";
        public string device { get; set; } = "";
        public string type { get; set; } = "";
        public string name { get; set; } = "";
        public string units { get; set; } = "";
    }

}
