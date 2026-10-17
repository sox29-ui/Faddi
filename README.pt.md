<p align="center">
  <img src="docs/banner.png" alt="Faddi · فضّي" width="100%">
</p>

<p align="center">
  <a href="README.md">English</a> &middot;
  <a href="README.ar.md">العربية</a> &middot;
  <a href="README.es.md">Español</a> &middot;
  <a href="README.fr.md">Français</a> &middot;
  <b>Português</b> &middot;
  <a href="README.ru.md">Русский</a> &middot;
  <a href="README.tr.md">Türkçe</a> &middot;
  <a href="README.zh.md">简体中文</a>
</p>

<p align="center">
  <b>Descubra o que está lotando seu disco, limpe com segurança e mantenha a RAM sob controle.</b><br>
  Um aplicativo gratuito e de código aberto para Windows que lê o disco inteiro em cerca de um minuto, organiza tudo o que encontra de acordo com o quanto é seguro excluir
  e libera a memória que os aplicativos retêm mas não estão usando. Um único exe pequeno, nada para instalar.
</p>

<p align="center">
  <a href="https://github.com/sox29-ui/Faddi/releases/latest"><img alt="Versão mais recente" src="https://img.shields.io/github/v/release/sox29-ui/Faddi?color=E8A33A&label=release"></a>
  <a href="https://github.com/sox29-ui/Faddi/releases"><img alt="Downloads" src="https://img.shields.io/github/downloads/sox29-ui/Faddi/total?color=3FBF8A"></a>
  <a href="LICENSE"><img alt="Licença: MIT" src="https://img.shields.io/badge/license-MIT-2F6BDC"></a>
  <img alt="Windows 10 e 11" src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4">
  <img alt=".NET 8" src="https://img.shields.io/badge/.NET-8-512BD4">
  <img alt="8 idiomas" src="https://img.shields.io/badge/languages-8-B67FD8">
  <img alt="Sem telemetria" src="https://img.shields.io/badge/telemetry-none-555">
</p>

<p align="center">
  <a href="#instalação">Instalação</a> &middot;
  <a href="#recursos">Recursos</a> &middot;
  <a href="#capturas-de-tela">Capturas de tela</a> &middot;
  <a href="#o-que-o-faddi-limpa">O que ele limpa</a> &middot;
  <a href="#o-otimizador-de-ram">Otimizador de RAM</a> &middot;
  <a href="#seguro-por-design">Seguro por design</a> &middot;
  <a href="#para-desenvolvedores">Para desenvolvedores</a> &middot;
  <a href="CONTRIBUTING.md">Como contribuir</a>
</p>

<p align="center">
  <img src="docs/home-en.png" alt="Página inicial do Faddi: uma barra divide o disco em jogos, sobras de desenvolvimento, caches, itens antigos, Windows e espaço livre" width="900">
</p>

---

## O que é o Faddi?

O Faddi (فضّي, “libere espaço” em árabe) mostra exatamente para onde foi o espaço do seu disco e ajuda você a recuperá-lo sem quebrar nada. Ele lê o disco inteiro, separa tudo em jogos e aplicativos, sobras de desenvolvimento, caches, arquivos antigos e o próprio Windows, e explica o que cada item é e o que acontece se você o excluir.

- **Veja para onde o espaço foi**: uma barra para o disco inteiro, um mapa de cada pasta, os maiores arquivos e os duplicados.
- **Limpe com segurança**: os caches já vêm marcados, o resto espera por você, e os arquivos pessoais vão para a Lixeira.
- **Mantenha a RAM sob controle**: libere memória com um clique ou automaticamente e veja qual aplicativo causou um pico repentino.
- **Conheça seu PC**: CPU, RAM, placa de vídeo, discos com o estado de saúde de cada um, rede e aplicativos de inicialização, tudo explicado em palavras simples.

Tudo fica no seu PC. O Faddi não envia nada pela rede.

## Instalação

### Uma linha no PowerShell (a mais fácil, sem tela de aviso)

Clique em Iniciar, digite `PowerShell`, pressione Enter, depois cole esta linha e pressione Enter:

```powershell
irm https://raw.githubusercontent.com/sox29-ui/Faddi/main/install.ps1 | iex
```

Ela baixa o `Faddi.exe` mais recente para `%LOCALAPPDATA%\Programs\Faddi`, adiciona o Faddi ao Menu Iniciar e à Área de Trabalho e o abre. Execute a mesma linha de novo para atualizar.

### Ou baixe o exe

<p>
  <a href="https://github.com/sox29-ui/Faddi/releases/latest/download/Faddi.exe"><img alt="Baixar Faddi.exe" src="https://img.shields.io/badge/Baixar-Faddi.exe-E8A33A?style=for-the-badge"></a>
</p>

