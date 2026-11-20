# Zagejmi

## Produktové plánování

Průběžné produktové požadavky, rozhodnutí a otevřené otázky jsou vedené v [Product Discovery](docs/product/README.md) a hlavním [Product Blueprintu](docs/product/PRODUCT_BLUEPRINT.md).

## Struktura řešení

| Projekt | Obsah |
|---|---|
| `src/Zagejmi.Shared` | `Result`/`ErrorDetail` a společné abstrakce bez závislostí |
| `src/Zagejmi.Domain` | Doménový model (uživatelé, profily hráčů a zákazníků, služby, recenze, peněženky a ledger, objednávky) |
| `src/Zagejmi.Application` | Use-casy rozdělené po funkcích (`Auth/Users/CreateUser`, `Profiles/SearchPlayers`, `Orders/PlaceOrder`, `Profiles/Reviews/WriteServiceReview`, …) a rozhraní, která implementuje infrastruktura (`*/Abstractions`) |
| `src/Zagejmi.Infrastructure` | EF Core (PostgreSQL), MinIO, SMTP, Argon2, SkiaSharp. Každá funkce má `Ctx/`, `Records/`, `Repositories/`; migrace jsou v `Migrations/<Context>/` |
| `src/Zagejmi.Api` | ASP.NET Core API (`Controllers/`) a Blazor Server UI (`Components/`) |
| `tests/` | Testy (xUnit v3 na Microsoft.Testing.Platform) |

Závislosti vedou pouze dovnitř: Api → Infrastructure → Application → Domain → Shared.

## Build

Požadavky: .NET SDK podle `global.json`, Node.js (kvůli TailwindCSS), Docker pro PostgreSQL/MinIO.
Doporučené prostředí je JetBrains Rider; spouštěcí konfigurace jsou v `.run/`.

```bash
dotnet tool restore
(cd src/Zagejmi.Api && npm ci)                        # Tailwind; wwwroot/styles.css pak generuje build
docker compose up -d postgresdb minio                 # nebo konfigurace "Docker Infra only"
dotnet build Zagejmi.slnx
dotnet test --solution Zagejmi.slnx
```

Testy infrastruktury proti skutečnému PostgreSQL (ledger a objednávky, vyhledávání, recenze) se spustí jen s proměnnou
`ZAGEJMI_TEST_POSTGRES` (connection string na server, kde smí uživatel zakládat databáze; každý běh si vytvoří a smaže
vlastní dočasnou databázi `zagejmi_test_…`), jinak se přeskočí. CI je spouští vždy:

```bash
ZAGEJMI_TEST_POSTGRES="Host=localhost;Port=5432;Database=postgres;Username=my_postgres_user;Password=my_strong_password" \
  dotnet test --solution Zagejmi.slnx
```

`dotnet build` spouští `npm run css:build` sám, když se změní `app.css` (téma a nastavení Tailwindu v4) nebo `.razor` soubory.
Při vývoji UI se `styles.css` dá průběžně generovat přes `npm run css:watch`, ale spolehlivě se v prohlížeči projeví až po buildu:
`MapStaticAssets` servíruje verzi zachycenou při posledním buildu.

### Migrace

Každý DbContext má vlastní schéma a vlastní migrace; aplikace je při startu pouští sama (vývoj vždy, produkce přes
`Database:MigrateOnStartup`):

| DbContext | Schéma | Složka | Obsah |
|---|---|---|---|
| `ZagejmiUsersContext` | `users` | `Migrations/Users` | uživatelé, ověření e-mailu, Ghost Mode |
| `ProfilesContext` | `profiles` | `Migrations/Profiles` | profily, služby, doporučení (`PlayerReviews`), recenze služeb (`ServiceReviews`) |
| `WalletsContext` | `wallets` | `Migrations/Wallets` | účty, ledger, bonusy a objednávky (objednávka a její pohyb Goinů se ukládají v jedné transakci) |
| `AccessTokensContext`, `BlobImagesContext` | `access_tokens`, `image_blobs` | `Migrations/AccessTokens`, `Migrations/BlobImages` | refresh tokeny, metadata obrázků |

