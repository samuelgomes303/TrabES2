using System;
using System.Collections.Generic;

namespace TrabalhoES2.Models;

public partial class Imovelarrendado
{
    public int ImovelarrendadoId { get; set; }

    public string Designacao { get; set; } = null!;

    public string Localizacao { get; set; } = null!;

    public decimal Valorimovel { get; set; }

    public decimal Valorrenda { get; set; }

    public decimal Valormensalcondo { get; set; }

    public decimal Valoranualdespesas { get; set; }

    public int BancoId { get; set; }

    public int AtivofinanceiroId { get; set; }

    public virtual Ativofinanceiro Ativofinanceiro { get; set; } = null!;

    public virtual Banco Banco { get; set; } = null!;
}
