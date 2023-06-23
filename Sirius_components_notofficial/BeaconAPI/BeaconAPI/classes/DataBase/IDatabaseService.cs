using BeaconAPI.classes.Power;

namespace BeaconAPI.classes.DataBase
{
    public interface IDatabaseService
    {
        Task<bool> InitAsync(string sse, string plant);

        bool AddDevices(List<PwrDevice> devices);
        bool AddDevice(PwrDevice device);

        bool AddMeasures(List<PwrMeasure> measures);

        bool StoreValues(string key, PwrMeasure measure, List<PwrValue> values, Dictionary<DateTimeOffset, PwrValue> statusValues, Dictionary<DateTimeOffset, PwrValue> downtimeValues);
        
        List<PwrMeasure> ReadMeasures();
        List<DLAvgDevSt> ReadAvgDevSt();
        List<DLAvgDevSt> ReadAvgDevStAll();
        
        bool AddAvgDevStd(List<DLAvgDevSt> allAvgDevSt);
        List<DLCentroid> ReadKMeans();
        public bool AddKMeans(List<DLCentroid> centroids);
    }
}
