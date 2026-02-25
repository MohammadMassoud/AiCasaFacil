
//using AiCasaFacil.Application.Interfaces;
//using AiCasaFacil.Application.Services;
//using AiCasaFacil.Domain.Entities;
//using AiCasaFacil.Infrastructure.AI;
//using AiCasaFacil.Infrastructure.Import;
//using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Components.Forms;

//namespace AiCasaFacil.Components.Pages;

//public partial class IATeste : ComponentBase

//{
//    [Inject] public OllamaService IAService { get; set; } = default!;
//    [Inject] public ProdutoIAService ProdutoIAService { get; set; } = default!;
//    [Inject] public PedidoIAService PedidoIA { get; set; } = default!;

//    [Inject] public IProdutoService ProdutoService { get; set; } = default!;
//    protected List<Produto> produtos = new();
//    private List<Pedido>? pedidos;

//    private string pergunta = "";
//    private string resposta = "";

//    private async Task PerguntaGeral()
//    {
//        resposta = "Pensando...";
//        resposta = await IAService.GenerateAsync(pergunta);
//    }

//    protected async Task PerguntaProduto()
//    {
//        if (string.IsNullOrWhiteSpace(pergunta))
//            return;

//        resposta = "Analisando...";

//        resposta = await ProdutoIAService.PerguntarAsync(pergunta);
//    }
//    protected async Task PerguntaPedido()
//    {
//        if (string.IsNullOrWhiteSpace(pergunta))
//            return;

//        resposta = "Analisando...";

//        resposta = await PedidoIA.PerguntarAsync(pedidos,pergunta);
//    }
    
//    //private async Task HandleFile(InputFileChangeEventArgs e)
//    //{
//    //    var file = e.File;

//    //    using var memoryStream = new MemoryStream();
//    //    await file.OpenReadStream().CopyToAsync(memoryStream);

//    //    memoryStream.Position = 0;

//    //    var service = new CsvPedidoImportService();
//    //    pedidos = service.ImportarCsvML(memoryStream, "Mercado Livre");

//    //    var produtos = ProdutoService.ListarTodos();

//    //    foreach (var pedido in pedidos)
//    //    {
//    //        var produto = produtos
//    //            .FirstOrDefault(p => p.Codigo == pedido.Codigo);

//    //        if (produto != null)
//    //        {
//    //            if(produto.Codigo == "BS341" && pedido.Produto.Contains("5 Uni"))
//    //            {
//    //                pedido.Quantidade = pedido.Quantidade * 5;
//    //                pedido.Custo = produto.ValorUnitario * pedido.Quantidade;
//    //            }
//    //            else
//    //            {
//    //                pedido.Custo = produto.ValorUnitario * pedido.Quantidade;
//    //            }

//    //        }
                
//    //    }

//    //}

//}

