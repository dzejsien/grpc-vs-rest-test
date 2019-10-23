using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gRpc.Vs.WebApi.Logic
{
    public class DataLoader
    {
        public static async IAsyncEnumerable<Data> GetData()
        {
            await using var stream = File.OpenRead("./MOCK_DATA1.json");
            using (var data = await JsonDocument.ParseAsync(stream))
            {
                var opt = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(), new ArgumentConverter() }
                };

                foreach (var jsonElement in data.RootElement.EnumerateArray())
                    // it should not allocating string like with GetRawText, but sth not working - for investigation! (JsonElementSerializer)
                    yield return JsonSerializer.Deserialize<Data>(jsonElement.GetRawText(), opt);
            }
        }
    }

    
}

