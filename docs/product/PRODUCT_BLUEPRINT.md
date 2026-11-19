# Zagejmi – Product Blueprint

> **Typ dokumentu:** živý produktový discovery dokument  
> **Stav:** průběžný brainstorming; nejde o finální technickou specifikaci  
> **Poslední zachycení:** 2026-11-19 (první vydání bude se skutečnými penězi, viz [Specifikace prvního vydání](./FIRST_RELEASE_SPEC.md))  
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
- **Objednávka (Order)** – zakoupení služby zákazníkem; cena leží do dokončení v **Escrow** (systémový účet platformy).
- **Recenze služby** – hodnocení jedné dokončené objednávky (1–5 hvězdiček + text).
- **Doporučení** – jedno hodnocení hráče od zákazníka (1–5 hvězdiček + text), až po dokončené objednávce u něj.

Požadavky používají stavy definované v [README](./README.md).

## 3. Přehled funkčních oblastí

| Oblast | Aktuální obraz |
|---|---|
| Authentication | Registrace, login a ověření e-mailu existují (AUTH-002). |
| Profiles | Jeden účet může mít profil Zákazník i Hráč a přepínat mezi nimi; hráč vypisuje služby (PROFILE-002). |
| Privacy | Ghost Mode: přepínač existuje a blokuje akce (objednávky, recenze); zbytek rozsahu je otevřený (PRIVACY-001). |
| Verification | Ověření věku/identity je plánované do budoucna. |
| Marketplace | Hráči i Služby jsou v navigaci; služby jdou objednat z profilu hráče. |
| Wallet | Ledger s oddělenými Paid/Bonus Goins, bonus po registraci, Escrow pro objednávky (viz WALLET-002, ORDER-001). |
| Orders & reviews | Objednávky s Escrow, lhůtami a řešením problémů; recenze služeb a doporučení hráčů (sekce 12). |
| Payments | Nákup Goinů přes Stripe a cashout hráčů podle vlastníka produktu zatím nejsou k dispozici. |
| Analytics | Rozšířené Player Analytics jsou plánované později. |
| UI | Web má fungovat při jakékoli šířce okna; horní lišta je opravená, ostatní stránky zbývá projít (UI-001). |
| Provoz | Produkce na Hetzneru (zagejmi.cz); e-maily přes SMTP relay, vlastní mail server připravený (OPS-001). **Produkce zatím neběží na aktuálním `master`**, viz audit (sekce 17). |
| Legal & launch | Podnikatelský, platební a regulatorní model musí být vyhodnocen před ostrým komerčním spuštěním. |

---

## 4. Authentication & access control

### AUTH-001 – Základní registrace a přihlášení
- **Stav:** `EXISTING`
- Základní registrace a login jsou již implementované.
- Před dalšími změnami je nutné ověřit jejich současné chování a datový model v kódu.
- **Odhlášení (opraveno 2026-11-15):** ikona odhlášení vede na `/logout`, který smaže tokeny v prohlížeči a přejde na
  úvodní stránku; funguje z každé stránky včetně chráněných (dřív odhlášení z např. `/profile` skončilo na přihlášení
  a samotná stránka `/logout` tokeny při prerenderu nesmazala).
- **Chybí (`CONFIRMED` jako bezpečnostní potřeba):** server tokeny při odhlášení nezneplatní; zkopírovaný token platí
  do vypršení (přístupový 2 h, obnovovací 30 dní). Issue #32.

### AUTH-002 – Ověření e-mailu
- **Stav:** `EXISTING` (ověření), `OPEN` (zásady pro neověřené účty)
- Po registraci má uživatel ověřit vlastnictví e-mailové adresy.
- **Implementováno:** po registraci přijde odkaz s jednorázovým tokenem platným 24 h; nový odkaz jde poslat znovu
  nejdřív po 1 minutě a starší odkazy tím přestanou platit. Přihlásit se jde až s ověřeným e-mailem; po ověření
  přijde signup bonus (REWARD-001).
- Zásady pro neověřené účty nad rámec „nepřihlásí se“ zůstávají otevřené (otevřené rozhodnutí 2).

### AUTH-003 – Veřejné prohlížení a přihlášení pro používání
- **Stav:** `CONFIRMED`
- Nepřihlášený návštěvník smí prohlížet veřejný obsah marketplace.
- Pro použití funkcí platformy, zejména akcí jako objednávání, musí být uživatel přihlášený.
- Soukromé údaje uživatelů nesmějí být dostupné nepřihlášeným návštěvníkům.
- Ochrana musí být vynucena serverově/API, nikoliv pouze skrytím prvků ve frontendu.
- Přihlášení samo o sobě nedává přístup k soukromým údajům ostatních uživatelů; je nutná autorizace podle vztahu a oprávnění.

