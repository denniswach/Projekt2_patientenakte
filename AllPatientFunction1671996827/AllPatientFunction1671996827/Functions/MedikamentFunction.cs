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
    public class MedikamentFunction
    {
        [HttpGet]
        [FunctionName("GetMedikamente")]
        [OpenApiOperation(operationId: "GetMedikamente", tags: new[] { "Medikament" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "wirkstoff", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Medikament>), Description = "The OK response")]
        public ActionResult<List<Medikament>> GetMedikamente(
       [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "medikamente")] HttpRequest req,
       [Sql("select * from Medikament",
                CommandType = CommandType.Text,
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Medikament> medikamente)
        {
            if (req.Query["name"].Count != 0) medikamente = medikamente.Where(m => m.Name == req.Query["name"]).ToList();
            if (req.Query["wirkstoff"].Count != 0) medikamente = medikamente.Where(m => m.Wirkstoff == req.Query["wirkstoff"]).ToList();
            return new OkObjectResult(medikamente);
        }

        [FunctionName("GetMedikamentId")]
        [OpenApiOperation(operationId: "GetMedikamentId", tags: new[] { "Medikament" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Medikament))]
        public ActionResult<Medikament> GetMedikamentId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "medikamente/{id}")] HttpRequest req,
            [Sql("select * from Medikament where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Medikament> medikamente)
        {
            return new OkObjectResult(medikamente.FirstOrDefault());
        }

        [FunctionName("CreateMedikament")]
        [OpenApiOperation(operationId: "CreateMedikament", tags: new[] { "Medikament" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(MedikamentRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Medikament))]
        public async Task<ActionResult<Medikament>> CreateKrankekassen(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "medikamente")] HttpRequest req,
        [Sql("dbo.medikament", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Medikament> medikamente)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Medikament>(requestBody);

            data.Id = Guid.NewGuid();

            await medikamente.AddAsync(data);
            await medikamente.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateMedikament")]
        [OpenApiOperation(operationId: "UpdateMedikament", tags: new[] { "Medikament" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(MedikamentRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Medikament))]
        public async Task<IActionResult> UpdateMedikament(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "medikamente/{id}")] HttpRequest req,
                [Sql("dbo.medikament",
                ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Medikament> medikamente,
             [Sql("select * from Medikament where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Medikament> ReadMedikamente)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<MedikamentRequest>(requestBody);
            var medikament = ReadMedikamente.FirstOrDefault();
            medikament.Name = data.Name ?? medikament.Name;
            medikament.Wirkstoff = data.Wirkstoff ?? medikament.Wirkstoff;

            await medikamente.AddAsync(medikament);
            await medikamente.FlushAsync();

            return new OkObjectResult(medikament);
        }

        [FunctionName("DeleteMedikament")]
        [OpenApiOperation(operationId: "DeleteMedikament", tags: new[] { "Medikament" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Medikament))]
        public IActionResult DeleteMedikamente(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "medikamente/{id}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.medikament where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Medikament> medikamente)
        {
            return new OkObjectResult(medikamente.FirstOrDefault());
        }
    }
}
