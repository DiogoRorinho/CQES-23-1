using System;

/* Modelos de dados e DTOs partilhados entre os vários módulos da aplicação.
 * Incluem entidades de domínio, objetos de input/output e estruturas de resultado. */
namespace GestorEventos.Partilhado {
    // --- Eventos ---
    class Evento {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int Capacidade { get; set; }
    }

    class DadosEvento {
        public string Nome { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public int Capacidade { get; set; }
    }

    class ResultadoOperacaoEvento {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }

    // --- Inscrições ---
    class Inscricao {
        public int Id { get; set; }
        public int IdEvento { get; set; }
        public string NomeParticipante { get; set; } = string.Empty;
        public int IdadeParticipante { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string EmailParticipante { get; set; } = string.Empty;
        public int Quantidade { get; set; } = 1;
    }

    class DadosInscricao {
        public int IdEvento { get; set; }
        public string NomeParticipante { get; set; } = string.Empty;
        public string EmailParticipante { get; set; } = string.Empty;
        public int IdadeParticipante { get; set; }
        public int Quantidade { get; set; }
    }
    
    class ResultadoCriacaoInscricao {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public DocumentoPdf? BilhetePdf { get; set; }
    }

    // --- Relatórios e PDFs ---
    class DadosRelatorio {
        public string Titulo { get; set; } = string.Empty;
        public string Conteudo { get; set; } = string.Empty;
    }

    class DocumentoPdf {
        public string Titulo { get; set; } = string.Empty;
        public string NomeFicheiro { get; set; } = string.Empty;
        public string CaminhoFicheiro { get; set; } = string.Empty;
    }
}
