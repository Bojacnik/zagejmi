# Vlastní mail server (Stalwart)

Aplikace dnes posílá e-maily přes SMTP relay (Brevo). Pro `@zagejmi.cz` je připravený vlastní mail server
**Stalwart** (SMTP, IMAP, DKIM/SPF/DMARC, spam filtr, webová administrace) jako služba `mail` v
[compose.yaml](compose.yaml). Je vypnutá, dokud v `.env` není `COMPOSE_PROFILES=mail`.

Hotové přepnutí znamená:

- aplikace posílá jako `noreply@zagejmi.cz` přes vlastní server;
- `@zagejmi.cz` má skutečné schránky (IMAP, např. v Thunderbirdu nebo v mobilu);
- vrátit se na relay jde jedním příkazem.

## Jak to je zapojené

| Co | Kde |
|---|---|
| Porty 25 (příjem pošty), 465 a 587 (odesílání z klientů), 993 (IMAP) | publikuje přímo kontejner `mail`, Caddy mezi tím není, takže Stalwart vidí skutečné IP odesílatelů |
| Webová administrace, MTA-STS, autoconfig | host Caddy → `127.0.0.1:8090` (`https://mx.zagejmi.cz`) |
| TLS certifikát | získá ho Caddy pro `mx.zagejmi.cz`; `zagejmi-mail-cert-sync` (systemd timer, denně) ho kopíruje do `/opt/zagejmi/mail-tls` a Stalwart ho načte bez restartu |
| Aplikace → mail server | uvnitř sítě `zagejmi` pod jménem `mx.zagejmi.cz` (alias), port 465, certifikát sedí |
| Data a nastavení | Docker volumes `zagejmi_mail_data`, `zagejmi_mail_config` (zálohovat spolu s databází) |
| Hesla | v `/opt/zagejmi/.env`: `MAIL_ADMIN` (správce), `MAIL_NOREPLY_PASSWORD`, `MAIL_POSTMASTER_PASSWORD` |

Skripty jsou v [mail/](mail) a nasazení je kopíruje do `/opt/zagejmi/mail`:

| Skript | Spouští | K čemu |
|---|---|---|
| `install-host.sh` | root, jednou | blok v Caddyfile, timer na certifikát, kontrola reverzního DNS a portu 25 |
| `setup-mail.sh` | deploy, kdykoli znovu | zapne a nastaví Stalwart, založí schránky, vypíše DNS záznamy |
| `switch-smtp.sh own\|relay\|status` | deploy | přepne, kudy aplikace posílá e-maily (jen řádky `SMTP_*` v `.env` + restart aplikace) |
| `sync-cert.sh` | timer (root) | kopie certifikátu od Caddy a jeho načtení ve Stalwartu |

## Přepnutí krok za krokem

### 1. Hetzner

V Hetzner konzoli (server → *Networking*):

- **Odblokovat odchozí porty 25 a 465.** Je to žádost na podporu; bez portu 25 se pošta pro cizí servery (Gmail,
  Seznam, …) jen hromadí ve frontě.
- **Reverzní DNS** IPv4 adresy `188.245.17.169` nastavit na `mx.zagejmi.cz`.
  - Bez toho velcí poskytovatelé poštu odmítají nebo dávají do spamu.
  - Vutregbot to nijak neovlivní.

### 2. DNS (Wedos), první část

Záznamy, které musí existovat dřív, než Caddy získá certifikát:

```
mx.zagejmi.cz.            A      188.245.17.169
mta-sts.zagejmi.cz.       CNAME  mx.zagejmi.cz.
autoconfig.zagejmi.cz.    CNAME  mx.zagejmi.cz.
autodiscover.zagejmi.cz.  CNAME  mx.zagejmi.cz.
```

- Jen A záznam, žádný AAAA. Pošta jde přes IPv4, kde je nastavené reverzní DNS.
- `mail.zagejmi.cz` je obsazené Brevem (vlastní doména pro sledování odkazů), proto se mail server jmenuje `mx`.

### 3. Server, jako root

```bash
ssh root@188.245.17.169
cd /opt/zagejmi && bash mail/install-host.sh
```

Skript:

1. Přidá do `/etc/caddy/Caddyfile` jeden nový blok pro `mx.zagejmi.cz` a další jména. Před tím soubor zazálohuje a
   pak ho ověří přes `caddy validate`. Ostatní weby (vutregbot) nemění.
2. Nainstaluje timer na kopírování certifikátu.
3. Počká na certifikát.
4. Zkontroluje reverzní DNS a port 25.

### 4. Server, jako deploy

```bash
ssh deploy@188.245.17.169
cd /opt/zagejmi && bash mail/setup-mail.sh
```

