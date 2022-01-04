namespace StackExchange.Redis.Extensions.Core.Models;

/// <summary>
/// The result class for rank results
/// </summary>
/// <typeparam name="T"></typeparam>
public class ScoreRankResult<T>
{
    /// <summary>
    /// The element into redis
    /// </summary>
    public T Element { get; set; }

    /// <summary>
    /// The score
    /// </summary>
    public double Score { get; set; }
}
