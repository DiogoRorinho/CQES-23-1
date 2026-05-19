// Implementação do serviço responsável por sincronizar estados dependentes da data atual.
using GestorEventos.Dados;
using Microsoft.Data.Sqlite;

namespace GestorEventos.Partilhado.Servicos
{
    public class AtualizadorEstadosService : IAtualizadorEstados
    {
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
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = @"
                UPDATE eventos
                SET estado = 'terminado',
                    atualizado_em = CURRENT_TIMESTAMP
                WHERE estado = 'ativo'
                  AND date(data) < date('now','localtime');
            ";

            comando.ExecuteNonQuery();
        }

        private static void AtualizarInscricoes(SqliteConnection ligacao, SqliteTransaction transacao)
        {
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

            comando.ExecuteNonQuery();
        }
    }
}