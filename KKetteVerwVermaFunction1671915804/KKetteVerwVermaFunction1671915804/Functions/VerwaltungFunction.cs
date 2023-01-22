using KKetteVerwVermaFunction1671915804.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
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

namespace KKetteVerwVermaFunction1671915804.Functions
{
    public class VerwaltungFunction
    {
        [HttpGet]
        [FunctionName("GetVerwaltungen")]
        [OpenApiOperation(operationId: "GetVerwaltungen", tags: new[] { "Verwaltung" })]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Verwaltung>), Description = "The OK response")]
        public ActionResult<List<Verwaltung>> GetVerwaltungen(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "verwaltungen")] HttpRequest req,
        [Sql("select * from verwaltung", ConnectionStringSetting = "SQLConnection")] IEnumerable<Verwaltung> verwaltungen)
        {
            return new OkObjectResult(verwaltungen);
        }

        [HttpGet]
        [FunctionName("GetVerwaltungenId")]
        [OpenApiOperation(operationId: "GetVerwaltungenId", tags: new[] { "Verwaltung" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Verwaltung))]
        public ActionResult<Verwaltung> GetVerwaltungenId(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "verwaltungen/{id}")] HttpRequest req,
        [Sql("select * from verwaltung where id = @Id ",
            CommandType = CommandType.Text,
            Parameters ="@Id={id}",
            ConnectionStringSetting = "SQLConnection")]
            IEnumerable<Verwaltung> verwaltungen)
        {
            return new OkObjectResult(verwaltungen.FirstOrDefault());
        }


        [HttpPost]
        [FunctionName("CreateVerwaltungen")]
        [OpenApiOperation(operationId: "CreateVerwaltungen", tags: new[] { "Verwaltung" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(VerwaltungRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Verwaltung))]
        public async Task<ActionResult<Verwaltung>> CreateVerwaltung(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "verwaltungen")] HttpRequest req,
        [Sql("dbo.krankenhauskette", ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Krankenhauskette> krankenhausketten,
        [Sql("dbo.verwaltung", ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Verwaltung> verwaltungen,
        [Sql("select id, name, verwaltung from Krankenhauskette",
            CommandType = CommandType.Text,
            ConnectionStringSetting = "SQLConnection")]
            SqlCommand command)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Verwaltung>(requestBody);
            data.Id = Guid.NewGuid();

            var krankenhauskette = new Krankenhauskette();
            command.CommandText += $" where id = {data.Krankenhauskette}";
            using (SqlConnection conn = command.Connection)
            {
                conn.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    krankenhauskette.Id = reader.GetGuid("id");
                    krankenhauskette.Name = reader.GetString("name");
                    krankenhauskette.Verwaltung = data.Id;
                }
            }
            await verwaltungen.AddAsync(data);
            await verwaltungen.FlushAsync();

            await krankenhausketten.AddAsync(krankenhauskette);
            await krankenhausketten.FlushAsync();

            return new OkObjectResult(data);
        }

        [FunctionName("UpdateVerwaltung")]
        [OpenApiOperation(operationId: "UpdateKrankenhausketten", tags: new[] { "Verwaltung" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(VerwaltungRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Verwaltung))]
        public async Task<ActionResult<Verwaltung>> UpdateVerwaltung(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "verwaltungen/{id}")] HttpRequest req,
        [Sql("select * from verwaltung where id = @Id ",
            CommandType = System.Data.CommandType.Text, Parameters ="@Id={id}", ConnectionStringSetting = "SQLConnection")] IEnumerable<Verwaltung> readVerwaltungen,
        [Sql("dbo.verwaltung", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Verwaltung> verwaltungen)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Verwaltung>(requestBody);

            var verwaltung = readVerwaltungen.FirstOrDefault();
            verwaltung.Adresse = data.Adresse ?? verwaltung.Adresse;
            verwaltung.Name = data.Name ?? verwaltung.Name;

            await verwaltungen.AddAsync(verwaltung);
            await verwaltungen.FlushAsync();

            return new OkObjectResult(verwaltung);
        }

        [FunctionName("DeleteVerwaltung")]
        [OpenApiOperation(operationId: "DeleteKrankenhausketten", tags: new[] { "Verwaltung" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Verwaltung))]
        public ActionResult<Verwaltung> DeleteVerwaltung(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "verwaltungen/{id}")] HttpRequest req,
        [Sql("delete from verwaltung where id = @Id ",
            CommandType = System.Data.CommandType.Text,
            Parameters ="@Id={id}", ConnectionStringSetting = "SQLConnection")] IEnumerable<Verwaltung> verwaltungen)
        {
            return new OkObjectResult(verwaltungen.FirstOrDefault());
        }
    }
}
