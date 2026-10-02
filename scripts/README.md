# Skript

## `publish_local.sh`

| | |
|---|---|
| **Vad den gör** | Bygger spelet för Windows och Linux (`build/win-x64`, `build/linux-x64`). Med en version som argument pushar den också till itch.io med butler. |
| **Hur du kör den** | `./scripts/publish_local.sh` (bara bygga) eller `./scripts/publish_local.sh 0.3.0` (bygga och pusha) |
| **Beroenden** | .NET 10 SDK. För push: `butler` installerat och inloggad (`butler login`). |
| **Vem kör den** | Utvecklaren, för manuella tester. Vanliga releaser görs av GitHub Actions (se `README.md`). |
