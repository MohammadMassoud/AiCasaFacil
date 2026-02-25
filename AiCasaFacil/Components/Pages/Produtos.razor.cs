using AiCasaFacil.Application.Interfaces;
using AiCasaFacil.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace AiCasaFacil.Components.Pages
{
    public partial class Produtos
    {
        [Inject] public IProdutoService produtoService { get; set; } = default!;

        protected string filtro = string.Empty;
        protected List<Produto> produtos = new();

        protected override void OnInitialized()
        {
            produtos = produtoService.ListarTodos().ToList();
        }

        protected void OnFiltroChanged(string value)
        {
            filtro = value;

            if (string.IsNullOrWhiteSpace(filtro))
            {
                produtos = produtoService.ListarTodos().ToList();
            }
            else
            {
                produtos = produtoService
                    .ListarTodos()
                    .Where(p => p.Codigo.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

    }
}
