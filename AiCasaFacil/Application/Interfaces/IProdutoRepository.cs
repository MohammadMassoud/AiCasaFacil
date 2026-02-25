using AiCasaFacil.Domain.Entities;
namespace AiCasaFacil.Application.Interfaces
{
    public interface IProdutoRepository
    {
        IReadOnlyList<Produto> ObterTodos();
        Produto? ObterPorCodigo(string codigo);

        void Adicionar(Produto produto);
        void Atualizar(Produto produto);
        void Remover(string codigo);
    }
}
