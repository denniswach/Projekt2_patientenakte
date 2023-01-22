using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Linq;
using KKetteVerwVermaFunction1671915804.Models;

namespace KKetteVerwVermaFunction1671915804.Functions
{
    public class KrankenhausketteFunction
    {
        private readonly ILogger<KrankenhausketteFunction> _logger;

        public KrankenhausketteFunction(ILogger<KrankenhausketteFunction> log)
        {
            _logger = log;
        }

        [FunctionName("GetKrankenhausketten")]
        [OpenApiOperation(operationId: "GetKrankenhausketten", tags: new[] { "Krankenhauskette" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Krankenhauskette>), Description = "The OK response")]
        public ActionResult<List<Krankenhauskette>> GetKrankenhausketten(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "krankenhausketten")] HttpRequest req,
          [Sql("select * from Krankenhauskette",
                CommandType = CommandType.Text,
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Krankenhauskette> krankenhausketten)
        {
            if (req.Query["name"].Count != 0) krankenhausketten = krankenhausketten.Where(k => k.Name == req.Query["name"]).ToList();
            return new OkObjectResult(krankenhausketten);
        }

        [FunctionName("GetKrankenhauskettenId")]
        [OpenApiOperation(operationId: "GetKrankenhauskettenId", tags: new[] { "Krankenhauskette" })]
        [OpenApiParameter(name: "idd", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Krankenhauskette))]
        public ActionResult<Krankenhauskette> GetKrankenhauskettenId(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "krankenhausketten/{id}")] HttpRequest req,
            [Sql("select * from Krankenhauskette where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Krankenhauskette> krankenhausketten)
        {
            return new OkObjectResult(krankenhausketten.FirstOrDefault());
        }

        [FunctionName("CreateKrankenhausketten")]
        [OpenApiOperation(operationId: "CreateKrankenhausketten", tags: new[] { "Krankenhauskette" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(KrankenhausketteRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Krankenhauskette))]
        public async Task<ActionResult<Krankenhauskette>> CreateKrankenhausketten(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "krankenhausketten")] HttpRequest req,
        [Sql("dbo.krankenhauskette", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Krankenhauskette> krankenhausketten)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Krankenhauskette>(requestBody);

            data.Id = Guid.NewGuid();

            await krankenhausketten.AddAsync(data);
            await krankenhausketten.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateKrankenhausketten")]
        [OpenApiOperation(operationId: "UpdateKrankenhausketten", tags: new[] { "Krankenhauskette" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(KrankenhausketteRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Krankenhauskette))]
        public async Task<IActionResult> UpdateKrankenhausketten(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "krankenhausketten/{id}")] HttpRequest req,
                [Sql("dbo.krankenhauskette",
                ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Krankenhauskette> krankenhausketten,
             [Sql("select * from Krankenhauskette where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Krankenhauskette> ReadKrankenhausketten)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Krankenhauskette>(requestBody);
            var krankenhauskette = ReadKrankenhausketten.FirstOrDefault();
            krankenhauskette.Name = data.Name ?? krankenhauskette.Name;
            krankenhauskette.Verwaltung = data.Verwaltung ?? krankenhauskette.Verwaltung;

            await krankenhausketten.AddAsync(krankenhauskette);
            await krankenhausketten.FlushAsync();

            return new OkObjectResult(krankenhauskette);
        }

        [FunctionName("DeleteKrankenhausketten")]
        [OpenApiOperation(operationId: "DeleteKrankenhausketten", tags: new[] { "Krankenhauskette" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Krankenhauskette))]
        public IActionResult DeleteKrankenhausketten(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "krankenhausketten/{id}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.krankenhauskette where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Krankenhauskette> krankenhausketten)
        {
            return new OkObjectResult(krankenhausketten.FirstOrDefault());
        }




    }
}

