# Zagejmi

## Build

### Doporučené vývojové prostředí
JetBrains Rider

### Jak na build
Při naklonování repozitáře a otevření řešení Zagejmi.sln přes
vývojové prostředí Rider, se mají automaticky dogenerovat profily pro build,
debugging a spuštění. Nicméně je třeba pomocí nástroje NPM doinstalovat TailwindCSS, to by měl JetBrains Rider sám navrhnout,
a pokud se na zařízení nachází nástroj NPM například z NodeJS balíčku (https://nodejs.org/en), tak by měl uspět a nainstalovat do projektu Zagejmi.Server do složky node_modules všechny potřebné moduly.
Rider by měl by také doinstalovat všechny nutné balíčky z NuGet, které jsou potřeba pro jednotlivé projekty v řešení.
Rider typicky vybere špatný profil pro spuštění, stačí změnil profil ze Zagejmi.Client na Zagejmi: http který nejdřív spustí server a pak i klienta na debugging.

## Využité technologie

### Framework
ASP.NET

### Frontendové knihovny
TailwindCSS

### Backendové knihovny
.NET

Serilog

AnyMapper

MassTransit.Mediatr



### Další využité technologie
LINQ

### Plánované do budoucna
Po ujasnění hlavní struktury projektu, otestování core logiky pomocí NUnit nebo lépe xUnit








