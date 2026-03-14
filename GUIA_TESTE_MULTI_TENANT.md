# Guia Completo: Testando Multi-Tenancy

## Cenário 1: Múltiplos Usuários no MESMO Tenant

### Como funciona:
- Vários usuários podem pertencer ao mesmo tenant
- Todos veem os MESMOS dados (clientes, atividades, etc)
- Útil para equipes trabalhando juntas

### Passo a Passo:

1. **Faça login como admin** (já está logado)
   - Login: `admin`
   - Senha: `123`

2. **Acesse o menu de Usuários/Attorneys**
   - Procure por "Advogados", "Usuários" ou "Attorneys" no menu

3. **Crie um novo usuário**
   - Nome: `Usuario Teste`
   - Email: `teste@empresa.com`
   - Login: `usuario1`
   - Senha: `123`
   - Departamento: Administrativo
   - Perfil: Padrão (não admin)

4. **Faça logout**

5. **Faça login com o novo usuário**
   - Login: `usuario1`
   - Senha: `123`

6. **Verifique que vê os mesmos dados**
   - Ambos os usuários (admin e usuario1) veem os mesmos clientes
   - Ambos pertencem ao Tenant 1 (Empresa Principal)

---

## Cenário 2: Testar Isolamento entre Tenants Diferentes

### Como funciona:
- Cada tenant é uma empresa/cliente diferente
- Dados completamente isolados
- Um tenant NUNCA vê dados de outro

### Passo a Passo:

#### Parte 1: Criar o Segundo Tenant

1. **Abra o SQL Server Management Studio**

2. **Execute o script:** `CRIAR_SEGUNDO_TENANT.sql`
   - Ou copie e cole o conteúdo abaixo:

```sql
USE TimeTrackerSaaS;

-- Criar segundo tenant
INSERT INTO Tenants (Name, Subdomain, Document, Email, Phone, IsActive, CreatedAt, MaxUsers, MaxClients, MaxStorageMB)
VALUES ('Empresa Teste', 'teste', '11111111111111', 'contato@teste.com', '11888888888', 1, GETDATE(), 10, 100, 2048);

-- Criar departamento
DECLARE @TenantId INT = (SELECT Id FROM Tenants WHERE Subdomain = 'teste');
INSERT INTO Department (Name, TenantId)
VALUES ('Administrativo Teste', @TenantId);

-- Criar usuário admin do tenant 2
DECLARE @TenantId2 INT = (SELECT Id FROM Tenants WHERE Subdomain = 'teste');
DECLARE @DeptId INT = (SELECT Id FROM Department WHERE TenantId = @TenantId2);

INSERT INTO Attorney (Name, Email, Phone, BirthDate, DepartmentId, Perfil, Password, RegisterDate, Login, TenantId, Inativo, UseBorder, UseCronometroAlwaysOnTop)
VALUES ('Admin Teste', 'admin@teste.com', '11777777777', '1990-01-01', @DeptId, 1, '40bd001563085fc35165329ea1ff5c5ecbdbbeef', GETDATE(), 'admin2', @TenantId2, 0, 0, 0);

-- Criar cliente de teste para o tenant 2
INSERT INTO Client (Name, Document, Email, Telephone, TenantId, ClienteInterno, ClienteInativo)
VALUES ('Cliente Teste Tenant 2', '12345678900', 'cliente@teste.com', '11666666666', @TenantId2, 0, 0);
```

3. **Verificar criação:**
```sql
SELECT 
    t.Name as Tenant,
    t.Subdomain,
    (SELECT COUNT(*) FROM Attorney WHERE TenantId = t.Id) as Usuarios,
    (SELECT COUNT(*) FROM Client WHERE TenantId = t.Id) as Clientes
FROM Tenants t
ORDER BY t.Id;
```

Deve mostrar:
```
Tenant                  Subdomain   Usuarios   Clientes
Empresa Principal       principal   1          0
Empresa Teste          teste       1          1
```

#### Parte 2: Testar o Isolamento

