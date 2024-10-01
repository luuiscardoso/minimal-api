using minimal_api.Dominio.DTO;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.ModelViews;
using minimal_api.Infraestrutura.Db;
using System.Text.RegularExpressions;

namespace minimal_api.Dominio.Servicos
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly BdContext _context;
        public AdministradorServico(BdContext context)
        {
            _context = context;
        }

        public Administrador? BuscaPorId(int id)
        {
            return _context.Administradores.Find(id);
        }

        public Administrador Incluir(Administrador administrador)
        {
            _context.Add(administrador);
            _context.SaveChanges();

            return administrador;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            Administrador? adm = _context.Administradores.FirstOrDefault(a => a.Email.Equals(loginDTO.Email) && a.Senha.Equals(loginDTO.Senha));

            return adm;
        }

        public List<Administrador> Todos(int? pag)
        {
            int itensPorPagina = 10;
            IQueryable<Administrador> query = _context.Administradores.AsQueryable();

            if (pag.HasValue)
            {
                return query.Skip((pag.Value - 1) * itensPorPagina).Take(itensPorPagina).ToList();
            }

            return query.ToList();
        }
    }
}
