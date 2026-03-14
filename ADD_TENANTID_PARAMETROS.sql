-- Migration: AddTenantIdToParametros
-- Execute este script no SQL Server Management Studio (SSMS)
-- Database: TimeTrackerSaaS

USE TimeTrackerSaaS;
GO

-- 1. Adicionar coluna TenantId (permitindo NULL temporariamente)
ALTER TABLE [dbo].[Parametros]
ADD [TenantId] INT NULL;
GO

-- 2. Atualizar registros existentes com TenantId = 1 (tenant padrão)
UPDATE [dbo].[Parametros]
SET [TenantId] = 1;
GO

-- 3. Tornar a coluna NOT NULL
ALTER TABLE [dbo].[Parametros]
ALTER COLUMN [TenantId] INT NOT NULL;
GO

-- 4. Criar índice
CREATE INDEX [IX_Parametros_TenantId] ON [dbo].[Parametros] ([TenantId]);
GO

-- 5. Adicionar Foreign Key
ALTER TABLE [dbo].[Parametros]
ADD CONSTRAINT [FK_Parametros_Tenants_TenantId] 
FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]) ON DELETE CASCADE;
GO

PRINT 'Migration concluída com sucesso!';
GO

-- Verificar o resultado
SELECT Id, Width, Height, TenantId FROM [dbo].[Parametros];
GO
