using BeaconAPI.classes.DataBase;
using BeaconAPI.classes.Power;
using Microsoft.EntityFrameworkCore;

namespace BeaconAPITests.classes.Mock
{
    internal class DatabaseServiceMock : IDatabaseService
    {
        public List<PwrMeasure> AllMeasures = new List<PwrMeasure>();
        public List<PwrDevice> AllDevices = new List<PwrDevice>();

        public bool AddAvgDevStd(List<DLAvgDevSt> allAvgDevSt)
        {
            throw new NotImplementedException();
        }

        public bool AddDBSet(string keys)
        {
            throw new NotImplementedException();
        }

        public bool AddDevice(PwrDevice device)
        {
            throw new NotImplementedException();
        }

        public bool AddDevices(List<PwrDevice> devices)
        {
            AllDevices.AddRange(devices);
            return true;
        }

        public bool AddMeasures(List<PwrMeasure> measures)
        {
            AllMeasures.AddRange(measures);
            return true;
        }

        public Task<bool> InitAsync(string sse, string plant)
        {
            return Task.FromResult(true);
        }

        public List<DLAvgDevSt> ReadAvgDevSt()
        {
            throw new NotImplementedException();
        }

        public List<DLAvgDevSt> ReadAvgDevStAll()
        {
            throw new NotImplementedException();
        }

        public List<Point> ReadKMeans()
        {
            throw new NotImplementedException();
        }

        public List<PwrMeasure> ReadMeasures()
        {
            throw new NotImplementedException();
        }

        public bool StoreValues(string key, PwrMeasure measure, List<PwrValue> values, Dictionary<DateTimeOffset, PwrValue> statusValues, Dictionary<DateTimeOffset, PwrValue> downtimeValues)
        {
            throw new NotImplementedException();
        }
    }
}