using AiCasaFacil.Application.Interfaces;
using AiCasaFacil.Application.Services;
using AiCasaFacil.Domain.Entities;
using AiCasaFacil.Infrastructure.Import;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using static AiCasaFacil.Domain.Enums.Enum;

namespace AiCasaFacil.Components.Pages;

public partial class ImportaPedidosMarketplace
{
    [Inject] public IProdutoService ProdutoService { get; set; } = default!;
    [Inject] public IAnalisePedidosService AnaliseService { get; set; } = default!;
    [Inject] public PedidoSessaoService SessaoService { get; set; } = default!;

    private List<Pedido>? pedidos;
    private List<PedidosBudi>? pedidosBudi;
    private List<PedidoComparativoViewModel> comparativoPedidos = new();

    private PlataformaImportacao plataformaSelecionada = PlataformaImportacao.ML;
    // Métricas
    private decimal lucroTotal;
    private decimal valorTotalVendas;
    private decimal valorTotalDevido;
    private decimal mediaMargem;
    private decimal totalFlex;
    private int quantidadePedidosFlex;
    private decimal totalTaxa;
    private bool houveDevolucao;
    private IEnumerable<object> resumoPorProduto = Enumerable.Empty<object>();
    private List<PedidoGridViewModel> detalhePedidos;
    private List<(string Codigo, string Produto, int Quantidade)> resumoML = new();
    private List<(string Codigo, string Produto, int Quantidade)> resumoBudi = new();
    private List<(string Codigo, string Produto, int QtdML, int QtdBudi)> diferencas = new();
    public class PedidoGridViewModel
    {
        public string NumeroPedido { get; set; }
        public string Codigo { get; set; }
        public string Produto { get; set; }
        public DateTime DataPedido { get; set; }

        public int Quantidade { get; set; }
        public decimal Custo { get; set; }

        public decimal ValorTotal { get; set; }
        public decimal ValorLiquido { get; set; }
        public decimal Taxa { get; set; }
        public decimal Flex { get; set; }

        public decimal Lucro { get; set; }
        public decimal Margem { get; set; }
    }

    protected override void OnInitialized()
    {
        // Se já tem pedidos na sessão, restaura o estado da página
        if (SessaoService.TemPedidos)
        {
            ProcessarPedidos(SessaoService.Pedidos.ToList());
        }

        if (SessaoService.TemPedidosBudi)
        {
            ProcessarPedidosBudi(SessaoService.PedidosBudi.ToList());
        }
    }
    public List<PedidoGridViewModel> GerarGrid(List<Pedido> pedidos)
    {
        var lista = new List<PedidoGridViewModel>();

        foreach (var pedido in pedidos)
        {
            var totalQuantidadePedido = pedido.Itens.Sum(i => i.Quantidade);

            foreach (var item in pedido.Itens)
            {
                var proporcao = totalQuantidadePedido == 0
                    ? 0
                    : item.Quantidade / (decimal)totalQuantidadePedido;

                var lucroItem = pedido.Lucro * proporcao;

                
                lista.Add(new PedidoGridViewModel
                {
                    NumeroPedido = pedido.NumeroPedido,
                    Codigo = item.Codigo,
                    Produto = item.NomeProduto,
                    DataPedido = pedido.DataPedido,

                    Quantidade = item.Quantidade,
                    Custo = item.Custo,

                    ValorTotal = pedido.ValorTotal,
                    ValorLiquido = pedido.ValorLiquido,
                    Taxa = pedido.Taxa,
                    Flex = pedido.Flex,

                    Lucro = lucroItem,
                    Margem = pedido.Margem
                });
            }
        }

        return lista.OrderByDescending(x => x.DataPedido).ToList();
    }

    private async Task HandleFileML(InputFileChangeEventArgs e)
    {
        var file = e.File;

        using var memoryStream = new MemoryStream();
        await file.OpenReadStream().CopyToAsync(memoryStream);

        memoryStream.Position = 0;

        var service = new CsvPedidoImportService(ProdutoService);
        pedidos = service.LerPedidosML(memoryStream);

        lucroTotal = AnaliseService.CalcularLucroTotal(pedidos);
        valorTotalVendas = AnaliseService.ValorTotalVendas(pedidos);
        valorTotalDevido = AnaliseService.ValorTotalDevido(pedidos);
        mediaMargem = AnaliseService.MediaMargem(pedidos);
        totalFlex = AnaliseService.TotalFlex(pedidos);
        quantidadePedidosFlex = AnaliseService.QuantidadePedidosFlex(pedidos);
        totalTaxa = AnaliseService.TotalTaxa(pedidos);
        houveDevolucao = AnaliseService.HouveDevolucao(pedidos);
        resumoPorProduto = AnaliseService.LucrosPorProdutos(pedidos);
        detalhePedidos = GerarGrid(pedidos);
    }

    private async Task HandleFileBudi(InputFileChangeEventArgs e)
    {
        var file = e.File;

        using var memoryStream = new MemoryStream();
        await file.OpenReadStream().CopyToAsync(memoryStream);

        memoryStream.Position = 0;

        var service = new CsvPedidoImportService(ProdutoService);
        pedidosBudi = service.LerPedidosBudi(memoryStream);

        var produtos = ProdutoService.ListarTodos();
        comparativoPedidos = GerarComparativo();
        GerarResumoProdutos();
    }

