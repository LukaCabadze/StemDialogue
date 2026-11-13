# STEM Dialogue — public website (Angular 22)

Public site of **სამეცნიერო დიალოგი — სკოლიდან უნივერსიტეტამდე** (SPG-26-185), built from the
Figma "Website Design · TZ v1.0" and backed by the ASP.NET Core API in `../backend`.

- Angular 22, standalone components, signals, zoneless change detection, `@if/@for` control flow
- One component = one folder with its own `.ts / .html / .scss`
- Data loading with `rxResource` (reloads automatically on project or language change)
- Georgian (default) and English, switched in the header; the API is called with `?lang=`
- No UI library and no Tailwind: design tokens are CSS custom properties (`src/styles/_tokens.scss`)

## Run locally

Requirements: Node.js 22.22+ or 24.15+ (Angular 22), npm 10+.

```bash
# 1. API (from ../backend) — listens on http://localhost:5080
dotnet run --project src/StemDialogue.Api

# 2. Website — http://localhost:4200
npm install
npm start
```

`ng serve` proxies `/api`, `/media` and `/sitemap.xml` to `http://localhost:5080` (`proxy.conf.json`),
so no CORS setup is needed in development. Opening `/` redirects to `/projects/school2university`.

| Command            | What it does                                   |
| ------------------ | ---------------------------------------------- |
| `npm start`        | Dev server with API proxy                      |
| `npm run build`    | Production build → `dist/stemdialogue-web/browser` |
| `npm test`         | Unit tests (Vitest)                            |

## Deploying

Serve `dist/stemdialogue-web/browser` as static files with an SPA fallback to `index.html`, and route
`/api`, `/media` and `/sitemap.xml` to the API on the same origin. The API base URL and the default
project live in `src/environments/environment.ts`.

## Structure

```
src/
├─ styles/                 design tokens, reset, typography, buttons, prose (global)
├─ environments/           API base URL, default project, site origin
└─ app/
   ├─ app.ts / app.config.ts / app.routes.ts
   ├─ core/                no UI — services and models only
   │  ├─ api/              one class per API area (site, home, pages, news, events, people, resources, ambassadors)
   │  ├─ models/           TypeScript contracts mirroring the API DTOs, one file per domain
   │  ├─ http/             language interceptor, API error classification
   │  ├─ i18n/             language service, ka/en interface copy, Tbilisi-time date formatting
   │  ├─ paging/           "show more" pagination on top of rxResource
   │  ├─ project/          current project slug + route guard
   │  ├─ seo/              title / description / Open Graph / canonical
   │  ├─ site/             project layout data shared by header and footer
   │  └─ util/             small pure helpers (initials, cover colours, links, file size…)
   ├─ layout/              public shell, header, main nav, language switch, funding strip, footer
   ├─ shared/
   │  ├─ pipes/            sdDate, initials, fileSize
   │  └─ ui/               reusable presentational components (cards, pills, avatars, states…)
   └─ features/            one folder per page area; each page and section is its own component
      ├─ home/             hero, about, featured, upcoming, news, archive, people, ambassadors, partners
      ├─ events/           STEM Saturdays list + one detail page for the whole event lifecycle
      ├─ news/             list, article, sidebar
      ├─ people/           team page, researcher / mentor directories, person profile
      ├─ resources/        library and resource detail
      ├─ ambassadors/      programme page and school initiatives
      ├─ about/  contact/  not-found/
```

Rules the code follows:

- `core` never imports from `layout`, `shared` or `features`; `shared/ui` never imports from `features`;
  features never import from each other.
- Pages (`*-page`) load data and handle loading / error / not-found states; section and card
  components only receive inputs.
- Every URL in the app is the `path` the API returns, so links always match the backend's routing.

## Content that is not in the API

Interface wording (navigation fallbacks, section overlines, the three STEM Ambassador steps, empty
states) lives in `core/i18n/ui-text.ka.ts` and `ui-text.en.ts`. Everything editors manage — titles,
leads, bodies, people, events, partner logos — comes from the CMS. Until partner logos are uploaded,
bundled logos in `public/brand/partners` are used for the funder and the two universities.
