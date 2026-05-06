// NOTA DE VERIFICACAO:
// Nesta versao, os metodos de criacao, alteracao e cancelamento de eventos
// nao devolvem ainda um resultado estruturado de sucesso/falha ao Controller.
// Numa iteracao posterior, e em alinhamento com o que ja foi adotado no modulo
// de Inscricoes, podera fazer sentido adaptar estes metodos para devolverem
// um objeto de resultado com confirmacao da operacao e mensagem associada,
// sobretudo quando a validacao completa e a integracao com SQLite estiverem consolidadas.

using System;
using System.Collections.Generic;
using System.Globalization;
using GestorEventos.Dados;
using GestorEventos.Partilhado;
using Microsoft.Data.Sqlite;

namespace GestorEventos.Eventos {
    class EventoModel {
        private readonly string connectionString;

        public delegate void EventoCanceladoHandler(object sender, EventoCanceladoEventArgs e);
        public event EventoCanceladoHandler? EventoCancelado;

        public EventoModel() {
            connectionString = ConfiguracaoAplicacao.ObterConnectionString();
        }

        // FUTURA MELHORIA:
        // devolver resultado estruturado (sucesso/mensagem) ao Controller,
        // em vez de este assumir sucesso apos chamada ao Model.
        public void CriarEvento(DadosEvento dados) {
            ValidarERegistarEvento(dados);
        }

        public void ValidarERegistarEvento(DadosEvento dados) {
            if (dados == null) {
                return;
            }

            if (string.IsNullOrWhiteSpace(dados.Nome) ||
                string.IsNullOrWhiteSpace(dados.Local) ||
                dados.Capacidade <= 0) {
                return;
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteTransaction transacao = ligacao.BeginTransaction();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = @"
                INSERT INTO eventos (nome, local, data, estado, capacidade)
                VALUES (@nome, @local, @data, 'ativo', @capacidade);";

            AdicionarParametrosEvento(comando, dados);
            comando.ExecuteNonQuery();
            transacao.Commit();
        }

        public List<Evento> ListarEventos() {
            return ObterListaEventos();
        }

        public List<Evento> ObterListaEventos() {
            List<Evento> eventos = new List<Evento>();

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.CommandText = @"
                SELECT id, nome, local, data, estado, capacidade
                FROM eventos
                ORDER BY data, id;";

            using SqliteDataReader leitor = comando.ExecuteReader();
            while (leitor.Read()) {
                eventos.Add(MapearEvento(leitor));
            }

            return eventos;
        }

        public Evento? ObterEvento(int idEvento) {
            return ObterDadosEvento(idEvento);
        }

        public Evento? ObterDadosEvento(int idEvento) {
            if (idEvento <= 0) {
                return null;
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.CommandText = @"
                SELECT id, nome, local, data, estado, capacidade
                FROM eventos
                WHERE id = @id;";
            comando.Parameters.AddWithValue("@id", idEvento);

            using SqliteDataReader leitor = comando.ExecuteReader();
            if (leitor.Read()) {
                return MapearEvento(leitor);
            }

            return null;
        }

        // FUTURA MELHORIA:
        // devolver resultado estruturado (sucesso/mensagem) ao Controller,
        // em vez de este assumir sucesso apos chamada ao Model.
        // Verificar se existem inscricoes que ultrapassem a nova capacidade, e impedir a alteracao se for o caso, por exemplo.
        public void AlterarEvento(int idEvento, DadosEvento dados) {
            ValidarEAtualizarEvento(idEvento, dados);
        }

        public void ValidarEAtualizarEvento(int idEvento, DadosEvento dados) {
            if (idEvento <= 0 || dados == null) {
                return;
            }

            if (string.IsNullOrWhiteSpace(dados.Nome) ||
                string.IsNullOrWhiteSpace(dados.Local) ||
                dados.Capacidade <= 0) {
                return;
            }

            int quantidadeInscrita = ObterQuantidadeInscritaAtiva(idEvento);
            if (quantidadeInscrita > dados.Capacidade) {
                return;
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteTransaction transacao = ligacao.BeginTransaction();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = @"
                UPDATE eventos
                SET nome = @nome,
                    local = @local,
                    data = @data,
                    capacidade = @capacidade,
                    atualizado_em = CURRENT_TIMESTAMP
                WHERE id = @id;";

            comando.Parameters.AddWithValue("@id", idEvento);
            AdicionarParametrosEvento(comando, dados);
            comando.ExecuteNonQuery();
            transacao.Commit();
        }

        // FUTURA MELHORIA:
        // devolver resultado estruturado (sucesso/mensagem) ao Controller,
        // em vez de este assumir sucesso apos chamada ao Model. 
        // ex. validação idEvento, atualizacao estado, evento já cancelado, ausência de subscritores, etc
        public void CancelarEvento(int idEvento) {
            Evento? eventoCancelado = ObterEvento(idEvento);

            if (eventoCancelado == null) {
                return;
            }

            AtualizarEstadoEvento(idEvento, "cancelado");
            eventoCancelado.Estado = "cancelado";

            DispararEventoCancelado(eventoCancelado);
        }

        public void AtualizarEstadoEvento(int idEvento, string estado) {
            if (idEvento <= 0 || string.IsNullOrWhiteSpace(estado)) {
                return;
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteTransaction transacao = ligacao.BeginTransaction();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = @"
                UPDATE eventos
                SET estado = @estado,
                    atualizado_em = CURRENT_TIMESTAMP,
                    cancelado_em = CASE
                        WHEN @estado = 'cancelado' THEN CURRENT_TIMESTAMP
                        ELSE cancelado_em
                    END
                WHERE id = @id;";

            comando.Parameters.AddWithValue("@id", idEvento);
            comando.Parameters.AddWithValue("@estado", estado);
            comando.ExecuteNonQuery();
            transacao.Commit();
        }

        private int ObterQuantidadeInscritaAtiva(int idEvento) {
            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.CommandText = @"
                SELECT COALESCE(SUM(quantidade), 0)
                FROM inscricoes
                WHERE id_evento = @idEvento
                  AND estado = 'ativa';";
            comando.Parameters.AddWithValue("@idEvento", idEvento);

            object? resultado = comando.ExecuteScalar();
            return Convert.ToInt32(resultado);
        }

        private void DispararEventoCancelado(Evento eventoCancelado) {
            EventoCanceladoEventArgs dadosCancelamento = new EventoCanceladoEventArgs(
                eventoCancelado.Id,
                eventoCancelado.Nome,
                DateTime.Now,
                eventoCancelado.Estado);

            EventoCancelado?.Invoke(this, dadosCancelamento);
        }

        public string ObterConnectionString() {
            return connectionString;
        }

        private static void AdicionarParametrosEvento(SqliteCommand comando, DadosEvento dados) {
            comando.Parameters.AddWithValue("@nome", dados.Nome.Trim());
            comando.Parameters.AddWithValue("@local", dados.Local.Trim());
            comando.Parameters.AddWithValue("@data", dados.Data.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            comando.Parameters.AddWithValue("@capacidade", dados.Capacidade);
        }

        private static Evento MapearEvento(SqliteDataReader leitor) {
            return new Evento {
                Id = leitor.GetInt32(0),
                Nome = leitor.GetString(1),
                Local = leitor.GetString(2),
                Data = DateTime.Parse(leitor.GetString(3), CultureInfo.InvariantCulture),
                Estado = leitor.GetString(4),
                Capacidade = leitor.GetInt32(5)
            };
        }
    }
}
