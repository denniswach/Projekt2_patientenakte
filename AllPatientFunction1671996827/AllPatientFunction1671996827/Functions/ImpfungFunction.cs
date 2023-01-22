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
    public class ImpfungFunction
    {
        [HttpGet]
        [FunctionName("GetImpfungen")]
        [OpenApiOperation(operationId: "GetImpfungen", tags: new[] { "Impfung" })]
        [OpenApiParameter(name: "impfstoffid", In = ParameterLocation.Query, Type = typeof(Guid))]
        [OpenApiParameter(name: "impfstoffid", In = ParameterLocation.Query, Type = typeof(Guid))]
        [OpenApiParameter(name: "arztid", In = ParameterLocation.Query, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<ImpfungResponse>), Description = "The OK response")]
        public ActionResult<List<ImpfungResponse>> GetImpfungen(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "impfungen")] HttpRequest req,
            [Sql("select i.patientid, i. impfstoffid, i.datum, i.arzt, i.anzahl, imp.bezeichnung as ibezeichnung, imp.wirkstoff as iwirkstoff " +
            "from Impfung as i " +
            "join impfstoff as imp on imp.id = i.impfstoffid",
                CommandType = CommandType.Text,
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<ImpfungResponse> impfungen)
        {
            if (req.Query["patientid"].Count != 0) impfungen = impfungen.Where(i => i.PatientId == req.Query["patientid"]).ToList();
            if (req.Query["impfstoffid"].Count != 0) impfungen = impfungen.Where(i => i.ImpfstoffId == req.Query["impfstoffid"]).ToList();
            if (req.Query["arztid"].Count != 0) impfungen = impfungen.Where(i => i.Arzt.ToString() == req.Query["arztid"]).ToList();
            return new OkObjectResult(impfungen);
        }

        [FunctionName("GetImpfungId")]
        [OpenApiOperation(operationId: "GetImpfungId", tags: new[] { "Impfung" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiParameter(name: "impfstoffid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ImpfungResponse))]
        public ActionResult<ImpfungResponse> GetImpfungId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "impfungen/{patientid}/{impfstoffid}")] HttpRequest req,
           [Sql("select i.patientid, i. impfstoffid, i.datum, i.arzt, i.anzahl, imp.bezeichnung as ibezeichnung, imp.wirkstoff as iwirkstoff " +
            "from Impfung as i " +
            "join impfstoff as imp on imp.id = i.impfstoffid " +
            "where i.patientid = @patientid and i.impfstoffid = @impfstoffid",
                CommandType = CommandType.Text,
                Parameters = "@patientid={patientid},@impfstoffid={impfstoffid}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<ImpfungResponse> impfungen)
        {
            return new OkObjectResult(impfungen.FirstOrDefault());
        }

        [FunctionName("CreateImpfung")]
        [OpenApiOperation(operationId: "CreateImpfung", tags: new[] { "Impfung" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ImpfungRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Impfung))]
        public async Task<ActionResult<Impfung>> CreateKrankekassen(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "impfungen")] HttpRequest req,
        [Sql("dbo.impfung", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Impfung> impfungen)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Impfung>(requestBody);

            data.Datum = DateTime.Now;

            await impfungen.AddAsync(data);
            await impfungen.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateImpfung")]
        [OpenApiOperation(operationId: "UpdateImpfung", tags: new[] { "Impfung" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiParameter(name: "impfstoffid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ImpfungRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Impfung))]
        public async Task<IActionResult> UpdateImpfung(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "impfungen/{patientid}/{impfstoffid}")] HttpRequest req,
                [Sql("dbo.impfung",
                ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Impfung> impfungen,
             [Sql("select * from Impfung where patientid = @patientid and impfstoffid = @impfstoffid",
                CommandType = CommandType.Text,
                Parameters = "@patientid={patientid},@impfstoffid={impfstoffid}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Impfung> ReadImpfungen)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<ImpfungRequest>(requestBody);
            var impfung = ReadImpfungen.FirstOrDefault();
            impfung.Arzt = data.Arzt ?? impfung.Arzt;
            impfung.Anzahl = data.Anzahl ?? impfung.Anzahl;
            impfung.Datum = data.Datum ?? impfung.Datum;

            await impfungen.AddAsync(impfung);
            await impfungen.FlushAsync();

            return new OkObjectResult(impfung);
        }

        [FunctionName("DeleteImpfung")]
        [OpenApiOperation(operationId: "DeleteImpfung", tags: new[] { "Impfung" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiParameter(name: "impfstoffid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Impfung))]
        public IActionResult DeleteImpfungen(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "impfungen/{patientid}/{impfstoffid}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.impfung where patientid = @patientid and impfstoffid = @impfstoffid",
            CommandType = CommandType.Text,
            Parameters = "@patientid={patientid},@impfstoffid={impfstoffid}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Impfung> impfungen)
        {
            return new OkObjectResult(impfungen.FirstOrDefault());
        }
    }
}
