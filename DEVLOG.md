# Devlog — Bipe Informática

17/12/2025 · Victoria Tauanny

## Visão do jogo

Bipe Informática é um jogo de diagnóstico e gestão numa loja de conserto de PCs no Brasil, entre 1994 e 2001. O jogador ouve clientes leigos, testa a máquina e deduz o defeito antes de a loja fechar.

**Ciclos de jogo**

- **30 segundos:** um cliente, uma queixa vaga, alguns testes, um diagnóstico.
- **5 minutos:** um dia de loja, com relógio, caixa, fama e aluguel.
- **1 hora:** a era avança (CD-ROM, internet discada, lan houses, bug do milênio), trazendo defeitos e clientes novos.

**Pilares de diferenciação**

1. **O bairro como personagem:** clientes que voltam, lembram de você e trazem dilemas morais.
2. **Brasil anos 90:** peças do Paraguai, Plano Real, CD pirata, lan house, bug do milênio.
3. **Dedução de verdade:** clientes que mentem, defeitos intermitentes ou em dupla, ferramentas compradas com o lucro.

**Referências de mercado**

| Jogo | O que tem em comum | Onde nos diferenciamos |
| --- | --- | --- |
| PC Building Simulator 1 e 2 | Modo carreira de conserto de PCs | Sem história nem humor; diagnóstico é achar a peça, não deduzir |
| Car Mechanic Simulator | Diagnosticar, consertar, cobrar | Foco em desmontagem 3D, outro tema |
| Papers, Please | Inspeção curta, dilemas, pressão de tempo | Referência de estrutura, não de tema |
| Hypnospace Outlaw | Nostalgia e humor dos anos 90 | Sem conserto de máquinas |

## Estado atual

O protótipo web roda no navegador: abra o arquivo `bipe-informatica.html`. O jogo principal está em `UnityProject/`.

- 7 testes com custo de tempo: conversar, ligar o PC (com bipes reais), abrir o gabinete, multímetro, antivírus, ScanDisk, gerenciador de dispositivos.
- 9 defeitos: 6 no dia 1 e mais 3 a partir do dia 2.
- Escolha de preço justo ou o dobro, com risco de perder fama.
- Dia das 9h às 18h, aluguel de R$ 60, fim de jogo com caixa negativo ou fama zero.
- Cola dos bipes num post-it no balcão.

## Diário de versões

| Data | Versão | O que mudou |
| --- | --- | --- |
| 17/12/2025 | v0.4 (Unity) | Diagnóstico mais profundo. Clientes que mentem (a partir do dia 2): a conversa aponta para outro defeito e abrir o gabinete mostra que alguém mexeu. Defeitos em dupla (a partir do dia 3): o PC pode ter dois defeitos, e dá para marcar até 2 consertos; quem só conserta um recebe pelo que acertou e o cliente volta com o outro. Ferramentas compradas: começa com conversa, ligar, abrir e gerenciador; multímetro (R$ 40), disquete de antivírus (R$ 30) e ScanDisk (R$ 35) são comprados no fim do dia. Testado na Unity. Projeto migrado do Built-In para URP com renderizador 2D, configurado automaticamente pelo UrpSetup.cs. Builds para Windows (.exe) e web (WebGL) pelo menu Bipe, os dois testados |
| 10/12/2025 | v0.3 (Unity) | Projeto Unity em Documentos\\BipeInformatica, com git. Mesmas funcionalidades da v0.2, portadas para C#: regras separadas da interface (Game.cs), defeitos como fichas editáveis (ScriptableObject), tela montada por código, bipes gerados por código, save automático em JSON. Testado na Unity 6000.6.7f1: abre, compila e roda. Números do placar e monitor em fonte Consolas (o 5 parecia um S) |
| 05/12/2025 | v0.2 | Clientes que voltam e lembram do atendimento (saudação, nº de visitas; quem pagou o dobro e descobriu não aceita mais). Diagnóstico errado faz o cliente voltar no dia seguinte com o mesmo defeito. Peças do Paraguai a 40% do preço, com 35% de chance de queimar e o cliente voltar pedindo garantia (honrar com peça original ou negar e perder 2 de fama) |
| 01/12/2025 | v0.1 | Primeiro protótipo: ciclo de diagnóstico, 9 defeitos, bipes sonoros, preço justo ou dobro, dia com relógio e aluguel |

## Decisões de design

| Data | Decisão | Por quê |
| --- | --- | --- |
| 08/12/2025 | Motor: Unity (Godot como alternativa). Render pipeline: URP com renderizador 2D, porque o Built-In está obsoleto na Unity 6 e o 2D dá Pixel Perfect para a arte futura | Preferência pessoal; C# é próximo do C que já uso; muito material de estudo; jogo 2D de interface roda bem em qualquer um dos dois |
| 03/12/2025 | Clientes que voltam e peças do Paraguai são o próximo passo | Testam os pilares 1 e 2 com pouco código |
| 03/12/2025 | Evitar montagem 3D realista e gestão pesada por enquanto | É o terreno do PC Building Simulator; o ciclo de diagnóstico vem antes |
| 03/12/2025 | Ambientação no Brasil, 1994 a 2001 | Nicho sem concorrente; nostalgia local e charme exótico para o público de fora |
| 01/12/2025 | Foco no ciclo de diagnóstico como quebra-cabeça | Evita que o jogo vire uma planilha com gráficos |
| 01/12/2025 | Loja de informática anos 90 escolhida entre 3 ideias | Gênero de gestão com pouca concorrência, arte nostálgica barata e humor que vira clipe |

## Próximos passos

- [x] v0.2: clientes que voltam com memória do atendimento anterior (feito, falta testar jogando)
- [x] v0.2: peças do Paraguai, mais baratas e com chance de falhar dias depois (feito, falta testar jogando)
- [ ] Jogar 3 dias seguidos e anotar se dá vontade de "só mais um dia"
- [x] Ferramentas compradas com o lucro (feito na v0.4: multímetro, antivírus, ScanDisk)
- [x] Clientes que mentem e defeitos em dupla (feito na v0.4, falta testar jogando)
- [x] Primeiro dilema moral: arquivos pessoais no HD de um cliente

**Perguntas em aberto**

- Os testes estão baratos ou caros demais? Sinal: se sempre rodo todos, estão baratos.
- Motor decidido: Unity. Projeto aberto e rodando na Unity 6000.6.7f1.
