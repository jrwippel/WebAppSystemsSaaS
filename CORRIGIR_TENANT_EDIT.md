# ✅ Correção: Erro de Foreign Key ao Editar Registros

## Problema Identificado

Ao editar registros (ProcessRecord, Client, Department, Attorney), o sistema apresentava erro:

```
SqlException: A instrução UPDATE conflitou com a restrição do FOREIGN KEY "FK_ProcessRecord_Tenants_TenantId". 
O conflito ocorreu no banco de dados "TimeTrackerSaaS", tabela "dbo.Tenants", column 'Id'.
```

## Causa Raiz

Quando um formulário HTML é submetido, o model binding do ASP.NET Core cria um novo objeto com apenas os campos que vieram do formulário. O campo `TenantId` não está no formulário (é um campo interno), então ele vem como `0` (valor padrão de int).

### Fluxo do Problema

```
1. Usuário edita um ProcessRecord (Id=10, TenantId=2)
2. Formulário envia: { Id=10, Description="...", ClientId=5, ... }
3. Model Binding cria: ProcessRecord { Id=10, TenantId=0, Description="...", ... }
4. Controller faz: _context.Update(processRecord)
5. Entity Framework tenta: UPDATE ProcessRecord SET TenantId=0, ... WHERE Id=10
6. SQL Server rejeita: TenantId=0 não existe na tabela Tenants
7. ERRO: Foreign Key constraint violation
```

### Por que o TenantId vem como 0?

O campo `TenantId` não está no formulário HTML (é um campo oculto do sistema), então quando o ASP.NET Core faz o model binding, ele usa o valor padrão do tipo `int`, que é `0`.

## Solução Implementada

Antes de fazer o `Update`, carregar o registro existente do banco e preservar o `TenantId` original.

### Arquivos Corrigidos

#### 1. ProcessRecordsController.cs - Método Edit

**ANTES (ERRADO)**:
```csharp
[HttpPost]
public async Task<IActionResult> Edit(int id, ProcessRecord processRecord)
{
    _context.Update(processRecord); // TenantId = 0 (ERRO!)
    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
```

**DEPOIS (CORRETO)**:
```csharp
[HttpPost]
public async Task<IActionResult> Edit(int id, ProcessRecord processRecord)
{
    // Buscar o registro existente do banco
    var existingRecord = await _context.ProcessRecord.FindAsync(id);
    
    // Atualizar apenas os campos editáveis (NÃO atualiza TenantId)
    existingRecord.Date = processRecord.Date;
    existingRecord.HoraInicial = processRecord.HoraInicial;
    existingRecord.HoraFinal = processRecord.HoraFinal;
    existingRecord.Description = processRecord.Description;
    existingRecord.ClientId = processRecord.ClientId;
    // ... outros campos ...
    
    // TenantId permanece inalterado (mantém o valor original do banco)
    
    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
```

#### 2. AttorneyService.cs - Método UpdateAsync

**ANTES (ERRADO)**:
```csharp
public async Task UpdateAsync(Attorney obj)
{
    var existingAttorney = await _context.Attorney.AsNoTracking().FirstOrDefaultAsync(x => x.Id == obj.Id);
    obj.Password = existingAttorney.Password;
    obj.RegisterDate = existingAttorney.RegisterDate;
    
    _context.Update(obj); // TenantId pode ser 0 (ERRO!)
    await _context.SaveChangesAsync();
}
```

**DEPOIS (CORRETO)**:
```csharp
public async Task UpdateAsync(Attorney obj)
{
    var existingAttorney = await _context.Attorney.AsNoTracking().FirstOrDefaultAsync(x => x.Id == obj.Id);
    obj.Password = existingAttorney.Password;
    obj.RegisterDate = existingAttorney.RegisterDate;
    obj.TenantId = existingAttorney.TenantId; // Preserva TenantId original
    
    _context.Update(obj);
    await _context.SaveChangesAsync();
}
```

#### 3. ClientsController.cs - Método Edit

```csharp
var existingClient = await _context.Client.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

// Preserva imagem existente
client.ImageData = existingClient.ImageData;
client.ImageMimeType = existingClient.ImageMimeType;

// Preserva o TenantId original (IMPORTANTE para multi-tenancy)
client.TenantId = existingClient.TenantId;

_context.Update(client);
await _context.SaveChangesAsync();
```

#### 4. DepartmentsController.cs - Método Edit

```csharp
var existingDepartment = await _context.Department.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
if (existingDepartment != null)
{
    department.TenantId = existingDepartment.TenantId; // Preserva TenantId original
}

_context.Update(department);
await _context.SaveChangesAsync();
```

## Por que Isso é Crítico para Multi-Tenancy?

### Segurança
Se o TenantId pudesse ser alterado, um usuário poderia:
1. Editar um registro da sua empresa (TenantId=2)
2. Alterar o TenantId para 3 (outra empresa)
3. O registro seria transferido para outra empresa
4. **VAZAMENTO DE DADOS ENTRE EMPRESAS!**

### Integridade
O TenantId é a chave que mantém os dados isolados entre empresas. Ele NUNCA deve ser alterado após a criação do registro.

## Regra Geral para Updates em Sistema Multi-Tenant

Sempre que fizer `_context.Update()` em uma entidade que implementa `ITenantEntity`:

```csharp
// 1. Buscar registro existente
var existing = await _context.Entity.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

// 2. Preservar TenantId
entity.TenantId = existing.TenantId;

// 3. Preservar outros campos que não devem ser editados
entity.CreatedDate = existing.CreatedDate;
entity.CreatedBy = existing.CreatedBy;

// 4. Fazer o update
_context.Update(entity);
await _context.SaveChangesAsync();
```

## Alternativa: Usar Entry().Property()

Outra abordagem é marcar apenas os campos específicos como modificados:

```csharp
var entry = _context.Entry(processRecord);
entry.Property(p => p.Description).IsModified = true;
entry.Property(p => p.HoraInicial).IsModified = true;
entry.Property(p => p.HoraFinal).IsModified = true;
// ... outros campos editáveis ...

// TenantId não é marcado como modificado, então não será atualizado
await _context.SaveChangesAsync();
```

Mas a abordagem de carregar o registro existente é mais clara e segura.

## Teste

1. Faça login no sistema
2. Vá em "Lançamentos" (ProcessRecords)
3. Edite um lançamento existente
4. Altere a descrição ou horários
5. Clique em "Salvar"
6. Deve salvar com sucesso (sem erro de Foreign Key)

Repita o teste para:
- Editar Cliente (Clients)
- Editar Departamento (Departments)
- Editar Usuário (Attorneys)

## Arquivos Modificados

1. `WebAppSystems/Controllers/ProcessRecordsController.cs` - Método Edit
2. `WebAppSystems/Services/AttorneyService.cs` - Método UpdateAsync
3. `WebAppSystems/Controllers/ClientsController.cs` - Método Edit
4. `WebAppSystems/Controllers/DepartmentsController.cs` - Método Edit

## Observação

Os outros controllers (PercentualAreasController, ParametrosController, MensalistasController, ValorClientesController) também usam `_context.Update()`, mas essas entidades não implementam `ITenantEntity`, então não têm o problema de TenantId.

Se no futuro essas entidades forem convertidas para multi-tenant, será necessário aplicar a mesma correção.
