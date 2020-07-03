using System;
using System.Collections.Generic;

namespace AgregacaoModel
{
    public class Zona
    {
        public int Numero { get; set; }

        public List<LocalVotacao> Locais { get; set; } = new List<LocalVotacao>();

        public String MunicipioSede { get; set; }

    }
}
