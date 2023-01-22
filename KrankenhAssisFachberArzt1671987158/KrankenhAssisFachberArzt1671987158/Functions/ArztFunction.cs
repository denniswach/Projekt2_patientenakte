using KrankenhAssisFachberArzt1671987158.Models;
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

namespace KrankenhAssisFachberArzt1671987158.Functions
{
    public class ArztFunction
    {

        private readonly string _salt;

        public ArztFunction()
        {
            _salt = Environment.GetEnvironmentVariable("Salt");
        }

        [FunctionName("GetArzte")]
        [OpenApiOperation(operationId: "GetArzte", tags: new[] { "Arzt" })]
        [OpenApiParameter(name: "email", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "username", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "krankenhaus", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "fachbereich", In = ParameterLocation.Query, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Arzt>), Description = "The OK response")]
        public ActionResult<List<Arzt>> GetArzt(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "arzte")] HttpRequest req,
        [Sql("select * from Arzt",
            CommandType = CommandType.Text,
            ConnectionStringSetting = "SQLConnection")]
            IEnumerable<Arzt> arzte)
        {
            if (req.Query["email"].Count != 0) arzte = arzte.Where(a => a.Email == req.Query["email"]).ToList();
            if (req.Query["username"].Count != 0) arzte = arzte.Where(a => a.Username == req.Query["username"]).ToList();
            if (req.Query["krankenhaus"].Count != 0) arzte = arzte.Where(a => a.Krankenhaus.ToString() == req.Query["krankenhaus"]).ToList();
            if (req.Query["fachbereich"].Count != 0) arzte = arzte.Where(a => a.Fachbereich.ToString() == req.Query["fachbereich"]).ToList();
            return new OkObjectResult(arzte);
        }

        [FunctionName("GetArztId")]
        [OpenApiOperation(operationId: "GetArztId", tags: new[] { "Arzt" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Arzt))]
        public ActionResult<Arzt> GetArzteId(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "arzte/{id}")] HttpRequest req,
        [Sql("select * from arzt where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
            IEnumerable<Arzt> arzte)
        {
            return new OkObjectResult(arzte.FirstOrDefault());
        }

        [FunctionName("CreateArzt")]
        [OpenApiOperation(operationId: "CreateArzt", tags: new[] { "Arzt" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ArztRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Arzt))]
        public async Task<ActionResult<Arzt>> CreateKrankenhausketten(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "arzte")] HttpRequest req,
        [Sql("dbo.arzt", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Arzt> arzte)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Arzt>(requestBody);

            data.Id = Guid.NewGuid();
            data.Passhash = BCrypt.Net.BCrypt.HashPassword(data.Passhash, _salt);

            await arzte.AddAsync(data);
            await arzte.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateArzt")]
        [OpenApiOperation(operationId: "UpdateArzt", tags: new[] { "Arzt" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ArztRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Arzt))]
        public async Task<IActionResult> UpdateArzt(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "arzte/{id}")] HttpRequest req,
        [Sql("dbo.arzt",
            ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Arzt> arzte,
        [Sql("select * from Arzt where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Arzt> readArztn)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Arzt>(requestBody);
            var arzt = readArztn.FirstOrDefault();

            arzt.Username = data.Username ?? arzt.Username;
            arzt.Passhash = (data.Passhash != null) ? data.Passhash = BCrypt.Net.BCrypt.HashPassword(data.Passhash, _salt) : arzt.Passhash;
            arzt.Name = data.Name ?? arzt.Name;
            arzt.Email = data.Email ?? arzt.Email;
            arzt.Vorname = data.Vorname ?? arzt.Vorname;
            arzt.Adresse = data.Adresse ?? arzt.Adresse;
            arzt.Geschlecht = data.Geschlecht ?? arzt.Geschlecht;
            arzt.Krankenhaus = (data.Krankenhaus != Guid.Empty) ? data.Krankenhaus : arzt.Krankenhaus;
            arzt.Fachbereich = (data.Fachbereich != Guid.Empty) ? data.Fachbereich : arzt.Fachbereich;
            arzt.Jobtitel = data.Jobtitel ?? arzt.Jobtitel;
            arzt.Spezialisierung = data.Spezialisierung ?? arzt.Spezialisierung;

            await arzte.AddAsync(arzt);
            await arzte.FlushAsync();

            return new OkObjectResult(arzt);
        }

        [FunctionName("DeleteArzt")]
        [OpenApiOperation(operationId: "DeleteArzt", tags: new[] { "Arzt" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Arzt))]
        public IActionResult DeleteKrankenhausketten(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "arzte/{id}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.arzt where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Arzt> arzte)
        {
            return new OkObjectResult(arzte.FirstOrDefault());
        }
    }
}
