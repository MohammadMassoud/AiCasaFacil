using AiCasaFacil.Application.Interfaces;
using AiCasaFacil.Domain.Entities;

namespace AiCasaFacil.Infrastructure.Data;

public class ProdutoRepositoryEmMemoria : IProdutoRepository
{
    private readonly List<Produto> _produtos = new()
    {
        new Produto { Codigo = "BS081", Descricao = "ARVORE DE NATAL 180CM 6", ValorUnitario = 112.90m },
        new Produto { Codigo = "BS105", Descricao = "GUARDA ROUPA DOBRAVEL", ValorUnitario = 65.90m },
        new Produto { Codigo = "BS115", Descricao = "COPO DIAMOND TRANSPARENTE", ValorUnitario = 4.65m },
        new Produto { Codigo = "BS321", Descricao = "PORTA SHAMPOO DE METAL RETO", ValorUnitario = 8.01m },
        new Produto { Codigo = "BS320", Descricao = "PORTA SHAMPOO DE METAL CANTO", ValorUnitario = 6.20m },
        new Produto { Codigo = "BS322", Descricao = "PORTA SHAMPOO PRATELEIRA", ValorUnitario = 14.89m },
        new Produto { Codigo = "BS323", Descricao = "PORTA SHAMPOO PRATELEIRA", ValorUnitario = 15.25m },
        new Produto { Codigo = "BS324", Descricao = "CABIDEIRO E SAPATEIRA", ValorUnitario = 23.90m },
        new Produto { Codigo = "BS325", Descricao = "SAPATEIRA 10 ANDARES", ValorUnitario = 18.63m },
        new Produto { Codigo = "BS326", Descricao = "FATIADOR DE ALIMENTOS", ValorUnitario = 24.43m },
        new Produto { Codigo = "BS327", Descricao = "MOP SPRAY CX 12", ValorUnitario = 19.00m },
        new Produto { Codigo = "BS328", Descricao = "REFIL PARA MOP SPRAY CX 60", ValorUnitario = 7.49m },
        new Produto { Codigo = "BS331", Descricao = "GUARDA SOL 2.4M", ValorUnitario = 49.67m },
        new Produto { Codigo = "BS332", Descricao = "GUARDA-SOL 2.2M X 2.48M", ValorUnitario = 53.08m },
        new Produto { Codigo = "BS336", Descricao = "ORGANIZADOR DE ROUPA", ValorUnitario = 88.50m },
        new Produto { Codigo = "BS337", Descricao = "SAPATEIRA 50 A 55 PARES", ValorUnitario = 99.90m },
        new Produto { Codigo = "BS338", Descricao = "KIT 4 CESTOS ORGANIZADORES", ValorUnitario = 11.42m },
        new Produto { Codigo = "BS339", Descricao = "PARAFUSADEIRA ELETRICA", ValorUnitario = 94.90m },
        new Produto { Codigo = "BS340", Descricao = "CHAVE DE IMPACTO", ValorUnitario = 176.9m },
        new Produto { Codigo = "BS341", Descricao = "BARRAS ALÇAS APOIO 80CM", ValorUnitario = 18.86m },
        new Produto { Codigo = "BS343", Descricao = "PISCINA INFLAVEL 100L", ValorUnitario = 26.00m },
        new Produto { Codigo = "BS344", Descricao = "PISCINA INFLAVEL 180L", ValorUnitario = 29.00m },
        new Produto { Codigo = "BS600", Descricao = "VARAL DE ROUPAS CABIDEIRO", ValorUnitario = 45.50m }
    };

    public IReadOnlyList<Produto> ObterTodos()
        => _produtos;

    public Produto? ObterPorCodigo(string codigo)
        => _produtos.FirstOrDefault(p => p.Codigo == codigo);

    public void Adicionar(Produto produto)
        => _produtos.Add(produto);

    public void Atualizar(Produto produto)
    {
        var existente = ObterPorCodigo(produto.Codigo);
        if (existente != null)
        {
            existente.Descricao = produto.Descricao;
            existente.ValorUnitario = produto.ValorUnitario;
        }
    }

    public void Remover(string codigo)
    {
        var produto = ObterPorCodigo(codigo);
        if (produto != null)
            _produtos.Remove(produto);
    }
}
