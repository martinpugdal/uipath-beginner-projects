# Venteliste-oprydning

UiPath-proces (REFramework) der gennemgår en venteliste i Excel og afgør, om hver række skal fjernes, beholdes eller sendes til manuel gennemgang. Resultatet skrives tilbage til Excel med status og fejlbesked.

## Teknisk overblik

| | |
|---|---|
| **Type** | UiPath Process (REFramework) |
| **Sprog** | Visual Basic |
| **Target** | Windows, .NET 10 |
| **Transaktionskilde** | Excel/DataTable (`DataRow` pr. transaktion) |
| **Input** | `Data/Venteliste.xlsx` (ark `Ark1`) |
| **Config** | `Data/Config.xlsx` |
| **Nøgle-workflows** | `Main.xaml`, `Framework/InitAllSettings.xaml`, `Framework/GetTransactionData.xaml`, `Framework/Process.xaml`, `Framework/EndProcess.xaml` |

## 1. Hvad var opgaven?

Opgaven var at automatisere oprydning på en venteliste, så patienter/rækker behandles efter faste regler. Målet var at reducere manuelt arbejde og sikre ensartet behandling: fjerne rækker der ikke længere skal være på ventelisten, beholde aktive ventende, og markere problematiske rækker til manuel gennemgang.

## 2. Hvordan byggede du den op?

Processen er bygget som en REFramework-lignende DataRow-proces:

- **Config (`Data/Config.xlsx`)** indeholder indstillinger som inputfil, max antal dage på venteliste, timeout og logfil.
- **GetTransactionData** læser ventelisten første gang, sikrer kolonnerne `Resultat` og `Fejlbesked`, og returnerer næste række som transaktion.
- **Process** anvender forretningsregler pr. række:
  - **Validering:** CPR og Navn skal være udfyldt. Henvisningsdato skal kunne tolkes (DateTime eller dato-streng). Ved valideringsfejl skrives `Manuel gennemgang` + fejlbesked, og der kastes Business Rule Exception.
  - **Aldersberegning:** Dage på venteliste beregnes som forskel mellem i dag og henvisningsdato.
  - **Beslutningsregler:**
    - `StatusISystem = Behandlet` → `Fjernet`
    - `StatusISystem = Aflyst` → `Fjernet`
    - `dage > MaxAntalDagePaaVenteliste` → `Fjernet`
    - `StatusISystem = Ventende` → `Beholdt`
    - ellers → `Manuel gennemgang`
  - Resultat og tom fejlbesked skrives tilbage til Excel; processen logger behandling med CPR, navn, status, dage og resultat.
- **EndProcess** afslutter og samler op på kørslen.

## 3. Hvilke udfordringer stødte du på?

- **Datovalidering:** Henvisningsdato kan komme som DateTime eller tekst i forskellige formater. Processen forsøger flere parse-strategier, men datoer er stadig en typisk fejlkilde.
- **Forretningsregler og rækkefølge:** Reglerne skal prioriteres korrekt. Fx skal en behandlet/aflyst eller for gammel række fjernes, mens kun relevante ventende rækker beholdes.
- **Excel som både input og output:** Samme fil bruges som datakilde og resultatlog. Det er enkelt, men kræver præcis række-mapping og kan give låse/konkurrensproblemer i drift.
- **Config-konsistens:** Der er risiko for mismatch mellem config-nøgler og de nøgler workflows forventer (fx filsti/sheet-navn). Det er en vigtig driftsrisiko, fordi processen så fejler tidligt eller læser forkert input.

## 4. Hvad ville du gøre anderledes i en rigtig driftsopsætning?

- **Bruge Queue i stedet for Excel-rækker:** Læg hver ventelistepost i en Orchestrator Queue for bedre transaktionssporbarhed, retries, genoptagelse og skalerbarhed.
- **Separere input, output og audit:** Behold input urørt, skriv resultat til en separat rapport/database og gem en audit-log med regelbegrundelse, tidspunkt og bruger/robot-id.
- **Centralisere config og credentials i Orchestrator:** Brug Assets for miljøafhængige værdier og sikre naming-konventioner, så config og kode ikke divergerer.
- **Styrke validering og datakontrakt:** Definér tydeligt skema for CPR, datoformat, tilladte statusser og obligatoriske felter; afvis eller rout fejl til en manuel kø.
- **Bedre observability:** Strukturerede logs, metrics (antal fjernet/beholdt/manuel), alerts ved mange BRE/system exceptions og tydelige dashboards i Orchestrator.
- **Testautomatisering:** Unit-/workflow-tests for hver regelgren, edge cases på datoer og ugyldige rækker; kør validate/analyze i CI før release.
