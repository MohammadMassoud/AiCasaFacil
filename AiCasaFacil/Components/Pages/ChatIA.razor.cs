// AiCasaFacil/Components/Pages/ChatIA.razor.cs
using AiCasaFacil.Application.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace AiCasaFacil.Components.Pages;

public partial class ChatIA : ComponentBase
{
    [Inject] public PedidoIAService _iaService { get; set; } = default!;
    [Inject] public PedidoSessaoService _sessao { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    private string _pergunta = string.Empty;
    private bool _carregando = false;

    private readonly List<string> _sugestoes = new()
    {
        "Qual foi o lucro total?",
        "Quais produtos tiveram melhor margem?",
        "Houve alguma devolução?",
        "Qual produto vendeu mais?",
        "Como está o comparativo ML x Budi?"
    };

    private async Task Enviar()
    {
        if (string.IsNullOrWhiteSpace(_pergunta) || _carregando)
            return;

        var perguntaEnviada = _pergunta;
        _pergunta = string.Empty;
        _carregando = true;
        StateHasChanged();

        await _iaService.PerguntarAsync(perguntaEnviada);

        _carregando = false;
        StateHasChanged();

        await RolarParaBaixo();
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
            await Enviar();
    }

    private void UsarSugestao(string sugestao)
    {
        _pergunta = sugestao;
    }

    private void LimparChat()
    {
        _iaService.LimparHistorico();
    }

    private async Task RolarParaBaixo()
    {
        await JS.InvokeVoidAsync("eval",
            "document.getElementById('chat-messages')?.scrollTo({top: 999999, behavior: 'smooth'})");
    }
}