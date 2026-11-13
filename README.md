# Zagejmi

## Struktura řešení

| Projekt | Obsah |
|---|---|
| `src/Zagejmi.Shared` | `Result`/`ErrorDetail` a společné abstrakce bez závislostí |
| `src/Zagejmi.Domain` | Doménový model (uživatelé, profily hráčů a zákazníků, peněženky) |
| `src/Zagejmi.Application` | Use-casy rozdělené po funkcích (`Auth/Users/CreateUser`, `Profiles/SearchPlayers`, …) a rozhraní, která implementuje infrastruktura (`*/Abstractions`) |
| `src/Zagejmi.Infrastructure` | EF Core (PostgreSQL), MinIO, SMTP, Argon2, SkiaSharp. Každá funkce má `Ctx/`, `Records/`, `Repositories/`; migrace jsou v `Migrations/<Context>/` |
| `src/Zagejmi.Api` | ASP.NET Core API (`Controllers/`) a Blazor Server UI (`Components/`) |
| `tests/` | Testy (xUnit v3 na Microsoft.Testing.Platform) |

Závislosti vedou pouze dovnitř: Api → Infrastructure → Application → Domain → Shared.

## Build

Požadavky: .NET SDK podle `global.json`, Node.js (kvůli TailwindCSS), Docker pro PostgreSQL/MinIO.
Doporučené prostředí je JetBrains Rider; spouštěcí konfigurace jsou v `.run/`.

```bash
dotnet tool restore
(cd src/Zagejmi.Api && npm ci && npm run css:build)   # vygeneruje wwwroot/styles.css
docker compose up -d postgresdb minio                 # nebo konfigurace "Docker Infra only"
dotnet build Zagejmi.slnx
dotnet test --solution Zagejmi.slnx
```

Při vývoji UI stačí nechat běžet `npm run css:watch`.

### Migrace

Každý DbContext má vlastní migrace:

```bash
dotnet ef migrations add <Nazev> --context ProfilesContext \
  -p src/Zagejmi.Infrastructure -s src/Zagejmi.Api -o Migrations/Profiles
```

## Konvence

- Styl kódu je popsaný v `.editorconfig` a `stylecop.json` a vynucuje se při buildu (StyleCop + .NET analyzéry).
  Build by měl projít bez varování.
- Verze NuGet balíčků se spravují centrálně v `Directory.Packages.props`.
- Namespace odpovídá cestě ke složce. Nové use-casy patří do `Application/<Funkce>/<UseCase>/` jako dvojice
  `*Command`/`*Query` + `*Handler`.
- Veřejné typy a členy mají XML dokumentaci.

## Využité technologie

ASP.NET Core, Blazor Server, Entity Framework Core + PostgreSQL, MinIO, SkiaSharp, TailwindCSS, xUnit v3.

## SMTP relay (produkce)

1. Vytvořte produkční env soubor z šablony `.env.example` jako `.env` a vyplňte relay údaje.
2. Ujistěte se, že DNS domény `zagejmi.cz` obsahuje SPF/DKIM/DMARC podle zvoleného providera.
3. Spusťte produkční stack přes base + prod compose.

```bash
cp .env.example .env
docker compose -f docker-compose.yaml -f docker-compose.prod.yaml up -d --build
```

Debug stack s Mailpit (databáze, MinIO a API jsou v base compose, debug přidává Mailpit a debug SMTP override):

```bash
docker compose -f docker-compose.yaml -f docker-compose.debug.yaml up -d --build
```
