# Authorization

Authorization is **policy-driven**, not role-attribute-driven. Every protected endpoint declares one of a small set of named policies, and the policy encodes both the JWT `aud` claim check and the role check in one gate.

This page documents the policies that exist today, how a request flows through them, and how to add a new endpoint to the right policy. The audience-split itself (why the platform has two `aud` values in the first place) lives on the [Authentication — Auth0](Authentication-Auth0) page.

## Why policies, not roles

A `[Authorize(Roles = "Recruiter")]` attribute only checks the role claim. It does not verify which API the token was minted for, so a token issued for the seeker side that somehow carries the `Recruiter` role would still pass the gate. Konnect's authorization model treats the JWT audience as a first-class security boundary — every policy enforces audience + role together. The two checks are independent: a token with the right audience but the wrong role fails, and vice versa.

Beyond audience boundaries, policies are also where the future per-resource checks land — `OwnsPostingRequirement` (verify the recruiter editing a job actually owns it) cannot be expressed as a role attribute because it needs to read the database. The handler pattern scales; the attribute pattern does not.

## Policy table

| Policy name | Requires | Used by |
|---|---|---|
| `SeekerAudience` | JWT `aud == Auth0:SeekerAudience` AND role claim `JobSeeker` | `POST /api/seeker/onboard` |
| `RecruiterAudience` | JWT `aud == Auth0:RecruiterAudience` AND role claim `Recruiter` | `POST /api/recruiter/onboard`, `PUT /api/recruiter/company`, GraphQL `Query.recruiter` subtree |

Both policies also implicitly require `RequireAuthenticatedUser()` — an anonymous request gets 401 before the audience handler runs.

The policy names live as constants in [`Konnect.Infrastructure/Services/Authentication/AuthorizationPolicyNames.cs`](https://github.com/win-son-dev/konnect-server/blob/main/Konnect.Platform/Konnect.Infrastructure/Services/Authentication/AuthorizationPolicyNames.cs) so both `Konnect.WebAPI` and `Konnect.GraphQL` reference the same strings.

## How a request flows through the gate

```
Request with bearer token
        ↓
1. JwtBearer middleware
   - Validates signature, issuer, audience, expiry
   - Hydrates ClaimsPrincipal from the JWT (with MapInboundClaims=false,
     so claim names are kept as raw JWT shortnames like "aud")
        ↓
2. KonnectAuthenticationMiddleware
   - Anonymous: passes through
   - Authenticated: rejects (401) if the namespaced external_id or role
     claim is missing/malformed
        ↓
3. [Authorize(Policy = "...")] on the controller / GraphQL field
   - Resolves the named policy
   - AudienceRequirementHandler reads IOptions<Auth0Settings> and verifies
     the principal's "aud" claim AND IsInRole(...)
   - All checks pass → endpoint runs
   - Any check fails → 403 (REST) or GraphQL "errors" array (GraphQL)
```

The 401 vs 403 split is intentional: 401 means "we cannot tell who you are", 403 means "we know who you are and this is not for you." A seeker token sent to a recruiter endpoint passes step 1 (the token is valid), passes step 2 (claims are well-formed), and only fails at step 3 — that's 403, not 401.

## Adding a new endpoint

1. **Decide which policy fits.** Recruiter-only endpoint → `RecruiterAudience`. Seeker-only endpoint → `SeekerAudience`. A public-by-default endpoint that just needs an authenticated caller (today only `/api/me`) uses bare `[Authorize]`.
2. **Annotate the controller / GraphQL field.** REST controllers use `[Authorize(Policy = AuthorizationPolicyNames.RecruiterAudience)]`; HotChocolate resolvers use the same `Policy = ...` form on `HotChocolate.Authorization.AuthorizeAttribute`.
3. **Cover the audience boundary in the integration sweep.** [`Konnect.Tests/WebAPI/Authorization/AuthorizationPolicySweepTests.cs`](https://github.com/win-son-dev/konnect-server/blob/main/Konnect.Platform/Konnect.Tests/WebAPI/Authorization/AuthorizationPolicySweepTests.cs) drives a matrix of no-token / wrong-audience / right-audience-wrong-role per endpoint. Adding a new endpoint to the table extends the sweep automatically — there's no per-endpoint copy/paste of audience-boundary tests.

## What is intentionally not here

Two policies that the original spec listed are deferred until their consumers exist:

- **`CompanyAdminPolicy`** — Phase 1 is one recruiter per company, so the admin-vs-member distinction gates nothing. Lands when team management ships.
- **`OwnsPostingRequirement`** — verifies the recruiter editing a posting owns its company. Lands together with the JobPosting REST endpoints (issue #25), where it has its first caller.

Both are tracked in the Phase 1 Story (#20). The policy registration in [`AuthorizationServiceCollectionExtensions`](https://github.com/win-son-dev/konnect-server/blob/main/Konnect.Platform/Konnect.WebAPI/Extensions/AuthorizationServiceCollectionExtensions.cs) is the single place they get added when those issues land.
