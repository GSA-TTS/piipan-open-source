# Use Event Sourcing for Match Resolution

## Status

Accepted

## Context

When matches are found, government entities must work together to resolve them. Our system needs to provide functionality to enable this collaboration and to audit this activity.

The system will need to:
- show the details and status of a match ("open" or "closed", etc.)
- changes the details and status of a match ("open" -> "closed") based on activity
- show activity history of a match (who did what? when did they do it? Why did they do it?)

On an abstract level, there are two major pieces here: state management and event tracking.

In a traditional [CRUD](https://en.wikipedia.org/wiki/Create,_read,_update_and_delete) system, state management entails updating the attributes of a single Domain record, e.g. a Match’s "status" field goes from "open" to "closed."

But inevitably, other questions arise:
- Who closed the match?
- When did the match go from open to closed?
- Why did the match to go from open to closed?

What these questions ultimately represent are not objects, but events, e.g. "Montana closed the match on February 7th when they set their final disposition."

Answering these questions simply by updating static fields on a single record quickly becomes unwieldy. Without some sort of auditing mechanism for these updates, conflicts may occur and history is lost.

Furthermore, in our context, updates to the details and status of matches will be done by multiple entities (e.g. two different states) as well as by both humans and automated systems. For example, when a state eligibility worker sets a final disposition on the match, the system may automatically set the status of the match from "open" to "closed." In these scenarios, the "Who?", "When?", and "Why?" questions become critical.

Event Sourcing is an established data pattern that is designed to record the "who", "what", "when", and "why" of changes. It is used in a variety of domains such as case management, e-commerce, ticketing, accounting, object tracking, and version control systems.

In its purest form, Event Sourcing saves records signifying events as the only data model and the single source of truth. These Event records save what domain attributes changed, when they changed, and who changed them. The Domain "record" is then composed of multiple Event records that are replayed to get the current status.

It’s important that Event records are Append-Only and immutable. It’s also vital that each record has an ID and timestamp (and/or incremental identifier) in order to replay events. It is also helpful to keep a version number on Event records for tracking schema changes.
#### Benefits of this approach:
- Single source of truth
- Auditing is baked into the design; querying Activity history is straightforward
- Immutability of data eliminates doubt over change history.
- When used with the [CQRS pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs), replaying events to compile stateful data does not require changes to the data store
- Deriving both the current state and former state is doable and of equal effort. For example, one could get what the status of a match is right now, or what it was a week ago with equal effort.

#### Drawback of this approach:
- Querying for stateful domain data becomes slightly more complex, could have scalability implications.**
- Depending on schema design, may involve redundant processes (e.g. creating multiple Event records to signify the creation of a single Domain record)

*(\*\*We anticipate a human-readable level of event records for a single match case, on a level of <25 event records for one case on average. Scalability risks can also be mitigated by caching the current state, or by using "snapshots" of the event history.)*

### A hybrid approach

Alternatively, many systems use Event Sourcing as a supplementary after-effect of actions performed on a Domain record. This is a hybrid between traditional CRUD and Event Sourcing. In this approach, when an action is performed on a single record, a related Event record is created immediately afterward as way to audit the action. Similar to the first approach, Event records are Append-Only and immutable, but the Domain record is mutable, and the source of truth is shared by both this record and its related Event records.

#### Benefits of this approach:
- Querying for current state is straightforward; querying for past state is still doable
- Creating a new Domain record is straightforward
- Many of the same benefits as the first approach
    - can derive both current and former states of the record
    - Immutability (but this can be complicated by data conflicts, see Drawbacks)

#### Drawbacks of this approach:
- No single source of truth
- If auditing fails, data conflicts can arise between the Domain record and its Event records.
- Instead of one task to record a change, there are now two: one to update the Domain record and another one to create the Event record.
- Data redundancy: If querying for state is possible through the Event records, then the Domain record can be redundant.
- Data schema needs to be synced between Domain record and Event record. If a new field is added or changed in the Domain record schema, the Event record schema needs to know about it.

Further Reading:
- [Overview of the Event Sourcing Pattern by Microsoft](https://docs.microsoft.com/en-us/azure/architecture/patterns/event-sourcing)
- [Event Sourcing by Martin Fowler](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Event Soucing Made Simple by Kickstarter](https://kickstarter.engineering/event-sourcing-made-simple-4a2625113224)

### What does "replaying" events look like?

Currently, our biggest need is to show the details and current status of a match to a handful of State Eligibility Workers, where the query for a match will be done immediately on their request.

In a traditional CRUD system using PostgreSQL, querying for a single Domain record is straightforward:

```sql
SELECT * FROM matches WHERE match_id = "ABCDE";
```

In a pure Event Sourcing model, a query would need to collect multiple records, order them, then "replay" them to get the Domain record.

It will likely start with a query to get all the necessary Match Events in order:

```sql
SELECT * from match_events
	WHERE match_id = "ABCDE"
	ORDER BY inserted_at ASC;
```

Depending on the Match Event schema, the replaying of events could happen either in the application code or in the database query itself.

If attribute changes are saved as a json field on the match event, then a [Merge technique](https://www.newtonsoft.com/json/help/html/MergeJson.htm) could be used in the application code to compose the current Domain object.

Example of a Postgres-only implementation of Event Sourcing: https://github.com/tobyhede/postgresql-event-sourcing

## Decision

I propose we use an Event Sourcing data model for Match Resolution.

Based on the amount of drawbacks with the second approach described above, I propose we use the first approach, where only event records are stored in the database and domain records are composed by aggregating event records.

## Consequences

### Data schema changes

When a match is found during a query, certain data is set on a match that is not intended to change during the resolution process. This includes:

- which state initiated the query
- the original query data of the initiating state*
- the original response payload of this query*
- the hash and hash type of the matched individual
- the matching state
- creation timestamp of the match

*\* This will include state-specific fields like Case ID and Participant ID*

During match resolution, states will repeatedly submit other types of data. This includes:

- whether a state considers a match valid or invalid
- whether the matched individual requires identitity protection
- the final disposition from each state

Data submitted by states has the potential to update the `status` of the match.

Because the schema for initial match data differs significantly from the data involved in resolution, and because initial match data is considered unchangeable, I propose we keep the current `matches` table with some slight modifications:

- Remove the `invalid` boolean field
- Remove the `status` field
- Treat `matches` as append-only, immutable records

With these changes, `matches` essentially become a type of event record, recording the state of the world at the time a match is created.

For data that can change during match resolution, I propose we add a new table called `match_res_events` (short for "Match Resolution Events") that are related to `matches` through the `match_id`. This table would have the following fields:

```sql
id serial PRIMARY KEY,
match_id text NOT NULL, -- references match ID of original match
inserted_at timestamptz NOT NULL, -- since events are immutable, we just need one timestamp
actor text NOT NULL, -- the person or automated system performing this change
actor_state text, -- indicates if the actor is associated with a state involved in the match. If the actor is an automated system or a NAC developer, this field would be null
delta jsonb NOT NULL -- json object representing data changes submitted by states, as well as stateful domain data like a match's "status"
```

Delta objects would be keyed by field name, with values being the data supplied by actors. The delta object can be one or more key/value pairs, depending on how many changes an actor provides in a single action.

Example of a `delta` value:

```json
{
  vulnerable_individual: TRUE,
  invalid: FALSE,
  final_disposition: "cancel benefits at end of current month"
}
```

Example of a complete `match_res_events` object:

```json
{
  id: 123,
  match_id: "ABCDE",
  inserted_at: "2022-04-23T18:25:43.511Z",
  actor: "john.doe@email.example",
  actor_state: "MO",
  delta: {
    vulnerable_individual: TRUE,
    invalid: FALSE,
    final_disposition: "cancel benefits at end of current month"
  }
}
```

Example of a `match_res_events` record that closes a match:

```json
{
  id: 456,
  match_id: "ABCDE",
  inserted_at: "2022-04-24T18:25:43.511Z",
  actor: "NAC System",
  actor_state: null,
  delta: {
    status: "closed"
  }
}
```

These `match_res_events` records would be append-only and also considered immutable.

### Getting a match's "status" and other stateful data

We have options for getting stateful domain data from matches and match resolution events. Different options can be deployed as we gain a better understanding of what metrics data will become important over time.

When getting the details of a single match, data from the original match record and its match resolution events can be merged into an aggregate object representing the domain data. This aggregeate object could be tailored depending on UI needs.

To figure out if a single match is "open" or "closed", collect all events with the same `match_id` and see if any of them have a `{ status: "closed" }` delta. If not, then the match is considered open.

What if we wanted to get all current open matches for a given state, or for the whole system? For this, we could save aggregate data in database views or cache it in some other way, then query against these data stores. This would be considered short-term data storage that could be wiped and replayed at any time.

### Consequences for the program

An Event Sourcing approach will open up a wide range of features and functionality with relatively little effort. Eligibility workers and FNS could easily view entire activity history for a match. They could view activity history for a single state or multiple states. They could also see the effect of automated processes on match resolution, like for the future "Bulk" matching.

There is also opportunity for finer grained metrics data. Not only would FNS be able to track how often matches are being closed, but they could monitor how quickly states are taking specific actions to resolve matches.
