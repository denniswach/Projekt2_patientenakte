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
    public class MedikationsplanFunction
    {
        [HttpGet]
        [FunctionName("GetMedikationsplane")]
        [OpenApiOperation(operationId: "GetMedikationsplane", tags: new[] { "Medikationsplan" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "medikamentid", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "arztid", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<MedikationsplanResponse>), Description = "The OK response")]
        public ActionResult<List<MedikationsplanResponse>> GetMedikationsplane(
       [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "medikationsplane")] HttpRequest req,
       [Sql("select m.patientid, m.medikamentid, m.von, m.bis, m.staerke, m.hinweise, m.grund, m.arzt, p.name as pname, p.vorname as pvorname, a.name as aname, a.vorname as avorname, me.name as mname, me.wirkstoff as mwirkstoff " +
            "from Medikationsplan as m " +
            "join patient as p on p.id = m.patientid " +
            "join medikament as me on me.id = m.medikamentid " +
            "join arzt as a on a.id = m.arzt",
                CommandType = CommandType.Text,
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<MedikationsplanResponse> medikationsplane)
        {
            if (req.Query["patientid"].Count != 0) medikationsplane = medikationsplane.Where(k => k.PatientId.ToString() == req.Query["patientid"]).ToList();
            if (req.Query["medikamentid"].Count != 0) medikationsplane = medikationsplane.Where(k => k.MedikamentId.ToString() == req.Query["medikamentid"]).ToList();
            if (req.Query["arztid"].Count != 0) medikationsplane = medikationsplane.Where(k => k.Arzt.ToString() == req.Query["arztid"]).ToList();
            return new OkObjectResult(medikationsplane);
        }

        [FunctionName("GetMedikationsplanId")]
        [OpenApiOperation(operationId: "GetMedikationsplanId", tags: new[] { "Medikationsplan" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiParameter(name: "medikamentid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MedikationsplanResponse))]
        public ActionResult<MedikationsplanResponse> GetMedikationsplanId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "medikationsplane/{patientid}/{medikamentid}")] HttpRequest req,
            [Sql("select m.patientid, m.medikamentid, m.von, m.bis, m.staerke, m.hinweise, m.grund, m.arzt, p.name as pname, p.vorname as pvorname, a.name as aname, a.vorname as avorname, me.name as mname, me.wirkstoff as mwirkstoff " +
            "from Medikationsplan as m " +
            "join patient as p on p.id = m.patientid " +
            "join medikament as me on me.id = m.medikamentid " +
            "join arzt as a on a.id = m.arzt " +
            "where m.patientid = @patientid and m.medikamentid = @medikamentid",
                CommandType = CommandType.Text,
                Parameters = "@patientid={patientid},@medikamentid={medikamentid}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<MedikationsplanResponse> medikationsplane)
        {
            return new OkObjectResult(medikationsplane.FirstOrDefault());
        }

        [FunctionName("CreateMedikationsplan")]
        [OpenApiOperation(operationId: "CreateMedikationsplan", tags: new[] { "Medikationsplan" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(MedikationsplanRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Medikationsplan))]
        public async Task<ActionResult<Medikationsplan>> CreateKrankekassen(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "medikationsplane")] HttpRequest req,
        [Sql("dbo.medikationsplan", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Medikationsplan> medikationsplane)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Medikationsplan>(requestBody);       

            await medikationsplane.AddAsync(data);
            await medikationsplane.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateMedikationsplan")]
        [OpenApiOperation(operationId: "UpdateMedikationsplan", tags: new[] { "Medikationsplan" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiParameter(name: "medikamentid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(MedikationsplanRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Medikationsplan))]
        public async Task<IActionResult> UpdateMedikationsplan(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "medikationsplane/{patientid}/{medikamentid}")] HttpRequest req,
                [Sql("dbo.medikationsplan",
                ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Medikationsplan> medikationsplane,
             [Sql("select * from Medikationsplan where patientid = @patientid and medikamentid = @medikamentid",
                CommandType = CommandType.Text,
                Parameters = "@patientid={patientid},@medikamentid={medikamentid}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Medikationsplan> ReadMedikationsplane)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<MedikationsplanRequest>(requestBody);
            var medikationsplan = ReadMedikationsplane.FirstOrDefault();
            medikationsplan.Von = data.Von ?? medikationsplan.Von;
            medikationsplan.Bis = data.Bis ?? medikationsplan.Bis;
            medikationsplan.Staerke = data.Staerke ?? medikationsplan.Staerke;
            medikationsplan.Hinweise = data.Hinweise ?? medikationsplan.Hinweise;
            medikationsplan.Grund = data.Grund ?? medikationsplan.Grund;
            medikationsplan.Arzt = data.Arzt ?? medikationsplan.Arzt;

            await medikationsplane.AddAsync(medikationsplan);
            await medikationsplane.FlushAsync();

            return new OkObjectResult(medikationsplan);
        }

        [FunctionName("DeleteMedikationsplan")]
        [OpenApiOperation(operationId: "DeleteMedikationsplan", tags: new[] { "Medikationsplan" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiParameter(name: "medikamentid", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Medikationsplan))]
        public IActionResult DeleteMedikationsplane(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "medikationsplane/{patientid}/{medikamentid}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.medikationsplan where patientid = @patientid and medikamentid = @medikamentid",
            CommandType = CommandType.Text,
            Parameters = "@patientid={patientid},@medikamentid={medikamentid}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Medikationsplan> medikationsplane)
        {
            return new OkObjectResult(medikationsplane.FirstOrDefault());
        }
    }
}
