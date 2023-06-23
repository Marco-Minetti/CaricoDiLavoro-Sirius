using System.Text.Json;
using System.Text.Json.Serialization;
using RestSharp;
using RestSharp.Authenticators;
using System.Net.Http.Headers;
using ProvaAPI.Classi;
using Newtonsoft.Json;
using SMYT.YouTrack;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System.ComponentModel.Design.Serialization;

namespace ProvaAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /*===============Richiesta del JSON da YouTrack========================*/
            var opzioni = new RestClientOptions("https://servizi.sirius.to.it/youtrack")
            {
                Authenticator = new HttpBasicAuthenticator("AccessoBot", "Bot2023!")
            };
            var client = new RestClient(opzioni);
            // tecnicamente questa richiesta si può semplificare.
            var request = new RestRequest("/api/issues?query=%23Unresolved%20Due%20Date:-%7BNo%20due%20date%7D%20Start%20Date:-%7BNo%20start%20date%7D&fields=id,reporter(@user),resolved,updated,created,fields(value(id,name,description,localizedName,archived,color(@color),minutes,presentation,text,ringId,login,fullName,avatarUrl,online,banned,canReadProfile,allUsersGroup,icon),id,$type,hasStateMachine,isUpdatable,projectCustomField($type,id,field(id,name,aliases,localizedName,fieldType(valueType,isBundleType,isMultiValue)),bundle(id),canBeEmpty,emptyFieldText,ordinal,isSpentTime)),project(id,ringId,name,shortName,iconUrl,template,pinned,archived,isDemo,hasArticles,team(@permittedGroups),plugins(timeTrackingSettings(id,enabled),helpDeskSettings(enabled))),visibility($type,implicitPermittedUsers(@user),permittedGroups(@permittedGroups),permittedUsers(@user)),tags(id,name,query,issuesUrl,color(@color),isDeletable,isShareable,isUpdatable,isUsable,owner(@user),readSharingSettings(@updateSharingSettings),tagSharingSettings(@updateSharingSettings),updateSharingSettings(@updateSharingSettings)),watchers(hasStar),usersTyping(timestamp,user(@user)),idReadable,summary;@updateSharingSettings:permittedGroups(@permittedGroups),permittedUsers(@user);@user:id,ringId,name,login,fullName,avatarUrl,online,banned,canReadProfile;@permittedGroups:id,name,ringId,allUsersGroup,icon;@color:id,background,foreground", Method.Get);

            request.AddHeader("Accept", "application/json");
            var response = client.Execute(request);

            var json = response.Content;

            /*==============Parsing del JSON dentro ad una lista===============*/
            List<IssueSemplice> Lista = new List<IssueSemplice>();
            IssueSemplice coso;
            JArray jsonArray = JArray.Parse(json);

            foreach (JToken issueToken in jsonArray)
            {

                coso = new IssueSemplice();
                coso.Fields = new Value();
                string idReadable = issueToken["idReadable"].ToString();
                string summary = issueToken["summary"].ToString();
                var progetto = issueToken["project"];
                var team = progetto["team"];
                coso.Fields.team = team["name"].ToString();

                coso.idReadable = idReadable;
                coso.summary = summary;

                JArray fieldsArray = (JArray)issueToken["fields"];
                for (int i = 0; i < fieldsArray.Count; i++)
                {
                    JToken valueToken = fieldsArray[i]["value"];
                    string value = GetValue(valueToken);
                    switch (i)
                    {
                        case 0: if (value != null) { coso.Fields.priority = value; }; break;
                        case 2: if (value != null) { coso.Fields.state = value; }; break;
                        case 3: if (value != null) { coso.Fields.assignee = value; }; break;
                        case 6: if (value != null) { coso.Fields.dueDate = giveHour(long.Parse(value)); }; break;
                        case 7: if (value != null) { coso.Fields.startDate = giveHour(long.Parse(value)); }; break;
                        //il problema è che salva i minuti in 300 modi diversi, credo. Quindi dovrei gestire più casi e più variabili
                        case 8:
                            try
                            {
                                var oggetto = fieldsArray[i]["value"];
                                var mins = oggetto["minutes"];
                                coso.Fields.workEffort = int.Parse(mins.ToString()); break;
                            }
                            catch { coso.Fields.workEffort = 0; break; }

                        case 9: if (value != null) { coso.Fields.state = value; }; break;

                    }
                }
                Lista.Add(coso);
            }
            //visualizza();
            List<IssueSemplice> VeraLista = new List<IssueSemplice>();
            foreach (var item in Lista)
            {
                if(item.Fields.workEffort > 0) { VeraLista.Add(item); }
            }

            /*================output totale tombale======================*/
            string jsonString = JsonConvert.SerializeObject(VeraLista);




            /*============Funzioni parsing====================*/
            string GetValue(JToken valueToken)
            {
                if (valueToken == null || valueToken.Type == JTokenType.Null)
                    return null;

                if (valueToken.Type == JTokenType.Object)
                {
                    JToken nameToken = valueToken["name"];
                    return nameToken?.ToString();
                }

                return valueToken.ToString();
            }
            DateTime giveHour(long a)
            {
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddMilliseconds(a).ToLocalTime();
                return dateTime;
            }

            


            /*================API in uscita============================*/
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(a =>
            {
                a.SwaggerDoc("YoutrackÈBrutto", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "YouTrack È Brutto",
                    Description = "È proprio divertente parsare i JSON"
                });
            });
            builder.Services.AddLogging();
            builder.Services.AddHttpClient();
            builder.Services.AddCors(p => p.AddPolicy("corsApp", builder =>
            {
                builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
            }));

            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/YoutrackÈBrutto/swagger.json", "YouTrack È Brutto");
                c.RoutePrefix = string.Empty;
            });
            app.UseHttpsRedirection();
            app.UseCors("corsApp");

            app.MapGet("/Dati", () =>
            {
                return Results.Json(VeraLista);
            })
            .WithName("ProvaAPI")
            .WithOpenApi();
            app.Run(); // C'è un problema
        }
    }
}