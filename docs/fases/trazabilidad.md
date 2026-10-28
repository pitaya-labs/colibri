# Matriz de trazabilidad

**Corte:** 22-10-2026 · Es la base de la Fase 11 (matriz de cumplimiento validada).

Cada fila une un requisito regulatorio con la regla de negocio, la historia, el lugar donde se implementa, la prueba que lo demuestra y la evidencia archivada. **Una fila P0 sin prueba ni evidencia impide cerrar su fase.**

## Columnas

| Columna | Contenido |
|---|---|
| REQ | ID del backlog regulatorio maestro (CLIN-, SIR-, PRIV-, RX-, GOV-) o de las matrices fuente (P3-, RF-, DAT-) |
| RN | Regla(s) del catálogo |
| Historia | US-… |
| Pantalla / Ruta | P… · ruta de colibri-ui |
| Endpoint / BD | Endpoint, tabla, trigger, política RLS |
| Prueba | Archivo y nombre de la prueba (que incluya el ID de la RN) |
| Evidencia | Dónde quedó: reporte de CI, captura, acta |
| Estado | ⚪ sin implementar · 🔵 implementado · 🟢 probado · ✅ con evidencia |

## Matriz

| REQ | RN | Historia | Pantalla / Ruta | Endpoint / BD | Prueba | Evidencia | Estado |
|---|---|---|---|---|---|---|---|
| CLIN-001 | RN-GEN-002, -003, RN-NOT-007 | US-C3-01, US-8-12 | P3→P4 · `/consultation/clinical-note` | `documento_version`, `now()` del servidor | | | ⚪ |
| CLIN-002 | RN-PAC-001, RN-ACC-011, -012 | US-C1-02, US-8-02 | P9 | `expediente.paciente_id UNIQUE`; RLS | | | ⚪ |
| CLIN-003 | RN-GEN-004, RN-PAC-005, RN-PAC-011, RN-PRIV-005 | US-C4-01 | — | `expediente.retencion_hasta`; `solicitud_eliminacion` | | | ⚪ |
| CLIN-004 | RN-ADE-001..003, RN-NOT-008 | US-C3-02, US-8-13 | P5 | `adenda` + `REVOKE` | | | ⚪ |
| SIR-001 | RN-NOT-007, RN-RX-006 | US-C3-01 | P4 | hash SHA-256 en `documento_version` | | | ⚪ |
| SIR-002 | RN-AUD-001..005 | US-C2-01 | — | `auditoria` + middleware | | | ⚪ |
| SIR-003 | RN-PAC-003 | US-8-02 | P9 | `paciente.curp` | | | ⚪ |
| SIR-004 | RN-PRIV-007 | US-8-04 | Expediente | Endpoint de exportación | | | ⚪ |
| PRIV-001 | RN-GEN-006 | — | — | Inventario de datos | | | ⚪ |
| PRIV-002 | RN-PRIV-001..003 | US-C4-03 | — | `aviso_privacidad`, `aceptacion_aviso` | | | ⚪ |
| PRIV-003 | RN-PRIV-006 | US-C4-04 | — | `solicitud_arco` | | | ⚪ |
| PRIV-004 | RN-ACC-001..003, RN-PRIV-009 | US-C1-01, US-C4-02 | P6 (indicador MFA) | `sesion`, `mfa_credencial`, `respaldo_ejecucion` | | | ⚪ |
| — (MD §6.10) | RN-ACC-006 | US-C1-02, US-8-04 | P1, P8, P10 | RLS + validación de `usuario.tipo` | Pruebas negativas, Fase 7 §6 | | ⚪ |
| — (MD §5) | RN-CON-004 | US-8-14 | P2 · `/consultation/detail` | `trg_validar_cierre_consulta` | | | ⚪ |
| — (MD §10) | RN-CIT-001..007 | US-8-05..09 | P1, P7, P8 · `/agenda` | `cita` | | | ⚪ |
| DAT-034..040 | RN-NOT-011, -012 (reemplazan a RN-NOT-002 y -003), RN-NOT-004, -005 | US-8-11, -12 | P3 | `nota_clinica`, `diagnostico`, `estudio_adjunto` | | | ⚪ |
| RX-001 | RN-RX-002, -011 | Fase 9 | — | `receta`, `linea_receta` | | | ⚪ |
| RX-002 | RN-RX-003 | Fase 9 | — | Catálogo de medicamentos | | | ⚪ |
| RX-003 | RN-RX-008 | Fase 9 | — | `firma_constancia` | | | ⚪ |
| RX-004 | RN-RX-005 | Fase 9 | — | Validación de antibióticos | | | ⚪ |
| RX-005 | RN-RX-004 | Fase 9 | — | `CHECK linea_receta.clasificacion` | | | ⚪ |
| RX-006 | RN-RX-001 | Fase 9 | — | `trg_validar_emision_receta` | | | ⚪ |
| RX-007 | RN-RX-006, -007 | Fase 9 | — | `receta.receta_sustituye_id` | | | ⚪ |
| — (DEC-P3-001) | RN-GEN-009, RN-ACC-011..013, RN-PAC-009, -010, RN-CON-010 | US-C1-02, US-8-01, US-8-02 | P1, P9, P10 | RLS por tenant (Médico) y por adscripción (Administrativo) | Pruebas negativas, Fase 7 §6 | | ⚪ |
| — (DEC-ARQ-008) | RN-ACC-014, RN-AUD-007 | US-C2-02 | — | `concesion_acceso`, `auditoria.concesion_id` | | | ⚪ |
| — (DEC-ARQ-007) | RN-NOT-010, RN-ADE-006 | US-C3-01, US-C3-02 | P3→P4, P5 | `firma_constancia.mecanismo = 'mfa_hash'` | | | ⚪ |
| — (DEC-ARQ-007) · **brecha abierta** | RN-GEN-008 | Hito e.firma (post-MVP) | Diálogo de firma | `firma_constancia` (`fea_sat`, `fea_psc`), `medico.rfc` | | | ⚪ |
| GOV-001 | — | — | — | Este documento | Revisión trimestral | | ⚪ |

## Cómo mantenerla

1. Al terminar una historia, llena **Prueba** con la ruta del archivo y el nombre de la prueba (ej. `api/test/consulta.spec.ts › RN-CON-004 rechaza cierre sin nota`).
2. Al pasar CI, guarda en **Evidencia** el enlace o la ruta del reporte.
3. Una RN sin REQ (derivada por ingeniería) también lleva fila. En REQ pon la sección del modelo o la decisión que la origina, para que en la Fase 11 se puedan revisar por separado las reglas que no son normativas.
