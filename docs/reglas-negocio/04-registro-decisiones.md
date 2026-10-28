# Registro de decisiones

**Corte:** 22-10-2026

Cada regla que no viene literal de una norma necesita una decisión que la justifique. Este registro concentra las decisiones de producto, dominio, arquitectura y UI, con su estado. Las decisiones con impacto grande pueden tener además un ADR propio en `docs/decisiones/ADR-NNN-titulo.md` (ver la plantilla al final).

Estados: `Aprobada` · `Adoptada` (en uso, sin aprobación formal) · `Propuesta` · `Pendiente` (bloquea algo) · `Reemplazada`

## Decisiones registradas

### Privacidad (Fase 3)

| ID | Decisión | Estado | Bloquea | Reglas |
|---|---|---|---|---|
| DEC-P3-001 | El aislamiento tiene como frontera al **médico**: cada médico es su propio tenant y ningún médico accede a pacientes o expedientes de otro, aunque compartan consultorio o centro. El centro médico es solo una agrupación administrativa. El Administrativo opera la agenda de los consultorios donde está adscrito (todos los médicos que operan ahí), con datos generales de paciente y metadatos de cita y consulta, sin contenido clínico; ve a un paciente solo si tiene cita o consulta en un consultorio de su adscripción. | **Aprobada** (27-10-2026) | — | RN-GEN-009, RN-ACC-011..013, RN-PAC-009, -010, RN-CON-010 |
| DEC-P3-002 | Excluir la atención a menores de la v1, salvo con representación verificable. | Pendiente de aprobación | Alta de pacientes menores | RN-PAC-006 |
| DEC-P3-003 | La receta se entrega al paciente; no hay envío a farmacias. | Pendiente de aprobación | Fase 9 | RN-RX-010 |
| DEC-P3-004 | Sin datos clínicos identificables para IA o analítica propia. | Pendiente de aprobación | Uso de datos reales | RN-GEN-006 |
| DEC-P3-005 | No usar aviso de privacidad corto. | Pendiente de aprobación | US-C4-03 | RN-PRIV-003 |
| DEC-P3-006 | El proveedor de pagos recibe los datos de tarjeta; el MVP no los almacena. | Pendiente de aprobación | Módulo de cobros | RN-PRIV-008 |
| DEC-P3-007 | El acceso de soporte es excepcional, temporal, autorizado y auditado. | Pendiente de aprobación | US-C4-05 | RN-ACC-009 |

### Dominio y producto

| ID | Decisión | Estado | Fecha | Reglas |
|---|---|---|---|---|
| DEC-DOM-001 | `Cita` es una entidad separada de `Consulta` y se convierte en ella al registrar la llegada; existe el walk-in sin cita. | Aprobada | 28-08-2026 | RN-CIT-*, RN-CON-002 |
| DEC-DOM-002 | Nuevo rol Administrativo, sin acceso clínico bajo ninguna circunstancia y aplicado en el backend. | Aprobada | 28-08-2026 | RN-ACC-006 |
| DEC-DOM-003 | "Médico sin restricciones" significa alcance funcional completo **dentro** de sus consultorios adscritos, no una excepción a negar por defecto. | **Reemplazada** por DEC-P3-001 (27-10-2026) | 28-08-2026 | RN-ACC-007 (reemplazada por RN-ACC-011) |
| DEC-DOM-004 | Consulta y nota son máquinas de estado independientes; no hay bloqueo por antigüedad del borrador, solo un indicador. Se revisa en las Fases 11 y 12. | Adoptada | 28-08-2026 | RN-CON-005, -007 |
| DEC-DOM-005 | El motivo pertenece a la Consulta, no a la Nota; "Resumen clínico" es otro documento, no un campo. | Aprobada | 28-08-2026 | RN-NOT-004, -005 |
| DEC-PRD-001 | ¿El pronóstico es obligatorio para finalizar la nota? Recomendación: sí, como selección rápida (bueno/reservado/malo) más una nota opcional. | **Aprobada en parte** (27-10-2026): el pronóstico es obligatorio para finalizar y sin valor preseleccionado. **Pendiente** su estructura: una selección o dos (para la vida y para la función) y sus valores. | — | RN-NOT-011 |
| DEC-PRD-002 | La teleconsulta queda fuera del MVP. No existe `modalidad_atencion` ni excepción para los signos vitales: TA, FC, FR y temperatura son siempre obligatorios para finalizar. | Aprobada | 27-10-2026 | RN-NOT-011 |
| DEC-DOM-006 | La retención es de 5 años desde el último acto médico (finalizar nota, agregar adenda, emitir receta, cerrar consulta). Se calcula en la capa de dominio, en la misma transacción del acto; no en un trigger de BD. | Aprobada | 27-10-2026 | RN-PAC-005, RN-PAC-011 |
| DEC-DOM-007 | Una consulta tiene una sola nota de evolución; las correcciones van como adendas. La regla vive en el dominio y `UNIQUE (consulta_id, tipo)` es respaldo declarativo. Se cruza con DEC-PEND-004 (descartar borradores). | Aprobada | 27-10-2026 | RN-NOT-001 |
| DEC-DOM-008 | Todo el listado de datos de la nota de evolución aportado es obligatorio: encabezado, evolución, signos vitales, declaración de estudios (la elige el médico), diagnósticos con CIE-10, tratamiento y pronóstico. Los adjuntos externos son inmutables. Los numerales de la norma se validan en la Fase 11. | Aprobada | 27-10-2026 | RN-NOT-011, RN-NOT-012 |

