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
using AllPatientFunction1671996827.Models;

namespace AllPatientFunction1671996827.Functions
{
    public class LoeschanfrageFunciton
    {

        [FunctionName("GetLoeschanfragen")]
        [OpenApiOperation(operationId: "GetLoeschanfragen", tags: new[] { "Loeschanfrage" })]
        [OpenApiParameter(name: "patient", In = ParameterLocation.Query, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Loeschanfrage>), Description = "The OK response")]
        public ActionResult<List<Loeschanfrage>> GetLoeschanfragen(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "loeschanfragen")] HttpRequest req,
          [Sql("select * from Loeschanfrage",
                CommandType = CommandType.Text,
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Loeschanfrage> loeschanfragen)
        {
            if (req.Query["patient"].Count != 0) loeschanfragen = loeschanfragen.Where(l => l.Patient.ToString() == req.Query["patient"]).ToList();
            return new OkObjectResult(loeschanfragen);
        }

        [FunctionName("GetLoeschanfrageId")]
        [OpenApiOperation(operationId: "GetLoeschanfrageId", tags: new[] { "Loeschanfrage" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Loeschanfrage))]
        public ActionResult<Loeschanfrage> GetLoeschanfrageId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "loeschanfragen/{id}")] HttpRequest req,
            [Sql("select * from Loeschanfrage where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Loeschanfrage> loeschanfragen)
        {
            return new OkObjectResult(loeschanfragen.FirstOrDefault());
        }

        [FunctionName("CreateLoeschanfrage")]
        [OpenApiOperation(operationId: "CreateLoeschanfrage", tags: new[] { "Loeschanfrage" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(LoeschanfrageRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Loeschanfrage))]
        public async Task<ActionResult<Loeschanfrage>> CreateLoeschanfragen(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "loeschanfragen")] HttpRequest req,
        [Sql("dbo.loeschanfrage", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Loeschanfrage> loeschanfragen)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Loeschanfrage>(requestBody);

            data.Id = Guid.NewGuid();
            data.Datum = DateTime.Now;

            await loeschanfragen.AddAsync(data);
            await loeschanfragen.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateLoeschanfrage")]
        [OpenApiOperation(operationId: "UpdateLoeschanfrage", tags: new[] { "Loeschanfrage" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(LoeschanfrageRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Loeschanfrage))]
        public async Task<IActionResult> UpdateLoeschanfrage(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "loeschanfragen/{id}")] HttpRequest req,
                [Sql("dbo.loeschanfrage",
                ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Loeschanfrage> loeschanfragen,
             [Sql("select * from Loeschanfrage where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Loeschanfrage> ReadLoeschanfragen)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Loeschanfrage>(requestBody);
            var loeschanfrage = ReadLoeschanfragen.FirstOrDefault();

            loeschanfrage.Patient = (data.Patient != Guid.Empty) ? data.Patient : loeschanfrage.Patient;
            loeschanfrage.Loeschanfragetext = data.Loeschanfragetext ?? loeschanfrage.Loeschanfragetext;

            await loeschanfragen.AddAsync(loeschanfrage);
            await loeschanfragen.FlushAsync();

            return new OkObjectResult(loeschanfrage);
        }

        [FunctionName("DeleteLoeschanfrage")]
        [OpenApiOperation(operationId: "DeleteLoeschanfrage", tags: new[] { "Loeschanfrage" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Loeschanfrage))]
        public IActionResult DeleteLoeschanfrage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "loeschanfragen/{id}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.loeschanfrage where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Loeschanfrage> loeschanfragen)
        {
            return new OkObjectResult(loeschanfragen.FirstOrDefault());
        }


    }
}
