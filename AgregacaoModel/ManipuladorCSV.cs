using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AgregacaoModel
{
    public class ManipuladorCSV
    {
        ILogger logger;
        
        public List<Zona> CarregarArquivo(String caminhoArquivo)
        {
            List<Zona> Zonas = new List<Zona>();
            var linhas = File.ReadAllLines(caminhoArquivo);
            foreach (var linha in linhas)
            {
                var termos = linha.Split(';');
                Secao s = new Secao();
                Zona z = null;
                LocalVotacao lv = null;

                int numSecao = int.Parse(termos[1]);
                int numZona = int.Parse(termos[0]);
                String nomeLocal = termos[3];
                int numLocal = int.Parse(termos[2]); 
                int qtdAptos = int.Parse(termos[4]); 

                s.Numero = numSecao;
                s.QuantidadeAptos = qtdAptos;
                if ((z = Zonas.Where(zx => zx.Numero == numZona).FirstOrDefault()) == null)
                {
                    z = new Zona() { Numero = numZona };
                    Zonas.Add(z);
                }

                s.Zona = z;
                if ((lv = z.Locais.Where(lx => lx.Numero == numLocal && lx.Nome == nomeLocal).FirstOrDefault()) == null)
                {
                    lv = new LocalVotacao() { Numero = numLocal, Nome = nomeLocal };
                    z.Locais.Add(lv);                   
                    lv.Zona = z;
                }
                lv.Secoes.Add(s);
                s.LocalVotacao = lv;

            }
            return Zonas;
        }

        public void GerarArquivo(List<Zona> Zonas, String caminhoArquivo)
        {

            List<String> linhas = new List<string>();
            foreach(Zona z in Zonas)
            {
                foreach(LocalVotacao lv in z.Locais)
                {
                    foreach(Secao s in lv.Secoes)
                    {
                        //Número Seção; Número Zona; Número Local de Votação; Número Local de Votação_1; Aptos e Suspensos; Aptos
                        linhas.AddRange(GerarLinhasSecoes(s));                        
                    }
                }
            }
            File.WriteAllLines(caminhoArquivo, linhas.ToArray());
        }
        public List<String> GerarLinhasSecoes(Secao s, int nivel = 0)
        {
            List<String> linhas = new List<string>();
            linhas.Add(GerarLinhaSecao(s, nivel));
            foreach(var ss in s.Agregadas)
            {
                linhas.AddRange(GerarLinhasSecoes(ss, nivel +1));
            }
            return linhas;
        }


        public String GerarLinhaSecao(Secao s, int nivel)
        {
            String Separador = ";";
            Zona z = s.LocalVotacao.Zona;
            LocalVotacao lv = s.LocalVotacao;
            String linha = z.Numero.ToString() + Separador +
                            s.Numero.ToString() + Separador +
                            lv.Nome + Separador + lv.Numero.ToString() + Separador;
            //Quantidade de Eleitores Aptos Após a Agregação.
            linha += Nivel(s, 0,nivel, Separador);
            linha += Nivel(s, 1,nivel, Separador);
            linha += Nivel(s, 2,nivel, Separador);

            //Quantidade de eleitores Aptos antes da agregação.
            //linha += s.QuantidadeAptos + Separador;
            if (s.SecaoAgregadora == null)
            {
                linha += "-";
            }
            else
            {
                linha += s.SecaoAgregadora.Numero;
            }
            linha += Separador;
            if (s.SecaoAgregadora == null)
            {
                if (s.Agregadas.Count == 0)
                {
                    linha += "-";
                }
                else
                {
                    linha += s.Numero;
                }
            }
            else
            {
                linha += s.SecaoAgregadora.Numero;
            }
            linha += Separador;
            linha += (s.TipoAgregacao == 1 ? "Definitiva" : s.TipoAgregacao == 2 ? "Provisoria" : "-") + Separador;
            linha += s.TTE ? "TTE" : "-";
            return linha;

        }

        private static string Nivel(Secao s, int NivelEscrita, int NivelVerificado, string Separador)
        {
            String linha = "";
            if (NivelEscrita == 0)
            {
                linha += s.QuantidadeAptos;
            }else if (NivelEscrita > 0 ){
                if(s.TipoAgregacao == 0)
                {
                    var Agregadas = s.Agregadas.Where(sa => sa.TipoAgregacao <= NivelEscrita).ToList();
                    linha += Agregadas.Sum(sa => sa.QuantidadeAptosTotal) + s.QuantidadeAptos;                    
                }
                else if (s.TipoAgregacao > NivelEscrita)
                {
                    var Agregadas = s.Agregadas.Where(sa => sa.TipoAgregacao <= NivelEscrita).ToList();
                    if (Agregadas.Count() > 0  )
                    {
                        linha += Agregadas.Sum(sa => sa.QuantidadeAptosTotal) + s.QuantidadeAptos;
                    }
                    else
                    {
                        linha += s.QuantidadeAptos;
                    }
                }else
                {
                    linha += "-";
                }

            }else {
                linha += "-" ;
            }
            linha += Separador;
            return linha;
        }
    }
}