### Arquitectura

| ID | Decisión | Estado | Reglas |
|---|---|---|---|
| DEC-ARQ-001 | Monolito modular (API → Aplicación → Dominio → Infraestructura). | Propuesta | — |
| DEC-ARQ-002 | PostgreSQL con Row-Level Security como segunda capa de autorización. | Adoptada | RN-ACC-005, -006 |
| DEC-ARQ-003 | Inmutabilidad genérica: `documento_version` (snapshot + SHA-256) y `adenda`, con `REVOKE UPDATE/DELETE` para el rol de aplicación. | Adoptada | RN-NOT-007, RN-ADE-003 |
| DEC-ARQ-004 | Sesiones persistidas en BD para poder revocarlas de inmediato. | Adoptada | RN-ACC-003 |
| DEC-ARQ-005 | Proveedor de identidad y MFA: propio, Keycloak autoalojado o servicio gestionado. | **Pendiente** — bloquea el Sprint 1 de la Fase 7 | RN-ACC-001..003 |
| DEC-ARQ-006 | RPO/RTO de respaldos (propuesta: RPO ≤ 24 h, RTO ≤ 8 h). | **Pendiente** | RN-PRIV-009 |
| DEC-ARQ-007 | Firma de documentos clínicos: el MVP respalda la finalización con sesión MFA + confirmación explícita + hash + hora del servidor (`firma_constancia.mecanismo = 'mfa_hash'`). La e.firma (SAT; PSC como alternativa) queda fuera del MVP, pero **es condición bloqueante para comercializar el producto**. Aplica a notas y adendas; la receta conserva su propia regla (DEC-RX-001). Brecha conocida frente al requisito de firma autógrafa o electrónica avanzada: se resuelve con la e.firma y se valida en la Fase 11. | **Aprobada** (27-10-2026) | RN-NOT-010, RN-ADE-006, RN-GEN-008 |
| DEC-ARQ-008 | Los auditores externos entran solo mediante una **concesión temporal de solo lectura**, con folio del requerimiento: auditor sanitario (únicamente los expedientes citados) o auditor técnico (únicamente bitácoras). Solo el médico dueño del tenant la otorga. El médico consulta por defecto la auditoría de su propio tenant; el centro médico no tiene auditoría agregada en el MVP. La disociación de datos para comités queda fuera del MVP. Los numerales citados se validan en la Fase 11. | **Aprobada** (27-10-2026) | RN-ACC-014, RN-AUD-007 |
| DEC-RX-001 | Firma autógrafa sobre la impresión; la FEA queda fuera hasta tener validación jurídica. | Adoptada | RN-RX-008 |
| DEC-RX-002 | Los medicamentos controlados se excluyen del MVP. | Adoptada | RN-RX-004 |

