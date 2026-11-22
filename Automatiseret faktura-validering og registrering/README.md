# Automatiseret faktura-validering og registrering

UiPath-proces (REFramework) til validering af fakturaer. Projektet er opdelt i en Dispatcher, der lægger fakturaer i en Orchestrator Queue, og en Performer (Main/REFramework), der behandler dem transaktionsvist.

## Teknisk overblik

| | |
|---|---|
| **Type** | UiPath Process (REFramework) |
| **Sprog** | C# |
| **Target** | Windows, .NET 10 |
| **Transaktionskilde** | Orchestrator Queue (`Fakturaer`) |
| **Input** | `Data/Fakturaer.xlsx` via `Dispatcher.xaml` |
| **Config** | `Data/Config.json` |
| **Nøgle-workflows** | `Dispatcher.xaml`, `Main.xaml`, `Framework/GetTransactionData.xaml`, `Framework/Process.xaml`, `Framework/SetTransactionStatus.xaml` |

## 1. Hvad var opgaven?

Opgaven var at automatisere validering og registrering af fakturaer, så hver faktura behandles ensartet og med tydelig fejlhåndtering. Processen skulle:

- læse fakturaer fra et input (Excel) og oprette queue-items;
- hente fakturaer én ad gangen som transaktioner;
- validere forretningsregler (obligatoriske felter, beløb, dato/alder);
- markere transaktionen som succes eller Business Rule Exception i REFramework-flowet.

## 2. Hvordan byggede du den op?

Løsningen følger REFramework-mønsteret med Dispatcher/Performer:

- **Dispatcher (`Dispatcher.xaml`)** læser `Data/Fakturaer.xlsx` og opretter queue-items i køen `Fakturaer` med felterne `FakturaNr`, `Leverandør`, `Dato`, `Beløb` og `Valuta`.
- **Init (`Framework/InitAllSettings.xaml` + `InitAllApplications.xaml`)** indlæser `Data/Config.json` og forbereder applikationer. Config indeholder bl.a. kønavn, max fakturaalder, logfil og retry-constanter.
- **Get Transaction (`Framework/GetTransactionData.xaml`)** henter næste `QueueItem` fra Orchestrator-køen. Den sætter `TransactionID` til `FakturaNr` og logfelter til dato/beløb for sporbarhed.
- **Process (`Framework/Process.xaml`)** udtrækker felter fra `SpecificContent` og validerer:
  - `FakturaNr` og `Leverandør` skal være udfyldt.
  - `Beløb` skal kunne parses og være > 0.
  - `Dato` skal kunne parses (da-DK/invariant) og må ikke være ældre end `MaxAlderDage`.
  - Ved brud på regler kastes `BusinessRuleException` med konkret årsag.
- **SetTransactionStatus** opdaterer transaktionsstatus i Orchestrator/REFramework og logger resultatet. System exceptions går gennem frameworkets retry/screenshot-flow.

## 3. Hvilke udfordringer stødte du på?

- **Datakvalitet og formater:** Beløb og dato kan komme i forskellige formater. Løsningen håndterer dette med trimming, decimal-parsing og dato-parsing med både invariant og `da-DK`.
- **Tydelig BRE vs. systemfejl:** Forretningsfejl (manglende felter, ugyldigt beløb, for gammel faktura) skal ikke retries som systemfejl. Derfor kastes `BusinessRuleException` eksplicit i `Process.xaml`.
- **Kø-baseret drift:** Dispatcher og Performer skal være enige om kønavn og feltnavne i `SpecificContent`. Små afvigelser giver runtime-fejl eller manglende data.
- **Konfiguration:** Nogle værdier ligger i Config, mens Dispatcher stadig har hardcodede værdier (fx filsti og QueueName). Det kræver disciplin at holde config og workflows synkroniseret.

## 4. Hvad ville du gøre anderledes i en rigtig driftsopsætning?

- **Flytte hardcodede værdier til Config/Assets:** QueueName, queue folder, inputsti og sheet-navn bør læses fra config eller Orchestrator Assets, så miljøskift ikke kræver kodeændringer.
- **Bruge Orchestrator Queue + Assets fuldt ud:** Credentials, miljø-URL’er og følsomme værdier skal ligge i Assets/Credential Assets. Køen bør have tydelig retry-policy og retention.
- **Stærkere logging og audit:** Skrive strukturerede logs (TransactionID, regel, resultat, tidspunkt) til en central logdestination og evt. et business-log-output. Tilføje tydelig exception-type og korrelations-id.
- **Idempotens og duplikatkontrol:** Bruge unik reference (fx FakturaNr) ved `Add Queue Item` for at undgå dobbeltbehandling.
- **Status-writeback:** Opdatere kilde-Excel/database eller et fakturasystem med behandlingsstatus, fejlbesked og tidsstempel.
- **Tests og kvalitetsgate:** Udbygge testcases for gyldig faktura, manglende felter, ugyldigt beløb, ugyldig dato, for gammel dato og queue-fejl. Køre `uip rpa validate` og `uip rpa analyze` som fast del af ændringer.
- **Robust dokumenthåndtering:** I drift ville fakturaer typisk komme som PDF/mail/API; her kunne Document Understanding eller validering mod ERP-data indgå.
