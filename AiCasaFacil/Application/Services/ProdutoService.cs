using AiCasaFacil.Application.Interfaces;
using AiCasaFacil.Domain.Entities;

namespace AiCasaFacil.Application.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly IProdutoRepository _repository;

        public ProdutoService(IProdutoRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<Produto> ListarTodos()
        {
            return _repository.ObterTodos();
        }

        public Produto? BuscarPorCodigo(string codigo)
        {
            return _repository.ObterPorCodigo(codigo);
        }

        public void Criar(Produto produto)
        {
            var existente = _repository.ObterPorCodigo(produto.Codigo);
            if (existente != null)
                throw new Exception("Produto já existe.");

            // Aqui você pode chamar método do repo depois
            // se ainda não tiver, vamos criar
        }

        public void Atualizar(Produto produto)
        {
            // mesma ideia
        }

        public void Remover(string codigo)
        {
            // mesma ideia
        }
    }


}
