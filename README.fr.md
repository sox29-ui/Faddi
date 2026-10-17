<p align="center">
  <img src="docs/banner.png" alt="Faddi · فضّي" width="100%">
</p>

<p align="center">
  <a href="README.md">English</a> &middot;
  <a href="README.ar.md">العربية</a> &middot;
  <a href="README.es.md">Español</a> &middot;
  <b>Français</b> &middot;
  <a href="README.pt.md">Português</a> &middot;
  <a href="README.ru.md">Русский</a> &middot;
  <a href="README.tr.md">Türkçe</a> &middot;
  <a href="README.zh.md">简体中文</a>
</p>

<p align="center">
  <b>Trouvez ce qui mange la place sur votre disque, nettoyez-le en toute sécurité et gardez votre RAM sous contrôle.</b><br>
  Une application Windows gratuite et open source qui lit tout votre disque en environ une minute, classe tout ce qu’elle trouve selon qu’il est plus ou moins sûr de le supprimer,
  et libère la mémoire que les applications retiennent sans l’utiliser. Un seul petit exe, rien à installer.
</p>

<p align="center">
  <a href="https://github.com/sox29-ui/Faddi/releases/latest"><img alt="Dernière version" src="https://img.shields.io/github/v/release/sox29-ui/Faddi?color=E8A33A&label=release"></a>
  <a href="https://github.com/sox29-ui/Faddi/releases"><img alt="Téléchargements" src="https://img.shields.io/github/downloads/sox29-ui/Faddi/total?color=3FBF8A"></a>
  <a href="LICENSE"><img alt="Licence : MIT" src="https://img.shields.io/badge/license-MIT-2F6BDC"></a>
  <img alt="Windows 10 et 11" src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4">
  <img alt=".NET 8" src="https://img.shields.io/badge/.NET-8-512BD4">
  <img alt="8 langues" src="https://img.shields.io/badge/languages-8-B67FD8">
  <img alt="Aucune télémétrie" src="https://img.shields.io/badge/telemetry-none-555">
</p>

<p align="center">
  <a href="#installation">Installation</a> &middot;
  <a href="#fonctionnalités">Fonctionnalités</a> &middot;
  <a href="#captures-décran">Captures d’écran</a> &middot;
  <a href="#ce-que-faddi-nettoie">Ce qu’il nettoie</a> &middot;
  <a href="#optimiseur-de-ram">Optimiseur de RAM</a> &middot;
  <a href="#sécurité-dès-la-conception">Sécurité</a> &middot;
  <a href="#pour-les-développeurs">Pour les développeurs</a> &middot;
  <a href="CONTRIBUTING.md">Contribuer</a>
</p>

<p align="center">
  <img src="docs/home-en.png" alt="Page d’accueil de Faddi : une barre répartit le disque entre les jeux, les restes de développement, les caches, les éléments anciens, Windows et l’espace libre" width="900">
</p>

---

## Qu’est-ce que Faddi ?

Faddi (فضّي, « libérez de la place » en arabe) vous montre exactement où est passé votre espace disque et vous aide à le récupérer sans rien casser. Il lit tout le disque, le répartit en jeux et applications, restes de développement, caches, anciens fichiers et Windows lui-même, puis vous dit ce qu’est chaque élément et ce qui se passe si vous le supprimez.

- **Voir où est passé l’espace** : une seule barre pour tout le disque, une carte de chaque dossier, les plus gros fichiers et les doublons.
- **Nettoyer sans risque** : les caches sont déjà cochés, tout le reste attend votre choix, et les fichiers personnels vont dans la Corbeille.
- **Garder la RAM sous contrôle** : libérez de la mémoire en un clic ou automatiquement, et voyez quelle application a provoqué un pic soudain.
- **Connaître votre PC** : processeur, RAM, carte graphique, disques avec leur état de santé, réseau et applications de démarrage, le tout expliqué en mots simples.

Tout reste sur votre PC. Faddi n’envoie rien sur le réseau.

## Installation

### Une seule ligne dans PowerShell (le plus simple, sans écran d’avertissement)

