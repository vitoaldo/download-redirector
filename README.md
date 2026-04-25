# Download Redirector Service

[![Release](https://github.com/vitoaldo/download-redirector/actions/workflows/release.yml/badge.svg)](https://github.com/vitoaldo/download-redirector/actions/workflows/release.yml)
[![Latest Release](https://img.shields.io/github/v/release/vitoaldo/download-redirector)](https://github.com/vitoaldo/download-redirector/releases/latest)

Serviço Windows (.NET 8) que monitora e organiza automaticamente arquivos de pastas configuráveis (como **Downloads**), categorizando-os em subpastas ou destinos personalizados por tipo de extensão. Roda em segundo plano como um Windows Service, sem necessidade de interação, recarregando configurações em tempo real.

---

## 📥 Instalação

### Via Instalador (Recomendado)

1. Acesse a página de [Releases](https://github.com/vitoaldo/download-redirector/releases/latest).
2. Baixe o arquivo `DownloadRedirector-Setup-x.x.x.exe`.
3. Execute como **Administrador**.
4. Pronto! O serviço será instalado e iniciado automaticamente.

> **Desinstalar:** Use **Adicionar ou Remover Programas** do Windows, ou execute o desinstalador presente na pasta de instalação.

### Via Linha de Comando (Manual)

Para instalar manualmente sem o instalador, siga os passos abaixo.

#### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.

#### 1. Gerar o Executável

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

O arquivo `.exe` resultante estará na pasta `./publish/`.

#### 2. Registrar o Serviço

Abra um **Terminal (PowerShell) em modo Administrador**:

```powershell
# Via sc.exe (atenção ao espaço obrigatório após "binpath=")
sc.exe create "DownloadRedirectorService" binpath= "C:\Caminho\Completo\publish\download-redirector.exe" start= auto

# Ou via PowerShell nativo:
New-Service -Name "DownloadRedirectorService" -BinaryPathName "C:\Caminho\Completo\publish\download-redirector.exe" -StartupType Automatic
```

#### 3. Iniciar o Serviço

```powershell
sc.exe start "DownloadRedirectorService"
```

---

## ⚙️ Configuração

O arquivo `appsettings.json` define as pastas a serem monitoradas e suas respectivas regras de redirecionamento. As modificações feitas neste arquivo entram em vigor automaticamente no próximo ciclo de varredura (por padrão a cada 20 minutos), sem a necessidade de reiniciar o serviço.

É possível utilizar variáveis de ambiente do Windows, como `%USERPROFILE%`.

**Localização:**
- Instalação via instalador: `C:\Program Files\DownloadRedirector\appsettings.json`
- Desenvolvimento local: raiz do projeto

**Exemplo de Configuração (`appsettings.json`):**

```json
{
  "WatcherSettings": {
    "Folders": [
      {
        "SourcePath": "%USERPROFILE%\\Downloads",
        "DefaultTargetPath": "%USERPROFILE%\\Downloads\\Outros",
        "Targets": [
          {
            "TargetPath": "%USERPROFILE%\\Downloads\\Documentos",
            "Extensions": [ ".txt", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".csv" ]
          },
          {
            "TargetPath": "%USERPROFILE%\\Downloads\\Videos",
            "Extensions": [ ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm" ]
          },
          {
            "TargetPath": "%USERPROFILE%\\Downloads\\Imagens",
            "Extensions": [ ".png", ".jpg", ".jpeg", ".gif", ".webp", ".bmp", ".svg" ]
          },
          {
            "TargetPath": "%USERPROFILE%\\Downloads\\Executaveis",
            "Extensions": [ ".exe", ".msi", ".bat", ".cmd", ".ps1" ]
          }
        ]
      }
    ]
  }
}
```

- **Folders**: Array contendo uma ou múltiplas pastas para vigiar.
- **SourcePath**: O caminho absoluto (ou relativo a `%USERPROFILE%`) da pasta sendo monitorada.
- **DefaultTargetPath**: A pasta para onde arquivos não reconhecidos nas extensões alvo serão enviados.
- **Targets**: Array com os destinos segmentados e sua lista de extensões compatíveis.

> **Nota:** O arquivo `appsettings.json` **não é sobrescrito** em atualizações via instalador, preservando suas personalizações.

---

## 📋 Gerenciamento do Serviço

Todos os comandos abaixo exigem um **Terminal em modo Administrador**.

```powershell
# Verificar status
sc.exe query "DownloadRedirectorService"

# Parar o serviço
sc.exe stop "DownloadRedirectorService"

# Iniciar o serviço
sc.exe start "DownloadRedirectorService"

# Remover o serviço (pare-o antes)
sc.exe delete "DownloadRedirectorService"
```

---

## 📄 Logs

Quando rodando como Serviço Windows, os logs são escritos no **Visualizador de Eventos (Event Viewer)**:

1. Pressione `Win + R`, digite `eventvwr` e pressione ENTER.
2. Navegue para: **Logs do Windows → Aplicativo**.
3. Filtre por fonte de evento `download-redirector`.

---

## 🛠️ Desenvolvimento

```powershell
# Rodar localmente (modo console)
dotnet run

# Para parar, pressione Ctrl+C
```

---

## 🚀 CI/CD — Releases Automáticas

O projeto usa **GitHub Actions** com **Inno Setup** para gerar instaladores automaticamente.

### Como funciona

1. Um desenvolvedor cria uma **tag** no formato `v*.*.*`
2. O workflow `release.yml` é disparado automaticamente.
3. O código é compilado como single-file self-contained para `win-x64`.
4. O Inno Setup gera o instalador `.exe`.
5. O instalador é publicado na aba **Releases** do GitHub.

### Como criar uma nova release

```powershell
# 1. Atualize a versão no .csproj (opcional, mas recomendado)
# 2. Commit e push das alterações
git add .
git commit -m "release: v1.1.0"
git push

# 3. Crie e envie a tag
git tag v1.1.0
git push origin v1.1.0
```

A release aparecerá automaticamente em: [github.com/vitoaldo/download-redirector/releases](https://github.com/vitoaldo/download-redirector/releases)

---

## 📜 Licença

Este projeto está licenciado sob a [MIT License](LICENSE).

Copyright (c) 2026 Victor Adalto Cavalcanti Valentim
