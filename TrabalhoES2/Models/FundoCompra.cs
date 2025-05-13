using System.ComponentModel.DataAnnotations;

namespace TrabalhoES2.Models;

public class FundoCompra
{
    [Key]
    public int FundoCompraId { get; set; }

    public int FundoinvestimentoId { get; set; }

    public decimal ValorPorUnidade { get; set; }

    public DateOnly DataCompra { get; set; }

    public Fundoinvestimento Fundoinvestimento { get; set; }
}