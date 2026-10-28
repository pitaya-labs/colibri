# Bitácora de fases

Registro de cierres de fase y de cambios relevantes en reglas de negocio. Las entradas más recientes van arriba. No se editan entradas pasadas: si algo cambió, se agrega una entrada nueva.

## Formato de entrada

```markdown
## AAAA-MM-DD — Fase N: <cerrada | hito | cambio de reglas>

**Qué se liberó:** …
**Reglas nuevas o cambiadas:** RN-… (nueva), RN-… (reemplazada por RN-…)
**Decisiones resueltas:** DEC-…
**Pendientes que pasan a la siguiente fase:** …
**Evidencia:** enlace a CI / acta / reporte
```

---

## 2026-10-27 — Cambio de reglas: aislamiento por médico, retención, auditoría y contenido de la nota

**Qué se liberó:** frontera de aislamiento por médico, acceso del Administrativo por consultorio, retención desde el último acto médico, concesión de acceso para auditores externos, una nota por consulta y contenido obligatorio de la nota de evolución.
**Reglas nuevas:** RN-GEN-009, RN-ACC-011..014, RN-PAC-009..011, RN-CON-010, RN-NOT-011, RN-NOT-012, RN-AUD-007.
**Reglas reemplazadas:** RN-GEN-005, RN-ACC-005, RN-ACC-007, RN-ACC-010, RN-PAC-008, RN-NOT-002, RN-NOT-003. RN-PAC-005 y RN-NOT-001 pasan a Vigente.
**Decisiones resueltas:** DEC-P3-001 (reescrita y aprobada), DEC-DOM-003 (reemplazada), DEC-PEND-001 → DEC-DOM-006, DEC-PEND-007 → DEC-DOM-007, DEC-PEND-006 en parte → DEC-ARQ-008, DEC-PRD-001 en parte, DEC-PRD-002 (teleconsulta fuera del MVP), DEC-DOM-008.
**Pendientes que pasan a la siguiente fase:** estructura del pronóstico (DEC-PRD-001); actor responsable de ARCO, aviso de privacidad, incidentes y acceso de soporte (DEC-PEND-006); motivo de consulta en el walk-in (RN-CON-010); entidad de centro médico y cambios de esquema, listados en `reglas-negocio/05-cambios-esquema-bd.md`; validación de los numerales citados de las normas en la Fase 11.
**Evidencia:** conversación de definición de reglas del 27-10-2026.

---

## 2026-10-27 — Cambio de reglas: firma de documentos clínicos

**Qué se liberó:** decisión sobre el mecanismo de firma del MVP y condición de comercialización.
**Reglas nuevas o cambiadas:** RN-NOT-010 (nueva, transitoria), RN-ADE-006 (nueva), RN-GEN-008 (nueva).
**Decisiones resueltas:** DEC-ARQ-007 (Aprobada). El MVP se respalda con MFA + confirmación + hash + hora del servidor. La e.firma queda fuera del MVP y es condición bloqueante de venta.
**Pendientes que pasan a la siguiente fase:** hito e.firma sin fase asignada; validación jurídica de la firma en la Fase 11; la brecha frente al requisito de firma autógrafa o electrónica avanzada queda abierta en `trazabilidad.md`.
**Evidencia:** conversación de definición de reglas del 27-10-2026.

---

## 2026-10-22 — Documentación de reglas de negocio y fases

**Qué se liberó:** catálogo de reglas de negocio (RN-*), ciclos de vida, matriz de roles y permisos, registro de decisiones, roadmap de entrega, especificaciones de las fases 7 y 8, matriz de trazabilidad y Definition of Done.
**Reglas nuevas:** catálogo inicial consolidado desde el modelo de dominio, el backlog regulatorio, las historias de la Fase 7, el diseño de BD y los prompts de pantallas.
**Decisiones abiertas detectadas:** DEC-PEND-001 a DEC-PEND-009 y DEC-UI-001.
**Estado del proyecto:** fases 0–6 cerradas; Fase 7 diseñada; prototipo de UI de la Fase 8 en curso en colibri-ui.

## 2026-08-31 — Fase 6: diseño de base de datos

**Qué se liberó:** esquema físico PostgreSQL con RLS, triggers de cierre de consulta y emisión de receta, y `documento_version` genérica.

## 2026-08-28 — Fases 3, 5 y 6

**Qué se liberó:** cierre de privacidad (P3-001..040, DEC-P3-001..007), backlog regulatorio maestro, modelo de dominio con `Cita` y rol Administrativo, historias e infraestructura de la Fase 7.
