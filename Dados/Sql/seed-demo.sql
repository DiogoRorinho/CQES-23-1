PRAGMA foreign_keys = ON;

INSERT INTO eventos (id, nome, local, data, estado, capacidade)
VALUES
    (1, 'Workshop de Arquitetura', 'Lisboa', '2026-05-15', 'ativo', 30),
    (2, 'Seminario MVC', 'Porto', '2026-06-10', 'ativo', 50),
    (3, 'Laboratorio de SQLite', 'Coimbra', '2026-07-02', 'ativo', 25);

INSERT INTO inscricoes (id, id_evento, nome_participante, email_participante, idade_participante, quantidade, estado)
VALUES
    (1, 1, 'Ana Martins', 'ana@exemplo.pt', 28, 1, 'ativa'),
    (2, 1, 'Bruno Silva', 'bruno@exemplo.pt', 34, 2, 'ativa'),
    (3, 2, 'Carla Sousa', 'carla@exemplo.pt', 41, 1, 'ativa'),
    (4, 2, 'Diogo Pereira', 'diogo@exemplo.pt', 22, 1, 'cancelada');
