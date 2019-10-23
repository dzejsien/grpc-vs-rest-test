using System.Threading.Tasks;
using Grpc.Core;
using gRpc.Vs.WebApi.RestClient;
using GrpcData;
using Microsoft.AspNetCore.Mvc;

namespace gRpc.Vs.WebApi.Gateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {
        private readonly GrpcDataService.GrpcDataServiceClient _grpcClient;
        private readonly IDataClient _restClient;

        public DataController(GrpcDataService.GrpcDataServiceClient grpcClient, IDataClient restClient)
        {
            _grpcClient = grpcClient;
            _restClient = restClient;
        }

        [HttpGet]
        [Route("get_grpc_simple")]
        public async Task<IActionResult> GetGrpc()
        {
            var result = await _grpcClient.GetDataAsync(new GetDataRequest());
            return Ok(result);
        }

        [HttpGet]
        [Route("get_rest_simple")]
        public async Task<IActionResult> GetRest()
        {
            var result = await _restClient.GetDataAsync(new DataRequest());
            return Ok(result);
        }

        [HttpGet]
        [Route("get_grpc_stream")]
        public async Task<IActionResult> GetGrpcStream()
        {
            GrpcData.DataResponse result = null;

            using (var call = _grpcClient.GetDataStream(new GetDataRequest()))
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    if (result is null)
                        result = response;
                    else result.MergeFrom(response);
                }
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("get_rest_stream")]
        public async Task<IActionResult> GetRestStream()
        {
            var result = await _restClient.GetDataStreamAsync(new DataRequest());
            return Ok(result);
        }
    }
}