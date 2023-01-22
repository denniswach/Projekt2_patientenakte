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
using KrankenhAssisFachberArzt1671987158.Models;

namespace KrankenhAssisFachberArzt1671987158.Functions
{
    public class AssistentFunction
    {
        private readonly string _salt;

        public AssistentFunction()
        {
            _salt = Environment.GetEnvironmentVariable("Salt");
        }

        [FunctionName("GetAssistenten")]
        [OpenApiOperation(operationId: "GetAssistenten", tags: new[] { "Assistent" })]
        [OpenApiParameter(name: "email", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "username", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "krankenhaus", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Assistent>), Description = "The OK response")]
        public ActionResult<List<Assistent>> GetAssistenten(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "assistenten")] HttpRequest req,
        [Sql("select * from Assistent",
            CommandType = CommandType.Text,
            ConnectionStringSetting = "SQLConnection")]
            IEnumerable<Assistent> assistenten)
        {
            if (req.Query["email"].Count != 0) assistenten = assistenten.Where(a => a.Email == req.Query["email"]).ToList();
            if (req.Query["username"].Count != 0) assistenten = assistenten.Where(a => a.Username == req.Query["username"]).ToList();
            if (req.Query["krankenhaus"].Count != 0) assistenten = assistenten.Where(a => a.Krankenhaus.ToString() == req.Query["krankenhaus"]).ToList();
            return new OkObjectResult(assistenten);
        }

        [FunctionName("GetAssistentenId")]
        [OpenApiOperation(operationId: "GetAssistentId", tags: new[] { "Assistent" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Assistent))]
        public ActionResult<Assistent> GetAssistentenId(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "assistenten/{id}")] HttpRequest req,
        [Sql("select * from assistent where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
            IEnumerable<Assistent> assistenten)
        {
            return new OkObjectResult(assistenten.FirstOrDefault());
        }

        [FunctionName("CreateAssistent")]
        [OpenApiOperation(operationId: "CreateAssistent", tags: new[] { "Assistent" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(AssistentRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Assistent))]
        public async Task<ActionResult<Assistent>> CreateAssistent(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "assistenten")] HttpRequest req,
        [Sql("dbo.assistent", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Assistent> assistenten)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Assistent>(requestBody);

            data.Id = Guid.NewGuid();
            data.Passhash = BCrypt.Net.BCrypt.HashPassword(data.Passhash, _salt);

            await assistenten.AddAsync(data);
            await assistenten.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateAssistent")]
        [OpenApiOperation(operationId: "UpdateAssistent", tags: new[] { "Assistent" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(AssistentRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Assistent))]
        public async Task<IActionResult> UpdateAssistent(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "assistenten/{id}")] HttpRequest req,
        [Sql("dbo.assistent",
            ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Assistent> assistenten,
        [Sql("select * from Assistent where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Assistent> readAssistentn)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Assistent>(requestBody);
            var assistent = readAssistentn.FirstOrDefault();

            assistent.Username = data.Username ?? assistent.Username;
            assistent.Passhash = (data.Passhash != null) ? data.Passhash = BCrypt.Net.BCrypt.HashPassword(data.Passhash, _salt) : assistent.Passhash;
            assistent.Name = data.Name ?? assistent.Name;
            assistent.Email = data.Email ?? assistent.Email;
            assistent.Vorname = data.Vorname ?? assistent.Vorname;
            assistent.Adresse = data.Adresse ?? assistent.Adresse;
            assistent.Geschlecht = data.Geschlecht ?? assistent.Geschlecht;
            assistent.Krankenhaus = (data.Krankenhaus != Guid.Empty) ? data.Krankenhaus : assistent.Krankenhaus;

            await assistenten.AddAsync(assistent);
            await assistenten.FlushAsync();

            return new OkObjectResult(assistent);
        }

        [FunctionName("DeleteAssistent")]
        [OpenApiOperation(operationId: "DeleteAssistent", tags: new[] { "Assistent" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Assistent))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Assistent))]
        public IActionResult DeleteAssistent(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "assistenten/{id}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.assistent where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Assistent> assistenten)
        {
            return new OkObjectResult(assistenten.FirstOrDefault());
        }

    }
}
