using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gRpc.Vs.WebApi.Logic;
using gRpc.Vs.WebApi.RestClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace gRpc.Vs.WebApi.RestServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {
        //private readonly ILogger _logger;

        //public DataController(ILogger<DataController> logger)
        //{
        //    _logger = logger;
        //}

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = new List<DataModel>(4000);

            await foreach (var elem in DataLoader.GetData())
                result.Add(new DataModel
                {
                    Id = elem.Id,
                    IsActive = elem.IsActive,
                    Name = elem.Name ?? string.Empty,
                    Status = GetGrpcStatus(elem.Status),
                    Arguments = elem.Arguments.Select(x => new ArgumentModel(x.First, x.Second)).ToList()
                });

            return Ok(new DataResponse
            {
                Data = result 
            });
        }

        [HttpGet("get_stream")]
        public async IAsyncEnumerable<DataResponse> Get2()
        {
            var result = new List<DataModel>(500);

            await foreach (var elem in DataLoader.GetData())
            {
                result.Add(new DataModel
                {
                    Id = elem.Id,
                    IsActive = elem.IsActive,
                    Name = elem.Name ?? string.Empty,
                    Status = GetGrpcStatus(elem.Status),
                    Arguments = elem.Arguments.Select(x => new ArgumentModel(x.First, x.Second)).ToList()
                });

                if (result.Count == 500)
                {
                    // cant clear here, data will be empty if you firstly assign result and then clear (its deferred)
                    // this coping is also very weak ... 
                    yield return new DataResponse { Data = new List<DataModel>(result) };
                    result = new List<DataModel>(500);
                    //_logger.LogInformation("Chunk sent");
                }
            }
        }

        private static RestClient.Status GetGrpcStatus(Logic.Status elemStatus) =>
            elemStatus switch
                {
                Logic.Status.None => RestClient.Status.None,
                Logic.Status.Active => RestClient.Status.Active,
                Logic.Status.Inactive => RestClient.Status.Inactive,
                _ => throw new ArgumentException("invalid"),
                };
    }
}
