-- Migration: ConvertRecordTypeToActivityType
-- Converte enum RecordType para tabela ActivityTypes dinâmica
-- Database: TimeTrackerSaaS

USE TimeTrackerSaaS;
GO

PRINT 'Iniciando conversão de RecordType para ActivityTypes...';
GO

-- 1. Criar tabela ActivityTypes
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ActivityTypes')
BEGIN
    PRINT 'Criando tabela ActivityTypes...';
    
    CREATE TABLE [dbo].[ActivityTypes] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [Color] NVARCHAR(7) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [DisplayOrder] INT NOT NULL,
        [TenantId] INT NOT NULL,
        CONSTRAINT [PK_ActivityTypes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ActivityTypes_Tenants_TenantId] FOREIGN KEY ([TenantId]) 
            REFERENCES [dbo].[Tenants] ([Id])
    );
    
    CREATE INDEX [IX_ActivityTypes_TenantId] ON [dbo].[ActivityTypes] ([TenantId]);
    
    PRINT 'Tabela ActivityTypes criada!';
END
ELSE
BEGIN
    PRINT 'Tabela ActivityTypes já existe.';
END
GO

-- 2. Inserir tipos de atividade padrão para cada tenant existente
PRINT 'Inserindo tipos de atividade padrão para cada tenant...';

INSERT INTO [dbo].[ActivityTypes] ([Name], [Description], [Color], [IsActive], [DisplayOrder], [TenantId])
SELECT 'Consultivo', 'Atividades de consultoria e orientação', '#4A90E2', 1, 1, [Id] FROM [dbo].[Tenants]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[ActivityTypes] WHERE [TenantId] = [Tenants].[Id] AND [Name] = 'Consultivo');

INSERT INTO [dbo].[ActivityTypes] ([Name], [Description], [Color], [IsActive], [DisplayOrder], [TenantId])
SELECT 'Contencioso', 'Atividades relacionadas a processos judiciais', '#E24A4A', 1, 2, [Id] FROM [dbo].[Tenants]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[ActivityTypes] WHERE [TenantId] = [Tenants].[Id] AND [Name] = 'Contencioso');

INSERT INTO [dbo].[ActivityTypes] ([Name], [Description], [Color], [IsActive], [DisplayOrder], [TenantId])
SELECT 'Proposta Específica', 'Elaboração de propostas e orçamentos', '#50C878', 1, 3, [Id] FROM [dbo].[Tenants]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[ActivityTypes] WHERE [TenantId] = [Tenants].[Id] AND [Name] = 'Proposta Específica');

INSERT INTO [dbo].[ActivityTypes] ([Name], [Description], [Color], [IsActive], [DisplayOrder], [TenantId])
SELECT 'Deslocamento', 'Tempo de deslocamento e viagens', '#FFA500', 1, 4, [Id] FROM [dbo].[Tenants]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[ActivityTypes] WHERE [TenantId] = [Tenants].[Id] AND [Name] = 'Deslocamento');

PRINT 'Tipos de atividade padrão inseridos!';
GO

-- 3. Adicionar coluna ActivityTypeId em ProcessRecord (permitindo NULL temporariamente)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProcessRecord]') AND name = 'ActivityTypeId')
BEGIN
    PRINT 'Adicionando coluna ActivityTypeId...';
    ALTER TABLE [dbo].[ProcessRecord] ADD [ActivityTypeId] INT NULL;
    PRINT 'Coluna ActivityTypeId adicionada!';
END
ELSE
BEGIN
    PRINT 'Coluna ActivityTypeId já existe.';
END
GO

-- 4. Migrar dados existentes de RecordType para ActivityTypeId
PRINT 'Migrando dados de RecordType para ActivityTypeId...';

-- RecordType 0 (Consultivo) → ActivityType "Consultivo"
UPDATE pr
SET pr.ActivityTypeId = at.Id
FROM [dbo].[ProcessRecord] pr
INNER JOIN [dbo].[Attorney] a ON pr.AttorneyId = a.Id
INNER JOIN [dbo].[ActivityTypes] at ON at.TenantId = a.TenantId
WHERE pr.RecordType = 0 AND at.Name = 'Consultivo' AND pr.ActivityTypeId IS NULL;

PRINT 'Migrados registros Consultivo: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- RecordType 1 (Contencioso) → ActivityType "Contencioso"
UPDATE pr
SET pr.ActivityTypeId = at.Id
FROM [dbo].[ProcessRecord] pr
INNER JOIN [dbo].[Attorney] a ON pr.AttorneyId = a.Id
INNER JOIN [dbo].[ActivityTypes] at ON at.TenantId = a.TenantId
WHERE pr.RecordType = 1 AND at.Name = 'Contencioso' AND pr.ActivityTypeId IS NULL;

