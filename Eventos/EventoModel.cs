using System.Collections.Generic;
using GestorEventosEsqueleto.Partilhado;

namespace GestorEventosEsqueleto.Eventos {
    class EventoModel {
        private readonly string connectionString;

        public delegate void EventoCanceladoHandler(object sender, EventoCanceladoEventArgs e);
        public event EventoCanceladoHandler EventoCancelado;

        public EventoModel() {
            connectionString = ConfiguracaoAplicacao.ObterConnectionString();
        }

        public void CriarEvento(DadosEvento dados) {
            ValidarERegistarEvento(dados);
        }

        public void ValidarERegistarEvento(DadosEvento dados) {
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
                    Data = new System.DateTime(2026, 5, 15),
                    Estado = "ativo",
                    Capacidade = 30
                },
                new Evento {
                    Id = 2,
                    Nome = "Seminario MVC",
                    Local = "Porto",
                    Data = new System.DateTime(2026, 6, 10),
                    Estado = "ativo",
                    Capacidade = 50
                }
            };
        }

        public Evento ObterEvento(int idEvento) {
            return ObterDadosEvento(idEvento);
        }

        public Evento ObterDadosEvento(int idEvento) {
            // Aqui ficará a query SQLite para obter um evento específico.
            return new Evento {
                Id = idEvento,
                Nome = "Evento " + idEvento,
                Local = "Local por definir",
                Data = new System.DateTime(2026, 7, 1),
                Estado = "ativo",
                Capacidade = 100
            };
        }

        public void AlterarEvento(int idEvento, DadosEvento dados) {
            ValidarEAtualizarEvento(idEvento, dados);
        }

        public void ValidarEAtualizarEvento(int idEvento, DadosEvento dados) {
            // Aqui ficarão a validação e o UPDATE SQLite do evento.
        }

        public void CancelarEvento(int idEvento) {
            Evento eventoCancelado = ObterEvento(idEvento);

            AtualizarEstadoEvento(idEvento, "cancelado");
            eventoCancelado.Estado = "cancelado";

            DispararEventoCancelado(eventoCancelado);
        }

        public void AtualizarEstadoEvento(int idEvento, string estado) {
            // Aqui ficará o UPDATE SQLite do estado do evento.
        }

        private void DispararEventoCancelado(Evento eventoCancelado) {
            if (EventoCancelado == null) {
                return;
            }

            EventoCanceladoEventArgs dadosCancelamento = new EventoCanceladoEventArgs(
                eventoCancelado.Id,
                eventoCancelado.Nome,
                System.DateTime.Now,
                eventoCancelado.Estado);

            EventoCancelado(this, dadosCancelamento);
        }

        public string ObterConnectionString() {
            return connectionString;
        }
    }
}