    public class PedidoComparativoViewModel
    {
        public string NumeroPedido { get; set; }

        public bool ExisteML { get; set; }
        public bool ExisteBudi { get; set; }

        public DateTime? DataML { get; set; }
        public DateTime? DataBudi { get; set; }
        public string Status =>
            ExisteML && ExisteBudi ? "OK" :
            ExisteML && !ExisteBudi ? "Somente ML" :
            !ExisteML && ExisteBudi ? "Somente Budi" :
            "Não existe";
    }
    private List<PedidoComparativoViewModel> GerarComparativo()
    {
        var lista = new List<PedidoComparativoViewModel>();

        var numerosML = pedidos.Select(p => p.NumeroPedido).Distinct().ToList();
        var numerosBudi = pedidosBudi.Select(p => p.NumeroPedido).Distinct().ToList();

        var todosNumeros = numerosML
            .Union(numerosBudi)
            .Distinct();

        foreach (var numero in todosNumeros)
        {
            var pedidoML = pedidos.FirstOrDefault(p => p.NumeroPedido == numero);
            var pedidoBudi = pedidosBudi.FirstOrDefault(p => p.NumeroPedido == numero);

            lista.Add(new PedidoComparativoViewModel
            {
                NumeroPedido = numero,
                ExisteML = pedidoML != null,
                ExisteBudi = pedidoBudi != null,
                DataML = pedidoML?.DataPedido,
                DataBudi = pedidoBudi?.DataPedido
            });
        }

        return lista.OrderBy(x => x.DataML).ToList();
    }

    private void GerarResumoProdutos()
    {
        // 🔹 RESUMO ML (baseado nos ITENS)
        resumoML = pedidos
            .SelectMany(p => p.Itens)
            .GroupBy(i => new { i.Codigo, i.NomeProduto })
            .Select(g => (
                g.Key.Codigo,
                g.Key.NomeProduto,
                g.Sum(x => x.Quantidade)
            ))
            .OrderBy(x => x.Codigo)
            .ToList();

        // 🔹 RESUMO BUDI (baseado nos ITENS)
        resumoBudi = pedidosBudi
            .SelectMany(p => p.Itens)
            .GroupBy(i => new { i.Codigo, i.NomeProduto })
            .Select(g => (
                g.Key.Codigo,
                g.Key.NomeProduto,
                g.Sum(x => x.Quantidade)
            ))
            .OrderBy(x => x.Codigo)
            .ToList();


        // 🔹 DIFERENÇAS
        var todosCodigos = resumoML
            .Select(x => x.Codigo)
            .Union(resumoBudi.Select(x => x.Codigo))
            .Distinct();

        diferencas.Clear();

        foreach (var codigo in todosCodigos)
        {
            var ml = resumoML.FirstOrDefault(x => x.Codigo == codigo);
            var budi = resumoBudi.FirstOrDefault(x => x.Codigo == codigo);

            int qtdML = ml.Quantidade;
            int qtdBudi = budi.Quantidade;

            if (qtdML != qtdBudi)
            {
                diferencas.Add((
                    codigo,
                    ml.Produto ?? budi.Produto?? "Sem nome",
                    qtdML,
                    qtdBudi
                ));
            }
        }
    }

    private async Task HandleFile(InputFileChangeEventArgs e)
    {
        var file = e.File;

        using var memoryStream = new MemoryStream();
        // Exemplo: permitir até 10MB
        long maxSize = 10 * 1024 * 1024;

        await file.OpenReadStream(maxSize).CopyToAsync(memoryStream);

        memoryStream.Position = 0;

        var service = new CsvPedidoImportService(ProdutoService);

        var resultado = service.LerPedidos(memoryStream, plataformaSelecionada);

        switch (resultado)
        {
            case List<Pedido> pedidosConvertidos:
                ProcessarPedidos(pedidosConvertidos);
                break;

            case List<PedidosBudi> pedidosBudiConvertidos:
                ProcessarPedidosBudi(pedidosBudiConvertidos);
                break;

            default:
                throw new Exception("Tipo de retorno não suportado");
        }
    }

    private void ProcessarPedidos(List<Pedido> pedidos)
    {
        this.pedidos = pedidos;

        SessaoService.SalvarPedidos(pedidos);

        lucroTotal = AnaliseService.CalcularLucroTotal(pedidos);
        valorTotalVendas = AnaliseService.ValorTotalVendas(pedidos);
        valorTotalDevido = AnaliseService.ValorTotalDevido(pedidos);
        mediaMargem = AnaliseService.MediaMargem(pedidos);
        totalFlex = AnaliseService.TotalFlex(pedidos);
        quantidadePedidosFlex = AnaliseService.QuantidadePedidosFlex(pedidos);
        totalTaxa = AnaliseService.TotalTaxa(pedidos);
        houveDevolucao = AnaliseService.HouveDevolucao(pedidos);
        resumoPorProduto = AnaliseService.LucrosPorProdutos(pedidos);
        detalhePedidos = GerarGrid(pedidos);
    }

    private void ProcessarPedidosBudi(List<PedidosBudi> pedidosBudi)
    {
        this.pedidosBudi = pedidosBudi;

        SessaoService.SalvarPedidosBudi(pedidosBudi);

        comparativoPedidos = GerarComparativo();
        GerarResumoProdutos();
    }
}