1. Baixe o **Faddi.exe** na [versão mais recente](https://github.com/sox29-ui/Faddi/releases/latest). É um único arquivo de cerca de 66 MB com tudo dentro, então não há nada para instalar.
2. Clique duas vezes nele. A análise começa na hora: cinco milhões de arquivos em um SSD NVMe levam de 45 a 90 segundos, e a barra do disco vai se preenchendo enquanto ele lê.

   O Windows pode mostrar **“O Windows protegeu o computador”** porque o exe é novo e não tem assinatura digital. Clique em **Mais informações** e depois em **Executar assim mesmo**. A linha do PowerShell acima evita essa tela.
3. Opcional: clique em **Executar como admin** na parte de baixo da barra lateral. Como admin, o Faddi consegue ler pastas protegidas, mostrar a temperatura e o desgaste do disco, limpar as sobras do próprio Windows e esvaziar o cache em espera.

> O Faddi usa o Microsoft Edge WebView2 Runtime, que vem com o Windows 11 e já está instalado em quase todos os PCs com Windows 10. Se ele estiver faltando, o Faddi avisa você.<br>
> Para remover o Faddi, desative **Iniciar com o Windows** em RAM e desempenho, depois exclua o `Faddi.exe` (ou `%LOCALAPPDATA%\Programs\Faddi`), os atalhos dele e a pasta `%LOCALAPPDATA%\Faddi`.

## Recursos

| Recurso | O que você pode fazer |
|---|---|
| <img src="docs/icons/strip.svg" width="22"> **Visão geral do disco** | Uma barra divide o disco em jogos e aplicativos, sobras de desenvolvimento, caches e temporários, itens antigos e grandes, Windows e todo o resto. |
| <img src="docs/icons/map.svg" width="22"> **Mapa do espaço** | Cada pasta vira um quadrado, com tamanho proporcional ao espaço que ocupa e cor que mostra há quanto tempo algo dentro dela foi alterado. Clique em um quadrado para entrar nele. |
| <img src="docs/icons/clean.svg" width="22"> **Limpeza por nível de segurança** | Caches e arquivos temporários já vêm marcados; sobras de desenvolvimento, jogos grandes e pastas sem alterações há 6 meses esperam por você. Cada item diz o que é e o que acontece se você o excluir. |
| <img src="docs/icons/code.svg" width="22"> **Sobras de desenvolvimento** | `node_modules` ao lado de um lockfile, ambientes Python (venv), `Library` do Unity, `DerivedDataCache` do Unreal, `bin`/`obj` do .NET, a saída de build do Gradle, do Flutter e do Rust, e os caches do npm, pip, uv, NuGet, Gradle e Hugging Face. |
| <img src="docs/icons/files.svg" width="22"> **Maiores arquivos e tipos** | Os maiores arquivos do disco e quais tipos de arquivo ocupam mais espaço. |
| <img src="docs/icons/dupes.svg" width="22"> **Arquivos duplicados** | Cópias encontradas comparando o conteúdo, não os nomes. Os hard links, que só parecem cópias, ficam de fora. |
| <img src="docs/icons/apps.svg" width="22"> **Aplicativos instalados** | Cada aplicativo com o tamanho real que ocupa no disco e um botão que abre o desinstalador do próprio aplicativo. |
| <img src="docs/icons/bolt.svg" width="22"> **Otimizador de RAM** | Libera memória com um clique, ou sozinho quando o uso passa de um limite ou a cada poucos minutos, só com a CPU ociosa. [Mais abaixo](#o-otimizador-de-ram). |
| <img src="docs/icons/ram.svg" width="22"> **Monitor de RAM** | Monitora a RAM a cada 2 segundos e registra os picos repentinos com os aplicativos responsáveis, além de dicas como definir um limite para o WSL, um driver com vazamento ou um daemon do Gradle que ficou em execução. |
| <img src="docs/icons/sys.svg" width="22"> **Informações do sistema** | CPU, pentes de RAM, placa de vídeo com temperatura em tempo real nas NVIDIA, discos com estado de saúde, temperatura e desgaste, rede, monitores, bateria e aplicativos de inicialização, cada um com uma explicação curta. |
| <img src="docs/icons/tray.svg" width="22"> **Na bandeja do sistema** | Ao fechar a janela, o Faddi continua na bandeja, de olho na RAM. Ele pode iniciar com o Windows. |
| <img src="docs/icons/globe.svg" width="22"> **Oito idiomas** | English, العربية, Español, Français, Português, Русский, Türkçe e 简体中文, da direita para a esquerda no árabe. Abre no idioma do seu Windows. |

## Capturas de tela

<table>
  <tr>
    <td width="50%"><img src="docs/home-en.png" alt="Início"><p align="center">Início: o disco inteiro em uma barra</p></td>
    <td width="50%"><img src="docs/map-en.png" alt="Mapa do espaço"><p align="center">Mapa do espaço, colorido pela idade</p></td>
  </tr>
  <tr>
    <td><img src="docs/clean-en.png" alt="Limpeza"><p align="center">Limpeza, organizada por nível de segurança</p></td>
    <td><img src="docs/ram-en.png" alt="Otimizador de RAM"><p align="center">Otimizador de RAM</p></td>
  </tr>
  <tr>
    <td><img src="docs/sys-en.png" alt="Informações do sistema"><p align="center">Informações do sistema, em tempo real</p></td>
    <td><img src="docs/home-ar.png" alt="Início em árabe"><p align="center">Árabe, da direita para a esquerda</p></td>
  </tr>
  <tr>
    <td><img src="docs/home-zh.png" alt="Início em chinês"><p align="center">简体中文</p></td>
    <td><img src="docs/home-ru.png" alt="Início em russo"><p align="center">Русский</p></td>
  </tr>
</table>

## O que o Faddi limpa

Tudo o que o Faddi encontra cai em um de cinco grupos, com as mesmas cores da barra do disco.

| Grupo | O que tem nele | O que acontece quando você limpa |
|---|---|---|
| <img src="docs/icons/dot-safe.svg" width="12"> **Limpeza segura** | Arquivos temporários com mais de uma semana, caches de navegadores e de shaders gráficos, e outros caches que os aplicativos recriam | Excluídos. Os aplicativos os recriam quando necessário. Já vêm marcados. |
| <img src="docs/icons/dot-dev.svg" width="12"> **Sobras de desenvolvimento** | `node_modules`, venvs, pastas de build e caches de pacotes | Excluídos. Voltam com um único comando, como `npm install`. |
| <img src="docs/icons/dot-games.svg" width="12"> **Jogos e aplicativos** | Jogos e aplicativos grandes instalados | Removidos com o desinstalador de cada um ou pelas Configurações do Windows. |
| <img src="docs/icons/dot-review.svg" width="12"> **Itens antigos e grandes** | Pastas sem alterações há 6 meses, downloads e vídeos antigos | Movidos para a Lixeira, então você pode restaurá-los. |
| <img src="docs/icons/dot-system.svg" width="12"> **Windows e sistema** | Pagefile, arquivo de hibernação, WinSxS, pontos de restauração | Alterados só pelas ferramentas do próprio Windows, com a explicação do que cada mudança significa. |

## O otimizador de RAM

Abra-o em **RAM e desempenho**, no botão **Otimizar** da barra lateral ou no ícone da bandeja. Ele pede a cada aplicativo que devolva a memória que retém mas não está usando; os aplicativos pegam de volta só o que realmente precisam, então um jogo ou um aplicativo pesado fica com mais espaço.

| Configuração | O que ela faz |
|---|---|
| <img src="docs/icons/bolt.svg" width="16"> **Otimizar RAM agora** | Libera memória na hora e mostra quanto voltou. |
| **Quando o uso de RAM chegar a** | Executa sozinho quando o uso passa do limite que você escolher (de 70 a 95%, com 85% por padrão), no máximo uma vez a cada 2 minutos. |
| **A cada** | Executa em intervalos fixos: a cada 3, 5, 10, 30 ou 60 minutos. |
| **Somente com a CPU ociosa** | As execuções automáticas esperam até o processador ficar abaixo de 30%, para nunca atrapalhar um jogo. Ativada por padrão. |
| **Liberar a RAM retida pelos aplicativos** | Esvazia o conjunto de trabalho de cada aplicativo: as páginas que ele retém mas não está usando voltam para o Windows. Ativada por padrão. |
| **Esvaziar o cache em espera do Windows** | Como admin, também esvazia o cache de arquivos usados recentemente que o Windows mantém na RAM. |
| **Limpar a área de transferência** | Esvazia o que você copiou por último, que pode ser uma imagem grande. |
| **Me avisar** | Mostra uma pequena notificação após cada execução automática. |
| <img src="docs/icons/tray.svg" width="16"> **Continuar na bandeja do sistema** | Ao fechar a janela, o Faddi continua na bandeja, ainda de olho na RAM e executando o otimizador. Ativada por padrão. |
| **Iniciar com o Windows** | Inicia na bandeja quando você entra no Windows. |

> A memória liberada volta a ser ocupada em parte quando os aplicativos voltam a usá-la; isso é esperado. O Windows gerencia bem a RAM sozinho, então o otimizador ajuda mais pouco antes de um jogo ou de um aplicativo pesado, ou depois que algo passou a ocupar memória demais.

## Seguro por design

- Nada é excluído até você clicar em **Limpar selecionados** e confirmar. Os arquivos pessoais (pastas antigas, downloads, vídeos) vão para a Lixeira, e o Windows pergunta antes de excluir de vez algo grande demais para ela.
- O Faddi nunca segue junções nem links simbólicos, então limpar `node_modules` não chega a uma pasta vinculada em outro lugar.
- Ficam fora de alcance as pastas Windows, Program Files e ProgramData\Microsoft, todos os perfis de usuário e as pastas Desktop, Documents e AppData de cada um. Nomes curtos como `C:\PROGRA~1` são resolvidos antes da verificação.
- Na pasta Temp, tudo o que mudou nos últimos 7 dias é mantido, e os arquivos em uso por um aplicativo em execução são ignorados. O cache de anexos do Outlook não é tocado.
- Os arquivos do sistema (pagefile, arquivo de hibernação, WinSxS, pontos de restauração) só são alterados pelas ferramentas do próprio Windows.
- Os processos essenciais do Windows não podem ser fechados pela página RAM e desempenho.

## Requisitos

- **Windows 10 ou 11, 64 bits.**
- **Microsoft Edge WebView2 Runtime**: vem incluso no Windows 11 e já está instalado em quase todos os PCs com Windows 10.
- **Permissão de admin**: opcional. Tudo funciona sem ela; alguns extras precisam dela.
- **Placa de vídeo NVIDIA**: opcional, para ver temperatura, ventoinha e consumo de energia da GPU em tempo real.

## Para desenvolvedores

Precisa do [SDK do .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) e do Windows.

```bash
git clone https://github.com/sox29-ui/Faddi.git
cd Faddi
dotnet run --project Faddi           # executa o aplicativo
dotnet run --project Faddi.Check     # verificações automáticas: scanner, regras de limpeza e a proteção de exclusão, em pastas descartáveis
node tools/i18n-check.mjs            # cada idioma tem todas as strings, com os respectivos marcadores {0}
```

Gere o `Faddi.exe` de arquivo único que as versões distribuem:

```bash
dotnet publish Faddi -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none -o publish
```

### Estrutura do projeto

```text
Faddi/
  Faddi/                  o aplicativo
    Program.cs            janela, ícone da bandeja, instância única e a ponte com a qual a página conversa
    Scanner.cs            análise paralela do disco inteiro, com os tamanhos reais em disco
    Cleanup.cs            regras de limpeza e a proteção que decide o que pode ser excluído
    Dupes.cs              localizador de duplicados: primeiro um hash por amostragem, depois o SHA-256 completo
    Optimizer.cs          otimizador de RAM
    Watcher.cs            amostragem de CPU, RAM, GPU, disco e rede, e o registro de picos de RAM
    SysInfo.cs            informações do sistema vindas do WMI, do Registro do Windows e do nvidia-smi
    Settings.cs, I18n.cs  configurações e traduções
    index.html            a interface inteira, em uma página
    lang/                 um arquivo JSON por idioma
  Faddi.Check/            verificações automáticas
  tools/i18n-check.mjs    verificador de traduções
  docs/                   logo, ícones e capturas de tela deste README
```

### Tecnologias usadas

.NET 8 WinForms hospedando uma página WebView2, sem servidor web e sem rede. A análise, o otimizador e o monitor conversam com o Windows diretamente: `FindFirstFileExW` para a análise, `EmptyWorkingSet` e `NtSetSystemInformation` para a memória, contadores de desempenho PDH para os números em tempo real e WMI para o hardware. A interface é um único arquivo HTML, sem framework.

### Adicionar um idioma

O árabe é escrito direto no código; cada um dos outros idiomas é um arquivo em `Faddi/lang`. Copie o `en.json` para o código do seu idioma (por exemplo, `de.json`), traduza os valores, defina `"@name"` com o nome do idioma na própria língua e execute `node tools/i18n-check.mjs` até ele não relatar nada faltando ou quebrado. O novo idioma aparece no seletor, na parte de baixo da barra lateral.

## Seus dados

- As configurações ficam em `%LOCALAPPDATA%\Faddi\settings.json` e o registro de picos de RAM em `spikes.json`, ao lado dele.
- O Faddi nunca envia nada e não tem contas, anúncios nem telemetria.

## Como contribuir

Issues e pull requests são bem-vindos, principalmente novos idiomas e regras de limpeza para ferramentas que o Faddi ainda não conhece. Comece pelo [guia de contribuição](CONTRIBUTING.md) e siga o [código de conduta](CODE_OF_CONDUCT.md).

Encontrou um problema de segurança, como um jeito de fazer o Faddi excluir algo que não deveria? Relate em particular, como descrito em [SECURITY.md](SECURITY.md).

## Licença

O Faddi é distribuído sob a [Licença MIT](LICENSE). As bibliotecas que ele inclui mantêm as próprias licenças, listadas em [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
