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
    public class PatientFunction
    {
        private readonly string _salt;

        public PatientFunction()
        {
            _salt = Environment.GetEnvironmentVariable("Salt");
        }

        [FunctionName("GetPatienten")]
        [OpenApiOperation(operationId: "GetPatienten", tags: new[] { "Patient" })]
        [OpenApiParameter(name: "email", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "username", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "krankenkasse", In = ParameterLocation.Query, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Patient>), Description = "The OK response")]
        public ActionResult<List<PatientResponse>> GetPatient(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "patienten")] HttpRequest req,
        [Sql("select p.id, p.[passhash], p.[name], p.[vorname], p.[geburtsdatum], p.[geschlecht], p.[adresse], p.[telefonnummer], p.[email], p.[krankenkasse], p.[dsgvo], p.[letzteaenderung], p.[username], p.[loeschanfrage], k.name as kname, k.standort as kstandort, k.isgesetzlich as kisgesetzlich " +
            " from Patient as p " +
            "join krankenkasse as k on k.id = p.krankenkasse",
            CommandType = CommandType.Text,
            ConnectionStringSetting = "SQLConnection")]
            IEnumerable<PatientResponse> patienten)
        {
            if (req.Query["email"].Count != 0) patienten = patienten.Where(p => p.Email == req.Query["email"]).ToList();
            if (req.Query["username"].Count != 0) patienten = patienten.Where(p => p.Username == req.Query["username"]).ToList();
            if (req.Query["krankenkasse"].Count != 0) patienten = patienten.Where(p => p.Krankenkasse.ToString() == req.Query["krankenkasse"]).ToList();
            return new OkObjectResult(patienten);
        }

        [FunctionName("GetPatientId")]
        [OpenApiOperation(operationId: "GetPatientId", tags: new[] { "Patient" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Patient))]
        public ActionResult<Patient> GetPatienteId(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "patienten/{id}")] HttpRequest req,
        [Sql("select p.id, p.[passhash], p.[name], p.[vorname], p.[geburtsdatum], p.[geschlecht], p.[adresse], p.[telefonnummer], p.[email], p.[krankenkasse], p.[dsgvo], p.[letzteaenderung], p.[username], p.[loeschanfrage], k.name as kname, k.standort as kstandort, k.isgesetzlich as kisgesetzlich " +
            " from Patient as p " +
            "join krankenkasse as k on k.id = p.krankenkasse " +
            "where p.id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
            IEnumerable<Patient> patienten)
        {
            return new OkObjectResult(patienten.FirstOrDefault());
        }

        [FunctionName("CreatePatient")]
        [OpenApiOperation(operationId: "CreatePatient", tags: new[] { "Patient" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(PatientRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Patient))]
        public async Task<ActionResult<Patient>> CreatePatient(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "patienten")] HttpRequest req,
        [Sql("dbo.patient", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Patient> patienten)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Patient>(requestBody);

            data.Id = Guid.NewGuid();
            data.Passhash = BCrypt.Net.BCrypt.HashPassword(data.Passhash, _salt);
            data.Letzteaenderung = DateTime.Now;

            await patienten.AddAsync(data);
            await patienten.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdatePatient")]
        [OpenApiOperation(operationId: "UpdatePatient", tags: new[] { "Patient" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(PatientRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Patient))]
        public async Task<IActionResult> UpdatePatient(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "patienten/{id}")] HttpRequest req,
        [Sql("dbo.patient",
            ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Patient> patienten,
        [Sql("select * from Patient where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Patient> readPatientn)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<PatientRequest>(requestBody);
            var patient = readPatientn.FirstOrDefault();

            patient.Username = data.Username ?? patient.Username;
            patient.Passhash = (data.Passhash != null) ? data.Passhash = BCrypt.Net.BCrypt.HashPassword(data.Passhash, _salt) : patient.Passhash;
            patient.Name = data.Name ?? patient.Name;
            patient.Email = data.Email ?? patient.Email;
            patient.Vorname = data.Vorname ?? patient.Vorname;
            patient.Adresse = data.Adresse ?? patient.Adresse;
            patient.Geschlecht = data.Geschlecht ?? patient.Geschlecht;
            patient.Krankenkasse = data.Krankenkasse ?? patient.Krankenkasse;
            patient.Letzteaenderung = DateTime.Now;
            patient.Dsgvo = data.Dsgvo ?? patient.Dsgvo;

            await patienten.AddAsync(patient);
            await patienten.FlushAsync();

            return new OkObjectResult(patient);
        }

        [FunctionName("DeletePatient")]
        [OpenApiOperation(operationId: "DeletePatient", tags: new[] { "Patient" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Patient))]
        public IActionResult DeletePatient(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "patienten/{id}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.patient where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Patient> patienten)
        {
            return new OkObjectResult(patienten.FirstOrDefault());
        }
    }
}
