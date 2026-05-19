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

            GarantirCompatibilidadeEstados(ligacao);
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
                private static void GarantirCompatibilidadeEstados(SqliteConnection ligacao) {
            if (!TabelaExiste(ligacao, "eventos") || !TabelaExiste(ligacao, "inscricoes")) {
                return;
            }

            string sqlEventos = ObterSqlTabela(ligacao, "eventos");
            string sqlInscricoes = ObterSqlTabela(ligacao, "inscricoes");

            bool eventosAceitamTerminado = sqlEventos.Contains("terminado", StringComparison.OrdinalIgnoreCase);
            bool inscricoesAceitamTerminada = sqlInscricoes.Contains("terminada", StringComparison.OrdinalIgnoreCase);

            if (!eventosAceitamTerminado || !inscricoesAceitamTerminada) {
                MigrarEsquemaEstados(ligacao);
            }
        }

        private static bool TabelaExiste(SqliteConnection ligacao, string nomeTabela) {
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.CommandText = @"
                SELECT COUNT(*)
                FROM sqlite_master
                WHERE type = 'table'
                  AND name = $nomeTabela;
            ";
            comando.Parameters.AddWithValue("$nomeTabela", nomeTabela);

            object? resultado = comando.ExecuteScalar();
            return Convert.ToInt32(resultado) > 0;
        }

        private static string ObterSqlTabela(SqliteConnection ligacao, string nomeTabela) {
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.CommandText = @"
                SELECT sql
                FROM sqlite_master
                WHERE type = 'table'
                  AND name = $nomeTabela;
            ";
            comando.Parameters.AddWithValue("$nomeTabela", nomeTabela);

            object? resultado = comando.ExecuteScalar();
            return resultado?.ToString() ?? string.Empty;
        }

        private static void MigrarEsquemaEstados(SqliteConnection ligacao) {
            using SqliteCommand desativarChaves = ligacao.CreateCommand();
            desativarChaves.CommandText = "PRAGMA foreign_keys = OFF;";
            desativarChaves.ExecuteNonQuery();

            try {
                using SqliteTransaction transacao = ligacao.BeginTransaction();
                using SqliteCommand comando = ligacao.CreateCommand();
                comando.Transaction = transacao;
                comando.CommandText = @"
                    DROP TABLE IF EXISTS inscricoes_antiga;
                    DROP TABLE IF EXISTS eventos_antiga;

                    ALTER TABLE inscricoes RENAME TO inscricoes_antiga;
                    ALTER TABLE eventos RENAME TO eventos_antiga;

                    CREATE TABLE eventos (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        nome TEXT NOT NULL,
                        local TEXT NOT NULL,
                        data TEXT NOT NULL,
                        estado TEXT NOT NULL DEFAULT 'ativo' CHECK (estado IN ('ativo', 'cancelado', 'terminado')),
                        capacidade INTEGER NOT NULL CHECK (capacidade > 0),
                        criado_em TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        atualizado_em TEXT,
                        cancelado_em TEXT
                    );

                    CREATE TABLE inscricoes (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        id_evento INTEGER NOT NULL,
                        nome_participante TEXT NOT NULL,
                        email_participante TEXT NOT NULL,
                        idade_participante INTEGER NOT NULL CHECK (idade_participante > 0),
                        quantidade INTEGER NOT NULL DEFAULT 1 CHECK (quantidade > 0),
                        estado TEXT NOT NULL DEFAULT 'ativa' CHECK (estado IN ('ativa', 'cancelada', 'cancelada_por_evento', 'terminada')),
                        criado_em TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        atualizado_em TEXT,
                        cancelado_em TEXT,
                        FOREIGN KEY (id_evento) REFERENCES eventos(id) ON UPDATE CASCADE ON DELETE RESTRICT
                    );

                    INSERT INTO eventos (id, nome, local, data, estado, capacidade, criado_em, atualizado_em, cancelado_em)
                    SELECT id, nome, local, data, estado, capacidade, criado_em, atualizado_em, cancelado_em
                    FROM eventos_antiga;

                    INSERT INTO inscricoes (id, id_evento, nome_participante, email_participante, idade_participante, quantidade, estado, criado_em, atualizado_em, cancelado_em)
                    SELECT id, id_evento, nome_participante, email_participante, idade_participante, quantidade, estado, criado_em, atualizado_em, cancelado_em
                    FROM inscricoes_antiga;

                    DROP TABLE inscricoes_antiga;
                    DROP TABLE eventos_antiga;

                    CREATE INDEX IF NOT EXISTS idx_eventos_estado ON eventos(estado);
                    CREATE INDEX IF NOT EXISTS idx_eventos_data ON eventos(data);
                    CREATE INDEX IF NOT EXISTS idx_inscricoes_evento ON inscricoes(id_evento);
                    CREATE INDEX IF NOT EXISTS idx_inscricoes_estado ON inscricoes(estado);
                ";
                comando.ExecuteNonQuery();
                transacao.Commit();
            }
            finally {
                using SqliteCommand ativarChaves = ligacao.CreateCommand();
                ativarChaves.CommandText = "PRAGMA foreign_keys = ON;";
                ativarChaves.ExecuteNonQuery();
            }
        }

    }
}
