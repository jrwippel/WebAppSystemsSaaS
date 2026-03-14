# 🔐 Como Funciona a Identificação de Tenant por Email

## Visão Geral

O sistema identifica automaticamente qual empresa (tenant) o usuário pertence através do email informado no login. Não é necessário que o usuário selecione a empresa - tudo é automático!

## Fluxo Completo do Login

### 1️⃣ Usuário Informa o Email
```
Tela de Login
┌─────────────────────────┐
│ Email: joao@ambev.com   │
│ Senha: ••••••••         │
│ [Entrar]                │
└─────────────────────────┘
```

### 2️⃣ Sistema Busca o Usuário por Email
**Arquivo**: `LoginController.cs` → Método `Entrar()`

```csharp
// Busca usuário por email (ignora filtros de tenant)
var usuario = _attorneyService.FindByEmailAsync(loginModel.Login);
```

**Importante**: O método `FindByEmailAsync` usa `IgnoreQueryFilters()` para buscar em TODOS os tenants:

```csharp
public Attorney FindByEmailAsync(string email)
{
    // Ignora filtros de tenant para permitir login por email de qualquer empresa
    return _context.Attorney
        .IgnoreQueryFilters()
        .FirstOrDefault(x => x.Email.ToUpper() == email.ToUpper());
}
```

### 3️⃣ Sistema Identifica o Tenant do Usuário
Quando o usuário é encontrado, ele já vem com o `TenantId`:

```csharp
// Exemplo de dados retornados:
Attorney {
    Id = 5,
    Name = "João Silva",
    Email = "joao@ambev.com",
    TenantId = 2,  // ← AQUI está a identificação da empresa!
    ...
}
```

### 4️⃣ Sistema Salva o TenantId na Sessão
**Arquivo**: `LoginController.cs` → Método `Entrar()`

```csharp
if (usuario.ValidaSenha(loginModel.Senha))
{
    // Login bem-sucedido
    _sessao.CriarSessaoDoUsuario(usuario);
    
    // Salvar TenantId na sessão para isolamento de dados
    HttpContext.Session.SetInt32("TenantId", usuario.TenantId);
    
    return RedirectToAction("Index", "Home");
}
```

### 5️⃣ Sistema Usa o TenantId em Todas as Requisições
**Arquivo**: `TenantService.cs` → Método `GetTenantId()`

```csharp
public int GetTenantId()
{
    var httpContext = _httpContextAccessor.HttpContext;
    if (httpContext != null)
    {
        // Obtém o TenantId da sessão
        var tenantIdFromSession = httpContext.Session.GetInt32("TenantId");
        if (tenantIdFromSession.HasValue)
        {
            return tenantIdFromSession.Value;
        }
    }
    
    return 1; // Tenant padrão (fallback)
}
```

### 6️⃣ Entity Framework Filtra Automaticamente os Dados
**Arquivo**: `WebAppSystemsContext.cs` → Método `OnModelCreating()`

```csharp
// Configurar filtros globais para multi-tenancy
modelBuilder.Entity<Attorney>().HasQueryFilter(e => e.TenantId == _tenantService.GetTenantId());
modelBuilder.Entity<Client>().HasQueryFilter(e => e.TenantId == _tenantService.GetTenantId());
modelBuilder.Entity<Department>().HasQueryFilter(e => e.TenantId == _tenantService.GetTenantId());
modelBuilder.Entity<ProcessRecord>().HasQueryFilter(e => e.TenantId == _tenantService.GetTenantId());
```

## Exemplo Prático

### Cenário: Duas Empresas Diferentes

#### Empresa 1: Ambev (TenantId = 2)
```
Usuário: joao@ambev.com
TenantId: 2
```

#### Empresa 2: Amazon (TenantId = 3)
```
Usuário: jeff@amazon.com
TenantId: 3
```

### Fluxo de Login - João da Ambev

```
1. João acessa: https://localhost:5095
2. Digita: joao@ambev.com / senha123
3. Sistema busca no banco:
   SELECT * FROM Attorney WHERE Email = 'joao@ambev.com'
   → Retorna: { Id: 5, Email: 'joao@ambev.com', TenantId: 2 }
4. Sistema salva na sessão: TenantId = 2
5. João é redirecionado para Home
6. Todas as consultas agora filtram por TenantId = 2:
   - SELECT * FROM Client WHERE TenantId = 2
   - SELECT * FROM ProcessRecord WHERE TenantId = 2
   - SELECT * FROM Department WHERE TenantId = 2
```

