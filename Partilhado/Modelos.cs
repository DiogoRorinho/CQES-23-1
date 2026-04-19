using System;

namespace GestorEventosEsqueleto.Partilhado {
    class Evento {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Local { get; set; }
        public DateTime Data { get; set; }
        public string Estado { get; set; }
        public int Capacidade { get; set; }
    }

    class DadosEvento {
        public string Nome { get; set; }
        public string Local { get; set; }
        public DateTime Data { get; set; }
        public int Capacidade { get; set; }
    }

    class Inscricao {
        public int Id { get; set; }
        public int IdEvento { get; set; }
        public string Estado { get; set; }
        public string EmailParticipante { get; set; }
    }

    class DadosInscricao {
        public int IdEvento { get; set; }
        public string NomeParticipante { get; set; }
        public string EmailParticipante { get; set; }
        public int Quantidade { get; set; }
    }

    class DadosRelatorio {
        public string Titulo { get; set; }
        public string Conteudo { get; set; }
    }

    class DocumentoPdf {
        public string Titulo { get; set; }
        public string NomeFicheiro { get; set; }
        public string CaminhoFicheiro { get; set; }
    }
}
