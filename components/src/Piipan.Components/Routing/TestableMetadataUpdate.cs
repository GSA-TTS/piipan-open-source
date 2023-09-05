// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Taken from https://github.com/dotnet/aspnetcore/tree/v6.0.6/src/Components/Components/src
// Unfortunately these classes are marked internal, so we had to pull them down to create the PiipanRouter

using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;

namespace Piipan.Components.Routing
{
    [ExcludeFromCodeCoverage]
    internal sealed class TestableMetadataUpdate
    {
        public static bool TestIsSupported { private get; set; }

        /// <summary>
        /// A proxy for <see cref="MetadataUpdater.IsSupported" /> that is testable.
        /// </summary>
        public static bool IsSupported => MetadataUpdater.IsSupported || TestIsSupported;
    }
}