using BeaconAPI.classes.Power;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeaconAPITests.classes.Mock
{
    internal class PowerManagerMock : IPowerManager
    {
        public Task<bool> Connect(string powerurl)
        {
            return Task.FromResult(true);
        }

        public Task<List<PwrDevice>> GetDevices(PwrDeviceRequest req)
        {
            string filename = $"./data/{req.sse}.{req.plant}.devices.json";
            List<PwrDevice> devices = JsonConvert.DeserializeObject<List<PwrDevice>>(File.ReadAllText(filename));
            if (devices == null) devices = new List<PwrDevice>();

            return Task.FromResult(devices);
        }

        public Task<List<PwrMeasure>> GetMeasures(PwrMeasureRequest req)
        {
            string filename = $"./data/{req.sse}.{req.plant}.{req.device}.measures.json";
            List<PwrMeasure> measures = JsonConvert.DeserializeObject<List<PwrMeasure>>(File.ReadAllText(filename));
            if (measures == null) measures = new List<PwrMeasure>();

            return Task.FromResult(measures);
        }

        public Task<List<PwrPlant>> GetPlants()
        {
            throw new NotImplementedException();
        }

        public Task<List<PwrValue>> GetValues(PwrValueRequest pwrValueRequest)
        {
            throw new NotImplementedException();
        }
    }
}
