<p align="center">
  <img src="docs/banner.png" alt="Faddi · فضّي" width="100%">
</p>

<p align="center">
  <a href="README.md">English</a> &middot;
  <a href="README.ar.md">العربية</a> &middot;
  <b>Español</b> &middot;
  <a href="README.fr.md">Français</a> &middot;
  <a href="README.pt.md">Português</a> &middot;
  <a href="README.ru.md">Русский</a> &middot;
  <a href="README.tr.md">Türkçe</a> &middot;
  <a href="README.zh.md">简体中文</a>
</p>

<p align="center">
  <b>Encuentra lo que se está comiendo tu disco, límpialo con seguridad y mantén la RAM bajo control.</b><br>
  Una aplicación gratuita y de código abierto para Windows que lee toda tu unidad en alrededor de un minuto, ordena todo lo que encuentra según lo seguro que sea eliminarlo
  y libera la memoria que las aplicaciones retienen sin usarla. Un exe pequeño, sin nada que instalar.
</p>

<p align="center">
  <a href="https://github.com/sox29-ui/Faddi/releases/latest"><img alt="Última versión" src="https://img.shields.io/github/v/release/sox29-ui/Faddi?color=E8A33A&label=release"></a>
  <a href="https://github.com/sox29-ui/Faddi/releases"><img alt="Descargas" src="https://img.shields.io/github/downloads/sox29-ui/Faddi/total?color=3FBF8A"></a>
  <a href="LICENSE"><img alt="Licencia: MIT" src="https://img.shields.io/badge/license-MIT-2F6BDC"></a>
  <img alt="Windows 10 y 11" src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4">
  <img alt=".NET 8" src="https://img.shields.io/badge/.NET-8-512BD4">
  <img alt="8 idiomas" src="https://img.shields.io/badge/languages-8-B67FD8">
  <img alt="Sin telemetría" src="https://img.shields.io/badge/telemetry-none-555">
</p>

<p align="center">
  <a href="#instalación">Instalación</a> &middot;
  <a href="#funciones">Funciones</a> &middot;
  <a href="#capturas-de-pantalla">Capturas</a> &middot;
  <a href="#qué-limpia-faddi">Qué limpia</a> &middot;
  <a href="#el-optimizador-de-ram">Optimizador de RAM</a> &middot;
  <a href="#seguro-por-diseño">Seguro por diseño</a> &middot;
  <a href="#para-desarrolladores">Para desarrolladores</a> &middot;
  <a href="CONTRIBUTING.md">Contribuir</a>
</p>

<p align="center">
  <img src="docs/home-en.png" alt="Página de inicio de Faddi: una franja divide el disco en juegos, restos de desarrollo, cachés, cosas antiguas, Windows y espacio libre" width="900">
</p>

---

## ¿Qué es Faddi?

Faddi (فضّي, «libéralo» en árabe) te muestra exactamente dónde fue a parar el espacio de tu disco y te ayuda a recuperarlo sin romper nada. Lee toda la unidad, la divide en juegos y aplicaciones, restos de desarrollo, cachés, archivos antiguos y el propio Windows, y te dice qué es cada cosa y qué pasa si la eliminas.

- **Mira dónde se fue el espacio**: una franja para todo el disco, un mapa de cada carpeta, los archivos más grandes y los duplicados.
- **Limpia con seguridad**: las cachés ya vienen marcadas, todo lo demás espera tu decisión y los archivos personales van a la Papelera de reciclaje.
- **Mantén la RAM bajo control**: libera memoria con un clic o de forma automática, y averigua qué aplicación causó un salto repentino.
- **Conoce tu PC**: CPU, RAM, GPU, discos con su estado, red y aplicaciones de inicio, cada cosa explicada con palabras sencillas.

Todo se queda en tu PC. Faddi no envía nada por la red.

## Instalación

### Una línea en PowerShell (lo más fácil, sin pantalla de aviso)

Haz clic en Inicio, escribe `PowerShell`, presiona Enter, luego pega esta línea y presiona Enter:

```powershell
irm https://raw.githubusercontent.com/sox29-ui/Faddi/main/install.ps1 | iex
```

Descarga el último `Faddi.exe` en `%LOCALAPPDATA%\Programs\Faddi`, agrega Faddi al menú Inicio y al Escritorio, y lo abre. Para actualizar, ejecuta la misma línea otra vez.

### O descarga el exe

<p>
  <a href="https://github.com/sox29-ui/Faddi/releases/latest/download/Faddi.exe"><img alt="Descargar Faddi.exe" src="https://img.shields.io/badge/Descargar-Faddi.exe-E8A33A?style=for-the-badge"></a>
</p>

