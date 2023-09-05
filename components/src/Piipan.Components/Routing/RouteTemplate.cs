// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Taken from https://github.com/dotnet/aspnetcore/tree/v6.0.6/src/Components/Components/src
// Unfortunately these classes are marked internal, so we had to pull them down to create the PiipanRouter

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Piipan.Components.Routing
{
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("{TemplateText}")]
    internal class RouteTemplate
    {
        public RouteTemplate(string templateText, TemplateSegment[] segments)
        {
            TemplateText = templateText;
            Segments = segments;

            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                if (segment.IsOptional)
                {
                    OptionalSegmentsCount++;
                }
                if (segment.IsCatchAll)
                {
                    ContainsCatchAllSegment = true;
                }
            }
        }

        public string TemplateText { get; }

        public TemplateSegment[] Segments { get; }

        public int OptionalSegmentsCount { get; }

        public bool ContainsCatchAllSegment { get; }
    }
}