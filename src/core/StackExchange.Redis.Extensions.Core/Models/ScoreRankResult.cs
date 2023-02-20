// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace StackExchange.Redis.Extensions.Core.Models;

/// <summary>
/// The result class for rank results
/// </summary>
/// <typeparam name="T"></typeparam>
public class ScoreRankResult<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScoreRankResult{T}"/> class.
    /// </summary>
    /// <param name="element">The element into redis.</param>
    /// <param name="score">The score.</param>
    public ScoreRankResult(T? element, double score)
    {
        Element = element;
        Score = score;
    }

    /// <summary>
    /// The element into redis
    /// </summary>
    public T? Element { get; }

    /// <summary>
    /// The score
    /// </summary>
    public double Score { get; }
}
