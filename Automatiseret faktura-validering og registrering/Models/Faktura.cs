namespace AutomatiseretFakturaValideringOgRegistrering.Models;

public class Faktura
{
    public string FakturaNr { get; set; } = string.Empty;
    public string Leverandør { get; set; } = string.Empty;
    public string Dato { get; set; } = string.Empty;
    public string Beløb { get; set; } = string.Empty;
    public string Valuta { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Fejlbesked { get; set; } = string.Empty;
}
