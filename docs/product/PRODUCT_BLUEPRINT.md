# Zagejmi – Product Blueprint

> **Typ dokumentu:** živý produktový discovery dokument  
> **Stav:** průběžný brainstorming; nejde o finální technickou specifikaci  
> **Poslední zachycení:** 2026-11-13 (doplněn audit kódu a rozhodnutí k MARKET-003)  
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

### PROFILE-003 – Upozornění na doplnění profilu
- **Stav:** `CONFIRMED` (záměr), `LATER`
- Hráče upozornit, že doplnění profilovky, banneru a obrázků služeb zlepší jeho pozici ve výsledcích (RANK-001).
- Upozornění má vycházet ze stejných vah jako řazení, aby hráči nejdřív doporučilo to, co pomůže nejvíc (profilovka > banner > obrázky služeb).
- Forma (banner v profilu, checklist, notifikace) a texty zatím nejsou navržené.

### PROFILE-004 – Vlastní plný profil hráče
- **Stav:** `CONFIRMED` (záměr), `OPEN` (rozsah a forma); zatím neimplementovat.
- Hráč má mít vlastní plnohodnotnou profilovou stránku, ne jen dnešní popup (detail hráče otevřený ze stránek Hráči a Služby).
- Na profilu si může napsat něco navíc o sobě: delší text „o mně“ nad rámec dnešního krátkého popisu.
- **Otevřené otázky:**
  - Adresa stránky (např. podle přezdívky, nebo podle ID) a co se stane po změně přezdívky.
  - Rozsah textu „o mně“ a jestli bude formátovaný (odstavce, odkazy, emoji) nebo prostý text.
  - Další obsah profilu (služby, hry a ranky, recenze, galerie, odkazy na sociální sítě, dostupnost) a jejich pořadí.
  - Vztah k popupu: jestli popup zůstane jako rychlý náhled s odkazem „Zobrazit celý profil“, nebo karty povedou rovnou na profil.
  - Moderace textů a obrázků a chování profilu v Ghost Mode (PRIVACY-001).
- **Souvislosti:** delší „o mně“ může být další položkou úplnosti profilu (RANK-001, PROFILE-003); o váze je potřeba rozhodnout.

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
- **Implementováno:** stránka Služby (`/services`) se všemi dostupnými službami všech hráčů; kliknutí otevře detail hráče. Řazení: nejdřív úplné profily a nabídky s obrázky, potom od nejnovějších (viz RANK-001).

### MARKET-002 – Výchozí rozdělení výsledků podle pohlaví
- **Stav:** `CONFIRMED` (výchozí UX), `OPEN` (okrajové případy a profilová data)
- Ve výchozím zobrazení mají být výsledky rozdělené do dvou skupin: ženy nahoře, muži níže.
- Uživatel může toto rozdělení vypnout a zobrazit společný seznam.
- Další filtry musí být použitelné jak při rozděleném, tak při společném zobrazení.
- Je nutné rozhodnout, jak zobrazit profily s jinou nebo neuvedenou hodnotou pohlaví; hodnota se nesmí odvozovat z fotografie ani odhadem.
- **Implementováno:** na stránkách Hráči i Služby jsou záložky **Přehled · Hráčky · Hráči · Ostatní · Všichni**, každá s počtem výsledků. Výchozí je Přehled se sekcemi Hráčky → Hráči → Ostatní; Všichni = jeden společný seznam. Použije se pouze pohlaví zvolené při registraci. Všechny filtry platí ve všech záložkách i v jejich počtech.
- **Rozvržení (rozhodnuto 2026-11-13):**
  - V přehledu má každá sekce nejvýše dvě řady karet (2–5 sloupců podle šířky obrazovky). Na mobilu jsou karty sekce ve vodorovném pásu, kterým se dá swipovat.
  - Nadpis sekce ukazuje celkový počet; odkaz „Zobrazit vše (N)“ přepne na záložku dané skupiny.
  - Záložka skupiny nebo Všichni ukáže úplný seznam se stránkováním „Načíst další“. Každá záložka má vlastní URL (`?gender=Women|Men|Others|All`), takže jde sdílet a funguje tlačítko zpět.
  - Carousel ani popup se pro „Zobrazit vše“ nepoužívá (dlouhý seznam, filtry a detail hráče by se v popupu nevešly); carousel případně později na úvodní stránce.
- **Návrh (`PROPOSED`, k potvrzení):** hodnoty „Jiné“ a „Nechci uvádět“ tvoří společnou třetí sekci „Ostatní“ pod muži. Volba přepínače se zatím nepamatuje (po načtení stránky je vždy zapnutý).

### MARKET-003 – Filtry discovery
- **Stav:** `CONFIRMED` (požadované typy filtrů), `OPEN` (konkrétní UI a datové možnosti)
- Filtry mají zahrnovat:
  - konkrétní službu,
  - kategorii,
  - videohru,
  - rank pro konkrétní hru,
  - kompetitivní versus nekompetitivní hraní.
- Filtry se mají dát kombinovat a fungovat v obou režimech zobrazení (rozdělený / společný seznam).
- **Rozhodnuto (2026-11-13):**
  - Hry a jejich ranky se vybírají z **předem připraveného seznamu** se známými ranky, nikoliv volným textem.
  - Kompetitivní / nekompetitivní hraní si **nastavuje Player u každé služby**: může nabízet jen jedno, nebo obojí.
