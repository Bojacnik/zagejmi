# Zagejmi – Product Blueprint

> **Typ dokumentu:** živý produktový discovery dokument  
> **Stav:** průběžný brainstorming; nejde o finální technickou specifikaci  
> **Poslední zachycení:** 2026-11-13  
> **Zdroj:** produktové rozhovory s vlastníkem Zagejmi

## 1. Produktový záměr

Zagejmi je marketplace pro herní služby. Zákazník může hledat buď konkrétního hráče, nebo konkrétní službu, kterou chce objednat. Hráč vytváří profil a nabízí jednu či více herních služeb.

Zagejmi má vlastní virtuální měnu **Goin** (jednotka: Goin, množné číslo: Goins). V ekonomice platformy se rozlišují placené a bonusové Goins.

Tento dokument se týká výhradně produktu Zagejmi. Nesmí se zaměňovat s projektem VUTRegBot.

## 2. Slovník a konvence

- **Customer** – uživatel, který hledá a objednává služby.
- **Player** – uživatel, který nabízí herní služby.
- **Service** – samostatná nabídka služby poskytovaná hráčem.
- **Goin / Goins** – virtuální měna Zagejmi. Nepoužívat zkratku „GOI“.
- **Paid Goins** – Goins získané nákupem.
- **Bonus Goins** – Goins přidělené platformou jako odměna nebo kompenzace.
- **Bonus CAP** – centrální omezená zásoba/limit určený pro vydávání Bonus Goinů.
- **Central Wallet / platform account** – centrální účet platformy používaný pro zprostředkování transakcí; jeho přesná účetní a právní povaha zatím není navržena.
- **Ghost Mode** – režim, ve kterém je uživatel anonymnější vůči ostatním uživatelům, ale zůstává identifikovaný pro server/platformu.

Požadavky používají stavy definované v [README](./README.md).

## 3. Přehled funkčních oblastí

| Oblast | Aktuální obraz |
|---|---|
| Authentication | Základní login a registrace podle vlastníka produktu existují; ověřování e-mailu je potřeba doplnit. |
| Profiles | Existují profily; rozlišení Player/Customer a jejich budoucí chování je potřeba porovnat s aktuálním kódem. |
| Privacy | Ghost Mode je plánovaný produktový celek. |
| Verification | Ověření věku/identity je plánované do budoucna. |
| Marketplace | Hráči jsou v navigaci; samostatná sekce Služby zatím chybí. |
| Wallet | Virtuální měna Goin už existuje; přesný stav wallet/ledger je nutné auditovat. |
| Payments | Nákup Goinů přes Stripe a cashout hráčů podle vlastníka produktu zatím nejsou k dispozici. |
| Analytics | Rozšířené Player Analytics jsou plánované později. |
| Legal & launch | Podnikatelský, platební a regulatorní model musí být vyhodnocen před ostrým komerčním spuštěním. |

---

## 4. Authentication & access control

### AUTH-001 – Základní registrace a přihlášení
- **Stav:** `EXISTING`
- Základní registrace a login jsou již implementované.
- Před dalšími změnami je nutné ověřit jejich současné chování a datový model v kódu.

### AUTH-002 – Ověření e-mailu
- **Stav:** `CONFIRMED`
- Po registraci má uživatel ověřit vlastnictví e-mailové adresy.
- Je potřeba navrhnout ověřovací odkaz/token, jeho expiraci, opakované zaslání a jednorázové použití.
- Zásady pro neověřené účty (co smějí dělat a kdy je vyžadováno ověření) zůstávají otevřené.

### AUTH-003 – Veřejné prohlížení a přihlášení pro používání
- **Stav:** `CONFIRMED`
- Nepřihlášený návštěvník smí prohlížet veřejný obsah marketplace.
- Pro použití funkcí platformy, zejména akcí jako objednávání, musí být uživatel přihlášený.
- Soukromé údaje uživatelů nesmějí být dostupné nepřihlášeným návštěvníkům.
- Ochrana musí být vynucena serverově/API, nikoliv pouze skrytím prvků ve frontendu.
- Přihlášení samo o sobě nedává přístup k soukromým údajům ostatních uživatelů; je nutná autorizace podle vztahu a oprávnění.

