# STEM Dialogue — Backend (School2University CMS API)

ASP.NET Core 8 Web API for the public site of **„სამეცნიერო დიალოგი — სკოლიდან უნივერსიტეტამდე“** (SPG-26-185) and its content management.
It implements the technical specification of 4 November 2026: news, STEM Saturdays (announcement → archive at the same URL), team / researchers / mentors with multi-role profiles, STEM ambassadors, resources, funder & partner identification, editable pages, SEO data and a secured admin API.

The stack matches the existing repository (.NET 8, EF Core 8, SQL Server, ASP.NET Core Identity + JWT), reorganized as a clean, layered solution.

```
backend/
├─ src/
│  ├─ StemDialogue.Domain          Entities and rules, no framework dependencies
│  ├─ StemDialogue.Application     DTOs, validators, public read services, CMS services
│  ├─ StemDialogue.Infrastructure  EF Core, Identity/JWT, file storage, HTML sanitizer, seeding
│  └─ StemDialogue.Api             Controllers, auth, caching, error handling, media serving
├─ tests/StemDialogue.Tests        Integration tests (in-memory API + SQLite) and unit tests
├─ docs/                           API reference, Georgian editor guide, deployment notes
├─ Dockerfile, docker-compose.yml
└─ Directory.Packages.props        Central NuGet versions
```

## Run locally (no SQL Server needed)

Prerequisite: .NET SDK 8 or newer.

```powershell
cd backend
dotnet run --project src/StemDialogue.Api
```

The Development profile uses SQLite (`src/StemDialogue.Api/App_Data/stemdialogue-dev.db`), creates the schema, and seeds the project, base pages, milestones, disciplines, the official core team and clearly marked `[დემო]` sample content.

- Swagger: http://localhost:5080/swagger
- Public API: http://localhost:5080/api/v1/projects/school2university
- Admin login (local only): `admin@stemdialogue.test` / `Local-demo-only-42`

To start over, delete `src/StemDialogue.Api/App_Data/`.

## Run with SQL Server

```powershell
$env:ConnectionStrings__Default = "Server=localhost;Database=StemDialogue;Integrated Security=true;TrustServerCertificate=true"
$env:Database__Provider = "SqlServer"
$env:Jwt__Key = "<random secret, at least 32 bytes>"
$env:Bootstrap__Email = "you@example.org"
$env:Bootstrap__Password = "<initial admin password>"

dotnet run --project src/StemDialogue.Api -- --migrate   # migrations + roles + admin + project content, then exits
dotnet run --project src/StemDialogue.Api
```

Docker: copy `.env.example` to `.env`, fill it in, run `docker compose up --build`. The `migrate` job runs before the API starts.

### Migrations

```powershell
dotnet tool restore
dotnet ef migrations add <Name> --project src/StemDialogue.Infrastructure --startup-project src/StemDialogue.Api --output-dir Persistence/Migrations
```

The design-time factory needs no running database or JWT key.

## Configuration

| Key | Default | Purpose |
| --- | --- | --- |
| `Database__Provider` | `SqlServer` | `SqlServer` (migrations) or `Sqlite` (dev/tests, schema created on start) |
| `ConnectionStrings__Default` | — | Database connection |
| `Jwt__Key` | — | HMAC signing secret, ≥ 32 bytes. The API refuses to start without it |
| `Jwt__AccessTokenMinutes` / `Jwt__RefreshTokenDays` | 15 / 14 | Session lifetimes |
| `Bootstrap__Email` / `Bootstrap__Password` / `Bootstrap__FullName` | — | First administrator, created only when no account exists |
| `Storage__RootPath` | `App_Data/media` | Upload folder. Mount it as a persistent volume and include it in backups |
| `Storage__PublicBasePath` | `/media` | URL prefix of uploads (a CDN URL also works) |
| `Media__MaxImageBytes` / `Media__MaxDocumentBytes` | 10 MB / 50 MB | Upload limits |
| `PublicSite__BaseUrl` | `https://stemdialogue.com` | Absolute URLs in `/sitemap.xml` |
| `Cors__AllowedOrigins` | `[]` | Only needed when the Angular app is served from another origin |
| `RateLimiting__AuthPerMinute` / `RateLimiting__RequestsPerMinute` | 10 / 600 | Per-IP limits |

## Tests

```powershell
dotnet test
```

On Windows you can also double-click:

- `scripts\verify.cmd`: restore, build, create the first migration if it is missing, run all tests. Output goes to `verify.log`.
- `scripts\migrate-local-sqlserver.cmd [server]`: apply migrations and base content to a local SQL Server `StemDialogue` database using Windows authentication. Output goes to `migrate.log`.

The integration tests boot the real API in memory against SQLite, seeded like production, and cover the acceptance criteria. They check news creation and publishing, scheduled publishing, slug redirects, the event lifecycle with its archive, one person holding several roles, the team-section split, the funder/partner split, upload content checks, auth cookie rotation, logout and role enforcement.

## How it maps to the specification

| Spec | Implementation |
| --- | --- |
| §2 Funding & partners on every page | `GET /projects/{p}` → `funding.shortText` (header strip), `funding.fullText` (footer), `funders[]` and `partners[]` as **separate** lists with official logos (light/dark variants) and links. Wording is editable by Admins |
| §3 Routes, permanent URLs | All content is scoped to `/projects/school2university`. Slugs of published items are protected: renaming leaves a redirect (HTTP 301). Published items cannot be deleted, only archived. `GET /api/v1/site` returns the default project for the root redirect |
| §4 Home blocks, no empty sections | `GET /projects/{p}/home` returns every block. Empty blocks are `null` or `[]` |
| §5 News | Categories, tags, cover and gallery, featured flag, related event, Draft/Published/Archived, scheduled publishing, filters and paging |
| §6 STEM Saturdays | Planned → Postponed / Cancelled / Completed through explicit endpoints. Recap, recording (privacy-friendly embed URL), photos and resources are added at the same URL. Times in UTC plus Tbilisi offset. Filters by year, month, discipline, tag, format |
| §7 Team, researchers, mentors | One `Person` with several `PersonRoleAssignment`s per project, each with dates. Ended roles stay in the archive as "former". Team page has 4 sections with `isOfficialGrantStaff`. Profiles list related events |
| §8 STEM ambassadors | Program page plus school activities. Photos can be published only after consent is confirmed. No student profiles |
| §9 Resources | Uploaded files or stable external links. Video only by link |
| §10–11 CMS & security | Admin/Editor roles checked on the server. JWT access tokens plus rotating HttpOnly refresh cookie, with reuse detection. Lockout and rate limits. Upload type checked by content signature, size limits, SVG script blocking. HTML sanitized with an allow-list. Optimistic concurrency (`concurrencyStamp`). Audit log of every change |
| §11 SEO / SSR | Every detail DTO carries `seo { title, description, imageUrl, canonicalPath }`. `/sitemap.xml` and `/api/v1/sitemap` list every public route for prerendering |
| §15.21 Instructions | [docs/admin-guide.ka.md](docs/admin-guide.ka.md) |

The **interview scheduling and scientist outreach modules** of the current repository are not part of this specification and are not included here. They can be added later as separate feature folders (`Domain/Scheduling`, `Application/Scheduling`, `/api/v1/admin/scheduling/...`) without touching the CMS.

See [docs/api.md](docs/api.md) for every endpoint and [docs/deployment.md](docs/deployment.md) for the reverse-proxy rules (root redirect, deep links, `/media`).
