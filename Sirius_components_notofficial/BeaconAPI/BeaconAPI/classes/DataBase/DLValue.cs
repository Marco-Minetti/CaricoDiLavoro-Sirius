using BeaconAPI.classes.Power;

namespace BeaconAPI.classes.DataBase
{


    public class DLValue
    {
      //  public PwrMeasure measure { get; set; } = new PwrMeasure();
        public DateTimeOffset datetime = DateTimeOffset.MinValue;
        public double value { get; set; } = 0;
        public bool valid { get; set; } = false;
        public int downtimeSec{ get; set; } = 0;
        public int Status { get; set; } = 0;

   //     public string message { get; set; } = "";
    //    public string agent { get; set; } = "";

        public long GMAddress { get; set; } = 0;
        
        

    }
}
