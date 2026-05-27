using System;
using System.IO;
using System.Text;
using GestorEventos.Eventos;
using GestorEventos.Inscricoes;
using GestorEventos.Partilhado.Servicos;

namespace GestorEventos.Aplicacao {
    /* Handler responsável por registar em ficheiro os principais acontecimentos
     * de domínio associados a eventos e inscrições. */
    class EventosDominioHandler {
        private readonly string caminhoLog;

        // Prepara a pasta e o ficheiro de log onde serão registadas as notificações do domínio.
        public EventosDominioHandler() {
            string pastaNotificacoes = Path.Combine(AppContext.BaseDirectory, "Notificacoes");
            Directory.CreateDirectory(pastaNotificacoes);
            caminhoLog = Path.Combine(pastaNotificacoes, "eventos-dominio.log");
        }

        public void OnEventoCriado(object sender, EventoCriadoEventArgs e) {
            Registar(string.Format(
                "EventoCriado | id={0} | nome={1} | data={2:yyyy-MM-dd} | capacidade={3}",
                e.IdEvento,
                e.NomeEvento,
                e.DataEvento,
                e.Capacidade));
        }

        public void OnEventoAlterado(object sender, EventoAlteradoEventArgs e) {
            Registar(string.Format(
                "EventoAlterado | id={0} | nome={1} | data={2:yyyy-MM-dd} | capacidade={3}",
                e.IdEvento,
                e.NomeEvento,
                e.DataEvento,
                e.Capacidade));
        }

        public void OnEventoTerminado(object? sender, EventoTerminadoEventArgs e) {
            Registar(string.Format(
                "EventoTerminado | id={0} | nome={1} | data_evento={2:yyyy-MM-dd}",
                e.IdEvento,
                e.NomeEvento,
                e.DataEvento));
        }

        public void OnInscricaoCriada(object sender, InscricaoCriadaEventArgs e) {
            Registar(string.Format(
                "InscricaoCriada | id={0} | evento={1} | participante={2} | qtd={3}",
                e.IdInscricao,
                e.IdEvento,
                e.NomeParticipante,
                e.Quantidade));
        }

        public void OnInscricaoAlterada(object sender, InscricaoAlteradaEventArgs e) {
            Registar(string.Format(
                "InscricaoAlterada | id={0} | evento={1} | participante={2} | qtd={3}",
                e.IdInscricao,
                e.IdEvento,
                e.NomeParticipante,
                e.Quantidade));
        }

        public void OnInscricaoCancelada(object sender, InscricaoCanceladaEventArgs e) {
            Registar(string.Format(
                "InscricaoCancelada | id={0} | estado={1}",
                e.IdInscricao,
                e.EstadoFinal));
        }

        public void OnInscricaoTerminada(object? sender, InscricaoTerminadaEventArgs e) {
            Registar(string.Format(
                "InscricaoTerminada | id={0} | evento={1} | email={2}",
                e.IdInscricao,
                e.IdEvento,
                e.EmailParticipante));
        }

        // Acrescenta ao ficheiro de log uma linha com timestamp e descrição do acontecimento.
        private void Registar(string mensagem) {
            string linha = string.Format("{0:yyyy-MM-dd HH:mm:ss} | {1}", DateTime.Now, mensagem);
            File.AppendAllText(caminhoLog, linha + Environment.NewLine, Encoding.UTF8);
        }
    }
}