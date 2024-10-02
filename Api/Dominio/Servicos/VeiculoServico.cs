using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Interfaces;
using minimal_api.Infraestrutura.Db;
using System.Linq;

namespace minimal_api.Dominio.Servicos
{
    public class VeiculoServico : IVeiculoServico
    {
        private readonly BdContext _context;
        public VeiculoServico(BdContext context)
        {
            _context = context;
        }
        public void Atualizar(Veiculo veiculo)
        {
            _context.Update(veiculo);
            _context.SaveChanges();
        }

        public Veiculo? BuscaPorId(int id)
        {
            return _context.Veiculos.Find(id);
        }

        public void Excluir(Veiculo veiculo)
        {
            _context.Veiculos.Remove(veiculo);
            _context.SaveChanges();
        }

        public void Incluir(Veiculo veiculo)
        {
            _context.Veiculos.Add(veiculo);
            _context.SaveChanges(); 
        }

        public List<Veiculo> Todos(int? pag = 1, string? nome = null, string? marca = null)
        {
            int itensPorPagina = 10;
            IQueryable<Veiculo> query = _context.Veiculos.AsQueryable();

            if (!string.IsNullOrEmpty(nome)) query = query.Where(v => v.Nome.ToLower() == nome.ToLower());
            if (!string.IsNullOrEmpty(marca)) query = query.Where(v => v.Marca.ToLower() == marca.ToLower());

            if (pag.HasValue) {
                return query.Skip((pag.Value - 1) * itensPorPagina).Take(itensPorPagina).ToList();
            }
            
            return query.ToList();
        }
    }
}
