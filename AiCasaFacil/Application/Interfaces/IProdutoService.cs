using AiCasaFacil.Domain.Entities;

namespace AiCasaFacil.Application.Interfaces
{
    public interface IProdutoService
    {
        IEnumerable<Produto> ListarTodos();
        Produto? BuscarPorCodigo(string codigo);
        void Criar(Produto produto);
        void Atualizar(Produto produto);
        void Remover(string codigo);
    }
}
