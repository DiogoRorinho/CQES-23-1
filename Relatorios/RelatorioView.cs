using System;
using System.Collections.Generic;
using GestorEventos.Partilhado;

namespace GestorEventos.Relatorios {
    /* View responsável pela apresentação do módulo de Relatórios em consola, incluindo
     * menus, seleção de eventos, conteúdo tabular e referência aos PDFs gerados. */
    class RelatorioView {
        public void MostrarMenuRelatorios() {
            Console.WriteLine();
            Console.WriteLine("=== Menu relatorios ===");
            Console.WriteLine("1 - Listagem de inscritos por evento");
            Console.WriteLine("2 - Eventos com ocupacao");
            Console.WriteLine("0 - Regressar ao menu principal");
            Console.Write("Escolha uma opcao: ");
        }

        // Apresenta os eventos em formato tabular, destacando a vermelho estados não ativos.
        public void MostrarListaEventos(List<Evento> listaEventos) {
            Console.WriteLine();
            Console.WriteLine("Lista de eventos:");
            Console.WriteLine(string.Format(
                "{0,-5} {1,-30} {2,-12} {3,-20} {4,10}   {5,-12}",
                "ID",
                "Nome",
                "Data",
                "Local",
                "Capacidade",
                "Estado"));
            Console.WriteLine(new string('-', 99));

            foreach (Evento evento in listaEventos) {
                Console.Write(string.Format(
                    "{0,-5} {1,-30} {2,-12:dd/MM/yyyy} {3,-20} {4,10}   ",
                    evento.Id,
                    evento.Nome,
                    evento.Data,
                    evento.Local,
                    evento.Capacidade));
                
                EscreverEstado(FormatarEstado(evento.Estado), EstadoAtivo(evento.Estado));
                Console.WriteLine();
            }
        }

        public void SolicitarIdEvento() {
            Console.Write("Indique o ID do evento (0 para sair): ");
        }

        // Apresenta o conteúdo do relatório na consola e indica o ficheiro PDF gerado.
        public void ApresentarRelatorioEPdf(DadosRelatorio dadosRelatorio, DocumentoPdf relatorioPdf) {
            Console.WriteLine();
            Console.WriteLine(string.Format("Relatorio: {0}", dadosRelatorio.Titulo));
            MostrarConteudoRelatorio(dadosRelatorio.Conteudo);
            Console.WriteLine(string.Format("PDF gerado: {0}", relatorioPdf.NomeFicheiro));
            Console.WriteLine(string.Format("Caminho: {0}", relatorioPdf.CaminhoFicheiro));
        }

        public void MostrarMensagem(string mensagem) {
            Console.WriteLine(mensagem);
        }

        public void MostrarErroMenu(string mensagem) {
            Console.WriteLine(string.Format("Erro no menu de relatorios: {0}", mensagem));
        }

        public void FinalizarOperacaoMenu() {
            Console.WriteLine();
        }

        // Mostra o conteúdo do relatório linha a linha, preservando o destaque visual dos estados.
        private void MostrarConteudoRelatorio(string conteudo) {
            string textoNormalizado = (conteudo ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            foreach (string linha in textoNormalizado.Split('\n')) {
                MostrarLinhaRelatorio(linha);
            }
        }

        // Trata cada linha do relatório, destacando a vermelho estados não ativos quando presentes.
        private void MostrarLinhaRelatorio(string linha) {
            if (TentarObterSegmentosEstado(linha, out _, out _, out _, out bool estadoVermelho) && estadoVermelho) {
                EscreverLinhaComCor(linha, true);
                return;
            }
            Console.WriteLine(linha);
        }

        private void EscreverLinhaComCor(string linha, bool vermelho) {
            ConsoleColor corOriginal = Console.ForegroundColor;

            if (vermelho) {
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.WriteLine(linha);
            Console.ForegroundColor = corOriginal;
        }

        /* Identifica o segmento correspondente ao estado numa linha textual,
         * para permitir destaque visual sem perder o alinhamento tabular. */
        private bool TentarObterSegmentosEstado(string linha, out string prefixo, out string estado, out string sufixo, out bool estadoVermelho) {
            prefixo = string.Empty;
            estado = string.Empty;
            sufixo = string.Empty;
            estadoVermelho = false;

            if (linha.StartsWith("Estado: ", StringComparison.OrdinalIgnoreCase)) {
                prefixo = "Estado: ";
                estado = linha.Substring(prefixo.Length);
                sufixo = string.Empty;
                estadoVermelho = !EstadoAtivo(estado);
                return true;
            }

            if (!linha.Contains("|")) {
                return false;
            }

            string[] partes = linha.Split('|');
            int indiceCampoEstado = ObterIndiceCampoEstado(partes);
            if (indiceCampoEstado < 0) {
                return false;
            }

            string campoEstado = partes[indiceCampoEstado];
            string estadoLimpo = campoEstado.Trim();

            int inicioCampoEstado = 0;
            for (int i = 0; i < indiceCampoEstado; i++) {
                inicioCampoEstado += partes[i].Length + 1;
            }

            int espacosAntes = campoEstado.Length - campoEstado.TrimStart().Length;
            int inicioEstado = inicioCampoEstado + espacosAntes;

            prefixo = linha.Substring(0, inicioEstado);
            estado = estadoLimpo;
            sufixo = linha.Substring(inicioEstado + estadoLimpo.Length);
            estadoVermelho = !EstadoAtivo(estadoLimpo);

            return true;
        }

        private int ObterIndiceCampoEstado(string[] partes) {
            for (int i = 0; i < partes.Length; i++) {
                if (EstadoConhecido(partes[i].Trim())) {
                    return i;
                }
            }
            return -1;
        }

        private bool EstadoConhecido(string estado) {
            string estadoNormalizado = (estado ?? string.Empty)
                .Trim()
                .Replace('_', ' ')
                .ToLowerInvariant();

            return estadoNormalizado == "ativo" ||
                estadoNormalizado == "cancelado" ||
                estadoNormalizado == "terminado" ||
                estadoNormalizado == "ativa" ||
                estadoNormalizado == "cancelada" ||
                estadoNormalizado == "cancelada por evento" ||
                estadoNormalizado == "terminada";
        }

        // Escreve o estado com cor diferenciada quando não se trata de um estado ativo.
        private void EscreverEstado(string estado, bool ativo) {
            ConsoleColor corOriginal = Console.ForegroundColor;

            if (!ativo) {
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.Write(estado);
            Console.ForegroundColor = corOriginal;
        }

        private bool EstadoAtivo(string estado) {
            string estadoNormalizado = (estado ?? string.Empty).Trim().ToLowerInvariant();
            return estadoNormalizado == "ativo" || estadoNormalizado == "ativa";
        }

        private string FormatarEstado(string estado) {
            string estadoNormalizado = (estado ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(estadoNormalizado)) {
                return string.Empty;
            }

            estadoNormalizado = estadoNormalizado.Replace('_', ' ').ToLowerInvariant();
            return char.ToUpperInvariant(estadoNormalizado[0]) + estadoNormalizado.Substring(1);
        }
    }
}
