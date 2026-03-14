# ⚠️ EXECUTAR ESTE SQL AGORA NO SQL SERVER MANAGEMENT STUDIO

## Passo 1: Corrigir usuário jeff.bezzos

Abra o SQL Server Management Studio e execute:

```sql
USE TimeTrackerSaaS;
GO

-- Corrigir o TenantId do usuário jeff.bezzos
-- Ele deve estar no Tenant 3 (Amazon SA), não no Tenant 1
UPDATE Attorney
SET TenantId = 3
WHERE Login = 'jeff.bezzos';
GO

-- Corrigir o DepartmentId também (deve ser o departamento do Tenant 3)
DECLARE @DeptIdAmazon INT = (SELECT Id FROM Department WHERE TenantId = 3);

UPDATE Attorney
SET DepartmentId = @DeptIdAmazon
WHERE Login = 'jeff.bezzos';
GO

-- Verificar se ficou correto
SELECT 
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
```

## Resultado esperado:

```
Usuario                  Login         TenantId  Tenant      DepartmentId  Departamento
Jackson Ricardo Wippel   jeff.bezzos   3         Amazon SA   3             Administrativo
```

---

## Passo 2: Verificar todos os usuários por tenant

```sql
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
```

## Resultado esperado:

```
TenantId  Tenant              Subdomain   TotalUsuarios  Usuarios
1         Empresa Principal   principal   2              admin, jackson.wippel
3         Amazon SA           amazon      1              jeff.bezzos
```

---

## ✅ Depois de executar, o sistema estará pronto!

Agora quando você fizer login:
- Login: `jeff.bezzos` → Sistema identifica automaticamente que é do Tenant 3 (Amazon)
- Login: `admin` → Sistema identifica automaticamente que é do Tenant 1 (Empresa Principal)

Cada usuário verá apenas os dados da sua empresa! 🎉
