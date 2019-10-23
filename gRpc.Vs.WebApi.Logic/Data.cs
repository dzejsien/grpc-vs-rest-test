using System;
using System.Collections.Generic;

namespace gRpc.Vs.WebApi.Logic
{
    public class Data
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public Status Status { get; set; }
        public IList<Argument> Arguments { get; set; }
    }

    public readonly struct Argument : IEquatable<Argument>
    {
        public Argument(int first, string second)
        {
            First = first;
            Second = second;
        }

        public int First { get; }
        public string Second { get; }

        public bool Equals(Argument other)
        {
            return First == other.First && string.Equals(Second, other.Second);
        }

        public override bool Equals(object obj)
        {
            return obj is Argument other && Equals(other);
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