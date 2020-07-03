using System;
using System.Collections.Generic;

namespace AgregacaoModel
{
    public class LocalVotacao
    {
        public int Numero { get; set; }
        public Zona Zona { get; set; }

        public List<Secao> Secoes { get; set; } = new List<Secao>();
        public string Nome { get; internal set; }

        internal static object Where(Func<object, bool> p)
        {
            throw new NotImplementedException();
        }
    }
}