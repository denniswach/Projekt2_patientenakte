using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
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
using Microsoft.Extensions.Logging;
using AllPatientFunction1671996827.Models;

namespace AllPatientFunction1671996827.Functions
{
    public class KrankenkasseFunction
    {
        [HttpGet]
        [FunctionName("GetKrankenkassen")]
        [OpenApiOperation(operationId: "GetKrankenkassen", tags: new[] { "Krankenkasse" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "standort", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Krankenkasse>), Description = "The OK response")]
        public ActionResult<List<Krankenkasse>> GetKrankenkassen(
         [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "krankenkassen")] HttpRequest req,
         [Sql("select * from Krankenkasse",
                CommandType = CommandType.Text,
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Krankenkasse> krankenkassen)
        {
            if (req.Query["standort"].Count != 0) krankenkassen = krankenkassen.Where(k => k.Standort == req.Query["standort"]).ToList();
            if (req.Query["name"].Count != 0) krankenkassen = krankenkassen.Where(k => k.Name == req.Query["name"]).ToList();
            return new OkObjectResult(krankenkassen);
        }

        [FunctionName("GetKrankenkasseId")]
        [OpenApiOperation(operationId: "GetKrankenkasseId", tags: new[] { "Krankenkasse" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Krankenkasse))]
        public ActionResult<Krankenkasse> GetKrankenkasseId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "krankenkassen/{id}")] HttpRequest req,
            [Sql("select * from Krankenkasse where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Krankenkasse> krankenkassen)
        {
            return new OkObjectResult(krankenkassen.FirstOrDefault());
        }

        [FunctionName("CreateKrankenkasse")]
        [OpenApiOperation(operationId: "CreateKrankenkasse", tags: new[] { "Krankenkasse" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(KrankenkasseRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Krankenkasse))]
        public async Task<ActionResult<Krankenkasse>> CreateKrankekassen(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "krankenkassen")] HttpRequest req,
        [Sql("dbo.krankenkasse", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Krankenkasse> krankenkassen)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Krankenkasse>(requestBody);

            data.Id = Guid.NewGuid();

            await krankenkassen.AddAsync(data);
            await krankenkassen.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateKrankenkasse")]
        [OpenApiOperation(operationId: "UpdateKrankenkasse", tags: new[] { "Krankenkasse" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(KrankenkasseRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Krankenkasse))]
        public async Task<IActionResult> UpdateKrankenkasse(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "krankenkassen/{id}")] HttpRequest req,
                [Sql("dbo.krankenkasse",
                ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Krankenkasse> krankenkassen,
             [Sql("select * from Krankenkasse where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Krankenkasse> ReadKrankenkassen)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<KrankenkasseRequest>(requestBody);
            var krankenkasse = ReadKrankenkassen.FirstOrDefault();
            krankenkasse.Name = data.Name ?? krankenkasse.Name;
            krankenkasse.Standort = data.Standort ?? krankenkasse.Standort;
            krankenkasse.IsGesetzlich = data.IsGesetzlich ?? krankenkasse.IsGesetzlich;

            await krankenkassen.AddAsync(krankenkasse);
            await krankenkassen.FlushAsync();

            return new OkObjectResult(krankenkasse);
        }

        [FunctionName("DeleteKrankenkasse")]
        [OpenApiOperation(operationId: "DeleteKrankenkasse", tags: new[] { "Krankenkasse" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Krankenkasse))]
        public IActionResult DeleteKrankenkassen(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "krankenkassen/{id}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.krankenkasse where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Krankenkasse> krankenkassen)
        {
            return new OkObjectResult(krankenkassen.FirstOrDefault());
        }

    }
}
