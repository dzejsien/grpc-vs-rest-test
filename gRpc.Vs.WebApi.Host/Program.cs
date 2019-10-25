using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using gRpc.Vs.WebApi.Logic;

namespace gRpc.Vs.WebApi.Host
{
    /*
     *BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17763.737 (1809/October2018Update/Redstone5)
       Intel Core i7-8650U CPU 1.90GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
       .NET Core SDK=3.0.100
       [Host]     : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT
       DefaultJob : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT
       
       
       |           Method |     Mean |     Error |   StdDev | Rank |     Gen 0 |    Gen 1 |    Gen 2 | Allocated |
       |----------------- |---------:|----------:|---------:|-----:|----------:|---------:|---------:|----------:|
       |  SyncDeserialize | 35.88 ms | 0.7709 ms | 1.676 ms |    2 | 1933.3333 | 600.0000 | 466.6667 |   3.25 KB |
       | AsyncDeserialize | 31.11 ms | 0.6449 ms | 1.077 ms |    1 |  687.5000 | 312.5000 |        - |  14.13 KB |

        
     *
     * when changed in Async to Deserialize<IEnumerbale>
     *
     * |           Method |     Mean |     Error |    StdDev | Rank |     Gen 0 |    Gen 1 |    Gen 2 | Allocated |
       |----------------- |---------:|----------:|----------:|-----:|----------:|---------:|---------:|----------:|
       |  SyncDeserialize | 31.46 ms | 0.4354 ms | 0.4073 ms |    1 | 2000.0000 | 750.0000 | 500.0000 |   3.25 KB |
       | AsyncDeserialize | 31.95 ms | 0.7008 ms | 1.9766 ms |    1 |  687.5000 | 312.5000 |  31.2500 |  14.53 KB |
     *
     *
     * fixed options - to prevent allocations
     *
     * |           Method |     Mean |     Error |    StdDev | Rank |     Gen 0 |    Gen 1 |    Gen 2 | Allocated |
       |----------------- |---------:|----------:|----------:|-----:|----------:|---------:|---------:|----------:|
       |  SyncDeserialize | 29.24 ms | 0.5523 ms | 0.5166 ms |    2 | 1781.2500 | 500.0000 | 281.2500 |   3.24 KB |
       | AsyncDeserialize | 26.67 ms | 0.4331 ms | 0.4051 ms |    1 |  687.5000 | 281.2500 |        - |   3.24 KB |
     */

    class Program
    {
        static void Main()
        { 
            BenchmarkRunner.Run<TestJsonDeserialization>();
        }
    }

    [RankColumn]
    [MemoryDiagnoser]
    public class TestJsonDeserialization
    {
        [Benchmark]
        public async Task<int> SyncDeserialize()
        {
            var data = DataLoader.GetDataOld().GetAsyncEnumerator();

            var resultChunks = new List<Data>(200);

            try
            {
                while (await data.MoveNextAsync())
                {
                    resultChunks.Add(data.Current);

                    if (resultChunks.Count == 200)
                    {
                        resultChunks.Clear();
                    }
                }
            }
            finally
            {
                await data.DisposeAsync();
            }

            return resultChunks.Count;
        }

        [Benchmark]
        public async Task<int> AsyncDeserialize()
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
                        resultChunks.Clear();
                    }
                }
            }
            finally
            {
                await data.DisposeAsync();
            }

            return resultChunks.Count;
        }
    }
}
