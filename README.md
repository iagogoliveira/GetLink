# URL Shortener

A URL shortener with authentication and click analytics, built with ASP.NET Core (.NET 10), EF Core, SQL Server and Angular 22.

The project is split into three independently deployable pieces:

```
encurtadorUrl/
├── AuthService/      :7001   user registration, login, JWT issuing
├── urlShortener/     :7000   short URLs, redirect, click analytics
├── frontend/         :4200   Angular SPA
├── docker-compose.yml        orchestrates the services + SQL Server
└── .env                      local secrets (git-ignored)
```

Each service has its own solution (`AuthService.sln`, `urlShortener.sln`) and its own test project. They share no project references — only the JWT contract.

## Features

**Accounts**
- Registration with unique login and e-mail (enforced by a DB index, not just a check)
- Passwords hashed with PBKDF2-HMAC-SHA256 at 600,000 iterations, per-password salt, constant-time comparison
- The stored hash carries its own iteration count, so the cost can be raised later without invalidating existing passwords
- Login responses take the same time whether or not the account exists, so the endpoint can't be used to enumerate users
- Rate limited to 5 attempts per minute per IP
- Inactive accounts (`Active = false`) cannot log in

**Short URLs**
- Create, update (including a custom path), delete, and public redirect
- URLs without a scheme are normalized on write (`exemplo.com` → `https://exemplo.com`), so the redirect always leaves the shortener's domain
- Every write verifies ownership against the JWT — another user's URL responds `404`, identical to a URL that doesn't exist, so IDs can't be enumerated

**Click analytics**
- Every redirect is recorded with a timestamp
- Clicks per day, and breakdowns by browser, device, operating system and traffic source
- Analytics failures never break the redirect

### A note on what is stored

Click tracking stores **derived metadata only**. The IP and user-agent are read from the request, used to determine device type, browser and OS, and then discarded — they are never persisted. The referrer is reduced to its host (`www.google.com`), never the full URL, since origin paths often carry search terms and identifiers.

The intent is to keep the analytics useful without putting personal data (LGPD/GDPR) in the database. A row looks like this:

```
ClickedAt                   | RefererHost    | DeviceType | Browser | OperatingSystem
2026-08-01 08:28:21.0004952 | www.google.com | Desktop    | Firefox | Windows
```

Browser/OS detection is heuristic by nature — user-agent strings are deliberately misleading for historical compatibility reasons. Treat it as a trend, not an exact count.

## Requirements

- .NET 10 SDK
- Node.js 20+ and npm (for the frontend)
- SQL Server — a local instance, or the containerized one from `docker-compose.yml`
- `dotnet-ef` for migrations:
  ```
  dotnet tool install --global dotnet-ef
  ```

## Running locally

### 1. Set the JWT secret

The secret is **not** in `appsettings.json`. Both services must sign and validate with the **same** value — the signature is symmetric. Generate one and store it via user-secrets:

```bash
# any random string with 32+ characters
openssl rand -base64 48

cd AuthService  && dotnet user-secrets set "JwtSettings:SecretKey" "<your-secret>"
cd urlShortener && dotnet user-secrets set "JwtSettings:SecretKey" "<same-secret>"
```

Both services refuse to start if the secret is missing or shorter than 32 characters — a loud failure at startup instead of confusing signature errors later.

### 2. Create the databases

The default connection strings point at `localhost` using Windows authentication, one database per service:

```bash
cd AuthService  && dotnet ef database update
cd urlShortener && dotnet ef database update
```

If you're using the SQL Server container instead of a local instance, override the connection string first — `Trusted_Connection` is Windows authentication and won't work against it:

```bash
docker compose up -d sqlserver
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=AuthServiceDb;User Id=sa;Password=<sa-password>;TrustServerCertificate=True;"
```

### 3. Start the services

Use the `http` profile — it avoids the self-signed certificate warning in the browser:

```bash
cd AuthService  && dotnet run --launch-profile http    # :7001
cd urlShortener && dotnet run --launch-profile http    # :7000
```

Swagger is available in development at `/swagger` on both.

### 4. Start the frontend

```bash
cd frontend
npm install
npm start          # :4200
```

Open http://localhost:4200, create an account, and shorten a URL.

> **Heads-up:** `UrlShortener:BaseUrl` defaults to `https://localhost:7000`, so generated links use `https`. If you run the services with the `http` profile, clicking a link from the UI will fail on the certificate. For local testing, set `UrlShortener:BaseUrl` to `http://localhost:7000` in `appsettings.Development.json`.

### CORS

Both services only accept the origins listed under `Cors:AllowedOrigins`. Development already allows `http://localhost:4200`. The production `appsettings.json` ships with an **empty list**, which allows nothing — fill it in before deploying.

## Running with Docker

`docker-compose.yml` reads secrets from a `.env` file at the repository root (git-ignored):

```env
MSSQL_SA_PASSWORD=<strong password>
JWT_SECRET=<32+ characters, same for both services>
```

Then:

```bash
docker compose up --build -d
```

The compose file overrides each service's connection string via environment variables. Migrations are **not** applied automatically — run `dotnet ef database update` against the container, or add `Database.Migrate()` at startup.

## Running the tests

```bash
cd AuthService  && dotnet test     # password hashing, login, timing, duplicates
cd urlShortener && dotnet test     # ownership, URL normalization, click metadata
cd frontend     && npm test        # watch mode; add -- --watch=false for a single run
```

## API

### AuthService — `:7001`

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/Auth/CreateUser` | — | `{ name, login, password, email }` |
| POST | `/Auth/Login` | — | `{ login, password }` → `{ token }`. Rate limited. |

### urlShortener — `:7000`

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/CreateNewUrl` | JWT | `{ originalUrl }` → `{ newUrl }` |
| PUT | `/UpdateUrl` | JWT | `{ id, originalUrl, newPath? }` — empty `newPath` keeps the current one |
| DELETE | `/DeleteUrl` | JWT | `{ id }` |
| GET | `/api/urls` | JWT | The caller's URLs with click totals |
| GET | `/api/urls/{id}/stats` | JWT | Full click breakdown for one URL |
| GET | `/{code}` | — | Redirects (302) and records the click |

Management endpoints live under `/api/` deliberately: the root is the short-code namespace, and any literal route added there would burn that code forever.

Protected routes require:

```
Authorization: Bearer <JWT>
```

The user ID is always taken from the token's `NameIdentifier` claim, never from the request body.

### Response codes worth knowing

| Code | When |
|---|---|
| `401` | Missing, invalid or expired token |
| `404` | URL not found **or** owned by someone else — deliberately indistinguishable |
| `429` | Login rate limit exceeded |

## Production checklist

- [ ] Provide `JwtSettings:SecretKey` via environment variable or a secrets manager (never `appsettings.json`)
- [ ] Fill in `Cors:AllowedOrigins` — the default empty list blocks every browser origin
- [ ] Use a dedicated low-privilege database user instead of `sa`
- [ ] Set `UrlShortener:BaseUrl` to the real public domain
- [ ] Enable HTTPS with a valid certificate
- [ ] Define a retention policy for the `Clicks` table

## Roadmap

- Retention/aggregation policy for old click rows
- Country breakdown (requires a GeoIP database)
- RS256 instead of HMAC, so the shortener only needs a public key and the two services stop sharing a secret
- Automatic migration on startup
- Role/claims-based authorization