- Doplní do `.env`:
  - `COMPOSE_PROFILES=mail`, takže mail server poběží i po každém dalším nasazení;
  - `MAIL_HOSTNAME`;
  - vygenerovaná hesla.
- Spustí Stalwart. Při prvním startu ho nastaví: jméno serveru, doména, klíče DKIM, logy do `docker compose logs mail`.
- Přidá certifikát a port 587 a založí schránky `noreply@` a `postmaster@`.
- Vypíše DNS záznamy. Skript jde pustit znovu, přidá jen to, co chybí.

### 5. DNS (Wedos), druhá část

Záznamy z výpisu `setup-mail.sh` doplňte s těmito úpravami:

- **SPF**, tedy jeden TXT záznam na `zagejmi.cz`. Dokud přes Brevo posílá i něco jiného jako `@zagejmi.cz` (vutregbot),
  musí v něm Brevo zůstat:
  `v=spf1 mx include:spf.brevo.com ~all`. Až Brevo nebude potřeba, stačí `v=spf1 mx -all`.
- **DKIM**: přidat oba nové záznamy `v1-…._domainkey`. Brevo záznamy `brevo1`/`brevo2` nechat, dokud se Brevo používá.
- **DMARC**: stávající záznam změnit na `v=DMARC1; p=none; rua=mailto:postmaster@zagejmi.cz`. Po několika týdnech bez
  problémů v reportech zpřísnit na `p=quarantine` a pak `p=reject`.
- **MX, SRV, MTA-STS, TLS-RPT, autoconfig**: přidat tak, jak jsou.
- **TLSA** skript záměrně nevypisuje: klíč certifikátu se s každou obnovou mění a zóna nemá DNSSEC.

Kontrola: `dig +short MX zagejmi.cz`, `dig +short TXT zagejmi.cz` a test na https://www.mail-tester.com (pošlete na
něj e-mail ze schránky `postmaster@`).

### 6. Přepnutí aplikace

```bash
cd /opt/zagejmi && bash mail/switch-smtp.sh own
```

Pak se na https://zagejmi.cz zaregistrujte s vlastní adresou a ověřte, že ověřovací e-mail dorazil a není ve spamu.

**Zpět na relay** (kdykoli, třeba když něco nedoručuje):

```bash
cd /opt/zagejmi && bash mail/switch-smtp.sh relay
```

`switch-smtp.sh status` ukáže, co se právě používá. Hesla nevypisuje.

## Schránky a správa

- **Webová administrace:** https://mx.zagejmi.cz/admin.
  - Přihlášení: `admin` a heslo z `MAIL_ADMIN` v `.env` (část za `admin:`).
  - Tady se zakládají další schránky (`info@`, `podpora@`, …), aliasy a hesla.
- **Postmaster:** do `postmaster@zagejmi.cz` chodí nedoručenky a reporty DMARC/TLS.
  - Heslo je `MAIL_POSTMASTER_PASSWORD` v `.env`.
  - Když ho změníte ve webové administraci, `.env` už neplatí.
- **Mail klient** (Thunderbird, mobil):
  - Server `mx.zagejmi.cz`, uživatel je celá e-mailová adresa.
  - IMAP 993 (SSL/TLS), SMTP 465 (SSL/TLS) nebo 587 (STARTTLS).
  - Díky autoconfig si to Thunderbird většinou najde sám.
- **Logy:** `docker compose logs -f mail`.
- **Fronta odchozí pošty a doručování:** webová administrace → *Queue*.

## Co je dobré vědět

- **Port 465 z aplikace:**
  - Aplikace se k vlastnímu serveru připojuje uvnitř Dockeru, takže blokace Hetzneru se jí netýká.
  - Relay mimo server musí být na 587, dokud Hetzner porty neodblokuje.
- **Docker a firewall:** porty publikované Dockerem (25, 465, 587, 993) obchází `ufw`. Jsou otevřené, jakmile mail
  server běží; nic dalšího se povolovat nemusí.
- **Aktualizace Stalwartu:**
  - Verze je pevně v `compose.yaml` (`stalwartlabs/stalwart:v1.0.3`).
  - Novější verzi vyzkoušet lokálně, změnit tag a nasadit.
  - Před velkou verzí zálohovat volumes.
- **Lokální zkouška:**
  - Stejné skripty jdou spustit na kopii `/opt/zagejmi` s `COMPOSE_PROJECT_NAME=…` a `compose.override.yaml`, který
    přemapuje porty na volné.
  - Certifikát z vlastní testovací CA. Pozor: aplikace (MailKit) kontroluje i revokaci, takže testovací CA potřebuje
    CRL dostupné z kontejneru.
