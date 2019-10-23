using System.Threading.Tasks;

namespace gRpc.Vs.WebApi.RestClient
{
    public interface IDataClient
    {
        Task<DataResponse> GetDataAsync(DataRequest request);
        Task<DataResponse> GetDataStreamAsync(DataRequest request);
    }
}