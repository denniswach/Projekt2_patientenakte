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
using Microsoft.SqlServer.Server;
using static System.Net.WebRequestMethods;
using System.Reflection.Metadata;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Sockets;

namespace RaumStatAufTerDok1672068366.Functions
{
    public class DokumentFunction
    {
        private readonly string _site;
        public DokumentFunction()
        {
            _site = Environment.GetEnvironmentVariable("Site");
        }

        [HttpGet]
        [FunctionName("GetDokumente")]
        [OpenApiOperation(operationId: "GetDokumente", tags: new[] { "Dokument" })]
        [OpenApiParameter(name: "patientid", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "bezeichnung", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Dokument>), Description = "The OK response")]
        public IActionResult GetDokumente(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dokumente")] HttpRequest req,
            [Sql("select id, patient, bezeichnung, dokumentname, contenttype, datum from Dokument",
                CommandType = CommandType.Text,
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<DokumentResponse> dokumente)
        {
            if (req.Query["patientid"].Count != 0) dokumente = dokumente.Where(d => d.Patient.ToString() == req.Query["patientid"]).ToList();
            if (req.Query["bezeichnung"].Count != 0) dokumente = dokumente.Where(d => d.Bezeichnung == req.Query["bezeichnung"]).ToList();
            foreach(var item in dokumente)
            {
                item.DokumentLink = $"{_site}/file/{item.Id}";
            }
            return new OkObjectResult(dokumente);
        }

        [FunctionName("GetDokumentId")]
        [OpenApiOperation(operationId: "GetDokumentId", tags: new[] { "Dokument" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Dokument))]
        public IActionResult GetDokumentId(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dokumente/{id}")] HttpRequest req,
            [Sql("select * from Dokument where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<DokumentResponse> dokumente)
        {
            var dokument = dokumente.FirstOrDefault();
            dokument.DokumentLink = $"{_site}/file/{dokument.Id}";
            return new OkObjectResult(dokument);
        }

        [FunctionName("GetFileId")]
        [OpenApiOperation(operationId: "GetFileId", tags: new[] { "Dokument" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Dokument))]
        public IActionResult GetFileId(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "file/{id}")] HttpRequest req,
            [Sql("select * from Dokument where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Dokument> dokumente)
        {
            var dokument = dokumente.FirstOrDefault();
            return new FileContentResult(dokument.DokumentStored, dokument.ContentType)
            {
                FileDownloadName = dokument.DokumentName
            };
        }


        [FunctionName("CreateDokument")]
        [OpenApiOperation(operationId: "CreateDokument", tags: new[] { "Dokument" })]
        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(BinaryData))]
        [OpenApiParameter(name: "patient", In = ParameterLocation.Query, Type = typeof(Guid))]
        [OpenApiParameter(name: "bezeichnung", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Dokument))]
        public async Task<ActionResult<string>> CreateKrankekassen(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "dokumente")] HttpRequest req,
        [Sql("dbo.dokument", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Dokument> dokumente)
        {
            var data = new Dokument();
            var formdata = await req.ReadFormAsync();
            var file = req.Form.Files["file"];

            if (file.Length > 500000) return new OkObjectResult("Filesize is too big");

            byte[] fileBytes;

            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                fileBytes = ms.ToArray();
                string s = Convert.ToBase64String(fileBytes);
            }
            data.DokumentStored = fileBytes;
            data.Id = Guid.NewGuid();
            data.Datum = DateTime.Now;
            data.ContentType = file.ContentType;
            data.DokumentName = file.FileName;
            data.Patient = (req.Query["patient"].Count != 0) ? Guid.Parse(req.Query["patient"]) : Guid.Empty;
            data.Bezeichnung = (req.Query["bezeichnung"].Count != 0) ? req.Query["bezeichnung"] : string.Empty;


            await dokumente.AddAsync(data);
            await dokumente.FlushAsync();

            return new OkObjectResult("Ok");
        }


        [FunctionName("UpdateDokument")]
        [OpenApiOperation(operationId: "UpdateDokument", tags: new[] { "Dokument" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(DokumentRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Dokument))]
        public async Task<IActionResult> UpdateDokument(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "dokumente/{id}")] HttpRequest req,
                [Sql("dbo.dokument",
                ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Dokument> dokumente,
             [Sql("select * from Dokument where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Dokument> ReadDokumente)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<DokumentRequest>(requestBody);
            var dokument = ReadDokumente.FirstOrDefault();
            dokument.Patient = data.Patient ?? dokument.Patient;
            dokument.Bezeichnung = data.Bezeichnung ?? dokument.Bezeichnung;
            dokument.Datum = data.Datum ?? dokument.Datum;

            await dokumente.AddAsync(dokument);
            await dokumente.FlushAsync();

            return new OkObjectResult(dokument);
        }

        [FunctionName("DeleteDokument")]
        [OpenApiOperation(operationId: "DeleteDokument", tags: new[] { "Dokument" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Dokument))]
        public IActionResult DeleteDokumente(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "dokumente/{id}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.dokument where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Dokument> dokumente)
        {
            return new OkObjectResult(dokumente.FirstOrDefault());
        }
    }
}
