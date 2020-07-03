using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgregacaoModel
{
    public class Agregador
    {

        public List<Zona> Agregar(List<Zona> Zonas, int qtdMaxAptos, int TipoAgregacao )
        {
            foreach (Zona z in Zonas)
            {
                foreach (LocalVotacao lv in z.Locais)
                {
                    var secoes = lv.Secoes.ToList();
                    secoes = secoes.OrderByDescending(s => s.QuantidadeAptosTotal).ToList();
                    for (int index = secoes.Count - 1; index >= 0; index = secoes.Count() - 1)
                    {
                        Secao sAvaliada = secoes[index];
                        secoes.Remove(sAvaliada);
                        int aptos = sAvaliada.QuantidadeAptosTotal;
                        Secao sAgregada = null;
                        while ((secoes.Count > 0) &&(qtdMaxAptos - sAvaliada.QuantidadeAptosTotal) > (sAgregada = secoes[0]).QuantidadeAptosTotal)
                        {
                            sAvaliada.Agregadas.Add(sAgregada);      
                            sAgregada.SecaoAgregadora = sAvaliada;
                            sAgregada.TipoAgregacao = TipoAgregacao;
                            secoes.Remove(sAgregada);
                            lv.Secoes.Remove(sAgregada);
                        }
                    }
                }
            }
            return Zonas;
        }

        public List<Zona> ZonasDeSecoes(List<Secao> Secoes)
        {
            List<Zona> zonas = Secoes.Select(s => s.Zona).Distinct().ToList();
            return zonas;
        }

        public List<Secao> SecoesDeZonas(List<Zona> zonas)
        {
            List<Secao> secoes = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).ToList();
            return secoes;
        }
        public List<Secao> TTE(List<Secao> secoes, int qtdMaxTTE)
        {
            var Zonas = ZonasDeSecoes(secoes);
            foreach (Zona z in Zonas)
            {
                foreach (LocalVotacao lv in z.Locais)
                {
                    var secoesLocal = lv.Secoes.ToList();                  
                    secoesLocal = secoesLocal.OrderByDescending(s => s.QuantidadeAptosTotal).ToList();
                    int totalAptosLocalVotacao = secoesLocal.Sum(s => s.QuantidadeAptosTotal);
                    int qtdSecoesTTE = (int)Math.Ceiling((double)totalAptosLocalVotacao / qtdMaxTTE);
                    
                    if (qtdSecoesTTE < lv.Secoes.Count)
                    {
                        List<Secao> SecoesTTE = new List<Secao>();
                        for (int index = 0; index < qtdSecoesTTE; index++)
                        {
                            var SecaoAvaliada = lv.Secoes[0];
                            totalAptosLocalVotacao -= qtdMaxTTE;
                            var secaoTTE = new Secao() {
                                Numero = SecaoAvaliada.Numero,
                                Municipio = SecaoAvaliada.Municipio,
                                QuantidadeAptos = Math.Min(totalAptosLocalVotacao, qtdMaxTTE),
                                LocalVotacao = lv,
                                Zona = lv.Zona,
                                TTE = true
                            };
                            secaoTTE.Agregadas.Add(SecaoAvaliada);
                            SecoesTTE.Add(secaoTTE);
                            lv.Secoes.Remove(SecaoAvaliada);                            
                        }
                        SecoesTTE.Last().Agregadas.AddRange(lv.Secoes);
                        lv.Secoes = SecoesTTE;
                    }                                       
                }
            }
            return SecoesDeZonas(Zonas);
        }
    }
}
