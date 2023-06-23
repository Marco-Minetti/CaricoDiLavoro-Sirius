using System.Data;

namespace BeaconAPI.classes
{
    public class Plant
    {
        public string Name { get; set; } = "";
        public string SSE { get; set; } = "";
        public double NominalPower { get; set; } = 0;

        public Plant() {
        }

        public bool from(DataRow row) 
        {
            if (row["sse"] == null) return false;

            SSE = $"{row["sse"]}";
            SSE=SSE.Trim();

            if (row["plant"] == null) return false;

            Name = $"{row["plant"]}";
            Name = Name.Trim();


            return true;
        }

    }
}