### AUTH-004 – Typ profilu při začátku používání
- **Stav:** `EXISTING`
- Každý účet má od registrace profil Zákazník; profil Hráč si může kdykoli založit v Profilu.
- Účet s oběma profily mezi nimi přepíná přepínačem v horní liště a může si nastavit, který je výchozí.

## 5. E-mailová infrastruktura

### OPS-001 – Oddělené SMTP prostředí
- **Stav:** `CONFIRMED`
- Development/debug: Mailpit.
- Současná produkce: existující SMTP relay vlastníka projektu.
- Pozdější produkce: vlastní mail server po uplynutí prvního měsíce provozu Hetzner serveru.
  - **Připraveno (2026-11-15), zatím vypnuté:** Stalwart (SMTP, IMAP, DKIM, spam filtr, webová administrace) jako služba
    `mail` v produkčním compose pro `@zagejmi.cz`, se schránkami `noreply@` (odesílá aplikace) a `postmaster@`, další
    schránky přes webovou administraci. Zapnutí, DNS záznamy a přepnutí aplikace (oběma směry jedním příkazem) popisuje
    `deploy/MAIL.md`; vyzkoušeno lokálně včetně ověření TLS certifikátu a podpisu DKIM.
  - Čeká se na Hetzner: odblokování odchozího portu 25 (a 465) a reverzní DNS `mx.zagejmi.cz`.
- Aplikační logika odesílání e-mailů má být nezávislá na konkrétním SMTP provideru (MailKit; STARTTLS na 587 i TLS
  hned od spojení na 465, nastavení `Smtp:Security`).
- Konkrétní údaje relay serveru ani tajné hodnoty nepatří do dokumentace v repozitáři.

---

## 6. Player & Customer profiles

### PROFILE-001 – Player a Customer profily
- **Stav:** `EXISTING`
- Základní typy profilu jsou Player a Customer; jeden účet může mít oba (AUTH-004).
- Zákazník a hráč mají oddělené peněženky (WALLET-002) a v UI rodově správné názvy (Zákazník/Zákaznice, Hráč/Hráčka).

### PROFILE-002 – Player nabízí vlastní služby
- **Stav:** `EXISTING` (základ), `OPEN` (kategorie, dostupnost)
- Player má mít možnost vypisovat služby, které poskytuje.
- Zákazník může najít hráče přes jeho profil nebo hledat přímo službu.
- **Implementováno:** služba je samostatná entita profilu hráče: název, popis, cena v Goinech za jednotku, jednotka,
  obrázek, hra, rank a styl hraní (MARKET-003), dostupnost zapnuto/vypnuto a nepovinná kategorie jako volný text
  (zobrazí se na kartě). Hráč je spravuje v Profilu.
- Pevný seznam kategorií (a filtr podle nich) a dostupnost v čase (kalendář) zatím nejsou (otevřené rozhodnutí 8).

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
- Rozhodnuto: v Ghost Mode nejde objednávat, měnit objednávky ani psát/mazat recenze a doporučení (jen číst).
- Přesné chování pro online status, profil, aktivitu, chat a historii je nutné upřesnit.
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
- **Stav:** `CONFIRMED` (2026-11-19), `REVIEW` z hlediska právního základu
- Zákazníkovi musí být aspoň 18 let. V prvním vydání to potvrdí zaškrtnutím při registraci (bez ověření dokladem);
  existující účty potvrzení udělají před prvním nákupem Goinů. Ověření dokladem je pozdější krok (VERIFY-001).

---

### VERIFY-004 – Integrace s Národní identitou (Identita občana)
- **Stav:** `LATER` (dlouhodobý produktový záměr), `REVIEW` (technická, právní a procesní proveditelnost)
- Zagejmi by mělo do budoucna podporovat integraci s českým systémem **Národní identita / Identita občana** jako součást širší identity a ověřovací infrastruktury platformy.
- Integraci chápat jako možnost pro uživatele, nikoliv jako povinný způsob registrace nebo přihlášení. Běžné účty a jiné podporované metody autentizace mají zůstat dostupné.
- Podle možností oficiálního rozhraní a schváleného integračního modelu zvážit:
  - přihlášení nebo propojení existujícího účtu Zagejmi prostřednictvím Identity občana;
  - ověření identity uživatele a případně ověření konkrétních atributů, například věku, pokud je daný atribut poskytován a jeho použití je oprávněné;
  - opětovné ověření při citlivých nebo regulovaných operacích, pokud to bude přiměřené a podporované.
