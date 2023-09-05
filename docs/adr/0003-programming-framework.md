# 3. Programming framework

Date: 2020-10-22

## Status

Accepted

## Context

We would like to have a default programming language/base framework for our server-side components so we can have common tooling and practices across sub-systems. Our partner agency has expressed that they have a strong preference for the Microsoft ecosystem – they have significant investments in teams, systems, and tools using that technology stack.

## Decision

We will use C# and .NET Core for our server-side components.

## Consequences

- This should allow other technology teams at our partner agency to more easily understand and maintain the system after the 18F engagement ends.
- The 18F Engineering Chapter's collective experience is not deep in C# and .NET – ramp up for new 18F engineers will take longer.
