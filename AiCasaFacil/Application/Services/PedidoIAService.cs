using AiCasaFacil.Application.Interfaces;
using AiCasaFacil.Domain.Entities;
using AiCasaFacil.Infrastructure.AI;
using System.Text;

namespace AiCasaFacil.Application.Services;

public class PedidoIAService
{
    private readonly IAnalisePedidosService _analise;
    private readonly OllamaService _ollama;
    private readonly List<string> _historico = new();

    public PedidoIAService(
        IAnalisePedidosService analise,
        OllamaService ollama)
    {
        _analise= analise;
        _ollama = ollama;
    }

    public async Task<string> PerguntarAsync(
        List<Pedido> pedidos,
        string pergunta)
    {
        if (pedidos == null || pedidos.Count == 0)
            return "Nenhum pedido carregado para análise.";

        var sb = new StringBuilder();

        // 🔹 Cálculos seguros no sistema
        var totalPedidos = pedidos.Count;
        var lucroTotal = _analise.CalcularLucroTotal(pedidos);
        var houveDevolucao = _analise.HouveDevolucao(pedidos);
        var valorTotalVendas = _analise.ValorTotalVendas(pedidos);
        //var valorTotalDevido = _analise.ValorTotalDevido(pedidos);
        var mediaMargem = _analise.MediaMargem(pedidos);
        var totalFlex = _analise.TotalFlex(pedidos);
        var totalTaxa = _analise.TotalTaxa(pedidos);


        //var topProdutos = pedidos
        //    .GroupBy(p => p.Codigo)
        //    .Select(g => new
        //    {
        //        Codigo = g.Key,
        //        Lucro = g.Sum(x => x.Lucro),
        //        Quantidade = g.Sum(x => x.Quantidade)
        //    })
        //    .OrderByDescending(x => x.Lucro)
        //    .Take(3)
        //    .ToList();

        //var piorProduto = pedidos
        //    .GroupBy(p => p.Codigo)
        //    .Select(g => new
        //    {
        //        Codigo = g.Key,
        //        Lucro = g.Sum(x => x.Lucro)
        //    })
        //    .OrderBy(x => x.Lucro)
        //    .FirstOrDefault();

        // 🔹 Monta prompt estruturado        
        sb.AppendLine("Você é um analista financeiro especialista em e-commerce.");
        sb.AppendLine("Analise os dados abaixo e responda a pergunta do usuário.");
        //sb.AppendLine("Regras:");
        //sb.AppendLine("- Responda apenas o que foi perguntado.");
        //sb.AppendLine("- Não dê sugestões.");
        //sb.AppendLine("- Não faça análises adicionais.");
        //sb.AppendLine("- Não Inventa.");
        sb.AppendLine();
        sb.AppendLine("Resumo financeiro atual:");
        sb.AppendLine($"Total de pedidos: {totalPedidos}");
        sb.AppendLine($"Lucro total: R$ {lucroTotal:N2}");
        sb.AppendLine($"Valor total vendido (receita): R$ {valorTotalVendas:N2}");
        //sb.AppendLine($"Custo total dos produtos: R$ {valorTotalDevido:N2}");
        sb.AppendLine($"Margem média: {mediaMargem:N2}%");
        sb.AppendLine($"Total pago em Flex: R$ {totalFlex:N2}");
        sb.AppendLine($"Total pago em taxas: R$ {totalTaxa:N2}");
        sb.AppendLine($"Houve devolução: {(houveDevolucao ? "Sim" : "Não")}");
        sb.AppendLine();

        sb.AppendLine();

        //var resumoProdutos = _analise.LucrosPorProdutos(pedidos);

        sb.AppendLine("Resumo de desempenho por produto:");

        //foreach (dynamic p in resumoProdutos)
        //{
        //    sb.AppendLine(
        //        $"Código: {p.Codigo} | " +
        //        $"Descrição: {p.Produto} | " +
        //        $"Custo total: R$ {p.Custo :N2} | " +
        //        $"Lucro total: R$ {p.Lucro:N2} | " +
        //        $"Quantidade vendida: {p.Quantidade} | " +
        //        $"Valor líquido total: R$ {p.ValorLiquido:N2}"
        //    );
        //}

        //sb.AppendLine("Top produtos por lucro:");
        //foreach (var p in topProdutos)
        //{
        //    sb.AppendLine($"- {p.Codigo} | Quantidade: {p.Quantidade} | Lucro: R$ {p.Lucro:N2}");
        //}

        //if (piorProduto != null)
        //{
        //    sb.AppendLine();
        //    sb.AppendLine($"Produto com menor lucro: {piorProduto.Codigo} | Lucro: R$ {piorProduto.Lucro:N2}");
        //}

        //sb.AppendLine("Esses são todos meus pedidos");
        //foreach (var p in pedidos)
        //{
        //    sb.AppendLine($"Codigo {p.Codigo} | Descricao: {p.Produto} | Lucro: R$ {p.Lucro:N2} | Margem: R$ {p.Margem:N2}");
        //}

        sb.AppendLine();
        sb.AppendLine("Histórico da conversa:");
        foreach (var item in _historico)
        {
            sb.AppendLine(item);
        }

        sb.AppendLine();
        sb.AppendLine($"Usuário: {pergunta}");

        var resposta = await _ollama.GenerateAsync(sb.ToString());

        _historico.Add($"Usuário: {pergunta}");
        _historico.Add($"Assistente: {resposta}");

        return resposta;
    }
}