- Rozlišovat autentizaci (potvrzení, kdo se přihlašuje), identifikaci (zjištění konkrétní identity) a ověření jednotlivých atributů (například dosažení věkové hranice). Úspěšné přihlášení přes Národní identitu automaticky neznamená, že Zagejmi získalo nebo smí uchovávat všechny osobní údaje uživatele.
- Preferovat předávání a ukládání pouze minimálního výsledku a nezbytných metadat ověření; neukládat kopie dokladů ani údaje, které platforma nepotřebuje.
- Zachovat jasnou vazbu mezi ověřenou identitou a účtem Zagejmi, včetně bezpečného procesu prvního propojení, opakovaného přihlášení, změny či odpojení identity a řešení duplicitních účtů.
- Integraci navrhnout tak, aby respektovala souhlas a informovanost uživatele, ochranu osobních údajů, účelové omezení, retenční pravidla a bezpečnost autentizačního toku.
- Vyšší registrační bonus po ověření Identitou občana (REWARD-002) je samostatná produktová motivace; nesmí být jediným důvodem ani skrytým předpokladem pro samotnou integraci.
- Konkrétní poskytovatel/rozhraní, podmínky přístupu, úrovně záruky identity, dostupné atributy, technický protokol, poplatky a právní základ zůstávají otevřené. Před implementací ověřit aktuální oficiální integrační možnosti Národní identity a požadavky na provozovatele služby.

## 9. GitHub integration and cross-entity linking

### GITHUB-001 – Přehled připojených GitHub repozitářů
- **Stav:** `CONFIRMED` (produktový záměr), `LATER` (plná integrace), `REVIEW` (rozsah oprávnění a technické řešení)
- Uživatel má mít možnost propojit svůj GitHub účet a sledovat všechny repozitáře, ke kterým dané propojení získá oprávnění. Neomezovat přehled pouze na repozitáře organizace Zagejmi.
- Umožnit přehled repozitářů a jejich aktuálního stavu, včetně dostupných informací o aktivitě, posledních změnách a upozorněních.
- U každého repozitáře zobrazovat přímý odkaz na GitHub a umožnit otevřít jeho detail.
- Rozsah viditelných repozitářů musí odpovídat oprávněním uděleným uživatelem a GitHubem; soukromé repozitáře ani jejich metadata se nesmějí zpřístupnit ostatním uživatelům bez výslovného oprávnění.
- Připojení a oprávnění musí být odvolatelné. Uživatel má mít přehled o tom, jaké účty a repozitáře jsou propojené a jaká oprávnění integrace používá.

### GITHUB-002 – Sledování GitHub objektů a jejich stavu
- **Stav:** `CONFIRMED` (produktový záměr), `LATER`
- U připojených repozitářů sledovat podle dostupných oprávnění zejména:
  - Issues včetně stavu, assignee, labels, milestones, komentářů a termínů, pokud jsou dostupné;
  - Pull requests včetně review, approvals, změn stavu a merge;
  - commity, branche a související aktivitu;
  - GitHub Actions workflows a jejich běhy, výsledky, chyby a odkazy na logy;
  - releases, deployments a další relevantní události, pokud je uživatel povolí.
- Každý importovaný objekt musí zachovat stabilní externí identifikátor, zdrojový repozitář, poslední známý stav a přímý odkaz na původní objekt na GitHubu.
- Stav synchronizovat průběžně prostřednictvím podporovaných webhooků a doplňkového periodického ověřování; počítat s opakováním, zpožděním a změnami událostí mimo pořadí.
- Rozlišovat data načtená z GitHubu od lokálních úkolů a ručních poznámek. Uživatel musí poznat, kde je zdroj pravdy a zda je údaj aktuální.
- Nevyvozovat automaticky, že uzavřené issue znamená dokončený libovolný úkol nebo projekt; mapování stavů a pravidla synchronizace musí být explicitní.

