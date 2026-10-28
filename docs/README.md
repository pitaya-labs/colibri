# Documentación de Colibrí

Expediente clínico electrónico ambulatorio para médicos independientes y consultorios pequeños en México.

Esta carpeta contiene las **reglas de negocio** y el **desarrollo por fases**. La investigación regulatoria (NOM-004, NOM-024, privacidad, receta) y el modelo de dominio viven en el proyecto "Atención Hospitalaria" de Claude; aquí se citan por nombre.

## Reglas de negocio — `reglas-negocio/`

| Documento | Para qué sirve |
|---|---|
| [01-catalogo-reglas-negocio.md](reglas-negocio/01-catalogo-reglas-negocio.md) | **Fuente única** de reglas, con IDs `RN-*` que se citan desde el código y las pruebas |
| [02-ciclos-de-vida.md](reglas-negocio/02-ciclos-de-vida.md) | Estados y transiciones permitidas de Cita, Consulta, Nota, Receta y entidades de soporte |
| [03-roles-y-permisos.md](reglas-negocio/03-roles-y-permisos.md) | Qué puede hacer cada rol, y reglas de UI por rol |
| [04-registro-decisiones.md](reglas-negocio/04-registro-decisiones.md) | Decisiones (`DEC-*`), preguntas abiertas y plantilla de ADR |
| [05-cambios-esquema-bd.md](reglas-negocio/05-cambios-esquema-bd.md) | Cambios al esquema físico que se derivan de las decisiones de 27-10-2026 |

## Desarrollo por fases — `fases/`

| Documento | Para qué sirve |
|---|---|
| [00-roadmap.md](fases/00-roadmap.md) | Estado de las fases 0–12, dependencias y criterios de salida |
| [_plantilla-fase.md](fases/_plantilla-fase.md) | Plantilla para especificar cada fase nueva |
| [fase-07-compliance.md](fases/fase-07-compliance.md) | Infraestructura de compliance |
| [fase-08-vertical-slice-clinico.md](fases/fase-08-vertical-slice-clinico.md) | Flujo clínico de punta a punta, con historias en Gherkin |
| [trazabilidad.md](fases/trazabilidad.md) | REQ → RN → Historia → Pantalla → BD → Prueba → Evidencia |
| [definition-of-done.md](fases/definition-of-done.md) | Cuándo una historia está hecha y una fase está cerrada |
| [bitacora-fases.md](fases/bitacora-fases.md) | Registro de cierres y cambios de reglas |

## Flujo de trabajo

```text
Norma / decisión  →  RN en el catálogo  →  Historia en el doc de fase
      →  código + prueba con el ID de la RN  →  fila en trazabilidad  →  bitácora al cerrar la fase
```
