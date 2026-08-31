import { writeFile } from 'node:fs/promises';
import { materialSymbols } from './material-symbols.icons.mjs';

const icons = [...new Set(materialSymbols)].sort();

const url =
  'https://fonts.googleapis.com/css2' +
  '?family=Material+Symbols+Outlined' +
  `&icon_names=${icons.join(',')}`;

const css = `@import url("${url}");

.material-symbols-outlined {
  font-variation-settings:
    "FILL" 0,
    "wght" 400,
    "GRAD" 0,
    "opsz" 24;
}

.material-symbols-outlined.filled {
  font-variation-settings:
    "FILL" 1,
    "wght" 400,
    "GRAD" 0,
    "opsz" 24;
}
`;

await writeFile('./src/assets/styles/material-symbols.generated.css', css);

console.log(`Material Symbols generados: ${icons.length} iconos`);
