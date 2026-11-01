# Bipe Informática

Jogo de diagnóstico e gestão numa loja de conserto de PCs no Brasil dos anos 90.

## Pastas

| Pasta | O que tem |
| --- | --- |
| `UnityProject/` | O jogo em Unity |
| `UnityProject/Assets/_Bipe/Scripts/Core` | Regras do jogo, sem depender da interface (`Game.cs`, `GameState.cs`, `DefaultContent.cs`, `FaultDefinition.cs`, `SaveSystem.cs`) |
| `UnityProject/Assets/_Bipe/Scripts/UI` | Tela montada por código (`GameUI.cs`) |
| `UnityProject/Assets/_Bipe/Scripts/Audio` | Bipes da placa-mãe gerados por código (`BeepSpeaker.cs`) |
| `UnityProject/Assets/_Bipe/Editor` | Menu **Bipe > Gerar fichas de defeitos** |
| `bipe-informatica.html` | Protótipo web v0.2, para comparar |

## Como abrir na Unity

Precisa da **Unity 6**.

1. No Unity Hub, clique em **Adicionar > Adicionar projeto do disco** e escolha a pasta `UnityProject`.
2. Abra o projeto. Na primeira vez a Unity cria as pastas que faltam (`Library`, configurações), o que leva alguns minutos.
3. Com o projeto aberto, aperte **Play**. A loja abre sozinha em qualquer cena, sem montar nada.

Se o Hub não aceitar a pasta: crie um projeto novo **2D (Built-In)** pelo Hub e copie a pasta
`UnityProject/Assets/_Bipe` para dentro da pasta `Assets` dele.

Se aparecer erro de `StandaloneInputModule`: em **Edit > Project Settings > Player > Active Input Handling**,
escolha **Input Manager (Old)** ou **Both**.

## Como criar um defeito novo

1. Menu **Bipe > Gerar fichas de defeitos**. Isso cria uma ficha por defeito em `Assets/_Bipe/Resources/Defeitos`.
2. Duplique uma ficha (Ctrl+D), mude a `key` para um nome único e preencha os textos no Inspector.
3. Aperte Play. Enquanto essa pasta tiver fichas, o jogo usa só elas.

Campos de teste vazios mostram o resultado normal ("Fonte OK", "Nenhum vírus" etc.).
No campo **Power Beep**: `S` = bipe curto, `L` = bipe longo, separados por espaço; vazio = silêncio.

## Save

O jogo salva sozinho a cada ação em `bipe-save.json`, na pasta `Application.persistentDataPath`
(no Windows, em `%USERPROFILE%\AppData\LocalLow\<empresa>\<projeto>`). Use o botão **Novo jogo** para recomeçar.