1. Descarga **Faddi.exe** desde la [última versión](https://github.com/sox29-ui/Faddi/releases/latest). Es un solo archivo de unos 66 MB con todo incluido, así que no hay nada que instalar.
2. Haz doble clic en el archivo. El análisis empieza de inmediato: cinco millones de archivos en un SSD NVMe tardan de 45 a 90 segundos, y la franja del disco se va completando mientras lee.

   Windows puede mostrar **«Windows protegió tu PC»** porque el exe es nuevo y no está firmado digitalmente. Haz clic en **Más información** y luego en **Ejecutar de todas formas**. La línea de PowerShell de arriba evita esta pantalla.
3. Opcional: haz clic en **Ejecutar como admin**, en la parte inferior de la barra lateral. Como administrador, Faddi puede leer carpetas protegidas, mostrar la temperatura y el desgaste del disco, limpiar los restos del propio Windows y vaciar la caché en espera.

> Faddi usa Microsoft Edge WebView2 Runtime, que viene con Windows 11 y ya está en casi todos los PC con Windows 10. Si falta, Faddi te avisa.<br>
> Para quitar Faddi, desactiva **Iniciar con Windows** en RAM y rendimiento. Después elimina `Faddi.exe` (o `%LOCALAPPDATA%\Programs\Faddi`), sus accesos directos y la carpeta `%LOCALAPPDATA%\Faddi`.

## Funciones

| Función | Qué puedes hacer |
|---|---|
| <img src="docs/icons/strip.svg" width="22"> **El disco de un vistazo** | Una franja divide la unidad en juegos y aplicaciones, restos de desarrollo, cachés y temporales, cosas antiguas y grandes, Windows y todo lo demás. |
| <img src="docs/icons/map.svg" width="22"> **Mapa de espacio** | Cada carpeta es un recuadro del tamaño del espacio que ocupa, con un color que indica cuándo cambió algo dentro por última vez. Haz clic en un recuadro para entrar. |
| <img src="docs/icons/clean.svg" width="22"> **Limpieza por nivel de seguridad** | Las cachés y los archivos temporales vienen marcados; los restos de desarrollo, los juegos grandes y las carpetas sin cambios desde hace 6 meses esperan tu decisión. Cada elemento dice qué es y qué pasa si lo eliminas. |
| <img src="docs/icons/code.svg" width="22"> **Restos de desarrollo** | `node_modules` junto a un lockfile, entornos venv de Python, `Library` de Unity, `DerivedDataCache` de Unreal, `bin`/`obj` de .NET, resultados de compilación de Gradle, Flutter y Rust, y las cachés de npm, pip, uv, NuGet, Gradle y Hugging Face. |
| <img src="docs/icons/files.svg" width="22"> **Archivos más grandes y tipos** | Los archivos más grandes de la unidad y qué tipos de archivo ocupan más espacio. |
| <img src="docs/icons/dupes.svg" width="22"> **Archivos duplicados** | Copias encontradas al comparar el contenido, no los nombres. Los enlaces físicos (hard links), que solo parecen copias, se dejan fuera. |
| <img src="docs/icons/apps.svg" width="22"> **Aplicaciones instaladas** | Cada aplicación con su tamaño real en disco y un botón que ejecuta su propio desinstalador. |
| <img src="docs/icons/bolt.svg" width="22"> **Optimizador de RAM** | Libera memoria con un clic, o de forma automática cuando el uso supera un límite o cada pocos minutos, solo mientras la CPU está inactiva. [Más abajo](#el-optimizador-de-ram). |
| <img src="docs/icons/ram.svg" width="22"> **Monitor de RAM** | Mide la RAM cada 2 segundos y registra los saltos repentinos junto con las aplicaciones que los causan, además de sugerencias como un límite para WSL, un controlador con fuga de memoria o un daemon de Gradle que quedó activo. |
| <img src="docs/icons/sys.svg" width="22"> **Info del sistema** | CPU, módulos de RAM, GPU (con temperatura en tiempo real en modelos NVIDIA), discos con estado de salud, temperatura y desgaste, red, pantallas, batería y aplicaciones de inicio, cada uno con una breve explicación. |
| <img src="docs/icons/tray.svg" width="22"> **Junto al reloj** | Al cerrar la ventana, Faddi se queda en la bandeja vigilando la RAM. Puede iniciarse con Windows. |
| <img src="docs/icons/globe.svg" width="22"> **Ocho idiomas** | English, العربية, Español, Français, Português, Русский, Türkçe y 简体中文, con escritura de derecha a izquierda para el árabe. Se abre en el idioma de tu Windows. |

## Capturas de pantalla

<table>
  <tr>
    <td width="50%"><img src="docs/home-en.png" alt="Inicio"><p align="center">Inicio: todo el disco en una sola franja</p></td>
    <td width="50%"><img src="docs/map-en.png" alt="Mapa de espacio"><p align="center">Mapa de espacio, con color según la antigüedad</p></td>
  </tr>
  <tr>
    <td><img src="docs/clean-en.png" alt="Limpieza"><p align="center">Limpieza, ordenada según su seguridad</p></td>
    <td><img src="docs/ram-en.png" alt="Optimizador de RAM"><p align="center">Optimizador de RAM</p></td>
  </tr>
  <tr>
    <td><img src="docs/sys-en.png" alt="Info del sistema"><p align="center">Info del sistema, en tiempo real</p></td>
    <td><img src="docs/home-ar.png" alt="Inicio en árabe"><p align="center">Árabe, de derecha a izquierda</p></td>
  </tr>
  <tr>
    <td><img src="docs/home-zh.png" alt="Inicio en chino"><p align="center">简体中文</p></td>
    <td><img src="docs/home-ru.png" alt="Inicio en ruso"><p align="center">Русский</p></td>
  </tr>
</table>

## Qué limpia Faddi

Todo lo que encuentra Faddi cae en uno de cinco grupos, con los mismos colores que la franja del disco.

| Grupo | Qué incluye | Qué pasa cuando lo limpias |
|---|---|---|
| <img src="docs/icons/dot-safe.svg" width="12"> **Limpieza segura** | Archivos temporales de más de una semana, cachés de navegadores y de sombreadores gráficos, y otras cachés que las aplicaciones vuelven a crear | Se eliminan. Las aplicaciones los vuelven a crear cuando hace falta. Ya vienen marcados. |
| <img src="docs/icons/dot-dev.svg" width="12"> **Restos de desarrollo** | `node_modules`, entornos venv, carpetas de compilación y cachés de paquetes | Se eliminan. Se recuperan con un solo comando, como `npm install`. |
| <img src="docs/icons/dot-games.svg" width="12"> **Juegos y aplicaciones** | Juegos y aplicaciones grandes instalados | Se quitan con su propio desinstalador, o desde Configuración de Windows. |
| <img src="docs/icons/dot-review.svg" width="12"> **Cosas antiguas y grandes** | Carpetas sin cambios desde hace 6 meses, descargas antiguas y videos | Se mueven a la Papelera de reciclaje, así que puedes restaurarlos. |
| <img src="docs/icons/dot-system.svg" width="12"> **Windows y sistema** | Pagefile (archivo de paginación), archivo de hibernación, WinSxS, puntos de restauración | Solo se cambian con las propias herramientas de Windows, y Faddi te explica qué implica cada cambio. |

## El optimizador de RAM

Ábrelo desde **RAM y rendimiento**, desde el botón **Optimizar** de la barra lateral o desde el icono de la bandeja. Pide a cada aplicación que devuelva la memoria que retiene sin usarla; las aplicaciones solo recuperan lo que de verdad necesitan, así que un juego o una aplicación pesada tiene más espacio.

| Opción | Qué hace |
|---|---|
| <img src="docs/icons/bolt.svg" width="16"> **Optimizar RAM ahora** | Libera memoria al instante y muestra cuánta se recuperó. |
| **Cuando el uso de RAM llegue a** | Se ejecuta por sí solo cuando el uso supera el límite que elijas (entre 70% y 95%; 85% por defecto), como máximo una vez cada 2 minutos. |
| **Cada** | Se ejecuta con un temporizador: cada 3, 5, 10, 30 o 60 minutos. |
| **Solo cuando la CPU esté inactiva** | Las ejecuciones automáticas esperan a que el procesador esté por debajo del 30%, así que nunca interfieren con un juego. Activada por defecto. |
| **Liberar la RAM que retienen las aplicaciones** | Vacía el conjunto de trabajo de cada aplicación: las páginas que retiene sin usarlas vuelven a Windows. Activada por defecto. |
| **Vaciar la caché en espera de Windows** | Como administrador, también vacía la caché de archivos usados recientemente que Windows guarda en la RAM. |
| **Borrar el portapapeles** | Borra lo último que copiaste, que puede ser una imagen grande. |
| **Avisarme** | Muestra una notificación pequeña después de cada ejecución automática. |
| <img src="docs/icons/tray.svg" width="16"> **Seguir en ejecución junto al reloj** | Al cerrar la ventana, Faddi se queda en la bandeja, vigilando la RAM y ejecutando el optimizador. Activada por defecto. |
| **Iniciar con Windows** | Arranca en la bandeja cuando inicias sesión. |

> La memoria liberada se vuelve a llenar en parte cuando las aplicaciones la usan de nuevo; es normal. Windows ya gestiona bien la RAM por su cuenta, así que el optimizador ayuda más justo antes de abrir un juego o una aplicación pesada, o después de que algo haya estado acaparando memoria.

## Seguro por diseño

- No se elimina nada hasta que hagas clic en **Limpiar selección** y confirmes. Los archivos personales (carpetas antiguas, descargas, videos) van a la Papelera de reciclaje, y Windows avisa antes de eliminar algo que no cabe en la Papelera.
- Nunca sigue junctions ni enlaces simbólicos, así que limpiar `node_modules` no puede llegar a una carpeta enlazada en otro lugar.
- Windows, Program Files, ProgramData\Microsoft, cada perfil de usuario y sus carpetas Escritorio, Documentos y AppData quedan blindados, y los nombres cortos como `C:\PROGRA~1` se resuelven antes de comprobar.
- En Temp se conserva todo lo que cambió en los últimos 7 días y se omiten los archivos que usa una aplicación en ejecución. La caché de archivos adjuntos de Outlook no se toca.
- Los archivos del sistema (pagefile, archivo de hibernación, WinSxS, puntos de restauración) solo se cambian con las propias herramientas de Windows.
- Los procesos esenciales de Windows no se pueden cerrar desde la página de RAM.

## Requisitos

- **Windows 10 u 11, de 64 bits.**
- **Microsoft Edge WebView2 Runtime**: incluido en Windows 11 y ya presente en casi todos los PC con Windows 10.
- **Permisos de administrador**: opcionales. Todo funciona sin ellos; unos pocos extras los necesitan.
- **GPU NVIDIA**: opcional, para ver en tiempo real la temperatura, el ventilador y la potencia de la GPU.

## Para desarrolladores

Necesitas el [SDK de .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) y Windows.

```bash
git clone https://github.com/sox29-ui/Faddi.git
cd Faddi
dotnet run --project Faddi           # ejecuta la aplicación
dotnet run --project Faddi.Check     # autocomprobaciones: escáner, reglas de limpieza y la protección contra eliminación, sobre carpetas desechables
node tools/i18n-check.mjs            # todos los idiomas tienen todas las cadenas, con sus marcadores {0}
```

Compila el `Faddi.exe` de un solo archivo que se distribuye en cada versión:

```bash
dotnet publish Faddi -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none -o publish
```

### Estructura del proyecto

```text
Faddi/
  Faddi/                  la aplicación
    Program.cs            ventana, icono de la bandeja, instancia única y el puente con el que habla la página
    Scanner.cs            análisis paralelo de toda la unidad con tamaños reales en disco
    Cleanup.cs            reglas de limpieza y la protección que decide qué se puede eliminar
    Dupes.cs              buscador de duplicados: primero un hash por muestreo, después SHA-256 completo
    Optimizer.cs          optimizador de RAM
    Watcher.cs            muestreo de CPU, RAM, GPU, disco y red, y el registro de saltos de RAM
    SysInfo.cs            información del sistema desde WMI, el registro y nvidia-smi
    Settings.cs, I18n.cs  configuración y traducciones
    index.html            toda la interfaz, en una sola página
    lang/                 un archivo JSON por idioma
  Faddi.Check/            autocomprobaciones
  tools/i18n-check.mjs    comprobador de traducciones
  docs/                   logo, iconos y capturas de este README
```

### Tecnologías

.NET 8 WinForms que aloja una página de WebView2, sin servidor web y sin red. El análisis, el optimizador y el monitor hablan directamente con Windows: `FindFirstFileExW` para el análisis, `EmptyWorkingSet` y `NtSetSystemInformation` para la memoria, los contadores de rendimiento PDH para las cifras en tiempo real y WMI para el hardware. La interfaz es un único archivo HTML, sin framework.

### Agregar un idioma

El árabe está escrito directamente en el código; cada uno de los demás idiomas es un archivo en `Faddi/lang`. Copia `en.json` y nómbralo con el código de tu idioma (por ejemplo `de.json`), traduce los valores, pon en `"@name"` el nombre del idioma escrito en ese mismo idioma y ejecuta `node tools/i18n-check.mjs` hasta que no informe de nada que falte o esté roto. El idioma nuevo aparece en el selector, al final de la barra lateral.

## Tus datos

- La configuración está en `%LOCALAPPDATA%\Faddi\settings.json` y el registro de saltos de RAM en `spikes.json`, en la misma carpeta.
- Faddi nunca sube nada y no tiene cuentas, anuncios ni telemetría.

## Contribuir

Los issues y los pull requests son bienvenidos, sobre todo los idiomas nuevos y las reglas de limpieza para herramientas que Faddi aún no conoce. Empieza por la [guía de contribución](CONTRIBUTING.md) y sigue el [código de conducta](CODE_OF_CONDUCT.md).

¿Encontraste un problema de seguridad, como una forma de hacer que Faddi elimine algo que no debería? Infórmalo en privado como se describe en [SECURITY.md](SECURITY.md).

## Licencia

Faddi se publica bajo la [Licencia MIT](LICENSE). Las bibliotecas que incluye conservan sus propias licencias, indicadas en [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
