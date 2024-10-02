using minimal_api.Dominio.Enums;

namespace minimal_api.Dominio.ModelViews
{
    public class AdministradorModelView
    {
        public int Id { get; set; }
        public string Email { get; set; }

        public string Perfil { get; set; }
    }
}
