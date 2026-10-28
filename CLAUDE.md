# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Proyecto
Sistema de expediente clínico electrónico para médicos independientes en México.

## Documentos de referencia
Todos los requisitos están en `specs/`.
Siempre lee el archivo `specs/REQUIREMENTS.md` antes de implementar funcionalidad clínica, de agenda o de pacientes — contiene el modelo de dominio, roles, esquema de BD y decisiones de arquitectura.

## Restricciones absolutas (no negociables)
- Los documentos firmados/finalizados son inmutables — las correcciones se hacen mediante adendas, nunca UPDATE/DELETE
- Toda nota clínica debe tener authorId + timestamp + estado (BORRADOR → FINALIZADO → FIRMADO)
- El rol Administrativo NUNCA accede a contenido clínico (nota, diagnóstico, tratamiento, historia clínica, receta) — ni en UI (se omite del DOM, no se deshabilita) ni en backend
- No implementar funcionalidad hospitalaria fuera del alcance del MVP (ver sección 2 y 13 de REQUIREMENTS.md)
- Cumplimiento con NOM-004-SSA3-2012 y NOM-024-SSA3-2012
- Cita y Consulta son entidades de dominio separadas (DEC-DOM-01) — no fusionarlas
- Multi-tenencia: cada médico independiente es su propio tenant; nunca compartir tenant entre médicos

## Estructura del repo
- `specs/REQUIREMENTS.md` — documento maestro de requerimientos (dominio, roles, BD, UI, historias de usuario)
- Monorepo: un solo repositorio git en la raíz; `colibri-ui/` y `colibri-api/` ya no tienen repos propios
- `colibri-ui/` — frontend Angular
- `colibri-api/` — backend .NET (ver `colibri-api/CLAUDE.md`)
- `docs/` — documentación de dominio y base de datos

## Stack UI y reglas de diseño (colibri-ui)
- **Framework:** Angular 22 standalone (componentes standalone, `loadComponent`/`loadChildren` para lazy loading, sin NgModules)
- **Componentes:** Angular Material (Material Design 3) — única librería de componentes UI, sin excepciones (sin PrimeNG)
- **Tabla de agenda:** `CdkTable` + `MatSort` + `MatPaginator` — nunca grids de terceros
- **Accesibilidad:** usar `A11yModule` del Angular CDK en toda la app
- **Estilos:** Tailwind CSS (v4, vía `@tailwindcss/postcss`) + `src/material-theme.scss` (genera los tokens `--mat-sys-*` con `mat.theme()`; se carga junto a `src/styles.css` en `angular.json`) para theming de Material
- La paleta de colores y las reglas de estado (borrador/finalizado/cancelado, etc.) están fijas en la sección 9 de `REQUIREMENTS.md` — el mismo estado usa siempre el mismo color, sin variar por pantalla
- Documentos inmutables: banner fijo + color verde + candado + metadatos de cierre; ningún control de edición visible, ni deshabilitado
- Acciones irreversibles usan el color primario teal (`#00695C`) y mayor peso visual que la acción reversible equivalente

## Comandos (ejecutar dentro de `colibri-ui/`)
```bash
npm start              # ng serve — http://localhost:4200
npm run build           # ng build (producción por defecto)
npm run watch            # ng build --watch --configuration development
npm test                  # ng test (Vitest)
npm run generate:icons   # regenera tools/material-symbols.icons.mjs
npx ng test --include='src/app/**/foo.spec.ts'   # una sola spec
```
- Íconos Material Symbols: un ícono nuevo debe agregarse primero a `tools/material-symbols.icons.mjs` y luego correr `npm run generate:icons`; si no, se renderiza como texto. `material-symbols.generated.css` es generado, no editar a mano
- Generar componentes con `ng generate component <ruta>` para mantener la estructura consistente
- Formateo con Prettier (`.prettierrc`: printWidth 100, comillas simples, parser `angular` para `*.html`)

## Arquitectura del frontend (`colibri-ui/src/app/`)
- `app.routes.ts` — rutas raíz; monta `layout/main` como shell y delega a rutas lazy por feature (`agenda`, `patients`, `consultation`)
- `layout/main/` — shell global (header, navegación persistente, selector de consultorio activo)
- `features/<feature>/` — un módulo por feature de dominio (`agenda`, `patient`, `consultation`), cada uno con su propio `routes.ts` cargado vía `loadChildren`
- `shared/ui/` — componentes de UI reutilizables entre features (ej. `page-structure` para header/breadcrumb/contenedor de página, `page-modal`)
- `shared/services/` — servicios transversales (ej. `modal-service`, `indexed-db-service` para persistencia local en IndexedDB mientras no hay backend)
- `app.config.ts` — transiciones de ruta con `withViewTransitions()`; los form fields de Material usan `appearance: 'outline'` por defecto
- Estilos globales: `src/material-theme.scss` y `src/styles.css` (entry points en `angular.json`); `src/assets/styles/` contiene `theme.css` (tokens Tailwind), `_theme-colors.scss` (paleta) y `components.scss` (overrides de componentes Material)

## Documentación de negocio (`colibri-ui/docs/`)
- `docs/reglas-negocio/01-catalogo-reglas-negocio.md` es la fuente única de reglas (`RN-*`); el código y las pruebas citan el ID de la RN. También: ciclos de vida (`02`), roles y permisos (`03`), decisiones `DEC-*` (`04`)
- `docs/fases/` tiene el roadmap por fases, definition-of-done y `trazabilidad.md` (REQ → RN → Historia → Pantalla → BD → Prueba); los cierres de fase se registran en `bitacora-fases.md`

### Patrón dentro de cada feature (ver `features/consultation/` como referencia)
- `<feature>.ts` / `.html` / `.css` — componente contenedor de la feature, cargado como entry point de las rutas hijas
- `routes.ts` — rutas propias de la feature (`export default routes`), referenciadas por lazy-loading desde el padre
- `<feature>-service.ts` — `@Injectable({ providedIn: 'root' })`, capa de acceso a datos/API (hoy son stubs, backend aún no existe en este repo)
- `<feature>-facade.ts` — `@Injectable()` (sin `providedIn: 'root'`, se provee a nivel de componente/ruta), envuelve el service y expone estado/lógica a los componentes
- `models/` — contiene DTOs (`*-dtos.ts`, forma del backend), modelos de UI/formulario (`*-models.ts`) y `mappers.ts` para convertir entre ambos — nunca usar los DTOs directamente en templates/formularios
- Subcarpetas por pantalla (`detail/`, `clinic-note/`, `create/`, `list/`) siguen el mismo trío `.ts`/`.html`/`.css`
