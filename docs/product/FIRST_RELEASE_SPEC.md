# Zagejmi – Specifikace prvního vydání (R1)

> **Typ dokumentu:** specifikace prvního veřejného vydání; odpovídá na otevřené rozhodnutí 18 v [Product Blueprintu](./PRODUCT_BLUEPRINT.md)  
> **Stav:** návrh k potvrzení – rozhodnutí vlastníka jsou `CONFIRMED`, ostatní body `PROPOSED` (význam stavů viz [README](./README.md))  
> **Poslední změna:** 2026-11-19  
> **Navazuje na:** [Product Blueprint](./PRODUCT_BLUEPRINT.md), [Competitive Positioning](./COMPETITIVE_STRATEGY.md)

Blueprint zůstává zdrojem požadavků (ID jako `ORDER-001`). Tento dokument z nich vybírá, co patří do prvního
veřejného vydání, doplňuje chybějící pravidla jako návrhy a rozkládá práci do kroků s kritérii přijetí.
Nové požadavky dostávají ID v Blueprintu; tady se na ně jen odkazuje.

## 1. Cíl prvního vydání

**R1 je první veřejné spuštění Zagejmi se skutečnými penězi** (`CONFIRMED`, 2026-11-19): zákazník si koupí Goins,
objedná službu hráče, hráč ji odehraje a své výdělky si vybere na bankovní účet.

Celý tok, který musí v R1 fungovat a dát se důvěřovat:

```
najít (Služby / Hráči) → detail služby → koupit Goins → objednat (Escrow)
→ hráč přijme a odehraje → zákazník potvrdí → recenze → hráč si vybere výdělek
```

R1 je hotové, když tímto tokem projde skutečný zákazník a skutečný hráč se skutečnými penězi a každý krok je
dohledatelný v ledgeru i ve Stripe.

## 2. Podmínky spuštění (launch gates)

Skutečné peníze nesmí na produkci, dokud nejsou splněné tyto podmínky. Jsou to `REVIEW` body Blueprintu; tato
specifikace je neřeší, jen je dělá blokujícími.

| ID | Podmínka | Proč blokuje |
|---|---|---|
| GATE-1 | Podnikatelská forma provozovatele (IČO / s.r.o.) a bankovní účet (LEGAL-001) | Stripe účet, smlouvy s uživateli, účetnictví |
| GATE-2 | Právní posouzení platebního modelu (LEGAL-002), viz rozhodnutí D1 | Uložené zůstatky Goinů mohou být elektronické peníze |
| GATE-3 | Obchodní podmínky, zásady ochrany osobních údajů, reklamační řád (LEGAL-004) | Spotřebitelské smlouvy a GDPR |
| GATE-4 | DAC7 a daňové povinnosti (LEGAL-003): sběr údajů hráčů, DPH z poplatku | Platforma zprostředkuje platby za služby |
| GATE-5 | Stripe účet v živém režimu, schválený pro tento typ obchodu | Bez něj nejde přijímat platby |

Do splnění bran běží vše v testovacím režimu Stripe; produkce může běžet veřejně bez nákupu Goinů (jako dnes).

## 3. Rozsah

### 3.1 V R1

| Oblast | Co | Požadavky |
|---|---|---|
| Platby | Nákup Goinů přes Stripe, refund, chargeback | PAY-001, nový PAY-004 |
| Výplaty | Onboarding hráče na výplaty, výběr výdělku na účet | PAY-002, PAY-003 |
| Ledger | Nové typy pohybů pro nákup, výplatu a chargeback | WALLET-001–003 |
| Objednávky | Dnešní tok s Escrow, doplněné shrnutí před objednáním | ORDER-001–003 |
| Discovery | Detail služby, plný profil hráče, filtry na stránce Hráči | MARKET-001–004, PROFILE-004 |
| Důvěra | Role administrátora, nahlášení uživatele a recenze, zneplatnění tokenů | AUTH-001, REVIEW-002, ORDER-003 |
| Věk | Hráč 18+ ověřený při onboardingu na výplaty | VERIFY-002 |
| UI | Celý tok funguje od 360 px šířky | UI-001 |
| Provoz | Produkce na aktuálním `master`, zálohy, monitoring plateb | OPS-001 |

