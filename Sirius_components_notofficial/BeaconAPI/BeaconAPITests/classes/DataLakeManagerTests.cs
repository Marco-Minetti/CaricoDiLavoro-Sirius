using Microsoft.VisualStudio.TestTools.UnitTesting;
using BeaconAPI.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Intrinsics.X86;
using BeaconAPI.classes.Power;
using BeaconAPITests.classes;
using BeaconAPITests.classes.Mock;
using BeaconAPI.classes.DataBase;

namespace BeaconAPI.classes.Tests
{
    [TestClass()]
    public class DataLakeManagerTests
    {
        public string powerurl = "http://10.100.0.151/vireoxpower/api";

        [TestMethod()]
        async public Task GetValidDevices1()
        {
            IPowerManager powerManager = new PowerManagerMock();
            IDatabaseService database = new DatabaseServiceMock();


            DataLakeManager dl = new DataLakeManager(powerManager, MockLogger.Create<DataLakeManager>(), database);
            bool connected = await dl.connect(powerurl);
            Assert.IsTrue(connected, $"Connected to {powerurl}");

            dl.sse = "SSEAmaroni";
            dl.plant = "AMAR";
            List<PwrDevice> devices = await dl.getValidDevices();
            Assert.AreEqual(2, devices.Count);

            //bool isOk = await dl.Extract("SSEAmaroni", "AMAR");

        }


        [TestMethod()]
        async public Task getAllCyclicMeasuresByTN1()
        {
            IPowerManager powerManager = new PowerManagerMock();
            IDatabaseService database = new DatabaseServiceMock();


            DataLakeManager dl = new DataLakeManager(powerManager, MockLogger.Create<DataLakeManager>(), database);
            bool connected = await dl.connect(powerurl);


            dl.sse = "SSEAmaroni";
            dl.plant = "AMAR";
            List<PwrDevice> devices = await dl.getValidDevices();
            Dictionary<string, List<PwrMeasure>> measures = await dl.getAllMeasuresByTN(devices);
            Assert.AreEqual(68, measures.Count);

        }


        [TestMethod()]
        async public Task Extract1()
        {
            IPowerManager powerManager = new PowerManagerMock();
            IDatabaseService database = new DatabaseServiceMock();


            DataLakeManager dl = new DataLakeManager(powerManager, MockLogger.Create<DataLakeManager>(), database);
            bool connected = await dl.connect(powerurl);


            dl.sse = "SSEAmaroni";
            dl.plant = "AMAR";
            DateTimeOffset dtStart = DateTimeOffset.Parse("2022/11/01 00:00");
            DateTimeOffset dtStop = DateTimeOffset.Parse("2023/01/01 00:00");

            bool res = await dl.Extract(dl.sse, dl.plant, dtStart, dtStop);

            Assert.AreEqual(true, res);

        }
    }
}