# C64 Frogger Nostalgia

[![Bygg](https://github.com/marcusjobb/c64-frogger/actions/workflows/build.yml/badge.svg)](https://github.com/marcusjobb/c64-frogger/actions/workflows/build.yml)
[![CodeQL](https://github.com/marcusjobb/c64-frogger/actions/workflows/codeql.yml/badge.svg)](https://github.com/marcusjobb/c64-frogger/actions/workflows/codeql.yml)
[![Publicera](https://github.com/marcusjobb/c64-frogger/actions/workflows/release.yml/badge.svg)](https://github.com/marcusjobb/c64-frogger/actions/workflows/release.yml)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)
![C# 14](https://img.shields.io/badge/C%23-14-239120)
![Raylib-cs](https://img.shields.io/badge/Raylib--cs-8-black)
[![itch.io](https://img.shields.io/badge/itch.io-spela%20%2F%20ladda%20ner-FA5C5C)](https://marcmed.itch.io/c64froggernostalgia)

Ett litet, oändligt Frogger i C64-stil, skrivet i C#. Spelet är ett **demoverktyg**:
syftet är att visa hela kedjan **idé → kod → GitHub → CI/CD → publicerat spel**.

Ladda ner det på itch.io: https://marcmed.itch.io/c64froggernostalgia

## Spelet

- Hoppa uppåt, så långt du kan. Det finns inget slut, bara poäng.
- Banorna slumpas fram medan du spelar: **gräs**, **väg** (bilar, lastbilar och snabba sportbilar) och **flod** (hoppa på stockar).
- Det blir svårare ju längre du kommer, och skärmen scrollar av sig själv så du kan inte stå still.
- Highscore sparas på din dator.

| Tangent | Gör |
|---------|-----|
| Piltangenter eller WASD | Hoppa en ruta |
| Mellanslag | Starta / börja om |

## Köra själv

Du behöver [.NET 10 SDK](https://dotnet.microsoft.com/download).

```
git clone https://github.com/marcusjobb/c64-frogger.git
cd c64-frogger/src/Frogger
dotnet run
```

## Så är koden uppbyggd

```
src/Frogger/
├── Program.cs     Fönster och spelloop. Ritar till en liten 320x200-yta som skalas upp.
├── Game.cs        Titel / spel / game over, kamera, död, ritning.
├── Player.cs      Grodan.
├── Lane.cs        En bana och sakerna som glider på den (Mover).
├── World.cs       Slumpar fram banor löpande, styr svårigheten.
├── Palette.cs     C64:s 16 färger.
├── Sfx.cs         Blip och blop, genererade i kod (inga ljudfiler).
└── HighScore.cs   Sparar rekordet i en textfil.
```

Allt ritas med enkla rektanglar och inga bildfiler. C64-känslan kommer från den låga
upplösningen (320x200), paletten och att pixlarna skalas upp utan utjämning.

## Hela flödet: från kod till spelat spel

Så här gör du om det själv. Alla steg är beskrivna i den ordning vi gjorde dem.

### 1. Skapa spelsidan på itch.io

1. Gå till https://itch.io/game/new (du måste vara inloggad).
2. Fyll i titel och **Project URL** (det blir *slug*:en, till exempel `c64froggernostalgia`).
3. **Kind of project:** `Downloadable`. Sätt **Visibility** till `Draft` så länge.
4. Spara. Adressen blir `https://<ditt-användarnamn>.itch.io/<slug>`.

> Slug:en är **sista delen** av spelsidans adress, inte bara din profilsida.

### 2. Installera butler och logga in

Butler är itch.io:s eget kommandoradsverktyg för att ladda upp byggen. Du behöver göra det bara en gång.

```
mkdir -p ~/.local/bin && cd /tmp
curl -L -o butler.zip https://broth.itch.zone/butler/linux-amd64/LATEST/archive/default
unzip -o butler.zip -d ~/.local/bin && chmod +x ~/.local/bin/butler
butler -V          # kontrollera att det fungerar
butler login       # öppnar webbläsaren, klicka "Authorize"
```

(Windows/macOS: ladda ner butler från https://itchio.itch.io/butler.)

### 3. Första, manuella pushen

Gör den manuellt först, så vet du att kopplingen fungerar innan du automatiserar den.

```
./scripts/publish_local.sh 0.1.0
```

Skriptet bygger för Windows och Linux och kör `butler push` till kanalerna `windows` och `linux`.
Kontrollera sedan med `butler status <användarnamn>/<slug>`.

### 4. Automatisera med GitHub Actions

1. På itch.io: https://itch.io/user/settings/api-keys → **Generate new API key**.
   Skapa en **egen nyckel för CI** (inte den som `butler login` gjorde), så kan du återkalla dem var för sig.
2. Lägg in nyckeln som GitHub-secret. Klistra in värdet när terminalen frågar, och aldrig i chatten eller i en fil:
   ```
   gh secret set BUTLER_API_KEY --repo <ditt-konto>/<repo>
   ```
3. Workflowet `.github/workflows/release.yml` startar när du pushar en tagg som börjar med `v`.

### 5. Släpp en ny version

```
git tag v1.0.0
git push origin v1.0.0
```

Gå till fliken **Actions** på GitHub. När körningen är klar (cirka en minut) har båda kanalerna
på itch.io fått den nya versionen. Taggen `v1.0.0` blir versionen `1.0.0` på itch.io.

### Så hänger det ihop

| Fil | När körs den | Vad gör den |
|-----|--------------|-------------|
| `build.yml` | Varje push och pull request | Kontrollerar att koden kompilerar |
| `codeql.yml` | Varje push, pull request och varje måndag | Letar efter säkerhetsproblem i koden |
| `release.yml` | När du pushar en `v*`-tagg | Bygger och publicerar till itch.io |
| `dependabot.yml` | Varje vecka | Föreslår uppdateringar av paket och actions |

## Vanliga fallgropar

**`butler status` eller `butler push` säger "invalid game".**
Spelsidan finns inte än, eller slug:en är fel. Slug:en är sista delen av spelsidans adress
(`https://anvandare.itch.io/DEN-HÄR`). Bara `https://anvandare.itch.io/` är din profil.

**Är det bara en profilsida jag ser, eller en 404?**
Skapa spelsidan först (steg 1). Skriv inte `NÅGOT` eller ett exempelnamn i adressfältet. 🙂

**Behöver jag en API-nyckel om jag redan har kört `butler login`?**
Lokalt nej. Men CI-flödet behöver en egen nyckel (steg 4), eftersom GitHub inte kan använda din dators inloggning.
Skapa en separat nyckel så kan du återkalla CI-nyckeln utan att påverka din dator.

**Får jag klistra in API-nyckeln i en chatt, ett issue eller en fil?**
Nej, aldrig. Ger du `gh secret set` en nyckel tar GitHub emot den direkt, och den visas aldrig i loggen.
Har en nyckel hamnat någonstans den inte borde: gå till itch.io → API keys och återkalla den, och skapa en ny.

**Workflowet har en blå bock, inte en grön. Gick det fel?**
Nej. Med färgblindsvänligt tema visar GitHub **blått** för lyckat och **orange eller rött** för fel.
En gul prick betyder att det pågår.

**Jag har ett färdigt bygge men inget syns på itch.io.**
Det tar en stund innan itch har behandlat uppladdningen. Kolla `butler status <användare>/<slug>`.
Är sidan i läge `Draft` syns den bara för dig, via den hemliga länken.

**Spelet startar men jag hör inga ljud (Linux).**
Det kan bero på vilken användare du kör spelet som. Ljud följer den inloggade skrivbordssessionen.
Kör du spelet som en annan användare än den som är inloggad på skärmen
(till exempel `sudo -u` eller `su`) får det en tom "Dummy Output" i stället för högtalarna.
Starta spelet från din egen session i stället.

**Windows säger "Windows skyddade din dator" när jag startar spelet.**
Det är SmartScreen. Programmet är inte kodsignerat (det kostar pengar), så Windows vet inte vem som gjort det.
Klicka *Mer information → Kör ändå*.

## Säkerhet

- **Secrets:** `BUTLER_API_KEY` ligger som GitHub-secret och skrivs aldrig ut i loggen.
- **CodeQL** analyserar koden, **Dependabot** föreslår uppdateringar, och GitHubs
  **secret scanning med push protection** blockerar försök att pusha nycklar.

## Om projektet

Planen och byggordningen finns i [PLAN.md](PLAN.md). Spelet byggdes i små sprintar,
och git-historiken visar i vilken ordning sakerna kom till.
