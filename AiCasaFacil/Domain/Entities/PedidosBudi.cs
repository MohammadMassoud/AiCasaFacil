namespace AiCasaFacil.Domain.Entities
{
    public class PedidosBudi
    {
        public string NumeroPedido { get; set; } = string.Empty;
        public string FormaEntrega { get; set; }
        public string NomeCliente { get; set; } = string.Empty;
        public DateTime DataPedido { get; set; }
        public DateTime DataEntregaBudi { get; set; }
        public bool isDevolucao { get; set; }
        public List<PedidoBudiItem> Itens { get; set; } = new();
    }
    public class PedidoBudiItem
    {
        public string NumeroPedido { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string NomeProduto { get; set; } = string.Empty;
        public decimal Custo { get; set; } 
        public int Quantidade { get; set; }
    }

}
