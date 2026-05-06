PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS eventos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nome TEXT NOT NULL,
    local TEXT NOT NULL,
    data TEXT NOT NULL,
    estado TEXT NOT NULL DEFAULT 'ativo' CHECK (estado IN ('ativo', 'cancelado')),
    capacidade INTEGER NOT NULL CHECK (capacidade > 0),
    criado_em TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    atualizado_em TEXT,
    cancelado_em TEXT
);

CREATE TABLE IF NOT EXISTS inscricoes (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    id_evento INTEGER NOT NULL,
    nome_participante TEXT NOT NULL,
    email_participante TEXT NOT NULL,
    idade_participante INTEGER NOT NULL CHECK (idade_participante > 0),
    quantidade INTEGER NOT NULL DEFAULT 1 CHECK (quantidade > 0),
    estado TEXT NOT NULL DEFAULT 'ativa' CHECK (estado IN ('ativa', 'cancelada', 'cancelada_por_evento')),
    criado_em TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    atualizado_em TEXT,
    cancelado_em TEXT,
    FOREIGN KEY (id_evento) REFERENCES eventos(id) ON UPDATE CASCADE ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS idx_eventos_estado ON eventos(estado);
CREATE INDEX IF NOT EXISTS idx_eventos_data ON eventos(data);
CREATE INDEX IF NOT EXISTS idx_inscricoes_evento ON inscricoes(id_evento);
CREATE INDEX IF NOT EXISTS idx_inscricoes_estado ON inscricoes(estado);
