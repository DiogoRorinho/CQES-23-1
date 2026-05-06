using System;
using System.IO;
using GestorEventos.Partilhado;
using Microsoft.Data.Sqlite;

namespace GestorEventos.Dados {
    static class BaseDados {
        private const string PastaSql = "Dados";
        private const string SubpastaSql = "Sql";
        private const string SchemaSql = "schema.sql";
        private const string SeedDemoSql = "seed-demo.sql";

        public static void Inicializar() {
            using SqliteConnection ligacao = CriarLigacaoAberta();

            ExecutarScript(ligacao, SchemaSql);

            if (ConfiguracaoAplicacao.DeveSemearDadosDemo() && BaseEstaVazia(ligacao)) {
                ExecutarScript(ligacao, SeedDemoSql);
            }
        }

        public static SqliteConnection CriarLigacaoAberta() {
            string connectionString = ObterConnectionStringNormalizada();

            if (string.IsNullOrWhiteSpace(connectionString)) {
                throw new InvalidOperationException("A connection string da base de dados nao esta configurada.");
            }

            SqliteConnection ligacao = new SqliteConnection(connectionString);
            ligacao.Open();

            using SqliteCommand comando = ligacao.CreateCommand();
            comando.CommandText = "PRAGMA foreign_keys = ON;";
            comando.ExecuteNonQuery();

            return ligacao;
        }

        private static string ObterConnectionStringNormalizada() {
            string connectionString = ConfiguracaoAplicacao.ObterConnectionString();

            if (string.IsNullOrWhiteSpace(connectionString)) {
                return string.Empty;
            }

            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder(connectionString);

            if (!string.IsNullOrWhiteSpace(builder.DataSource) &&
                builder.DataSource != ":memory:" &&
                !Path.IsPathRooted(builder.DataSource)) {
                builder.DataSource = Path.Combine(AppContext.BaseDirectory, builder.DataSource);
            }

            return builder.ConnectionString;
        }

        private static void ExecutarScript(SqliteConnection ligacao, string nomeFicheiro) {
            string caminhoScript = ObterCaminhoScript(nomeFicheiro);
            string scriptSql = File.ReadAllText(caminhoScript);

            using SqliteTransaction transacao = ligacao.BeginTransaction();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = scriptSql;
            comando.ExecuteNonQuery();
            transacao.Commit();
        }

        private static string ObterCaminhoScript(string nomeFicheiro) {
            string caminhoScript = Path.Combine(
                AppContext.BaseDirectory,
                PastaSql,
                SubpastaSql,
                nomeFicheiro);

            if (!File.Exists(caminhoScript)) {
                throw new FileNotFoundException("Ficheiro SQL nao encontrado.", caminhoScript);
            }

            return caminhoScript;
        }

        private static bool BaseEstaVazia(SqliteConnection ligacao) {
            return ObterTotalRegistos(ligacao, "eventos") == 0 &&
                   ObterTotalRegistos(ligacao, "inscricoes") == 0;
        }

        private static int ObterTotalRegistos(SqliteConnection ligacao, string tabela) {
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.CommandText = string.Format("SELECT COUNT(*) FROM {0};", tabela);

            object? resultado = comando.ExecuteScalar();
            return Convert.ToInt32(resultado);
        }
    }
}
