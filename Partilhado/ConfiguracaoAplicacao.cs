using System;
using System.IO;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace GestorEventos.Partilhado {
    /* Classe utilitária responsável por centralizar a leitura da configuração da aplicação,
     * nomeadamente connection string, pasta de PDFs e opções de inicialização. */
    static class ConfiguracaoAplicacao {
        private const string NomeConnectionString = "GestorEventosDb";
        private const string ChavePastaPdfs = "PastaPdfs";
        private const string ChaveSeedDemoData = "SeedDemoData";
        private static readonly Lazy<IConfigurationRoot> Configuracao = new Lazy<IConfigurationRoot>(CriarConfiguracao);

        public static string ObterConnectionString() {
            return Configuracao.Value.GetConnectionString(NomeConnectionString) ?? string.Empty;
        }

        // Devolve a pasta base onde serão guardados os ficheiros PDF gerados pela aplicação.
        public static string ObterPastaPdfs() {
            string? pastaConfigurada = Configuracao.Value[ChavePastaPdfs];

            if (string.IsNullOrWhiteSpace(pastaConfigurada)) {
                pastaConfigurada = "Pdfs";
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pastaConfigurada);
        }

        // Constrói o caminho completo de um PDF a partir do nome do ficheiro.
        public static string CombinarCaminhoPdf(string nomeFicheiro) {
            return Path.Combine(ObterPastaPdfs(), nomeFicheiro);
        }

        // Indica se a aplicação deve criar dados de demonstração no arranque.
        public static bool DeveSemearDadosDemo() {
            string? valorConfigurado = Configuracao.Value[ChaveSeedDemoData];
            return bool.TryParse(valorConfigurado, out bool deveSemear) && deveSemear;
        }

        // Cria a configuração a partir do ficheiro appsettings.json.
        private static IConfigurationRoot CriarConfiguracao() {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();
        }
    }
}
