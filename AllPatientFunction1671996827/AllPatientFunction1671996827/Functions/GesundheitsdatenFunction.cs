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
    public class GesundheitsdatenFunction
    {
        [HttpGet]
        [FunctionName("GetGesundheitsdatenn")]
        [OpenApiOperation(operationId: "GetGesundheitsdatenn", tags: new[] { "Gesundheitsdaten" })]
        [OpenApiParameter(name: "patient", In = ParameterLocation.Query, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Gesundheitsdaten>), Description = "The OK response")]
        public ActionResult<List<Gesundheitsdaten>> GetGesundheitsdatenn(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gesundheitsdaten")] HttpRequest req,
            [Sql("select * from gesundheitsdaten",
            CommandType = CommandType.Text,
            ConnectionStringSetting = "SQLConnection")]
            IEnumerable<Gesundheitsdaten> gesundheitsdaten)
        {
            if (req.Query["patient"].Count != 0) gesundheitsdaten = gesundheitsdaten.Where(g => g.Patient.ToString() == req.Query["patient"]).ToList();
            return new OkObjectResult(gesundheitsdaten);
        }

        [FunctionName("GetGesundheitsdatenId")]
        [OpenApiOperation(operationId: "GetGesundheitsdatenId", tags: new[] { "Gesundheitsdaten" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Gesundheitsdaten))]
        public ActionResult<Gesundheitsdaten> GetGesundheitsdatenId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gesundheitsdaten/{id}")] HttpRequest req,
            [Sql("select * from gesundheitsdaten where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Gesundheitsdaten> gesundheitsdaten)
        {
            return new OkObjectResult(gesundheitsdaten.FirstOrDefault());
        }

        [FunctionName("CreateGesundheitsdaten")]
        [OpenApiOperation(operationId: "CreateGesundheitsdaten", tags: new[] { "Gesundheitsdaten" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(GesundheitsdatenRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Gesundheitsdaten))]
        public async Task<ActionResult<Gesundheitsdaten>> CreateKrankekassen(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gesundheitsdaten")] HttpRequest req,
        [Sql("dbo.gesundheitsdaten", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Gesundheitsdaten> gesundheitsdaten)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Gesundheitsdaten>(requestBody);

            data.Id = Guid.NewGuid();

            await gesundheitsdaten.AddAsync(data);
            await gesundheitsdaten.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateGesundheitsdaten")]
        [OpenApiOperation(operationId: "UpdateGesundheitsdaten", tags: new[] { "Gesundheitsdaten" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(GesundheitsdatenRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Gesundheitsdaten))]
        public async Task<IActionResult> UpdateGesundheitsdaten(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gesundheitsdaten/{id}")] HttpRequest req,
                [Sql("dbo.gesundheitsdaten",
                ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Gesundheitsdaten> gesundheitsdaten,
             [Sql("select * from gesundheitsdaten where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Gesundheitsdaten> ReadGesundheitsdaten)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<GesundheitsdatenRequest>(requestBody);
            var gesundheitsdatum = ReadGesundheitsdaten.FirstOrDefault();
            gesundheitsdatum.Vorerkrankungen = data.Vorerkrankungen ?? gesundheitsdatum.Vorerkrankungen;
            gesundheitsdatum.Allergien = data.Allergien ?? gesundheitsdatum.Allergien;
            gesundheitsdatum.GewichtKG = data.GewichtKG ?? gesundheitsdatum.GewichtKG;
            gesundheitsdatum.GroeßeCM = data.GroeßeCM ?? gesundheitsdatum.GroeßeCM;
            gesundheitsdatum.Ops = data.Ops ?? gesundheitsdatum.Ops;
            gesundheitsdatum.Patientenverfuegung = data.Patientenverfuegung ?? gesundheitsdatum.Patientenverfuegung;
            gesundheitsdatum.Vorsorgevollmacht = data.Vorsorgevollmacht ?? gesundheitsdatum.Vorsorgevollmacht;

            await gesundheitsdaten.AddAsync(gesundheitsdatum);
            await gesundheitsdaten.FlushAsync();

            return new OkObjectResult(gesundheitsdatum);
        }

        [FunctionName("DeleteGesundheitsdaten")]
        [OpenApiOperation(operationId: "DeleteGesundheitsdaten", tags: new[] { "Gesundheitsdaten" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Gesundheitsdaten))]
        public IActionResult DeleteGesundheitsdatenn(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "gesundheitsdaten/{id}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.gesundheitsdaten where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Gesundheitsdaten> gesundheitsdaten)
        {
            return new OkObjectResult(gesundheitsdaten.FirstOrDefault());
        }
    }
}