### LINK-001 – Propojený model objektů napříč Zagejmi
- **Stav:** `CONFIRMED` (produktový záměr), `LATER` (postupná realizace), `OPEN` (datový model a pravidla vztahů)
- Zagejmi má umožnit vzájemně propojovat relevantní objekty napříč celou aplikací, nikoliv je držet jako izolované moduly.
- Propojitelné objekty mají zahrnovat zejména:
  - GitHub repozitáře, issues, pull requests, commity, branche a CI/CD běhy;
  - interní projekty, úkoly, podúkoly, milníky a plánované pracovní bloky;
  - kalendářní události, termíny, připomínky a osobní plán;
  - uživatelské a osobní profily, osoby, spolupracovníky a relevantní vztahy;
  - další budoucí entity, například dokumenty, poznámky, konverzace nebo objednávky, pokud to dává smysl.
- Jeden objekt může být propojen s více objekty a vztahy mohou mít různé významy (například „souvisí s“, „realizuje“, „blokuje“, „patří do“, „řeší“ nebo „účastní se“). Vztahy mají být dohledatelné z obou stran.
- Zachovat přímé odkazy na externí zdroje a interní stabilní ID; nevytvářet nekontrolované kopie stejného objektu v různých modulech.
- Uživatel musí mít možnost vztahy ručně vytvořit, upravit a odstranit. Automatické návrhy vztahů musí být vysvětlitelné a potvrditelné.

### AI-001 – AI jako kontextový koordinátor napříč aplikací
- **Stav:** `CONFIRMED` (produktový záměr), `LATER` (implementace), `REVIEW` (oprávnění, ochrana dat a bezpečnost)
- AI má umět pracovat s propojeným kontextem napříč kalendářem, úkoly, projekty, GitHubem, osobními profily a dalšími částmi Zagejmi, pokud k nim uživatel udělí přístup.
- Má umět rozpoznávat souvislosti a navrhovat párování, například:
  - GitHub issue ↔ interní úkol nebo podúkol;
  - pull request / commit ↔ úkol a projekt;
  - GitHub milestone nebo release ↔ projektový milník a kalendářní termín;
  - CI/CD selhání ↔ související úkol, projekt a odpovědná osoba;
  - schůzka nebo událost ↔ projekt, úkoly a účastníci;
  - osoba z profilu ↔ spolupracovník, autor změny nebo účastník související aktivity, pokud je vazba doložitelná.
- AI má umět využít tyto vazby při plánování kalendáře, prioritizaci úkolů, přípravě na schůzky, shrnutí stavu projektů a upozorňování na blokery nebo blížící se termíny.
- Návrhy vazeb musí obsahovat důvod a odkazy na zdrojové objekty. Nejednoznačné shody mají být předloženy uživateli k potvrzení; AI nesmí domýšlet identitu osoby ani automaticky slučovat různé objekty pouze podle podobného názvu.
- Automatické vytváření nebo změna úkolů, termínů, issue, PR a dalších objektů podléhá samostatným oprávněním a nastavení potvrzování. Čtení dat nesmí automaticky znamenat právo je měnit.
- AI smí vyhledávat pouze v datech, která smí aktuální uživatel vidět. Oprávnění se musí kontrolovat i při přístupu přes propojené objekty, vyhledávání, souhrny a AI nástroje.
- Citlivé nebo soukromé osobní profily a jejich části se AI zpřístupní pouze na základě samostatného výslovného oprávnění. Přístup k GitHubu nesmí sám o sobě udělit přístup k osobním údajům.
- Zaznamenávat původ použitých informací a provedené AI akce tak, aby šlo dohledat, z čeho doporučení vycházelo a co AI změnila.

## 9. Marketplace discovery – Players & Services

### MARKET-001 – Samostatná navigace Hráči a Služby
- **Stav:** `CONFIRMED`
- Navigace má obsahovat dvě samostatné sekce:
  - **Hráči:** zákazník hledá konkrétního člověka a prohlíží jeho profil a nabídky.
  - **Služby:** zákazník hledá konkrétní službu bez nutnosti začít výběrem konkrétního hráče.
