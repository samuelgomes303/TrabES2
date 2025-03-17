using System;
using System.Collections.Generic;

namespace TrabalhoES2.Models;

public partial class Ativofinanceiro
{
    public int AtivofinanceiroId { get; set; }

    public decimal? Percimposto { get; set; }

    public int? Duracaomeses { get; set; }

    public DateOnly? Datainicio { get; set; }

    public int CarteiraId { get; set; }

    public virtual Carteira Carteira { get; set; } = null!;

    public virtual Depositoprazo? Depositoprazo { get; set; }

    public virtual Fundoinvestimento? Fundoinvestimento { get; set; }

    public virtual Imovelarrendado? Imovelarrendado { get; set; }
}
