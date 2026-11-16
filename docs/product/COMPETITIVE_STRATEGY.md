# Zagejmi – Competitive positioning & product experience

> **Typ dokumentu:** produktová strategie a UX návrh  
> **Stav:** pracovní návrh (PROPOSED); není automaticky schválenou specifikací  
> **Zpracováno:** 2026-11-16  
> **Rozsah:** produkt Zagejmi; nikoli VUTRegBot  
> **Měna:** Goin (jednotka), Goins (množné číslo)

## 1. Executive summary

Zagejmi má být marketplace, na kterém si člověk snadno najde konkrétního člověka pro společné hraní nebo jinou jasně popsanou herní či sociální službu. Nejde primárně o coaching platformu ani o sociální síť, jejíž hlavní funkcí je feed.

**Doporučené produktové vymezení:**

> **Najdi parťáka na hraní podle hry, stylu a toho, jaký zážitek hledáš. Předem víš, s kým hraješ, co spolu budete dělat a kolik to stojí.**

Hlavní konkurenční inspirací je E-Pal, který už pokrývá placené herní parťáky, sociální/chill služby, vlastní profily, katalog služeb, chat, recenze a úrovně poskytovatelů. Metafy je důležitý sousední konkurent v placených herních sessions, ale jeho veřejná prezentace je výrazně orientovaná na zlepšování výkonu, coaching, guides a komunity. Zagejmi by nemělo kopírovat jejich šíři funkcí. Má vyhrát srozumitelnějším výběrem konkrétního zážitku, transparentní nabídkou a důvěryhodným objednáním.

**Nejdůležitější strategická změna:** navrhnout marketplace jako jeden propojený discovery a booking zážitek. Hráč a služba zůstávají dva legitimní způsoby hledání, ale nemají působit jako dva oddělené produkty. Úvodní stránka musí návštěvníka dostat k relevantním nabídkám, nikoli pouze ukázat několik profilových karet.

Tento dokument je návrh směru. Neznámé obchodní a právní otázky zůstávají otevřené; žádný UX návrh nesmí předstírat, že už je vyřešen platební nebo regulatorní model Goinů.

## 2. Co víme o konkurenci

Analýza vychází z veřejných produktových a help-center stránek, nikoli z neveřejných dat, interních algoritmů ani z tvrzení o jejich skutečné konverzi. Funkce se mohou měnit; odkazy a pozorování byly ověřeny 2026-11-16.

### 2.1 E-Pal – přímý konkurent

