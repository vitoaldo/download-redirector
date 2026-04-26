<div align="center">
  <img src="logo.svg" alt="Download Redirector Logo" width="180" height="180" />
  <h1>Download Redirector</h1>
  <p><strong>Organização automática e invisível de arquivos para Windows.</strong></p>
  <h3><a href="https://github.com/vitoaldo/download-redirector/releases/latest">⬇️ Baixar Instalador (.exe) - Última Versão</a></h3>
  <br/>
</div>
---

[![Build Status](https://github.com/vitoaldo/download-redirector/actions/workflows/release.yml/badge.svg)](https://github.com/vitoaldo/download-redirector/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET Version](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)

O Download Redirector é um aplicativo de bandeja (System Tray) para Windows, extremamente leve e focado em alta performance. Ele automatiza a organização da sua vida digital, monitorando múltiplas pastas de sua escolha e transferindo arquivos para diretórios específicos com base nas suas extensões. Tudo isso silenciosamente, sem roubar recursos preciosos do seu processador.

---

## ✨ Principais Funcionalidades

- **Múltiplos Diretórios e Regras Avançadas**: Você não está mais restrito à pasta Downloads. Adicione quantas pastas quiser para serem vigiadas e defina múltiplos caminhos de destino escolhidos a dedo pelas extensões que devem receber.
- **Interface Visual (UI/UX)**: Esqueça arquivos complexos ocultos. O aplicativo agora conta com uma interface limpa, intuitiva e moderna, acessível direto da barra de tarefas, com suporte nativo a **Modos Claro e Escuro**.
- **Controle em Tempo Real**: Pause e retome a organização automática a qualquer momento, além de controlar qual será o intervalo de tempo exato de execução das varreduras através das Configurações.
- **Otimização Extrema de Performance (Kernel-Level)**: Migrado estruturalmente para o novíssimo **.NET 9**, o aplicativo instrui o escalonador do Windows a executá-lo com a prioridade mais baixa possível (`Idle`). Isso garante que ele só utilizará os núcleos físicos ou lógicos do processador se estiverem 100% ociosos. Jogos e softwares de edição jamais sofrerão impacto.
- **Ícone Dinâmico e Independente**: O ícone do programa é gerado dinamicamente via código na memória nativa, sem depender de arquivos de imagem (.ico/.png) instalados na máquina, mantendo o binário hiper-compacto e com zero quebras.
- **Instalador Traduzido (pt-BR)**: O empacotamento com GitHub Actions e Inno Setup gera um executável `.exe` totalmente em Português do Brasil de forma limpa, criando seu próprio atalho de inicialização automática (Startup) junto ao Windows.

---

## 🚀 Como rodar e desenvolver localmente

Pensando na melhor experiência de desenvolvimento, adotamos a mecânica prática de **Task Runner** (similar aos scripts de um `package.json` de Node/JS).

### Pré-requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Usando o Task Runner
Abra o terminal na raiz do projeto e use o nosso script gerenciador:

**Se usar PowerShell:**
```powershell
.\tasks.ps1 start
```

**Se usar CMD (Prompt de Comando):**
```cmd
tasks.bat start
```

O projeto será compilado com todos os pacotes restaurados e o ícone azul do app aparecerá automaticamente perto do seu relógio do Windows!

### Comandos de Desenvolvimento Disponíveis
Ao usar o `tasks.bat` ou `.\tasks.ps1`, você tem acesso rápido aos fluxos padronizados:
- `start` : Inicia a aplicação no seu desktop
- `test` : Executa toda a suíte de testes unitários (xUnit) garantindo a integridade do código
- `build` : Apenas compila o código e os testes (útil para validar a sintaxe e gerar builds temporárias)
- `installer` : Compila os binários de Release e gera o `.exe` final de instalação (requer o compilador *Inno Setup* instalado localmente)
- `help` : Lista na tela os comandos disponíveis

---

## ⚙️ Regras de Organização Avançadas

Ao clicar em "Configurações" pelo ícone da bandeja, você terá acesso imediato à edição do arquivo `appsettings.json`. A nova estrutura permite uma rede infinita de origens e destinos:

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
            "Extensions": [ ".txt", ".pdf", ".docx", ".xlsx" ]
          },
          {
            "TargetPath": "D:\\Arquivos_Pesados\\Videos",
            "Extensions": [ ".mp4", ".mkv", ".avi" ]
          }
        ]
      },
      {
        "SourcePath": "C:\\Pasta_Trabalho_Baguncada",
        "DefaultTargetPath": "C:\\Arquivados",
        "Targets": [
          {
            "TargetPath": "C:\\Relatorios",
            "Extensions": [ ".csv", ".xml" ]
          }
        ]
      }
    ]
  }
}
```

**Destaques Importantes:**
- Você define exatamente a janela de tempo da periodicidade do serviço (`ExecutionIntervalMinutes`).
- Suporte 100% confiável a variáveis do sistema (como o `%USERPROFILE%`).
- Qualquer alteração que você realizar pela interface passará a valer no **mesmo instante** sem a necessidade de fechar ou reiniciar a aplicação!

---

## 🗂️ Arquitetura do Projeto (Clean Code)

Esta arquitetura abraçou as mais modernas e recomendadas práticas de Clean Code, o que deixa as portas totalmente abertas para recursos futuros complexos — como integrações com LLMs (IA) locais para processamento de metadados:

- `Services/OrganizerService.cs`: É o "Motor Base". Controla o estado asssíncrono (Pausa/Retomar), o ciclo de vida do delay infinito, resolve o processamento inteligente das matrizes de extensões e protege os arquivos que sofrem bloqueios (Locks).
- `UI/MainApplicationContext.cs`: A "Mão". Contexto minimalista puro da API do Windows Forms injetado em um `.NET Host` que sustenta a existência no Windows Tray desenhando o próprio ícone pixel por pixel e abrindo Menus.
- `UI/ThemeManager.cs`: O "Arquiteto Visual". Responsável absoluto por interceptar toda e qualquer janela ou controle criado na interface para aplicar injetivamente as cores harmônicas que compõem o modo Claro ou Escuro nativo.
- `download-redirector.Tests/`: Suíte isolada sob a tecnologia **xUnit**, que assegura através do pipeline de integração contínua na Nuvem (Actions) que toda a lógica vital do projeto continua intacta após cada Commit.

---

## 📄 Licença
Distribuído sob a licença MIT. Veja o arquivo `LICENSE` para maiores detalhes.
