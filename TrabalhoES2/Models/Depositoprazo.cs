using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrabalhoES2.Models;

public partial class Depositoprazo
{
    public int DepositoprazoId { get; set; }

    public decimal Valorinicial { get; set; }

    public int BancoId { get; set; }

    public string Nrconta { get; set; } = null!;

    public string Titular { get; set; } = null!;

    [Range(0.01, 50.00, ErrorMessage = "A taxa de juro anual deve estar entre 0.01% e 50%.")]
    public decimal Taxajuroanual { get; set; }

    public decimal Valoratual { get; set; }

    public int AtivofinanceiroId { get; set; }

    public virtual Ativofinanceiro Ativofinanceiro { get; set; } = null!;

    public virtual Banco Banco { get; set; } = null!;
}
