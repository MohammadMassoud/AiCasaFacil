namespace AiCasaFacil.Domain.Entities
{
    public class Produto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorUnitario { get; set; }
    }
}
