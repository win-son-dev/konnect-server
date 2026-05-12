# GraphQL — Companies

The first GraphQL type backed by real data. Public visitors can look up any company by slug; the company's own recruiter can view their company through a recruiter-scoped subtree. Updates go through the REST surface (see [Update endpoint](#update-endpoint)) — Konnect's transport split is **GraphQL = queries (reads), REST = commands (writes)**, so the GraphQL schema has no `Mutation` root.

## Schema

```graphql
type Query {
  healthcheck: String!
  company(slug: String!): Company
  recruiter: RecruiterQueries! @authorize(roles: ["Recruiter"])
}

type RecruiterQueries {
  company: Company!
}

type Company {
  id: UUID!
  name: String!
  slug: String!
  description: String
  websiteUrl: String
  verified: Boolean!
  createdAt: DateTime!
  updatedAt: DateTime!
}
```

`Company` deliberately does **not** expose `Recruiters` or `JobPostings` — the public schema is for browse/profile, recruiter rosters are never world-readable, and job postings will land on a separate top-level field once issue #25 ships rather than as a nested projection.

## Public — `Query.company(slug)`

Anonymous lookup. Powers the public company profile page.

### Request

```http
POST /graphql
Content-Type: application/json

{
  "query": "{ company(slug: \"acme\") { id name slug description websiteUrl verified } }"
}
```

### Response

```json
{
  "data": {
    "company": {
      "id": "8f9a4d2b-9d3a-4f1c-9c1e-2a8e1f7b1c2d",
      "name": "Acme Corp",
      "slug": "acme",
      "description": "We make anvils.",
      "websiteUrl": "https://acme.test",
      "verified": false
    }
  }
}
```

Returns `null` inside `data.company` if no company has the slug.

## Recruiter-scoped — `Query.recruiter.company`

Requires a recruiter access token. The target company is derived from the recruiter's JWT `external_id` claim — there is no `slug` or `id` argument, so a recruiter cannot read another recruiter's company through this field.

The wrapper field `Query.recruiter` carries `[Authorize(Roles = "Recruiter")]`; every nested field (today only `company`, more once #25 lands) is gated by virtue of being reachable through it.

### Request

```http
POST /graphql
Authorization: Bearer <recruiter access_token>
Content-Type: application/json

{
  "query": "{ recruiter { company { id name slug description websiteUrl } } }"
}
```

### Response

```json
{
  "data": {
    "recruiter": {
      "company": {
        "id": "8f9a4d2b-9d3a-4f1c-9c1e-2a8e1f7b1c2d",
        "name": "Acme Corp",
        "slug": "acme",
        "description": "We make anvils.",
        "websiteUrl": "https://acme.test"
      }
    }
  }
}
```

| Token | Result |
|---|---|
| No token | GraphQL response with `errors[].extensions.code = "AUTH_NOT_AUTHENTICATED"` |
| Seeker token | GraphQL response with `errors[].extensions.code = "AUTH_NOT_AUTHORIZED"` |
| Recruiter token | `data.recruiter.company` populated |

The wrapper subtree returns whatever the recruiter is authorised to see; querying `recruiter` itself does not 401 the HTTP response (HotChocolate keeps GraphQL authorisation failures inside the JSON envelope), it surfaces them as `errors`.

## Update endpoint

Writes are REST. The endpoint pairs with the recruiter-scoped read field above; identity always comes from the JWT, so a recruiter can only ever update their own company.

### `PUT /api/recruiter/company`

```http
PUT /api/recruiter/company
Authorization: Bearer <recruiter access_token>
Content-Type: application/json

{
  "name": "Acme Renamed",
  "description": "We now make jet engines.",
  "websiteUrl": "https://acme-renamed.test"
}
```

| Token | Status |
|---|---|
| No token | `401 Unauthorized` |
| Seeker token | `403 Forbidden` |
| Recruiter token | `200 OK` with the updated company body |

`slug` is intentionally not editable here — slug renames touch caches and inbound links and will land as a separate operation if/when product asks for it. `verified` is set by Konnect, not by recruiters. `created_at` / `updated_at` are owned by Postgres (column default + `set_updated_at()` trigger) and never assigned by the application.

### Response

```json
{
  "id": "8f9a4d2b-9d3a-4f1c-9c1e-2a8e1f7b1c2d",
  "name": "Acme Renamed",
  "slug": "acme",
  "description": "We now make jet engines.",
  "websiteUrl": "https://acme-renamed.test",
  "verified": false,
  "updatedAt": "2026-05-11T08:14:22.391+00:00"
}
```

## How the layers fit

```
GraphQL resolver (CompanyQueries / RecruiterCompanyQueries)
        │
        ▼
ICompanyQueryService     ──── reads
                              ┌─ ICompanyRepository.GetBySlugAsync
                              └─ IUserRepository + ICompanyRepository.GetByIdAsync

REST controller (RecruiterCompanyController)
        │
        ▼
ICompanyCommandService   ──── writes
                              └─ ICompanyRepository.UpdateAsync
                                  (UPDATE; updated_at refreshed by Postgres trigger)
```

The CQRS-lite split (`Konnect.Services/Companies/{Queries,Commands}/`) maps directly onto the transport split: query services back GraphQL resolvers, command services back REST controllers. Repositories stay unified — the data shape is identical on both sides.