### AUTH-004 – Typ profilu při začátku používání
- **Stav:** `CONFIRMED` (základní záměr), `OPEN` (detail)
- Při začátku používání si uživatel vytvoří Player nebo Customer profil.
- Není rozhodnuto, zda může jeden účet později provozovat oba typy profilů, přepínat mezi nimi nebo je kombinovat.

## 5. E-mailová infrastruktura

### OPS-001 – Oddělené SMTP prostředí
- **Stav:** `CONFIRMED`
- Development/debug: Mailpit.
- Současná produkce: existující SMTP relay vlastníka projektu.
- Pozdější produkce: vlastní mail server po uplynutí prvního měsíce provozu Hetzner serveru.
- Aplikační logika odesílání e-mailů má být nezávislá na konkrétním SMTP provideru.
- Konkrétní údaje relay serveru ani tajné hodnoty nepatří do dokumentace v repozitáři.

---

## 6. Player & Customer profiles

### PROFILE-001 – Player a Customer profily
- **Stav:** `CONFIRMED` (produktový záměr), `REVIEW` (stávající implementace)
- Základní typy profilu jsou Player a Customer.
- Při vytvoření profilu uživatel na začátku zvolí jeden z těchto typů.
- Možnost pozdějšího vytvoření druhého typu profilu je otevřená.

### PROFILE-002 – Player nabízí vlastní služby
- **Stav:** `CONFIRMED` (produktový záměr), `PROPOSED` (datový návrh)
- Player má mít možnost vypisovat služby, které poskytuje.
- Zákazník může najít hráče přes jeho profil nebo hledat přímo službu.
- **Návrh:** Service by měla být samostatná entita navázaná na Player profil, s vlastní kategorií, hrou, parametry, cenou a dostupností. Tento datový návrh je potřeba ověřit proti existujícímu modelu a potvrdit před implementací.

---

## 7. Privacy – Ghost Mode

### PRIVACY-001 – Ghost Mode / anonymnější přítomnost
- **Stav:** `CONFIRMED` (záměr), `OPEN` (konkrétní rozsah)
- Uživatel se může přepnout do Ghost Mode.
- Server stále zná skutečný přihlášený účet a identitu uživatele.
- Ostatním uživatelům se má zobrazovat anonymněji/neviditelněji; uživatel popsal režim jako kombinaci invisibility a anonymity.
- Přesné chování pro online status, profil, aktivitu, chat, objednávky, recenze a historii je nutné upřesnit.
- Ghost Mode není anonymita vůči platformě, administraci ani zákonným povinnostem.

### PRIVACY-002 – Ghost Mode platba
- **Stav:** `CONFIRMED` (zamýšlené chování), `REVIEW` (ledger, bezpečnost a regulace)
- Když zákazník v Ghost Mode zaplatí za službu ze své peněženky, hráč nemá vidět ID zákaznické peněženky.
- Zamýšlený tok: Customer wallet → centrální Goin účet platformy → Player wallet.
- U transakce na straně hráče má být viditelné, že platba byla zprostředkována v Ghost Mode.
- Platforma/server musí zachovat interní vazbu na skutečného plátce kvůli účetnictví, sporům, refundům a případným právním povinnostem.
- Přesné účetní zápisy, vypořádání, refundy a poplatky zatím nejsou navrženy.
- Central Wallet pro zprostředkované platby musí být odlišena od Bonus CAP.

---

## 8. Age & identity verification

### VERIFY-001 – Ověření věku a identity
- **Stav:** `LATER`
- Plánované metody:
  - ověření občanským průkazem,
  - ověření obličejem,
  - ověření přes Identitu občana.
- Je nutné rozlišovat ověření věku od úplného ověření identity; nemusí jít o totožné úrovně ověření.
- Výběr poskytovatele, dostupnost metod, ukládání důkazů a retenční pravidla jsou otevřené.
- **Návrh k bezpečnostnímu posouzení:** pokud to poskytovatel dovolí, ukládat pouze výsledek a nezbytná metadata ověření, nikoliv kopie dokladů nebo biometrické šablony.

### VERIFY-002 – Minimální věk Player
- **Stav:** `CONFIRMED` jako produktové pravidlo, `REVIEW` z hlediska právního základu
- Vlastník produktu souhlasil s pravidlem, že poskytovatelé (Player účty) mají být minimálně 18letí.
- Jde o produktové rozhodnutí pro bezpečnost platformy, nikoliv o potvrzení, že české právo obecně stanoví pro všechny podobné služby stejnou hranici.
- Jak a kdy se věk ověří, včetně případů výjimek a odvolání, je otevřené.

