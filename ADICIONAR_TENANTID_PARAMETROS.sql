-- Script para adicionar TenantId na tabela Parametros
-- Execute este script no SQL Server Management Studio

USE TimeTrackerSaaS;
GO

-- 1. Adicionar coluna TenantId (permitindo NULL temporariamente)
ALTER TABLE Parametros
ADD TenantId INT NULL;
GO

-- 2. Atualizar registros existentes com TenantId = 1 (tenant padrão)
UPDATE Parametros
SET TenantId = 1
WHERE TenantId IS NULL;
GO

-- 3. Tornar a coluna NOT NULL
ALTER TABLE Parametros
ALTER COLUMN TenantId INT NOT NULL;
GO

-- 4. Adicionar Foreign Key para Tenants
ALTER TABLE Parametros
ADD CONSTRAINT FK_Parametros_Tenants_TenantId 
FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE CASCADE;
GO

-- 5. Criar índice para melhor performance
CREATE INDEX IX_Parametros_TenantId ON Parametros(TenantId);
GO

PRINT 'TenantId adicionado com sucesso na tabela Parametros!';
GO