Cliquez sur Démarrer, tapez `PowerShell`, appuyez sur Entrée, puis collez cette ligne et appuyez de nouveau sur Entrée :

```powershell
irm https://raw.githubusercontent.com/sox29-ui/Faddi/main/install.ps1 | iex
```

Elle télécharge la dernière version de `Faddi.exe` dans `%LOCALAPPDATA%\Programs\Faddi`, ajoute Faddi au menu Démarrer et au Bureau, puis l’ouvre. Exécutez de nouveau la même ligne pour mettre à jour.

### Ou téléchargez l’exe

<p>
  <a href="https://github.com/sox29-ui/Faddi/releases/latest/download/Faddi.exe"><img alt="Télécharger Faddi.exe" src="https://img.shields.io/badge/T%C3%A9l%C3%A9charger-Faddi.exe-E8A33A?style=for-the-badge"></a>
</p>

1. Téléchargez **Faddi.exe** depuis la [dernière version](https://github.com/sox29-ui/Faddi/releases/latest). C’est un seul fichier d’environ 66 MB qui contient tout, donc il n’y a rien à installer.
2. Double-cliquez dessus. L’analyse démarre tout de suite : cinq millions de fichiers sur un SSD NVMe prennent de 45 à 90 secondes, et la barre du disque se remplit pendant la lecture.

   Windows peut afficher **« Windows a protégé votre ordinateur »**, car l’exe est récent et n’a pas de signature de code. Cliquez sur **Informations complémentaires**, puis sur **Exécuter quand même**. La ligne PowerShell ci-dessus évite cet écran.
3. Facultatif : cliquez sur **Exécuter en admin** en bas de la barre latérale. En administrateur, Faddi peut lire les dossiers protégés, afficher la température et l’usure des disques, nettoyer les restes de Windows lui-même et vider le cache de Windows (Standby).

> Faddi utilise Microsoft Edge WebView2 Runtime, fourni avec Windows 11 et déjà présent sur presque tous les PC Windows 10. S’il est absent, Faddi vous le signale.<br>
> Pour supprimer Faddi, désactivez **Démarrer avec Windows** dans RAM et performances, puis supprimez `Faddi.exe` (ou `%LOCALAPPDATA%\Programs\Faddi`), ses raccourcis et le dossier `%LOCALAPPDATA%\Faddi`.

## Fonctionnalités

| Fonctionnalité | Ce que vous pouvez faire |
|---|---|
| <img src="docs/icons/strip.svg" width="22"> **Le disque d’un coup d’œil** | Une seule barre répartit le disque entre les jeux et applications, les restes de développement, les caches et temporaires, les éléments anciens et volumineux, Windows et tout le reste. |
| <img src="docs/icons/map.svg" width="22"> **Carte de l’espace** | Chaque dossier est une case, proportionnelle à l’espace qu’il occupe et colorée selon le temps écoulé depuis le dernier changement à l’intérieur. Cliquez sur une case pour y entrer. |
| <img src="docs/icons/clean.svg" width="22"> **Nettoyage par niveau de sécurité** | Les caches et fichiers temporaires sont présélectionnés ; les restes de développement, les gros jeux et les dossiers inchangés depuis 6 mois attendent votre choix. Chaque élément indique ce qu’il est et ce qui se passe si vous le supprimez. |
| <img src="docs/icons/code.svg" width="22"> **Restes de développement** | Dossiers `node_modules` à côté d’un lockfile, environnements Python (venv), dossiers `Library` d’Unity, `DerivedDataCache` d’Unreal, `bin`/`obj` de .NET, dossiers de build de Gradle, Flutter et Rust, ainsi que les caches de npm, pip, uv, NuGet, Gradle et Hugging Face. |
| <img src="docs/icons/files.svg" width="22"> **Plus gros fichiers et types** | Les plus gros fichiers du disque, et les types de fichiers qui prennent le plus de place. |
| <img src="docs/icons/dupes.svg" width="22"> **Fichiers en double** | Des copies repérées en comparant le contenu, pas les noms. Les liens physiques, qui ne font que ressembler à des copies, sont exclus. |
| <img src="docs/icons/apps.svg" width="22"> **Applications installées** | Chaque application avec sa taille réelle sur le disque, et un bouton qui lance son propre programme de désinstallation. |
| <img src="docs/icons/bolt.svg" width="22"> **Optimiseur de RAM** | Libère de la mémoire en un clic, ou tout seul quand l’utilisation dépasse une limite ou toutes les quelques minutes, uniquement quand le processeur est inactif. [Détails plus bas](#optimiseur-de-ram). |
| <img src="docs/icons/ram.svg" width="22"> **Surveillance de la RAM** | Mesure la RAM toutes les 2 secondes et enregistre les pics soudains avec les applications responsables, plus des conseils comme une limite pour WSL, un pilote qui fuit ou un daemon Gradle resté actif. |
| <img src="docs/icons/sys.svg" width="22"> **Infos système** | Processeur, barrettes de RAM, carte graphique avec température en direct sur NVIDIA, disques avec état de santé, température et usure, réseau, écrans, batterie et applications de démarrage, chacun avec une courte explication. |
| <img src="docs/icons/tray.svg" width="22"> **Près de l’horloge** | Quand vous fermez la fenêtre, Faddi reste dans la zone de notification et continue de surveiller la RAM. Il peut démarrer avec Windows. |
| <img src="docs/icons/globe.svg" width="22"> **Huit langues** | English, العربية, Español, Français, Português, Русский, Türkçe et 简体中文, avec l’écriture de droite à gauche pour l’arabe. Faddi s’ouvre dans la langue de Windows. |

## Captures d’écran

<table>
  <tr>
    <td width="50%"><img src="docs/home-en.png" alt="Accueil"><p align="center">Accueil : tout le disque en une seule barre</p></td>
    <td width="50%"><img src="docs/map-en.png" alt="Carte de l’espace"><p align="center">Carte de l’espace, colorée selon l’ancienneté</p></td>
  </tr>
  <tr>
    <td><img src="docs/clean-en.png" alt="Nettoyage"><p align="center">Nettoyage, classé selon le niveau de sécurité</p></td>
    <td><img src="docs/ram-en.png" alt="Optimiseur de RAM"><p align="center">Optimiseur de RAM</p></td>
  </tr>
  <tr>
    <td><img src="docs/sys-en.png" alt="Infos système"><p align="center">Infos système, en direct</p></td>
    <td><img src="docs/home-ar.png" alt="Accueil en arabe"><p align="center">Arabe, de droite à gauche</p></td>
  </tr>
  <tr>
    <td><img src="docs/home-zh.png" alt="Accueil en chinois"><p align="center">简体中文</p></td>
    <td><img src="docs/home-ru.png" alt="Accueil en russe"><p align="center">Русский</p></td>
  </tr>
</table>

## Ce que Faddi nettoie

Tout ce que Faddi trouve est rangé dans l’un des cinq groupes ci-dessous, avec les mêmes couleurs que la barre du disque.

| Groupe | Ce qu’il contient | Ce qui se passe quand vous le nettoyez |
|---|---|---|
| <img src="docs/icons/dot-safe.svg" width="12"> **Nettoyage sans risque** | Fichiers temporaires de plus d’une semaine, caches des navigateurs et de shaders graphiques, et autres caches que les applications reconstruisent | Supprimés. Les applications les recréent au besoin. Présélectionnés. |
| <img src="docs/icons/dot-dev.svg" width="12"> **Restes de développement** | `node_modules`, environnements venv, dossiers de build et caches de packages | Supprimés. Ils reviennent avec une seule commande, comme `npm install`. |
| <img src="docs/icons/dot-games.svg" width="12"> **Jeux et applications** | Gros jeux et applications installés | Retirés avec leur propre programme de désinstallation, ou depuis les Paramètres Windows. |
| <img src="docs/icons/dot-review.svg" width="12"> **Éléments anciens et volumineux** | Dossiers inchangés depuis 6 mois, anciens téléchargements et vidéos | Placés dans la Corbeille, pour que vous puissiez les restaurer. |
| <img src="docs/icons/dot-system.svg" width="12"> **Windows et système** | Pagefile, fichier de mise en veille prolongée, WinSxS, points de restauration | Modifiés uniquement avec les outils propres à Windows, en expliquant ce que chaque changement implique. |

## Optimiseur de RAM

Ouvrez-le depuis **RAM et performances**, le bouton **Optimiser** de la barre latérale ou l’icône de la zone de notification. Il demande à chaque application de rendre la mémoire qu’elle retient sans l’utiliser ; les applications ne reprennent que ce dont elles ont réellement besoin, ce qui laisse plus de place à un jeu ou à une application lourde.

| Paramètre | Ce qu’il fait |
|---|---|
| <img src="docs/icons/bolt.svg" width="16"> **Optimiser la RAM maintenant** | Libère de la mémoire tout de suite et indique combien a été récupéré. |
| **Quand l’utilisation de la RAM atteint** | Lance l’optimisation toute seule quand l’utilisation dépasse la limite choisie (de 70 à 95%, 85% par défaut), au plus une fois toutes les 2 minutes. |
| **Toutes les** | Lance l’optimisation selon un minuteur : toutes les 3, 5, 10, 30 ou 60 minutes. |
| **Seulement quand le processeur est inactif** | Les optimisations automatiques attendent que le processeur soit sous les 30%, donc elles ne ralentissent jamais un jeu. Activé par défaut. |
| **Libérer la RAM retenue par les applications** | Vide le jeu de travail de chaque application : les pages qu’elle retient sans les utiliser sont rendues à Windows. Activé par défaut. |
| **Vider le cache de Windows (Standby)** | En administrateur, vide aussi le cache des fichiers récemment utilisés que Windows garde en RAM. |
| **Vider le presse-papiers** | Vide le contenu copié en dernier, qui peut être une grande image. |
| **Me prévenir** | Affiche une petite notification après chaque optimisation automatique. |
| <img src="docs/icons/tray.svg" width="16"> **Rester actif près de l’horloge** | Quand vous fermez la fenêtre, Faddi reste dans la zone de notification, où il surveille toujours la RAM et lance l’optimiseur. Activé par défaut. |
| **Démarrer avec Windows** | Démarre dans la zone de notification à l’ouverture de votre session. |

> La mémoire libérée se remplit en partie de nouveau quand les applications s’en resservent ; c’est normal. Windows gère bien la RAM tout seul, donc l’optimiseur sert surtout juste avant un jeu ou une application lourde, ou après qu’une application a accaparé la mémoire.

## Sécurité dès la conception

- Rien n’est supprimé tant que vous n’avez pas cliqué sur **Nettoyer la sélection** et confirmé. Les fichiers personnels (anciens dossiers, téléchargements, vidéos) vont dans la Corbeille, et Windows vous prévient avant de supprimer ce qui est trop gros pour la Corbeille.
- Faddi ne suit jamais les jonctions ni les liens symboliques, donc nettoyer `node_modules` ne peut pas atteindre un dossier lié situé ailleurs.
- Windows, Program Files, ProgramData\Microsoft, chaque profil utilisateur avec ses dossiers Bureau, Documents et AppData sont hors limites, et les noms courts comme `C:\PROGRA~1` sont résolus avant la vérification.
- Le dossier Temp conserve tout ce qui a été modifié ces 7 derniers jours, et Faddi ignore les fichiers qu’une application en cours d’exécution utilise. Le cache des pièces jointes d’Outlook n’est pas touché.
- Les fichiers système (pagefile, fichier de mise en veille prolongée, WinSxS, points de restauration) ne sont modifiés qu’avec les outils propres à Windows.
- Les processus essentiels de Windows ne peuvent pas être fermés depuis la page RAM et performances.

## Configuration requise

- **Windows 10 ou 11, 64 bits.**
- **Microsoft Edge WebView2 Runtime** : intégré à Windows 11 et déjà présent sur presque tous les PC Windows 10.
- **Droits d’administrateur** : facultatifs. Tout fonctionne sans eux ; quelques fonctions supplémentaires en ont besoin.
- **Carte graphique NVIDIA** : facultative, pour afficher en direct sa température, son ventilateur et sa puissance.

## Pour les développeurs

Nécessite le [SDK .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) et Windows.

```bash
git clone https://github.com/sox29-ui/Faddi.git
cd Faddi
dotnet run --project Faddi           # lance l’application
dotnet run --project Faddi.Check     # vérifications automatiques (analyseur, règles de nettoyage, garde-fou de suppression) sur des dossiers jetables
node tools/i18n-check.mjs            # chaque langue a toutes les chaînes, avec leurs marqueurs {0}
```

Générez le `Faddi.exe` unique fourni dans les versions publiées :

```bash
dotnet publish Faddi -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none -o publish
```

### Structure du projet

```text
Faddi/
  Faddi/                  l’application
    Program.cs            fenêtre, icône de la zone de notification, instance unique, et le pont avec lequel la page communique
    Scanner.cs            analyse parallèle de tout le disque, avec les tailles réelles sur disque
    Cleanup.cs            règles de nettoyage, et le garde-fou qui décide de ce qui peut être supprimé
    Dupes.cs              recherche de doublons, d’abord par hachage d’échantillons, puis par SHA-256 complet
    Optimizer.cs          optimiseur de RAM
    Watcher.cs            mesures du processeur, de la RAM, de la carte graphique, du disque et du réseau, et journal des pics de RAM
    SysInfo.cs            infos système tirées de WMI, du registre et de nvidia-smi
    Settings.cs, I18n.cs  paramètres et traductions
    index.html            toute l’interface, en une seule page
    lang/                 un fichier JSON par langue
  Faddi.Check/            vérifications automatiques
  tools/i18n-check.mjs    vérificateur de traductions
  docs/                   logo, icônes et captures d’écran pour ce README
```

### Technologies utilisées

.NET 8 WinForms qui héberge une page WebView2, sans serveur web et sans réseau. L’analyse, l’optimiseur et la surveillance parlent directement à Windows : `FindFirstFileExW` pour l’analyse, `EmptyWorkingSet` et `NtSetSystemInformation` pour la mémoire, les compteurs de performance PDH pour les chiffres en direct, et WMI pour le matériel. L’interface est un seul fichier HTML, sans framework.

### Ajouter une langue

L’arabe est écrit directement dans le code ; chaque autre langue est un fichier dans `Faddi/lang`. Copiez `en.json` dans un fichier nommé d’après le code de votre langue (par exemple `de.json`), traduisez les valeurs, renseignez `"@name"` avec le nom de la langue écrit dans cette langue, puis lancez `node tools/i18n-check.mjs` jusqu’à ce qu’il ne signale plus rien de manquant ni de cassé. La nouvelle langue apparaît dans le sélecteur de langue, en bas de la barre latérale.

## Vos données

- Les paramètres se trouvent dans `%LOCALAPPDATA%\Faddi\settings.json` et le journal des pics de RAM dans `spikes.json`, juste à côté.
- Faddi n’envoie jamais rien, et n’a ni compte, ni publicité, ni télémétrie.

## Contribuer

Les issues et les pull requests sont les bienvenues, surtout les nouvelles langues et les règles de nettoyage pour des outils que Faddi ne connaît pas encore. Commencez par le [guide de contribution](CONTRIBUTING.md) et merci de respecter le [code de conduite](CODE_OF_CONDUCT.md).

Vous avez trouvé un problème de sécurité, par exemple un moyen de faire supprimer à Faddi quelque chose qu’il ne devrait pas ? Merci de le signaler en privé, comme décrit dans [SECURITY.md](SECURITY.md).

## Licence

Faddi est publié sous la [licence MIT](LICENSE). Les bibliothèques fournies avec lui gardent leurs propres licences, indiquées dans [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
