// NOTA DE VERIFICACAO:
// Nesta versao, os metodos de criacao, alteracao e cancelamento de eventos
// nao devolvem ainda um resultado estruturado de sucesso/falha ao Controller.
// Numa iteracao posterior, e em alinhamento com o que ja foi adotado no modulo
// de Inscricoes, podera fazer sentido adaptar estes metodos para devolverem
// um objeto de resultado com confirmacao da operacao e mensagem associada,
// sobretudo quando a validacao completa e a integracao com SQLite estiverem consolidadas.

using System;
using System.Collections.Generic;
using GestorEventos.Partilhado;

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

            // Aqui ficarão a validação e o INSERT SQLite do evento.
        }

        public List<Evento> ListarEventos() {
            return ObterListaEventos();
        }

        public List<Evento> ObterListaEventos() {
            // Aqui ficará a query SQLite para obter eventos.
            return new List<Evento> {
                new Evento {
                    Id = 1,
                    Nome = "Workshop de Arquitetura",
                    Local = "Lisboa",
                    Data = new DateTime(2026, 5, 15),
                    Estado = "ativo",
                    Capacidade = 30
                },
                new Evento {
                    Id = 2,
                    Nome = "Seminario MVC",
                    Local = "Porto",
                    Data = new DateTime(2026, 6, 10),
                    Estado = "ativo",
                    Capacidade = 50
                }
            };
        }

        public Evento? ObterEvento(int idEvento) {
            return ObterDadosEvento(idEvento);
        }

        public Evento? ObterDadosEvento(int idEvento) {
            if (idEvento <= 0) {
                return null;
            }

            foreach (Evento evento in ObterListaEventos()) {
                if (evento.Id == idEvento) {
                    return evento;
                }
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

            // Aqui ficarão a validação e o UPDATE SQLite do evento.
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

            // Aqui ficará o UPDATE SQLite do estado do evento.
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
    }
}
