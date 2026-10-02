# PLAN — C64 Frogger (demoprojekt)

> Syfte: visa en student hela kedjan **idé → kod → GitHub → CI/CD → publicerat spel**.
> Spelet är ett demoverktyg, inte en produkt. Håll det litet.

## 1. Teknikstack

| Del | Val | Motivering |
|-----|-----|------------|
| Språk | C# 14 / .NET 10 | Studentens språk |
| Grafik/ljud/input | `Raylib-cs` (NuGet) | Studenten använder det redan, enkel spelloop |
| Retro-look | `RenderTexture` 320×200, nearest-neighbour, 16 C64-färger | Ger C64-känsla utan bildfiler |
| Ljud | Genererade vågformer (fyrkant/brus) i kod | Blip/blop utan ljudfiler |
| Bygge | `dotnet publish` self-contained, `win-x64` + `linux-x64` | Spelaren behöver inte installera .NET |
| Publicering | `butler push` → itch.io-kanalerna `windows` och `linux` | Zip-nedladdning, itch-appen sköter installation |
| CI/CD | GitHub Actions, trigger: tagg `v*` | |

Inga bildfiler, inga ljudfiler. Allt ritas och genereras i kod.

## 2. Mappstruktur

```
c64-frogger/
├── PLAN.md
├── README.md              # Flödet steg för steg, för studenten
├── .gitignore
├── .github/workflows/
│   └── release.yml        # Tagg v* → bygg → butler push
├── scripts/
│   └── publish_local.sh   # Bygger + zippar lokalt (samma som CI)
└── src/Frogger/
    ├── Frogger.csproj
    ├── Program.cs         # Fönster + spelloop
    ├── Game.cs            # Tillstånd: Title / Playing / GameOver
    ├── Player.cs          # Grodan (hopp en ruta i taget)
    ├── Lane.cs            # Lane-typer + objekt som rör sig
    ├── World.cs           # Genererar banor löpande, svårighet
    ├── Palette.cs         # De 16 C64-färgerna
    ├── Sound.cs           # Blip/blop
    └── HighScore.cs       # Sparas i en liten textfil lokalt
```

Få filer, korta filer. Studenten ska kunna läsa allt på en kvart.

## 3. Spelmekanik

- Spelplan 320×200 (40×25 rutor à 8 px, som C64:s teckenskärm). Grodan hoppar en ruta åt gången med piltangenter/WASD.
- **Oändlig**: kameran följer grodan uppåt. Banor genereras framför, raderas bakom.
- **Poäng** = längsta rad nådd. Highscore sparas i en lokal fil (`highscore.txt` i användarens datamapp).
- **Lane-typer:**
  - **Gräs** — säker.
  - **Väg** — bilar i olika fart, krock = död.
  - **Flod** — stockar, vatten = död, stocken bär grodan med sig. Åker man ut ur skärmen = död.
  - **Tåg** (om tid finns) — varningsblink, sedan snabbt tåg.
- **Svårighet:** ju högre rad, desto högre fart, tätare trafik och större andel väg/flod jämfört med gräs.
- **Game over** visar poäng + highscore, mellanslag startar om.

## 4. Definition av "klart"

Spelet är klart när:

1. Man kan spela från titelskärm → dö → game over → starta om, utan krasch.
2. Minst tre lane-typer (gräs, väg, flod) fungerar och slumpas.
3. Svårigheten märkbart stiger.
4. Highscore överlever att stänga och starta om spelet.
5. Blip/blop vid hopp, död och ny rekordrad.
6. En `v*`-tagg bygger och publicerar via CI, och en nedladdad zip från itch.io startar på en ren maskin.
7. README låter studenten göra om hela flödet själv.

## 5. MVP kontra "om tid finns"

**MVP (måste):** gräs + väg + flod, oändlig generering, poäng, highscore, enkelt ljud, C64-palett och pixelfont (inbyggd raylib-font, skalad), CI-publicering.

