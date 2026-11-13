# Deployment notes

## Reverse proxy (Caddy)

The Angular site and the API share one origin. Caddy serves the SPA and proxies the API, media and sitemap.

```caddy
{$DOMAIN:stemdialogue.com} {
    encode zstd gzip

    # Spec §3: the bare domain temporarily redirects to the project (302 now, platform home later).
    @root path /
    redir @root /projects/school2university 302

    handle /api/*       { reverse_proxy api:8080 }
    handle /media/*     { reverse_proxy api:8080 }
    handle /sitemap.xml { reverse_proxy api:8080 }
    handle /health      { reverse_proxy api:8080 }

    # Deep links and refresh never return 404 (spec §3, §15.19).
    handle {
        root * /srv
        try_files {path} /index.html
        file_server
    }
}
```

Add `img-src 'self' data: https://i.ytimg.com` and `frame-src https://www.youtube-nocookie.com https://player.vimeo.com` to the site's Content-Security-Policy so recordings can be embedded.

## Production checklist (spec §11)

- **HTTPS** at the proxy. The refresh cookie is `Secure`.
- **Secrets** come only from environment variables: `Jwt__Key`, `ConnectionStrings__Default` and `Bootstrap__*`. Remove `Bootstrap__Password` after the first start.
- **Migrations** run as a separate job: `dotnet StemDialogue.Api.dll --migrate`. The serving process never migrates.
- **Database user**: give the API a login with data read/write only. Run the migration job with a privileged login.
- **Backups** cover two things: the SQL database (daily, keep copies off the server) and the `Storage__RootPath` volume (uploaded media). Restore tests should cover both.
- **Logging** goes to stdout (Docker collects it). Unhandled errors are logged with the request path and a `traceId` that also appears in the error response.
- **Access**: create named Editor accounts, and keep only one or two Admins. Deactivating a user ends their sessions immediately.
- **Angular SSR/prerender** can read the route list from `/api/v1/sitemap`. Use `seo.*` from each detail response for `<title>`, meta description and Open Graph tags.
