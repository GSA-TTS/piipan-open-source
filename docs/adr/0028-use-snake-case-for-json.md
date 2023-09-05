# Use Snake Case for JSON and Pascal Case for C#

## Status

Proposed

## Context

Our C# apps expose JSON API's. In order to accept and return data for these API's, the apps need to serialize C# data into JSON, and vice versa. For serialization, we use the [Newtonsoft Json.NET framework](https://www.newtonsoft.com/json).

If data being serialized/deserialized is in a consistent naming style, then serialization is straightforward. We annotate C# properties with a `JsonProperty` name, and Newtonsoft handles the rest. If naming styles are inconsistent, more workarounds are needed, which can lead to more bugs and incompatible API schemas.

To avoid these outcomes, we should use consistent naming styles for both C# and JSON properties. The naming styles don't need to be the same between the two languages, and in general, which styles we choose isn't as important as keeping them consistent. But since we've already been leaning towards certain choices, choosing the naming styles that are mostly already in place would result in the fewest changes to the codebase at this time.

For C# properties, we've been using `PascalCase` in accordance with Microsoft's [Capitalization Conventions](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/capitalization-conventions). For JSON data, `snake_case` has been our implicit convention. The reasoning for our JSON convention is less clear: while the JSON API specification offers [guidance](https://jsonapi.org/format/#document-member-names) on things like case sensitivity and allowed/reserved characters, it doesn't go as far as to set a specific case style for member names. Our reason for using `snake_case` simply could be that the JSON API's we're most familiar with use it too.

## Decision

For JSON properties, we will use `snake_case`. For C# properties, we will use `PascalCase`.

## Consequences

The only change to the current codebase I forsee is that any `jsonb` data stored in Postgres databases will need to be serialized into `snake_case`. Currently, this means only changing the participant `input` and `data` data on `matches` to be serialized into `snake_case` prior to database insertion.

We will continue to use a JSON serializer that can serialize/deserialize properties. We can use the same one we've been using (Newtonsoft), so no change is needed on that front.
