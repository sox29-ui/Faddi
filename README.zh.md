<p align="center">
  <img src="docs/banner.png" alt="Faddi · فضّي" width="100%">
</p>

<p align="center">
  <a href="README.md">English</a> &middot;
  <a href="README.ar.md">العربية</a> &middot;
  <a href="README.es.md">Español</a> &middot;
  <a href="README.fr.md">Français</a> &middot;
  <a href="README.pt.md">Português</a> &middot;
  <a href="README.ru.md">Русский</a> &middot;
  <a href="README.tr.md">Türkçe</a> &middot;
  <b>简体中文</b>
</p>

<p align="center">
  <b>找出是什么在吃你的磁盘空间，安全地清理它，并管好你的内存。</b><br>
  一款免费、开源的 Windows 应用。它用大约一分钟读完整个磁盘，按删除的安全程度把找到的所有内容分好类，再释放应用占着却没在使用的内存。只有一个很小的 exe 文件，无需安装。
</p>

<p align="center">
  <a href="https://github.com/sox29-ui/Faddi/releases/latest"><img alt="最新发行版" src="https://img.shields.io/github/v/release/sox29-ui/Faddi?color=E8A33A&label=release"></a>
  <a href="https://github.com/sox29-ui/Faddi/releases"><img alt="下载量" src="https://img.shields.io/github/downloads/sox29-ui/Faddi/total?color=3FBF8A"></a>
  <a href="LICENSE"><img alt="许可证：MIT" src="https://img.shields.io/badge/license-MIT-2F6BDC"></a>
  <img alt="Windows 10 和 11" src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4">
  <img alt=".NET 8" src="https://img.shields.io/badge/.NET-8-512BD4">
  <img alt="8 种语言" src="https://img.shields.io/badge/languages-8-B67FD8">
  <img alt="无遥测" src="https://img.shields.io/badge/telemetry-none-555">
</p>

<p align="center">
  <a href="#安装">安装</a> &middot;
  <a href="#功能">功能</a> &middot;
  <a href="#截图">截图</a> &middot;
  <a href="#faddi-会清理什么">会清理什么</a> &middot;
  <a href="#内存优化">内存优化</a> &middot;
  <a href="#从设计上保证安全">从设计上保证安全</a> &middot;
  <a href="#面向开发者">面向开发者</a> &middot;
  <a href="CONTRIBUTING.md">参与贡献</a>
</p>

<p align="center">
  <img src="docs/home-zh.png" alt="Faddi 的主页：一条横条把磁盘分成游戏、开发残留文件、缓存、旧文件、Windows 和可用空间" width="900">
</p>

---

## 什么是 Faddi？

Faddi（فضّي，阿拉伯语，意为「释放空间」）会准确告诉你磁盘空间都去了哪里，并帮你把空间拿回来，而且不会弄坏任何东西。它会读取整个磁盘，把内容分为游戏和应用、开发残留文件、缓存、旧文件以及 Windows 本身，并告诉你每一项是什么，删除后会怎样。

- **看清空间去了哪里**：一条彩色横条展示整个磁盘，一张地图展示每个文件夹，另有最大的文件和重复文件。
- **安全地清理**：缓存默认已勾选，其他内容等你来决定，个人文件会移到回收站。
- **管好内存**：一键或自动释放内存，并查看是哪个应用造成了突增。
- **了解你的电脑**：CPU、内存、GPU、磁盘及其健康状况、网络和启动应用，每一项都有通俗易懂的说明。

所有数据都留在你的电脑里。Faddi 不会通过网络发送任何内容。

## 安装

### 一行 PowerShell 命令（最简单，没有警告界面）

点击「开始」，输入 `PowerShell`，按 Enter 键，然后粘贴下面这一行，再按 Enter 键：

```powershell
irm https://raw.githubusercontent.com/sox29-ui/Faddi/main/install.ps1 | iex
```

它会把最新的 `Faddi.exe` 下载到 `%LOCALAPPDATA%\Programs\Faddi`，把 Faddi 添加到开始菜单和桌面，并打开它。想更新时，再运行同一行命令即可。

### 或者下载 exe

<p>
  <a href="https://github.com/sox29-ui/Faddi/releases/latest/download/Faddi.exe"><img alt="下载 Faddi.exe" src="https://img.shields.io/badge/%E4%B8%8B%E8%BD%BD-Faddi.exe-E8A33A?style=for-the-badge"></a>
