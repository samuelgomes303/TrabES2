using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace TrabalhoES2.Models;

public partial class Imovelarrendado
{
    public int ImovelarrendadoId { get; set; }

    public string Designacao { get; set; } = null!;

    public string Localizacao { get; set; } = null!;

    [Display(Name = "Valor do Imóvel")] 
    public decimal Valorimovel { get; set; }

    [Display(Name = "Valor da Renda")]
    public decimal Valorrenda { get; set; }

    [Display(Name = "Valor Mensal de Condomínio")]
    public decimal Valormensalcondo { get; set; }

    [Display(Name = "Valor Anual de Despesas")]
    public decimal Valoranualdespesas { get; set; }

    [Required]
    public int BancoId { get; set; }

    public int AtivofinanceiroId { get; set; }

    public virtual Ativofinanceiro Ativofinanceiro { get; set; } = null!;

    public virtual Banco Banco { get; set; } = null!;
}
