using System.Collections.Generic;
using System.Linq;

namespace AgregacaoModel
{
    public class Secao
    {
        public int Numero { get; set; }
        public int QuantidadeAptos {  get;  set; }

        public string Municipio { get; set; }

        public LocalVotacao LocalVotacao{get;set;}

        public Zona Zona { get; set; }

        public List<Secao> Agregadas { get; set; } = new List<Secao>();

        public Secao SecaoAgregadora { get; set; }

        public int QuantidadeAptosTotal { get => TTE?QuantidadeAptos:this.QuantidadeAptos + Agregadas.Sum(s => s.QuantidadeAptosTotal); }

        public bool TTE { get; set; } = false;

        public int TipoAgregacao { get; set; }
    }
}