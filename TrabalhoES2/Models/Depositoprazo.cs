using System;
using System.Collections.Generic;

namespace TrabalhoES2.Models;

public partial class Depositoprazo
{
    public int DepositoprazoId { get; set; }

    public decimal Valorinicial { get; set; }

    public int BancoId { get; set; }

    public string Nrconta { get; set; } = null!;

    public string Titular { get; set; } = null!;

    public decimal Taxajuroanual { get; set; }

    public decimal Valoratual { get; set; }

    public int AtivofinanceiroId { get; set; }

    public virtual Ativofinanceiro Ativofinanceiro { get; set; } = null!;

    public virtual Banco Banco { get; set; } = null!;
}
