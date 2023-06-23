using Newtonsoft.Json;
using RestSharp.Authenticators;
using RestSharp;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BeaconAPI.classes.Power;

public class PowerManager : IPowerManager
{
    #region ATTRIBUTI

    private string PowerPath = "";
    private HttpClient HttpClient;

    #endregion ATTRIBUTI

    /*          
            ██████╗ ██╗   ██╗██████╗ ██╗     ██╗ ██████╗    ███╗   ███╗███████╗████████╗██╗  ██╗ ██████╗ ██████╗ ███████╗
            ██╔══██╗██║   ██║██╔══██╗██║     ██║██╔════╝    ████╗ ████║██╔════╝╚══██╔══╝██║  ██║██╔═══██╗██╔══██╗██╔════╝
            ██████╔╝██║   ██║██████╔╝██║     ██║██║         ██╔████╔██║█████╗     ██║   ███████║██║   ██║██║  ██║███████╗
            ██╔═══╝ ██║   ██║██╔══██╗██║     ██║██║         ██║╚██╔╝██║██╔══╝     ██║   ██╔══██║██║   ██║██║  ██║╚════██║
            ██║     ╚██████╔╝██████╔╝███████╗██║╚██████╗    ██║ ╚═╝ ██║███████╗   ██║   ██║  ██║╚██████╔╝██████╔╝███████║
            ╚═╝      ╚═════╝ ╚═════╝ ╚══════╝╚═╝ ╚═════╝    ╚═╝     ╚═╝╚══════╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝ ╚═════╝ ╚══════╝
    */

    private readonly ILogger<PowerManager> Logger;
    private string PowerUrl = "";
    private RestClient client;

    public PowerManager(IHttpClientFactory httpClientFactory, ILogger<PowerManager> logger)
    {
        HttpClient = httpClientFactory.CreateClient("PowerManagerClient");
        Logger = logger;

        client = new RestClient();
    }




    public async Task<bool> Connect(string powerurl)
    {
        Logger.LogInformation($"Connecting {powerurl}");

        client = new RestClient(powerurl);

        RestRequest request = new RestRequest("authenticator", Method.Post);
        request.Timeout = 2000;

        request.AddParameter("username", "sirius");
        request.AddParameter("password", "s1r1us11235");

        RestResponse response = await client.ExecuteAsync(request);

        if (response.ResponseStatus != ResponseStatus.Completed)
        {
            Logger.LogError($"Non connesso a {response.Content}");
            return false;
        }

        if (response.Content == null)
        {
            Logger.LogError($"Non connesso: nessuna risposta");
            return false;
        }

        AuthResponse? authResponse = JsonSerializer.Deserialize<AuthResponse>(response.Content);
        if (authResponse == null)
        {
            Logger.LogError($"Non connesso: json non pasrificabile");
            return false;
        }
        Logger.LogInformation($"Connesso a {authResponse.status}: {authResponse.message}");


        client.AddDefaultHeader("Authorization", $"Bearer {authResponse.data.access_token}");
        

        return true;
    }

    public async Task<List<PwrPlant>> GetPlants()
    {
        Logger.LogInformation($"GetPlants");
        List<PwrPlant> res= new List<PwrPlant>();

        RestRequest request = new RestRequest("getPlants", Method.Post);
        request.Timeout = 10000;

        RestResponse response = await client.ExecuteAsync(request);

        if (response.ResponseStatus != ResponseStatus.Completed)
        {
            Logger.LogError($"Risposta errata {response.Content}");
            return res;
        }

        if (response.Content == null)
        {
            Logger.LogError($"Nessuna risposta");
            return res;
        }

        GetPlantResponse? getPlantResponse = JsonSerializer.Deserialize<GetPlantResponse>(response.Content);
        if (getPlantResponse == null)
        {
            Logger.LogError($" json non pasrificabile");
            return res;
        }


        return getPlantResponse.data.plants;
    }


