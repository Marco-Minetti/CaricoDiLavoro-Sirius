using BeaconAPI.classes.Power;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace BeaconAPI.classes.DataBase
{
    internal class DatabaseService : DbContext, IDatabaseService
    {
        public DbSet<PwrMeasure>? AllMeasures { get; set; }
        public DbSet<PwrDevice>? AllDevices { get; set; }
        public DbSet<DLValue>? AllValues { get; set; }

        public DbSet<DLAvgDevSt>? AllAvgDevSt { get; set; }
        public DbSet<DLCentroid>? AllCentroids { get; set; }

        private readonly DatabaseSettings DatabaseSettings;
        private readonly ILogger<DatabaseService> Logger;
        private ModelBuilder? builder;


        private string sse = "";
        private string plant = "";

        public DatabaseService(IOptions<DatabaseSettings> databaseSettings, ILogger<DatabaseService> logger)
        {
            DatabaseSettings = databaseSettings.Value;
            Logger = logger;
        }

        public async Task<bool> InitAsync(string sse, string plant)
        {
            this.sse = sse;
            this.plant = plant;
            DatabaseSettings.DatabaseName = $"datalake {sse}.{plant}"; //TODO: Creare il nuovo nome se già esiste

            if (!Directory.Exists(DatabaseSettings.DatabasePath))
            {
                Directory.CreateDirectory(DatabaseSettings.DatabasePath);
            }

            Logger.LogTrace("Init database... {0}", Database);
            
            await base.Database.EnsureDeletedAsync();
            if (await base.Database.EnsureCreatedAsync())
            {
                Logger.LogInformation("Database created: {0}", Database);
                return true;
            }

            return false;
        }


        public bool AddDevices(List<PwrDevice> devices)
        {
            AllDevices?.AddRange(devices);

            int cnt = SaveChanges();

            if (cnt > 0)
            {
                Logger.LogInformation($"Devices added: {cnt}");
                return true;
            }

            Logger.LogError("Error while adding devices");
            return false;
        }

        public bool AddDevice(PwrDevice device)
        {
            AllDevices?.Add(device);
            int cnt = SaveChanges();
            return false;
        }

        public bool AddMeasures(List<PwrMeasure> measures)
        {
            AllMeasures?.AddRange(measures);

            int cnt = SaveChanges();

            if (cnt > 0)
            {
                Logger.LogInformation($"Measures added: {measures[0].device} {cnt}");
                return true;
            }

            Logger.LogError("Error while adding measures");
            return false;
        }


        public bool AddAvgDevStd(List<DLAvgDevSt> list)
        {
            AllAvgDevSt?.AddRange(list);

            int cnt = SaveChanges();

            if (cnt > 0)
            {
                Logger.LogInformation($"AllAvgDevSt added: {cnt}");
                return true;
            }

            Logger.LogError("Error while adding AvgDevSt");
            return false;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source=\"{Path.Combine(DatabaseSettings.DatabasePath, DatabaseSettings.DatabaseName)}.sqlite3\"");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
                    
            modelBuilder.Entity<PwrMeasure>()
                .HasKey(nameof(PwrMeasure.sse), nameof(PwrMeasure.plant), nameof(PwrMeasure.subsystem), nameof(PwrMeasure.section), nameof(PwrMeasure.device), nameof(PwrMeasure.type), nameof(PwrMeasure.name));
            modelBuilder.Entity<PwrDevice>()
                .HasKey(nameof(PwrDevice.sse), nameof(PwrDevice.plant), nameof(PwrDevice.subsystem), nameof(PwrDevice.section), nameof(PwrDevice.device));

            modelBuilder.Entity<DLValue>()
                .HasKey(nameof(DLValue.GMAddress), nameof(DLValue.datetime));

            modelBuilder.Entity<DLAvgDevSt>()
                .HasKey(nameof(DLAvgDevSt.section), nameof(DLAvgDevSt.device),nameof(DLAvgDevSt.type), nameof(DLAvgDevSt.name));

            modelBuilder.Entity<DLCentroid>()
                .HasKey(nameof(DLCentroid.section), nameof(DLCentroid.device), nameof(DLCentroid.GMAddress1), nameof(DLCentroid.GMAddress2), nameof(DLCentroid.cluster));
            

              builder = modelBuilder;
        }

        public override string ToString()
        {
            return DatabaseSettings.DatabaseName;
        }

        public bool StoreValues(string key, PwrMeasure measure, List<PwrValue> values, Dictionary<DateTimeOffset, PwrValue> statusValues, Dictionary<DateTimeOffset, PwrValue> downtimeValues)
        {


            foreach(var pwrValue in values)
            {
                try
                {
                    DLValue dlValue = pwrValue.Adapt<DLValue>();
                    dlValue.datetime = DateTimeOffset.FromUnixTimeMilliseconds(pwrValue.timestamp);

                    if (statusValues.ContainsKey(dlValue.datetime) && statusValues[dlValue.datetime].valid)
                        dlValue.Status = Convert.ToInt16(statusValues[dlValue.datetime].value);
                    else
                    {
                        dlValue.Status = 7;
                        dlValue.valid = true;
                    }

                    if (downtimeValues.ContainsKey(dlValue.datetime) && downtimeValues[dlValue.datetime].valid)
                        dlValue.downtimeSec = 600;
                    else
                    {
                        dlValue.downtimeSec = 0;
                        dlValue.valid = true;
                    }

                    dlValue.GMAddress=measure.GMAddress;

                    AllValues?.Add(dlValue);
                }
                catch(Exception ex)
                {
                    Logger.LogError($"StoreValue: {ex.Message}");
                }
            }            
            int recordsAffected = SaveChanges();
            
            return recordsAffected > 0;
        }

        public List<PwrMeasure> ReadMeasures()
        {
            List<PwrMeasure> res=new List<PwrMeasure>();

            if(AllMeasures!=null)
                res= AllMeasures.ToList();

            return res;
        }

        public List<DLAvgDevSt> ReadAvgDevSt()
        {
            //List<DLAvgDevSt> res = Database.SqlQuery<DLAvgDevSt>($"SELECT section,device,type,name,avg(value) as avg,(AVG(value*value) - AVG(value)*AVG(value)) as devst  FROM AllValues inner join AllMeasures on AllValues.GMAddress= AllMeasures.GMAddress where  valid=1 and status = 2 and downtimeSec=0 group by  section,device,type,name order by devst desc;").ToList();
            List<DLAvgDevSt> res = new List<DLAvgDevSt>();
            if (AllAvgDevSt != null)
                res = RelationalQueryableExtensions.FromSql<DLAvgDevSt>(AllAvgDevSt, $"SELECT section,device,type,name,avg(value) as avg,(AVG(value*value) - AVG(value)*AVG(value)) as devst  FROM AllValues inner join AllMeasures on AllValues.GMAddress= AllMeasures.GMAddress where  valid=1 and status = 2 and downtimeSec=0 group by  section,device,type,name order by devst desc;").ToList();

            return res;
        }

        public List<DLAvgDevSt> ReadAvgDevStAll()
        {
            List<DLAvgDevSt> res = new List<DLAvgDevSt>();
            if (AllAvgDevSt != null)
                res = RelationalQueryableExtensions.FromSql<DLAvgDevSt>(AllAvgDevSt, $"SELECT 'all' as section,'all' as device,type,name,avg(value) as avg,(AVG(value*value) - AVG(value)*AVG(value)) as devst  FROM AllValues inner join AllMeasures on AllValues.GMAddress= AllMeasures.GMAddress where  valid=1 and status = 2 and downtimeSec=0 group by  type,name order by devst desc;").ToList();

            return res.ToList();
        }


        public List<DLCentroid> ReadKMeans()
        {
            List<DLCentroid> res = new List<DLCentroid>();
            Dictionary<string, List<PwrMeasure>> MeasureByDevice= ReadMeasuresByDevice();
            

            foreach (List<PwrMeasure> listaPerDevice in MeasureByDevice.Values)
            {
                Dictionary<string, bool> done = new Dictionary<string, bool>();
                foreach (PwrMeasure measure1 in listaPerDevice)
                {
                    foreach (PwrMeasure measure2 in listaPerDevice)
                    {
                        string index1, index2, indexAB, indexBA;
                        index1 = ($"{measure1.type}.{measure1.name}").ToLower();
                        index2 = ($"{measure2.type}.{measure2.name}").ToLower();

                        if (index1 == index2) continue;

                        indexAB = $"{index1}-{index2}";
                        indexBA = $"{index2}-{index1}";
                        
                        if (done.ContainsKey(indexAB)) { continue; }
                        if (done.ContainsKey(indexBA)) { continue; }
                        done[indexAB] = true;

                        Dictionary<DateTimeOffset, DLValue> m1 = ReadValuesByDT(measure1);
                        Dictionary<DateTimeOffset, DLValue> m2 = ReadValuesByDT(measure2);

                        List<Point> points = AggregateScatter(m1, m2);
                        KMeans km = new KMeans(20, points); //20 è il parametro di clusterizzazione
                        List<List<Point>> clusters = km.Cluster();
                        List<Point> cents = km.GetCentroids(clusters);

                        cents.Sort(delegate (Point p1, Point p2) 
                        {
                            double dist1 = Math.Sqrt((p1.X * p1.X) + (p1.Y * p1.Y));
                            double dist2 = Math.Sqrt((p2.X * p2.X) + (p2.Y * p2.Y));

                            if (dist1 < dist2) return -1;
                            if (dist1 > dist2) return 1;

                            if (p1.X < p2.X) return -1;
                            if (p1.X > p2.X) return 1;

                            if (p1.Y < p2.Y) return -1;
                            if (p1.Y > p2.Y) return 1;

                            return 0;
                        });

                        int i = 0;
                        foreach (var cent in cents)
                        {
                            if (Double.IsNaN(cent.X)) continue;
                            if (Double.IsNaN(cent.Y)) continue;

                            DLCentroid newCentr = new DLCentroid();
                            newCentr.section = measure1.section;
                            newCentr.device = measure1.device;
                            newCentr.index = indexAB;
                            newCentr.GMAddress1 = measure1.GMAddress;
                            newCentr.GMAddress2 = measure2.GMAddress;
                            newCentr.cluster = i++;
                            newCentr.X = cent.X;
                            newCentr.Y = cent.Y;
                            res.Add(newCentr);

                        }                        
                        Logger.LogInformation($"{measure1.section}.{measure1.device} -> {indexAB}: {clusters.Count} centroids, {points.Count} points");                        
                    }
                }
            }


            return res;
        }

        public bool AddKMeans(List<DLCentroid> centroids)
        {
            AllCentroids?.AddRange(centroids);

            int cnt = SaveChanges();

            if (cnt > 0)
            {
                Logger.LogInformation($"Centroids added: {cnt}");
                return true;
            }

            Logger.LogError("Error while adding Centroids");
            return false;
        }

        private List<Point> AggregateScatter(Dictionary<DateTimeOffset, DLValue> m1, Dictionary<DateTimeOffset, DLValue> m2)
        {
            List<Point> points = new List<Point>();
            foreach(DLValue item1 in m1.Values)
            {
                if (m2.ContainsKey(item1.datetime) == false) continue;
                points.Add(new Point(item1.value, m2[item1.datetime].value));
            }

            return points;
        }

        private Dictionary<DateTimeOffset, DLValue> ReadValuesByDT(PwrMeasure measure1)
        {
            Dictionary<DateTimeOffset, DLValue> res = new Dictionary<DateTimeOffset, DLValue>();
            var list= from t in AllValues where t.valid==true && t.downtimeSec==0 && t.Status==2 && t.GMAddress == measure1.GMAddress
                      select new DLValue
                      {
                          value=t.value,
                          //valid= t.valid,
                          datetime=t.datetime,
                         // downtimeSec=t.downtimeSec,
                         // GMAddress=t.GMAddress,
                          //Status = t.Status
                      }
                      ;
            foreach(var item in list)
            {
                res[item.datetime] = item;
            }

            return res;
        }

        private Dictionary<string, List<PwrMeasure>> ReadMeasuresByDevice()
        {
            Dictionary<string, List<PwrMeasure>> res = new Dictionary<string, List<PwrMeasure>>();
            List<PwrMeasure> lista = ReadMeasures();

            foreach (PwrMeasure measure in lista)
            {
                string index = $"{measure.device}";
                index = index.ToLower();
                if (res.ContainsKey(index)==false) res[index]=new List<PwrMeasure>();
                res[index].Add(measure);
            }

            return res;
        }


    }
}
