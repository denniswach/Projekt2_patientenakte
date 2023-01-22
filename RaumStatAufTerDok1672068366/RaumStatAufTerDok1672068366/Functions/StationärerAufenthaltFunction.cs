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
    public class StationärerAufenthaltFunction
    {
        [HttpGet]
        [FunctionName("GetStationaererAufenthalte")]
        [OpenApiOperation(operationId: "GetStationaererAufenthalte", tags: new[] { "StationaererAufenthalt" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Query, Type = typeof(Guid))]
        [OpenApiParameter(name: "raumid", In = ParameterLocation.Query, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<StationaererAufenthalt>), Description = "The OK response")]
        public ActionResult<List<StationaererAufenthaltResponse>> GetStationaererAufenthalte(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "stationaererAufenthalte")] HttpRequest req,
            [Sql("select s.PatientId, p.name as pname, p.vorname as pvorname, s.raumid, r.raumnummer, s.Aufnahmezeitpunkt, s.Entlassungszeitraum, s.Hinweise " +
            "from StationärerAufenthalt as s " +
            "join raum as r on s.raumid = r.id " +
            "join patient as p on p.id = s.patientid ",
                CommandType = CommandType.Text,
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<StationaererAufenthaltResponse> stationaererAufenthalte)
        {
            if (req.Query["patientid"].Count != 0) stationaererAufenthalte = stationaererAufenthalte.Where(s => s.PatientId.ToString() == req.Query["patientid"]).ToList();
            if (req.Query["raumid"].Count != 0) stationaererAufenthalte = stationaererAufenthalte.Where(s => s.RaumId.ToString() == req.Query["raumid"]).ToList();
            return new OkObjectResult(stationaererAufenthalte);
        }

        [FunctionName("GetStationaererAufenthaltId")]
        [OpenApiOperation(operationId: "GetStationaererAufenthaltId", tags: new[] { "StationaererAufenthalt" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiParameter(name: "raumid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(StationaererAufenthaltRequest))]
        public ActionResult<StationaererAufenthalt> GetStationaererAufenthaltId(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "stationaererAufenthalte/{patientid}/{raumid}")] HttpRequest req,
            [Sql("select s.PatientId, p.name as pname, p.vorname as pvorname, s.raumid, r.raumnummer, s.Aufnahmezeitpunkt, s.Entlassungszeitraum, s.Hinweise " +
            "from StationärerAufenthalt as s " +
            "join raum as r on s.raumid = r.id " +
            "join patient as p on p.id = s.patientid " +
            "where s.patientid = @patientid and s.raumid = @raumid",
                CommandType = CommandType.Text,
                Parameters = "@patientid={patientid},@raumid={raumid}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<StationaererAufenthaltResponse> stationaererAufenthalte)
        {
            return new OkObjectResult(stationaererAufenthalte.FirstOrDefault());
        }

        [FunctionName("CreateStationaererAufenthalt")]
        [OpenApiOperation(operationId: "CreateStationaererAufenthalt", tags: new[] { "StationaererAufenthalt" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(StationaererAufenthaltRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(StationaererAufenthalt))]
        public async Task<ActionResult<StationaererAufenthalt>> CreateStationaererAufenthalt(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "stationaererAufenthalte")] HttpRequest req,
        [Sql("dbo.stationärerAufenthalt", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<StationaererAufenthalt> stationaererAufenthalte)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<StationaererAufenthalt>(requestBody);

            await stationaererAufenthalte.AddAsync(data);
            await stationaererAufenthalte.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateStationaererAufenthalt")]
        [OpenApiOperation(operationId: "UpdateStationaererAufenthalt", tags: new[] { "StationaererAufenthalt" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiParameter(name: "raumid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(StationaererAufenthaltRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(StationaererAufenthalt))]
        public async Task<IActionResult> UpdateStationaererAufenthalt(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "stationaererAufenthalte/{patientid}/{raumid}")] HttpRequest req,
                [Sql("dbo.stationärerAufenthalt",
                ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<StationaererAufenthalt> stationaererAufenthalte,
             [Sql("select * from StationärerAufenthalt where patientid = @patientid and raumid = @raumid",
                CommandType = CommandType.Text,
                Parameters = "@patientid={patientid},@raumid={raumid}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<StationaererAufenthalt> ReadStationaererAufenthalte)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<StationaererAufenthaltRequest>(requestBody);
            var stationärerAufenthalt = ReadStationaererAufenthalte.FirstOrDefault();
            stationärerAufenthalt.Aufnahmezeitpunkt = data.Aufnahmezeitpunkt ?? stationärerAufenthalt.Aufnahmezeitpunkt;
            stationärerAufenthalt.Entlassungszeitraum = data.Entlassungszeitraum ?? stationärerAufenthalt.Entlassungszeitraum;
            stationärerAufenthalt.Hinweise = data.Hinweise ?? stationärerAufenthalt.Hinweise;

            await stationaererAufenthalte.AddAsync(stationärerAufenthalt);
            await stationaererAufenthalte.FlushAsync();

            return new OkObjectResult(stationärerAufenthalt);
        }

        [FunctionName("DeleteStationaererAufenthalt")]
        [OpenApiOperation(operationId: "DeleteStationaererAufenthalt", tags: new[] { "StationaererAufenthalt" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiParameter(name: "raumid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(StationaererAufenthalt))]
        public IActionResult DeleteStationaererAufenthalte(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "stationaererAufenthalte/{patientid}/{raumid}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.stationärerAufenthalt where patientid = @patientid and raumid = @raumid",
            CommandType = CommandType.Text,
            Parameters = "@patientid={patientid},@raumid={raumid}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<StationaererAufenthalt> stationaererAufenthalte)
        {
            return new OkObjectResult(stationaererAufenthalte.FirstOrDefault());
        }
    }
}
