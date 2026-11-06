# 🌙 KeePass-DarkMode

<p align="center">
  <a href="https://github.com/jimmybonesde/KeePass-DarkMode/releases/latest"><img src="https://img.shields.io/github/v/release/jimmybonesde/KeePass-DarkMode?style=flat-square&label=Version&color=blue" alt="Version"></a>
  <a href="https://github.com/jimmybonesde/KeePass-DarkMode/releases/latest/download/KeeTheme.plgx"><img src="https://img.shields.io/github/downloads/jimmybonesde/KeePass-DarkMode/total?style=flat-square&label=Downloads&color=green" alt="Downloads"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-green?style=flat-square" alt="License"></a>
  <a href="https://github.com/jimmybonesde/KeePass-DarkMode/actions"><img src="https://img.shields.io/github/actions/workflow/status/jimmybonesde/KeePass-DarkMode/build.yml?style=flat-square&label=Build" alt="Build"></a>
</p>

<p align="center">
  <strong>Dunkles Theme für KeePass — augenfreundlich, auch nachts.</strong><br>
  Kein Umstieg, kein Verlust: Alle Funktionen bleiben erhalten, nur die Optik ändert sich.
</p>

---

## ⚡ Schnellstart

```
STRG+T    Theme umschalten
```

oder über das Menü: **Tools → DarkTheme**

---

## ✨ Features

| | |
|---|---|
| 🌑 **Dunkles Theme** | Moderne, ruhige Farbpalette für die gesamte KeePass-Oberfläche |
| ⌨️ **Hotkey** | `STRG+T` zum schnellen Umschalten — frei konfigurierbar |
| 🎨 **Theme-Editor** | Eigene Farbwelten ohne Editieren von Textdateien erstellen |
| 📦 **Plgx-Format** | Einfache Installation, Update über die KeePass-eigene Plugin-Funktion |
| 🪟 **Win11-Unterstützung** | Eigenes Theme für Windows-11-Optik (`DarkThemeWin11.ini`) |

---

## 🚀 Installation

### Methode 1: Manuell

1. [KeeTheme.plgx](https://github.com/jimmybonesde/KeePass-DarkMode/releases/latest/download/KeeTheme.plgx) herunterladen
2. In den Plugin-Ordner kopieren (Standard: `C:\Program Files (x86)\KeePass Password Safe 2\Plugins\`)
3. KeePass neu starten — fertig

Alternativ funktioniert auch die klassische Variante über **Tools → Plugins → Add…**, einfach die `.plgx` dort reinziehen.

### Methode 2: Über Chocolatey

```powershell
choco install keepass-plugin-keetheme
```

KeePass danach einmal neu starten.

---

## 🎨 Eigene Themes

Mit dem eingebauten Theme-Editor erstellst du im Handumdrehen eigene Farbwelten:

![Theme-Editor](docs/KeePassDarkThemeEditor.png)

Eigene Theme-Dateien (`.ini`) einfach in den Plugin-Ordner legen — sie erscheinen dann automatisch in der Theme-Auswahl unter **Tools → Optionen → KeeTheme**.

Zwei Startpunkte liegen bei:

| Theme | Beschreibung |
|---|---|
| `DarkTheme.ini` | Klassisches dunkles Theme |
| `DarkThemeWin11.ini` | An Windows-11-Flair angepasst |

---

## ⚠️ Hinweise

KeePass basiert auf klassischen Windows-Controls, die sich nur eingeschränkt umfärben lassen. Einzelne Bereiche können deshalb weiterhin hell erscheinen. Das Plugin ist so ausgereift wie möglich — gut genug für den Alltag, aber kein kompromissloses Pixel-Perfect-Theming.

---

## 🛠️ Entwicklung

```bash
# Build unter Windows (benötigt .NET Framework 4.x SDK)
msbuild KeeTheme.sln /p:Configuration=Release
```

Der [CI-Workflow](.github/workflows/build.yml) baut automatisch bei jedem Push — KeePass wird dazu automatisch als Referenz-Assembly heruntergeladen.

---

## 📄 Lizenz

[MIT](LICENSE) — Nutzung, Bearbeitung und Weitervergabe frei.

---

*Basierend auf [KeeTheme](https://github.com/xatupal/KeeTheme) von Krzysztof Łaputa — Dank an den Originalautor.*
