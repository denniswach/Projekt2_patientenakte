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
    public class FachbereichFunction
    {
        [FunctionName("GetFachbereich")]
        [OpenApiOperation(operationId: "GetFachbereich", tags: new[] { "Fachbereich" })]
        [OpenApiParameter(name: "bezeichnung", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "krankenhaus", In = ParameterLocation.Query, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Fachbereich>), Description = "The OK response")]
        public ActionResult<List<Fachbereich>> GetFachbereiche(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "fachbereiche")] HttpRequest req,
        [Sql("select * from Fachbereich",
            CommandType = CommandType.Text,
            ConnectionStringSetting = "SQLConnection")]
            IEnumerable<Fachbereich> fachbereiche)
        {
            if (req.Query["bezeichnung"].Count != 0) fachbereiche = fachbereiche.Where(f => f.Bezeichnung == req.Query["bezeichnung"]).ToList();
            if (req.Query["krankenhaus"].Count != 0) fachbereiche = fachbereiche.Where(k => k.Krankenhaus.ToString() == req.Query["krankenhaus"]).ToList();
            return new OkObjectResult(fachbereiche);
        }

        [FunctionName("GetFachbereicheId")]
        [OpenApiOperation(operationId: "GetFachbereichnId", tags: new[] { "Fachbereich" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Fachbereich))]
        public ActionResult<Fachbereich> GetFachbereichnId(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "fachbereiche/{id}")] HttpRequest req,
            [Sql("select * from Fachbereich where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Fachbereich> fachbereiche)
        {
            return new OkObjectResult(fachbereiche.FirstOrDefault());
        }

        [FunctionName("CreateFachbereich")]
        [OpenApiOperation(operationId: "CreateFachbereich", tags: new[] { "Fachbereich" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(FachbereichRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Fachbereich))]
        public async Task<ActionResult<Fachbereich>> CreateFachbereiche(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fachbereiche")] HttpRequest req,
        [Sql("dbo.fachbereich", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Fachbereich> krankenhausketten)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Fachbereich>(requestBody);

            data.Id = Guid.NewGuid();

            await krankenhausketten.AddAsync(data);
            await krankenhausketten.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateFachbereich")]
        [OpenApiOperation(operationId: "UpdateFachbereich", tags: new[] { "Fachbereich" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(FachbereichRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Fachbereich))]
        public async Task<IActionResult> UpdateFachbereiche(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fachbereiche/{id}")] HttpRequest req,
        [Sql("dbo.fachbereich",
            ConnectionStringSetting = "SQLConnection")]
        IAsyncCollector<Fachbereich> fachbereiche,
        [Sql("select * from Fachbereich where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Fachbereich> ReadFachbereiche)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Fachbereich>(requestBody);
            var fachbereich = ReadFachbereiche.FirstOrDefault();
            fachbereich.Bezeichnung = data.Bezeichnung ?? fachbereich.Bezeichnung;
            fachbereich.Krankenhaus = (data.Krankenhaus != Guid.Empty) ? data.Krankenhaus : fachbereich.Krankenhaus;

            await fachbereiche.AddAsync(fachbereich);
            await fachbereiche.FlushAsync();

            return new OkObjectResult(fachbereich);
        }

        [FunctionName("DeleteFachbereich")]
        [OpenApiOperation(operationId: "DeleteFachbereich", tags: new[] { "Fachbereich" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Fachbereich))]
        public IActionResult DeleteFachbereichn(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "fachbereich/{id}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.fachbereich where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Fachbereich> fachbereiche)
        {
            return new OkObjectResult(fachbereiche.FirstOrDefault());
        }
    }
}
