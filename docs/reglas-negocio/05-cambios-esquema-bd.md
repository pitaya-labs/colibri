# Cambios pendientes al esquema de base de datos

**Corte:** 27-10-2026 · Complementa a `Diseno_Base_de_Datos_MVP_Clinico_Mexico_2026-08-31.md`, que no se edita (es un documento fechado). Este archivo lista lo que el esquema de agosto debe cambiar para reflejar las decisiones posteriores. Cada cambio cita la regla o decisión que lo origina.

Estado de cada cambio: **Decidido** (viene de una decisión aprobada) · **Por confirmar** (consecuencia de ingeniería que falta validar al diseñar la Fase 7).

## 1. Aislamiento por médico (DEC-P3-001)

| Cambio | Detalle | Regla | Estado |
|---|---|---|---|
| Tenant = médico | `medico` queda 1:1 con `tenant`. Un médico es una sola identidad que puede trabajar en varios consultorios y centros. | RN-GEN-009 | Decidido |
| Entidad `centro_medico` | Agrupación administrativa de consultorios y personal. No tiene acceso clínico ni RLS clínico. | RN-GEN-009 | Decidido |
| `consultorio` sin dueño único | Hoy tiene `tenant_id`. Un consultorio es un espacio compartido entre tenants, así que ese campo no puede ser su dueño. Se evalúa relacionarlo con `centro_medico`. | RN-ACC-011 | Por confirmar |
| `usuario` del Administrativo | `usuario.tenant_id` único no sirve: el Administrativo opera sobre varios médicos. Su alcance lo define la `adscripcion` a consultorios. | RN-ACC-012 | Por confirmar |
| `paciente.tenant_id` | Es el médico responsable. El alta por un Administrativo recibe al médico como dato obligatorio. | RN-PAC-010 | Decidido |
| RLS en dos políticas | **Médico:** `tenant_id = app.tenant_id`. **Administrativo:** adscripción activa al consultorio, solo sobre `cita`, `consulta` (sin `motivo`) y `paciente` con cita o consulta en sus consultorios. Las tablas clínicas se cierran a todo usuario que no sea el médico dueño. | RN-ACC-011, -012, -006 | Decidido |
| `consulta.motivo` oculto al Administrativo | Se expone el `motivo` de la `cita`, no el de la `consulta` editado por el médico. Falta definir el walk-in. | RN-CON-010 | Por confirmar |
| `tenant_id` faltante | `mfa_credencial`, `sesion`, `nota_estudio`, `aceptacion_aviso` y `respaldo_ejecucion` no lo tienen; `auditoria.tenant_id` no tiene llave foránea. | RN-GEN-009 | Por confirmar |

## 2. Retención (DEC-DOM-006)

| Cambio | Detalle | Regla | Estado |
|---|---|---|---|
| Corregir el comentario del DDL | `expediente.retencion_hasta` no es `fecha_apertura + 5 años`; es 5 años desde el último acto médico. | RN-PAC-005 | Decidido |
| Sin trigger | El recálculo ocurre en la capa de dominio, en la misma transacción del acto: `GREATEST(retencion_hasta, acto + 5 años)`. | RN-PAC-011 | Decidido |

## 3. Nota clínica (DEC-DOM-007, DEC-DOM-008)

| Cambio | Detalle | Regla | Estado |
|---|---|---|---|
| Una nota por consulta | `UNIQUE (consulta_id, tipo)` como respaldo declarativo. Si se permite descartar borradores (DEC-PEND-004), pasa a ser un índice único parcial. | RN-NOT-001 | Decidido |
| Declaración de estudios | Columnas para la elección del médico (`con_estudios` / `sin_estudios`) y el resumen de hallazgos. Hoy `nota_estudio` solo enlaza adjuntos. | RN-NOT-011 | Decidido |
| Pronóstico | `pronostico` es un `text` libre. La estructura (una o dos selecciones y sus valores) sigue pendiente en DEC-PRD-001. | RN-NOT-011 | Pendiente |
| Signos vitales | Se conserva el `jsonb`. Su obligatoriedad y rangos los valida el dominio. Sin excepción por teleconsulta. | RN-NOT-011, DEC-PRD-002 | Decidido |
| Diagnóstico con CIE-10 | `diagnostico.catalogo_codigo` pasa a `NOT NULL`. Se necesita la tabla de catálogo CIE-10. `tipo` pasa a `CHECK IN ('principal', 'secundario')`, con al menos un principal por nota. | RN-NOT-011 | Decidido |
| Snapshot completo | `contenido_snapshot` debe congelar nombre y cédula del médico y la edad del paciente, para que cambios posteriores no alteren una nota finalizada. | RN-NOT-011 | Por confirmar |
| `tratamiento_resumen` | Duplica a la tabla `tratamiento`. Revisar si se conserva. | — | Por confirmar |

## 4. Paciente y expediente (RN-NOT-011)

| Cambio | Detalle | Estado |
|---|---|---|
| `expediente.numero_expediente` | Número legible, único por tenant. Hoy solo existe el `uuid`. | Decidido |
| Nombre separado | `paciente.nombre_completo` se separa en nombre(s), apellido paterno y apellido materno. La obligatoriedad del materno sigue pendiente en DEC-PEND-002. | Decidido (separación) · Pendiente (materno) |
| `paciente.sexo` | Pasa a `NOT NULL` con `CHECK`. | Decidido |
| `medico.rfc` | Necesario para validar la e.firma después. | Por confirmar |

## 5. Adjuntos de estudios (RN-NOT-012)

| Cambio | Detalle | Estado |
|---|---|---|
| Columnas nuevas en `estudio_adjunto` | `origen`, `fecha_estudio`, `institucion_emisora` y `hash_sha256`. | Decidido |
| Inmutabilidad | Se elimina el estado `reemplazado` y se aplica `REVOKE UPDATE, DELETE`, como en `documento_version`. | Decidido |

## 6. Firma (DEC-ARQ-007)

| Cambio | Detalle | Estado |
|---|---|---|
| `firma_constancia.mecanismo` | Hoy solo admite `autografa_sobre_impresion`. Se agrega `mfa_hash` para notas y adendas. Debe quedar extensible a `fea_sat` y `fea_psc`. | Decidido |
| Receta | Conserva `autografa_sobre_impresion` (DEC-RX-001). | Decidido |

## 7. Auditores externos (DEC-ARQ-008)

| Cambio | Detalle | Estado |
|---|---|---|
| Tabla `concesion_acceso` | `tenant_id`, `perfil` (`auditor_sanitario` / `auditor_tecnico`), `otorgada_por`, `folio_requerimiento`, `autoridad`, `vigencia_desde`, `vigencia_hasta`, y los expedientes cubiertos cuando el perfil es sanitario. Siempre de solo lectura. | Decidido |
| `usuario.tipo` | Se agrega `auditor_externo`. Su capacidad sale solo de la concesión. | Decidido |
| `auditoria.concesion_id` | Columna nullable que liga cada evento de un auditor a su concesión. | Decidido |
