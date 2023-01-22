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
using System.ComponentModel;
using Newtonsoft.Json.Converters;

namespace RaumStatAufTerDok1672068366.Functions
{
    public class TerminFunction
    {
        [HttpGet]
        [FunctionName("GetTermine")]
        [OpenApiOperation(operationId: "GetTermine", tags: new[] { "Termin" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Query, Type = typeof(Guid))]
        [OpenApiParameter(name: "arztid", In = ParameterLocation.Query, Type = typeof(Guid))]
        [OpenApiParameter(name: "datetime", In = ParameterLocation.Query, Type = typeof(DateTime))]
        [OpenApiParameter(name: "date", In = ParameterLocation.Query, Type = typeof(string)), Description("TT.MM.YYYY")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Termin>), Description = "The OK response")]
        public ActionResult<List<TerminResponse>> GetTermine(
             [HttpTrigger(AuthorizationLevel.Function, "get", Route = "termine")] HttpRequest req,
             [Sql("select t.patientid, p.vorname as pvorname, p.name as pname, t.arztid, a.name as aname, a.vorname as avorname, t.datum, t.behandlung, t.raum, r.raumnummer, t.anliegen " +
            "from Termin as t " +
            "join raum as r on r.id = t.raum " +
            "join arzt as a on a.id = t.arztid " +
            "join patient as p on p.id = t.patientid",
                CommandType = CommandType.Text,
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<TerminResponse> termine)
        {
            if (req.Query["patientid"].Count != 0) termine = termine.Where(k => k.PatientId.ToString() == req.Query["patientid"]).ToList();
            if (req.Query["arztid"].Count != 0) termine = termine.Where(k => k.ArztId.ToString() == req.Query["arztid"]).ToList();

            if (req.Query["datetime"].Count != 0)
            {
                var datetime = DateTime.Parse(req.Query["datetime"]).AddHours(-1);

                termine = termine.Where(k => k.Datum == datetime).ToList();
            }
            if (req.Query["date"].Count != 0) termine = termine.Where(k => k.Datum.Date.ToString().Split(" ")[0] == req.Query["date"]).ToList();
            return new OkObjectResult(termine);
        }

        [FunctionName("GetTerminId")]
        [OpenApiOperation(operationId: "GetTerminId", tags: new[] { "Termin" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiParameter(name: "arztid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiParameter(name: "datum", In = ParameterLocation.Path, Required = true, Type = typeof(DateTime))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Termin))]
        public ActionResult<TerminResponse> GetTerminId(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "termine/{patientid}/{arztid}/{datum}")] HttpRequest req,
            [Sql("select t.patientid, p.vorname as pvorname, p.name as pname, t.arztid, a.name as aname, a.vorname as avorname, t.datum, t.behandlung, t.raum, r.raumnummer, t.anliegen " +
            "from Termin as t " +
            "join raum as r on r.id = t.raum " +
            "join arzt as a on a.id = t.arztid " +
            "join patient as p on p.id = t.patientid " +
            "where t.patientid = @patientid and t.arztid = @arztid and t.datum = @datum ",
                CommandType = CommandType.Text,
                Parameters = "@patientid={patientid},@arztid={arztid},@datum={datum}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Termin> termine)
        {
            return new OkObjectResult(termine.FirstOrDefault());
        }

        [FunctionName("CreateTermin")]
        [OpenApiOperation(operationId: "CreateTermin", tags: new[] { "Termin" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(TerminRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Termin))]
        public async Task<ActionResult<Termin>> CreateKrankekassen(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "termine")] HttpRequest req,
        [Sql("dbo.termin", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Termin> termine)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Termin>(requestBody);

            await termine.AddAsync(data);
            await termine.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateTermin")]
        [OpenApiOperation(operationId: "UpdateTermin", tags: new[] { "Termin" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiParameter(name: "arztid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiParameter(name: "datum", In = ParameterLocation.Path, Required = true, Type = typeof(DateTime))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(TerminRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Termin))]
        public async Task<IActionResult> UpdateTermin(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "termine/{patientid}/{arztid}/{datum}")] HttpRequest req,
                [Sql("dbo.termin",
                ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Termin> termine,
             [Sql("select * from Termin where patientid = @patientid and arztid = @arztid and datum = @datum",
                CommandType = CommandType.Text,
                Parameters = "@patientid={patientid},@arztid={arztid},@datum={datum}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Termin> ReadTermine)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<TerminRequest>(requestBody);
            var termin = ReadTermine.FirstOrDefault();
            termin.Behandlung = data.Behandlung ?? termin.Behandlung;
            termin.Raum = data.Raum ?? termin.Raum;
            termin.Anliegen = data.Anliegen ?? termin.Anliegen;

            await termine.AddAsync(termin);
            await termine.FlushAsync();

            return new OkObjectResult(termin);
        }

        [FunctionName("DeleteTermin")]
        [OpenApiOperation(operationId: "DeleteTermin", tags: new[] { "Termin" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiParameter(name: "arztid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiParameter(name: "datum", In = ParameterLocation.Path, Required = true, Type = typeof(DateTime))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Termin))]
        public IActionResult DeleteTermin(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "termine/{patientid}/{arztid}/{datum}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.termin where patientid = @patientid and arztid = @arztid and datum = @datum",
            CommandType = CommandType.Text,
            Parameters = "@patientid={patientid},@arztid={arztid},@datum={datum}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Termin> termine)
        {
            return new OkObjectResult(termine.FirstOrDefault());
        }
    }
}
