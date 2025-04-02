using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TrabalhoES2.Models
{
    public partial class Utilizador : IdentityUser<int>
    {
        public enum TipoUtilizador
        {
            Cliente,
            Admin,
            UserManager
        }
        
        public string Nome { get; set; } = null!;

        public TipoUtilizador TpUtilizador { get; set; }

        public virtual ICollection<Carteira> Carteiras { get; set; } = new List<Carteira>();
    }
}