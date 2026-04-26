# Download Redirector

[![Build Status](https://github.com/vitoaldo/download-redirector/actions/workflows/release.yml/badge.svg)](https://github.com/vitoaldo/download-redirector/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

O Download Redirector é um aplicativo de bandeja super leve para Windows, criado para organizar sua vida digital de forma automatizada. Ele monitora continuamente suas pastas (como a de Downloads) e transfere arquivos para os diretórios corretos com base em suas extensões. Ele roda silenciosamente em segundo plano e foi desenhado para usar quase zero do seu processamento.

---

## 🚀 O que ele faz?

Construído em `.NET 8` com foco absoluto em performance, este projeto:
1. Coloca um pequeno ícone na bandeja do seu sistema (junto ao relógio).
2. De tempos em tempos, verifica as pastas que você configurou.
3. Se encontrar algo, separa e organiza automaticamente.
4. Foi programado em baixo nível para delegar a prioridade da sua thread ao Sistema Operacional. Ele roda estritamente nos ciclos ociosos da sua CPU, garantindo que você nunca terá perda de performance em outras atividades (como jogar ou trabalhar pesado).

---

## 🛠️ Como rodar e desenvolver localmente

Se você deseja rodar no seu ambiente, modificar o código ou adicionar novos recursos, o fluxo é bastante simples.

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Script rápido para rodar
Para facilitar a vida de todos os desenvolvedores, criamos um **Task Runner** que funciona de maneira idêntica aos scripts de um `package.json` no mundo JavaScript.

Basta abrir o seu terminal na raiz do projeto e usar o nosso script:

**Se você usar PowerShell:**
```powershell
.\tasks.ps1 start
```

**Se você usar Prompt de Comando (CMD):**
```cmd
tasks.bat start
```

O projeto será compilado e o ícone aparecerá automaticamente na sua barra de tarefas! Clique com o botão direito nele para acessar as configurações ou pausar o serviço.

### Comandos de Desenvolvimento
Você pode rodar as seguintes tarefas de desenvolvimento usando o script (`tasks.bat <comando>` ou `.\tasks.ps1 <comando>`):
- `start`: Inicia a aplicação no seu desktop (usando `dotnet run`)
- `test`: Executa toda a suíte de testes unitários para garantir que não haja bugs
- `build`: Apenas compila o código (útil para CI)
- `installer`: Empacota e cria o `.exe` de instalação limpa (requer Inno Setup)
- `help`: Mostra o menu de ajuda com a lista de comandos

---

## ⚙️ Configurando as Regras

A mágica acontece lendo as configurações do arquivo `appsettings.json`. Você pode editá-lo diretamente pelo aplicativo (clicando em "Configurações").

Aqui está a estrutura de exemplo:

```json
{
  "WatcherSettings": {
    "ExecutionIntervalMinutes": 20,
    "Folders": [
      {
        "SourcePath": "%USERPROFILE%\\Downloads",
        "DefaultTargetPath": "%USERPROFILE%\\Downloads\\Outros",
        "Targets": [
          {
            "TargetPath": "%USERPROFILE%\\Downloads\\Documentos",
            "Extensions": [ ".txt", ".pdf", ".docx", ".xlsx", ".pptx", ".csv" ]
          },
          {
            "TargetPath": "D:\\MeusVideos",
            "Extensions": [ ".mp4", ".mkv", ".avi", ".webm" ]
          }
        ]
      }
    ]
  }
}
```

**Destaques:**
- `ExecutionIntervalMinutes`: A cada quantos minutos a varredura deve ocorrer.
- `SourcePath`: Pode ser um caminho absoluto (`C:\pasta`) ou usar variáveis do Windows (`%USERPROFILE%\pasta`).
- O aplicativo entende essas mudanças sem precisar ser reiniciado. Apenas salve o arquivo!

---

## 🗂️ Arquitetura do Projeto

Nós adotamos padrões de código limpo para que seja fácil entender e expandir o aplicativo. Se você for contribuir, note os três pilares do repositório:
- `Services/OrganizerService.cs`: O "motor" por trás de tudo. É onde o monitoramento acontece usando assincronicidade e manipulação de arquivos.
- `UI/MainApplicationContext.cs`: Este é o esqueleto do nosso app de bandeja. Ele desenha o ícone usando primitivas gráficas nativas e não depende de imagens externas para funcionar.
- `UI/ThemeManager.cs`: Central de UI/UX. Todos os controles (sejam caixas de texto ou botões) recebem suas cores a partir daqui, já suportando Modo Claro e Escuro de forma amigável.

Sempre que criar uma nova tela, lembre-se de chamar `ThemeManager.ApplyTheme(this)` no construtor.

---

## 📄 Licença
Distribuído sob a licença MIT. Veja o arquivo `LICENSE` para mais informações.