### Fluxo de Login - Jeff da Amazon

```
1. Jeff acessa: https://localhost:5095
2. Digita: jeff@amazon.com / senha456
3. Sistema busca no banco:
   SELECT * FROM Attorney WHERE Email = 'jeff@amazon.com'
   → Retorna: { Id: 8, Email: 'jeff@amazon.com', TenantId: 3 }
4. Sistema salva na sessão: TenantId = 3
5. Jeff é redirecionado para Home
6. Todas as consultas agora filtram por TenantId = 3:
   - SELECT * FROM Client WHERE TenantId = 3
   - SELECT * FROM ProcessRecord WHERE TenantId = 3
   - SELECT * FROM Department WHERE TenantId = 3
```

## Isolamento de Dados

### Como o Sistema Garante que João não Vê Dados da Amazon?

1. **No Login**: Sistema identifica que João pertence ao TenantId = 2
2. **Na Sessão**: TenantId = 2 fica armazenado na sessão HTTP
3. **Em Cada Query**: Entity Framework adiciona automaticamente `WHERE TenantId = 2`
4. **Resultado**: João só vê dados da Ambev (TenantId = 2)

### Exemplo de Query Automática

**Código do Desenvolvedor**:
```csharp
var clientes = await _context.Client.ToListAsync();
```

**Query Real Executada pelo Entity Framework**:
```sql
SELECT * FROM Client WHERE TenantId = 2
```

O filtro `WHERE TenantId = 2` é adicionado AUTOMATICAMENTE pelo Entity Framework!

## Vantagens desta Abordagem

### ✅ Segurança
- Email é único no sistema (não pode repetir)
- Cada usuário pertence a apenas um tenant
- Isolamento automático de dados

### ✅ Simplicidade para o Usuário
- Não precisa selecionar a empresa
- Não precisa lembrar de subdomínio
- Apenas email + senha

### ✅ Manutenção Fácil
- Filtros automáticos em todas as queries
- Não precisa adicionar `WHERE TenantId = X` manualmente
- Menos chance de erro (vazamento de dados)

## Arquivos Envolvidos

| Arquivo | Responsabilidade |
|---------|------------------|
| `LoginController.cs` | Busca usuário por email e salva TenantId na sessão |
| `AttorneyService.cs` | Método `FindByEmailAsync()` que busca usuário ignorando filtros |
| `TenantService.cs` | Obtém o TenantId da sessão para uso em queries |
| `WebAppSystemsContext.cs` | Configura filtros globais para isolamento automático |
| `Sessao.cs` | Gerencia a sessão do usuário logado |

## Fluxograma Completo

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Usuário digita email + senha                             │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. LoginController.Entrar()                                 │
│    - Busca usuário por email (ignora filtros)               │
│    - Valida senha                                            │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. Usuário encontrado com TenantId                          │
│    Attorney { Id: 5, Email: "joao@ambev.com", TenantId: 2 } │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. Sistema salva na sessão                                  │
│    HttpContext.Session.SetInt32("TenantId", 2)              │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│ 5. Usuário navega pelo sistema                              │
│    - TenantService.GetTenantId() retorna 2                  │
│    - Entity Framework filtra: WHERE TenantId = 2            │
│    - Usuário só vê dados da sua empresa                     │
└─────────────────────────────────────────────────────────────┘
```

## Perguntas Frequentes

### 1. E se dois usuários tiverem o mesmo email?
❌ Não é possível! O sistema valida que o email é único em todo o sistema (todos os tenants).

### 2. E se eu quiser usar subdomínio ao invés de email?
O `TenantService` já tem suporte para subdomínio (`GetSubdomainFromRequest()`), mas atualmente não está sendo usado. Você pode implementar isso no futuro se quiser URLs como `ambev.seuapp.com`.

### 3. Como o sistema sabe qual tenant durante migrations?
Durante migrations, o `TenantService.GetTenantId()` retorna `1` (tenant padrão) como fallback.

### 4. O que acontece se eu remover o TenantId da sessão?
O sistema retorna ao tenant padrão (TenantId = 1). Por isso é importante sempre validar a sessão.

### 5. Posso ter um usuário em múltiplos tenants?
❌ Não com a arquitetura atual. Cada usuário pertence a apenas um tenant. Se precisar, você teria que criar emails diferentes (joao@ambev.com e joao@amazon.com).
