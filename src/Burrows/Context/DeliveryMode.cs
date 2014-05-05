namespace Burrows.Context
{
    /// <summary>
    /// Specifies the delivery mode of the message
    /// </summary>
    public enum DeliveryMode
    {
        /// <summary>
        /// Message is persisted to durable storage as part of delivery
        /// </summary>
        Persistent = 0,

        /// <summary>
        /// Message is not persisted to disk as part of delivery
        /// </summary>
        InMemory,
    }
}