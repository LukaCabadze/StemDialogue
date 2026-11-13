# STEM Dialogue — School2University

Website and CMS API for **სამეცნიერო დიალოგი — სკოლიდან უნივერსიტეტამდე** (SPG-26-185).

| Folder      | What it is                                                                 |
| ----------- | -------------------------------------------------------------------------- |
| `backend/`  | ASP.NET Core 8 API (Clean Architecture, EF Core, Identity + JWT) — see `backend/README.md` |
| `frontend/` | Angular 22 public website built from the Figma design — see `frontend/README.md` |
| `tools/`    | Small Windows helper scripts (environment check, Node.js install, package setup) |

## Run locally (Windows)

Requirements: .NET SDK 8+ and Node.js 22.22+ / 24.15+ / 26+.

Double-click **`start-all.cmd`** — it opens two windows:

- API → http://localhost:5080 (Swagger at `/swagger`, local SQLite database created automatically)
- Website → http://localhost:4200 (proxies `/api` and `/media` to the API)

Or start them separately with `start-backend.cmd` and `start-frontend.cmd`.

Manual commands:

```bash
cd backend  && dotnet run --project src/StemDialogue.Api
cd frontend && npm install && npm start
```