```bash
dotnet ef migrations add <Nazev> --context ProfilesContext \
  -p src/Zagejmi.Infrastructure -s src/Zagejmi.Api -o Migrations/Profiles
```

Migrace, která už je na `master`, se nikdy nemaže ani nepřepisuje; změna se vždy dělá novou migrací.

### Nastavení aplikace

Výchozí hodnoty jsou v kódu, přepsat je jde v `appsettings*.json` nebo proměnnými prostředí (`Sekce__Klic`):

| Sekce | Klíč | Výchozí | K čemu |
|---|---|---|---|
| `Wallets` | `SignUpBonus`, `BonusLifetime` | 50, 90 dní | bonus po ověření e-mailu a jeho platnost |
| `Orders` | `FeePercent` | 10 | poplatek platformy z objednávky (zaokrouhleno dolů, min. 1 Goin) |
| `Orders` | `AcceptWindow`, `ConfirmWindow` | 48 h, 72 h | lhůta hráče na přijetí; lhůta zákazníka na potvrzení nebo nahlášení problému |
| `Orders` | `RefundedBonusGrace`, `TimeoutInterval` | 7 dní, 5 min | jak dlouho je vrácený bonus ještě použitelný; jak často běží kontrola lhůt |
| `Reviews` | `MinReviewsToShowRating` | 5 | od kolika recenzí se průměr ukáže na kartách |
| `Admin` | `Emails` | prázdné | e-maily administrátorů (ověřené), kteří rozhodují nahlášené objednávky na `/admin/orders` |
| `Smtp` | `Security` | `Auto` | zabezpečení spojení: `Auto` (port 465 = TLS hned, jinak STARTTLS podle `EnableSsl`), `StartTls`, `SslOnConnect`, `None` |
| `Smtp` | `Timeout` | 30 s | jak dlouho smí trvat jeden krok SMTP (spojení, přihlášení, odeslání) |

## Konvence

- Styl kódu je popsaný v `.editorconfig` a `stylecop.json` a vynucuje se při buildu (StyleCop + .NET analyzéry).
  Build by měl projít bez varování.
- Verze NuGet balíčků se spravují centrálně v `Directory.Packages.props`.
- Namespace odpovídá cestě ke složce. Nové use-casy patří do `Application/<Funkce>/<UseCase>/` jako dvojice
  `*Command`/`*Query` + `*Handler`.
- Veřejné typy a členy mají XML dokumentaci.

## Využité technologie

ASP.NET Core, Blazor Server, Entity Framework Core + PostgreSQL, MinIO, SkiaSharp, MailKit, TailwindCSS, xUnit v3.
V produkci Docker Compose, host Caddy a (připravený) mail server Stalwart.

## Produkce (Hetzner)

Nasazuje se pushem do větve `production`: GitHub Actions pustí testy, sestaví ARM64 Docker image a nasadí ho na server
(aplikace, PostgreSQL a MinIO v Dockeru; HTTPS dělá Caddy nainstalovaný přímo na serveru). Návod k prvnímu nastavení serveru, tajné hodnoty a vracení verzí je
v [deploy/README.md](deploy/README.md).

```bash
git push origin master:production
```

E-maily dnes odcházejí přes SMTP relay (Brevo) nastavený v `/opt/zagejmi/.env` na serveru. Připravený je i vlastní
mail server (Stalwart, služba `mail` v `deploy/compose.yaml`, vypnutá); jeho zapnutí a přepnutí aplikace na něj
popisuje [deploy/MAIL.md](deploy/MAIL.md). Aby šly řešit nahlášené objednávky, nastavte tam i `ADMIN_EMAIL`.

Debug stack s Mailpit (databáze, MinIO a API jsou v base compose, debug přidává Mailpit a debug SMTP override):

```bash
docker compose -f docker-compose.yaml -f docker-compose.debug.yaml up -d --build
```

## Branding

- [Create a logo for Zagejmi](https://github.com/Bojacnik/zagejmi/issues/37) — explore and select a recognizable logo and suitable variants for the website, small icons, and marketing materials. The visual direction is intentionally open.
