-- Migration: AddTenantIdToFinancialTables
-- Adiciona TenantId às tabelas ValorCliente, Mensalista e PercentualAreas
-- Database: TimeTrackerSaaS

USE TimeTrackerSaaS;
GO

PRINT 'Iniciando migration para tabelas financeiras...';
GO

-- ===== ValorCliente =====
PRINT 'Processando tabela ValorCliente...';

ALTER TABLE [dbo].[ValorCliente]
ADD [TenantId] INT NULL;
GO

UPDATE [dbo].[ValorCliente]
SET [TenantId] = 1;
GO

ALTER TABLE [dbo].[ValorCliente]
ALTER COLUMN [TenantId] INT NOT NULL;
GO

CREATE INDEX [IX_ValorCliente_TenantId] ON [dbo].[ValorCliente] ([TenantId]);
GO

ALTER TABLE [dbo].[ValorCliente]
ADD CONSTRAINT [FK_ValorCliente_Tenants_TenantId] 
FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]);
GO

PRINT 'ValorCliente concluída!';
GO

-- ===== Mensalista =====
PRINT 'Processando tabela Mensalista...';

ALTER TABLE [dbo].[Mensalista]
ADD [TenantId] INT NULL;
GO

UPDATE [dbo].[Mensalista]
SET [TenantId] = 1;
GO

ALTER TABLE [dbo].[Mensalista]
ALTER COLUMN [TenantId] INT NOT NULL;
GO

CREATE INDEX [IX_Mensalista_TenantId] ON [dbo].[Mensalista] ([TenantId]);
GO

ALTER TABLE [dbo].[Mensalista]
ADD CONSTRAINT [FK_Mensalista_Tenants_TenantId] 
FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]);
GO

PRINT 'Mensalista concluída!';
GO

-- ===== PercentualAreas =====
PRINT 'Processando tabela PercentualAreas...';

ALTER TABLE [dbo].[PercentualAreas]
ADD [TenantId] INT NULL;
GO

UPDATE [dbo].[PercentualAreas]
SET [TenantId] = 1;
GO

ALTER TABLE [dbo].[PercentualAreas]
ALTER COLUMN [TenantId] INT NOT NULL;
GO

CREATE INDEX [IX_PercentualAreas_TenantId] ON [dbo].[PercentualAreas] ([TenantId]);
GO

ALTER TABLE [dbo].[PercentualAreas]
ADD CONSTRAINT [FK_PercentualAreas_Tenants_TenantId] 
FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]);
GO

PRINT 'PercentualAreas concluída!';
GO

PRINT 'Migration concluída com sucesso!';
GO

-- Verificar resultados
SELECT 'ValorCliente' AS Tabela, COUNT(*) AS Registros FROM [dbo].[ValorCliente]
UNION ALL
SELECT 'Mensalista', COUNT(*) FROM [dbo].[Mensalista]
UNION ALL
SELECT 'PercentualAreas', COUNT(*) FROM [dbo].[PercentualAreas];
GO