### VERIFY-003 – Minimální věk Customer
- **Stav:** `OPEN`
- Minimální věk zákazníka a pravidla pro nezletilé zákazníky nebyly rozhodnuty.

---

## 9. Marketplace discovery – Players & Services

### MARKET-001 – Samostatná navigace Hráči a Služby
- **Stav:** `CONFIRMED`
- Navigace má obsahovat dvě samostatné sekce:
  - **Hráči:** zákazník hledá konkrétního člověka a prohlíží jeho profil a nabídky.
  - **Služby:** zákazník hledá konkrétní službu bez nutnosti začít výběrem konkrétního hráče.
- Obě cesty mají pracovat se stejnými hráči a jejich nabídkami; nejde o dva nezávislé marketplace katalogy.

### MARKET-002 – Výchozí rozdělení výsledků podle pohlaví
- **Stav:** `CONFIRMED` (výchozí UX), `OPEN` (okrajové případy a profilová data)
- Ve výchozím zobrazení mají být výsledky rozdělené do dvou skupin: ženy nahoře, muži níže.
- Uživatel může toto rozdělení vypnout a zobrazit společný seznam.
- Další filtry musí být použitelné jak při rozděleném, tak při společném zobrazení.
- Je nutné rozhodnout, jak zobrazit profily s jinou nebo neuvedenou hodnotou pohlaví; hodnota se nesmí odvozovat z fotografie ani odhadem.

### MARKET-003 – Filtry discovery
- **Stav:** `CONFIRMED` (požadované typy filtrů), `OPEN` (konkrétní UI a datové možnosti)
- Filtry mají zahrnovat:
  - konkrétní službu,
  - kategorii,
  - videohru,
  - rank pro konkrétní hru,
  - kompetitivní versus nekompetitivní hraní.
- Filtry se mají dát kombinovat a fungovat v obou režimech zobrazení (rozdělený / společný seznam).

### MARKET-004 – Samostatné služby a jejich atributy
- **Stav:** `CONFIRMED` (potřeba samostatné sekce Služby), `OPEN` (detail katalogu)
- Nabídka služby je spojena s Playerem.
- Konkrétní kategorie, atributy, pravidla pro hry/ranky, cenotvorba, dostupnost a varianty objednávky se doplní při detailním návrhu.

---

## 10. Relevance & leaderboard

### RANK-001 – Řazení výsledků podle relevance
- **Stav:** `CONFIRMED` (potřeba relevance), `REVIEW` (benchmark a algoritmus)
- Výsledky mají být seřazeny podle relevance pro zákazníka.
- Inspirací má být relevance na e-pal.gg.
- Přesné chování e-pal.gg je nutné samostatně zanalyzovat; cílem je vytvořit vlastní vhodný algoritmus, ne kopírovat neveřejný kód nebo interní implementaci.
- Kandidátní faktory jako shoda služby/hry/ranku, hodnocení, počet recenzí, dostupnost nebo cena jsou zatím pouze `PROPOSED` a musí být ověřeny.

### RANK-002 – Leaderboard hráčů
- **Stav:** `CONFIRMED` (požadavek), `OPEN` (metrika a pravidla)
- Marketplace má obsahovat leaderboard.
- Není rozhodnuto, zda půjde o žebříček podle výdělků, hodnocení, aktivity, kvality, popularity, nebo kombinovaného skóre.
- Leaderboard nemusí používat stejný algoritmus jako relevance vyhledávání.

---

## 11. Wallet, Goin & payments

### WALLET-001 – Goin jako virtuální měna
- **Stav:** `EXISTING`
- Virtuální měna Goin už v aplikaci existuje.
- Přesný současný model peněženek, ledgeru a transakčních typů je nutné auditovat v kódu.

### WALLET-002 – Oddělení Paid a Bonus Goins
- **Stav:** `CONFIRMED`
- Goins jsou rozděleny na:
  - **Paid Goins:** zakoupené Goins.
  - **Bonus Goins:** Goins získané od platformy jako odměna.
- Tyto typy musí být v produktu a účetní evidenci rozlišitelné.
- Pravidla útraty, převodu mezi typy, expirace, refundace a cashoutu zůstávají otevřená.