E-Pal se sám popisuje jako platforma pro herní a sociální parťáky. Veřejné materiály uvádějí přes 100 her a rozlišují tři základní typy nabídky: Gaming, Chilling a Custom. Poskytovatelé si připravují služby, cenu a profil; uživatelé si vybírají ePala podle nabídky. Platforma používá úrovně poskytovatelů a Store Rating pro kvalitu prezentace a viditelnost. Objednávky často začínají komunikací v instant messagingu a vlastní herní služba se obvykle odehrává v externí hře nebo nástroji. [E-Pal – Introduction to ePal](https://www.epal.gg/help/faq/16), [E-Pal – ePals Page](https://www.epal.gg/help/faq/17), [E-Pal – Service Application](https://www.epal.gg/help/faq/30)

**Co E-Pal dělá dobře (pozorování):**
- Jasně chápe, že zákazník kupuje společnost, hraní nebo konkrétní aktivitu – nejen výuku.
- Nabídky rozděluje na herní, sociální a vlastní/custom služby.
- Poskytovatele motivuje k doplnění profilu a služeb a vysvětluje, že úroveň ovlivňuje viditelnost.
- Má rozsáhlou nabídku a mnoho kategorií; šíře katalogu je sama o sobě konkurenční výhodou.
- Má proces reklamace objednávky s evidencí důkazů, lhůtami a možností částečného či úplného refundu. [E-Pal – Dispute Orders](https://www.epal.gg/help/faq/31)

**Co z toho plyne pro Zagejmi:**
- Pouhé „najdi si někoho na hraní“ není diferenciace – E-Pal to už dělá.
- Katalog služeb musí jasně odlišit *co se bude dít* (např. casual duo, ranked duo, coaching, společné plnění úkolů, chill/voice) a *kdo to poskytuje*.
- Komunikace před objednávkou může být užitečná, ale nesmí být jediným způsobem, jak zjistit cenu, obsah nebo podmínky nabídky.
- Reklamace, důkazy a bezpečnost musí být součástí návrhu celého flow, ne dodatečná administrativní stránka.

### 2.2 Metafy – sousední konkurent

Metafy se prezentuje jako místo, kde zkušení hráči pomáhají ostatním zlepšit se. Nabízí 1:1 sessions, VOD reviews, events, guides a komunity. Poskytovatel nastavuje typ, délku, cenu a dostupnost; kupující vybírá profil, nabídku a termín. Metafy také popisuje vlastní live room, záznam session, poznámky a shrnutí. [Metafy – What is Metafy and How Does It Work?](https://help.metafy.gg/en/articles/14676002-what-is-metafy-and-how-does-it-work), [Metafy – Our Offerings](https://help.metafy.gg/en/articles/11867539-our-offerings), [Metafy – Sessions on Platform](https://help.metafy.gg/en/articles/17600545-sessions-on-platform-student-guide)

**Co Metafy dělá dobře (pozorování):**
- Nabídka má jasný formát, délku, cenu a případně termín.
- Profil poskytovatele funguje jako storefront a obsahuje jeho zkušenosti, nabídky a recenze.
- Booking je navázaný na kalendář a konkrétní session.
- Produkt dobře podporuje opakovaný vztah mezi tvůrcem a zákazníkem.

**Co z toho plyne pro Zagejmi:**
- Strukturované nabídky a transparentní booking jsou dobrý vzor.
- Zagejmi ale nemá převzít coaching-first jazyk a architekturu. Jeho hlavní příslib je kompatibilní parťák a společný herní zážitek; coaching může být jedním z typů služby, nikoli definicí celého produktu.
- Později lze zvážit plánované session, balíčky nebo opakované hraní, ale až po ověření základního objednání.

### 2.3 Konkurenční závěr

| Oblast | E-Pal | Metafy | Doporučení pro Zagejmi |
|---|---|---|---|
| Primární slib | Herní/socialní parťák | Zlepšení výkonu a vzdělávání | Najdi správného parťáka a konkrétní zážitek |
| Discovery | ePals, hry, typy služeb | Hry, tvůrci, sessions, guides | Jeden katalog s přepnutím pohledu Hráči / Služby |
| Nabídka | Gaming / Chilling / Custom | Strukturované sessions, VOD, events, guides | Strukturovaná služba s jasným výsledkem, cenou a podmínkami |
| Důvěra | Úrovně, Store Rating, recenze, spory | Profily, produktové recenze, platformní booking | Ověřitelné signály + recenze navázané na objednávku + jasná ochrana |
| Booking | Komunikace a objednávka; často externí realizace | Session a plánování termínu | Nejdřív přesný popis a cena; termín/availability jako další krok |
| Hlavní příležitost | Velký katalog a šíře funkcí | Silný coaching workflow | Srozumitelnost, kompatibilita a důvěra v každém kroku |

## 3. Základní uživatelé a jejich úkoly

Segmenty níže jsou **pracovní hypotézy**, ne potvrzené persony. Je nutné je ověřit rozhovory a testováním.

### Zákazník
Chce najít člověka, se kterým si dobře zahraje. Může mu jít o:
- společné casual hraní a zábavu;
- ranked/competitive duo nebo týmovou spolupráci;
- pomoc s konkrétní hrou, misí, achievementem nebo obtížnou částí;
- coaching nebo zpětnou vazbu;
- společnost, komunikaci a chill.

Jeho hlavní otázky: *Hraje tuhle hru? Sedne mi jeho styl? Je dostupný? Co přesně dostanu? Kolik zaplatím? Můžu mu věřit?*

### Hráč / poskytovatel
Chce prezentovat svou osobnost, schopnosti a styl hraní, nastavit nabídky a získávat objednávky bez zbytečného vyjednávání. Potřebuje vědět, co zákazník očekává, kdy má službu poskytnout, kolik vydělá a jak se řeší problém.

### Provozovatel
Potřebuje zdravý marketplace: dostatečnou nabídku v konkrétních hrách a časech, kvalitní objednávky, nízký počet sporů, bezpečnost a udržitelnou ekonomiku. Samotný počet registrovaných profilů není dostatečná metrika.

## 4. Doporučené produktové principy

1. **Zážitek před profilem.** Návštěvník nejprve rozumí tomu, co může dělat; profil vysvětluje, s kým to může dělat.
2. **Konkrétní nabídka před chatem.** Cena, jednotka, obsah a důležité podmínky jsou viditelné před objednáním. Chat slouží k doplnění detailů, ne k odhalení základních informací.
3. **Kompatibilita před popularitou.** Výsledky mají odpovídat hře, módu, jazyku, stylu, preferencím a dostupnosti. Popularita sama nesmí vytlačit nováčky ani relevantní nabídky.
4. **Důvěra bez falešných slibů.** Rozlišovat ověřenou identitu, ověřený herní rank, historii dokončených objednávek a běžné tvrzení v biu. Jedna značka „verified“ nesmí znamenat všechno.
5. **Transparentní cena.** U každé nabídky ukázat jednotku a celkovou cenu před potvrzením. Goiny musí být srozumitelně vysvětlené; dokud není vyřešen právní/platební model, neprezentovat je jako skutečné peníze ani garantovanou výplatu.
6. **Mobile-first, ale ne mobile-only.** Discovery, profil, objednávka a správa nabídky musí fungovat i na úzkém viewportu.
7. **Nesoutěžit počtem funkcí.** Nejprve vyřešit discovery → výběr → objednávku → realizaci → důvěryhodnou recenzi. Feed, livestreaming, předplatné, AI parťáci a komplexní leaderboard jsou mimo první produktový fokus.

## 5. Doporučená informační architektura

Zachovat dvě vstupní cesty, které už odpovídají produktovému záměru, ale sjednotit je do jednoho marketplace systému:

- **Objevovat** – primární vstupní stránka s vyhledáváním podle hry, aktivity a záměru; doporučené nabídky a hráči.
- **Služby** – katalog konkrétních nabídek; primární cesta pro zákazníka, který ví, co chce dělat.
- **Hráči** – katalog lidí; cesta pro zákazníka, který vybírá osobnost nebo chce najít konkrétního člověka.
- **Profil hráče** – plnohodnotná veřejná stránka s bio, hrami, ranky, jazyky, stylem, dostupností, nabídkami a recenzemi.
- **Detail služby** – samostatný detail nabídky s očekávaným průběhem a objednáním.
- **Objednávky** – přehled probíhajících i minulých objednávek, stavů, lhůt a akcí.
- **Peněženka** – přehled Goinů a pohybů; jasné oddělení placených a bonusových Goinů.
- **Můj profil / Player dashboard** – správa profilu, nabídky, objednávek a později analytics.

Hráč a služba musí být propojené obousměrně: služba vždy ukazuje poskytovatele; profil vždy ukazuje jeho aktivní nabídky. Kliknutí na kartu služby má vést na detail konkrétní služby, ne pouze na obecný popup hráče.

## 6. Návrh klíčových obrazovek

### 6.1 Úvodní stránka / Objevovat

Dnešní homepage v **Home.razor** je obsahově velmi jednoduchá: uvítací blok, čtyři doporučení hráčů a obecný komunitní text. Má i vlastní pastelové pink/yellow/purple styly, zatímco globální **app.css** definuje tmavý základ a fialový akcent. To vytváří nekonzistenci a homepage dnes nevede přímo k hledání konkrétní nabídky.

**Doporučená hierarchie:**
1. Krátký, konkrétní headline: „Najdi parťáka na další hru.“
2. Velké vyhledávání: hra + volitelně „co chceš dělat?“
3. Rychlé volby záměru: „Casual“, „Ranked duo“, „Coaching“, „Chill / pokec“, „Pomoc s úkolem“ – pouze pokud budou tyto typy skutečně podporované.
4. Dostupní / brzy dostupní hráči a služby s jasným označením skutečné availability; nikdy neukazovat falešný online status.
5. Kurátorované kategorie her a nabídky, které mají dostatečnou hustotu.
6. Krátké vysvětlení procesu a ochrany objednávky.
7. Samostatná výzva pro hráče: „Nabízej své služby“.

Nedělat z homepage obecný marketingový landing page s dlouhým textem. Primární funkcí je zahájit discovery.

### 6.2 Discovery – Hráči a Služby

Současné oddělené stránky jsou rozumný základ a mají zůstat dostupné. Jejich UX ale potřebuje sjednotit:
- sdílený filtr hry, herního režimu, jazyka a dostupnosti;
- jasný přepínač „Hledám člověka / Hledám službu“;
- zachování filtrů, URL a historie prohlížeče;
- řazení vysvětlitelné uživateli, nikoli neprůhledný „nejlepší“ žebříček;
- prázdné stavy s návrhem uvolnit filtr nebo zkusit jinou hru;
- jasné označení počtu objednávek/recenzí a stáří profilu, pokud jsou tato data spolehlivá.

**Karty hráčů:** fotografie/avatar, přezdívka, primární hra, styl, jazyky, stručné bio, relevantní rank (pokud je ověřitelný nebo jasně označený jako deklarovaný), hodnocení + počet recenzí, dostupnost a odkaz na profil.

**Karty služeb:** název a výsledek služby, hra/mód, poskytovatel, délka nebo jednotka, cena v Goinech, hodnocení služby + počet recenzí, dostupnost. Obrázek má podporovat pochopení nabídky, nikoli zabírat prostor bez informační hodnoty.

### 6.3 Veřejný profil hráče

Popup může zůstat jako rychlý náhled, ale nesmí být jediným profilem. Plný profil by měl mít:
- jasnou identitu a krátký osobní úvod;
- „Hry, které hraju“ a úroveň/rank s označením zdroje ověření;
- herní a komunikační styl, jazyky, platformy/voice preference;
- aktivní služby se zřetelnými cenami;
- dostupnost nebo realistické očekávání odpovědi;
- recenze konkrétních objednávek a odpovědi hráče (pokud budou zavedeny);
- report / blokování a pravidla bezpečného kontaktu.

Nezobrazovat citlivé soukromé údaje. Ghost Mode nesmí být prezentován jako anonymita vůči platformě.

### 6.4 Detail služby a objednávka

Dnešní objednávkový dialog již ukazuje množství, cenu za jednotku, celkovou cenu, zůstatek, escrow a lhůty. To je dobrý základ. Chybí však samostatný prostor, kde zákazník před otevřením dialogu plně pochopí, co služba obsahuje.

**Detail služby musí odpovědět:**
- Co přesně spolu budete dělat?
- Co služba zahrnuje a co nezahrnuje?
- Jak dlouho trvá jednotka a co znamená počet kusů?
- Která hra, server/region, mód, rank a platforma jsou relevantní?
- Jak proběhne komunikace (in-game voice, Discord apod.)?
- Co musí zákazník připravit?
- Jaká jsou pravidla změny termínu, nedostavení se a zrušení?
- Kdy a jak bude služba považována za dokončenou?

**Objednávka:** před zaplacením zobrazit shrnutí služby, poskytovatele, množství, celkovou cenu, použití bonusových/placených Goinů, stav objednávky a hlavní pravidla. Po zaplacení zobrazit další krok a termín/lhůtu, nikoli pouze obecné „Objednáno“.

### 6.5 Profil a dashboard hráče

Onboarding poskytovatele má být průvodce vytvořením první kvalitní nabídky, ne dlouhý formulář bez kontextu. Doporučené kroky:
1. základní profil a přezdívka;
2. hry a herní styl;
3. první konkrétní služba;
4. cena a jednotka;
5. dostupnost a způsob komunikace;
6. náhled veřejného profilu a kontrolní checklist.

Ukazovat, co ještě brání publikování nebo snižuje srozumitelnost nabídky. Nepoužívat „doplň fotku, abys měl lepší rank“ jako jedinou motivaci; vysvětlit přínos pro zákazníka.

## 7. Funkční mezery proti navrženému zážitku

Následující tabulka porovnává blueprint a prohlédnuté části master (zejména **Home.razor**, **People.razor**, **Services.razor**, **Profile.razor**, **OrderDialog.razor**, **app.css**). Nejde o kompletní audit každé komponenty ani o vizuální kontrolu běžící aplikace.

| Oblast | Zjištěný základ v master | Doporučený další krok |
|---|---|---|
| Homepage | Čtyři hráči, obecný uvítací obsah; odlišné pastelové styly oproti globálnímu dark/violet theme | Přepracovat na discovery-first homepage a sjednotit design systém |
| Hráči | Karty a hledání přezdívky; gender tabs | Doplnit relevantní filtry a plnohodnotný profil |
| Služby | Katalog, text, hra, minimální rank a styl hraní | Detail konkrétní služby, dostupnost, další strukturované parametry |
| Karty služeb | Kliknutí otevírá profilový popup hráče | Odkazovat na detail konkrétní služby; profil poskytovatele jako sekundární cesta |
| Profil hráče | Detail přes modal podle současného blueprintu | Samostatná veřejná stránka; modal jen jako quick view |
| Nabídka hráče | Název, popis, cena/jednotka, obrázek, hra, rank, styl a availability on/off (dle blueprintu) | Strukturované podmínky, délka, jazyky, region/platforma, plánování |
| Objednávka | Množství, cena, zůstatek, escrow a časová okna | Lepší předobjednávkové shrnutí, termín/next step, jasná pravidla a bezpečnost |
| Trust & safety | E-mailové ověření, Ghost Mode, escrow, recenze a admin dispute flow | Definovat úrovně ověření, report/block, důkazy, odvolání a moderaci |
| Ranking | Profilová úplnost a novost; hodnocení zatím pořadí neovlivňuje | Relevantní matching; oddělit kvalitu, dostupnost a placené zvýraznění |
| Platby | Goins/ledger a escrow; nákup Goinů a cashout chybí dle blueprintu | Před UX pro skutečné peníze vyřešit právní a platební model |

## 8. Doporučené pořadí práce

### Fáze A – Základní produktový zážitek (nejvyšší priorita)
- Homepage zaměřená na hledání.
- Sjednocené discovery a konzistentní komponenty karet/filtrů.
- Samostatný detail služby a veřejný profil hráče.
- Jednoznačné CTA a cesta od výsledku k objednávce.
- Responzivní a přístupné chování na mobilu i desktopu.

### Fáze B – Kvalita marketplace
- Strukturované atributy služeb a lepší filtrování.
- Dostupnost a plánování termínů.
- Lepší onboarding poskytovatele a checklist kvality nabídky.
- Vysvětlitelné řazení a prevence dominance starých/populárních účtů.
- Trust signals, report/block a moderace recenzí.

### Fáze C – Růst a opakované použití
- Oblíbení hráči a opakované objednání.
- Balíčky / více session, pokud je potvrdí data.
- Player analytics a doporučení.
- Komunitní funkce pouze tehdy, pokud podporují opakované hraní a retenci.

**Mimo fokus do ověření product-market fit:** feed jako hlavní stránka, livestreaming, vlastní voice/video infrastruktura, AI companions, subscription economy, složitý leaderboard a rozsáhlá gamifikace.

## 9. Metriky a ověření hypotéz

Měřit celý marketplace funnel, ne jen návštěvnost:

- **Discovery success:** podíl návštěv, které zobrazí relevantní výsledky a otevřou detail.
- **Profile/service engagement:** otevření detailu → kliknutí na objednávku.
- **Booking conversion:** zahájená objednávka → zaplacená objednávka.
- **Fulfilment:** přijaté objednávky → dokončené objednávky.
- **Trust:** reklamace, refundy, nedostavení se, reporty a čas vyřešení.
- **Supply health:** aktivní hráči podle hry/regionu/časového okna, podíl služeb s aktuální dostupností.
- **Repeat:** zákazníci, kteří objednají znovu stejného hráče nebo jinou službu.
- **Provider success:** čas do první objednávky, výdělek po poplatcích a míra přijetí objednávek.

Před implementací provést alespoň:
1. 5–8 rozhovorů se zákazníky, kteří někdy platili za herního parťáka nebo coaching;
2. 5 rozhovorů s potenciálními poskytovateli;
3. moderovaný test prototypu homepage → discovery → detail služby → objednávka;
4. test porovnání dvou nabídek, aby bylo vidět, zda lidé rozumí rozdílu ceny, obsahu a poskytovatele.

Počty jsou doporučený kvalitativní start, nikoli statisticky reprezentativní vzorek.

## 10. Otevřená rozhodnutí

Tyto body nesmí být považovány za schválené:
1. Primární cílový segment a počáteční hry/region.
2. Zda je hlavní jednotkou nabídky čas (např. 30/60 minut), konkrétní aktivita, nebo obojí.
3. Zda objednávka vyžaduje pevný termín, nebo může být „domluva po zaplacení“.
4. Jaké údaje o jazyce, regionu, platformě, voice chatu a ranku budou povinné.
5. Zda a jak ověřovat herní rank (API, screenshot, ruční kontrola, nebo pouze deklarace).
6. Jak definovat online/dostupný status a jak zabránit zavádějícím indikátorům.
7. Jaké jsou přesné podmínky storna, nedostavení se, reklamace a částečného plnění.
8. Jaké úrovně ověření budou potřeba pro zákazníka a poskytovatele; právní a privacy review.
9. Jaký právní a platební model budou mít Paid Goins, Bonus Goins, escrow a cashout.
10. Zda se má pohlaví vůbec používat jako výrazný výchozí organizační princip discovery; současné chování je implementované, ale jeho přínos a dopad na použitelnost je nutné ověřit.
11. Jaké placené zvýraznění je přijatelné a jak bude odděleno od organické relevance.

## 11. Zásady pro další práci v repozitáři

- Tento dokument je strategický návrh. Schválené produktové požadavky patří do PRODUCT_BLUEPRINT.md se stabilním ID a stavem.
- Před úpravou existujícího chování ověřit implementaci a závislosti; nepředpokládat, že popis blueprintu je úplný.
- Nové požadavky nejdřív označit jako PROPOSED nebo OPEN; teprve po potvrzení je převést na CONFIRMED.
- Nevytvářet hromadně GitHub issues, dokud není odsouhlasený rozsah a acceptance criteria.
- Designový návrh nesmí obcházet serverovou autorizaci, ledgerové invarianty, bezpečnost objednávek ani právní review.
- Nezaměňovat Zagejmi s VUTRegBot. V celé dokumentaci a UI používat název měny Goin/Goins, nikoli GOI.

## Zdroje

### Primární zdroje – konkurence
- E-Pal, [Introduction to ePal](https://www.epal.gg/help/faq/16)
- E-Pal, [ePals Page](https://www.epal.gg/help/faq/17)
- E-Pal, [Service Application](https://www.epal.gg/help/faq/30)
- E-Pal, [Dispute Orders](https://www.epal.gg/help/faq/31)
- Metafy, [What is Metafy and How Does It Work?](https://help.metafy.gg/en/articles/14676002-what-is-metafy-and-how-does-it-work)
- Metafy, [Our Offerings](https://help.metafy.gg/en/articles/11867539-our-offerings)
- Metafy, [Sessions on Platform: Student Guide](https://help.metafy.gg/en/articles/17600545-sessions-on-platform-student-guide)
- Metafy, [Smart Player Guide](https://help.metafy.gg/en/articles/8775516-metafy-s-smart-player-guide)

### Interní zdroje
- docs/product/PRODUCT_BLUEPRINT.md
- src/Zagejmi.Api/Components/Pages/Home.razor
- src/Zagejmi.Api/Components/Pages/People.razor
- src/Zagejmi.Api/Components/Pages/Services.razor
- src/Zagejmi.Api/Components/Pages/Profile.razor
- src/Zagejmi.Api/Components/Elements/OrderDialog.razor
- src/Zagejmi.Api/wwwroot/app.css
