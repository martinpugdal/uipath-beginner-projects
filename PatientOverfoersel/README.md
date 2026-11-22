# PatientOverfoersel

UiPath-proces (REFramework) der behandler patientrækker fra Excel og overfører data til en webformular. Processen validerer input, udfylder formularfelter i browseren og skriver resultat/fejlbesked tilbage til Excel.

## Teknisk overblik

| | |
|---|---|
| **Type** | UiPath Process (REFramework) |
| **Sprog** | Visual Basic |
| **Target** | Windows, .NET 10 |
| **Transaktionskilde** | Excel/DataTable (`DataRow` pr. transaktion) |
| **Input** | `Data/Patienter.xlsx` (konfigureres i `Data/Config.xlsx`) |
| **UI-automation** | UiPath.UIAutomation.Activities / NApplicationCard mod DemoQA-formular |
| **Nøgle-workflows** | `Main.xaml`, `Framework/InitAllSettings.xaml`, `Framework/GetTransactionData.xaml`, `Framework/Process.xaml`, `Framework/SetTransactionStatus.xaml` |

## 1. Hvad var opgaven?

Opgaven var at automatisere en enkel patientoverførsel: tage patientdata fra en kildefil, indtaste relevante felter i et formularsystem og registrere om behandlingen lykkedes eller fejlede. Processen skulle kunne håndtere flere patienter i træk og give sporbarhed pr. række.

## 2. Hvordan byggede du den op?

Løsningen er bygget som en REFramework-proces med Excel som transaktionskilde:

- **Config (`Data/Config.xlsx`)** styrer runtime-parametre som `ExcelPath`, `SheetName`, `FormUrl` og `MaxConsecutiveSystemExceptions`.
- **InitAllSettings** læser Config.xlsx ind i `Config`-dictionary.
- **GetTransactionData** indlæser Excel første gang, sikrer at kolonnerne `Resultat` og `Fejlbesked` findes, skriver headers og returnerer næste `DataRow` baseret på `TransactionNumber`.
- **Process**:
  - læser `Fornavn` og `Efternavn` fra transaktionsrækken;
  - validerer at obligatoriske felter er udfyldt; ved mangler skrives `Fejl` + fejlbesked til Excel, og der kastes en Business Rule Exception;
  - åbner browser via `NApplicationCard` og udfylder DemoQA-formularen (`firstName`, `lastName`) og klikker `submit`;
  - ved succes skrives `OK` i resultatkolonnen;
  - ved teknisk/UI-fejl skrives `Fejl` og exception-message, hvorefter fejlen rethrows til REFramework-håndtering.
- **SetTransactionStatus / Main** håndterer transaktionsstatus, tællere og afslutning efter REFramework-mønsteret.

## 3. Hvilke udfordringer stødte du på?

- **UI-stabilitet:** Browserautomation er følsom over for selectors, load-tider og UI-ændringer. NApplicationCard/selectors skal være præcise, og timeouts/ready-state betyder meget.
- **Data og række-mapping:** Når resultat skrives tilbage til Excel, skal rækkenummeret matche den korrekte transaktion (`in_TransactionNumber + 1` / header-offset). Det kræver konsistent håndtering af kolonner og rækkeindeks.
- **Fejltyper:** Manglende patientdata er en forretningsfejl, mens selector/timeouts typisk er systemfejl. Det er vigtigt at skelne, så REFramework kan retrye det rigtige.
- **Excel som transaktionskilde:** Excel er enkel at starte med, men giver udfordringer med samtidighed, låsning, audit og skalerbarhed.

## 4. Hvad ville du gøre anderledes i en rigtig driftsopsætning?

- **Bruge Orchestrator Queue i stedet for Excel-rækker:** Hver patient/henvisning som queue-item giver bedre retry, status, audit, parallelisering og driftsoverblik.
- **Flytte følsomme og miljøspecifikke værdier til Assets:** URL, credentials, filstier og miljøparametre bør ligge i Orchestrator Assets/Credential Assets.
- **Bedre logging og sporbarhed:** Logge TransactionID/CPR-maskeret reference, status, fejltype og tidsstempel centralt. Undgå persondata i logs.
- **Mere robust UI-automation:** Bruge Object Repository, stabile anchors, explicit waits og genoprettelse af browser/session ved systemfejl.
- **Sikrere dataskrivning:** Skrive resultat til en separat outputfil/database eller via transaktionsopdatering, så input ikke låses/korruptes.
- **Validering og PII-håndtering:** Validere CPR/navn-format, maskere persondata i logs/screenshots og sikre at løsningen følger GDPR/organisationskrav.
- **Test og deployment:** Tilføje testcases for happy path, manglende felter, UI-fejl og genkørsel; køre validate/analyze/build og bruge kontrolleret publish til Orchestrator.
