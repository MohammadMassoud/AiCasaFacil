using AiCasaFacil.Application.Interfaces;
using AiCasaFacil.Domain.Entities;
using AiCasaFacil.Infrastructure.AI;
using System.Text;

namespace AiCasaFacil.Application.Services;

public class PedidoIAService
{
    private readonly IAnalisePedidosService _analise;
    private readonly OllamaService _ollama;
    private readonly PedidoSessaoService _sessao;
    private readonly List<(string Papel, string Mensagem)> _historico = new();

    public PedidoIAService(
        IAnalisePedidosService analise,
        OllamaService ollama,
        PedidoSessaoService sessao)
    {
        _analise = analise;
        _ollama = ollama;
        _sessao = sessao;
    }

    public IReadOnlyList<(string Papel, string Mensagem)> Historico => _historico;

    public void LimparHistorico() => _historico.Clear();

    public async Task<string> PerguntarAsync(string pergunta)
    {
        var pedidos = _sessao.Pedidos.ToList();
        var pedidosBudi = _sessao.PedidosBudi.ToList();

        if (!pedidos.Any() && !pedidosBudi.Any())
            return "Nenhum pedido carregado. Por favor, importe uma planilha primeiro.";

        var sb = new StringBuilder();

        sb.AppendLine("Você é um consultor financeiro especialista em e-commerce brasileiro.");
        sb.AppendLine("Seu papel é ajudar pequenos vendedores de marketplace a entender seus números e tomar decisões melhores.");
        sb.AppendLine("Ao analisar os dados:");
        sb.AppendLine("- Identifique padrões, oportunidades e riscos");
        sb.AppendLine("- Dê sugestões práticas e objetivas");
        sb.AppendLine("- Use os números para justificar suas recomendações");
        sb.AppendLine("- Responda sempre em português");
        sb.AppendLine();

        sb.AppendLine("Você pode:");
        sb.AppendLine("- Comparar produtos entre si");
        sb.AppendLine("- Sugerir quais produtos focar ou abandonar");
        sb.AppendLine("- Identificar se as taxas estão altas demais");
        sb.AppendLine("- Avaliar se o Flex compensa financeiramente");
        sb.AppendLine("- Dar recomendações de negócio baseadas nos dados");
        sb.AppendLine();
        sb.AppendLine("Use markdown na sua resposta: **negrito** para destacar números e produtos importantes, listas para sugestões e tópicos.");

        // Dados financeiros dos pedidos ML/TikTok/Magalu
        if (pedidos.Any())
        {
            var lucroTotal = _analise.CalcularLucroTotal(pedidos);
            var valorTotalVendas = _analise.ValorTotalVendas(pedidos);
            var mediaMargem = _analise.MediaMargem(pedidos);
            var totalFlex = _analise.TotalFlex(pedidos);
            var totalTaxa = _analise.TotalTaxa(pedidos);
            var houveDevolucao = _analise.HouveDevolucao(pedidos);
            var resumoProdutos = _analise.LucrosPorProdutos(pedidos);

            sb.AppendLine("=== RESUMO FINANCEIRO DOS PEDIDOS ===");
            sb.AppendLine($"Total de pedidos: {pedidos.Count}");
            sb.AppendLine($"Valor total vendido: R$ {valorTotalVendas:N2}");
            sb.AppendLine($"Lucro total: R$ {lucroTotal:N2}");
            sb.AppendLine($"Margem média: {mediaMargem:N2}%");
            sb.AppendLine($"Total pago em Flex: R$ {totalFlex:N2}");
            sb.AppendLine($"Total pago em taxas: R$ {totalTaxa:N2}");
            sb.AppendLine($"Houve devolução: {(houveDevolucao ? "Sim" : "Não")}");
            sb.AppendLine();

            sb.AppendLine("=== DESEMPENHO POR PRODUTO ===");
            foreach (dynamic p in resumoProdutos)
            {
                sb.AppendLine(
                    $"Código: {p.Codigo} | " +
                    $"Produto: {p.Produto} | " +
                    $"Qtd: {p.Quantidade} | " +
                    $"valor médio: {p.PrecoMedio} | " +
                    $"Custo: R$ {p.Custo:N2} | " +
                    $"Lucro: R$ {p.Lucro:N2} | " +
                    $"Margem: {p.Margem:N2}%"
                );
            }
            sb.AppendLine();
        }

        // Dados do comparativo Budi
        if (pedidosBudi.Any() && pedidos.Any())
        {
            sb.AppendLine("=== COMPARATIVO ML x BUDI ===");

            var numerosML = pedidos.Select(p => p.NumeroPedido).ToHashSet();
            var numerosBudi = pedidosBudi.Select(p => p.NumeroPedido).ToHashSet();

            var somenteML = numerosML.Except(numerosBudi).Count();
            var somenteBudi = numerosBudi.Except(numerosML).Count();
            var emAmbos = numerosML.Intersect(numerosBudi).Count();

            sb.AppendLine($"Pedidos em ambos (OK): {emAmbos}");
            sb.AppendLine($"Somente no ML: {somenteML}");
            sb.AppendLine($"Somente no Budi: {somenteBudi}");
            sb.AppendLine();
        }

        // Histórico da conversa
        if (_historico.Any())
        {
            sb.AppendLine("=== HISTÓRICO DA CONVERSA ===");
            foreach (var (papel, mensagem) in _historico)
                sb.AppendLine($"{papel}: {mensagem}");
            sb.AppendLine();
        }

        sb.AppendLine($"Usuário: {pergunta}");
        sb.AppendLine("Assistente:");

        var resposta = await _ollama.GenerateAsync(sb.ToString());

        _historico.Add(("Usuário", pergunta));
        _historico.Add(("Assistente", resposta));

        return resposta;
    }
}