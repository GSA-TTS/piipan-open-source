// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Taken from https://github.com/dotnet/aspnetcore/tree/v6.0.6/src/Components/Components/src
// Unfortunately these classes are marked internal, so we had to pull them down to create the PiipanRouter

using System.Diagnostics.CodeAnalysis;

namespace Piipan.Components.Routing
{
    [ExcludeFromCodeCoverage]
    internal class RouteTable
    {
        public RouteTable(RouteEntry[] routes)
        {
            Routes = routes;
        }

        public RouteEntry[] Routes { get; }

        public void Route(RouteContext routeContext)
        {
            for (var i = 0; i < Routes.Length; i++)
            {
                Routes[i].Match(routeContext);
                if (routeContext.Handler != null)
                {
                    return;
                }
            }
        }
    }
}