-- Script para dropar e recriar o banco de dados limpo
-- Execute este script no SQL Server Management Studio

USE master;
GO

-- Fecha todas as conexões ativas com o banco
ALTER DATABASE TimeTrackerSaaS SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- Dropa o banco de dados
DROP DATABASE TimeTrackerSaaS;
GO

-- Recria o banco de dados
CREATE DATABASE TimeTrackerSaaS;
GO

-- Mensagem de sucesso
PRINT 'Banco de dados TimeTrackerSaaS recriado com sucesso!';
PRINT 'Agora execute a aplicação para que o Entity Framework crie as tabelas e o SeedingService popule os dados iniciais.';
GO
