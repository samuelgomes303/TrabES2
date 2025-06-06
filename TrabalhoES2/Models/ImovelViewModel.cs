using TrabalhoES2.Models;

namespace TrabalhoES2.ViewModels
{
    public class ImovelViewModel
    {
        public Imovelarrendado Imovel { get; set; } = new();
        public Ativofinanceiro Ativo { get; set; } = new();
    }
}