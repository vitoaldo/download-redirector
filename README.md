# Download Redirector Service

Este é um template minimalista de um Serviço Windows (Windows Service) construído em C# (.NET 8). Foi projetado para ser pequeno, eficiente e fácil de estender, mantendo focado na execução de tarefas em segundo plano (background tasks).

## Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.

## Como Desenvolver e Testar Localmente

Para rodar o projeto localmente (no modo console para testes), abra um terminal nesta pasta e execute:
```powershell
dotnet run
```
Para parar, pressione `Ctrl+C`.

## Como Gerar o Instalável (Build & Publish)

Você não precisa do Visual Studio para gerar o executável. Para preparar o serviço para instalação em uma máquina de produção como um único arquivo sem depender de um runtime instalado, use o seguinte comando:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

O arquivo `.exe` resultante ficará disponível no seguinte diretório:
`bin\Release\net8.0\win-x64\publish\`

> **Nota:** Navegue até este diretório antes de executar os comandos de instalação abaixo, ou use o caminho completo para o `.exe`.

## Como Gerenciar o Serviço no Windows

Todas estas ações exigem um **Terminal (PowerShell) em modo Administrador**.
Assuma que o nome do projeto executável seja `download-redirector.exe`.

### 1. Instalação
Utilize o utilitário nativo do Windows, `sc.exe` (Service Control), para instalar seu serviço.

```powershell
# Exemplo usando sc.exe (Atenção ao espaço obrigatório após "binpath=")
sc.exe create "DownloadRedirectorService" binpath= "C:\Caminho\Completo\Para\Sua\Pasta\bin\Release\net8.0\win-x64\publish\download-redirector.exe" start= auto
```

Você também pode usar comandos nativos do PowerShell, se preferir:
```powershell
New-Service -Name "DownloadRedirectorService" -BinaryPathName "C:\Caminho\Completo\Para\Sua\Pasta\bin\Release\net8.0\win-x64\publish\download-redirector.exe" -StartupType Automatic
```

### 2. Iniciar o Serviço
```powershell
sc.exe start "DownloadRedirectorService"
# ou no PowerShell: Start-Service "DownloadRedirectorService"
```

### 3. Verificar o Status
```powershell
sc.exe query "DownloadRedirectorService"
# ou no PowerShell: Get-Service "DownloadRedirectorService"
```

### 4. Parar o Serviço
```powershell
sc.exe stop "DownloadRedirectorService"
# ou no PowerShell: Stop-Service "DownloadRedirectorService"
```

### 5. Desinstalar (Remover) o Serviço
**Atenção:** Certifique-se de parar o serviço antes de removê-lo.
```powershell
sc.exe delete "DownloadRedirectorService"
# ou no PowerShell (disponível no PowerShell 6+): Remove-Service "DownloadRedirectorService"
```

## Como Visualizar Logs (Visualizador de Eventos)

Por padrão, quando rodando como Serviço Windows puro, o `Microsoft.Extensions.Hosting.WindowsServices` escreve os logs de nível *Warning* e *Error* diretamente no **Visualizador de Eventos (Event Viewer)** do Windows, e em certos casos o nível de log *Information* não é registrado automaticamente a não ser configurado de forma explícita.

Para ver os logs do sistema:
1. Aperte `Win + R`, digite `eventvwr` e dê ENTER.
2. Navegue para: **Logs do Windows -> Aplicativo** (Windows Logs -> Application).
3. Na barra lateral direita, clique em **Filtrar Log Atual...** (Filter Current Log...)
4. No campo "Fontes de eventos" (Event sources), selecione o nome do seu aplicativo (ex: `download-redirector`).

### Dica de Logs Avançados
Se você deseja salvar logs em arquivos físicos (ex: `.txt`), é altamente recomendável adicionar um provedor de logs como o [Serilog](https://serilog.net/) ou o [NLog](https://nlog-project.org/) ao projeto. Eles são leves e facilmente configuráveis pelo `Program.cs`.