- Obě cesty mají pracovat se stejnými hráči a jejich nabídkami; nejde o dva nezávislé marketplace katalogy.
- **Implementováno:** stránka Služby (`/services`) se všemi dostupnými službami všech hráčů; kliknutí otevře detail hráče. Řazení: nejdřív úplné profily a nabídky s obrázky, potom od nejnovějších (viz RANK-001).
- Úvodní stránka má sekci „Doporučujeme“: první čtyři hráče ve stejném pořadí jako na stránce Hráči.

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
- **Návrh (`PROPOSED`, k potvrzení):** hodnoty „Jiné“ a „Nechci uvádět“ tvoří společnou třetí sekci „Ostatní“ pod muži. Zvolená záložka je jen v adrese, po novém otevření stránky je výchozí Přehled.

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
- **Zatím chybí:** filtr podle kategorie (kategorie nejsou rozhodnuté) a filtry hra/rank/styl na stránce Hráči (ta má jen hledání podle přezdívky). Rozdělení podle pohlaví funguje na obou stránkách (MARKET-002).
- **Návrh (`PROPOSED`, k potvrzení):**
  - Rank u služby znamená rank hráče v dané hře. Rozsah ranků pro služby typu boosting zatím není řešen.
  - Ranky jsou pouze úrovně (tiers) bez divizí (např. „Gold“, ne „Gold 2“).
  - Rank je nepovinný; hra bez ranků (např. Minecraft) rank nemá.
  - Služba nemusí být vázaná na hru (např. pokec); pak nemá rank ani styl hraní.
  - Počáteční seznam her a ranků je potřeba zkontrolovat a doplnit.

### MARKET-004 – Samostatné služby a jejich atributy
- **Stav:** `CONFIRMED` (potřeba samostatné sekce Služby), `OPEN` (detail katalogu)
- Nabídka služby je spojena s Playerem.
- Objednávka kupuje 1–10 jednotek služby (cena × počet); služba s cenou 0 jde objednat zdarma.
- Konkrétní kategorie, atributy, pravidla pro hry/ranky, cenotvorba a dostupnost se doplní při detailním návrhu.

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
- Další faktory relevance (hodnocení, dostupnost, …) zůstávají `PROPOSED`. Hodnocení (REVIEW-001/002) se na kartách
  ukazuje, ale **řazení zatím neovlivňuje**.

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
- **Rozhodnuto (2026-11-13/14):**
  - Zákazník má oddělené Paid a Bonus části peněženky, hráč vlastní peněženku výdělků. Převody mezi uživateli nejsou.
  - Při platbě se nejdřív spotřebují Bonus Goins (od nejdřív expirujícího bonusu), potom Paid Goins.
  - Bonus Goins expirují 90 dní po připsání; nevyčerpaný zbytek se vrací do `PromoBudget`.
  - Hráč dostává výplatu vždy v Paid Goins; část objednávky zaplacenou bonusem pokryje `PromoBudget`.
  - Refund vrací každou část tam, odkud přišla (bonus do stejného bonusu); bonus, který mezitím vypršel, dostane 7 dní navíc.
- Cashout a převod výdělků hráče do vlastní zákaznické peněženky zůstávají otevřené (PAY-002).

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
- **Rozhodnuto (2026-11-19):** výdělek jde vybrat 14 dní po dokončení objednávky; minimální výběr 500 Goins.
- Poplatky, ověření identity, způsob výplat a řešení sporů jsou otevřené (návrh ve [Specifikaci prvního vydání](./FIRST_RELEASE_SPEC.md)).
- Není rozhodnuto, zda mohou být vypláceny pouze Paid Goins, nebo jiný typ zůstatku. Bonus Goins jsou zatím zamýšlené jako odměnová měna; možnost cashoutu není schválena.