PRINT 'Migrados registros Contencioso: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- RecordType 2 (Proposta Específica) → ActivityType "Proposta Específica"
UPDATE pr
SET pr.ActivityTypeId = at.Id
FROM [dbo].[ProcessRecord] pr
INNER JOIN [dbo].[Attorney] a ON pr.AttorneyId = a.Id
INNER JOIN [dbo].[ActivityTypes] at ON at.TenantId = a.TenantId
WHERE pr.RecordType = 2 AND at.Name = 'Proposta Específica' AND pr.ActivityTypeId IS NULL;

PRINT 'Migrados registros Proposta Específica: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- RecordType 3 (Deslocamento) → ActivityType "Deslocamento"
UPDATE pr
SET pr.ActivityTypeId = at.Id
FROM [dbo].[ProcessRecord] pr
INNER JOIN [dbo].[Attorney] a ON pr.AttorneyId = a.Id
INNER JOIN [dbo].[ActivityTypes] at ON at.TenantId = a.TenantId
WHERE pr.RecordType = 3 AND at.Name = 'Deslocamento' AND pr.ActivityTypeId IS NULL;

PRINT 'Migrados registros Deslocamento: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO

-- 5. Verificar se há registros sem ActivityTypeId
DECLARE @nullCount INT;
SELECT @nullCount = COUNT(*) FROM [dbo].[ProcessRecord] WHERE ActivityTypeId IS NULL;

IF @nullCount > 0
BEGIN
    PRINT 'AVISO: Existem ' + CAST(@nullCount AS VARCHAR) + ' registros sem ActivityTypeId!';
    PRINT 'Atribuindo ao primeiro tipo de atividade disponível...';
    
    UPDATE pr
    SET pr.ActivityTypeId = (
        SELECT TOP 1 at.Id 
        FROM [dbo].[ActivityTypes] at
        INNER JOIN [dbo].[Attorney] a ON at.TenantId = a.TenantId
        WHERE a.Id = pr.AttorneyId
        ORDER BY at.DisplayOrder
    )
    FROM [dbo].[ProcessRecord] pr
    WHERE pr.ActivityTypeId IS NULL;
    
    PRINT 'Registros corrigidos: ' + CAST(@@ROWCOUNT AS VARCHAR);
END
ELSE
BEGIN
    PRINT 'Todos os registros foram migrados com sucesso!';
END
GO

-- 6. Tornar ActivityTypeId NOT NULL
PRINT 'Tornando ActivityTypeId obrigatório...';
ALTER TABLE [dbo].[ProcessRecord]
ALTER COLUMN [ActivityTypeId] INT NOT NULL;
PRINT 'ActivityTypeId agora é obrigatório!';
GO

-- 7. Criar índice e FK
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProcessRecord_ActivityTypeId')
BEGIN
    PRINT 'Criando índice IX_ProcessRecord_ActivityTypeId...';
    CREATE INDEX [IX_ProcessRecord_ActivityTypeId] ON [dbo].[ProcessRecord] ([ActivityTypeId]);
    PRINT 'Índice criado!';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ProcessRecord_ActivityTypes_ActivityTypeId')
BEGIN
    PRINT 'Criando Foreign Key...';
    ALTER TABLE [dbo].[ProcessRecord]
    ADD CONSTRAINT [FK_ProcessRecord_ActivityTypes_ActivityTypeId] 
    FOREIGN KEY ([ActivityTypeId]) REFERENCES [dbo].[ActivityTypes] ([Id]);
    PRINT 'Foreign Key criada!';
END
GO

-- 8. Remover coluna RecordType antiga
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProcessRecord]') AND name = 'RecordType')
BEGIN
    PRINT 'Removendo coluna RecordType antiga...';
    ALTER TABLE [dbo].[ProcessRecord] DROP COLUMN [RecordType];
    PRINT 'Coluna RecordType removida!';
END
ELSE
BEGIN
    PRINT 'Coluna RecordType já foi removida.';
END
GO

PRINT '===========================================';
PRINT 'Migration concluída com sucesso!';
PRINT '===========================================';
GO

-- Verificar resultados
SELECT 'ActivityTypes' AS Tabela, COUNT(*) AS Total FROM [dbo].[ActivityTypes]
UNION ALL
SELECT 'ProcessRecord com ActivityTypeId', COUNT(*) FROM [dbo].[ProcessRecord] WHERE ActivityTypeId IS NOT NULL;
GO

-- Mostrar distribuição de tipos por tenant
SELECT 
    t.Name AS Tenant,
    at.Name AS TipoAtividade,
    at.Color AS Cor,
    COUNT(pr.Id) AS TotalRegistros
FROM [dbo].[ActivityTypes] at
INNER JOIN [dbo].[Tenants] t ON at.TenantId = t.Id
LEFT JOIN [dbo].[ProcessRecord] pr ON pr.ActivityTypeId = at.Id
GROUP BY t.Name, at.Name, at.Color
ORDER BY t.Name, at.DisplayOrder;
GO
