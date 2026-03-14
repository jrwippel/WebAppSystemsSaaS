USE TimeTrackerSaaS;
GO

ALTER TABLE [dbo].[DocumentAnalysis]
ADD [TenantId] INT NULL;
GO

UPDATE [dbo].[DocumentAnalysis]
SET [TenantId] = 1;
GO

ALTER TABLE [dbo].[DocumentAnalysis]
ALTER COLUMN [TenantId] INT NOT NULL;
GO

CREATE INDEX [IX_DocumentAnalysis_TenantId] ON [dbo].[DocumentAnalysis] ([TenantId]);
GO

ALTER TABLE [dbo].[DocumentAnalysis]
ADD CONSTRAINT [FK_DocumentAnalysis_Tenants_TenantId] 
FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]);
GO

PRINT 'DocumentAnalysis concluída!';
GO
