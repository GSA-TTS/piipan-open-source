# OpenAPI specifications

## Prerequisites
- [Swagger/OpenAPI CLI](https://github.com/APIDevTools/swagger-cli)

## Summary

Piipan's APIs are described according to the [OpenAPI 3.0 Specification](https://swagger.io/specification/) (OAS). The OAS documents are structured as:

- Base definition in `./openapi/{api-name}/index.yaml`
- Schemas for components (request bodies, models, etc) in `./openapi/schemas/{schema-name}.yaml`
- External schemas are included within `index.yaml` as relative imports in the `components` section:
```
    # index.yaml
    components:
        schemas:
            Example:
                $ref: '../schemas/example.yaml#/Example'
```
- Those components are then utilized as local references elsewhere:
```
    # index.yaml
    content:
        application/json:
            schema:
                $ref: '#/components/schemas/Example'
```

This structure limits the use of inline models when generating bundled files and/or code ([detailed explanation](https://mux.com/blog/an-adventure-in-openapi-v3-api-code-generation/)).

## Validating and bundling specs

To validate a spec, use the `swagger-cli validate` and pass it the location of the base `index.yaml` file. For example:

```
    swagger-cli validate path/to/index.yaml
```

Many popular tools do not support references to external files. To avoid this issue, use `swagger-cli bundle` to bundle `index.yaml` into a single file with only internal references. For example:

```
    swagger-cli bundle path/to/index.yaml -o openapi.yaml -t yaml
```

These two commands have been combined into a helper script at `tools/generate-specs.bash` which runs against the orchestrator (`openapi/orchestrator`) spec:

```
    cd match
    ./tools/generate-specs.bash
```

If the spec validates successfully it will be bundled and the result will be outputted to `docs/openapi/orchestrator/generated/openapi.yaml`.

## Notes

- Currently, the output from `generate-specs` is ignored in version control.
