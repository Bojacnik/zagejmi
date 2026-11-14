# Nasazení na Hetzner

Produkce běží na jednom Hetzner Cloud serveru v Dockeru. Nasazuje se **pushem do větve `production`**:

```
push do production ─▶ testy ─▶ Docker image (ghcr.io/bojacnik/zagejmi:<sha>) ─▶ SSH na server ─▶ docker compose up ─▶ čeká na /healthz
```

Workflow: [`.github/workflows/deploy.yml`](../.github/workflows/deploy.yml). Když testy neprojdou, nic se nenasadí.

Server: Hetzner CAX (ARM64, Debian 13), `188.245.17.169`. Image se proto staví na ARM runneru GitHubu.

HTTPS dělá **Caddy nainstalovaný přímo na serveru** (systemd, `/etc/caddy/Caddyfile`), který obsluhuje i další weby
(např. `vutregbot.zagejmi.cz`). Pro Zagejmi v něm stačí:

```
zagejmi.cz, www.zagejmi.cz {
	reverse_proxy 127.0.0.1:3000
}
```

V Dockeru (`/opt/zagejmi`) běží:

| Služba     | K čemu                                                    | Zvenku                                 |
|------------|-----------------------------------------------------------|----------------------------------------|
| `api`      | aplikace Zagejmi; migrace databáze pouští sama při startu | jen přes Caddy (`127.0.0.1:3000`)      |
| `postgres` | databáze                                                  | ne                                     |
| `minio`    | obrázky                                                   | ne                                     |

## Vydání nové verze

```bash
# z masteru do produkce (nebo PR master → production na GitHubu)
git push origin master:production
```

Průběh je v **Actions → Deploy**. Hotovo je, když krok „Wait until healthy“ projde.

- **Vrácení zpět:** Actions → Deploy → *Run workflow* a do `image_tag` dát celé SHA starší verze. Testy ani build se
  nepouští, nasadí se rovnou hotový image. Druhá možnost je `git revert` na větvi `production`.
- **Znovu nasadit to samé:** *Run workflow* s prázdným `image_tag`.

## První nastavení (jednou)

### 1. DNS

U registrátora domény nastavte záznamy na IP serveru:

```
zagejmi.cz.      A     <IP serveru>
www.zagejmi.cz.  A     <IP serveru>
```

(Volitelně i `AAAA` na IPv6 adresu serveru.) Caddy si certifikát vyžádá sám, DNS ale musí platit dřív.

### 2. Klíč pro GitHub Actions

Na svém počítači vytvořte samostatný klíč jen pro nasazování (bez hesla):

```bash
ssh-keygen -t ed25519 -N "" -C "github-deploy-zagejmi" -f ~/.ssh/zagejmi_deploy
```

### 3. Příprava serveru

Skript nainstaluje Docker, vytvoří uživatele `deploy` (přihlášení jen klíčem z kroku 2), složku `/opt/zagejmi`,
firewall (jen SSH, HTTP, HTTPS) a automatické bezpečnostní aktualizace. Caddy nechá být. Spustit jako root, jde to
i opakovaně:

```bash
ssh root@<IP serveru> "bash -s -- '$(cat ~/.ssh/zagejmi_deploy.pub)'" < deploy/setup-server.sh
```

### 4. Tajné hodnoty na serveru

```bash
scp deploy/.env.example root@<IP serveru>:/opt/zagejmi/.env
ssh root@<IP serveru>
  cd /opt/zagejmi && chown deploy:deploy .env && chmod 600 .env
  for v in POSTGRES_PASSWORD MINIO_ROOT_PASSWORD JWT_KEY PASSWORD_PEPPER; do
    sed -i "s/^$v=$/$v=$(openssl rand -hex 32)/" .env
  done
  nano .env   # zkontrolovat DOMAIN, doplnit SMTP a ADMIN_EMAIL
```

`ADMIN_EMAIL` je e-mail (ověřeného) účtu, který na `/admin/orders` rozhoduje nahlášené objednávky: vrátí Goiny
zákazníkovi, nebo vyplatí hráče. Bez něj zůstanou nahlášené objednávky zmrazené. Další nastavení (poplatek, lhůty
objednávek, práh pro zobrazení hodnocení) mají rozumné výchozí hodnoty, viz „Nastavení aplikace“ v hlavním
[README](../README.md); pro změnu je potřeba přidat proměnnou do `environment` služby `api` v `compose.yaml`.

Po změně `.env` stačí `docker compose up -d` (s `IMAGE_TAG=$(cat .deployed-tag)`), viz „Na serveru“ níže.

> **`PASSWORD_PEPPER` po první registraci už nikdy neměňte**: je součástí každého hashe hesla a po změně by se nikdo
> nepřihlásil. `.env` si zálohujte mimo server (třeba ve správci hesel).

### 5. GitHub

**Settings → Environments → New environment** `production`. Volitelně:

- *Deployment branches*: jen `production`;
- *Required reviewers*: pokud má každé nasazení někdo schválit.

Do prostředí `production` přidejte tyto **secrets**:

| Secret               | Hodnota                                                      |
|----------------------|--------------------------------------------------------------|
| `DEPLOY_HOST`        | IP serveru                                                   |
| `DEPLOY_USER`        | `deploy`                                                     |
| `DEPLOY_SSH_KEY`     | obsah `~/.ssh/zagejmi_deploy` (soukromý klíč, celý soubor)   |
| `DEPLOY_KNOWN_HOSTS` | výstup `ssh-keyscan <IP serveru>`                            |

Doporučeno je i **Settings → Branches → pravidlo pro `production`**: zakázat force-push a mazání větve.

### 6. První nasazení

```bash
git push origin master:production
```

Po prvním úspěšném nasazení bude image na **github.com/Bojacnik?tab=packages**. Server se přihlašuje při každém
nasazení dočasným tokenem, takže image může zůstat soukromý.

## Na serveru

```bash
ssh deploy@<IP serveru>
cd /opt/zagejmi
export IMAGE_TAG=$(cat .deployed-tag)   # verze, která právě běží

docker compose ps                       # stav služeb
docker compose logs -f api              # logy aplikace
docker compose restart api
docker compose exec postgres psql -U zagejmi zagejmi
```

**Zálohy** zatím nejsou automatické. Ruční záloha databáze:

```bash
docker compose exec -T postgres pg_dump -U zagejmi -Fc zagejmi > zagejmi-$(date +%F).dump
```

Pro celý server stačí zapnout **Backups** v Hetzner Cloud Console (denní snapshoty, +20 % ceny serveru).
