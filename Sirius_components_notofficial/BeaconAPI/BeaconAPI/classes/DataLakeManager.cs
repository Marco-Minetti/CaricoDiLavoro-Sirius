using BeaconAPI.classes.DataBase;
using BeaconAPI.classes.Power;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.X86;

namespace BeaconAPI.classes
{
    public class DataLakeManager
    {
        private readonly ILogger<DataLakeManager> Logger;

        private string powerurl = "";
        private IPowerManager pm;
        private IDatabaseService db;
        public bool connected = false;
        public string sse = "";
        public string plant = "";

        public DataLakeManager(IPowerManager powerManager, ILogger<DataLakeManager> logger, IDatabaseService db)
        {
            this.Logger = logger;
            this.pm = powerManager;
            this.db = db;
        }

        public async Task<bool> connect(string powerurl)
        {
            this.powerurl = powerurl;
            bool connected = await pm.Connect(powerurl);
            return connected;
        }


        public async Task<bool> Extract(string sse, string plant, DateTimeOffset dateStart, DateTimeOffset dateStop)
        {
            this.sse = sse;
            this.plant = plant;

            List<PwrDevice> devices = await getValidDevices();
            if (devices.Count == 0) { Logger.LogError("No device found"); return false; }


            Dictionary<string, List<PwrMeasure>> allMeasuresByTN = await getAllMeasuresByTN(devices);
            if (allMeasuresByTN.Count == 0) { Logger.LogError("No measure compatible found"); return false; }

            Dictionary<string, List<PwrMeasure>> allMeasuresByDevice = getAllMeasuresByDevice(allMeasuresByTN);


            bool created = await db.InitAsync(sse, plant);
            if (created == false) { Logger.LogError("Database NOT initialized"); return false; }


            //Creo le anagrafiche
            bool res = db.AddDevices(devices);
            int gma = 1;
            foreach (var measures in allMeasuresByDevice.Values)
            {
                //battezzo gli indirizzi
                foreach (var m in measures)
                {
                    m.GMAddress = gma;
                    gma++;
                }

                res = db.AddMeasures(measures);
            }



            //Leggo i valori
            foreach (var pair in allMeasuresByDevice)
            {
                //Una volta per device leggo i valori degli stati
                PwrMeasure Status = new PwrMeasure();
                Status.clone(pair.Value.First<PwrMeasure>());
                Status.GMAddress = 0;
                if (Status.subsystem.ToLower() == "wpp")
                {
                    Status.type = "DMAV";
                    Status.name = "WTG Status";
                }
                if (Status.subsystem.ToLower() == "pvp")
                {
                    Status.type = "QAV";
                    Status.name = "INV Status";
                }
                Dictionary<DateTimeOffset, PwrValue> StatusValues = await ExtractValueByTime(dateStart, dateStop, Status);

                PwrMeasure Downtime = new PwrMeasure();
                Downtime.clone(pair.Value.First<PwrMeasure>());
                Downtime.GMAddress = 0;
                if (Downtime.subsystem.ToLower() == "wpp")
                {
                    Downtime.type = "DMAV";
                    Downtime.name = "downtime";
                }
                if (Downtime.subsystem.ToLower() == "pvp")
                {
                    Downtime.type = "QAV";
                    Downtime.name = "downtime";
                }
                Dictionary<DateTimeOffset, PwrValue> DowntimeValues = await ExtractValueByTime(dateStart, dateStop, Downtime);

                foreach (var measure in pair.Value)
                {
                    List<PwrValue> values = await getValues(measure, dateStart, dateStop);
                    if (values.Count == 0) { Logger.LogError($"No values found for {measure.device}.{measure.type}.{measure.name}"); continue; }

                    db.StoreValues(pair.Key, measure, values, StatusValues, DowntimeValues); //crea la tabella e salva
                }
            }


            return true;
        }

        private async Task<Dictionary<DateTimeOffset, PwrValue>> ExtractValueByTime(DateTimeOffset dateStart, DateTimeOffset dateStop, PwrMeasure source)
        {
            List<PwrValue> StatusList = await getValues(source, dateStart, dateStop);
            Dictionary<DateTimeOffset, PwrValue> dest = new Dictionary<DateTimeOffset, PwrValue>();
            foreach (var value in StatusList)
            {
                dest.Add(DateTimeOffset.FromUnixTimeMilliseconds(value.timestamp), value);
            }

            return dest;
        }