    public async Task<List<PwrDevice>> GetDevices(PwrDeviceRequest req)
    {
        Logger.LogInformation($"GetDevices: {req.sse}.{req.plant}");
        List<PwrDevice> res = new List<PwrDevice>();

        RestRequest request = new RestRequest("getDevices", Method.Post);
        request.Timeout = 10000;
        request.AddStringBody( JsonSerializer.Serialize(req),ContentType.Plain);
        
        RestResponse response = await client.ExecuteAsync(request);

        if (response.ResponseStatus != ResponseStatus.Completed)
        {
            Logger.LogError($"Risposta errata {response.Content}");
            return res;
        }

        if (response.Content == null)
        {
            Logger.LogError($"Nessuna risposta");
            return res;
        }

        GetDeviceResponse? getDeviceResponse = JsonSerializer.Deserialize<GetDeviceResponse>(response.Content);
        if (getDeviceResponse == null)
        {
            Logger.LogError($"json non pasrificabile");
            return res;
        }


        return getDeviceResponse.data.devices;
    }

    public async Task<List<PwrMeasure>> GetMeasures(PwrMeasureRequest req)
    {
        Logger.LogInformation($"GetMeasures: {req.sse}.{req.plant}.{req.subsystem}.{req.section}.{req.device}");
        List<PwrMeasure> res = new List<PwrMeasure>();

        RestRequest request = new RestRequest("getMeasures", Method.Post);
        request.Timeout = 10000;
        request.AddStringBody(JsonSerializer.Serialize(req), ContentType.Plain);

        RestResponse response = await client.ExecuteAsync(request);

        if (response.ResponseStatus != ResponseStatus.Completed)
        {
            Logger.LogError($"Risposta errata {response.Content}");
            return res;
        }

        if (response.Content == null)
        {
            Logger.LogError($"Nessuna risposta");
            return res;
        }

        GetMeasureResponse? getMeasureResponse = JsonSerializer.Deserialize<GetMeasureResponse>(response.Content);
        if (getMeasureResponse == null)
        {
            Logger.LogError($"json non pasrificabile");
            return res;
        }


        return getMeasureResponse.data.measures;
    }


    public async Task<List<PwrValue>> GetValues(PwrValueRequest req)
    {
        Logger.LogInformation($"GetMeasures: {req.section}.{req.device}.{req.type}.{req.name} [{req.date_start},{req.date_stop}]");
        List<PwrValue> res = new List<PwrValue>();

        RestRequest request = new RestRequest("getDataByDate", Method.Post);
        request.Timeout = 10000;
        request.AddStringBody(JsonSerializer.Serialize(req), ContentType.Plain);

        RestResponse response = await client.ExecuteAsync(request);

        if (response.ResponseStatus != ResponseStatus.Completed)
        {
            Logger.LogError($"Risposta errata {response.Content}");
            return res;
        }

        if (response.Content == null)
        {
            Logger.LogError($"Nessuna risposta");
            return res;
        }

        GetValueResponse? getValueResponse = JsonSerializer.Deserialize<GetValueResponse>(response.Content);
        if (getValueResponse == null)
        {
            Logger.LogError($"json non pasrificabile");
            return res;
        }


        return getValueResponse.data;
    }
}


public class AuthData
{
    public string access_token { get; set; } = "";
    public string refresh_token { get; set; } = "";
    public Int64 expires_at { get; set; } = 0;
}

public class AuthResponse
{
    public string status { get; set; } = "";
    public string message { get; set; } = "";
    public AuthData data { get; set; } = new AuthData();
}


public class PlantData
{
    public List<PwrPlant> plants { get; set; } = new List<PwrPlant>();
}

public class PwrPlant
{
    public string name { get; set; } = "";
    public string description { get; set; } = "";
    public string plant { get; set; } = "";
    public string sse { get; set; } = "";
    public string state { get; set; } = "";
    public string region { get; set; } = "";
    public string district { get; set; } = "";
    public string up { get; set; } = "";
    public string asset { get; set; } = "";
    public string nominalPower { get; set; } = "";
    public string type { get; set; } = "";
}

public class GetPlantResponse
{
    public string status { get; set; } = "";
    public PlantData data { get; set; } = new PlantData();
}


public class DeviceData
{
    public List<PwrDevice> devices { get; set; } = new List<PwrDevice>();
}

