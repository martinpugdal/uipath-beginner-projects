# UiPath Beginner Projects

En lille samling af tre UiPath-projekter, bygget som øvelser i REFramework, transaktionshåndtering, validering og UI-automation. Repoet er tænkt som et begyndervenligt portfolio, der kan gennemgås i en samtale eller åbnes i UiPath Studio.

## Projekter

| Projekt | Fokus | Transaktionstype | Vigtige teknologier |
|---|---|---|---|
| [Automatiseret faktura-validering og registrering](./Automatiseret%20faktura-validering%20og%20registrering/) | Fakturavalidering med forretningsregler | Orchestrator Queue (`QueueItem`) | REFramework, Dispatcher/Performer, C#, Config.json, Business Rule Exceptions |
| [PatientOverfoersel](./PatientOverfoersel/) | Overførsel af patientdata til webformular | Excel/DataTable (`DataRow`) | REFramework, VB, UI Automation, NApplicationCard, Excel |
| [Venteliste-oprydning](./Venteliste-oprydning/) | Regelbaseret oprydning af venteliste | Excel/DataTable (`DataRow`) | REFramework, VB, datavalidering, forretningsregler, Excel |

## Hvad projekterne viser

- **REFramework-mønster:** Initialization, Get Transaction Data, Process Transaction, End Process.
- **Fejlhåndtering:** Forskel på Business Rule Exceptions og System Exceptions.
- **Config-drevet design:** Indstillinger læses fra config-filer frem for hardcoded værdier (med plads til forbedring).
- **Datavalidering:** Tjek af obligatoriske felter, datoer, beløb og statusser.
- **UI Automation:** Browserinteraktion i PatientOverfoersel med UiPath Modern UI.
- **Drift-tankegang:** Hvert projekt indeholder en README med “Hvad ville du gøre anderledes i en rigtig driftsopsætning?”.

## Kom godt i gang

### Forudsætninger

- Windows
- UiPath Studio 26.10+ eller kompatibel version
- Adgang til nødvendige UiPath-pakker (restore dependencies ved åbning)
- For Faktura-projektet: Orchestrator Queue/setup hvis Dispatcher/Performer skal køres end-to-end

### Åbn et projekt

1. Klon eller download repoet.
2. Åbn den ønskede projektmappe i UiPath Studio.
3. Lad Studio restore dependencies.
4. Gennemgå `Data/Config.json` eller `Data/Config.xlsx` før kørsel.
5. Læs projektets egen `README.md` for opgave, opbygning, udfordringer og driftsforbedringer.

## Struktur

```text
.
├── Automatiseret faktura-validering og registrering/  # Queue-baseret fakturavalidering
├── PatientOverfoersel/                                # Excel + browserautomation
├── Venteliste-oprydning/                              # Regelbaseret Excel-oprydning
└── README.md
```

## Noter

- Projekterne er **lærings-/demoprojekter** og bruger testdata/demo-UI.
- Før produktionsbrug bør de hårdnes med bl.a. Orchestrator Assets, Queue-baseret transaktionshåndtering hvor relevant, bedre logging/audit, idempotens og tests/CI.
- Se de enkelte projekt-README’er for detaljeret dokumentation.