### WALLET-003 – Centrální Bonus CAP
- **Stav:** `CONFIRMED` (koncept), `OPEN` (účetní model a limity)
- Bonus Goins se mají vydávat z centrálního CAP/limitu, aby platforma měla kontrolu nad množstvím bonusové měny.
- Přesná podoba CAP (pevný supply, rozpočet, doplňování, limity, rezervace, expirační pravidla) není rozhodnuta.
- Bonus CAP je odlišný od Central Wallet používaného pro zprostředkované platby.

### PAY-001 – Nákup Goins přes Stripe
- **Stav:** `CONFIRMED` jako požadavek, `OPEN` jako návrh řešení
- Uživatel má mít možnost koupit Goins přes Stripe.
- Podle současného popisu tato možnost ještě není implementována.
- Je potřeba navrhnout potvrzení platby, bezpečné webhooky, idempotenci, refundy, chargebacky a vazbu platby na připsané Paid Goins.
- Konkrétní Stripe produkt/služba (např. Stripe Connect) není rozhodnutá.

### PAY-002 – Cashout pro Player
- **Stav:** `CONFIRMED` jako požadavek
- Player má mít možnost vyplatit způsobilý zůstatek mimo platformu na svůj bankovní účet.
- Podle současného popisu cashout zatím není implementovaný.
- Způsobilost zůstatku, čekací lhůty, poplatky, minimální částka, ověření identity, výplaty a řešení sporů jsou otevřené.
- Není rozhodnuto, zda mohou být vypláceny pouze Paid Goins, nebo jiný typ zůstatku. Bonus Goins jsou zatím zamýšlené jako odměnová měna; možnost cashoutu není schválena.

### PAY-003 – Central Wallet pro zprostředkované transakce
- **Stav:** `CONFIRMED` jako záměr pro Ghost Mode, `OPEN` pro obecné platby
- Vlastník produktu předpokládá, že současný systém centrální platformní účet nemá.
- Je požadováno posoudit centrální účet pro zprostředkování plateb, zejména Ghost Mode.
- Tento účet se nesmí zaměnit s Bonus CAP.
- Právní a účetní model musí být navržen před použitím skutečných peněz.

---

## 12. Rewards & signup incentives

### REWARD-001 – Signup bonus po ověření e-mailu
- **Stav:** `CONFIRMED`
- Po úspěšném ověření e-mailu má uživatel získat jednorázový registrační bonus v Bonus Goins.

### REWARD-002 – Vyšší signup bonus po ověření Identitou občana
- **Stav:** `CONFIRMED` (princip), `OPEN` (výše a způsob doplatku)
- Po ověření přes Identitu občana má uživatel získat vyšší registrační bonus.
- Není rozhodnuto, zda se připíše pouze rozdíl oproti základní odměně, nebo další samostatná odměna.
- Konkrétní částky nejsou stanovené.
- Odměny mají být financované z Bonus CAP.
- Připsání musí být jednorázové i při opakovaných požadavcích/webhookích.

---

## 13. Player Dashboard & Analytics

### ANALYTICS-001 – Rozšíření Player sidebaru o Analytics
- **Stav:** `LATER`
- Do sidebaru Player dashboardu se má později přidat samostatná sekce/pohled Analytics.
- Stávající přehled pohybů zůstává samostatný.

### ANALYTICS-002 – Měsíční průměrné výdělky
- **Stav:** `LATER`
- Zobrazovat průměrné měsíční výdělky hráče.
- Definice výdělku (hrubý příjem, po provizi, pouze vypořádané platby) a období výpočtu zůstávají otevřené.

### ANALYTICS-003 – Statistiky jednotlivých služeb
- **Stav:** `LATER`
- Grafy a metriky relevantní pro služby, které daný Player poskytuje.
- Příklady požadovaných přehledů:
  - nejvýdělečnější služba,
  - výdělky podle jednotlivých služeb,
  - reklamovanost jednotlivých služeb.
- Přesná definice reklamace, období, minimálního počtu objednávek a způsobu započítání refundů je otevřená.

