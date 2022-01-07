// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

using StackExchange.Redis.Extensions.Core.Abstractions;

namespace StackExchange.Redis.Extensions.Core.Models;

/// <summary>
/// Function in order to retrieve appropriate instance of the <see cref="IStateAwareConnection"/>
/// </summary>
/// <param name="connectionMultiplexer"><see cref="IConnectionMultiplexer"/> to wrap</param>
/// <param name="logger">Optional logger</param>
/// <returns>Appropriate instance of <see cref="IStateAwareConnection"/></returns>
public delegate IStateAwareConnection StateAwareConnectionResolver(IConnectionMultiplexer connectionMultiplexer, ILogger logger);
