# State-facing API Specification

## Prerequisites
- [Swagger/OpenAPI CLI](https://github.com/APIDevTools/swagger-cli)
- [Swagger Markdown tool](https://www.npmjs.com/package/swagger-markdown)

These are the OpenAPI specifications for our state-facing APIs. The specs mirror what's found in our Azure API Management (APIM) instance as the "Duplication Participation" API.

The goal of the specs is to generate documentation that states will use to integrate with our system.

## Directory Structure

Within the top-level `openapi` directory, there is an OpenAPI spec representing each individual API. Since the APIs are essentially a compilation of our various subsystem APIs, the specs are composed of paths from various subsystem OpenAPI specs. The subsystem specs are managed in their own subdirectories as the single source of truth, and merely referenced here.

The top-level directory also has a tools directory for automatic documentation generation.

Generating documentation will alter the contents of the `generated` directory. Generated documentation is checked into source control and should be re-generated whenever the specs are edited.

## Generating Documentation

From piipan project root, `cd docs/openapi` then run:
```
./tools/generate-docs.bash
```

This creates markdown files within the `generated` directory that are used for external documentation.
