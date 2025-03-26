using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace TrabalhoES2.Models;

public partial class Utilizador
{
    public enum TipoUtilizador
    {
        Cliente,
        Admin,
        UserManager
    }   
    public int UtilizadorId { get; set; }

    public string Nome { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } 
    
    public TipoUtilizador TpUtilizador { get; set; }
    
    public string IdentityUserId { get; set; }
    
    public IdentityUser IdentityUser { get; set; } 
    public virtual ICollection<Carteira> Carteiras { get; set; } = new List<Carteira>();
}
