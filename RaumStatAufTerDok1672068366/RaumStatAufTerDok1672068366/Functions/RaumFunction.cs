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
using RaumStatAufTerDok1672068366.Models;

namespace RaumStatAufTerDok1672068366.Functions
{
    public class RaumFunction
    {
        [HttpGet]
        [FunctionName("GetRaume")]
        [OpenApiOperation(operationId: "GetRaume", tags: new[] { "Raum" })]
        [OpenApiParameter(name: "krankenhausid", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "raumnummer", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Raum>), Description = "The OK response")]
        public ActionResult<List<Raum>> GetRaume(
             [HttpTrigger(AuthorizationLevel.Function, "get", Route = "raume")] HttpRequest req,
             [Sql("select * from Raum",
                CommandType = CommandType.Text,
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Raum> raume)
        {
            if (req.Query["krankenhausid"].Count != 0) raume = raume.Where(r => r.Krankenhaus.ToString() == req.Query["krankenhausid"]).ToList();
            if (req.Query["raumnummer"].Count != 0) raume = raume.Where(r => r.Raumnummer.ToString() == req.Query["raumnummer"]).ToList();
            return new OkObjectResult(raume);
        }

        [FunctionName("GetRaumId")]
        [OpenApiOperation(operationId: "GetRaumId", tags: new[] { "Raum" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Raum))]
        public ActionResult<Raum> GetRaumId(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "raume/{id}")] HttpRequest req,
            [Sql("select * from Raum where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Raum> raume)
        {
            return new OkObjectResult(raume.FirstOrDefault());
        }

        [FunctionName("CreateRaum")]
        [OpenApiOperation(operationId: "CreateRaum", tags: new[] { "Raum" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(RaumRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Raum))]
        public async Task<ActionResult<Raum>> CreateKrankekassen(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "raume")] HttpRequest req,
        [Sql("dbo.raum", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Raum> raume)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Raum>(requestBody);

            data.Id = Guid.NewGuid();

            await raume.AddAsync(data);
            await raume.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateRaum")]
        [OpenApiOperation(operationId: "UpdateRaum", tags: new[] { "Raum" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(RaumRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Raum))]
        public async Task<IActionResult> UpdateRaum(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "raume/{id}")] HttpRequest req,
                [Sql("dbo.raum",
                ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Raum> raume,
             [Sql("select * from Raum where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Raum> ReadRaume)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<RaumRequest>(requestBody);
            var raum = ReadRaume.FirstOrDefault();
            raum.Krankenhaus = data.Krankenhaus ?? raum.Krankenhaus;
            raum.Raumnummer = data.Raumnummer ?? raum.Raumnummer;

            await raume.AddAsync(raum);
            await raume.FlushAsync();

            return new OkObjectResult(raum);
        }

        [FunctionName("DeleteRaum")]
        [OpenApiOperation(operationId: "DeleteRaum", tags: new[] { "Raum" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Raum))]
        public IActionResult DeleteRaume(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "raume/{id}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.raum where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Raum> raume)
        {
            return new OkObjectResult(raume.FirstOrDefault());
        }
    }
}
