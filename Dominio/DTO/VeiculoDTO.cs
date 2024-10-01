using System.ComponentModel.DataAnnotations;

namespace minimal_api.Dominio.DTO
{
    public class VeiculoDTO
    {
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
