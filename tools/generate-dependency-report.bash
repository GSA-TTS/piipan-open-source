#!/usr/bin/env bash

#
# Lists Node and DotNet dependencies for Piipan workspace
# Should be run at the root level of Piipan workspace/repository e.g. ./tools/generate-dependency-report.bash
#
# usage: generate-dependency-report.bash

cd dashboard/src/Piipan.Dashboard || exit
npm ls
cd ../../.. || exit
cd query-tool/src/Piipan.QueryTool || exit
npm ls
cd ../../.. || exit

dotnet list package