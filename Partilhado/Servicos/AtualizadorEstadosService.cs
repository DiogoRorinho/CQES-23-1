// Implementação do serviço responsável por sincronizar estados dependentes da data atual.
using GestorEventos.Dados;
using Microsoft.Data.Sqlite;

namespace GestorEventos.Partilhado.Servicos
{
    public class AtualizadorEstadosService : IAtualizadorEstados
    {
        public static event EventHandler<EventoTerminadoEventArgs>? EventoTerminado;
        public static event EventHandler<InscricaoTerminadaEventArgs>? InscricaoTerminada;

        public void AtualizarEstados()
        {
            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteTransaction transacao = ligacao.BeginTransaction();

            AtualizarEventos(ligacao, transacao);
            AtualizarInscricoes(ligacao, transacao);

            transacao.Commit();
        }

        private static void AtualizarEventos(SqliteConnection ligacao, SqliteTransaction transacao)
        {
            List<EventoTerminadoEventArgs> eventosTerminados = ObterEventosParaTerminar(ligacao, transacao);
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = @"
                UPDATE eventos
                SET estado = 'terminado',
                    atualizado_em = CURRENT_TIMESTAMP
                WHERE estado = 'ativo'
                  AND date(data) < date('now','localtime');
            ";

            int linhasAfetadas = comando.ExecuteNonQuery();
            if (linhasAfetadas > 0) {
                foreach (EventoTerminadoEventArgs evento in eventosTerminados) {
                    EventoTerminado?.Invoke(null, evento);
                }
            }
        }

        private static void AtualizarInscricoes(SqliteConnection ligacao, SqliteTransaction transacao)
        {
            List<InscricaoTerminadaEventArgs> inscricoesTerminadas = ObterInscricoesParaTerminar(ligacao, transacao);
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = @"
                UPDATE inscricoes
                SET estado = 'terminada',
                    atualizado_em = CURRENT_TIMESTAMP
                WHERE estado = 'ativa'
                  AND id_evento IN (
                      SELECT id
                      FROM eventos
                      WHERE estado = 'terminado'
                  );
            ";

            int linhasAfetadas = comando.ExecuteNonQuery();
            if (linhasAfetadas > 0) {
                foreach (InscricaoTerminadaEventArgs inscricao in inscricoesTerminadas) {
                    InscricaoTerminada?.Invoke(null, inscricao);
                }
            }
        }

        private static List<EventoTerminadoEventArgs> ObterEventosParaTerminar(SqliteConnection ligacao, SqliteTransaction transacao) {
            List<EventoTerminadoEventArgs> eventos = new List<EventoTerminadoEventArgs>();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = @"
                SELECT id, nome, data
                FROM eventos
                WHERE estado = 'ativo'
                  AND date(data) < date('now','localtime');
            ";

            using SqliteDataReader leitor = comando.ExecuteReader();
            while (leitor.Read()) {
                eventos.Add(new EventoTerminadoEventArgs(
                    leitor.GetInt32(0),
                    leitor.GetString(1),
                    DateTime.Parse(leitor.GetString(2)),
                    DateTime.Now));
            }

            return eventos;
        }

        private static List<InscricaoTerminadaEventArgs> ObterInscricoesParaTerminar(SqliteConnection ligacao, SqliteTransaction transacao) {
            List<InscricaoTerminadaEventArgs> inscricoes = new List<InscricaoTerminadaEventArgs>();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = @"
                SELECT i.id, i.id_evento, i.email_participante
                FROM inscricoes i
                INNER JOIN eventos e ON e.id = i.id_evento
                WHERE i.estado = 'ativa'
                  AND e.estado = 'terminado';
            ";

            using SqliteDataReader leitor = comando.ExecuteReader();
            while (leitor.Read()) {
                inscricoes.Add(new InscricaoTerminadaEventArgs(
                    leitor.GetInt32(0),
                    leitor.GetInt32(1),
                    leitor.GetString(2),
                    DateTime.Now));
            }

            return inscricoes;
        }
    }
}