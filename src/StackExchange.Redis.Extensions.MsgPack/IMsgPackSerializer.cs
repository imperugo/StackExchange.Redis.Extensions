namespace StackExchange.Redis.Extensions.MsgPack
{
    public interface IMsgPackSerializer
    {
        /// <summary>
        /// Serializes the specified bytes .
        /// </summary>
        /// <param name="item">The bytes.</param>
        /// <returns></returns>
        byte[] Serialize<T>(object item);   
        
        /// <summary>
        /// Deserializes the specified bytes .
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        T Deserialize<T>(byte[] bytes);
    }
}