</p>

1. 从[最新发行版](https://github.com/sox29-ui/Faddi/releases/latest)下载 **Faddi.exe**。它是一个约 66 MB 的单文件，所有内容都已包含在内，因此无需安装。
2. 双击它。扫描会立即开始：在 NVMe SSD 上，500 万个文件需要 45 到 90 秒，磁盘横条会在读取过程中逐渐填满。

   Windows 可能会显示 **「Windows 已保护你的电脑」**，因为这个 exe 文件比较新，也没有经过代码签名。点击**更多信息**，再点击**仍要运行**。上面的 PowerShell 命令可以避开这个界面。
3. 可选：点击侧边栏底部的**以管理员身份运行**。以管理员身份运行时，Faddi 可以读取受保护的文件夹、显示磁盘温度和磨损情况、清理 Windows 自身的残留文件，并清空备用缓存。

> Faddi 使用 Microsoft Edge WebView2 Runtime。Windows 11 自带它，几乎每台 Windows 10 电脑上也已经装好了。如果缺少，Faddi 会提示你。<br>
> 要卸载 Faddi，请先在「内存和性能」页面关闭**随 Windows 启动，并在任务栏时钟旁运行**，再删除 `Faddi.exe`（或 `%LOCALAPPDATA%\Programs\Faddi`）、它的快捷方式和 `%LOCALAPPDATA%\Faddi` 文件夹。

## 功能

| 功能 | 你可以做什么 |
|---|---|
| <img src="docs/icons/strip.svg" width="22"> **磁盘总览** | 一条横条把磁盘分成游戏和应用、开发残留文件、缓存和临时文件、旧的大文件和文件夹、Windows，以及其他所有文件。 |
| <img src="docs/icons/map.svg" width="22"> **空间地图** | 每个文件夹都是一个方块，大小与它占用的空间成正比，颜色表示里面的内容已有多久没改动。点击方块即可进入。 |
| <img src="docs/icons/clean.svg" width="22"> **按安全程度清理** | 缓存和临时文件默认已勾选；开发残留文件、大型游戏和 6 个月没动过的文件夹，则等你来决定。每一项都会说明它是什么，以及删除后会发生什么。 |
| <img src="docs/icons/code.svg" width="22"> **开发残留文件** | 旁边有锁文件（lockfile）的 `node_modules`、Python 环境（venv）、Unity 的 `Library`、Unreal 的 `DerivedDataCache`、.NET 的 `bin`/`obj`、Gradle、Flutter 和 Rust 的构建输出，以及 npm、pip、uv、NuGet、Gradle 和 Hugging Face 的缓存。 |
| <img src="docs/icons/files.svg" width="22"> **最大文件和文件类型** | 磁盘上最大的文件，以及哪些类型的文件占用的空间最多。 |
| <img src="docs/icons/dupes.svg" width="22"> **重复文件** | 通过比较内容而不是文件名来找出副本。硬链接只是看起来像副本，所以不会列出。 |
| <img src="docs/icons/apps.svg" width="22"> **已安装的应用** | 每个应用在磁盘上的实际大小，还有一个按钮可以运行它自带的卸载程序。 |
| <img src="docs/icons/bolt.svg" width="22"> **内存优化** | 一键释放内存，也可以自动释放：使用率超过上限时，或每隔几分钟一次，而且只在 CPU 空闲时进行。[详见下文](#内存优化)。 |
| <img src="docs/icons/ram.svg" width="22"> **内存监控** | 每 2 秒采样一次，记录内存突增以及造成突增的应用，还会给出建议，例如 WSL 的内存上限、泄漏内存的驱动程序，或一直没关闭的 Gradle 守护进程。 |
| <img src="docs/icons/sys.svg" width="22"> **系统信息** | CPU、内存条、GPU（NVIDIA 显卡可显示实时温度）、磁盘及其健康状况、温度和磨损情况、网络、显示器、电池和启动应用，每一项都附有简短说明。 |
| <img src="docs/icons/tray.svg" width="22"> **任务栏时钟旁** | 关闭窗口后，Faddi 会留在托盘里继续监控内存。它还可以随 Windows 启动。 |
| <img src="docs/icons/globe.svg" width="22"> **八种语言** | English、العربية、Español、Français、Português、Русский、Türkçe 和简体中文，阿拉伯语从右到左排版。启动时使用你的 Windows 语言。 |

## 截图

<table>
  <tr>
    <td width="50%"><img src="docs/home-en.png" alt="主页"><p align="center">主页：一条横条看清整个磁盘</p></td>
    <td width="50%"><img src="docs/map-en.png" alt="空间地图"><p align="center">空间地图，按新旧程度上色</p></td>
  </tr>
  <tr>
    <td><img src="docs/clean-en.png" alt="清理"><p align="center">清理，按安全程度排序</p></td>
    <td><img src="docs/ram-en.png" alt="内存优化"><p align="center">内存优化</p></td>
  </tr>
  <tr>
    <td><img src="docs/sys-en.png" alt="系统信息"><p align="center">系统信息，实时更新</p></td>
    <td><img src="docs/home-ar.png" alt="阿拉伯语版主页"><p align="center">阿拉伯语，从右到左</p></td>
  </tr>
  <tr>
    <td><img src="docs/home-zh.png" alt="中文版主页"><p align="center">简体中文</p></td>
    <td><img src="docs/home-ru.png" alt="俄语版主页"><p align="center">Русский</p></td>
  </tr>
</table>

## Faddi 会清理什么

Faddi 找到的所有内容都会归入五个分组之一，颜色与磁盘横条上的一致。

| 分组 | 包含什么 | 清理时会发生什么 |
|---|---|---|
| <img src="docs/icons/dot-safe.svg" width="12"> **安全清理** | 超过一周的临时文件、浏览器缓存和显卡着色器缓存，以及应用会自行重新生成的其他缓存 | 直接删除。需要时应用会重新生成它们。默认已勾选。 |
| <img src="docs/icons/dot-dev.svg" width="12"> **开发残留文件** | `node_modules`、Python 环境（venv）、构建文件夹和软件包缓存 | 直接删除。用一条命令（如 `npm install`）就能恢复。 |
| <img src="docs/icons/dot-games.svg" width="12"> **游戏和应用** | 已安装的大型游戏和应用 | 用它们自带的卸载程序卸载，或在 Windows 设置中卸载。 |
| <img src="docs/icons/dot-review.svg" width="12"> **旧的大文件和文件夹** | 6 个月没动过的文件夹、旧的下载文件和视频 | 会移到回收站，所以你可以还原它们。 |
| <img src="docs/icons/dot-system.svg" width="12"> **Windows 和系统** | 页面文件（pagefile）、休眠文件、WinSxS、还原点 | 只通过 Windows 自带的工具修改，并会说明每项修改意味着什么。 |

## 内存优化

你可以从**内存和性能**页面、侧边栏的**优化**按钮，或托盘图标打开它。它会请每个应用交还占着却没在使用的内存；应用只会重新取回它们真正需要的部分，让游戏或大型应用有更多可用内存。

| 设置 | 作用 |
|---|---|
| <img src="docs/icons/bolt.svg" width="16"> **立即优化内存** | 立刻释放内存，并显示释放了多少。 |
| **当内存使用率达到……时** | 使用率超过你选定的上限（70% 到 95%，默认 85%）时自动运行，最多每 2 分钟一次。 |
| **每隔** | 按计时器运行：每隔 3、5、10、30 或 60 分钟一次。 |
| **仅在 CPU 空闲时** | 自动运行会等到处理器占用率低于 30% 再开始，所以不会影响游戏。默认开启。 |
| **释放应用占用的内存** | 清空每个应用的工作集：它占着却没在使用的内存页会交还给 Windows。默认开启。 |
| **清空 Windows 备用缓存** | 以管理员身份运行时，还会清空 Windows 在内存里保留的、最近用过的文件缓存。 |
| **清空剪贴板** | 清空你最后复制的内容，它可能是一张很大的图片。 |
| **每次自动优化后通知我** | 每次自动运行后，显示一条小通知。 |
| <img src="docs/icons/tray.svg" width="16"> **关闭窗口后继续在任务栏时钟旁运行** | 关闭窗口后，Faddi 会留在托盘里，继续监控内存并运行内存优化。默认开启。 |
| **随 Windows 启动，并在任务栏时钟旁运行** | 登录后在托盘中启动。 |

> 释放出来的内存，有一部分会在应用再次用到时被重新占用，这是正常的。Windows 自己就能把内存管得很好，所以内存优化最有用的时候，是在玩游戏或运行大型应用之前，或者某个应用占用了过多内存之后。

## 从设计上保证安全

- 在你点击**清理所选项**并确认之前，不会删除任何内容。个人文件（旧文件夹、下载文件、视频）会移到回收站；对于大到回收站放不下的内容，Windows 会先发出警告。
- 它从不跟随联接点（junction）或符号链接，所以清理 `node_modules` 时，不会碰到链接到别处的文件夹。
- Windows、Program Files、ProgramData\Microsoft，以及每个用户配置文件及其 Desktop、Documents 和 AppData 文件夹，都是受保护的位置，不会被清理；检查前，会先把 `C:\PROGRA~1` 这样的短名称解析成完整路径。
- Temp 会保留最近 7 天内改动过的所有内容，并跳过正在被应用使用的文件。Outlook 的附件缓存不会被触碰。
- 系统文件，包括页面文件（pagefile）、休眠文件、WinSxS 和还原点，只通过 Windows 自带的工具修改。
- 无法在「内存和性能」页面中关闭 Windows 的核心进程。

## 系统要求

- **Windows 10 或 11，64 位。**
- **Microsoft Edge WebView2 Runtime**：Windows 11 已内置，几乎每台 Windows 10 电脑上也已安装。
- **管理员权限**：可选。没有它，Faddi 也能正常使用；少数额外功能需要它。
- **NVIDIA GPU**：可选，用于显示 GPU 的实时温度、风扇和功耗。

## 面向开发者

需要 [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 和 Windows。

```bash
git clone https://github.com/sox29-ui/Faddi.git
cd Faddi
dotnet run --project Faddi           # 运行应用
dotnet run --project Faddi.Check     # 自检：在一次性文件夹上测试扫描器、清理规则和删除保护
node tools/i18n-check.mjs            # 检查每种语言是否都包含全部字符串及其 {0} 占位符
```

构建发行版附带的单文件 `Faddi.exe`：

```bash
dotnet publish Faddi -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none -o publish
```

### 项目结构

```text
Faddi/
  Faddi/                  应用本身
    Program.cs            窗口、托盘图标、单实例，以及页面与程序通信的桥梁
    Scanner.cs            并行扫描整个磁盘，得到磁盘上的实际大小
    Cleanup.cs            清理规则，以及决定哪些内容允许被删除的保护层
    Dupes.cs              重复文件查找：先对抽样内容做哈希，再做完整的 SHA-256
    Optimizer.cs          内存优化
    Watcher.cs            CPU、内存、GPU、磁盘和网络的采样，以及内存突增记录
    SysInfo.cs            来自 WMI、注册表和 nvidia-smi 的系统信息
    Settings.cs, I18n.cs  设置和翻译
    index.html            整个界面，只有一个页面
    lang/                 每种语言一个 JSON 文件
  Faddi.Check/            自检
  tools/i18n-check.mjs    翻译检查工具
  docs/                   此 README 用到的标志、图标和截图
```

### 技术栈

.NET 8 WinForms 承载一个 WebView2 页面，没有网页服务器，也不联网。扫描、内存优化和监控直接与 Windows 交互：扫描用 `FindFirstFileExW`，内存用 `EmptyWorkingSet` 和 `NtSetSystemInformation`，实时数据用 PDH 性能计数器，硬件信息用 WMI。界面是一个 HTML 文件，不使用任何框架。

### 添加语言

阿拉伯语直接写在代码里；其他每种语言都是 `Faddi/lang` 中的一个文件。把 `en.json` 复制为你的语言代码（例如 `de.json`），翻译其中的值，把 `"@name"` 设为该语言自己的名称，然后反复运行 `node tools/i18n-check.mjs`，直到它不再报告缺失或损坏的内容。新语言会出现在侧边栏底部的语言选择器中。

## 你的数据

- 设置保存在 `%LOCALAPPDATA%\Faddi\settings.json`，内存突增记录保存在它旁边的 `spikes.json`。
- Faddi 从不上传任何内容，没有账号、广告，也没有遥测。

## 参与贡献

欢迎提交 issue 和 pull request，尤其是新语言，以及 Faddi 还不认识的工具的清理规则。请先阅读[贡献指南](CONTRIBUTING.md)，并遵守[行为准则](CODE_OF_CONDUCT.md)。

发现了安全问题，比如能让 Faddi 删除不该删的东西？请按 [SECURITY.md](SECURITY.md) 中的说明私下报告。

## 许可证

Faddi 以 [MIT 许可证](LICENSE)发布。它附带的库保留各自的许可证，详见 [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md)。
