using System;
using System.Collections.Generic;
using System.Globalization;
using GestorEventos.Dados;
using GestorEventos.Partilhado;
using GestorEventos.Partilhado.Servicos;
using Microsoft.Data.Sqlite;

namespace GestorEventos.Eventos {

    class EventoModel : IAtualizadorEstados {
        private readonly string connectionString;
        private readonly IAtualizadorEstados atualizadorEstados;

        public delegate void EventoCanceladoHandler(object sender, EventoCanceladoEventArgs e);
        public delegate void EventoCriadoHandler(object sender, EventoCriadoEventArgs e);
        public delegate void EventoAlteradoHandler(object sender, EventoAlteradoEventArgs e);
        public event EventoCanceladoHandler? EventoCancelado;
        public event EventoCriadoHandler? EventoCriado;
        public event EventoAlteradoHandler? EventoAlterado;

        public EventoModel() {
            connectionString = ConfiguracaoAplicacao.ObterConnectionString();
            atualizadorEstados = new AtualizadorEstadosService();
        }

        public void AtualizarEstados() {
            atualizadorEstados.AtualizarEstados();
        }

        public ResultadoOperacaoEvento CriarEvento(DadosEvento dados) {
            return ValidarERegistarEvento(dados);
        }

        public ResultadoOperacaoEvento ValidarERegistarEvento(DadosEvento dados) {
            if (dados == null) {
                return CriarResultado(false, "Dados do evento invalidos.");
            }

            if (string.IsNullOrWhiteSpace(dados.Nome) ||
                string.IsNullOrWhiteSpace(dados.Local) ||
                dados.Capacidade <= 0) {
                return CriarResultado(false, "Dados do evento invalidos.");
            }

            if (!DataEventoEhFutura(dados.Data)) {
                return CriarResultado(false, "A data do evento deve ser futura.");
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteTransaction transacao = ligacao.BeginTransaction();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = @"
                INSERT INTO eventos (nome, local, data, estado, capacidade)
                VALUES (@nome, @local, @data, 'ativo', @capacidade);
                SELECT last_insert_rowid();";

            AdicionarParametrosEvento(comando, dados);
            int idEventoCriado = Convert.ToInt32(comando.ExecuteScalar(), CultureInfo.InvariantCulture);
            transacao.Commit();

            DispararEventoCriado(idEventoCriado, dados);

            return CriarResultado(true, "Evento criado com sucesso.");
        }

        public List<Evento> ListarEventos() {
            return ObterListaEventos();
        }

        public List<Evento> ListarEventosAtivos() {
            return ObterListaEventos("ativo", ordenarPorId: true);
        }

        public List<Evento> ObterListaEventos() {
            return ObterListaEventos(null, ordenarPorId: false);
        }

        private List<Evento> ObterListaEventos(string? estado, bool ordenarPorId) {
            List<Evento> eventos = new List<Evento>();
            AtualizarEstados();
            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteCommand comando = ligacao.CreateCommand();
            string ordem = ordenarPorId ? "id" : "data, id";
            comando.CommandText = string.Format(@"
                SELECT id, nome, local, data, estado, capacidade
                FROM eventos{0}
                ORDER BY {1};",
                string.IsNullOrWhiteSpace(estado) ? string.Empty : " WHERE estado = @estado",
                ordem);

            if (!string.IsNullOrWhiteSpace(estado)) {
                comando.Parameters.AddWithValue("@estado", estado);
            }

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
            AtualizarEstados();
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

        public ResultadoOperacaoEvento AlterarEvento(int idEvento, DadosEvento dados) {
            return ValidarEAtualizarEvento(idEvento, dados);
        }

        public ResultadoOperacaoEvento ValidarEAtualizarEvento(int idEvento, DadosEvento dados) {
            AtualizarEstados();
            if (idEvento <= 0 || dados == null) {
                return CriarResultado(false, "ID de evento invalido.");
            }

            if (string.IsNullOrWhiteSpace(dados.Nome) ||
                string.IsNullOrWhiteSpace(dados.Local) ||
                dados.Capacidade <= 0) {
                return CriarResultado(false, "Dados do evento invalidos.");
            }

            if (!DataEventoEhFutura(dados.Data)) {
                return CriarResultado(false, "A data do evento deve ser futura.");
            }

            int quantidadeInscrita = ObterQuantidadeInscritaAtiva(idEvento);
            if (quantidadeInscrita > dados.Capacidade) {
                return CriarResultado(false, "A capacidade nao pode ser inferior ao numero de inscricoes ativas.");
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
            int linhasAfetadas = comando.ExecuteNonQuery();
            transacao.Commit();

            if (linhasAfetadas == 0) {
                return CriarResultado(false, "Evento nao encontrado.");
            }

            DispararEventoAlterado(idEvento, dados);

            return CriarResultado(true, "Evento alterado com sucesso.");
        }

        public ResultadoOperacaoEvento CancelarEvento(int idEvento) {
            AtualizarEstados();
            Evento? eventoCancelado = ObterEvento(idEvento);

            if (eventoCancelado == null) {
                return CriarResultado(false, "Evento nao encontrado.");
            }

            if (!string.Equals(eventoCancelado.Estado, "ativo", StringComparison.OrdinalIgnoreCase)) {
                if (string.Equals(eventoCancelado.Estado, "terminado", StringComparison.OrdinalIgnoreCase)) {
                    return CriarResultado(false, "Evento ja se encontra terminado.");
                }

                return CriarResultado(false, "Evento ja se encontra cancelado.");
            }

            if (!AtualizarEstadoEvento(idEvento, "cancelado")) {
                return CriarResultado(false, "Evento ja se encontra cancelado.");
            }

            eventoCancelado.Estado = "cancelado";

            DispararEventoCancelado(eventoCancelado);
            return CriarResultado(true, "Evento cancelado com sucesso.");
        }

        public bool AtualizarEstadoEvento(int idEvento, string estado) {
            if (idEvento <= 0 || string.IsNullOrWhiteSpace(estado)) {
                return false;
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
                        WHEN @estado = 'cancelado' AND cancelado_em IS NULL THEN CURRENT_TIMESTAMP
                        ELSE cancelado_em
                    END
                WHERE id = @id
                  AND (@estado <> 'cancelado' OR estado = 'ativo');";

            comando.Parameters.AddWithValue("@id", idEvento);
            comando.Parameters.AddWithValue("@estado", estado);
            int linhasAfetadas = comando.ExecuteNonQuery();
            transacao.Commit();

            return linhasAfetadas > 0;
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

        private void DispararEventoCriado(int idEvento, DadosEvento dados) {
            EventoCriadoEventArgs dadosCriacao = new EventoCriadoEventArgs(
                idEvento,
                dados.Nome,
                dados.Local,
                dados.Data,
                dados.Capacidade,
                DateTime.Now);

            EventoCriado?.Invoke(this, dadosCriacao);
        }

        private void DispararEventoAlterado(int idEvento, DadosEvento dados) {
            EventoAlteradoEventArgs dadosAlteracao = new EventoAlteradoEventArgs(
                idEvento,
                dados.Nome,
                dados.Local,
                dados.Data,
                dados.Capacidade,
                DateTime.Now);

            EventoAlterado?.Invoke(this, dadosAlteracao);
        }

        public string ObterConnectionString() {
            return connectionString;
        }

        private static bool DataEventoEhFutura(DateTime dataEvento) {
            return dataEvento.Date > DateTime.Today;
        }

        private static ResultadoOperacaoEvento CriarResultado(bool sucesso, string mensagem) {
            return new ResultadoOperacaoEvento {
                Sucesso = sucesso,
                Mensagem = mensagem
            };
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
