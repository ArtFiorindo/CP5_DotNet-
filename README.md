# CP5 - Programação de API com Microsserviços

Este projeto implementa uma arquitetura de microsserviços baseada em mensagens usando RabbitMQ para o Checkpoint 5 da FIAP.

## Membros da Equipe
- **Artur Fiorindo** - RM553481
- **Eduardo Função** - RM553362
- **Erick Lopes** - RM553927
- **Jhoe Hashimoto** - RM553927

## Arquitetura do Projeto

A solução consiste nos seguintes componentes:

1. **Sender1**: Envia informações sobre frutas sazonais para o serviço de validação
2. **Sender2**: Envia informações de cadastro de usuários para o serviço de validação
3. **Validation**: Valida mensagens de ambos os emissores antes de encaminhá-las
4. **Receiver1**: Recebe informações validadas sobre frutas
5. **Receiver2**: Recebe informações validadas de usuários
6. **RabbitMQ**: Broker de mensagens rodando em Docker

O fluxo da arquitetura segue este padrão:

```
Sender1 --> [FruitExchange] --> Validation --> [ValidationExchange] --> Receiver1
Sender2 --> [UserExchange] --> Validation --> [ValidationExchange] --> Receiver2
```

## Tecnologias Utilizadas

- .NET 6.0
- C#
- RabbitMQ
- Docker Desktop

## Instruções de Configuração

### Pré-requisitos

- Visual Studio 2022
- Docker Desktop
- .NET 6.0 SDK

### Executando o Projeto

1. **Inicie o container RabbitMQ**:
   ```
   docker-compose up -d
   ```

2. **Acesse a interface de gerenciamento do RabbitMQ**:
    - Abra http://localhost:15672/
    - Faça login com guest/guest

3. **Compile e execute os projetos**:
    - Abra a solução no Visual Studio 2022
    - Clique com o botão direito na solução no Solution Explorer
    - Selecione "Set Startup Projects..."
    - Escolha "Multiple startup projects" e defina todos os projetos como "Start"
    - Clique em "Start" ou pressione F5 para executar a solução

## Estrutura do Projeto

- **CP5.Common**: Modelos e constantes compartilhados
    - **Models/FruitMessage.cs**: Modelo para informações de frutas
    - **Models/UserMessage.cs**: Modelo para informações de usuários
    - **Constants.cs**: Nomes de filas, chaves de roteamento, etc.

- **CP5.Sender1**: Envia informações de frutas com data/hora atual
- **CP5.Sender2**: Envia dados de usuários com data/hora de registro
- **CP5.Validation**: Valida mensagens de ambos os emissores
- **CP5.Receiver1**: Processa informações validadas sobre frutas
- **CP5.Receiver2**: Processa informações validadas de usuários

## Exemplos de Teste

### Enviando Informações de Frutas

1. Inicie todas as aplicações
2. No console do Sender1, pressione Enter para enviar uma mensagem com informações aleatórias de frutas
3. O serviço de validação validará a mensagem
4. O Receiver1 exibirá as informações validadas

Exemplo de saída do Sender1:
```
==== Emissor de Informações de Frutas ====
Conexão RabbitMQ estabelecida. Pronto para enviar informações.
Pressione Enter para enviar dados (Ctrl+C para sair)

[2025-04-29 20:15:32] Informações enviadas:
Nome: Manga
Descrição: Sazonal no verão. Contém vitaminas A e C.
Horário: 2025-04-29 20:15:32
------------------------------
```

Exemplo de saída do serviço de validação:
```
==== Serviço de Validação ====
Conexão RabbitMQ estabelecida. Pronto para validar mensagens.

[2025-04-29 20:15:32] Mensagem recebida para validação:
Nome: Manga
Descrição: Sazonal no verão. Contém vitaminas A e C.
Resultado: Válido
Mensagem encaminhada para Receiver 1
------------------------------
```

Exemplo de saída do Receiver1:
```
==== Receptor de Informações de Frutas ====
Conexão RabbitMQ estabelecida. Aguardando mensagens...

==================================
[2025-04-29 20:15:32] Informações recebidas:
Fruta: Manga
Descrição: Sazonal no verão. Contém vitaminas A e C.
Horário: 2025-04-29 20:15:32
Status: Válido
Mensagem: Válido
==================================

Processado informação para: Manga
Informação sazonal: Manga é sazonal no Verão (Maio a Setembro)
Está na estação: Sim
```

### Enviando Informações de Usuário

1. Inicie todas as aplicações
2. No console do Sender2, pressione Enter para enviar uma mensagem com informações aleatórias de usuário
3. O serviço de validação validará a mensagem
4. O Receiver2 exibirá as informações validadas

Exemplo de saída do Sender2:
```
==== Emissor de Informações de Usuário ====
Conexão RabbitMQ estabelecida. Pronto para enviar informações.
Pressione Enter para enviar dados (Ctrl+C para sair)

[2025-04-29 20:16:45] Informações enviadas:
Nome: João da Silva
Endereço: Rua das Flores, 123 - São Paulo
RG: 12.345.678-9
CPF: 123.456.789-00
Horário: 2025-04-29 20:16:45
------------------------------
```

Exemplo de saída do serviço de validação:
```
==== Serviço de Validação ====
Conexão RabbitMQ estabelecida. Pronto para validar mensagens.

[2025-04-29 20:16:45] Mensagem recebida para validação:
Nome: João da Silva
CPF: 123.456.789-00
Resultado: Válido
Mensagem encaminhada para Receiver 2
------------------------------
```

Exemplo de saída do Receiver2:
```
==== Receptor de Informações de Usuário ====
Conexão RabbitMQ estabelecida. Aguardando mensagens...

==================================
[2025-04-29 20:16:45] Informações recebidas:
Nome: João da Silva
Endereço: Rua das Flores, 123 - São Paulo
RG: 12.345.678-9
CPF: 123.456.789-00
Horário: 2025-04-29 20:16:45
Status: Válido
Mensagem: Válido
==================================

Processado cadastro para: João da Silva
Tempo desde registro: 0.00 horas
Cadastro verificado e processado com sucesso
```

## Interface de Gerenciamento do RabbitMQ

Você pode monitorar os exchanges, filas e mensagens usando a interface web:

1. Acesse http://localhost:15672/
2. Login com guest/guest
3. Explore as abas (Visão Geral, Conexões, Canais, Exchanges, Filas)
4. Na aba Filas, você pode monitorar taxas de mensagens, publicações/entregas e mais

