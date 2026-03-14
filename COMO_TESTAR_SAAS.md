# Como Testar o Sistema SaaS Multi-Tenant

## ✅ Configuração Automática

O sistema agora cria automaticamente:
- Tenant padrão ("Empresa Principal")
- Departamento "Administrativo"
- Usuário admin (login: `admin`, senha: `123`)
- Associa todos os dados existentes ao tenant padrão

**Não precisa mais executar SQL manualmente!**

## Passo 1: Rodar a Aplicação

```bash
cd WebAppSystems
dotnet run
```

O SeedingService vai automaticamente:
1. Criar o tenant padrão se não existir
2. Associar todos os dados existentes (TenantId = 0) ao tenant padrão (TenantId = 1)
3. Criar o usuário admin se não existir

## Passo 2: Fazer Login

- **Login:** `admin`
- **Senha:** `123`

## Passo 3: Verificar no Banco (Opcional)

Se quiser confirmar que funcionou:

```sql
-- Ver o tenant criado
SELECT * FROM Tenants;

-- Ver distribuição de dados
SELECT 
    'Attorneys' as Tabela, 
    COUNT(*) as Total, 
    TenantId 
FROM Attorney 
GROUP BY TenantId

UNION ALL

SELECT 'Clients', COUNT(*), TenantId 
FROM Client 
GROUP BY TenantId

UNION ALL

SELECT 'Departments', COUNT(*), TenantId 
FROM Department 
GROUP BY TenantId;
```

## Passo 4: Criar Segundo Tenant para Teste (Opcional)

```sql
-- Criar departamento para o tenant teste
INSERT INTO Department (Name, TenantId)
VALUES ('Departamento Teste', 2);

-- Criar usuário para o tenant teste
INSERT INTO Attorney (Name, Email, Phone, BirthDate, DepartmentId, Perfil, Password, RegisterDate, Login, TenantId)
VALUES ('Usuario Teste', 'teste@teste.com', '11777777777', '1990-01-01', 
        (SELECT Id FROM Department WHERE TenantId = 2), 
        0, 'HASH_SENHA_AQUI', GETDATE(), 'teste', 2);

-- Criar cliente para o tenant teste
INSERT INTO Client (Name, Document, Email, Telephone, TenantId)
VALUES ('Cliente Teste', '12345678900', 'cliente@teste.com', '11666666666', 2);
```

## Passo 4: Testar o Isolamento

### Teste 1: Verificar Isolamento de Dados

```sql
-- Ver dados do Tenant 1 (Principal)
SELECT 'Tenant 1 - Attorneys' as Info, COUNT(*) as Total FROM Attorney WHERE TenantId = 1
UNION ALL
SELECT 'Tenant 1 - Clients', COUNT(*) FROM Client WHERE TenantId = 1;

-- Ver dados do Tenant 2 (Teste)
SELECT 'Tenant 2 - Attorneys' as Info, COUNT(*) as Total FROM Attorney WHERE TenantId = 2
UNION ALL
SELECT 'Tenant 2 - Clients', COUNT(*) FROM Client WHERE TenantId = 2;
```

### Teste 2: Simular Acesso via Código

No código, o sistema vai filtrar automaticamente. Para testar:

1. **Faça login com um usuário do Tenant 1**
   - O sistema deve mostrar apenas dados do Tenant 1

2. **Modifique manualmente a sessão para Tenant 2** (apenas para teste)
   - Adicione um breakpoint no LoginController
   - Após login, force: `HttpContext.Session.SetInt32("TenantId", 2);`
   - Navegue pelo sistema
   - Deve mostrar apenas dados do Tenant 2

## Passo 5: Testar Criação Automática de TenantId

```sql
-- Antes de criar, verificar o TenantId atual na sessão
-- Depois criar um novo cliente via interface
-- Verificar se o TenantId foi preenchido automaticamente

SELECT TOP 1 * FROM Client ORDER BY Id DESC;
-- O TenantId deve estar preenchido automaticamente!
```

## Passo 6: Atualizar o LoginController (Próximo Passo)

Você precisará modificar o `LoginController` para:

1. Identificar o tenant do usuário que está fazendo login
2. Salvar o TenantId na sessão
3. Adicionar o TenantId no JWT (se usar API)

Exemplo básico:

```csharp
// No LoginController, após validar login/senha:
var attorney = await _context.Attorney
    .Include(a => a.Tenant)
    .FirstOrDefaultAsync(a => a.Login == login);

if (attorney != null && attorney.ValidaSenha(senha))
{
    // Salvar TenantId na sessão
    HttpContext.Session.SetInt32("TenantId", attorney.TenantId);
    
    // Salvar usuário na sessão (como já faz)
    _sessao.CriarSessaoDoUsuario(attorney);
    
    return RedirectToAction("Index", "Home");
}
```

## Verificações Importantes

### ✅ O que deve funcionar:

1. Usuários de tenants diferentes não veem dados uns dos outros
2. Ao criar novos registros, o TenantId é preenchido automaticamente
3. Todas as queries filtram por TenantId automaticamente
4. Cada tenant tem seus próprios limites (MaxUsers, MaxClients, etc)

### ⚠️ O que ainda precisa ser implementado:

1. Modificar LoginController para identificar e salvar TenantId
2. Criar página de cadastro de novos tenants (onboarding)
3. Criar painel administrativo para gerenciar tenants
4. Implementar validação de limites (MaxUsers, MaxClients)
5. Sistema de cobrança/assinatura

## Comandos Úteis para Debug

```sql
-- Ver todos os tenants
SELECT * FROM Tenants;

-- Ver distribuição de dados por tenant
SELECT 
    t.Name as Tenant,
    (SELECT COUNT(*) FROM Attorney WHERE TenantId = t.Id) as Usuarios,
    (SELECT COUNT(*) FROM Client WHERE TenantId = t.Id) as Clientes,
    (SELECT COUNT(*) FROM ProcessRecord WHERE TenantId = t.Id) as Atividades
FROM Tenants t;

-- Resetar TenantId (se precisar recomeçar)
UPDATE Attorney SET TenantId = 0;
UPDATE Client SET TenantId = 0;
UPDATE Department SET TenantId = 0;
UPDATE ProcessRecord SET TenantId = 0;
```

## Próximos Passos

1. ✅ Banco renomeado para TimeTrackerSaaS
2. ✅ Models com TenantId
3. ✅ DbContext com filtros globais
4. ✅ Migration aplicada
5. ⏳ Atualizar LoginController (PRÓXIMO)
6. ⏳ Criar sistema de onboarding
7. ⏳ Criar painel administrativo
8. ⏳ Implementar validação de limites

## Dúvidas Comuns

**P: Por que alguns dados não aparecem?**
R: Verifique se o TenantId está correto na sessão. Use o SQL acima para verificar.

**P: Como adicionar um novo tenant?**
R: Por enquanto, via SQL. Depois criaremos uma interface para isso.

**P: Os dados antigos vão funcionar?**
R: Sim, desde que você execute o Passo 2 para associá-los ao Tenant 1.

**P: Posso ter subdomínios diferentes?**
R: Sim! O campo `Subdomain` permite isso. Ex: escritorio1.seuapp.com, escritorio2.seuapp.com
