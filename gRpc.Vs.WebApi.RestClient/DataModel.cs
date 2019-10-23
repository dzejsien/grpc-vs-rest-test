using System;
using System.Collections.Generic;

namespace gRpc.Vs.WebApi.RestClient
{

    public class DataModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public Status Status { get; set; }
        public IList<ArgumentModel> Arguments { get; set; } = new ArgumentModel[0];
    }

    public readonly struct ArgumentModel : IEquatable<ArgumentModel>
    {
        public ArgumentModel(int first, string second)
        {
            First = first;
            Second = second;
        }

        public int First { get; }
        public string Second { get; }

        public bool Equals(ArgumentModel other)
        {
            return First == other.First && string.Equals(Second, other.Second);
        }

        public override bool Equals(object obj)
        {
            return obj is ArgumentModel other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (First * 397) ^ (Second != null ? Second.GetHashCode() : 0);
            }
        }
    }

    public enum Status
    {
        None,
        Active,
        Inactive,
    }
}