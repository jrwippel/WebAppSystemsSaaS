BEGIN TRANSACTION;
GO

ALTER TABLE [Parametros] ADD [AliquotaTributos] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260321001503_AddAliquotaTributosToParametros', N'8.0.0');
GO

COMMIT;
GO

