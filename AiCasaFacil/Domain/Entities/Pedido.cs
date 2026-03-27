using AiCasaFacil.Application.Interfaces;
using Microsoft.AspNetCore.Components;

namespace AiCasaFacil.Domain.Entities
{
    public class Pedido
    {        
        public string NumeroPedido { get; set; } = string.Empty;
        public DateTime DataPedido { get; set; }
        public decimal ValorBruto { get; set; }
        public decimal ValorLiquido { get; set; }
        public decimal ValorTotal { get; set; }        
        public bool Devolvido { get; set; }
        public string FormaEntrega { get; set; }
        
        public decimal CustoTotal => Itens.Sum(i => i.Custo);
        public decimal Flex => FormaEntrega.Contains("Flex")? 12 : 0;
        public decimal Taxa => (ValorTotal - ValorLiquido) ;
        public decimal Lucro => (ValorLiquido - CustoTotal) - Flex;
        public decimal Margem => ValorBruto == 0        ? 0        : Math.Round((Lucro / ValorBruto) * 100, 2);

        public List<PedidoItem> Itens { get; set; } = new();
    }
    public class PedidoItem
    {
        public string PacoteId { get; set; } = string.Empty;
        public string NumeroPedido { get; set; } = string.Empty;
        public string Produto { get; set; } = string.Empty;
        public string NomeProduto { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal Custo { get; set; }        
    }
}
