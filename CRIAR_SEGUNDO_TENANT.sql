-- ============================================
-- Script para criar SEGUNDO TENANT para testes
-- ============================================

USE TimeTrackerSaaS;
GO

-- 1. Criar o segundo tenant
INSERT INTO Tenants (Name, Subdomain, Document, Email, Phone, IsActive, CreatedAt, MaxUsers, MaxClients, MaxStorageMB)
VALUES ('Empresa Teste', 'teste', '11111111111111', 'contato@teste.com', '11888888888', 1, GETDATE(), 10, 100, 2048);
GO

-- 2. Pegar o ID do tenant criado
DECLARE @TenantId INT = (SELECT Id FROM Tenants WHERE Subdomain = 'teste');

-- 3. Criar departamento para o tenant 2
INSERT INTO Department (Name, TenantId)
VALUES ('Administrativo Teste', @TenantId);
GO

-- 4. Pegar o ID do departamento criado
DECLARE @TenantId INT = (SELECT Id FROM Tenants WHERE Subdomain = 'teste');
DECLARE @DepartmentId INT = (SELECT Id FROM Department WHERE TenantId = @TenantId);

-- 5. Criar usuário admin para o tenant 2
-- Senha: 123 (hash: 40bd001563085fc35165329ea1ff5c5ecbdbbeef)
INSERT INTO Attorney (
    Name, 
    Email, 
    Phone, 
    BirthDate, 
    DepartmentId, 
    Perfil, 
    Password, 
    RegisterDate, 
    Login, 
    TenantId,
    Inativo,
    UseBorder,
    UseCronometroAlwaysOnTop
)
VALUES (
    'Admin Teste', 
    'admin@teste.com', 
    '11777777777', 
    '1990-01-01', 
    @DepartmentId,
    1, -- Admin
    '40bd001563085fc35165329ea1ff5c5ecbdbbeef', -- Senha: 123
    GETDATE(), 
    'admin2', 
    @TenantId,
    0, -- Não inativo
    0, -- UseBorder
    0  -- UseCronometroAlwaysOnTop
);
GO

-- 6. Criar alguns dados de teste para o Tenant 2
DECLARE @TenantId INT = (SELECT Id FROM Tenants WHERE Subdomain = 'teste');

-- Criar cliente de teste
INSERT INTO Client (Name, Document, Email, Telephone, TenantId, ClienteInterno, ClienteInativo)
VALUES ('Cliente Teste Tenant 2', '12345678900', 'cliente@teste.com', '11666666666', @TenantId, 0, 0);
GO

-- 7. Verificar o que foi criado
SELECT 
    'Tenant 2 criado!' as Mensagem,
    t.Id as TenantId,
    t.Name as TenantName,
    t.Subdomain,
    (SELECT COUNT(*) FROM Attorney WHERE TenantId = t.Id) as Usuarios,
    (SELECT COUNT(*) FROM Client WHERE TenantId = t.Id) as Clientes,
    (SELECT COUNT(*) FROM Department WHERE TenantId = t.Id) as Departamentos
FROM Tenants t
WHERE t.Subdomain = 'teste';
GO

-- 8. Mostrar credenciais
SELECT 
    'CREDENCIAIS DO TENANT 2' as Info,
    'admin2' as Login,
    '123' as Senha,
    'Empresa Teste' as Tenant;
GO

-- 9. Comparar os dois tenants
SELECT 
    t.Name as Tenant,
    t.Subdomain,
    (SELECT COUNT(*) FROM Attorney WHERE TenantId = t.Id) as Usuarios,
    (SELECT COUNT(*) FROM Client WHERE TenantId = t.Id) as Clientes,
    (SELECT COUNT(*) FROM Department WHERE TenantId = t.Id) as Departamentos,
    (SELECT COUNT(*) FROM ProcessRecord WHERE TenantId = t.Id) as Atividades
FROM Tenants t
ORDER BY t.Id;
GO
