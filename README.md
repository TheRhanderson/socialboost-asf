# SocialBoost - Amplificação de Interações no ArchiSteamFarm

O SocialBoost é um plug-in complementar para o ArchiSteamFarm, projetado para potencializar a interação nas plataformas Steam. Este plugin oferece funcionalidades para impulsionar o número de curtidas e favoritos em imagens, guias, e outros tipos de conteúdo, além de possibilitar a análise de jogos de usuários (Útil/Engraçado) e o seguimento da Oficina de jogadores, com mais recursos a serem adicionados em breve.

## Funcionalidades

### Sharedfiles
Para mídias do tipo sharedfiles, os seguintes comandos estão disponíveis:

- **SHAREDLIKE [Bots] [Id]:** Envia curtidas para uma URL sharedfiles específica.
- **SHAREDFAV [Bots] [Id]:** Envia favoritos para uma URL sharedfiles específica.
- **SHAREDFILES [Bots] [Id]:** Envia curtidas e favoritos para uma URL sharedfiles específica.

Exemplo de uso: `SHAREDFILES ASF 3142209500` (O Id 3142209500 refere-se ao final da URL).

### Análise de Jogos
Para análise de jogos, o comando disponível é:

- **RATEREVIEW [Bots] [Url Análise] [Tipo]:** Envia uma recomendação (Útil ou Engraçado) para uma análise de jogo. O tipo 1 é para Útil, e o tipo 2 é para Engraçado.

Exemplo de uso: `RATEREVIEW ASF https://steamcommunity.com/id/xxxxxxxxx/recommended/739630 1` (A URL é referente à análise de jogo, e o 1 indica uma recomendação Útil).

### Oficina Steam
Para seguir a Oficina de um perfil Steam, utilize o comando:

- **WORKSHOP [Bots] [Url Perfil]:** Começa a seguir a Oficina deste perfil Steam. Contas limitadas são compatíveis.

Exemplo de uso: `WORKSHOP ASF https://steamcommunity.com/id/xxxxxxxxxxxxxx` (A URL deve ser a mesma usada para visitar o perfil no navegador).

---