### ANALYTICS-004 – Doporučení pro hráče
- **Stav:** `LATER` (požadavek), `PROPOSED` (metoda)
- Dashboard má obsahovat prostor pro doporučení vycházející z dat o výkonu služeb.
- Příklady: upozornění na službu s vysokou reklamovaností nebo návrh zaměřit se na úspěšnější nabídku.
- Doporučení mají být vysvětlitelná a založená na dostatečném množství dat. AI generování není potvrzené.

---

## 14. Business, legal & launch readiness

### LEGAL-001 – Provoz bez IČO během vývoje
- **Stav:** `REVIEW`
- Byla vznesena otázka, zda lze Zagejmi začít provozovat bez IČO.
- Vývoj a interní testování je třeba odlišit od soustavného komerčního provozu marketplace.
- Před veřejným spuštěním placených služeb je nutné ověřit vhodnou podnikatelskou strukturu a povinnosti provozovatele.
- Tento dokument nepředstavuje právní stanovisko a nepovažuje dřívější orientační odpověď za právní potvrzení.

### LEGAL-002 – Platební/regulatorní model
- **Stav:** `REVIEW`
- Před ostrými platbami je potřeba posoudit právní povahu Paid Goins, předplacených zůstatků, převodů mezi uživateli, zprostředkování plateb, Central Wallet a cashoutu.
- Ověřit dopad pravidel pro platební služby a elektronické peníze a zda se uplatní konkrétní výjimka.
- Stripe jako poskytovatel infrastruktury sám o sobě automaticky neřeší regulatorní postavení Zagejmi.

### LEGAL-003 – DAC7 a platformní povinnosti
- **Stav:** `REVIEW`
- Prověřit, zda nabízené služby a role Zagejmi spadají do oznamovacích povinností digitálních platforem (DAC7) a jaké údaje by případně bylo potřeba sbírat a reportovat.

### LEGAL-004 – Ochrana osobních údajů a ověřování identity
- **Stav:** `REVIEW`
- Posoudit GDPR, právní titul, minimalizaci dat, retenční dobu a role poskytovatelů při ověřování dokladem/obličejem/Identitou občana.
- Ghost Mode nesmí být prezentován jako anonymita vůči platformě nebo zákonným povinnostem.

---

## 15. Otevřená rozhodnutí

Následující body vyžadují další produktový brainstorming nebo technickou/právní analýzu:

1. Může jeden účet později používat Player i Customer profil?
2. Které funkce vyžadují pouze login a které také ověřený e-mail?
3. Jak přesně se Ghost Mode projeví v profilu, presence, chatu, objednávkách a recenzích?
4. Má být Ghost Mode globální přepínač, nebo jej půjde použít jen pro konkrétní objednávku/platbu?
5. Jak se zachází s jinou nebo neuvedenou hodnotou pohlaví ve výchozím rozděleném zobrazení?
6. Jaké faktory a váhy budou určovat relevanci hráčů a služeb?
7. Podle čeho se počítá leaderboard?
8. Jaké jsou přesné atributy Service a jaké kategorie budou podporované?
9. Jaká pravidla platí pro útratu Paid versus Bonus Goins?
10. Jak se nastaví, čerpá a doplňuje Bonus CAP?
11. Jak se budou řešit refundy, chargebacky, spory a záporné zůstatky?
12. Jaký právní a účetní model bude mít Central Wallet?
13. Jaké Goins lze vyplatit hráči a za jakých podmínek?
14. Jaké budou částky signup bonusů a bude druhá úroveň doplatkem rozdílu?
15. Jaký minimální věk bude vyžadován u Customer účtů?
16. Které metody ověření věku/identity budou dostupné při spuštění?
17. Jak se budou definovat měsíční průměrné výdělky a reklamovanost v Analytics?
18. Jak bude vypadat první veřejná a placená release fáze a co do ní nepatří?

## 16. Doporučený další postup

1. Pokračovat v brainstormingu a průběžně doplňovat tento dokument.
2. U každého nového požadavku přidělit ID a stav.
3. Jakmile bude daná oblast dostatečně jasná, udělat audit současného kódu a datového modelu.
4. Rozdělit schválený rozsah na epics a implementační GitHub Issues s acceptance criteria.
5. Teprve po potvrzení závislostí a priorit sestavit časovou roadmapu.

**Nyní nevytvářet hromadně implementační issues.** Většina zde zapsaných bodů zatím představuje produktové požadavky, nikoliv hotové vývojové zadání.