public class PwrDevice
{
    public string name { get; set; } = "";
    public string description { get; set; } = "";
    public string plant { get; set; } = "";
    public string sse { get; set; } = "";
    public string state { get; set; } = "";
    public string region { get; set; } = "";
    public string district { get; set; } = "";
    public string up { get; set; } = "";
    public string asset { get; set; } = "";
    public string nominalPower { get; set; } = "";
    public string device { get; set; } = "";
    public string subsystem { get; set; } = "";
    public string section { get; set; } = "";
    public string model { get; set; } = "";
    public string serial { get; set; } = "";
    public string altitude { get; set; } = "";
    public string longitude { get; set; } = "";
    public string latitude { get; set; } = "";
    public string year { get; set; } = "";
    public string deviceType { get; set; } = "";
    public string ecName { get; set; } = "";
    public string notes { get; set; } = "";
    public string vendor { get; set; } = "";
    public string municipality { get; set; } = "";

    public PwrDevice(string sse,  string plant,   string subsystem, string section, string device)
    {
        this.sse = sse;
        this.plant = plant;
        this.subsystem = subsystem;
        this.section = section;
        this.device = device;
    }
}

public class GetDeviceResponse
{
    public string status { get; set; } = "";
    public DeviceData data { get; set; } = new DeviceData();
}

public class PwrDeviceRequest
{
    public PwrDeviceRequest(string sse, string plant, bool addictionalInfo)
    {
        this.sse = sse;
        this.plant = plant;
        this.addictionalInfo = addictionalInfo;
    }

    public string plant { get; set; } = "";
    public string sse { get; set; } = "";
    public bool  addictionalInfo { get; set; } = false;
}


public class MeasureData
{
    public List<PwrMeasure> measures { get; set; } = new List<PwrMeasure>();
}

public class PwrMeasure
{
    public string units { get; set; } = "";
    public string sse { get; set; } = "";
    public string plant { get; set; } = "";
    public string subsystem { get; set; } = "";
    public string section { get; set; } = "";
    public string device { get; set; } = "";
    public string type { get; set; } = "";
    public string name { get; set; } = "";

    public long GMAddress { get; set; } = 0;

    
    public void clone(PwrMeasure other) 
    {
        this.units = other.units;
        this.sse = other.sse;
        this.plant = other.plant;
        this.subsystem = other.subsystem;
        this.section = other.section;
        this.device = other.device;
        this.type = other.type;
        this.name = other.name; 
        this.GMAddress = other.GMAddress;   
      
    }
}

public class GetMeasureResponse
{
    public string status { get; set; } = "";
    public MeasureData data { get; set; } = new MeasureData();
}


public class PwrMeasureRequest
{
    public PwrMeasureRequest(string sse, string plant, string subsystem, string section, string device)
    {
        this.sse = sse;
        this.plant = plant;
        this.subsystem = subsystem;
        this.section = section;
        this.device = device;
    }

    public string sse { get; set; } = "";
    public string plant { get; set; } = "";
    public string subsystem { get; set; } = "";
    public string section { get; set; } = "";
    public string device { get; set; } = "";
}


public class PwrValueRequest
{
    public PwrValueRequest(string sse, string plant, string subsystem, string section, string device, string type, string name, string date_start, string date_stop)
    {
        this.sse = sse;
        this.plant = plant;
        this.subsystem = subsystem;
        this.section = section;
        this.device = device;
        this.type = type;
        this.name = name;
        this.date_start = date_start;
        this.date_stop = date_stop;
    }

    public string sse { get; set; } = "";
    public string plant { get; set; } = "";
    public string subsystem { get; set; } = "";
    public string section { get; set; } = "";
    public string device { get; set; } = "";
    public string type { get; set; } = "";
    public string name { get; set; } = "";
    public string date_start { get; set; } = "";
    public string date_stop { get; set; } = "";
    public string time_zone { get; set; } = "Europe/Rome";

    public bool as_array { get; set; } = true;
}


public class GetValueResponse
{
    public string status { get; set; } = "";
    //public ValueData data { get; set; } = new ValueData();
    public List<PwrValue> data { get; set; } = new List<PwrValue>();
}

public class ValueData
{
    public List<PwrValue> values { get; set; } = new List<PwrValue>();
}

public class PwrValue
{
    public bool valid { get; set; } = false;
    public double value { get; set; } = 0;
    public Int64 timestamp { get; set; } = 0;
    public string date { get; set; } = "";
    public string message { get; set; } = "";
    public string usr { get; set; } = "";
    public string agent { get; set; } = "";
    public string source { get; set; } = "";
    public int stAlarm { get; set; } = 0;
    public int stAck { get; set; } = 0;
    public string labAlarm { get; set; } = "";
    //public DateTimeOffset  datetime = DateTimeOffset.MinValue;

}