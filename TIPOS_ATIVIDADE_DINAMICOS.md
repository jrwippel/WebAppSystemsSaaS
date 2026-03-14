# Tipos de Atividade Dinâmicos (SaaS)

## Problema Identificado
O sistema atual usa um enum `RecordType` com valores fixos:
- Consultivo
- Contencioso  
- Proposta Específica
- Deslocamento

Isso não funciona para SaaS, pois cada tenant pode ter tipos de atividade diferentes:
- Escritório de advocacia: Consultivo, Contencioso, Audiência
- Consultoria: Reunião, Análise, Proposta
- Agência: Criação, Revisão, Apresentação
- TI: Desenvolvimento, Suporte, Reunião

## Solução Implementada

### 1. Nova Tabela: ActivityType
Tabela personalizável por tenant com os campos:
- `Id`: Identificador único
- `Name`: Nome do tipo (ex: "Consultivo", "Reunião")
- `Description`: Descrição opcional
- `Color`: Cor em hex para gráficos (#FF5733)
- `IsActive`: Se está ativo
- `DisplayOrder`: Ordem de exibição
- `TenantId`: Isolamento multi-tenant

### 2. Mudança no ProcessRecord
- **ANTES**: `RecordType RecordType` (enum)
- **DEPOIS**: `int ActivityTypeId` + `ActivityType ActivityType` (FK)

### 3. Migration de Conversão
A migration vai:
1. Criar tabela `ActivityTypes`
2. Adicionar coluna `ActivityTypeId` em `ProcessRecord`
3. Migrar dados existentes:
   - RecordType = 0 (Consultivo) → ActivityType "Consultivo"
   - RecordType = 1 (Contencioso) → ActivityType "Contencioso"
   - RecordType = 2 (Proposta Específica) → ActivityType "Proposta Específica"
   - RecordType = 3 (Deslocamento) → ActivityType "Deslocamento"
4. Remover coluna `RecordType` antiga

### 4. CRUD de Tipos de Atividade
Novo controller `ActivityTypesController` com:
- Index: Listar tipos do tenant
- Create: Criar novo tipo
- Edit: Editar tipo existente
- Delete: Desativar tipo (soft delete)

### 5. Seeding Automático
Quando um novo tenant é criado, o sistema cria automaticamente 4 tipos padrão:
- Consultivo (#4A90E2)
- Contencioso (#E24A4A)
- Proposta Específica (#50C878)
- Deslocamento (#FFA500)

## Benefícios
✅ Cada tenant pode ter seus próprios tipos de atividade
✅ Nomes personalizáveis
✅ Cores para visualização em gráficos
✅ Fácil adicionar/remover tipos
✅ Mantém compatibilidade com dados existentes

## Próximos Passos
1. ✅ Criar modelo `ActivityType`
2. ⏳ Atualizar modelo `ProcessRecord`
3. ⏳ Criar migration de conversão
4. ⏳ Atualizar DbContext
5. ⏳ Criar controller e views CRUD
6. ⏳ Atualizar seeding para novos tenants
7. ⏳ Atualizar formulários de registro de atividade
8. ⏳ Atualizar relatórios e gráficos
