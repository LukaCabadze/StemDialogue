# API reference

Base path `/api/v1`. JSON uses camelCase; enums are strings (`Published`, `StemSaturday`). Timestamps sent to the API are ISO‑8601 UTC (`2026-11-14T08:00:00Z`). Public event times come back with the Tbilisi offset (`+04:00`).
Errors are `application/problem+json`. Status codes: 400 validation (with `errors.{field}`), 401 not signed in, 403 role, 404 not found, 409 conflict / stale `concurrencyStamp` / invalid state change, 413 upload too large, 429 rate limit.

Bilingual fields are sent as `{ "ka": "…", "en": "…" }`. Georgian is required and English is optional. Public endpoints return the resolved string for `?lang=ka|en`, falling back to Georgian.

## Public (anonymous, cached 60 s, evicted on every CMS change)

`{p}` = project slug, e.g. `school2university`.

| Method | Path | Returns |
| --- | --- | --- |
| GET | `/site` | Default project (for the `/` → `/projects/…` redirect) and project list |
| GET | `/projects/{p}` | Layout data: name, logo, **funding** (short/full text, funders, partners), contact, social links, navigation, default SEO |
| GET | `/projects/{p}/home` | Hero, about teaser + milestones, featured item, upcoming events, latest news, archive highlights, people entries, ambassadors teaser, funding |
| GET | `/projects/{p}/about` | About page, milestones (with state), funding block |
| GET | `/projects/{p}/contact` | Contact page, contacts, social links, partners |
| GET | `/projects/{p}/pages/{key}` | Intro + SEO for `news`, `stem-saturdays`, `team`, `researchers`, `mentors`, `resources`, … |
| GET | `/projects/{p}/milestones` | Timeline |
| GET | `/projects/{p}/news?category=&tag=&year=&q=&page=&pageSize=` | Paged news cards |
| GET | `/projects/{p}/news/categories` | Categories in use with counts |
| GET | `/projects/{p}/news/{slug}` | Article (301 if the slug was renamed) |
| GET | `/projects/{p}/stem-saturdays?scope=Upcoming\|Past\|All&year=&month=&discipline=&tag=&format=&type=&q=` | Paged event cards |
| GET | `/projects/{p}/stem-saturdays/filters` | Years, disciplines, tags, formats, types |
| GET | `/projects/{p}/stem-saturdays/{slug}` | Event: announcement, status note, participants, and the archive (recap, recording + embed URL, gallery, resources, related news) |
| GET | `/projects/{p}/team` | Four sections with current and former members |
| GET | `/projects/{p}/researchers?q=&discipline=&includeFormer=` | Paged profiles |
| GET | `/projects/{p}/mentors?…` | Paged profiles |
| GET | `/projects/{p}/people/{slug}` | Profile with roles, links, upcoming/past events, resources |
| GET | `/projects/{p}/resources?type=&event=&discipline=&tag=&q=` | Paged resources |
| GET | `/projects/{p}/resources/{slug}` | Resource with related items |
| GET | `/projects/{p}/stem-ambassadors` | Program page + latest school activities |
| GET | `/projects/{p}/stem-ambassadors/activities` / `…/activities/{slug}` | Activity archive / detail |
| GET | `/sitemap.xml`, `/api/v1/sitemap` | All public URLs (XML / JSON) |
| GET | `/health` | Liveness + database check |

Media files are served from `/media/{yyyy}/{MM}/{id}.{ext}` with immutable caching.

## Authentication

| Method | Path | Notes |
| --- | --- | --- |
| POST | `/auth/login` | `{ email, password }` → `{ accessToken, expiresAtUtc, user }` + `sd_refresh` cookie (HttpOnly, Secure, SameSite=Strict, path `/api/v1/auth`) |
| POST | `/auth/refresh` | Uses the cookie, rotates it, returns a new access token. Call it before the token expires or on a 401 |
| POST | `/auth/logout` | Revokes the session and invalidates issued access tokens |
| GET | `/auth/me` | Current account |
| POST | `/auth/change-password` | `{ currentPassword, newPassword }`. All sessions end |

Send `Authorization: Bearer <accessToken>`. Keep the access token in memory. Angular must call `/auth/*` with `withCredentials: true` when the API is on another origin.

## CMS (Admin or Editor unless marked **Admin**)

Project-scoped: `/admin/projects/{p}/…`

| Resource | Endpoints |
| --- | --- |
| Project settings | `GET /admin/projects`, `GET /admin/projects/{p}`, **Admin** `PUT /admin/projects/{p}` (names, funding wording, partners with roles, contacts, social links, SEO) |
| Pages | `GET …/pages`, `GET/PUT …/pages/{key}` |
| Milestones | `GET/POST …/milestones`, `PUT/DELETE …/milestones/{id}`, `PUT …/milestones/order` |
| News | `GET/POST …/news`, `GET/PUT/DELETE …/news/{id}`, `POST …/news/{id}/publish` (`{ publishAtUtc? }`), `/unpublish`, `/archive` |
| Events | `GET/POST …/events`, `GET/PUT/DELETE …/events/{id}`, `POST …/events/{id}/publish`, `/unpublish`, `/complete` (`{ recap?, recordingUrl? }`), `/cancel` (`{ note }`), `/postpone` (`{ newStartsAtUtc, newEndsAtUtc?, note }`), `/restore` |
| Resources | `GET/POST …/resources`, `GET/PUT/DELETE …/resources/{id}` |
| School activities | `GET/POST …/school-activities`, `GET/PUT/DELETE …/school-activities/{id}` |
| Dashboard / lookups | `GET …/dashboard`, `GET …/lookups?lang=` |

Global:

| Resource | Endpoints |
| --- | --- |
| People | `GET/POST /admin/people`, `GET/PUT/DELETE /admin/people/{id}`, `PUT /admin/people/{id}/roles` |
| Organizations | `GET /admin/organizations`, `GET /admin/organizations/{id}`, **Admin** `POST/PUT/DELETE` |
| Taxonomy | `GET/POST /admin/tags`, `PUT/DELETE /admin/tags/{id}`, same for `/admin/disciplines` |
| Media | `GET /admin/media?kind=&search=`, `POST /admin/media` (multipart: `file`, `altKa`, `altEn`, `captionKa`, …), `GET/PUT/DELETE /admin/media/{id}`, `GET /admin/media/{id}/usages` |
| Users (**Admin**) | `GET/POST /admin/users`, `PUT /admin/users/{id}`, `POST /admin/users/{id}/reset-password`, `POST /admin/users/{id}/unlock` |
| Audit (**Admin**) | `GET /admin/audit?entityType=&entityId=&userId=` |

### Editing rules

- Every update must send the `concurrencyStamp` from the last read (`audit.concurrencyStamp`). A stale stamp returns **409**, so two editors never silently overwrite each other.
- Rich text (`body`, `description`, `recap`, `biography`) is HTML and is sanitized on the server. Scripts, styles, iframes and event handlers are removed.
- Slugs are generated from the Georgian title (`STEM შაბათობა: ფიზიკა` → `stem-shabatoba-pizika`) or set explicitly. Renaming a published item's slug keeps the old URL as a 301 redirect.
- Published news, events and profiles cannot be deleted; archive, unpublish, cancel or end the role instead.
- Images used publicly need Georgian alt text. The dashboard counts images without it.
