# URL Shortener Solution

A simple URL shortener built with ASP.NET Core (.NET 10), EF Core, and SQL Server. The solution has two services:

- **AuthService** – handles user registration, login, and JWT token generation.
- **UrlShortener** – creates, updates, deletes, and redirects short URLs. Write operations require a valid JWT; redirection is public.

## Requirements

- .NET 10 SDK
- Docker & Docker Compose (for containerized deployment)
- SQL Server (local, remote, or containerized)
- (Optional) `dotnet-ef` for running migrations locally:
  ```
  dotnet tool install --global dotnet-ef
  ```

## Configuration

Both services read their settings from `appsettings.json`:

- **JwtSettings** – `SecretKey`, `Issuer`, `Audience`. AuthService and UrlShortener must use the **same** values so tokens issued by AuthService are accepted by UrlShortener.
- **ConnectionStrings:DefaultConnection** – points to your SQL Server instance.

⚠️ Change the JWT secret key and the SQL `sa` password before deploying to production. Don't commit real secrets — use environment variables or a secrets manager instead.

## Running Locally

1. Restore and build from the solution root:
   ```
   dotnet restore
   dotnet build
   ```
2. Set your connection string in `appsettings.json` (or via environment variables), then apply migrations for each project:
   ```
   dotnet ef database update --project AuthService --startup-project AuthService
   dotnet ef database update --project UrlShortener --startup-project UrlShortener
   ```
3. Run each service:
   ```
   dotnet run --project AuthService
   dotnet run --project UrlShortener
   ```
4. Swagger UI (development only):
   - AuthService: `https://localhost:7001/swagger`
   - UrlShortener: `https://localhost:7000/swagger`

## Running with Docker

Each project includes a Dockerfile. Example docker-compose setup running SQL Server plus both APIs:

```yaml
version: '3.8'
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrongPassword123"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"

  authservice:
    build: ./AuthService
    depends_on:
      - db
    environment:
      ConnectionStrings__DefaultConnection: "Server=db;Database=AuthDb;User Id=sa;Password=YourStrongPassword123;TrustServerCertificate=True;"
      ASPNETCORE_URLS: "http://+:8080"
    ports:
      - "8081:8080"

  urlshortener:
    build: ./UrlShortener
    depends_on:
      - db
    environment:
      ConnectionStrings__DefaultConnection: "Server=db;Database=UrlShortenerDb;User Id=sa;Password=YourStrongPassword123;TrustServerCertificate=True;"
      UrlShortener__BaseUrl: "http://localhost:8080"
      ASPNETCORE_URLS: "http://+:8080"
    ports:
      - "8080:8080"
```

Start everything with:
```
docker compose up --build -d
```

**Note:** Apply EF migrations before running in production — either run `dotnet ef database update` locally against the container's SQL Server, or add `Database.Migrate()` at startup.

## API Endpoints

### AuthService

| Method | Route | Description |
|---|---|---|
| POST | `/Auth/CreateUser` | Create a user. Body: `{ Name, Login, Password, Email }` |
| POST | `/Auth/Login` | Log in and receive a JWT. Body: `{ Login, Password }` |

### UrlShortener

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/CreateNewUrl` | Required | Body: `{ OriginalUrl }` → returns the shortened URL |
| PUT | `/UpdateUrl` | Required | Body: `{ Id, OriginalUrl, NewPath? }` |
| DELETE | `/DeleteUrl` | Required | Body: `{ Id }` |
| GET | `/{code}` | Public | Redirects to the original URL |

Protected routes require the header:
```
Authorization: Bearer <JWT>
```

The JWT must include the user's ID as the `NameIdentifier` claim.

## Example Requests

**Create a user:**
```
POST /Auth/CreateUser
{
  "name": "User Name",
  "login": "user1",
  "password": "secret",
  "email": "user@example.com"
}
```

**Log in:**
```
POST /Auth/Login
{
  "login": "user1",
  "password": "secret"
}
```
Returns the JWT in the response body.

**Shorten a URL (with token):**
```
POST /CreateNewUrl
Authorization: Bearer <token>
{
  "OriginalUrl": "https://example.com/some/long/path"
}
```

## Production Checklist

- [ ] Replace the JWT secret key with a strong, securely stored value
- [ ] Replace the SQL `sa` password / use a dedicated low-privilege DB user
- [ ] Store secrets in environment variables or a secrets manager, not in `appsettings.json`
- [ ] Enable HTTPS with a valid certificate

## Roadmap

- Automatic DB migration on startup
- Role/claims-based authorization
