// <copyright file="Logging.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Infrastructure.Logging;

using Microsoft.Extensions.Logging;

/// <summary>
/// Provides access to a global ILoggerFactory and typed loggers.
/// Set this up once in the startup (e.g., WPF App).
/// </summary>
public static class Logging
{
    public static ILoggerFactory Factory { get; set; } = null!;

    public static ILogger<T> CreateLogger<T>() => Factory.CreateLogger<T>();
}
