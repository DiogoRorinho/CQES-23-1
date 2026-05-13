using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using GestorEventos.Dados;
using GestorEventos.Partilhado;
using Microsoft.Data.Sqlite;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace GestorEventos.Inscricoes
{
    class InscricaoModel
    {
        private readonly string connectionString;
        private readonly string pastaPdfs;

        // Construtor que inicializa as configurações necessárias para o modelo de inscrição
        public InscricaoModel()
        {
            connectionString = ConfiguracaoAplicacao.ObterConnectionString();
            pastaPdfs = ConfiguracaoAplicacao.ObterPastaPdfs();
        }

        // Lista os eventos que possuem vagas disponíveis para inscrição
        public List<Evento> ListarEventosDisponiveis()
        {
            return ObterEventosComDisponibilidade();
        }

        // Obtém a lista de eventos que ainda possuem vagas disponíveis, considerando as inscrições ativas
        public List<Evento> ObterEventosComDisponibilidade()
        {
            List<Evento> eventos = new List<Evento>();

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.CommandText = @"
                SELECT e.id, e.nome, e.local, e.data, e.estado, e.capacidade
                FROM eventos e
                LEFT JOIN inscricoes i
                    ON i.id_evento = e.id
                   AND i.estado = 'ativa'
                WHERE e.estado = 'ativo'
                GROUP BY e.id, e.nome, e.local, e.data, e.estado, e.capacidade
                HAVING e.capacidade > COALESCE(SUM(i.quantidade), 0)
                ORDER BY e.data, e.id;";

            using SqliteDataReader leitor = comando.ExecuteReader();
            while (leitor.Read())
            {
                eventos.Add(MapearEvento(leitor));
            }

            return eventos;
        }

        // Verifica se é possível realizar uma inscrição para um evento específico, considerando a quantidade desejada
        public bool VerificarDisponibilidade(int idEvento, int quantidade)
        {
            return ValidarDisponibilidade(idEvento, quantidade);
        }

        // Valida se existem vagas suficientes para realizar uma inscrição no evento indicado, considerando a quantidade desejada
        public bool ValidarDisponibilidade(int idEvento, int quantidade)
        {
            if (idEvento <= 0 || quantidade <= 0)
            {
                return false;
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            Evento? evento = ObterEvento(ligacao, null, idEvento);

            if (evento == null || !string.Equals(evento.Estado, "ativo", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            int quantidadeInscrita = ObterQuantidadeInscritaAtiva(ligacao, null, idEvento, null);
            return evento.Capacidade - quantidadeInscrita >= quantidade;
        }

        // Cria uma nova inscrição para um evento, validando os dados e a disponibilidade, e gerando um bilhete em PDF
        public ResultadoCriacaoInscricao CriarInscricao(DadosInscricao dados)
        {
            if (!DadosInscricaoValidos(dados))
            {
                return CriarResultado(false, "Dados de inscricao invalidos.", null);
            }

            if (!ValidarDisponibilidade(dados.IdEvento, dados.Quantidade))
            {
                return CriarResultado(false, "Nao existem vagas suficientes para o numero de inscricoes pretendido.", null);
            }

            DocumentoPdf bilhetePdf = ValidarRegistarInscricaoEGerarBilhete(dados);

            return CriarResultado(true, "Inscricao criada com sucesso.", bilhetePdf);
        }

        // Valida os dados de inscrição, registra a inscrição no banco de dados e gera um bilhete em PDF para o participante
        public DocumentoPdf ValidarRegistarInscricaoEGerarBilhete(DadosInscricao dados)
        {
            if (!DadosInscricaoValidos(dados))
            {
                throw new InvalidOperationException("Dados de inscricao invalidos.");
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteTransaction transacao = ligacao.BeginTransaction();

            Evento? evento = ObterEvento(ligacao, transacao, dados.IdEvento);
            if (evento == null || !string.Equals(evento.Estado, "ativo", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Evento nao encontrado ou indisponivel.");
            }

            int quantidadeInscrita = ObterQuantidadeInscritaAtiva(ligacao, transacao, dados.IdEvento, null);
            if (evento.Capacidade - quantidadeInscrita < dados.Quantidade)
            {
                throw new InvalidOperationException("Nao existem vagas suficientes para o numero de inscricoes pretendido.");
            }

            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = @"
                INSERT INTO inscricoes
                    (id_evento, nome_participante, email_participante, idade_participante, quantidade, estado)
                VALUES
                    (@idEvento, @nome, @email, @idade, @quantidade, 'ativa');
                SELECT last_insert_rowid();";

            AdicionarParametrosInscricao(comando, dados);
            int idInscricao = Convert.ToInt32(comando.ExecuteScalar(), CultureInfo.InvariantCulture);
            transacao.Commit();

            Inscricao inscricao = new Inscricao
            {
                Id = idInscricao,
                IdEvento = dados.IdEvento,
                NomeParticipante = dados.NomeParticipante.Trim(),
                EmailParticipante = dados.EmailParticipante.Trim(),
                IdadeParticipante = dados.IdadeParticipante,
                Quantidade = dados.Quantidade,
                Estado = "ativa"
            };

            // Gerar o bilhete em PDF para a inscrição criada
            DocumentoPdf bilhetePdf = CriarDocumentoPdf(
                "Bilhete de inscricao",
                "bilhete-inscricao-" + idInscricao + ".pdf");

            GerarFicheiroPdf(bilhetePdf, ConstruirConteudoBilhete(inscricao, evento));
            return bilhetePdf;
        }

        // Lista todas as inscrições registradas no sistema, independentemente do estado ou evento associado
        public List<Inscricao> ListarInscricoes()
        {
            return ObterListaInscricoes();
        }

        // Obtém a lista completa de inscrições, incluindo detalhes como participante, evento e estado da inscrição
        public List<Inscricao> ObterListaInscricoes()
        {
            List<Inscricao> inscricoes = new List<Inscricao>();

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.CommandText = @"
                SELECT id, id_evento, nome_participante, email_participante,
                       idade_participante, quantidade, estado
                FROM inscricoes
                ORDER BY id;";

            using SqliteDataReader leitor = comando.ExecuteReader();
            while (leitor.Read())
            {
                inscricoes.Add(MapearInscricao(leitor));
            }

            return inscricoes;
        }

        // Obtém os detalhes de uma inscrição específica com base no seu ID, retornando null se a inscrição não for encontrada
        public Inscricao? ObterInscricao(int idInscricao)
        {
            return ObterDadosInscricao(idInscricao);
        }

        // Obtém os detalhes completos de uma inscrição específica, incluindo informações do participante e do evento associado, com base no ID da inscrição
        public Inscricao? ObterDadosInscricao(int idInscricao)
        {
            if (idInscricao <= 0)
            {
                return null;
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.CommandText = @"
                SELECT id, id_evento, nome_participante, email_participante,
                       idade_participante, quantidade, estado
                FROM inscricoes
                WHERE id = @idInscricao;";
            comando.Parameters.AddWithValue("@idInscricao", idInscricao);

            using SqliteDataReader leitor = comando.ExecuteReader();
            if (leitor.Read())
            {
                return MapearInscricao(leitor);
            }

            return null;
        }

        // Verifica se é possível realizar uma alteração em uma inscrição existente, considerando o evento de destino e a quantidade desejada
        public bool ValidarAlteracaoInscricao(int idInscricao, DadosInscricao dados)
        {
            return VerificarSeAlteracaoEhPossivel(idInscricao, dados);
        }

        // Verifica se uma alteração em uma inscrição é possível, validando os dados fornecidos e verificando a disponibilidade no evento de destino, considerando a quantidade desejada
        public bool VerificarSeAlteracaoEhPossivel(int idInscricao, DadosInscricao dados)
        {
            if (idInscricao <= 0 || !DadosInscricaoValidos(dados))
            {
                return false;
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            Inscricao? inscricaoAtual = ObterInscricao(ligacao, null, idInscricao);
            Evento? eventoDestino = ObterEvento(ligacao, null, dados.IdEvento);

            if (inscricaoAtual == null ||
                !string.Equals(inscricaoAtual.Estado, "ativa", StringComparison.OrdinalIgnoreCase) ||
                eventoDestino == null ||
                !string.Equals(eventoDestino.Estado, "ativo", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            int quantidadeInscrita = ObterQuantidadeInscritaAtiva(ligacao, null, dados.IdEvento, idInscricao);
            return eventoDestino.Capacidade - quantidadeInscrita >= dados.Quantidade;
        }

        // Realiza a alteração de uma inscrição existente, atualizando os dados no banco de dados e gerando um novo bilhete em PDF para o participante
        public DocumentoPdf AlterarInscricao(int idInscricao, DadosInscricao dados)
        {
            return ValidarAtualizarInscricaoEGerarBilhete(idInscricao, dados);
        }

        // Valida os dados para atualização de uma inscrição, realiza a alteração no banco de dados e gera um novo bilhete em PDF refletindo as mudanças realizadas
        public DocumentoPdf ValidarAtualizarInscricaoEGerarBilhete(int idInscricao, DadosInscricao dados)
        {
            if (!VerificarSeAlteracaoEhPossivel(idInscricao, dados))
            {
                throw new InvalidOperationException("Nao foi possivel alterar a inscricao com os dados indicados.");
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteTransaction transacao = ligacao.BeginTransaction();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = @"
                UPDATE inscricoes
                SET id_evento = @idEvento,
                    nome_participante = @nome,
                    email_participante = @email,
                    idade_participante = @idade,
                    quantidade = @quantidade,
                    atualizado_em = CURRENT_TIMESTAMP
                WHERE id = @idInscricao
                  AND estado = 'ativa';";

            comando.Parameters.AddWithValue("@idInscricao", idInscricao);
            AdicionarParametrosInscricao(comando, dados);
            int linhasAfetadas = comando.ExecuteNonQuery();
            transacao.Commit();

            if (linhasAfetadas == 0)
            {
                throw new InvalidOperationException("Inscricao nao encontrada ou indisponivel para alteracao.");
            }

            // Gerar o bilhete atualizado em PDF para a inscrição alterada
            Inscricao inscricaoAtualizada = ObterInscricao(idInscricao) ?? new Inscricao();
            Evento? evento = ObterEvento(dados.IdEvento);

            DocumentoPdf bilhetePdf = CriarDocumentoPdf(
                "Bilhete atualizado",
                "bilhete-atualizado-" + idInscricao + ".pdf");

            GerarFicheiroPdf(bilhetePdf, ConstruirConteudoBilhete(inscricaoAtualizada, evento));
            return bilhetePdf;
        }

        // Cancela uma inscrição existente, atualizando seu estado para "cancelada" no banco de dados, desde que a inscrição esteja atualmente ativa
        public void CancelarInscricao(int idInscricao)
        {
            AtualizarEstadoInscricao(idInscricao, "cancelada");
        }

        // Atualiza o estado de uma inscrição para um novo valor, permitindo a transição para estados como "cancelada" ou "cancelada_por_evento", e registrando a data de cancelamento quando aplicável
        public void AtualizarEstadoInscricao(int idInscricao, string estado)
        {
            if (idInscricao <= 0 || string.IsNullOrWhiteSpace(estado))
            {
                return;
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteTransaction transacao = ligacao.BeginTransaction();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.Transaction = transacao;
            comando.CommandText = @"
                UPDATE inscricoes
                SET estado = @estado,
                    atualizado_em = CURRENT_TIMESTAMP,
                    cancelado_em = CASE
                        WHEN @estado IN ('cancelada', 'cancelada_por_evento') AND cancelado_em IS NULL THEN CURRENT_TIMESTAMP
                        ELSE cancelado_em
                    END
                WHERE id = @idInscricao
                  AND estado = 'ativa';";
            comando.Parameters.AddWithValue("@idInscricao", idInscricao);
            comando.Parameters.AddWithValue("@estado", estado.Trim());
            comando.ExecuteNonQuery();
            transacao.Commit();
        }

        // Obtém a lista de inscrições ativas que estão associadas a um evento específico, permitindo identificar os participantes afetados por alterações ou cancelamentos do evento
        public List<Inscricao> ObterInscritosAfetados(int idEvento)
        {
            List<Inscricao> resultados = new List<Inscricao>();

            if (idEvento <= 0)
            {
                return resultados;
            }

            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            using SqliteCommand comando = ligacao.CreateCommand();
            comando.CommandText = @"
                SELECT id, id_evento, nome_participante, email_participante,
                       idade_participante, quantidade, estado
                FROM inscricoes
                WHERE id_evento = @idEvento
                  AND estado = 'ativa'
                ORDER BY id;";
            comando.Parameters.AddWithValue("@idEvento", idEvento);

            using SqliteDataReader leitor = comando.ExecuteReader();
            while (leitor.Read())
            {
                resultados.Add(MapearInscricao(leitor));
            }

            return resultados;
        }

        // Cancela ou invalida uma inscrição específica, atualizando seu estado para "cancelada_por_evento" no banco de dados, indicando que a inscrição foi afetada por uma alteração ou cancelamento do evento associado
        public void CancelarOuInvalidarInscricao(int idInscricao)
        {
            AtualizarEstadoInscricao(idInscricao, "cancelada_por_evento");
        }

        // Gera um comprovativo de cancelamento para uma inscrição específica, criando um documento PDF que detalha as informações do cancelamento e do evento associado
        public DocumentoPdf GerarComprovativoCancelamento(int idInscricao)
        {
            Inscricao? inscricao = ObterInscricao(idInscricao);
            Evento? evento = inscricao == null ? null : ObterEvento(inscricao.IdEvento);

            DocumentoPdf comprovativo = CriarDocumentoPdf(
                "Comprovativo de cancelamento",
                "comprovativo-cancelamento-" + idInscricao + ".pdf");

            GerarFicheiroPdf(comprovativo, ConstruirConteudoCancelamento(inscricao, evento));
            return comprovativo;
        }

        // Exibe o comprovativo de cancelamento para uma inscrição específica, mostrando os detalhes do cancelamento e do evento associado
        public string ObterConnectionString()
        {
            return connectionString;
        }

        // Exibe a pasta onde os arquivos PDF gerados estão armazenados, permitindo que o usuário saiba onde encontrar os bilhetes e comprovativos gerados
        public string ObterPastaPdfs()
        {
            return pastaPdfs;
        }

        // Cria um resultado de criação de inscrição, encapsulando o sucesso da operação, uma mensagem descritiva e um possível documento PDF gerado como bilhete
        private ResultadoCriacaoInscricao CriarResultado(bool sucesso, string mensagem, DocumentoPdf? bilhetePdf)
        {
            return new ResultadoCriacaoInscricao
            {
                Sucesso = sucesso,
                Mensagem = mensagem,
                BilhetePdf = bilhetePdf
            };
        }

        // Valida os dados fornecidos para a criação ou alteração de uma inscrição, garantindo que todos os campos necessários estejam preenchidos corretamente e que os valores sejam válidos
        private bool DadosInscricaoValidos(DadosInscricao dados)
        {
            return dados != null &&
                   dados.IdEvento > 0 &&
                   !string.IsNullOrWhiteSpace(dados.NomeParticipante) &&
                   !string.IsNullOrWhiteSpace(dados.EmailParticipante) &&
                   dados.IdadeParticipante > 0 &&
                   dados.Quantidade > 0;
        }

        // Adiciona os parâmetros necessários para a criação ou alteração de uma inscrição em um comando SQL, garantindo que os valores sejam corretamente associados aos campos correspondentes na consulta
        private void AdicionarParametrosInscricao(SqliteCommand comando, DadosInscricao dados)
        {
            comando.Parameters.AddWithValue("@idEvento", dados.IdEvento);
            comando.Parameters.AddWithValue("@nome", dados.NomeParticipante.Trim());
            comando.Parameters.AddWithValue("@email", dados.EmailParticipante.Trim());
            comando.Parameters.AddWithValue("@idade", dados.IdadeParticipante);
            comando.Parameters.AddWithValue("@quantidade", dados.Quantidade);
        }

        // Obtém os detalhes de um evento específico com base no seu ID, retornando null se o evento não for encontrado ou estiver indisponível
        private Evento? ObterEvento(int idEvento)
        {
            using SqliteConnection ligacao = BaseDados.CriarLigacaoAberta();
            return ObterEvento(ligacao, null, idEvento);
        }

        // Obtém os detalhes de um evento específico com base no seu ID, utilizando uma conexão e transação SQL fornecidas, retornando null se o evento não for encontrado ou estiver indisponível
        private Evento? ObterEvento(SqliteConnection ligacao, SqliteTransaction? transacao, int idEvento)
        {
            if (idEvento <= 0)
            {
                return null;
            }

            using SqliteCommand comando = ligacao.CreateCommand();
            if (transacao != null)
            {
                comando.Transaction = transacao;
            }

            comando.CommandText = @"
                SELECT id, nome, local, data, estado, capacidade
                FROM eventos
                WHERE id = @idEvento;";
            comando.Parameters.AddWithValue("@idEvento", idEvento);

            using SqliteDataReader leitor = comando.ExecuteReader();
            if (leitor.Read())
            {
                return MapearEvento(leitor);
            }

            return null;
        }

        // Obtém os detalhes de uma inscrição específica com base no seu ID, utilizando
        private Inscricao? ObterInscricao(SqliteConnection ligacao, SqliteTransaction? transacao, int idInscricao)
        {
            if (idInscricao <= 0)
            {
                return null;
            }

            using SqliteCommand comando = ligacao.CreateCommand();
            if (transacao != null)
            {
                comando.Transaction = transacao;
            }

            comando.CommandText = @"
                SELECT id, id_evento, nome_participante, email_participante,
                       idade_participante, quantidade, estado
                FROM inscricoes
                WHERE id = @idInscricao;";
            comando.Parameters.AddWithValue("@idInscricao", idInscricao);

            using SqliteDataReader leitor = comando.ExecuteReader();
            if (leitor.Read())
            {
                return MapearInscricao(leitor);
            }

            return null;
        }

        // Obtém a quantidade total de inscrições ativas para um evento específico, permitindo verificar a disponibilidade de vagas considerando as inscrições existentes, e opcionalmente excluindo uma inscrição específica da contagem
        private int ObterQuantidadeInscritaAtiva(
            SqliteConnection ligacao,
            SqliteTransaction? transacao,
            int idEvento,
            int? idInscricaoAExcluir)
        {
            using SqliteCommand comando = ligacao.CreateCommand();
            if (transacao != null)
            {
                comando.Transaction = transacao;
            }

            comando.CommandText = @"
                SELECT COALESCE(SUM(quantidade), 0)
                FROM inscricoes
                WHERE id_evento = @idEvento
                  AND estado = 'ativa'
                  AND (@idExcluir IS NULL OR id <> @idExcluir);";
            comando.Parameters.AddWithValue("@idEvento", idEvento);
            comando.Parameters.AddWithValue("@idExcluir", idInscricaoAExcluir.HasValue ? (object)idInscricaoAExcluir.Value : DBNull.Value);

            object? resultado = comando.ExecuteScalar();
            return Convert.ToInt32(resultado, CultureInfo.InvariantCulture);
        }

        //
        private Evento MapearEvento(SqliteDataReader leitor)
        {
            return new Evento
            {
                Id = LerInteiro(leitor, "id"),
                Nome = LerTexto(leitor, "nome"),
                Local = LerTexto(leitor, "local"),
                Data = LerData(leitor, "data"),
                Estado = LerTexto(leitor, "estado"),
                Capacidade = LerInteiro(leitor, "capacidade")
            };
        }

        // Mapeia os dados de uma inscrição a partir de um leitor de dados SQL, criando um objeto Inscricao com as informações correspondentes extraídas do banco de dados
        private Inscricao MapearInscricao(SqliteDataReader leitor)
        {
            return new Inscricao
            {
                Id = LerInteiro(leitor, "id"),
                IdEvento = LerInteiro(leitor, "id_evento"),
                NomeParticipante = LerTexto(leitor, "nome_participante"),
                EmailParticipante = LerTexto(leitor, "email_participante"),
                IdadeParticipante = LerInteiro(leitor, "idade_participante"),
                Quantidade = LerInteiro(leitor, "quantidade"),
                Estado = LerTexto(leitor, "estado")
            };
        }

        // Lê um valor inteiro de uma coluna específica em um leitor de dados SQL, tratando valores nulos e garantindo que o resultado seja um inteiro válido
        private int LerInteiro(SqliteDataReader leitor, string coluna)
        {
            int ordinal = leitor.GetOrdinal(coluna);

            if (leitor.IsDBNull(ordinal))
            {
                return 0;
            }

            return Convert.ToInt32(leitor.GetValue(ordinal), CultureInfo.InvariantCulture);
        }

        // Lê um valor de texto de uma coluna específica em um leitor de dados SQL, tratando valores nulos e garantindo que o resultado seja uma string válida
        private string LerTexto(SqliteDataReader leitor, string coluna)
        {
            int ordinal = leitor.GetOrdinal(coluna);

            if (leitor.IsDBNull(ordinal))
            {
                return string.Empty;
            }

            return Convert.ToString(leitor.GetValue(ordinal), CultureInfo.InvariantCulture) ?? string.Empty;
        }
        
        // Lê um valor de data de uma coluna específica em um leitor de dados SQL, tratando valores nulos e garantindo que o resultado seja uma data válida
        private DateTime LerData(SqliteDataReader leitor, string coluna)
        {
            int ordinal = leitor.GetOrdinal(coluna);

            if (leitor.IsDBNull(ordinal))
            {
                return DateTime.MinValue;
            }

            object valor = leitor.GetValue(ordinal);

            if (valor is DateTime dataDireta)
            {
                return dataDireta;
            }

            string texto = Convert.ToString(valor, CultureInfo.InvariantCulture) ?? string.Empty;

            if (DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime data) ||
                DateTime.TryParse(texto, CultureInfo.CurrentCulture, DateTimeStyles.None, out data))
            {
                return data;
            }

            return DateTime.MinValue;
        }

        // Cria um objeto DocumentoPdf com as informações fornecidas, incluindo o título, nome do arquivo e caminho completo para onde o arquivo PDF será salvo
        private DocumentoPdf CriarDocumentoPdf(string titulo, string nomeFicheiro)
        {
            return new DocumentoPdf
            {
                Titulo = titulo,
                NomeFicheiro = nomeFicheiro,
                CaminhoFicheiro = ConfiguracaoAplicacao.CombinarCaminhoPdf(nomeFicheiro)
            };
        }

        // Constrói o conteúdo textual para um bilhete de inscrição, incluindo detalhes do participante, evento e estado da inscrição, formatando as informações de maneira clara e organizada para exibição no PDF
        private string ConstruirConteudoBilhete(Inscricao inscricao, Evento? evento)
        {
            StringBuilder conteudo = new StringBuilder();
            conteudo.AppendLine("Bilhete de inscricao");
            conteudo.AppendLine(string.Format("Inscricao: {0}", inscricao.Id));
            conteudo.AppendLine(string.Format("Participante: {0}", inscricao.NomeParticipante));
            conteudo.AppendLine(string.Format("Email: {0}", inscricao.EmailParticipante));
            conteudo.AppendLine(string.Format("Idade: {0}", inscricao.IdadeParticipante));
            conteudo.AppendLine(string.Format("Quantidade: {0}", inscricao.Quantidade));
            conteudo.AppendLine(string.Format("Estado: {0}", inscricao.Estado));

            if (evento != null)
            {
                conteudo.AppendLine(string.Format("Evento: {0}", evento.Nome));
                conteudo.AppendLine(string.Format("Local: {0}", evento.Local));
                conteudo.AppendLine(string.Format("Data: {0:dd/MM/yyyy}", evento.Data));
            }
            else
            {
                conteudo.AppendLine(string.Format("Evento: {0}", inscricao.IdEvento));
            }

            return conteudo.ToString();
        }

        // Constrói o conteúdo textual para um comprovativo de cancelamento, incluindo detalhes do cancelamento, participante, evento e estado da inscrição, formatando as informações de maneira clara e organizada para exibição no PDF
        private string ConstruirConteudoCancelamento(Inscricao? inscricao, Evento? evento)
        {
            StringBuilder conteudo = new StringBuilder();
            conteudo.AppendLine("Comprovativo de cancelamento");
            conteudo.AppendLine(string.Format("Data do cancelamento: {0:dd/MM/yyyy HH:mm}", DateTime.Now));

            if (inscricao == null)
            {
                conteudo.AppendLine("Inscricao nao encontrada.");
                return conteudo.ToString();
            }

            conteudo.AppendLine(string.Format("Inscricao: {0}", inscricao.Id));
            conteudo.AppendLine(string.Format("Participante: {0}", inscricao.NomeParticipante));
            conteudo.AppendLine(string.Format("Email: {0}", inscricao.EmailParticipante));
            conteudo.AppendLine(string.Format("Quantidade: {0}", inscricao.Quantidade));
            conteudo.AppendLine(string.Format("Estado atual: {0}", inscricao.Estado));

            if (evento != null)
            {
                conteudo.AppendLine(string.Format("Evento: {0}", evento.Nome));
                conteudo.AppendLine(string.Format("Local: {0}", evento.Local));
                conteudo.AppendLine(string.Format("Data: {0:dd/MM/yyyy}", evento.Data));
            }
            else
            {
                conteudo.AppendLine(string.Format("Evento: {0}", inscricao.IdEvento));
            }

            return conteudo.ToString();
        }

        // Gera um arquivo PDF com o conteúdo fornecido, utilizando a biblioteca PdfSharp para criar o documento, formatar o texto e salvar o arquivo no caminho especificado
        private void GerarFicheiroPdf(DocumentoPdf documentoPdf, string conteudo)
        {
            string? pastaDestino = Path.GetDirectoryName(documentoPdf.CaminhoFicheiro);

            if (!string.IsNullOrWhiteSpace(pastaDestino))
            {
                Directory.CreateDirectory(pastaDestino);
            }

            using PdfDocument documento = new PdfDocument();
            documento.Info.Title = documentoPdf.Titulo;

            PdfPage pagina = documento.AddPage();
            pagina.Size = PageSize.A4;

            XGraphics grafico = XGraphics.FromPdfPage(pagina);
            XFont fonteTitulo = new XFont("Arial", 16, XFontStyleEx.Bold);
            XFont fonteCorpo = new XFont("Arial", 10, XFontStyleEx.Regular);

            const double margem = 40;
            const double alturaLinha = 14;
            double y = margem;

            grafico.DrawString(
                documentoPdf.Titulo,
                fonteTitulo,
                XBrushes.Black,
                new XRect(margem, y, pagina.Width.Point - margem * 2, 24),
                XStringFormats.TopLeft);

            y += 34;

            foreach (string linha in SepararLinhasPdf(conteudo, grafico, fonteCorpo, pagina.Width.Point - margem * 2))
            {
                if (y + alturaLinha > pagina.Height.Point - margem)
                {
                    grafico.Dispose();
                    pagina = documento.AddPage();
                    pagina.Size = PageSize.A4;
                    grafico = XGraphics.FromPdfPage(pagina);
                    y = margem;
                }

                grafico.DrawString(
                    linha,
                    fonteCorpo,
                    XBrushes.Black,
                    new XRect(margem, y, pagina.Width.Point - margem * 2, alturaLinha),
                    XStringFormats.TopLeft);

                y += alturaLinha;
            }

            grafico.Dispose();
            documento.Save(documentoPdf.CaminhoFicheiro);
        }

        // Separa o texto em linhas adequadas para exibição em um PDF, considerando a largura máxima disponível e utilizando a medição de texto da biblioteca gráfica para garantir que as linhas não ultrapassem os limites do layout
        private List<string> SepararLinhasPdf(string texto, XGraphics grafico, XFont fonte, double larguraMaxima)
        {
            List<string> linhas = new List<string>();
            string textoNormalizado = (texto ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            foreach (string linhaOriginal in textoNormalizado.Split('\n'))
            {
                linhas.AddRange(QuebrarLinhaPdf(linhaOriginal, grafico, fonte, larguraMaxima));
            }

            return linhas;
        }

        // Quebra uma linha de texto em múltiplas linhas, garantindo que cada linha resultante não ultrapasse a largura máxima especificada, utilizando a medição de texto da biblioteca gráfica para determinar o ponto de quebra adequado
        private List<string> QuebrarLinhaPdf(string linhaOriginal, XGraphics grafico, XFont fonte, double larguraMaxima)
        {
            List<string> linhas = new List<string>();

            if (string.IsNullOrWhiteSpace(linhaOriginal))
            {
                linhas.Add(string.Empty);
                return linhas;
            }

            string linhaAtual = string.Empty;

            foreach (string palavra in linhaOriginal.Split(' '))
            {
                string candidata = string.IsNullOrEmpty(linhaAtual)
                    ? palavra
                    : linhaAtual + " " + palavra;

                if (grafico.MeasureString(candidata, fonte).Width <= larguraMaxima)
                {
                    linhaAtual = candidata;
                    continue;
                }

                if (!string.IsNullOrEmpty(linhaAtual))
                {
                    linhas.Add(linhaAtual);
                }

                linhaAtual = palavra;
            }

            if (!string.IsNullOrEmpty(linhaAtual))
            {
                linhas.Add(linhaAtual);
            }

            return linhas;
        }
    }
}
