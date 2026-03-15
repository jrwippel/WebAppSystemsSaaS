-- Adiciona coluna TrialEndsAt na tabela Tenants
-- Execute com: sqlcmd -S <servidor> -d <banco> -i ADD_TRIALENDSAT_TENANTS.sql

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Tenants' AND COLUMN_NAME = 'TrialEndsAt'
)
BEGIN
    ALTER TABLE Tenants ADD TrialEndsAt datetime2 NULL;
    PRINT 'Coluna TrialEndsAt adicionada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Coluna TrialEndsAt ja existe.';
END

-- Opcional: definir trial de 14 dias para tenants existentes que ainda nao tem valor
UPDATE Tenants
SET TrialEndsAt = DATEADD(DAY, 14, CreatedAt)
WHERE TrialEndsAt IS NULL AND SubscriptionExpiresAt IS NULL;
