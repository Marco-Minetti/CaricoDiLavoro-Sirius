using Newtonsoft.Json;

namespace ProvaAPI.Classi
{
    public class Issue
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("$type")]
        public string type { get; set; }
        [JsonProperty("created")]
        public string data { get; set; }

        public static int contatore;
        public Issue(string id, string type, string data)
        {
            this.id = id;
            this.type = type;
            this.data = data;
            contatore++;
        }
        ~Issue()
        {
            contatore--;
        }
        public DateTime giveHour()
        {
            long tempo = long.Parse(data);
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(tempo).ToLocalTime();
            return dateTime;
        }
    }
}

