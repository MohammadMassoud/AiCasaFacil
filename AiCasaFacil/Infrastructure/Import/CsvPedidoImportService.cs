using AiCasaFacil.Application.Interfaces;
using AiCasaFacil.Components.Pages;
using AiCasaFacil.Domain.Entities;
using AiCasaFacil.Domain.Enums;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static AiCasaFacil.Domain.Enums.Enum;

namespace AiCasaFacil.Infrastructure.Import;

public class CsvPedidoImportService
{
    private readonly IProdutoService _produtoService;

    public CsvPedidoImportService(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    public object LerPedidos(Stream stream, PlataformaImportacao plataforma)
    {
        return plataforma switch
        {
            PlataformaImportacao.ML => LerPedidosML(stream),
            PlataformaImportacao.Tiktok => LerPedidosTiktok(stream),
            PlataformaImportacao.Budi => LerPedidosBudi(stream),
            _ => throw new Exception("Plataforma não suportada")
        };
    }
    private DateTime ConverterData(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return DateTime.MinValue;

        texto = texto.Replace(" hs.", "").Trim();

        var cultura = new CultureInfo("pt-BR");

        if (DateTime.TryParse(texto, cultura, DateTimeStyles.None, out var data))
            return data;

        return DateTime.MinValue;
    }

    public List<Pedido> LerPedidosML(Stream stream)
    {
        try
        {
            var produtos = _produtoService.ListarTodos();
            var numeroPedido= "";
            var qtdePacote = 0;
            var pedidos = new List<Pedido>();            
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            var rows = worksheet.RangeUsed().RowsUsed();

            foreach (var row in rows)
            {
                if (row.Cell(1).IsEmpty())
                    continue;

                if (!row.Cell(1).GetString().StartsWith("20000"))
                    continue;

                // Verifica se o pedido já existe
                var pedido = pedidos.FirstOrDefault(p => p.NumeroPedido == numeroPedido);
                if (qtdePacote <=0)
                {
                    numeroPedido = row.Cell(1).GetString();
                    pedido = new Pedido
                    {                        
                        NumeroPedido = row.Cell(1).GetString(),
                        DataPedido = ConverterData(row.Cell(2).GetString()),
                        ValorBruto = row.Cell(27).TryGetValue<decimal>(out var vb) ? vb : 0m,
                        ValorTotal = row.Cell(9).TryGetValue<decimal>(out var vt) ? vt : 0m,
                        ValorLiquido = row.Cell(19).TryGetValue<decimal>(out var vl) ? vl : 0m,
                        FormaEntrega = row.Cell(43).GetString()
                    };

                    if (pedido.FormaEntrega.Contains("Flex") && pedido.ValorBruto > 79)
                        pedido.ValorLiquido += 1.1m; 
                    else if (pedido.FormaEntrega.Contains("Flex") && pedido.ValorBruto < 79)
                        pedido.ValorLiquido += 11m;
                    
                    pedidos.Add(pedido);
                }

                if(row.Cell(4).GetString().StartsWith("Pacote"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(row.Cell(3).GetString(), @"\d+");

                    if (match.Success)
                        qtdePacote = int.Parse(match.Value);
                    continue;
                }
                
                var item = new PedidoItem
                {                    
                    PacoteId = numeroPedido,
                    NumeroPedido = row.Cell(1).GetString(),
                    Produto = row.Cell(25).GetString(),
                    Codigo = row.Cell(23).GetString().Replace("-",""),
                    Quantidade = row.Cell(8).TryGetValue<int>(out var qtd) ? qtd : 0,
                    Custo = 0m 
                };

                var produto = produtos.FirstOrDefault(p => p.Codigo.Trim() == item.Codigo.Trim());

                if (produto != null)
                {
                    item.NomeProduto = produto.Descricao;
                    if (produto.Codigo == "BS341" && item.Produto.Contains("5 Uni"))
                    {
                        item.Quantidade = item.Quantidade * 5;
                        pedido.ValorBruto = pedido.ValorBruto / 5;
                    }
                    else if(produto.Codigo == "BS321")
                    {
                        item.Quantidade = item.Quantidade * 2;
                        pedido.ValorBruto = pedido.ValorBruto / 2;
                    }                    

                    item.Custo = produto.ValorUnitario * item.Quantidade;
                }
                qtdePacote = qtdePacote>0?qtdePacote-1:qtdePacote;
                pedido.Itens.Add(item);
            }
            
            return pedidos;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new List<Pedido>();
        }
    }

    public List<PedidosBudi> LerPedidosBudi(Stream stream)
    {
        try
        {
            var pedido = new PedidosBudi();
            var item = new PedidoBudiItem();

            var pedidoExiste = new PedidosBudi(); ;
            bool PegouPedido = false;
            bool PegouNomeCliente = false;
            bool PegouProduto = false;
            bool ExisteMaisProdutos = false;
            var produtos = _produtoService.ListarTodos();
            var numeroPedido = "";
            var pedidos = new List<PedidosBudi>();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            var rows = worksheet.RangeUsed().RowsUsed();

            foreach (var row in rows)
            {
                if (row.Cell(9).IsEmpty() || row.Cell(9).GetString().StartsWith("Receiver Name"))
                    continue;
                
                if (row.Cell(15).GetString().StartsWith("20000"))
                {
                    pedidoExiste = pedidos.FirstOrDefault(p => p.NumeroPedido == row.Cell(15).GetString());
                    if (pedidoExiste != null)
                    {
                        continue;
                    }
                }else if (pedidoExiste != null && !row.Cell(15).GetString().StartsWith("20000") && !row.Cell(9).GetString().StartsWith("Pedido"))
                {
                    continue;
                }

                pedidoExiste = null;
                if (!PegouPedido && !PegouNomeCliente && !PegouProduto && !row.Cell(9).GetString().StartsWith("BS"))
                {
                    pedido = new PedidosBudi();                    
                }
                
                if (!PegouPedido)
                {
                    if (row.Cell(9).GetString().StartsWith("BS"))
                    {
                        PegouPedido = true;
                        ExisteMaisProdutos = true;
                    }
                    else
                    {
                        if (row.Cell(15).GetString() == "") 
                            numeroPedido = row.Cell(9).GetString().Replace("Pedido: ","");
                        else
                            numeroPedido = row.Cell(15).GetString();

                        pedido.NumeroPedido = numeroPedido;
                        pedido.FormaEntrega = row.Cell(24).GetString();
                        PegouPedido = true;
                        continue;
                    }
                                            
                }

                if (!PegouNomeCliente)
                {
                    if (row.Cell(9).GetString().StartsWith("BS"))
                    {
                        PegouNomeCliente = true;
                    }
                    else
                    {
                        pedido.NomeCliente = row.Cell(9).GetString();
                        pedido.DataPedido = ConverterData(row.Cell(19).GetString());
                        pedido.DataEntregaBudi = ConverterData(row.Cell(24).GetString());
                        PegouNomeCliente = true;
                        continue;
                    }                    
                }

                if (!PegouProduto)
                {                    
                    item = new PedidoBudiItem
                    {                        
                        NumeroPedido = numeroPedido,
                        Codigo = row.Cell(9).GetString(),
                        Quantidade = row.Cell(13).TryGetValue<int>(out var qtd) ? qtd : 0
                    };                    
                    PegouProduto = true;

                    var produto = produtos.FirstOrDefault(p => p.Codigo.Trim() == item.Codigo.Trim());
                    if (produto != null)
                    {
                        item.NomeProduto = produto.Descricao;                                                
                        item.Custo = produto.ValorUnitario * item.Quantidade;
                    }
                }

                if (PegouPedido && PegouNomeCliente && PegouProduto)
                {
                    if(!ExisteMaisProdutos) pedidos.Add(pedido);
                    pedido.Itens.Add(item);
                    PegouPedido = false; PegouNomeCliente = false; PegouProduto = false; ExisteMaisProdutos = false;
                }
                //qtdePacote = qtdePacote > 0 ? qtdePacote - 1 : qtdePacote;
                //pedido.Itens.Add(item);
            }

            return pedidos;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new List<PedidosBudi>();
        }
    }

    public List<Pedido> LerPedidosTiktok(Stream stream)
    {
        try
        {
            var produtos = _produtoService.ListarTodos();
            var numeroPedido = "";
            var qtdePacote = 0;
            var pedidos = new List<Pedido>();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            var rows = worksheet.RangeUsed().RowsUsed();

            foreach (var row in rows)
            {
                if (row.Cell(1).IsEmpty())
                    continue;

                if (!row.Cell(1).GetString().StartsWith("5"))
                    continue;

                // Verifica se o pedido já existe
                var pedido = pedidos.FirstOrDefault(p => p.NumeroPedido == numeroPedido);
                if (qtdePacote <= 0)
                {
                    numeroPedido = row.Cell(1).GetString();
                    pedido = new Pedido
                    {
                        NumeroPedido = row.Cell(1).GetString(),
                        DataPedido = ConverterData(row.Cell(29).GetString()),
                        ValorBruto = (row.Cell(14).TryGetValue<decimal>(out var vb) ? vb : 0m) - ((row.Cell(17).TryGetValue<decimal>(out var vd) ? vd : 0m) / (row.Cell(12).TryGetValue<decimal>(out var qtde) ? qtde : 0m)),
                        ValorTotal = (row.Cell(15).TryGetValue<decimal>(out var vt) ? vt : 0m) - (row.Cell(17).TryGetValue<decimal>(out var vtd) ? vtd : 0m),
                        ValorLiquido = row.Cell(18).TryGetValue<decimal>(out var vl) ? vl : 0m,
                        FormaEntrega = row.Cell(41).GetString()
                    };

                    pedidos.Add(pedido);
                }

                if (row.Cell(3).GetString().StartsWith("Pacote"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(row.Cell(3).GetString(), @"\d+");

                    if (match.Success)
                        qtdePacote = int.Parse(match.Value);
                    continue;
                }

                var item = new PedidoItem
                {
                    PacoteId = numeroPedido,
                    NumeroPedido = row.Cell(1).GetString(),
                    Produto = row.Cell(9).GetString(),
                    Codigo = row.Cell(8).GetString().Replace("-",""),
                    Quantidade = row.Cell(12).TryGetValue<int>(out var qtd) ? qtd : 0,
                    Custo = 0m
                };

                var produto = produtos.FirstOrDefault(p => p.Codigo.Trim() == item.Codigo.Trim());

                if (produto != null)
                {
                    item.NomeProduto = produto.Descricao;
                    if (produto.Codigo == "BS341" && item.Produto.Contains("5 Uni"))
                    {
                        item.Quantidade = item.Quantidade * 5;
                    }
                    else if (produto.Codigo == "BS321")
                    {
                        item.Quantidade = item.Quantidade * 2;
                    }

                    item.Custo = produto.ValorUnitario * item.Quantidade;
                }
                qtdePacote = qtdePacote > 0 ? qtdePacote - 1 : qtdePacote;
                pedido.Itens.Add(item);
            }

            return pedidos;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new List<Pedido>();
        }
    }
}