### UI

| ID | Decisión | Estado | Nota |
|---|---|---|---|
| DEC-UI-001 | Tabla de la agenda (P1): el prompt de diseño pide PrimeNG, pero colibri-ui solo tiene Angular Material + CDK, y P10 ya usa CdkTable. | **Pendiente** | Recomendación: usar CdkTable en todas las tablas y evitar una segunda librería de componentes. |

## Preguntas abiertas detectadas al consolidar (DEC-PEND)

Las encontré al cruzar los documentos del proyecto. Cada una debe resolverse y moverse a la tabla de arriba con su ID definitivo.

| ID | Pregunta | Por qué importa | Recomendación |
|---|---|---|---|
| DEC-PEND-001 → **Resuelta: DEC-DOM-006** | Retención: el diseño de BD calcula `retencion_hasta = fecha_apertura + 5 años`, pero la NOM-004 5.4 (y el RNF-002 del análisis) cuenta **5 años desde el último acto médico**. | Con el cálculo actual, un paciente que sigue en atención podría quedar "fuera de retención" antes de tiempo. | Recalcular `retencion_hasta` en cada consulta, nota o receta finalizada (RN-PAC-005). |
| DEC-PEND-002 | ¿Apellido materno obligatorio? Hay personas con un solo apellido. | Bloquearía el alta de pacientes reales. | Obligar el apellido paterno y aceptar el materno vacío con la marca "no tiene". |
| DEC-PEND-003 | ¿Quién puede agregar una adenda: solo el autor de la nota o cualquier médico adscrito al consultorio? | Afecta la autoría y la responsabilidad clínica. | Cualquier médico adscrito; la adenda conserva su propio autor. |
| DEC-PEND-004 | ¿Se puede descartar un borrador de nota que nunca se finalizó? | Acumula basura o deja consultas sin poder cerrarse. | Permitir el descarte como baja lógica auditada, solo para el autor. |
| DEC-PEND-005 | ¿Qué vigencia tiene una receta (para el estado `vencida`)? | Sin vigencia, el estado `vencida` nunca se alcanza. | Investigar en el docx de receta electrónica; hacerla configurable. |
| DEC-PEND-006 → **Resuelta en parte: DEC-ARQ-008** (solo la auditoría; siguen sin actor asignado ARCO, aviso de privacidad, incidentes y acceso de soporte) | El rol "Responsable de cumplimiento" se usa en las historias, pero no existe en `usuario.tipo`. | Nadie podría consultar la auditoría. | Agregarlo como tipo o como un permiso asignable a un usuario. |
| DEC-PEND-007 → **Resuelta: DEC-DOM-007** | ¿Una consulta tiene una sola nota de evolución? | Define una restricción UNIQUE y el flujo de P2. | Sí: una nota por consulta; las correcciones se hacen con adendas. |
| DEC-PEND-008 | ¿Quién cierra la consulta? | El modelo no lo dice. | Solo el Médico; el Administrativo no ve el estado de la nota. |
| DEC-PEND-009 | El Administrativo captura alergias en el alta (P9), pero las alergias son información clínica. | Tensión con RN-ACC-006. | Permitirle capturarlas solo en el alta, sin poder leerlas después; o mover el campo al expediente. |

---

## Plantilla de ADR

Copia esta plantilla a `docs/decisiones/ADR-NNN-titulo-corto.md` cuando una decisión necesite más contexto que una fila.

```markdown
# ADR-NNN — <Título>

- **Estado:** Propuesta | Aprobada | Reemplazada por ADR-XXX
- **Fecha:** AAAA-MM-DD
- **Decide:** <persona/rol>
- **ID en registro:** DEC-<ÁREA>-NNN

## Contexto
Qué problema o ambigüedad origina la decisión. Cita la norma, historia o pantalla.

## Opciones consideradas
1. Opción A — pros / contras
2. Opción B — pros / contras

## Decisión
Qué se eligió, en una o dos oraciones.

## Consecuencias
- Reglas de negocio que crea o modifica (RN-…)
- Cambios en BD, API o UI
- Riesgo regulatorio residual y cuándo revisarlo (Fase 11/12)
```
