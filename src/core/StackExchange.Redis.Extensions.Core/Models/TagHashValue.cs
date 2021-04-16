using System;
using System.Runtime.Serialization;

namespace StackExchange.Redis.Extensions.Core.Models
{
    [Serializable]
    [DataContract]
    public class TagHashValue
    {
        [DataMember(Order = 1)]
        public string HashKey { get; set; }

        [DataMember(Order = 2)]
        public string Key { get; set; }
    }
}
