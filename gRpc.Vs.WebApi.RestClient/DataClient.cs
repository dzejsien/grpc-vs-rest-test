using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gRpc.Vs.WebApi.RestClient
{
    public class DataClient : IDataClient
    {
        private const string GetSimple = "data";
        private const string GetStream = "data/get_stream";

        private readonly HttpClient _client;

        public DataClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<DataResponse> GetDataAsync(DataRequest request)
        {
            var response = await _client.GetAsync(GetSimple);

            response.EnsureSuccessStatusCode();

            var opt = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(), new ArgumentModelConverter() }
            };

            var stream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<DataResponse>(stream, opt);
            return result;
        }

        public async Task<DataResponse> GetDataStreamAsync(DataRequest request)
        {
            var response = await _client.GetStreamAsync(GetStream);

            var opt = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(), new ArgumentModelConverter() }
            };
            // chunks - this merging can be done better i think ...
            var result = await JsonSerializer.DeserializeAsync<DataResponse[]>(response, opt);
            var mergedResult = new DataResponse { Data = result.SelectMany(x => x.Data).ToList() };
            return mergedResult;
        }
    }
}