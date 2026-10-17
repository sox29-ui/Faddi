# Security policy

## Supported versions

Only the [latest release](https://github.com/sox29-ui/Faddi/releases/latest) gets fixes. Please check that you're on it before reporting.

## What counts as a security problem

Faddi deletes files and can run as administrator, so these matter most:

- Faddi deletes, moves or empties something you didn't select, or something its guard should protect (Windows, Program Files, user profiles and their main folders).
- Cleaning follows a junction or symbolic link into a folder somewhere else.
- A way to make Faddi run a program or code it shouldn't, especially while it runs as administrator.
- Anything that sends data off your PC. Faddi should never use the network.

## How to report

Please don't open a public issue. Use **[Report a vulnerability](https://github.com/sox29-ui/Faddi/security/advisories/new)** in the Security tab; only the maintainer can see it. Include:

- your Faddi version and Windows version,
- what you did, what happened, and what you expected,
- the folders or files needed to make it happen again, if any.

You'll get a reply as soon as possible. The fix ships in a new release, and the advisory is published afterwards, crediting you if you'd like.
