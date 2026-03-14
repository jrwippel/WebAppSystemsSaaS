-- Migration: AddTenantIdToParametros
-- Execute este script no SQL Server Management Studio (SSMS)
-- Database: TimeTrackerSaaS

USE TimeTrackerSaaS;
GO

-- Verificar se a coluna já existe
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Parametros]') AND name = 'TenantId')
BEGIN
    PRINT 'Adicionando coluna TenantId à tabela Parametros...';
    
    -- 1. Adicionar coluna TenantId (permitindo NULL temporariamente)
    ALTER TABLE [dbo].[Parametros]
    ADD [TenantId] INT NULL;
    
    PRINT 'Coluna TenantId adicionada. Atualizando registros existentes...';
    
    -- 2. Atualizar registros existentes com TenantId = 1 (tenant padrão)
    UPDATE [dbo].[Parametros]
    SET [TenantId] = 1;
    
    -- 3. Tornar a coluna NOT NULL
    ALTER TABLE [dbo].[Parametros]
    ALTER COLUMN [TenantId] INT NOT NULL;
    
    -- 4. Criar índice
    CREATE INDEX [IX_Parametros_TenantId] ON [dbo].[Parametros] ([TenantId]);
    
    -- 5. Adicionar Foreign Key
    ALTER TABLE [dbo].[Parametros]
    ADD CONSTRAINT [FK_Parametros_Tenants_TenantId] 
    FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]) ON DELETE CASCADE;
    
    PRINT 'Coluna TenantId adicionada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Coluna TenantId já existe na tabela Parametros.';
END
GO

-- Verificar o resultado
SELECT * FROM [dbo].[Parametros];
GO
