// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Taken from https://github.com/dotnet/aspnetcore/tree/v6.0.6/src/Components/Components/src
// Unfortunately these classes are marked internal, so we had to pull them down to create the PiipanRouter

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;

[assembly: MetadataUpdateHandler(typeof(Piipan.Components.Routing.HotReloadManager))]

namespace Piipan.Components.Routing
{
    [ExcludeFromCodeCoverage]
    internal static class HotReloadManager
    {
        public static event Action? OnDeltaApplied;

        /// <summary>
        /// Gets a value that determines if OnDeltaApplied is subscribed to.
        /// </summary>
        public static bool IsSubscribedTo => OnDeltaApplied is not null;

        /// <summary>
        /// MetadataUpdateHandler event. This is invoked by the hot reload host via reflection.
        /// </summary>
        public static void UpdateApplication(Type[]? _) => OnDeltaApplied?.Invoke();
    }
}