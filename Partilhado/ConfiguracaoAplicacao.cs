using System;
using System.IO;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace GestorEventos.Partilhado {
    static class ConfiguracaoAplicacao {
        private const string NomeConnectionString = "GestorEventosDb";
        private const string ChavePastaPdfs = "PastaPdfs";
        private static readonly Lazy<IConfigurationRoot> Configuracao = new Lazy<IConfigurationRoot>(CriarConfiguracao);

        public static string ObterConnectionString() {
            return Configuracao.Value.GetConnectionString(NomeConnectionString) ?? string.Empty;
        }

        public static string ObterPastaPdfs() {
            string? pastaConfigurada = Configuracao.Value[ChavePastaPdfs];

            if (string.IsNullOrWhiteSpace(pastaConfigurada)) {
                pastaConfigurada = "Pdfs";
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pastaConfigurada);
        }

        public static string CombinarCaminhoPdf(string nomeFicheiro) {
            return Path.Combine(ObterPastaPdfs(), nomeFicheiro);
        }

        private static IConfigurationRoot CriarConfiguracao() {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();
        }
    }
}
