/* Define o contrato para serviços responsáveis por sincronizar estados dependentes da data,
 * nomeadamente eventos e inscrições que transitam automaticamente para terminados. */
namespace GestorEventos.Partilhado.Servicos {
    public interface IAtualizadorEstados {
        void AtualizarEstados();
    }
}