### 3.2 Mimo R1

Zůstávají v Blueprintu jako `LATER` nebo `OPEN`: Identita občana a vyšší bonus (VERIFY-004, REWARD-002),
ověření věku zákazníka dokladem (VERIFY-001), Ghost Mode nad rámec dnešního chování (PRIVACY-001),
leaderboard (RANK-002), relevance podle hodnocení (RANK-001), kalendář dostupnosti, chat, Analytics
(ANALYTICS-001–004), GitHub integrace a AI koordinátor (GITHUB-001/002, LINK-001, AI-001), vlastní mail server
(OPS-001, nezávisle na R1).

## 4. Ekonomika Goinů

Kurz, balíčky a poplatek Stripe rozhodl vlastník 2026-11-19 (D2, D7); ostatní řádky jsou `PROPOSED`.

| Pravidlo | Návrh / rozhodnutí |
|---|---|
| Kurz | `CONFIRMED`: 1 Goin = 1 Kč při nákupu; cena se zobrazuje v Goinech i v Kč |
| Balíčky | `CONFIRMED`: 200, 500, 1 000, 2 000 Goins; minimální nákup 200 Goins |
| Poplatek Stripe | `CONFIRMED`: nese platforma (zákazník platí přesně kurz × počet) |
| Poplatek platformy | Beze změny: 10 % z objednávky, dolů, min. 1 Goin (ORDER-001) |
| Výplata hráči | Jen výdělky (`PlayerEarnings`), vždy v Paid Goins (WALLET-002) |
| Bonus Goins | Nelze koupit ani vyplatit; jsou marketingový náklad platformy |
| Vrácení nevyužitých Paid Goins | Na žádost do 14 dnů od nákupu, pokud z nákupu nic neutratil (viz D4) |

**Důsledek, který je potřeba vědět:** část objednávky zaplacená Bonus Goiny se hráči vyplácí v Paid Goins
z `PromoBudget` (WALLET-002). Každý vydaný bonus je tedy budoucí skutečný výdaj platformy; výše signup bonusu
(dnes 50) a velikost `PromoBudget` jsou v R1 rozpočtové rozhodnutí (D5).

## 5. Nákup Goinů – PAY-001 (`PROPOSED`)

- Zákazník na stránce Peněženka zvolí balíček a zaplatí přes **Stripe Checkout** (hostovaná platební stránka;
  karta, Apple Pay, Google Pay). Zagejmi nikdy nevidí údaje karty.
- Goins se připíší **jen na základě webhooku** `checkout.session.completed` (ověřený podpis), nikdy podle
  návratu prohlížeče. Návratová stránka jen ukáže stav „čeká se na potvrzení“ / „připsáno“.
- Idempotence: Stripe session ID je unikátní klíč nákupu; opakovaný webhook nic nepřipíše podruhé.
- Ledger: `TopUpClearing → CustomerPaid` (nový typ `TopUp`); `TopUpClearing` jde do minusu o částku, kterou
  platforma dluží (odpovídá penězům přijatým přes Stripe).
- Záznam nákupu (`Purchase`): uživatel, balíček, Goins, částka v Kč, Stripe session/payment intent, stav
  (`Pending`, `Paid`, `Refunded`, `Disputed`), časy.
- Zákazník v Ghost Mode nakupovat smí (nákup se týká jen jeho vlastní peněženky).

### PAY-004 – Refund a chargeback (nový požadavek, `PROPOSED`)

- **Refund nákupu** (rozhoduje administrátor, D4): `CustomerPaid → TopUpClearing` (typ `TopUpRefund`) a refund
  přes Stripe API. Jde jen do výše Paid Goinů, které z nákupu zbývají.
- **Chargeback** (`charge.dispute.created`): stejný pohyb jako refund do výše zůstatku; zbytek, který zákazník
  už utratil, jde na nový systémový účet `ChargebackLosses`. Účet zákazníka se zablokuje pro nákupy a objednávky,
  dokud administrátor nerozhodne. Objednávky v Escrow se nemění.
