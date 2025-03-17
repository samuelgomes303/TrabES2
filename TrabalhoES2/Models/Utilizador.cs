using System;
using System.Collections.Generic;

namespace TrabalhoES2.Models;

public partial class Utilizador
{
    public int UtilizadorId { get; set; }

    public string Nome { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Carteira> Carteiras { get; set; } = new List<Carteira>();
}
