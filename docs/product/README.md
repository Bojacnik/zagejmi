# Product discovery

Tato složka je centrální zdroj produktových požadavků pro Zagejmi. Zachycuje průběžný brainstorming, produktové záměry a rozhodnutí tak, aby bylo možné navázat z jiného zařízení nebo s jiným AI coding agentem.

## Dokumenty

- [Product Blueprint](./PRODUCT_BLUEPRINT.md) – aktuální přehled produktu, požadavků, stavu implementace a otevřených rozhodnutí.

## Jak dokumentaci číst

Každý požadavek má stabilní identifikátor a stav:

| Stav | Význam |
|---|---|
| `EXISTING` | Podle aktuální informace už funkcionalita v aplikaci existuje; implementace zatím nemusí být auditovaná. |
| `CONFIRMED` | Požadavek nebo produktový záměr výslovně potvrdil vlastník produktu. |
| `PROPOSED` | Návrh k diskusi; není schválený a nesmí se považovat za závazný požadavek. |
| `OPEN` | Chybí rozhodnutí, detail nebo potřebná analýza. |
| `LATER` | Požadavek je zamýšlený pro pozdější etapu, nikoliv jako předpoklad prvního spuštění. |
| `REVIEW` | Vyžaduje technické, bezpečnostní nebo právní ověření před implementací či produkčním spuštěním. |

Stav a časové zařazení jsou dvě různé věci. Například požadavek může být `CONFIRMED` a současně `LATER`.

## Pravidla práce

1. **Brainstorming není automaticky specifikace.** Nové nápady se zapisují jako `OPEN` nebo `PROPOSED`, dokud je vlastník produktu nepotvrdí.
2. **Nedomýšlet rozhodnutí.** Neznámé částky, hranice, workflow, výjimky a technické volby zůstávají výslovně otevřené.
3. **Oddělovat požadavek od implementačního návrhu.** Požadavek popisuje, co má produkt dělat. Technické řešení se zapisuje jako návrh, dokud není ověřeno proti existujícímu kódu a architektuře.
4. **Před vytvořením issues nejprve upřesnit rozsah.** Issue má mít kontext, cíl, acceptance criteria, závislosti a nevyřešené předpoklady. Velké celky se nejprve rozdělí na epicy/features.
5. **Před implementací kontrolovat existující aplikaci.** Označení `EXISTING` vychází z dosavadního popisu; před plánováním změn je nutný audit kódu.
6. **Právní poznámky nejsou právní stanovisko.** Regulatorní oblasti se musí ověřit s kvalifikovaným odborníkem podle finálního obchodního a platebního modelu.

## Konvence identifikátorů

- `AUTH` – autentizace a autorizace
- `PROFILE` – uživatelské profily
- `PRIVACY` – soukromí a Ghost Mode
- `VERIFY` – věk a identita
- `MARKET` – marketplace, služby a discovery
- `RANK` – relevance a leaderboard
- `WALLET` – Goin, peněženky a ledger
- `PAY` – Stripe, zprostředkované platby a cashout
- `ORDER` – objednávky, Escrow, lhůty a řešení problémů
- `REVIEW` – recenze služeb a doporučení hráčů
- `REWARD` – Bonus CAP a odměny
- `ANALYTICS` – statistiky hráče
- `OPS` – provoz a infrastruktura
- `UI` – uživatelské rozhraní, responzivita a přístupnost
- `LEGAL` – právní a regulatorní připravenost

Identifikátory se po přidělení nemění ani při přesunu požadavku do jiné sekce.
