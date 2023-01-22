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
using KrankenhAssisFachberArzt1671987158.Models;

namespace KrankenhAssisFachberArzt1671987158.Functions
{
    public class KrankenhausFunction
    {
        [FunctionName("GetKrankenhauser")]
        [OpenApiOperation(operationId: "GetKrankenhauser", tags: new[] { "Krankenhaus" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(name: "krankenhauskette", In = ParameterLocation.Query, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Krankenhaus>), Description = "The OK response")]
        public ActionResult<List<Krankenhaus>> GetKrankenhausn(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "krankenhauser")] HttpRequest req,
        [Sql("select * from Krankenhaus",
            CommandType = CommandType.Text,
            ConnectionStringSetting = "SQLConnection")]
            IEnumerable<Krankenhaus> krankenhauser)
        {
            if (req.Query["name"].Count != 0) krankenhauser = krankenhauser.Where(k => k.Name == req.Query["name"]).ToList();
            if (req.Query["krankenhauskette"].Count != 0) krankenhauser = krankenhauser.Where(k => k.Krankenhauskette.ToString() == req.Query["krankenhauskette"]).ToList();
            return new OkObjectResult(krankenhauser);
        }

        [FunctionName("GetKrankenhauserId")]
        [OpenApiOperation(operationId: "GetKrankenhauserId", tags: new[] { "Krankenhaus" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Krankenhaus))]
        public ActionResult<Krankenhaus> GetKrankenhausnId(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "krankenhauser/{id}")] HttpRequest req,
            [Sql("select * from Krankenhaus where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Krankenhaus> krankenhauser)
        {
            return new OkObjectResult(krankenhauser.FirstOrDefault());
        }

        [FunctionName("CreateKrankenhaus")]
        [OpenApiOperation(operationId: "CreateKrankenhaus", tags: new[] { "Krankenhaus" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(KrankenhausRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Krankenhaus))]
        public async Task<ActionResult<Krankenhaus>> CreateKrankenhausn(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "krankenhauser")] HttpRequest req,
        [Sql("dbo.Krankenhaus", ConnectionStringSetting = "SQLConnection")] IAsyncCollector<Krankenhaus> krankenhauser)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Krankenhaus>(requestBody);

            data.Id = Guid.NewGuid();

            await krankenhauser.AddAsync(data);
            await krankenhauser.FlushAsync();

            return new OkObjectResult(data);
        }


        [FunctionName("UpdateKrankenhaus")]
        [OpenApiOperation(operationId: "UpdateKrankenhaus", tags: new[] { "Krankenhaus" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(KrankenhausRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Krankenhaus))]
        public async Task<IActionResult> UpdateKrankenhausn(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "krankenhauser/{id}")] HttpRequest req,
                [Sql("dbo.Krankenhaus",
                ConnectionStringSetting = "SQLConnection")]
            IAsyncCollector<Krankenhaus> krankenhauser,
             [Sql("select * from Krankenhaus where id = @id",
                CommandType = CommandType.Text,
                Parameters = "@id={id}",
                ConnectionStringSetting = "SQLConnection")]
                IEnumerable<Krankenhaus> readKrankenhauser)
        {
            if (req.Path.Value.Split("/")[0] == null) return new BadRequestObjectResult("No id parameter.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Krankenhaus>(requestBody);
            var krankenhaus = readKrankenhauser.FirstOrDefault();
            krankenhaus.Name = data.Name ?? krankenhaus.Name;
            krankenhaus.Krankenhauskette = (data.Krankenhauskette != Guid.Empty) ? data.Krankenhauskette : krankenhaus.Krankenhauskette;
            krankenhaus.Adresse = data.Adresse ?? krankenhaus.Adresse;

            await krankenhauser.AddAsync(krankenhaus);
            await krankenhauser.FlushAsync();

            return new OkObjectResult(krankenhauser);
        }

        [FunctionName("DeleteKrankenhaus")]
        [OpenApiOperation(operationId: "DeleteKrankenhaus", tags: new[] { "Krankenhaus" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Krankenhaus))]
        public IActionResult DeleteKrankenhausn(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "krankenhauser/{id}")] HttpRequest req,
        ILogger log,
        [Sql("delete from dbo.Krankenhaus where id = @id",
            CommandType = CommandType.Text,
            Parameters = "@id={id}",
            ConnectionStringSetting = "SQLConnection")]
        IEnumerable<Krankenhaus> krankenhauser)
        {
            return new OkObjectResult(krankenhauser.FirstOrDefault());
        }
    }
}
