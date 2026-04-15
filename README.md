# Vision Edit

> KI-gestützte Textbearbeitungs-App mit OpenAI-Integration, gebaut mit .NET MAUI

---

## Voraussetzungen

Folgende Software muss auf dem Ziel-PC installiert sein:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [MySQL Server 8.x](https://dev.mysql.com/downloads/)
- Visual Studio 2022 (v17.12+) mit MAUI-Workload **oder** dotnet CLI
- Git

---

## OpenAI API-Key

Die App benötigt einen gültigen OpenAI API-Key (GPT-4o & GPT-4o-mini).

- Key erstellen: https://platform.openai.com/api-keys
- Der Key wird **nicht** im Quellcode hinterlegt, sondern direkt in der App eingetragen (siehe Schritt 7).

> ⚠️ **Niemals** den API-Key in ein öffentliches Repository einchecken.

---

## Installation

### 1. Repository klonen

```bash
git clone https://github.com/Werzu25/Vision_Edit.git
cd Vision_Edit
```

### 2. Datenbankverbindung anpassen

Öffne `ORM/DbManager.cs` und passe die Verbindungszeichenkette in `OnConfiguring()` an:

```csharp
Server=localhost;Database=vision_edit;User=DEIN_USER;Password=DEIN_PASSWORT
```

| Parameter  | Beschreibung                          | Standard (Entwicklung) |
|------------|---------------------------------------|------------------------|
| `Server`   | Hostname des MySQL-Servers            | `localhost`            |
| `Database` | Datenbankname                         | `vision_edit`          |
| `User`     | MySQL-Benutzername                    | `root`                 |
| `Password` | MySQL-Passwort                        | *(leer lassen / anpassen)* |

### 3. Datenbank erstellen

```bash
cd ORM
dotnet ef database update
```

Erstellt alle Tabellen (`Users`, `Documents`) automatisch via EF Core Migrations.

### 4. Solution bauen

```bash
cd ..
dotnet build
```

### 5. API starten

```bash
cd "Vision Edit API"
dotnet run
```

Die API läuft auf **https://localhost:44311**. Diese URL ist im MAUI-Client als Basisadresse hinterlegt.

### 6. MAUI-App starten

```bash
cd "Vision Edit"
dotnet run -f net10.0-windows10.0.19041.0
```

### 7. OpenAI API-Key eintragen

Beim ersten Start erscheint ein Dialog zur Eingabe des API-Keys. Den Key von https://platform.openai.com/api-keys eintragen. Er wird lokal gespeichert.

---

## Sensible Daten – Checkliste

- [ ] Verbindungszeichenkette in `ORM/DbManager.cs` angepasst (kein Klartext-Passwort im Repo)
- [ ] Kein OpenAI API-Key im Quellcode
- [ ] `appsettings.json` enthält keine Geheimnisse
- [ ] `bin/` und `obj/` Ordner vor dem Weitergeben gelöscht

---

## Häufige Probleme

**MySQL-Fehler beim Start**  
→ Prüfe, ob der MySQL-Dienst läuft und ob die Zugangsdaten in `DbManager.cs` korrekt sind.

**Port-Konflikt (44311 belegt)**  
→ Passe die URL in `Vision Edit API/Properties/launchSettings.json` und in `Vision Edit/MauiProgram.cs` (HttpClient-Basisadresse) an.

**MAUI-Build schlägt fehl**  
→ MAUI-Workload installieren:
```bash
dotnet workload install maui
```

---

## Projektstruktur

```
Vision_Edit/
├── Vision Edit/          # .NET MAUI Client (UI)
├── Vision Edit API/      # ASP.NET Core Web API
├── Models/               # Shared DTOs & Entities
├── ORM/                  # EF Core + MySQL (DbManager)
├── Tools/                # OpenAI SDK, UserManager, Validation
└── dokus/
    ├── README.md
    ├── Projektbeschreibung.docx
    └── Installation.docx
```
