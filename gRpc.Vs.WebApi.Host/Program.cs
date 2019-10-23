using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gRpc.Vs.WebApi.Logic;

namespace gRpc.Vs.WebApi.Host
{
    class Program
    {
        static async Task Main()
        {
            var data = DataLoader.GetData().GetAsyncEnumerator();

            var resultChunks = new List<Data>(200);

            try
            {
                while (await data.MoveNextAsync())
                {
                    resultChunks.Add(data.Current);

                    if (resultChunks.Count == 200)
                    {
                        Console.WriteLine("Read 200 elements in chunk");
                        resultChunks.Clear();
                    }
                }
            }
            finally
            {
                await data.DisposeAsync();
            }
        }
    }
}
