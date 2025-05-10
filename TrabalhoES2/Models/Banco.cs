using System;
using System.Collections.Generic;

namespace TrabalhoES2.Models;

public partial class Banco
{
    public int BancoId { get; set; }

    public string Nome { get; set; } = null!;

    public virtual ICollection<Depositoprazo> Depositoprazos { get; set; } = new List<Depositoprazo>();

    public virtual ICollection<Fundoinvestimento> Fundoinvestimentos { get; set; } = new List<Fundoinvestimento>();

    public virtual ICollection<Imovelarrendado> Imovelarrendados { get; set; } = new List<Imovelarrendado>();

}
