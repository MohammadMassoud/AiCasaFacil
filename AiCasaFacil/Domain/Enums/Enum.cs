namespace AiCasaFacil.Domain.Enums;

public class Enum
{
    public enum SituacaoPedido
    {
        Pendente = 0,
        Aprovada = 1,
        Cancelada = 2,
        Devolvida = 3,
        Reembolsada = 4
    }

    public enum PlataformaImportacao
    {
        ML,
        Tiktok,        
        Magalu,
        Budi
    }
}
