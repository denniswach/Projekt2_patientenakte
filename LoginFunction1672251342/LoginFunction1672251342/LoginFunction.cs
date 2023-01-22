using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Jose;
using System.Collections.Generic;
using System;
using System.Text;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LoginFunction1672251342
{
    public class LoginFunction
    {
        private readonly string _salt;
        private readonly string _tokenKey;

        public LoginFunction()
        {
            _salt = Environment.GetEnvironmentVariable("Salt");
            _tokenKey = Environment.GetEnvironmentVariable("TokenKey");
        }
        [FunctionName("Login")]
        [OpenApiOperation(operationId: "Login", tags: new[] { "name" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(RequestBody), Description= "Rollen: patient, arzt, verwaltungsmitarbeiter, assistent")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login")] HttpRequest req,
              [Sql(commandText: "select username, email, name, passhash, vorname, id from ",
            CommandType = System.Data.CommandType.Text,
            ConnectionStringSetting = "SQLConnection")]
        SqlCommand command)
        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<RequestBody>(requestBody);
            data.Rolle.ToLower();
            switch (data.Rolle)
            {
                case "patient":
                    command.CommandText += $"patient where username = '{data.Benutzer}'";
                    break;
                case "arzt":
                    command.CommandText += $"arzt where username = '{data.Benutzer}'";
                    break;
                case "verwaltungsmitarbeiter":
                    command.CommandText += $"verwaltungsmitarbeiter where username = '{data.Benutzer}'";
                    break;
                case "assistent":
                    command.CommandText += $"assistent where username = '{data.Benutzer}'";
                    break;
            }
            var benutzer = new Benutzer();
            using (SqlConnection conn = command.Connection)
            {
                conn.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        benutzer.Id = reader.GetGuid("id");
                        benutzer.Username = reader.GetString("username");
                        benutzer.Passhash = reader.GetString("passhash");
                        benutzer.Email = reader.GetString("email");
                        benutzer.Rolle = data.Rolle;
                    }
                }
            }
            if (benutzer.Id == Guid.Empty ) return new BadRequestObjectResult("User not found");
            if ( benutzer.Passhash != BCrypt.Net.BCrypt.HashPassword(data.Passwort, _salt)) return new BadRequestObjectResult("Wrong Password");


            var payload = new Dictionary<string, object>()
            {
                {"username", benutzer.Username },
                {"email", benutzer.Email },
                {"id", benutzer.Id},
                {"rolle", benutzer.Rolle},
                {"exp", DateTimeOffset.Now.AddMonths(1).ToUnixTimeSeconds()}
            };
            var key = Encoding.ASCII.GetBytes(_tokenKey);
            var token = Jose.JWT.Encode(payload, key, JwsAlgorithm.HS256);

            return new OkObjectResult(token);
        }
    }
}

