using System.ComponentModel.DataAnnotations;

namespace minimal_api.Dominio.Entidades
{
    public class Veiculo
    {
        public int VeiculoId { get; set; }

        [Required]
        [StringLength(150)]
        public string Nome { get; set; }

        [Required]
        [StringLength(50)]
        public string Marca { get; set; }


        [Required]
        public int Ano { get; set; }
    }
}
