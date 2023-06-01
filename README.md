<p align="center">
  <img src="https://github.com/andrrff/BugSearch/assets/13167954/e6fdc0e7-1a8a-4d64-8b69-8d0379022b86" style="width: 300px;" alt="BugSearch drawio">
  <h1>BugSearch</h1>
</p>

[![bugsearchshared-service](https://github.com/andrrff/BugSearch/actions/workflows/bugsearchshared-service.yaml/badge.svg)](https://github.com/andrrff/BugSearch/actions/workflows/bugsearchshared-service.yaml)
[![bugsearchcrawler-service](https://github.com/andrrff/BugSearch/actions/workflows/bugsearchcrawler-service.yaml/badge.svg)](https://github.com/andrrff/BugSearch/actions/workflows/bugsearchcrawler-service.yaml)
[![bugsearchapi-service](https://github.com/andrrff/BugSearch/actions/workflows/bugsearchapi-service.yaml/badge.svg?branch=master)](https://github.com/andrrff/BugSearch/actions/workflows/bugsearchapi-service.yaml)

---

BugSearch é um motor de pesquisa de páginas indexadas pelo crawler BugSearch.Crawler. O projeto é dividido em duas partes: o lado do Bot (Bot side) e o lado do Cliente (Client side).

## Bot side

O lado do Bot é onde o crawler pode ser executado a partir de uma lista de links, explorando cada página e verificando recursivamente os links contidos nela. Para persistir os dados, utilizamos duas coleções no MongoDB: uma para armazenar o dicionário de termos e outra para armazenar os EventCrawlers. O modelo do EventCrawler é representado da seguinte forma:

```csharp
public class EventCrawler
{
    public string? _id { get; set; }
    public string? Url { get; set; }
    public string? Favicon { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Body { get; set; }
    public string[] Terms { get; set; } = Array.Empty<string>();
    public double Pts { get; set; } = 0;
}
```

O dicionário consiste em pares de chave-valor simples, onde a chave é um termo e o valor é seu identificador (_id), ambos representados como strings.

Todo o processo ocorre por meio de um sistema de mensageria RabbitMQ, onde os eventos são enfileirados. É possível ter várias instâncias do BugSearch.Crawler, criadas via Kubernetes, apontando para um único serviço do RabbitMQ na exchange BugSearch.

Exemplo de uso:

```shell
curl -X 'POST' \
  'http://localhost:5031/Api/robot' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '[
  "https://www.example.com/"
]'
```

## Client side

O lado do Cliente é responsável pela consulta aos sites catalogados. Ele oferece um resumo da quantidade de sites e termos armazenados no banco de dados, bem como um endpoint de prompt para o OpenAI.

### Endpoint de Search

Esse endpoint permite pesquisar por sites dentro do MongoDB e possui um algoritmo de pontuação para reorganizar os links a serem exibidos na resposta. Recebe como parâmetros "q" para a query e "l" para o limite/profundidade.

Exemplo de uso:

```shell
curl -X 'GET' \
  'http://localhost:5088/Search?q=Acidente%20Grave&l=20' \
  -H 'accept: text/plain'
```

### Endpoint de Summary

Esse endpoint retorna um resumo dos sites catalogados e a quantidade de termos registrados. Não recebe parâmetros.

Exemplo de uso:

```shell
curl -X 'GET' \
  'http://localhost:5088/Summary' \
  -H 'accept: text/plain'
```

### Endpoint de Prompt

Esse endpoint pode ser chamado em paralelo com o Search e retorna uma resposta gerada por uma IA. Recebe como parâmetro "q".

Exemplo de uso:

```shell
curl -X 'GET' \
  'http://localhost:5088/Prompt?q=ping' \
  -H 'accept: text/plain'
```

Obs: Todos os serviços do lado do Cliente também estão contidos em containers Docker e são adequados para implantação contínua no AKS (Azure Kubernetes Service).

## Exemplos



Aqui estão os exemplos de uso em uma tabela:

| Descrição             | Comando                                                                                           |
| --------------------- | ------------------------------------------------------------------------------------------------- |
| Executar o crawler    | `curl -X 'POST' 'http://localhost:5031/Api/robot' -H 'accept: */*' -H 'Content-Type: application/json' -d '["https://www.example.com/"]'` |
| Pesquisar             | `curl -X 'GET' 'http://localhost:5088/Search?q=Acidente%20Grave&l=20' -H 'accept: text/plain'`    |
| Resumo                | `curl -X 'GET' 'http://localhost:5088/Summary' -H 'accept: text/plain'`                            |
| Prompt                | `curl -X 'GET' 'http://localhost:5088/Prompt?q=ping' -H 'accept: text/plain'`                      |

## Arquitetura

<p align="center">
  <img src="https://github.com/andrrff/BugSearch/assets/13167954/55f78bb5-9f1e-4b01-8a96-7b404075cb8f" alt="BugSearch drawio">
</p>


## Contribuindo

Se você deseja contribuir com o projeto BugSearch, sinta-se à vontade para abrir issues e enviar pull requests no repositório do GitHub.

## Licença

Este projeto está licenciado nos termos da licença APACHE. Consulte o arquivo [LICENSE](LICENSE) para obter mais informações.
