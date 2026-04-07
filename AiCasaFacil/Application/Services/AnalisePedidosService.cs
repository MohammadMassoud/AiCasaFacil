using AiCasaFacil.Application.Interfaces;
using AiCasaFacil.Domain.Entities;
using System.Collections;

namespace AiCasaFacil.Application.Services;

public class AnalisePedidosService : IAnalisePedidosService
{
    public decimal CalcularLucroTotal(List<Pedido> pedidos)
        => pedidos?.Sum(p => p.Lucro) ?? 0;

    public int TotalVendidoProduto(List<Pedido> pedidos, string codigo)
        => pedidos?
            .SelectMany(p => p.Itens)
            .Where(i => i.Codigo.Contains(codigo, StringComparison.OrdinalIgnoreCase))
            .Sum(i => i.Quantidade) ?? 0;


    public bool HouveDevolucao(List<Pedido> pedidos)
        => pedidos?.Any(p => p.Devolvido) ?? false;

    public decimal LucroPorProduto(List<Pedido> pedidos, string codigoProduto)
    {
        if (pedidos == null)
            return 0;

        decimal totalLucro = 0;

        foreach (var pedido in pedidos)
        {
            var itensProduto = pedido.Itens
                                     .Where(i => i.Codigo == codigoProduto)
                                     .ToList();

            if (!itensProduto.Any())
                continue;

            var proporcao = itensProduto.Sum(i => i.Quantidade) /
                            (decimal)pedido.Itens.Sum(i => i.Quantidade);

            totalLucro += pedido.Lucro * proporcao;
        }

        return totalLucro;
    }


    public decimal ValorTotalVendas(List<Pedido> pedidos)
        => pedidos?.Sum(p => p.ValorLiquido) ?? 0;

    public decimal ValorTotalDevido(List<Pedido> pedidos)
        => pedidos?.Sum(p => p.CustoTotal) ?? 0;

    public decimal MediaMargem(List<Pedido> pedidos)
        => pedidos != null && pedidos.Any()
            ? pedidos.Average(p => p.Margem)
            : 0;

    public decimal TotalFlex(List<Pedido> pedidos)
        => pedidos?.Sum(p => p.Flex) ?? 0;

    public int QuantidadePedidosFlex(List<Pedido> pedidos)
    => pedidos?.Count(p => p.Flex > 0) ?? 0;

    public decimal TotalTaxa(List<Pedido> pedidos)
        => pedidos?.Sum(p => p.Taxa) ?? 0;


    public IEnumerable<object> LucrosPorProdutos(List<Pedido> pedidos)
    {
        if (pedidos == null)
            return Enumerable.Empty<object>();

        return pedidos
            .SelectMany(p => p.Itens, (pedido, item) => new
            {
                pedido,
                item
            })
            .GroupBy(x => new { x.item.Codigo, x.item.NomeProduto })
            .Select(g =>
            {
                var pedidosGrupo = g.Select(x => x.pedido).Distinct();

                return new
                {
                    Codigo = g.Key.Codigo,
                    Produto = g.Key.NomeProduto,
                    Quantidade = g.Sum(x => x.item.Quantidade),
                    Custo = g.Sum(x => x.item.Custo),
                    PrecoMedio = pedidosGrupo.Average(x => x.ValorBruto),
                    ValorLiquido = pedidosGrupo.Sum(p => p.ValorLiquido),
                    Lucro = pedidosGrupo.Sum(p => p.Lucro),
                    Margem = pedidosGrupo.Sum(p => p.ValorLiquido) > 0? (pedidosGrupo.Sum(p => p.Lucro) / pedidosGrupo.Sum(p => p.ValorLiquido)) * 100: 0
                };
            })
            .OrderByDescending(x => x.Lucro);
    }

}