**Om tid finns:** tåg-lane, C64-blå ram runt skärmen (border), "READY."-blinkande cursor på titelskärmen, ljud i flera toner, gamepad, macOS-build, riktig installer.

**Medvetet utanför scope:** meny/inställningar, flera liv, banor med design, bildfiler, web-build.

## 6. Byggordning (dag för dag, ~1 vecka)

Principen: **publicera ett tomt skal först.** Då upptäcker vi publiceringsproblem dag 1, inte dag 6.

| Dag | Mål | Klart när |
|-----|-----|-----------|
| 1 | Repo + "Hello C64"-fönster (blå bakgrund, text) + manuell `butler push` | Skalet finns på itch.io och startar |
| 2 | CI: tagg `v0.1.0` → publicerat via Actions | Grön workflow, ny version på itch |
| 3 | Grodan + rörelse + kamera + gräs/väg-lanes + död | Kan spelas och dö |
| 4 | Flod + stockar, oändlig generering, poäng, svårighet | Kan spela länge |
| 5 | Highscore, ljud, titel/game over, C64-polish | Alla MVP-punkter gröna |
| 6 | README, städning, release `v1.0.0` via CI | Live på itch.io, **dagen före visning** |
| 7 | Visning | |

Små commits: en förändring per commit, svenska eller engelska meddelanden men tydliga ("Lägg till flod-lane med stockar").

## 7. Publicering (egen del)

**Mål:** `git tag v1.0.0 && git push --tags` ⇒ spelet uppdateras på itch.io.

### 7.1 Engångsuppsättning

1. **itch.io-sida** finns redan under `https://marcmed.itch.io/<slug>`. Slug bestäms innan dag 1.
2. **Installera butler** lokalt (guidas steg för steg).
3. **`butler login`** lokalt (öppnar webbläsaren, ingen nyckel i filer).
4. **API-nyckel för CI:** skapas på itch.io → Settings → API keys. Läggs in som GitHub-secret `BUTLER_API_KEY`. Nyckeln visas aldrig i chatten och committas aldrig.
5. **Itch-sidans inställningar:** Kind of project = *Downloadable*. (Ingen "played in the browser"-kryssruta, eftersom vi inte har någon web-build.)

### 7.2 Manuell push (görs först, visas för studenten)

```
dotnet publish -c Release -r win-x64 --self-contained -o build/win
butler push build/win marcmed/<slug>:windows --userversion 0.1.0
```
Samma för `linux-x64` → kanal `linux`. Butler zippar och skickar bara ändringarna.

### 7.3 CI-workflow (`.github/workflows/release.yml`)

- Trigger: `push` av tagg `v*`.
- Steg: checkout → setup-dotnet 10 → `dotnet publish` för båda plattformarna → ladda ner butler → `butler push` till `windows` och `linux` med versionen från taggen.
- `BUTLER_API_KEY` läses från `secrets`, skickas som miljövariabel, aldrig utskriven.

### 7.4 Verifiering

- Tagga `v0.1.0` → kontrollera att båda kanalerna får ny build på itch.io-dashboarden.
- Ladda ner zipen via itch och starta på en maskin utan .NET.

### 7.5 Risker

| Risk | Åtgärd |
|------|--------|
| Raylib-native-lib saknas i self-contained-bygget | Testas dag 1 |
| Linux-build kräver systembibliotek (GL) | Dokumenteras i README |
| Windows SmartScreen varnar för osignerad .exe | Förklaras för studenten, inte åtgärdat |
| Butler-nyckel läcker | Bara som secret, aldrig i logg eller fil |

## 8. Öppna punkter

- [ ] Itch-slug för spelet (`marcmed.itch.io/<slug>`)
- [ ] GitHub-konto/org för repot (inloggad som `marcusjobb`) och att det är publikt
- [x] Godkännande av planen
- [ ] README: FAQ "Vanliga fallgropar" byggd på de riktiga frågorna under bygget
  (slug kontra profil-URL, "invalid game", butler login kontra CI-nyckel,
  nyckeln får aldrig klistras in i chatten, Draft-läge på itch-sidan)
