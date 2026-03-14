# Plano de Renomeação Completo - Attorney → User, ProcessRecord → Activity

## ⚠️ ATENÇÃO: Esta é uma mudança MASSIVA

Esta renomeação afeta:
- 69+ arquivos de migration
- Todos os Controllers (15+ arquivos)
- Todos os Services
- Todos os Views
- DbContext
- DTOs
- Relacionamentos entre tabelas

## Recomendação: Abordagem Alternativa Mais Segura

### Opção A: Usar Table Mapping (SEM renomear classes)
Manter os nomes das classes mas mudar os nomes das tabelas no banco:

```csharp
// No DbContext
modelBuilder.Entity<Attorney>().ToTable("Users");
modelBuilder.Entity<ProcessRecord>().ToTable("Activities");
```

**Vantagens:**
- Não quebra código existente
- Migrations continuam funcionando
- Mudança rápida e segura
- Banco fica com nomes genéricos

**Desvantagens:**
- Código ainda usa termos jurídicos

### Opção B: Renomeação Completa (TRABALHOSO)
Renomear tudo: classes, arquivos, controllers, services, views, migrations.

**Passos necessários:**
1. Criar backup completo do projeto
2. Renomear classes principais (Attorney → User, ProcessRecord → Activity)
3. Atualizar 15+ Controllers
4. Atualizar 10+ Services  
5. Atualizar DbContext e todas as referências
6. Atualizar todos os Views (.cshtml)
7. Atualizar DTOs
8. DELETAR pasta Migrations inteira
9. Criar migration inicial do zero
10. Testar TUDO novamente

**Tempo estimado:** 4-6 horas de trabalho
**Risco de bugs:** ALTO

### Opção C: Híbrida (RECOMENDADA)
1. Usar Table Mapping para o banco (Opção A)
2. Criar aliases/wrappers para novos desenvolvimentos:
```csharp
using User = WebAppSystems.Models.Attorney;
using Activity = WebAppSystems.Models.ProcessRecord;
```
3. Gradualmente refatorar em futuras versões

## Minha Recomendação

Para conversão SaaS, sugiro **Opção A** (Table Mapping):
- Foco no multi-tenancy (objetivo principal)
- Banco com nomes genéricos
- Código continua funcionando
- Refatoração completa pode ser feita depois, com calma

## Decisão

Qual opção você prefere?
- [ ] Opção A - Table Mapping (rápido e seguro)
- [ ] Opção B - Renomeação completa (trabalhoso)
- [ ] Opção C - Híbrida (meio termo)
