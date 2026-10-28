# Fase NN — <Nombre>

> Copia este archivo como `fase-NN-nombre-corto.md`. Borra las instrucciones entre `>` al terminar.

- **Estado:** Por iniciar | En curso | Cerrada
- **Fechas:** inicio AAAA-MM-DD · cierre AAAA-MM-DD
- **Dueño:**
- **Entregable (según `Ruta.md`):**

## 1. Objetivo

> Una o dos oraciones: qué capacidad nueva existe al terminar la fase.

## 2. Alcance

| Incluye | No incluye |
|---|---|
| | |

## 3. Dependencias y decisiones bloqueantes

| ID | Descripción | Estado | ¿Bloquea el inicio? |
|---|---|---|---|
| DEC-… | | | Sí/No |

## 4. Reglas de negocio que implementa esta fase

> Lista los RN del catálogo cuya columna **Fase** es este número. Si encuentras una regla nueva, agrégala primero al catálogo.

| RN | Resumen | Capa |
|---|---|---|
| RN-… | | API/BD/UI |

## 5. Épicas e historias

### Épica EN-1 — <nombre>

#### US-N-01 · <título> — P0|P1

Como **<rol>**, quiero **<acción>** para **<beneficio>**.

**Criterios de aceptación**

```gherkin
Escenario: <caso feliz>
  Dado …
  Cuando …
  Entonces …

Escenario: <caso negativo ligado a una RN>
  Dado …
  Cuando …
  Entonces se rechaza con <error> (RN-…)
  Y se registra el evento de auditoría <accion> con resultado "denegado"
```

- **Pantalla:** P…
- **Trazabilidad:** REQ (CLIN-/SIR-/PRIV-/RX-) · RN-…
- **Estado:** Pendiente | En desarrollo | Hecha

## 6. Diseño técnico de la fase

> Endpoints, tablas, migraciones y componentes. Enlaza al diseño de BD en lugar de copiarlo.

| Elemento | Detalle |
|---|---|
| Endpoints | |
| Tablas/migraciones | |
| Rutas/componentes UI | |

## 7. Plan de pruebas

| Tipo | Qué cubre | Herramienta |
|---|---|---|
| Unitarias | Validaciones RN de tipo V | Vitest |
| Integración API | Transiciones T, autorización A (positivas y negativas) | |
| BD | Triggers, RLS, REVOKE | |
| E2E | Flujo completo de la fase | |

## 8. Criterios de salida

- [ ] Todas las historias P0 están en "Hecha" según la Definition of Done.
- [ ] Toda RN de esta fase tiene una prueba registrada en `trazabilidad.md`.
- [ ] Las decisiones bloqueantes están resueltas en `04-registro-decisiones.md`.
- [ ] Hay una entrada nueva en `bitacora-fases.md`.
- [ ] <criterio específico de la fase>

## 9. Riesgos

| Riesgo | Mitigación |
|---|---|
| | |
