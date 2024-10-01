using minimal_api.Dominio.Enums;

namespace minimal_api.Dominio.DTO
{
    public class AdministradorDTO
    {
        public string Email { get; set; }
        public string Senha { get; set; }
        public PerfilEnum Perfil { get; set; }
    }
}
