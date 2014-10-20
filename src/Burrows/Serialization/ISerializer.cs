using System;

namespace Burrows.Serialization
{
    public interface ISerializer
    {
        string Serialize(object payload);

        object Deserialize(Type type, string payload);

        T Deserialize<T>(string payload);
    }
}
