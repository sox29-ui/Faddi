// Checks that every language file under Faddi/lang translates every Arabic UI string.
// Arabic is the source: '...' literals in index.html and T("...") calls in the C# files.
//   node tools/i18n-check.mjs          -> report missing / unused keys, exit 1 if any is missing
//   node tools/i18n-check.mjs --keys   -> print every key as a JSON object (handy to start a new language)
import { readFileSync, readdirSync } from 'node:fs';

const app = new URL('../Faddi/', import.meta.url);
const arabic = /[؀-ۿ]/;
// Arabic grammar the code handles itself, never looked up
const notKeys = new Set(['قبل ', 'ص', 'م', 'العربية']);

const keys = new Set();
const html = readFileSync(new URL('index.html', app), 'utf8');
for (const m of html.matchAll(/'((?:[^'\\\n]|\\.)*)'/g)) if (arabic.test(m[1]) && !notKeys.has(m[1])) keys.add(m[1].replace(/\\(.)/g, '$1'));
for (const file of readdirSync(app).filter(f => f.endsWith('.cs'))) {
  const cs = readFileSync(new URL(file, app), 'utf8');
  for (const m of cs.matchAll(/\bT\("((?:[^"\\\n]|\\.)*)"/g)) if (arabic.test(m[1])) keys.add(m[1].replace(/\\(.)/g, '$1'));
}

if (process.argv.includes('--keys')) {
  console.log(JSON.stringify(Object.fromEntries([...keys].sort().map(k => [k, ''])), null, 2));
  process.exit(0);
}

// a translation keeps the key's {0} placeholders and <b> tags, and no stray brace (string.Format would throw)
const marks = s => [...s.matchAll(/\{\d+\}|[{}]|<\/?\w+>/g)].map(m => m[0]).sort().join(' ');

let missing = 0;
for (const file of readdirSync(new URL('lang/', app)).filter(f => f.endsWith('.json'))) {
  const dict = JSON.parse(readFileSync(new URL('lang/' + file, app), 'utf8').replace(/^﻿/, ''));
  const lacking = [...keys].filter(k => !dict[k]);
  const broken = [...keys].filter(k => dict[k] && marks(dict[k]) !== marks(k));
  const unused = Object.keys(dict).filter(k => !k.startsWith('@') && !keys.has(k));
  missing += lacking.length + broken.length + (dict['@name'] ? 0 : 1);
  console.log(`${file}: ${keys.size - lacking.length}/${keys.size} translated, ${broken.length} broken, ${unused.length} unused`);
  if (!dict['@name']) console.log('  missing: @name');
  for (const k of lacking) console.log('  missing: ' + k);
  for (const k of broken) console.log(`  broken:  ${dict[k]}  (needs ${marks(k)})`);
  for (const k of unused) console.log('  unused:  ' + k);
}
process.exit(missing ? 1 : 0);
