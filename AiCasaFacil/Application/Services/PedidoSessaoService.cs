// AiCasaFacil/Application/Services/PedidoSessaoService.cs
using AiCasaFacil.Domain.Entities;

namespace AiCasaFacil.Application.Services;

public class PedidoSessaoService
{
    private List<Pedido> _pedidos = new();
    private List<PedidosBudi> _pedidosBudi = new();

    public IReadOnlyList<Pedido> Pedidos => _pedidos;
    public IReadOnlyList<PedidosBudi> PedidosBudi => _pedidosBudi;

    public bool TemPedidos => _pedidos.Any();
    public bool TemPedidosBudi => _pedidosBudi.Any();

    public void SalvarPedidos(List<Pedido> pedidos) => _pedidos = pedidos;
    public void SalvarPedidosBudi(List<PedidosBudi> pedidos) => _pedidosBudi = pedidos;

    public void Limpar()
    {
        _pedidos.Clear();
        _pedidosBudi.Clear();
    }
}