1. **Faça login como Tenant 1 (Empresa Principal)**
   - Login: `admin`
   - Senha: `123`
   - Crie um cliente: "Cliente da Empresa Principal"
   - Anote quantos clientes você vê

2. **Faça logout**

3. **Faça login como Tenant 2 (Empresa Teste)**
   - Login: `admin2`
   - Senha: `123`
   - Veja que já existe 1 cliente: "Cliente Teste Tenant 2"
   - Crie outro cliente: "Segundo Cliente Empresa Teste"

4. **Faça logout novamente**

5. **Faça login como Tenant 1 novamente**
   - Login: `admin`
   - Senha: `123`
   - **IMPORTANTE:** Você NÃO deve ver os clientes do Tenant 2!
   - Deve ver apenas o cliente que você criou no passo 1

#### Parte 3: Verificar no Banco

```sql
-- Ver todos os clientes com seus tenants
SELECT 
    c.Id,
    c.Name as Cliente,
    c.TenantId,
    t.Name as Tenant
FROM Client c
INNER JOIN Tenants t ON c.TenantId = t.Id
ORDER BY c.TenantId, c.Id;
```

Deve mostrar algo como:
```
Id   Cliente                          TenantId   Tenant
1    Cliente da Empresa Principal     1          Empresa Principal
2    Cliente Teste Tenant 2           2          Empresa Teste
3    Segundo Cliente Empresa Teste    2          Empresa Teste
```

---

## Resumo dos Testes

### ✅ Teste 1: Múltiplos usuários no mesmo tenant
- Criar usuário pela interface
- Ambos veem os mesmos dados
- Útil para equipes

### ✅ Teste 2: Isolamento entre tenants
- Criar segundo tenant via SQL
- Cada tenant vê apenas seus dados
- Isolamento total garantido

---

## Credenciais para Testes

### Tenant 1 (Empresa Principal)
- **Login:** `admin`
- **Senha:** `123`
- **TenantId:** 1

### Tenant 2 (Empresa Teste)
- **Login:** `admin2`
- **Senha:** `123`
- **TenantId:** 2

---

## Comandos SQL Úteis

### Ver todos os tenants
```sql
SELECT * FROM Tenants;
```

### Ver distribuição de dados
```sql
SELECT 
    t.Name as Tenant,
    (SELECT COUNT(*) FROM Attorney WHERE TenantId = t.Id) as Usuarios,
    (SELECT COUNT(*) FROM Client WHERE TenantId = t.Id) as Clientes,
    (SELECT COUNT(*) FROM ProcessRecord WHERE TenantId = t.Id) as Atividades
FROM Tenants t;
```

### Ver todos os usuários com seus tenants
```sql
SELECT 
    a.Login,
    a.Name,
    a.TenantId,
    t.Name as Tenant
FROM Attorney a
INNER JOIN Tenants t ON a.TenantId = t.Id
ORDER BY a.TenantId;
```

### Deletar o tenant 2 (se quiser recomeçar)
```sql
DELETE FROM ProcessRecord WHERE TenantId = 2;
DELETE FROM Client WHERE TenantId = 2;
DELETE FROM Attorney WHERE TenantId = 2;
DELETE FROM Department WHERE TenantId = 2;
DELETE FROM Tenants WHERE Id = 2;
```

---

## O que você deve observar

### ✅ Funcionando Corretamente:
1. Usuários do Tenant 1 NÃO veem dados do Tenant 2
2. Usuários do Tenant 2 NÃO veem dados do Tenant 1
3. Novos registros recebem TenantId automaticamente
4. Múltiplos usuários no mesmo tenant veem os mesmos dados

### ❌ Se algo não funcionar:
1. Verifique se o TenantId está sendo salvo na sessão após login
2. Verifique se os filtros globais estão ativos
3. Verifique no banco se o TenantId está correto nos registros

---

## Próximo Passo

Depois de testar, você pode:
1. Atualizar o LoginController para salvar TenantId na sessão automaticamente
2. Criar interface para cadastro de novos tenants (onboarding)
3. Criar painel admin para gerenciar tenants
