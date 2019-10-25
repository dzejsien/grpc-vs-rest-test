using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gRpc.Vs.WebApi.Logic
{
    public class DataLoader
    {
        static readonly JsonSerializerOptions Opt = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(), new ArgumentConverter() }
        };

        public static async IAsyncEnumerable<Data> GetDataOld()
        {
            await using var stream = File.OpenRead("./MOCK_DATA1.json");
            using var data = await JsonDocument.ParseAsync(stream);

            foreach (var jsonElement in data.RootElement.EnumerateArray())
                yield return JsonSerializer.Deserialize<Data>(jsonElement.GetRawText(), Opt); // it should not allocating string like with GetRawText, but sth not working - for investigation! (JsonElementSerializer)
        }

        public static async IAsyncEnumerable<Data> GetData()
        {
            await using var stream = File.OpenRead("MOCK_DATA1.json");
            var list = await JsonSerializer.DeserializeAsync<IList<Data>>(stream, Opt);

            foreach (var elem in list)
                yield return elem;
        }
    }


}

