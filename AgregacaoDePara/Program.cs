using AgregacaoModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AgregacaoDePara
{
    class Program
    {
        static void Main(string[] args)
        {
            int qtdMax = 0;
            String caminho;
            if(args.Length < 3)
            {
                Console.WriteLine("Parâmetros:");
                Console.WriteLine("1º Número máximo de eleitores por seção.");
                Console.WriteLine("2º Nome do arquivo CSV com as seções. CSV: Número da Seção, Número Zona, Número Local Votação, Nome Local de Votação, Quantidade Eleitores ");
                Console.WriteLine("Exemplo:");
                Console.WriteLine( System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName 
                    + @" 425 c:\secoes.csv");
                Console.WriteLine("Digite a quantidade máxima de eleitores:");
                //qtdMax = int.Parse(Console.ReadLine());
                qtdMax = 500;
                Console.WriteLine("Digite o endereço do arquivo:");
                //caminho = Console.ReadLine();
                caminho = @"D:\secoes.csv";
            }
            else {
                qtdMax = int.Parse(args[0]);
                caminho = args[1];
            }



            /*Execucao(@"D:\secoes.csv", @"D:\secoes425.csv", 425);
            Execucao(@"D:\secoes.csv", @"D:\secoes500.csv", 500);
            Execucao(@"D:\secoes.csv", @"D:\secoes425Depois500.csv", 425, 500);
            Execucao(@"D:\secoes.csv", @"D:\secoes550.csv", 550);
            Execucao(@"D:\secoes.csv", @"D:\secoes4550Depois550.csv", 450,550);*/
            /*Execucao(@"D:\secoes.csv", @"D:\secoes450Depois600.csv", 450, 600);
            Execucao(@"D:\secoes.csv", @"D:\secoes500Depois600.csv", 500, 600);
            Execucao(@"D:\secoes.csv", @"D:\secoes450Depois650.csv", 450, 650);
            Execucao(@"D:\secoes.csv", @"D:\secoes450-450Depois650-650.csv", 450, 650, 450,650);
            Execucao(@"D:\secoes.csv", @"D:\secoes450-500Depois600.csv", 450, 600,500);
            Execucao(@"D:\secoes.csv", @"D:\secoes450-500Depois600-650.csv", 450, 600, 500,650);*/
            Execucao(@"D:\secoes.csv", @"D:\secoesn600.csv", dePara6Total: 450);
            Execucao(@"D:\secoes.csv", @"D:\secoesn600_tte.csv", dePara6Total: 450,agregacaoProvisoria: 600, TTE: 600);
            Execucao(@"D:\secoes.csv", @"D:\secoesntte.csv", TTE: 450);

        }

        static void Execucao(String arquivoEntrada, String arquivoSaida, int dePara6Total = 0, int agregacaoProvisoria=0, int dePara6Capital=0, int agregacaoProvisoriaCapital=0, int TTE = 0)
        {
            var Agregador = new Agregador();
            var zonas = (new ManipuladorCSV()).CarregarArquivo(arquivoEntrada);
            int totalSecoes = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).Count();
            if (dePara6Capital != 0)
            {
                var zonasInterior = (new Agregador()).Agregar(
                    zonas.Where(zona => !(new int[] { 1, 2, 3, 4, 145, 174, 175, 176, 177, 178 }.Contains(zona.Numero))).Select(z => z).ToList()
                    , dePara6Total,
                    1);
                var zonasCapital = (new Agregador()).Agregar(
                    zonas.Where(zona => new int[] { 1, 2, 3, 4, 145, 174, 175, 176, 177, 178 }.Contains(zona.Numero)).Select(z => z).ToList()
                    , dePara6Capital,
                    1);
                zonas.Clear();
                zonas.AddRange(zonasInterior);
                zonas.AddRange(zonasCapital);
                int totalSecoesPasso1 = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).Count();
                Console.WriteLine($"De Para 6 com no máximo({dePara6Total}) eleitores no interior e ({dePara6Capital}) na Capital foram reduzidos de {totalSecoes} seções para {totalSecoesPasso1} seções");
            }
            else if(dePara6Total>0)
            {
                zonas = (new Agregador()).Agregar(
                    zonas
                    , dePara6Total,
                    1);
                int totalSecoesPasso1 = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).Count();
                Console.WriteLine($"De Para 6 com no máximo({dePara6Total}) eleitores foram reduzidos de {totalSecoes} seções para {totalSecoesPasso1} seções");
            }
            if (agregacaoProvisoria != 0 || agregacaoProvisoriaCapital >0) {
                if (agregacaoProvisoriaCapital != 0)
                {
                    var zonasInterior = (new Agregador()).Agregar(
                        zonas.Where(zona => !(new int[] { 1, 2, 3, 4, 145, 174, 175, 176, 177, 178 }.Contains(zona.Numero))).Select(z => z).ToList()
                        , agregacaoProvisoria,
                        2);
                    var zonasCapital = (new Agregador()).Agregar(
                        zonas.Where(zona => new int[] { 1, 2, 3, 4, 145, 174, 175, 176, 177, 178 }.Contains(zona.Numero)).Select(z => z).ToList()
                        , agregacaoProvisoriaCapital,
                        2);
                    zonas.Clear();
                    zonas.AddRange(zonasInterior);
                    zonas.AddRange(zonasCapital);
                    int totalSecoesPasso2 = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).Count();
                    Console.WriteLine($" e depois agregação provisória com ({agregacaoProvisoria}) eleitores no interior e ({agregacaoProvisoriaCapital}) na capital reduzimos para {totalSecoesPasso2} seções");
                }
                else                
                {
                    zonas = (new Agregador()).Agregar(zonas, agregacaoProvisoria,2);
                    int totalSecoesPasso2 = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).Count();
                    Console.WriteLine($" e depois agregação provisória com ({agregacaoProvisoria}) eleitores reduzimos para {totalSecoesPasso2} seções");
                }
            }
            int passoAnterior = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).Count();
            if (TTE > 0)
            {
                /*
                int[] ZonasTTE = new int[] { 1, 2, 3, 4, 145, 174, 175, 176, 177, 178 };//,66,137,192,41,42,146,157 };
                var zonasCapital = Agregador.ZonasDeSecoes(Agregador.TTE(
                   Agregador.SecoesDeZonas(zonas.Where(zona => ZonasTTE.Contains(zona.Numero)).Select(z => z).ToList())
                   , TTE));
                var zonasInterior = zonas.Where(zona => !(ZonasTTE.Contains(zona.Numero))).Select(z => z).ToList();
                zonas.Clear();
                zonas.AddRange(zonasInterior);
                zonas.AddRange(zonasCapital);
                */
                zonas = Agregador.ZonasDeSecoes(Agregador.TTE(
                   Agregador.SecoesDeZonas(zonas)
                   , TTE));
                int totalSecoesTTE = zonas.SelectMany(z => z.Locais).SelectMany(l => l.Secoes).Count();
                Console.WriteLine($"TTE com ({TTE}) eleitores foram reduzidos de {passoAnterior} seções para {totalSecoesTTE} seções");
            }
            Console.WriteLine(" ");
            
            
            (new ManipuladorCSV()).GerarArquivo(zonas, arquivoSaida);

        }




       


    }
}