- Výsledek sporu (`charge.dispute.closed`) jen zapíše stav; vyhraný spor nevrací automaticky nic.

## 6. Výplaty hráčům – PAY-002 (`PROPOSED`)

- Výplaty přes **Stripe Connect (Express účty)**: hráč při prvním výběru projde onboardingem u Stripe (identita,
  datum narození, bankovní účet). Stripe tím pokryje KYC a Zagejmi neukládá doklady (VERIFY-001 návrh).
- **Věk:** výplaty a nové služby jen pro hráče, kterému je podle Stripe ověřeného data narození aspoň 18 let
  (VERIFY-002). Profil hráče jde založit dál bez ověření, ale bez služeb se nezobrazí v katalogu.
- **Dostupnost výdělku:** výdělek z dokončené objednávky jde vybrat **14 dní** po dokončení (rezerva na
  reklamace a chargebacky). Do té doby je v peněžence vidět jako „čeká“.
- **Minimální výběr:** 500 Goins; výběr na žádost hráče, nejvýš jeden za den.
- Ledger: `PlayerEarnings → PayoutClearing` (typ `Payout`) a Stripe transfer na Connect účet; při selhání
  transferu zpětný pohyb (`PayoutFailed`). Stripe výplaty na banku řídí Stripe.
- Převod výdělků hráče do vlastní zákaznické peněženky v R1 není (WALLET-002, otevřené).

## 7. Discovery a objednávka (`PROPOSED`)

Podle Fáze A v [Competitive Positioning](./COMPETITIVE_STRATEGY.md) (sekce 8):

- **Detail služby** `/services/{id}`: název, hráč (odkaz na profil), hra, rank, styl, popis, jednotka, cena za
  jednotku v Goinech i Kč, průměr recenzí (od 5), poslední recenze, tlačítko Objednat. Karta služby vede sem,
  ne do popupu hráče.
- **Profil hráče** `/players/{id}` (PROFILE-004, minimální rozsah): profilovka, banner, přezdívka, „o mně“
  (prostý text do 2 000 znaků), hry a ranky ze služeb, aktivní služby, doporučení. Adresa podle ID, aby změna
  přezdívky nerozbila odkazy. Popup zůstává jako rychlý náhled s odkazem na profil.
- **Stránka Hráči:** stejné filtry jako Služby (hra, minimální rank, styl) – MARKET-003.
- **Shrnutí před objednáním:** služba, počet jednotek, celková cena, kolik se zaplatí z Bonus a z Paid Goinů,
  lhůty (48 h přijetí, 72 h potvrzení), co se stane při odmítnutí a nedostavení se. Při nedostatku Goinů rovnou
  nabídnout nákup balíčku a po zaplacení se vrátit k objednávce.

## 8. Důvěra a bezpečnost (`PROPOSED`)

- **Role administrátora v databázi** místo `Admin:Emails` (ORDER-003); udělení jen přes administraci nebo
  příkaz na serveru. Akce administrátora (rozhodnutí sporu, refund, blokace) se zapisují do auditního logu.
