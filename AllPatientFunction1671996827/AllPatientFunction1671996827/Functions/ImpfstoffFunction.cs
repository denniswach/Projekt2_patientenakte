using AllPatientFunction1671996827.Models;
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

namespace AllPatientFunction1671996827.Functions
{
    public class ImpfstoffFunction
    {
        [HttpGet]
        [FunctionName("GetImpfstoffe")]
        [OpenApiOperation(operationId: "GetImpfstoffe", tags: new[] { "Impfstoff" })]
        [OpenApiParameter(name: "wirkstoff", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "bezeichnung", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Impfstoff>), Description = "The OK response")]
        public ActionResult<List<Impfstoff>> GetImpfstoffe(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "impfstoffe")] HttpRequest req,
        [Sql("select * from Impfstoff",
                CommandType = CommandType.Text,
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Impfstoff> impfstoffe)
        {
            if (req.Query["wirkstoff"].Count != 0) impfstoffe = impfstoffe.Where(i => i.Wirkstoff == req.Query["wirkstoff"]).ToList();
            if (req.Query["bezeichnung"].Count != 0) impfstoffe = impfstoffe.Where(i => i.Bezeichnung == req.Query["bezeichnung"]).ToList();
            return new OkObjectResult(impfstoffe);
        }

        [FunctionName("GetImpfstoffId")]
        [OpenApiOperation(operationId: "GetImpfstoffId", tags: new[] { "Impfstoff" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Impfstoff))]
        public ActionResult<Impfstoff> GetImpfstoffId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "impfstoffe/{id}")] HttpRequest req,
            [Sql("select * from Impfstoff where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Impfstoff> impfstoffe)
        {
            return new OkObjectResult(impfstoffe.FirstOrDefault());
        }

        [FunctionName("CreateImpfstoff")]
        [OpenApiOperation(operationId: "CreateImpfstoff", tags: new[] { "Impfstoff" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ImpfstoffRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Impfstoff))]
        public async Task<ActionResult<Impfstoff>> CreateKrankekassen(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "impfstoffe")] HttpRequest req,
        [Sql("dbo.impfstoff", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Impfstoff> impfstoffe)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Impfstoff>(requestBody);

            data.Id = Guid.NewGuid();

            await impfstoffe.AddAsync(data);
            await impfstoffe.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateImpfstoff")]
        [OpenApiOperation(operationId: "UpdateImpfstoff", tags: new[] { "Impfstoff" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ImpfstoffRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Impfstoff))]
        public async Task<IActionResult> UpdateImpfstoff(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "impfstoffe/{id}")] HttpRequest req,
                [Sql("dbo.impfstoff",
                ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Impfstoff> impfstoffe,
             [Sql("select * from Impfstoff where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Impfstoff> ReadImpfstoffe)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<ImpfstoffRequest>(requestBody);
            var impfstoff = ReadImpfstoffe.FirstOrDefault();
            impfstoff.Bezeichnung = data.Bezeichnung ?? impfstoff.Bezeichnung;
            impfstoff.Wirkstoff = data.Wirkstoff ?? impfstoff.Wirkstoff;

            await impfstoffe.AddAsync(impfstoff);
            await impfstoffe.FlushAsync();

            return new OkObjectResult(impfstoff);
        }

        [FunctionName("DeleteImpfstoff")]
        [OpenApiOperation(operationId: "DeleteImpfstoff", tags: new[] { "Impfstoff" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Impfstoff))]
        public IActionResult DeleteImpfstoffe(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "impfstoffe/{id}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.impfstoff where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Impfstoff> impfstoffe)
        {
            return new OkObjectResult(impfstoffe.FirstOrDefault());
        }
    }
}
