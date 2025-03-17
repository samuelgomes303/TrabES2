using System;
using System.Collections.Generic;

namespace TrabalhoES2.Models;

public partial class Fundoinvestimento
{
    public int FundoinvestimentoId { get; set; }

    public int BancoId { get; set; }

    public string Nome { get; set; } = null!;

    public decimal Montanteinvestido { get; set; }

    public decimal Taxajuropdefeito { get; set; }

    public int AtivofinanceiroId { get; set; }

    public virtual Ativofinanceiro Ativofinanceiro { get; set; } = null!;

    public virtual Banco Banco { get; set; } = null!;
}
