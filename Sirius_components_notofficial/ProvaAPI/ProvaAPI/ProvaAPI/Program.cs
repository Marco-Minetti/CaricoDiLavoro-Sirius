using System.Text.Json;
using System.Text.Json.Serialization;
using RestSharp;
using RestSharp.Authenticators;
using System.Net.Http.Headers;
using ProvaAPI.Classi;
using Newtonsoft.Json;

namespace ProvaAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var opzioni = new RestClientOptions("https://servizi.sirius.to.it/youtrack") 
            {
                Authenticator = new HttpBasicAuthenticator("AccessoBot", "Bot2023!")
            };
            var client = new RestClient(opzioni);
            // Il problema è che la data di creazione non sempre coincide con la data di inizio
            var request = new RestRequest("/api/issues?query=%23Unresolved, Start Date:-&fields=id,created,StartDate", Method.Get);
            //var request = new RestRequest("/api/workItems?fields=endDate");
            request.AddHeader("Accept", "application/json");
            var response = client.Execute(request);

            var json = response.Content;
            List<Issue> oggetti = JsonConvert.DeserializeObject<List<Issue>>(json);
            Console.WriteLine(Issue.contatore);
            Console.WriteLine(oggetti[0].giveHour());
        }
    }
}