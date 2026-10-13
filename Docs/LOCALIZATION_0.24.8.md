# Languages (0.24.8)

The game is written in English and translated into nine languages: Simplified Chinese (`zh-CN`), Japanese (`ja`),
Korean (`ko`), Spanish (`es`, one neutral Spanish for Spain and Latin America), Brazilian Portuguese (`pt-BR`),
German (`de`), French (`fr`), Russian (`ru`) and Polish (`pl`).

**Status: written and compiled in Unity (`Tools/unity.sh compile`), never played.** The translations are Claude's,
with no native review.
Steam's language table should say Interface and Subtitles for these languages, not Full Audio: the 25 spoken
clips stay English and their captions are translated.

Every line the code and the content assets ask for is translated: 1,462 lines x 9 (`python3 Tools/Text/check.py`
reports 0 missing). The second pass (2026-10-13) added the names and descriptions kept in data assets (loot,
loot sets, gear, cosmetics, achievements, contract modifiers, truck upgrades, the boss's voicemails and the
contract cards' quips), made a language change re-word the whole game at once, and wrote Steam's files.

## How it works

- **The English stays in the code and is the key.** `Loc.T("Back")` gives "Zurück" in German and "Back" wherever
  a line has no translation yet, so a missing line never shows a key. English itself never reads the sheets.
- Values go in as `{0}`, `{1}`: `Loc.T("Saved to {0}", file)`. A translation may move them.
- Counted lines: `Loc.N(count, "{0} item", "{0} items")`. `{0}` is the count.
- `Loc.K("some.id", english)` asks by id, for a line whose English alone is ambiguous or lives in the sheet
  (the tips: `hint.<HintId>`, where `{key:Interact}` is the player's own key).
- English kept in a `const`, a static list or a data asset is marked with a trailing `// @loc` comment (or
  `Loc.Mark("...")` inside an expression) and worded where it is shown: `Loc.T(name)`.
- Content assets (loot, gear, cosmetics, modifiers, threats, achievements, truck upgrades) keep their English
  `DisplayName` and `Description`; screens show `LocalName` and `LocalDescription`. A new loot item still needs
  no code: until its name is in a sheet it shows in English.
- **Nothing about language is networked.** Each machine words everything itself. Where the host sends a sentence
  (a refusal, a death cause, a kick reason) it sends the English, and the receiving machine looks that up.
  Player names and chat are never translated.
- The language is the player's pick in Settings > Gameplay > Language; without one it follows the game's language
  in Steam, then the computer's, then English. Batch runs (tests, nettests) are always English.
- **Changing the language re-words everything at once**, in the menus or in the middle of a job:
  - the menu screens are built again (`MenuUi.RebuildForLanguage`);
  - the HUD is emptied (`HudLayer.Clear`) and each piece builds itself again the same frame, the way it already
    does when it finds its elements gone. A new HUD piece must follow that pattern
    (`if (x == null || x.panel == null) Build();`) and reset what it remembers having shown inside `Build`;
  - a world-opened screen (`ScreenPanel`) has the language's version in its build key, so one that is open
    rebuilds too; the truck's board, which is its own document, does the same;
  - the chat keeps its lines and only sets its fixed words again.
  What is already on screen for a few seconds (a toast, a caption, a radio line) keeps its wording until it goes.

Code: `Scripts/Core/Text/` (`Loc`, `TextTable`, `Languages`, `LanguageInfo`, `Plurals`, `TextDebugOverlay`),
`Scripts/UI/LanguageFonts.cs`, `Scripts/UI/Menus/MenuUi.Language.cs`.

## The sheets

`Game/Assets/_Project/Data/Text/Resources/Text/*.txt`, one file per part of the game. A sheet is a list of blocks
split by a blank line, one line per language:

```
en: Back
zh-CN: 返回
ja: 戻る
de: Zurück
```

- The sheets named `data_*.txt` hold what the content assets say (`data_loot`, `data_gear`, `data_cosmetics`,
  `data_achievements`, `data_jobs` for contract modifiers and truck upgrades, `data_voicemail`) and `data_world`
  the words saved in scenes or made up at run time (camera labels, "Player 2", the credits' headings).
- The `en:` line must match the English in the code exactly. A block may start with `key: some.id` instead.
- A missing language line means that language shows the English.
- `#` starts a comment line. `\n` is a line break. No line may start or end with a space.
- Counted lines list their forms split by ` | `: `one | other` for English, German, Spanish, French and
  Portuguese; `one | few | many` for Russian and Polish; a single form for Chinese, Japanese and Korean.
- Keep `{0}`, `{1:N0}` and `{key:Interact}` exactly as they are; they may move within the sentence.

**Check the sheets** (reads files only, nothing is run in Unity):

```bash
python3 Tools/Text/check.py
```

It lists English lines the code asks for that no sheet has, sheet lines nothing asks for, languages missing from
a block, placeholders that differ from the English, and lines in the wrong script.

## What stays English on purpose

- The 25 spoken clips (their captions are translated).
- Player names and chat, and the game's and the company's names.
- Lettering that is part of the world's art: the EXIT sign in the mall and the store logos and sale posters
  (they are pictures).
- Key names on key caps ("Space", "LMB", "D-Pad Up"): they come from the input system and name the icons.
- The credits' entries (makers and licences); their two headings are translated.
- F1 debug lines and anything else only developer builds show.

## Steam's own files

Steam shows these outside the game, so they are uploaded on the partner site, not read by the game:

- `Tools/Steam/rich_presence_<language>.vdf`: what friends see under your name, one file per Steam language
  (Spain and Latin America share the Spanish text). Upload each under Rich Presence localization.
- `Tools/Steam/achievements_languages.csv`: every achievement's name and description per Steam language, made
  from the sheets by `python3 Tools/Text/steam_achievements.py` (run it again after changing the wording).

## Fonts

| Script | Font |
|---|---|
| Latin (English, Spanish, Portuguese, German, French, Polish) | The interface fonts themselves |
| Cyrillic (Russian) | IBM Plex Mono has it; headings fall back to Noto Sans |
| Chinese, Japanese, Korean | Noto Sans SC / JP / KR Medium (fallbacks) |

Chinese, Japanese and Korean share characters but draw them differently, so `LanguageFonts` puts the font of the
language in use first. A character already drawn keeps its font until the game restarts, so switching between
Chinese and Japanese mid-session can leave some shared characters in the other's shapes.

The first Chinese and Korean files (0.19.9) were variable fonts whose default weight is Thin; 0.24.8 replaces
them with fixed Medium files and adds Japanese. **Run once in Unity:**
**Tools > Abandoned > UI > Language Fonts (0.24.8)** (or `Tools/unity.sh exec Abandoned.EditorTools.Release0248.AuthorAndExit`).
It makes the three font assets, puts them on the panel's fallback list, removes the two old files and regenerates
the credits. Until it has run, Chinese and Korean use the old thin files and Japanese borrows Chinese shapes.

## Debug

- F1 shows a text line: the language, lines loaded, and how many English lines were asked for without a
  translation. Turning F1 off and on lists those lines in the Console, ready to paste into a sheet.
- Developer builds list one more language, **Stretched text (dev)**: English in brackets and a third longer.
  Anything not in brackets never went through `Loc`; anything cut off will be cut off in German and Russian.

## Wording

One word per thing, in every sheet. Address the player informally where the language has a choice
(German du, Spanish tú, Portuguese você, Polish ty), French vous, Russian вы-form imperatives, Korean -세요,
Japanese plain instructions (〜しよう / 〜だ).

| English | zh-CN | ja | ko | es | pt-BR | de | fr | ru | pl |
|---|---|---|---|---|---|---|---|---|---|
| job | 任务 | 仕事 | 작업 | trabajo | trabalho | Auftrag | mission | заказ | zlecenie |
| crew / crewmate | 队伍 / 队友 | クルー / 仲間 | 팀 / 동료 | equipo / compañero | equipe / colega | Team / Kollege | équipe / coéquipier | бригада / напарник | ekipa / kolega |
| HQ | 总部 | 本部 | 본부 | la central | QG | Zentrale | QG | база | baza |
| the company | 公司 | 会社 | 회사 | la empresa | a empresa | die Firma | l'entreprise | компания | firma |
| loot | 战利品 | 回収品 | 전리품 | botín | saque | Beute | butin | добыча | łupy |
| haul | 收获 | 回収額 | 수확 | carga | carga | Ausbeute | chargement | улов | urobek |
| quota | 定额 | ノルマ | 할당량 | cuota | cota | Soll | quota | норма | norma |
| truck | 卡车 | トラック | 트럭 | camión | caminhão | Laster | camion | грузовик | ciężarówka |
| van | 厢式车 | バン | 밴 | furgoneta | van | Transporter | camionnette | фургон | furgonetka |
| scan | 扫描 | スキャン | 스캔 | escanear | escanear | scannen | scanner | сканировать | skanować |
| flashlight | 手电筒 | 懐中電灯 | 손전등 | linterna | lanterna | Taschenlampe | lampe torche | фонарик | latarka |
| pockets | 口袋 | ポケット | 주머니 | bolsillos | bolsos | Taschen | poches | карманы | kieszenie |
| stamina | 体力 | スタミナ | 체력 | aguante | fôlego | Ausdauer | endurance | выносливость | kondycja |
| health | 生命 | 体力 (HP) | 생명력 | salud | vida | Leben | santé | здоровье | zdrowie |
| host | 房主 | ホスト | 호스트 | anfitrión | anfitrião | Host | hôte | хост | host |
| the boss | 老板 | ボス | 사장 | el jefe | o chefe | der Chef | le patron | начальник | szef |
| ghost | 幽灵 | 幽霊 | 유령 | fantasma | fantasma | Geist | fantôme | призрак | duch |
| locker | 储物柜 | ロッカー | 사물함 | casillero | armário | Spind | casier | шкафчик | szafka |
| shutter | 卷帘门 | シャッター | 셔터 | persiana | porta de enrolar | Rolltor | rideau métallique | рольставни | roleta |
| breaker panel | 配电箱 | 分電盤 | 분전반 | cuadro eléctrico | quadro de disjuntores | Sicherungskasten | tableau électrique | щиток | skrzynka bezpieczników |
| freight lift | 货梯 | 貨物リフト | 화물 승강기 | montacargas | elevador de carga | Lastenaufzug | monte-charge | грузовой лифт | winda towarowa |
| vent | 通风管 | 通気ダクト | 환풍구 | conducto de ventilación | duto de ventilação | Lüftungsschacht | conduit d'aération | вентиляция | szyb wentylacyjny |
| safe | 保险箱 | 金庫 | 금고 | caja fuerte | cofre | Tresor | coffre-fort | сейф | sejf |
| drill | 钻机 | ドリル | 드릴 | taladro | furadeira | Bohrer | perceuse | дрель | wiertarka |
| winch | 绞盘 | ウインチ | 윈치 | cabrestante | guincho | Winde | treuil | лебёдка | wyciągarka |
| crowbar | 撬棍 | バール | 쇠지렛대 | palanca | pé de cabra | Brecheisen | pied-de-biche | лом | łom |
| bolt cutters | 断线钳 | ボルトカッター | 볼트 커터 | cizalla | alicate de corte | Bolzenschneider | coupe-boulons | болторез | nożyce do metalu |
| medkit | 急救包 | 救急キット | 구급상자 | botiquín | kit médico | Verbandskasten | trousse de soins | аптечка | apteczka |
| trolley | 手推车 | 台車 | 손수레 | carretilla | carrinho | Sackkarre | diable | тележка | wózek |
| The Blind One | 盲者 | 盲目の者 | 눈먼 자 | El Ciego | O Cego | Der Blinde | L'Aveugle | Слепой | Ślepiec |
| The Stalker | 潜行者 | ストーカー | 스토커 | El Acechador | O Perseguidor | Der Pirscher | Le Rôdeur | Преследователь | Prześladowca |
| The Collector | 收藏家 | コレクター | 수집가 | El Coleccionista | O Colecionador | Der Sammler | Le Collectionneur | Коллекционер | Kolekcjoner |
| The Hunter | 猎手 | ハンター | 사냥꾼 | El Cazador | O Caçador | Der Jäger | Le Chasseur | Охотник | Łowca |
| Crawlers | 爬行者 | クローラー | 크롤러 | Reptadores | Rastejantes | Krabbler | Rampants | Ползуны | Pełzacze |
| Relaxed / Standard / Brutal | 轻松 / 标准 / 残酷 | やさしい / 標準 / 過酷 | 여유 / 표준 / 가혹 | Relajada / Estándar / Brutal | Tranquila / Padrão / Brutal | Entspannt / Standard / Brutal | Détendue / Standard / Brutale | Лёгкая / Обычная / Жестокая | Luźny / Standardowy / Brutalny |

The company in the fiction, **Abandoned Salvaging**, and the game's title are never translated.
