using AiCasaFacil.Application.Interfaces;
using AiCasaFacil.Domain.Entities;
using AiCasaFacil.Infrastructure.AI;
using System.Text;
using AiCasaFacil.Shared;

namespace AiCasaFacil.Application.Services;

public class ProdutoIAService
{
    private readonly IProdutoService _produtoService;
    private readonly OllamaService _ollama;
    private readonly List<string> _historico = new();

    public ProdutoIAService(
        IProdutoService produtoService,
        OllamaService ollama)
    {
        _produtoService = produtoService;
        _ollama = ollama;
    }

    public async Task<string> PerguntarAsync(string pergunta)
    {
        var produtos = _produtoService.ListarTodos();

        var sb = new StringBuilder();

        //sb.AppendLine("Você é um assistente que responde sobre produtos.");
        //sb.AppendLine("Responda somente com base nos produtos listados.");
        sb.AppendLine("Você é um analista financeiro especialista em e-commerce.");
        sb.AppendLine("Analise os dados abaixo e responda a pergunta do usuário.");
        sb.AppendLine("Regras:");
        sb.AppendLine("- Responda apenas o que foi perguntado.");
        sb.AppendLine("- Não dê sugestões.");
        sb.AppendLine("- Não faça análises adicionais.");
        sb.AppendLine("- Não Inventa.");
        sb.AppendLine();

        sb.AppendLine("Segue Lista dos Pedidos:");
        foreach (var p in produtos)
        {
            sb.AppendLine($"Código: {p.Codigo} | Descrição: {p.Descricao} | Valor: {p.ValorUnitario}");
        }

        sb.AppendLine();
        sb.AppendLine("Histórico da conversa:");

        foreach (var item in _historico)
        {
            sb.AppendLine(item);
        }

        sb.AppendLine();
        sb.AppendLine($"Usuário: {pergunta}");

        var resposta = await _ollama.GenerateAsync(sb.ToString());

        // Salva no histórico
        _historico.Add($"Usuário: {pergunta}");
        _historico.Add($"Assistente: {resposta}");

        return resposta;
    }

}
