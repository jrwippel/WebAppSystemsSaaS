# Conversão para SaaS - Passo 1: Infraestrutura Multi-Tenant

## O que foi criado

### 1. Interface ITenantEntity
- Todas as entidades que precisam ser isoladas por tenant implementarão esta interface
- Garante que cada registro pertence a um tenant específico

### 2. Modelo Tenant
- Representa cada cliente (escritório/empresa) no sistema
- Campos principais:
  - `Name`: Nome do escritório
  - `Subdomain`: Identificador único (ex: "escritorio1")
  - `IsActive`: Controle de ativação/desativação
  - `SubscriptionExpiresAt`: Data de expiração da assinatura
  - `MaxUsers`, `MaxClients`, `MaxStorageMB`: Limites do plano

### 3. TenantService
- Identifica o tenant atual da requisição
- Métodos de identificação (em ordem de prioridade):
  1. Sessão do usuário
  2. Claims do JWT
  3. Subdomínio da URL (escritorio1.seuapp.com)

## Próximos Passos

### Passo 2: Adicionar TenantId aos Models
- Modificar: Attorney, Client, Department, ProcessRecord, etc
- Implementar ITenantEntity em cada modelo

### Passo 3: Configurar Filtros Globais no DbContext
- Adicionar query filters para isolar dados automaticamente
- Nenhuma query retornará dados de outro tenant

### Passo 4: Atualizar Controllers
- Injetar ITenantService
- Validar limites do plano antes de criar registros

### Passo 5: Criar Migration
- Adicionar coluna TenantId em todas as tabelas
- Criar tabela Tenants

### Passo 6: Sistema de Onboarding
- Página de cadastro de novo tenant
- Criação automática do primeiro usuário admin

### Passo 7: Painel Administrativo
- Gerenciar tenants
- Visualizar uso e limites
- Ativar/desativar contas

## Como Testar (após implementação completa)

1. Criar dois tenants diferentes
2. Fazer login em cada um
3. Verificar que os dados são completamente isolados
4. Testar acesso via subdomínio

## Observações Importantes

- Durante desenvolvimento local (localhost), o sistema usará sessão/JWT para identificar tenant
- Em produção, o subdomínio será o método principal
- Todos os dados existentes precisarão ser migrados para um tenant padrão
