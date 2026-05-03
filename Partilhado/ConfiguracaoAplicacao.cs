using System;
using System.Configuration;
using System.IO;

namespace GestorEventos.Partilhado {
    static class ConfiguracaoAplicacao {
        private const string NomeConnectionString = "GestorEventosDb";
        private const string ChavePastaPdfs = "PastaPdfs";

        public static string ObterConnectionString() {
            ConnectionStringSettings config = ConfigurationManager.ConnectionStrings[NomeConnectionString];
            return config != null ? config.ConnectionString : string.Empty;
        }

        public static string ObterPastaPdfs() {
            string pastaConfigurada = ConfigurationManager.AppSettings[ChavePastaPdfs];

            if (string.IsNullOrWhiteSpace(pastaConfigurada)) {
                pastaConfigurada = "Pdfs";
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pastaConfigurada);
        }

        public static string CombinarCaminhoPdf(string nomeFicheiro) {
            return Path.Combine(ObterPastaPdfs(), nomeFicheiro);
        }
    }
}
