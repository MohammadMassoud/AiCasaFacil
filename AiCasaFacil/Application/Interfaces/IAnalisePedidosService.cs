using AiCasaFacil.Domain.Entities;

namespace AiCasaFacil.Application.Interfaces;

public interface IAnalisePedidosService
{
    decimal CalcularLucroTotal(List<Pedido> pedidos);
    int TotalVendidoProduto(List<Pedido> pedidos, string codigo);
    bool HouveDevolucao(List<Pedido> pedidos);
    decimal LucroPorProduto(List<Pedido> pedidos, string codigoProduto);
    decimal ValorTotalVendas(List<Pedido> pedidos);
    decimal ValorTotalDevido(List<Pedido> pedidos);
    decimal MediaMargem(List<Pedido> pedidos);
    decimal TotalFlex(List<Pedido> pedidos);
    int QuantidadePedidosFlex(List<Pedido> pedidos);
    decimal TotalTaxa(List<Pedido> pedidos);

    IEnumerable<object> LucrosPorProdutos(List<Pedido> pedidos);
}