- **Zneplatnění tokenů** při odhlášení a při blokaci účtu (AUTH-001, issue #32).
- **Nahlášení:** uživatele (z profilu) a recenze (u recenze), s důvodem; fronta na `/admin/reports`.
  Blokace účtu zakáže přihlášení, objednávky a výplaty; zůstatky zůstanou.
- **Odpověď hráče na recenzi** jedním veřejným textem (REVIEW-002, otevřený bod) – pomáhá řešit spory veřejně.
- **Minimální věk zákazníka** (`CONFIRMED` 2026-11-19, D3): 18 let. Při registraci povinné potvrzení „je mi 18 let“
  (checkbox, ne ověření dokladem); u existujících účtů se potvrzení vyžádá před prvním nákupem Goinů.

## 9. Datový model (`PROPOSED`)

| Změna | Kde |
|---|---|
| `LedgerTransactionType`: `TopUp`, `TopUpRefund`, `Chargeback`, `Payout`, `PayoutFailed` | Wallets |
| Systémový účet `ChargebackLosses` (`AccountKind`, `SystemAccounts`) | Wallets |
| `Purchase` (nákup Goinů) s unikátním Stripe session ID | Wallets, nové schéma `payments` nebo `wallets` |
| `Payout` (výběr) s unikátním Stripe transfer ID a stavem | Wallets |
| `PayoutAccount`: uživatel ↔ Stripe Connect účet, stav onboardingu, ověřené datum narození | nové |
| `StripeEvent`: ID přijatého webhooku (deduplikace) | nové |
| Dostupnost výdělku: datum, od kdy je pohyb `OrderPayout` vybratelný | Wallets |
| Role administrátora, auditní log, nahlášení, blokace účtu | Users |
| `About` u profilu hráče; odpověď hráče u recenze | Profiles |

Každá migrace je nová (README: migrace na `master` se nemění).

## 10. Provoz (`PROPOSED`)

- Produkce běží na aktuálním `master` dřív, než se začne s platbami (Blueprint, sekce 17).
- Stripe klíče a webhook secret jen v `.env` na serveru; testovací a živý režim oddělené.
- Denní záloha PostgreSQL mimo server; ověřená obnova před spuštěním.
- Kontrola souladu: denní úloha porovná součet `TopUpClearing` a `PayoutClearing` s daty ve Stripe a pošle
  administrátorovi e-mail při rozdílu.
- Upozornění administrátorovi e-mailem na nový chargeback, nahlášení a selhanou výplatu.

## 11. Plán dodání

Každý krok je samostatné PR (nebo několik) se splněnými kritérii přijetí a testy; pořadí kroků 1–4 jde měnit,
krok 5 je poslední.

| Krok | Obsah | Kritéria přijetí |
|---|---|---|
| 0 | Produkce na `master`, zálohy, `ADMIN_EMAIL` | zagejmi.cz běží na posledním `master`; obnova ze zálohy vyzkoušená |
| 1 | Důvěra: role admin, audit log, nahlášení, blokace, zneplatnění tokenů | blokovaný uživatel se nepřihlásí a jeho starý token neplatí; admin akce jsou v logu |
| 2 | Discovery: detail služby, profil hráče, filtry Hráči, shrnutí objednávky | karta služby vede na detail; objednávka z detailu ukáže rozpad Bonus/Paid; vše od 360 px |
| 3 | Nákup Goinů (testovací Stripe): Checkout, webhook, refund, chargeback | opakovaný webhook nepřipíše dvakrát; chargeback po utracení zablokuje účet a zapíše ztrátu |
| 4 | Výplaty (testovací Stripe): Connect onboarding, 18+, 14 dní, výběr | hráč pod 18 nevybere; výdělek je vybratelný až po 14 dnech; selhaný transfer vrátí Goins |
| 5 | Spuštění: brány GATE-1–5, živé klíče, první skutečný nákup a výplata | skutečný nákup a výplata dohledatelné v ledgeru i ve Stripe; kontrola souladu bez rozdílu |

## 12. Otevřená rozhodnutí pro R1

| # | Rozhodnutí | Návrh |
|---|---|---|
| D1 | Platební model: uložené zůstatky Goinů (dnešní peněženka) vs. platba každé objednávky zvlášť přes Stripe Connect (Goins jen jako bonus) | Rozhodnout po právní konzultaci (GATE-2); spec počítá s peněženkou, protože ji kód už má |
| ~~D2~~ | Kurz a balíčky Goinů | Rozhodnuto 2026-11-19: 1 Goin = 1 Kč; 200 / 500 / 1 000 / 2 000 |
| ~~D3~~ | Minimální věk zákazníka a forma potvrzení (VERIFY-003) | Rozhodnuto 2026-11-19: 18+, potvrzení při registraci |
| D4 | Vracení nevyužitých Paid Goinů | Na žádost do 14 dnů, jen neutracená část nákupu |
| D5 | Výše signup bonusu a rozpočet `PromoBudget` při skutečných penězích (WALLET-003) | Ponechat 50 Goins; rozpočet stanovit před spuštěním |
| D6 | Doba do dostupnosti výdělku a minimální výběr | 14 dní; 500 Goins |
| ~~D7~~ | Kdo platí poplatek Stripe | Rozhodnuto 2026-11-19: platforma |
