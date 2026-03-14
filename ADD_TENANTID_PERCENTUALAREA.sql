USE TimeTrackerSaaS;
GO

ALTER TABLE [dbo].[PercentualArea]
ADD [TenantId] INT NULL;
GO

UPDATE [dbo].[PercentualArea]
SET [TenantId] = 1;
GO

ALTER TABLE [dbo].[PercentualArea]
ALTER COLUMN [TenantId] INT NOT NULL;
GO

CREATE INDEX [IX_PercentualArea_TenantId] ON [dbo].[PercentualArea] ([TenantId]);
GO

ALTER TABLE [dbo].[PercentualArea]
ADD CONSTRAINT [FK_PercentualArea_Tenants_TenantId] 
FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]);
GO

PRINT 'PercentualArea concluída!';
GO
