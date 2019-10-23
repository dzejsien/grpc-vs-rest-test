using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using gRpc.Vs.WebApi.Logic;
using GrpcData;

namespace gRpc.Vs.WebApi.GrpcServer.Services
{
    public class DataService : GrpcDataService.GrpcDataServiceBase
    {
        public override async Task<DataResponse> GetData(GetDataRequest request, ServerCallContext context)
        {
            var result = new List<DataProto>(4000);

            await foreach (var elem in DataLoader.GetData())
                result.Add(new DataProto
                {
                    Id = elem.Id,
                    IsActive = elem.IsActive,
                    Name = elem.Name ?? string.Empty,
                    Status = GetGrpcStatus(elem.Status),
                    Arguments = { elem.Arguments.Select(x => new GrpcData.Argument { First = x.First, Second = x.Second }) }
                });

            return new DataResponse
            {
                Data = { result }
            };
        }

        public override async Task GetDataStream(GetDataRequest request, IServerStreamWriter<DataResponse> responseStream, ServerCallContext context)
        {
            var result = new List<DataProto>(500);

            await foreach (var elem in DataLoader.GetData())
            {
                result.Add(new DataProto
                {
                    Id = elem.Id,
                    IsActive = elem.IsActive,
                    Name = elem.Name ?? string.Empty,
                    Status = GetGrpcStatus(elem.Status),
                    Arguments = { elem.Arguments.Select(x => new GrpcData.Argument { First = x.First, Second = x.Second }) }
                });

                if (result.Count == 500)
                {
                    await responseStream.WriteAsync(new DataResponse { Data = { result } });
                    result.Clear();
                }
            }
        }

        private static GrpcData.Status GetGrpcStatus(Logic.Status elemStatus) =>
                elemStatus switch
                {
                    Logic.Status.None => GrpcData.Status.None,
                    Logic.Status.Active => GrpcData.Status.Active,
                    Logic.Status.Inactive => GrpcData.Status.Inactive,
                    _ => throw new ArgumentException("invalid"),
                };
    }
}