- **Implementováno:** katalog her a ranků (`GameCatalog`), služba má hru, rank a styl hraní (kompetitivně / pro zábavu / obojí). Stránka Služby filtruje podle textu (název služby nebo hráče), hry, minimálního ranku („Gold a výš“) a stylu hraní.
- **Zatím chybí:** filtr podle kategorie (kategorie nejsou rozhodnuté), stejné filtry na stránce Hráči a rozdělení podle pohlaví (MARKET-002).
- **Návrh (`PROPOSED`, k potvrzení):**
  - Rank u služby znamená rank hráče v dané hře. Rozsah ranků pro služby typu boosting zatím není řešen.
  - Ranky jsou pouze úrovně (tiers) bez divizí (např. „Gold“, ne „Gold 2“).
  - Rank je nepovinný; hra bez ranků (např. Minecraft) rank nemá.
  - Služba nemusí být vázaná na hru (např. pokec); pak nemá rank ani styl hraní.
  - Počáteční seznam her a ranků je potřeba zkontrolovat a doplnit.

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
- **Potvrzený faktor (2026-11-13):** úplnost profilu. Hráči s profilovkou, bannerem a službami s obrázky mají být v pořadí preferováni.
- **Potvrzené pořadí důležitosti (2026-11-13):** profilovka (vysoko) > banner (vysoko) > obrázky služeb (až třetí).
- **Implementováno:** v každé sekci (i ve společném seznamu) se řadí podle skóre úplnosti, při shodě od nejnovějších.
  - Váhy: profilovka 4, banner 2, obrázek služby 1. Jsou to mocniny dvojky, takže důležitější prvek vždy převáží všechny méně důležité dohromady.
  - Hráči: obrázek služby = aspoň jedna dostupná služba s obrázkem.
  - Služby: profilovka a banner hráče, který službu nabízí, a obrázek dané služby.
  - Váhy jsou na jednom místě v kódu (`ProfileCompleteness`), aby je mohl použít i PROFILE-003.
- Další faktory relevance (hodnocení, dostupnost, …) zůstávají `PROPOSED`.

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

## 15. Audit kódu (2026-11-13)

Porovnání tohoto dokumentu s kódem na větvi `master`. Část požadavků je hotová dříve, než je dokument popisoval:

| Požadavek | Stav v kódu |
|---|---|
| AUTH-002 Ověření e-mailu | Hotovo: odkaz po registraci, opakované zaslání, login vyžaduje ověřený e-mail. |
| REWARD-001 Signup bonus | Hotovo: 50 Bonus Goins po ověření e-mailu (konfigurovatelné). |
| WALLET-002 Paid vs. Bonus | Hotovo: každý zápis v ledgeru je označen Paid/Bonus. |
| WALLET-003 Bonus CAP | Částečně: systémový účet `PromoBudget`; bonusy expirují po 90 dnech a zbytek se vrací do rozpočtu. |
| PAY-003 Central Wallet | Částečně: existují systémové účty `Escrow`, `TopUpClearing`, `PayoutClearing`, zatím se nepoužívají. |
| PRIVACY-001 Ghost Mode | Částečně: přepínač a serverový filtr blokující akce v Ghost Mode. |
| AUTH-004 / PROFILE-001 | Jeden účet může mít Player i Customer profil a přepínat mezi nimi (odpovídá na otevřenou otázku 1). |
| MARKET-001 / MARKET-003 | Částečně: stránka Služby s filtry hra, minimální rank, styl hraní a text (viz MARKET-003). |
| MARKET-002 | Hotovo s návrhem pro „Jiné“ / „Nechci uvádět“ (viz MARKET-002). |

Zatím chybí: objednávky, filtry hra/rank/styl na stránce Hráči, nákup Goinů (Stripe), cashout.

## 16. Otevřená rozhodnutí

Následující body vyžadují další produktový brainstorming nebo technickou/právní analýzu:

1. ~~Může jeden účet později používat Player i Customer profil?~~ Ano, už je implementováno (viz audit).
2. Které funkce vyžadují pouze login a které také ověřený e-mail?
3. Jak přesně se Ghost Mode projeví v profilu, presence, chatu, objednávkách a recenzích?
4. Má být Ghost Mode globální přepínač, nebo jej půjde použít jen pro konkrétní objednávku/platbu?
5. Jak se zachází s jinou nebo neuvedenou hodnotou pohlaví ve výchozím rozděleném zobrazení?
6. Jaké faktory a váhy budou určovat relevanci hráčů a služeb?
7. Podle čeho se počítá leaderboard?
8. Jaké jsou přesné atributy Service a jaké kategorie budou podporované? (Hra, rank a styl hraní jsou rozhodnuté; kategorie zůstávají otevřené.)
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
19. Jak bude vypadat plný profil hráče (PROFILE-004): adresa, obsah, formátování textu „o mně“ a vztah k dnešnímu popupu?

## 17. Doporučený další postup

1. Pokračovat v brainstormingu a průběžně doplňovat tento dokument.
2. U každého nového požadavku přidělit ID a stav.
3. Jakmile bude daná oblast dostatečně jasná, udělat audit současného kódu a datového modelu.
4. Rozdělit schválený rozsah na epics a implementační GitHub Issues s acceptance criteria.
5. Teprve po potvrzení závislostí a priorit sestavit časovou roadmapu.

**Nyní nevytvářet hromadně implementační issues.** Většina zde zapsaných bodů zatím představuje produktové požadavky, nikoliv hotové vývojové zadání.
