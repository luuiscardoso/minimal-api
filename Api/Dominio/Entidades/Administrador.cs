using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace minimal_api.Dominio.Entidades
{
    public class Administrador
    {
        public int AdministradorId { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string Senha { get; set; }


        [StringLength(10)]
        public string Perfil { get; set; }

    }
}