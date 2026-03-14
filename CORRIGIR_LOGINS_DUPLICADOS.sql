-- Script para corrigir logins duplicados no banco de dados
-- Execute este script para remover o usuário jeff.bezzos duplicado

-- PROBLEMA: Dois usuários com login "jeff.bezzos" em tenants diferentes
-- Id 4 (TenantId 1) e Id 6 (TenantId 4)

-- SOLUÇÃO: Manter apenas o usuário do TenantId 4 (Amazon SA) e remover o do TenantId 1

-- 1. Verificar os registros duplicados
SELECT Id, Name, Login, Email, TenantId 
FROM Attorney 
WHERE Login = 'jeff.bezzos';

-- 2. Remover o usuário jeff.bezzos do TenantId 1 (Id = 4)
-- Este é o registro incorreto que foi criado com TenantId errado
DELETE FROM Attorney WHERE Id = 4 AND Login = 'jeff.bezzos' AND TenantId = 1;

-- 3. Verificar se ficou apenas um registro
SELECT Id, Name, Login, Email, TenantId 
FROM Attorney 
WHERE Login = 'jeff.bezzos';

-- Resultado esperado: Apenas 1 registro (Id = 6, TenantId = 4)

-- 4. Verificar todos os logins para garantir que não há outras duplicatas
SELECT Login, COUNT(*) as Quantidade, STRING_AGG(CAST(TenantId AS VARCHAR), ', ') as Tenants
FROM Attorney
GROUP BY Login
HAVING COUNT(*) > 1;

-- Resultado esperado: Nenhum registro (tabela vazia)
