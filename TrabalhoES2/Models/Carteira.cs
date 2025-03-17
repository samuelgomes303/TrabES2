using System;
using System.Collections.Generic;

namespace TrabalhoES2.Models;

public partial class Carteira
{
    public int CarteiraId { get; set; }

    public string Nome { get; set; } = null!;

    public int UtilizadorId { get; set; }

    public virtual ICollection<Ativofinanceiro> Ativofinanceiros { get; set; } = new List<Ativofinanceiro>();

    public virtual Utilizador Utilizador { get; set; } = null!;
}