        //Converte l'array in modo che sia più veloce da gestire per le letture degli stati
        private Dictionary<string, List<PwrMeasure>> getAllMeasuresByDevice(Dictionary<string, List<PwrMeasure>> allMeasuresByTN)
        {
            Dictionary<string, List<PwrMeasure>> result = new Dictionary<string, List<PwrMeasure>>();
            foreach (var lista in allMeasuresByTN.Values)
            {
                foreach (var measure in lista)
                {
                    string index = $"{measure.section.ToLower()}.{measure.device.ToLower()}";
                    string type = measure.type.ToUpper();

                    if (result.ContainsKey(index) == false) result[index] = new List<PwrMeasure>();

                    result[index].Add(measure);
                }
            }

            return result;
        }

        private async Task<List<PwrValue>> getValues(PwrMeasure measure, DateTimeOffset dateStart, DateTimeOffset dateStop)
        {
            List<PwrValue> values = await pm.GetValues(new PwrValueRequest(measure.sse, measure.plant, measure.subsystem, measure.section, measure.device, measure.type, measure.name, dateStart.ToString("yyyy-MM-dd"), dateStop.ToString("yyyy-MM-dd")));

            return values;
        }

        /// <summary>
        /// Restituisce tutte le misure periodiche che sono presenti su tutti i device, raggruppati per type.name
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        async public Task<Dictionary<string, List<PwrMeasure>>> getAllMeasuresByTN(List<PwrDevice> devices)
        {
            Dictionary<string, List<PwrMeasure>> allMeasures = new Dictionary<string, List<PwrMeasure>>();
            int gma = 1;
            foreach (PwrDevice device in devices)
            {
                List<PwrMeasure> measures = await pm.GetMeasures(new PwrMeasureRequest(sse, plant, device.subsystem, device.section, device.device));
                foreach (PwrMeasure measure in measures)
                {
                    string index = $"{measure.type.ToLower()}.{measure.name.ToLower()}";
                    string type = measure.type.ToUpper();
                    if ((type != "DMAV") && (type != "QMAV") && (type != "DE") && (type != "QE") // && (type != "CNT") && (type != "QCNT")
                        && (type != "DMMAX") && (type != "QMMAX") && (type != "DMMIN") && (type != "QMMIN") && (type != "DMSD") && (type != "QMSD"))
                        continue;

                    //if ((measure.name.ToLower() != "active power") && (measure.name.ToLower() != "wind speed")) continue; //da togliere

                    if (allMeasures.ContainsKey(index) == false) allMeasures[index] = new List<PwrMeasure>();
                    measure.GMAddress = gma++;


                    allMeasures[index].Add(measure);
                }
            }

            int n = devices.Count;
            foreach (var pair in allMeasures)
            {
                if (pair.Value.Count != n)
                    allMeasures.Remove(pair.Key);
            }

            return allMeasures;
        }

        async public Task<List<PwrDevice>> getValidDevices()
        {
            List<PwrDevice> DevicesFromPower = await pm.GetDevices(new PwrDeviceRequest(sse, plant, false));

            List<PwrDevice> devices = new List<PwrDevice>();
            foreach (PwrDevice dev in DevicesFromPower)
            {
                if ((dev.deviceType.ToLower() != "wtg") && (dev.deviceType.ToLower() != "inv")) continue;
                devices.Add(dev);

                //if (devices.Count == 2) break; //Da togliere
            }

            return devices;
        }

        public bool ComputeAvdDevSt()
        {
            List<DLAvgDevSt> allAvgDevSt = db.ReadAvgDevSt();
            db.AddAvgDevStd(allAvgDevSt);

            List<DLAvgDevSt> allAvgDevAll = db.ReadAvgDevStAll();
            db.AddAvgDevStd(allAvgDevAll);

            return true;
        }

        internal bool ComputeKMeans()
        {
            List<DLCentroid> allKMeans = db.ReadKMeans();
            db.AddKMeans(allKMeans);

            return true;
        }
    }
}


