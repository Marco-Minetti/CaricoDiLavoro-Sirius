namespace BeaconAPI.classes.Power
{
    public interface IPowerManager
    {
        public Task<bool> Connect(string powerurl);
        public Task<List<PwrPlant>> GetPlants();
        public Task<List<PwrDevice>> GetDevices(PwrDeviceRequest req);
        public Task<List<PwrMeasure>> GetMeasures(PwrMeasureRequest req);
        public Task<List<PwrValue>> GetValues(PwrValueRequest pwrValueRequest);
    }
}
