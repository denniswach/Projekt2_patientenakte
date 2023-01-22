using KKetteVerwVermaFunction1671915804.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KKetteVerwVermaFunction1671915804.Functions
{
    public class VerwaltungsmitarbeiterFunction
    {
        private readonly string _salt;

        public VerwaltungsmitarbeiterFunction()
        {
            _salt = Environment.GetEnvironmentVariable("Salt");
        }

        [FunctionName("GetVerwaltungsmitarbeiter")]
        [OpenApiOperation(operationId: "GetVerwaltungsmitarbeiter", tags: new[] { "Verwaltungsmitarbeiter" })]
        [OpenApiParameter(name: "email", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Verwaltungsmitarbeiter>), Description = "The OK response")]
        public ActionResult<List<Verwaltungsmitarbeiter>> GetVerwaltungsmitarbeiter(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "verwaltungsmitarbeiter")] HttpRequest req,
        [Sql("select * from Verwaltungsmitarbeiter",
            CommandType = CommandType.Text,
            ConnectionStringSetting = "SQLConnection")]
            IEnumerable<Verwaltungsmitarbeiter> verwaltungsmitarbeitern)
        {
            if (req.Query["email"].Count != 0) verwaltungsmitarbeitern = verwaltungsmitarbeitern.Where(v => v.Email == req.Query["email"]).ToList(); 
            return new OkObjectResult(verwaltungsmitarbeitern);
        }

        [FunctionName("GetVerwaltungsmitarbeiterId")]
        [OpenApiOperation(operationId: "GetVerwaltungsmitarbeiterId", tags: new[] { "Verwaltungsmitarbeiter" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Verwaltungsmitarbeiter))]
        public ActionResult<Verwaltungsmitarbeiter> GetVerwaltungsmitarbeiterId(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Verwaltungsmitarbeiter/{id}")] HttpRequest req,
        [Sql("select * from Verwaltungsmitarbeiter where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
            IEnumerable<Verwaltungsmitarbeiter> verwaltungsmitarbeitern)
        {
            return new OkObjectResult(verwaltungsmitarbeitern.FirstOrDefault());
        }

        [FunctionName("CreateVerwaltungsmitarbeiter")]
        [OpenApiOperation(operationId: "CreateVerwaltungsmitarbeiter", tags: new[] { "Verwaltungsmitarbeiter" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(VerwaltungsmitarbeiterRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Verwaltungsmitarbeiter))]
        public async Task<ActionResult<Verwaltungsmitarbeiter>> CreateKrankenhausketten(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "verwaltungsmitarbeiter")] HttpRequest req,
        [Sql("dbo.verwaltungsmitarbeiter", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Verwaltungsmitarbeiter> verwaltungsmitarbeitern)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Verwaltungsmitarbeiter>(requestBody);

            data.Id = Guid.NewGuid();
            data.Passhash = BCrypt.Net.BCrypt.HashPassword(data.Passhash, _salt);

            await verwaltungsmitarbeitern.AddAsync(data);
            await verwaltungsmitarbeitern.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateVerwaltungsmitarbeiter")]
        [OpenApiOperation(operationId: "UpdateVerwaltungsmitarbeiter", tags: new[] { "Verwaltungsmitarbeiter" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(VerwaltungsmitarbeiterRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Verwaltungsmitarbeiter))]
        public async Task<IActionResult> UpdateKrankenhausketten(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "verwaltungsmitarbeiter/{id}")] HttpRequest req,
        [Sql("dbo.verwaltungsmitarbeiter",
            ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Verwaltungsmitarbeiter> verwaltungsmitarbeitern,
        [Sql("select * from Verwaltungsmitarbeiter where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Verwaltungsmitarbeiter> readVerwaltungsmitarbeitern)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Verwaltungsmitarbeiter>(requestBody);
            var verwaltungsmitarbeiter = readVerwaltungsmitarbeitern.FirstOrDefault();

            verwaltungsmitarbeiter.Email = data.Email ?? verwaltungsmitarbeiter.Email;
            verwaltungsmitarbeiter.Adresse = data.Adresse ?? verwaltungsmitarbeiter.Adresse;
            verwaltungsmitarbeiter.Name = data.Name?? verwaltungsmitarbeiter.Name;
            verwaltungsmitarbeiter.Vorname = data.Vorname ?? verwaltungsmitarbeiter.Vorname;
            verwaltungsmitarbeiter.Verwaltung = (data.Verwaltung != Guid.Empty) ? data.Verwaltung : verwaltungsmitarbeiter.Verwaltung;
            verwaltungsmitarbeiter.Email = data.Email ?? verwaltungsmitarbeiter.Email;
            verwaltungsmitarbeiter.Passhash = (data.Passhash != null) ? data.Passhash = BCrypt.Net.BCrypt.HashPassword(data.Passhash, _salt) : verwaltungsmitarbeiter.Passhash; 

            await verwaltungsmitarbeitern.AddAsync(verwaltungsmitarbeiter);
            await verwaltungsmitarbeitern.FlushAsync();

            return new OkObjectResult(verwaltungsmitarbeiter);
        }

        [FunctionName("DeleteVerwaltungsmitarbeiter")]
        [OpenApiOperation(operationId: "DeleteVerwaltungsmitarbeiter", tags: new[] { "Verwaltungsmitarbeiter" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Verwaltungsmitarbeiter))]
        public IActionResult DeleteKrankenhausketten(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "verwaltungsmitarbeiter/{id}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.verwaltungsmitarbeiter where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Verwaltungsmitarbeiter> verwaltungsmitarbeitern)
        {
            return new OkObjectResult(verwaltungsmitarbeitern.FirstOrDefault());
        }



    }
}
