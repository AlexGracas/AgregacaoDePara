using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgregacaoModel
{
    public class Agregador
    {

        public ILogger logger { get; set; } = null;

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

        public int[] ZonasSelecionadas { get; set; } = null;
        void Execucao(String arquivoEntrada, String arquivoSaida, int dePara6Total = 0, int agregacaoProvisoria = 0, int dePara6Capital = 0, int agregacaoProvisoriaCapital = 0, int TTE = 0, int TTE_Capital = 0)
        {
            var zonas = (new ManipuladorCSV()).CarregarArquivo(arquivoEntrada);
            int totalSecoes = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).Count();
            logger.LogInformation($" Inicialmente total de { totalSecoes}  seções ");
            if (dePara6Capital != 0)
            {
                var zonasInterior = (new Agregador()).Agregar(
                    zonas.Where(zona => !(ZonasSelecionadas.Contains(zona.Numero))).Select(z => z).ToList()
                    , dePara6Total,
                    1);
                var zonasCapital = (new Agregador()).Agregar(
                    zonas.Where(zona => ZonasSelecionadas.Contains(zona.Numero)).Select(z => z).ToList()
                    , dePara6Capital,
                    1);
                zonas.Clear();
                zonas.AddRange(zonasInterior);
                zonas.AddRange(zonasCapital);
                int totalSecoesPasso1 = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).Count();
                logger.LogInformation($"De Para 6 com no máximo({dePara6Total}) eleitores no interior e ({dePara6Capital}) na Capital foram reduzidos  para {totalSecoesPasso1} seções");
            }
            else if (dePara6Total > 0)
            {
                zonas = (new Agregador()).Agregar(
                    zonas
                    , dePara6Total,
                    1);
                int totalSecoesPasso1 = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).Count();
                logger.LogInformation($"De Para 6 com no máximo({dePara6Total}) eleitores foram reduzidos de {totalSecoes} seções para {totalSecoesPasso1} seções");
            }
            if (agregacaoProvisoria != 0 || agregacaoProvisoriaCapital > 0)
            {
                if (agregacaoProvisoriaCapital != 0)
                {
                    var zonasInterior = (new Agregador()).Agregar(
                        zonas.Where(zona => !(ZonasSelecionadas.Contains(zona.Numero))).Select(z => z).ToList()
                        , agregacaoProvisoria,
                        2);
                    var zonasCapital = (new Agregador()).Agregar(
                        zonas.Where(zona => ZonasSelecionadas.Contains(zona.Numero)).Select(z => z).ToList()
                        , agregacaoProvisoriaCapital,
                        2);
                    zonas.Clear();
                    zonas.AddRange(zonasInterior);
                    zonas.AddRange(zonasCapital);
                    int totalSecoesPasso2 = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).Count();
                    logger.LogInformation($" e depois agregação provisória com ({agregacaoProvisoria}) eleitores no interior e ({agregacaoProvisoriaCapital}) na capital reduzimos para {totalSecoesPasso2} seções");
                }
                else
                {
                    zonas = (new Agregador()).Agregar(zonas, agregacaoProvisoria, 2);
                    int totalSecoesPasso2 = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).Count();
                    logger.LogInformation($" e depois agregação provisória com ({agregacaoProvisoria}) eleitores reduzimos para {totalSecoesPasso2} seções");
                }
            }
            int passoAnterior = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).Count();
            if (TTE > 0 || TTE_Capital > 0)
            {
                if (TTE_Capital > 0)
                {
                    var zonasCapital = this.ZonasDeSecoes(this.TTE(
                       this.SecoesDeZonas(zonas.Where(zona => ZonasSelecionadas.Contains(zona.Numero)).Select(z => z).ToList())
                       , TTE_Capital));
                    List<Zona> zonasInterior = null;
                    if (TTE > 0)
                    {
                        zonasInterior = this.ZonasDeSecoes(this.TTE(
                        this.SecoesDeZonas(zonas.Where(zona => !ZonasSelecionadas.Contains(zona.Numero)).Select(z => z).ToList())
                        , TTE));
                    }
                    else
                    {
                        zonasInterior = zonas.Where(zona => !(ZonasSelecionadas.Contains(zona.Numero))).Select(z => z).ToList();
                    }
                    zonas.Clear();
                    zonas.AddRange(zonasInterior);
                    zonas.AddRange(zonasCapital);
                    int totalSecoesTTE = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).Count();

                    logger.LogInformation($"TTE com ({TTE}) eleitores e nas selecionadas ({TTE_Capital}) foram reduzidos de {passoAnterior} seções para {totalSecoesTTE} seções");
                }
                else
                {
                    zonas = this.ZonasDeSecoes(this.TTE(
                       this.SecoesDeZonas(zonas)
                       , TTE));
                    int totalSecoesTTE = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).Count();
                    logger.LogInformation($"TTE com ({TTE}) eleitores foram reduzidos de {passoAnterior} seções para {totalSecoesTTE} seções");
                }
            }
            


            (new ManipuladorCSV()).GerarArquivo(zonas, arquivoSaida);

        }
    }
}
