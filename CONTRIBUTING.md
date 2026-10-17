# Contributing to Faddi

Thanks for helping. Faddi touches people's files, so the bar is simple: every change has to be safe, clear and tested.

## Ways to help

- **Report a bug** with the [bug form](https://github.com/sox29-ui/Faddi/issues/new?template=bug_report.yml). If Faddi deleted or changed something it shouldn't have, report it privately instead, as described in [SECURITY.md](SECURITY.md).
- **Suggest an idea** with the [feature form](https://github.com/sox29-ui/Faddi/issues/new?template=feature_request.yml).
- **Translate**: fix a word in an existing language or add a new one ([below](#translations)).
- **Teach Faddi a new kind of junk**: a cache or build folder from a tool it doesn't know yet ([below](#cleanup-rules)).
- **Write code**: pick an open issue, or open one first for anything bigger than a small fix so we can agree on the approach.

## Set up

You need Windows 10 or 11, the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and [Node.js](https://nodejs.org) (for the translation checker).

```bash
git clone https://github.com/sox29-ui/Faddi.git
cd Faddi
dotnet run --project Faddi
```

The interface is one page, `Faddi/index.html`, inside a WebView2. To inspect it, set `FADDI_DEVTOOLS=1` before starting the app and press F12 in its window.

## Before you send a pull request

Run the same checks CI runs:

```bash
dotnet build Faddi.Check
dotnet run --project Faddi.Check     # scanner, cleanup rules and the delete guard, on throwaway folders
node tools/i18n-check.mjs            # every language has every string, with its {0} placeholders
```

Then try your change in the app, and add screenshots to the pull request for anything you can see.

## Cleanup rules

Rules live in `Faddi/Cleanup.cs`, and every deletion goes through the guard in `Files` in the same file. A new rule has to:

- match only what it describes, by folder name and by a file that proves what it is (a `node_modules` folder counts only next to a lockfile, for example);
- say in plain words what the item is and what happens if it's deleted;
- send anything personal to the Recycle Bin instead of deleting it;
- never follow junctions or symbolic links, and never touch anything the guard protects;
- come with a self-check in `Faddi.Check/Program.cs` that builds a throwaway folder and shows the rule finds the right thing and leaves the rest alone.

## Translations

The app's text is written in Arabic in the code (`T("…")` in C#, `t('…')` in `index.html`); every other language is one file in `Faddi/lang` that maps the Arabic text to its translation.

- **Fix a string**: edit the value in `Faddi/lang/<code>.json`.
- **Add a language**: copy `en.json` to your language code, for example `de.json`. Translate the values (not the keys), set `"@name"` to the language's own name (`"Deutsch"`), and keep every `{0}` placeholder and `<b>` tag. The language shows up in the picker by itself, and Faddi opens in it on Windows set to that language.
- **Translate the README**: copy `README.md` to `README.<code>.md`, translate it, and add the language to the switcher at the top of every README.

Run `node tools/i18n-check.mjs` until your file reports nothing missing or broken, then check the longest screens in the app (Cleanup and RAM & performance) for text that doesn't fit.

## Style

- Match the code around you: its naming, its comment density, its patterns.
- Comments explain why, not what.
- Interface text is plain and short: name things the way users would, describe what a button does, and don't apologize in errors; say what happened and how to fix it.
- Keep the app offline. Faddi doesn't make network requests.

## Commits and pull requests

- One topic per pull request, with a short description of what changed and why.
- Commit messages in the imperative: "Add Rust target folders to developer leftovers".
- By contributing, you agree your work is released under the project's [MIT License](LICENSE), and you follow the [code of conduct](CODE_OF_CONDUCT.md).
