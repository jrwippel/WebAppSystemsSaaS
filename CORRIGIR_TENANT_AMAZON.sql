-- ============================================
-- Script para corrigir TenantId do usuário Amazon
-- ============================================

USE TimeTrackerSaaS;
GO

-- 1. Verificar situação atual
SELECT 
    'ANTES DA CORREÇÃO' as Status,
    a.Id,
    a.Name as Usuario,
    a.Login,
    a.TenantId,
    t.Name as Tenant
FROM Attorney a
LEFT JOIN Tenants t ON a.TenantId = t.Id
WHERE a.Login = 'jeff.bezzos';
GO

-- 2. Corrigir o TenantId do usuário jeff.bezzos
-- Ele deve estar no Tenant 3 (Amazon SA), não no Tenant 1
UPDATE Attorney
SET TenantId = 3
WHERE Login = 'jeff.bezzos';
GO

-- 3. Corrigir o DepartmentId também (deve ser o departamento do Tenant 3)
DECLARE @DeptIdAmazon INT = (SELECT Id FROM Department WHERE TenantId = 3);

UPDATE Attorney
SET DepartmentId = @DeptIdAmazon
WHERE Login = 'jeff.bezzos';
GO

-- 4. Verificar situação após correção
SELECT 
    'DEPOIS DA CORREÇÃO' as Status,
    a.Id,
    a.Name as Usuario,
    a.Login,
    a.TenantId,
    t.Name as Tenant,
    a.DepartmentId,
    d.Name as Departamento
FROM Attorney a
LEFT JOIN Tenants t ON a.TenantId = t.Id
LEFT JOIN Department d ON a.DepartmentId = d.Id
WHERE a.Login = 'jeff.bezzos';
GO

-- 5. Verificar todos os usuários por tenant
SELECT 
    t.Id as TenantId,
    t.Name as Tenant,
    t.Subdomain,
    COUNT(a.Id) as TotalUsuarios,
    STRING_AGG(a.Login, ', ') as Usuarios
FROM Tenants t
LEFT JOIN Attorney a ON t.Id = a.TenantId
GROUP BY t.Id, t.Name, t.Subdomain
ORDER BY t.Id;
GO