### PAY-004 – Refund nákupu a chargeback
- **Stav:** `CONFIRMED` jako potřeba (první vydání je se skutečnými penězi), `PROPOSED` jako řešení
- Nákup Goinů jde vrátit a karetní spor (chargeback) musí mít v ledgeru jasný dopad, i když zákazník Goins už utratil.
- Návrh pravidel a ledgerových pohybů je ve [Specifikaci prvního vydání](./FIRST_RELEASE_SPEC.md#pay-004--refund-a-chargeback-nový-požadavek-proposed).

### PAY-003 – Central Wallet pro zprostředkované transakce
- **Stav:** `CONFIRMED` jako záměr pro Ghost Mode, `OPEN` pro obecné platby
- Systémový účet `Escrow` už drží Goins otevřených objednávek (ORDER-001); obecný model pro skutečné peníze zůstává otevřený.
- Je požadováno posoudit centrální účet pro zprostředkování plateb, zejména Ghost Mode.
- Tento účet se nesmí zaměnit s Bonus CAP.
- Právní a účetní model musí být navržen před použitím skutečných peněz.

---

## 12. Orders & reviews

### ORDER-001 – Objednávka služby s Escrow
- **Stav:** `EXISTING` (2026-11-14)
- Přihlášený zákazník (ne v Ghost Mode, ne vlastní služba) objedná 1–10 jednotek služby z profilu hráče. Celá cena
  odejde z jeho peněženky do `Escrow` (bonus první, viz WALLET-002). Název, jednotka a cena se do objednávky kopírují.
- Průběh: **čeká na přijetí** → hráč přijme (nebo odmítne = plný refund) → **probíhá** → hráč ukončí s výsledkem
  „služba proběhla“ / „zákazník se nedostavil“ → **čeká na potvrzení** → zákazník potvrdí → **dokončeno**.
- Lhůty: nepřijatá objednávka se po 48 h zruší a vrátí; nepotvrzená se 72 h po ukončení dokončí sama.
- Zákazník může objednávku zrušit, dokud ji hráč nepřijme (plný refund).
- Při dokončení dostane hráč cenu minus poplatek platformy: 10 % zaokrouhleno dolů, min. 1 Goin (nastavitelné).
- Když se zákazník nedostaví, hráč je zaplacen (rezervoval čas); zákazník to může do 72 h nahlásit.
- Souběžné akce (např. přijetí vs. zrušení) nemohou vyhrát obě; změna stavu a pohyb Goinů se ukládají naráz.

### ORDER-002 – Přehled objednávek
- **Stav:** `EXISTING`
- `/orders` (moje objednávky) a `/orders/incoming` (objednávky mých služeb) s aktuálním stavem, další lhůtou a jen
  těmi akcemi, které daná strana právě smí.

### ORDER-003 – Nahlášení problému a rozhodnutí administrátora
- **Stav:** `EXISTING` (minimální verze)
- Zákazník může probíhající nebo ukončenou objednávku nahlásit s popisem; Goins zůstanou v Escrow, dokud nerozhodne
  administrátor.
- Administrátor (ověřený účet s e-mailem v nastavení `Admin:Emails`) na `/admin/orders` buď vrátí Goins zákazníkovi,
  nebo vyplatí hráče. Role administrátora v databázi zatím neexistuje.
- Otevřené: lhůta pro rozhodnutí, komunikace se stranami, opakované nahlašování, sankce.

### REVIEW-001 – Doporučení hráče
- **Stav:** `EXISTING`
- Zákazník může každého hráče doporučit jednou (1–5 hvězdiček + nepovinný text do 1000 znaků), až po aspoň jedné
  dokončené objednávce u něj, kde služba proběhla. Doporučení může upravit nebo smazat jen autor; hráč ne.
- Průměr doporučení se na kartách hráčů (Hráči, úvodní stránka) ukáže až od 5 doporučení.

### REVIEW-002 – Recenze služby po každé dokončené objednávce
- **Stav:** `EXISTING`
- Každou dokončenou objednávku, kde služba proběhla, může zákazník ohodnotit jednou (1–5 hvězdiček + text), takže
  stálý zákazník hodnotí každou objednávku zvlášť. Úprava a smazání jen autorem; nedostavení se recenzi neodemyká.
- Recenze jsou vidět na profilu hráče i s názvem služby; zůstanou i po smazání služby.
- Průměr služby se na kartách (Služby, služby v profilu hráče) ukáže až od 5 recenzí (`Reviews:MinReviewsToShowRating`),
  aby kartu neurčila jediná recenze.
- Otevřené: odpověď hráče na recenzi, nahlašování nevhodných recenzí, vliv hodnocení na řazení (RANK-001).

---

## 13. Rewards & signup incentives

### REWARD-001 – Signup bonus po ověření e-mailu
- **Stav:** `EXISTING`
- Po úspěšném ověření e-mailu má uživatel získat jednorázový registrační bonus v Bonus Goins.
- **Implementováno:** 50 Bonus Goins na zákaznický účet z `PromoBudget`, platnost 90 dní (`Wallets:SignUpBonus`,
  `Wallets:BonusLifetime`); připíše se jen jednou.

### REWARD-002 – Vyšší signup bonus po ověření Identitou občana
- **Stav:** `CONFIRMED` (princip), `OPEN` (výše a způsob doplatku)
- Po ověření přes Identitu občana má uživatel získat vyšší registrační bonus.
- Není rozhodnuto, zda se připíše pouze rozdíl oproti základní odměně, nebo další samostatná odměna.
- Konkrétní částky nejsou stanovené.
- Odměny mají být financované z Bonus CAP.
- Připsání musí být jednorázové i při opakovaných požadavcích/webhookích.

---

## 14. Player Dashboard & Analytics

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

## 15. UI, responzivita a přístupnost

### UI-001 – Web funguje při jakékoli šířce okna
- **Stav:** `CONFIRMED`
- Celý web musí fungovat na mobilu, tabletu i desktopu a při změně velikosti okna; prvky se nesmějí překrývat ani slévat dohromady.
- Na užší obrazovce se obsah má přeskládat (zalomit, schovat do menu, posouvat), ne překrýt.
- **Zjištěný stav (2026-11-13):** vlastník produktu hlásí, že se při změně velikosti některé prvky slévají dohromady.
  - Pravděpodobně hlavní místo: horní lišta. Navigace uprostřed (Domů, Hráči, Služby, Peněženka, Profil) se zobrazí od šířky 768 px a na středních šířkách se nevejde vedle přepínače profilu, zůstatku Goinů, jména a tlačítka Odhlásit se. Přidání odkazu Služby (MARKET-001) tento problém zhoršilo.
  - Pod 768 px není hlavní navigace dostupná jinak než přes postranní panel; je potřeba ověřit, že se dá na mobilu dostat na všechny stránky.
  - Ostatní stránky (Profil, Peněženka, formuláře, popupy) zatím nebyly systematicky zkontrolovány.
- **Opraveno (2026-11-13), ověřeno screenshoty v Chrome v šířkách 360–1536 px:**
  - Horní lišta: hlavní navigace je v liště od 1024 px, pod tím v panelu „Menu“; jméno uživatele od 1536 px; „Odhlásit se“ je od 1024 px ikona s popiskem (2026-11-14; pod 1024 px je jako text v panelu Menu); pod 640 px je přepínač profilu v panelu Menu. Lišta je flex, takže pravá část už nepřetéká přes navigaci.
  - Přepínač profilu (Zákazník/Hráč) se už nikdy nezužuje (dřív se v šířkách 1024–1180 px slil do sebe, issue #27): obě poloviny jsou stejně široké jako delší popisek. Aby se vešel vedle navigace, v šířkách 1024–1279 px se schová text „Zagejmi“ u loga a odkazy v navigaci mají menší odsazení (2026-11-15, ověřeno v šířkách 640–1920 px pro Zákazník/Hráč i Zákaznice/Hráčka).
  - Postranní panel: od 1024 px sloupec vedle obsahu jako dřív; pod 1024 px je ve výchozím stavu zavřený a vysouvá se přes obsah, po navigaci se zavře.
  - Odsazení obsahu se zmenšuje podle šířky obrazovky.
- **Zbývá (`PROPOSED`):**
  - Projít zbývající stránky (Profil, Peněženka, Domů, přihlášení a registrace, popupy) ve stejných šířkách a sepsat konkrétní chyby.
  - Kontrolu šířek přidat do postupu před každým PR, který mění UI.

---

## 16. Business, legal & launch readiness

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

## 17. Audit kódu (2026-11-13, aktualizováno 2026-11-15)

Porovnání tohoto dokumentu s kódem na větvi `master`. Část požadavků je hotová dříve, než je dokument popisoval.

**`master` ≠ produkce.** Na zagejmi.cz běží verze z PR #19 (2026-11-14). Od té doby je v `master`, ale **ještě není
nasazené**: objednávky (ORDER-001–003), recenze a doporučení (REVIEW-001/002), ikona odhlášení, oprava přepínače
profilu (UI-001), přesměrování na HTTPS jen při vývoji, MailKit a připravený mail server (OPS-001). Před nasazením
nastavit `ADMIN_EMAIL` na serveru (deploy/README.md), jinak nahlášené objednávky nemá kdo rozhodnout.

| Požadavek | Stav v kódu |
|---|---|
| AUTH-002 Ověření e-mailu | Hotovo: odkaz po registraci, opakované zaslání, login vyžaduje ověřený e-mail. |
| REWARD-001 Signup bonus | Hotovo: 50 Bonus Goins po ověření e-mailu (konfigurovatelné). |
| WALLET-002 Paid vs. Bonus | Hotovo: každý zápis v ledgeru je označen Paid/Bonus. |
| WALLET-003 Bonus CAP | Částečně: systémový účet `PromoBudget`; bonusy expirují po 90 dnech a zbytek se vrací do rozpočtu. |
| PAY-003 Central Wallet | Částečně: `Escrow` drží Goins otevřených objednávek; `TopUpClearing` a `PayoutClearing` čekají na nákup a cashout. |
| ORDER-001–003 Objednávky | Hotovo (2026-11-14): Escrow, lhůty 48 h / 72 h, poplatek 10 %, nahlášení a rozhodnutí administrátora. |
| REVIEW-001/002 Recenze | Hotovo (2026-11-14): recenze každé dokončené objednávky, doporučení po první objednávce, průměr od 5 recenzí. |
| PRIVACY-001 Ghost Mode | Částečně: přepínač a serverový filtr blokující akce v Ghost Mode. |
| AUTH-004 / PROFILE-001 | Jeden účet může mít Player i Customer profil a přepínat mezi nimi (odpovídá na otevřenou otázku 1). |
| MARKET-001 / MARKET-003 | Částečně: stránka Služby s filtry hra, minimální rank, styl hraní a text (viz MARKET-003). |
| MARKET-002 | Hotovo s návrhem pro „Jiné“ / „Nechci uvádět“ (viz MARKET-002). |
| PROFILE-002 Služby | Hotovo (základ): služba s cenou, jednotkou, obrázkem, hrou, rankem a stylem hraní; kategorie jen jako volný text. |
| OPS-001 E-maily | Relay v provozu; vlastní mail server (Stalwart) připravený a vypnutý, čeká na Hetzner (deploy/MAIL.md). |
| UI-001 | Horní lišta opravená (včetně přepínače profilu, issue #27); ostatní stránky zbývá projít. |

Zatím chybí: nákup Goinů (Stripe), takže zákazníci zatím utrácí jen bonus z registrace; cashout a převod výdělků hráče;
filtry hra/rank/styl na stránce Hráči; role administrátora v databázi; vliv hodnocení na řazení; odpověď hráče na
recenzi a nahlašování recenzí; kategorie služeb; leaderboard; plný profil hráče (PROFILE-004); Analytics.
Otevřené GitHub issues: #1 (ikona webu ve správné velikosti).

## 18. Otevřená rozhodnutí

Následující body vyžadují další produktový brainstorming nebo technickou/právní analýzu:

1. ~~Může jeden účet později používat Player i Customer profil?~~ Ano, už je implementováno (viz audit).
2. Které funkce vyžadují pouze login a které také ověřený e-mail?
3. Jak přesně se Ghost Mode projeví v profilu, presence a chatu? (Objednávky a recenze: v Ghost Mode jen čtení.)
4. Má být Ghost Mode globální přepínač, nebo jej půjde použít jen pro konkrétní objednávku/platbu?
5. Jak se zachází s jinou nebo neuvedenou hodnotou pohlaví ve výchozím rozděleném zobrazení?
6. Jaké faktory a váhy budou určovat relevanci hráčů a služeb?
7. Podle čeho se počítá leaderboard?
8. Jaké jsou přesné atributy Service a jaké kategorie budou podporované? (Hra, rank a styl hraní jsou rozhodnuté; kategorie zůstávají otevřené.)
9. ~~Jaká pravidla platí pro útratu Paid versus Bonus Goins?~~ Bonus první, výplata hráči v Paid (viz WALLET-002).
10. Jak se nastaví, čerpá a doplňuje Bonus CAP?
11. Jak se budou řešit chargebacky a záporné zůstatky? (Refundy objednávek a spory: ORDER-001, ORDER-003.)
12. Jaký právní a účetní model bude mít Central Wallet?
13. Jaké Goins lze vyplatit hráči a za jakých podmínek?
14. Jaké budou částky signup bonusů a bude druhá úroveň doplatkem rozdílu?
15. ~~Jaký minimální věk bude vyžadován u Customer účtů?~~ 18 let, potvrzení při registraci (VERIFY-003).
16. Které metody ověření věku/identity budou dostupné při spuštění?
17. Jak se budou definovat měsíční průměrné výdělky a reklamovanost v Analytics?
18. ~~Jak bude vypadat první veřejná a placená release fáze a co do ní nepatří?~~ První vydání bude se skutečnými
    penězi (rozhodnuto 2026-11-19); rozsah a návrhy viz [Specifikace prvního vydání](./FIRST_RELEASE_SPEC.md).
19. Jak bude vypadat plný profil hráče (PROFILE-004): adresa, obsah, formátování textu „o mně“ a vztah k dnešnímu popupu?

## 19. Doporučený další postup

1. Pokračovat v brainstormingu a průběžně doplňovat tento dokument.
2. U každého nového požadavku přidělit ID a stav.
3. Jakmile bude daná oblast dostatečně jasná, udělat audit současného kódu a datového modelu.
4. Rozdělit schválený rozsah na epics a implementační GitHub Issues s acceptance criteria.
5. Teprve po potvrzení závislostí a priorit sestavit časovou roadmapu.

**Nyní nevytvářet hromadně implementační issues.** Většina zde zapsaných bodů zatím představuje produktové požadavky, nikoliv hotové vývojové zadání.
