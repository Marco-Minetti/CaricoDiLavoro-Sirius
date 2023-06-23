using BeaconAPI.classes;
using BeaconAPI.classes.DataBase;
using BeaconAPI.classes.Power;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.X86;
using System.Xml.Linq;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
builder.Services.AddHttpClient();

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));

builder.Services.AddTransient<IPowerManager, PowerManager>();
builder.Services.AddTransient<IDatabaseService, DatabaseService>();
builder.Services.AddTransient<DataLakeManager>();

builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("corsapp");


string FixedPowerUrl = "http://10.100.0.151/vireoxpower/api";


app.MapGet("/plants", async (string powerurl) =>
{
    IPowerManager pm = app.Services.GetRequiredService<IPowerManager>();

    powerurl = FixedPowerUrl;
    bool connected = await pm.Connect(powerurl);

    List<PwrPlant> plants = await pm.GetPlants();
    var resultPlants = plants.Adapt<List<PlantApiResponse>>();

    return resultPlants;
});

app.MapGet("/devices", async ([FromQuery] string powerurl, [FromQuery] string sse, [FromQuery] string plant, [FromQuery] bool addictionalInfo) =>
{
    IPowerManager pm = app.Services.GetRequiredService<IPowerManager>();

    powerurl = FixedPowerUrl;
    bool connected = await pm.Connect(powerurl);

    List<PwrDevice> devices = await pm.GetDevices(new PwrDeviceRequest(sse, plant, false));
    var resultDevices = devices.Adapt<List<DeviceApiResponse>>();

    return resultDevices;
});


app.MapGet("/measures", async ([FromQuery] string powerurl, [FromQuery] string sse, [FromQuery] string plant, [FromQuery] string subsystem, [FromQuery] string section, [FromQuery] string device) =>
{

    IPowerManager pm = app.Services.GetRequiredService<IPowerManager>();

    powerurl = FixedPowerUrl;
    bool connected = await pm.Connect(powerurl);

    List<PwrMeasure> measures = await pm.GetMeasures(new PwrMeasureRequest(sse, plant, subsystem, section, device));
    var resultDevices = measures.Adapt<List<MeasureApiResponse>>();

    return resultDevices;
});



app.MapPost("/datalake", async (DatalakeRequest body) =>
{


    body.powerurl = FixedPowerUrl;
    DataLakeManager dl = app.Services.GetRequiredService<DataLakeManager>();
    bool connected = await dl.connect(body.powerurl);
    if (connected == false) return false;


    DateTimeOffset dtStart = DateTimeOffset.Parse("2023/01/01");
    DateTimeOffset dtStop = DateTimeOffset.Parse("2023/03/01");
    bool isOk=await dl.Extract(body.sse, body.plant, dtStart, dtStop);
    //Aggiungo il device medio
    isOk &= dl.ComputeAvdDevSt();

    isOk &= dl.ComputeKMeans();
    return isOk;
})

.WithName("BeaconAPI")
.WithOpenApi();



app.Run();

class DatalakeRequest
{
    public string powerurl { get; set; } = "";
    public string sse { get; set; } = "";
    public string plant { get; set; } = "";
    public string dateStart { get; set; } = "";
    public string dateStop { get; set; } = "";
};

