-- Cria o banco de dados do FaturamentoService
-- O estoque_db já é criado automaticamente pelo POSTGRES_DB no docker-compose
SELECT 'CREATE DATABASE faturamento_db'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'faturamento_db')\gexec
