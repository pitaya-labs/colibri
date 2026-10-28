# Fase 7 — Infraestructura base de compliance

- **Estado:** Diseñada, sin construir
- **Dueño:** Beto
- **Entregable (según `Ruta.md`):** infraestructura base de compliance del producto.
- **Fuentes:** `Historias_de_Usuario_Fase_7_Compliance_MVP`, `Infraestructura_Base_de_Compliance_MVP`, `Propuesta_Diseno_Inicial_Fase7_Compliance_MVP`, `Diseno_Base_de_Datos_MVP_Clinico_Mexico`. Este documento **no repite** esas fuentes: las amarra a las reglas de negocio, las decisiones y la trazabilidad.

## 1. Objetivo

Que ninguna funcionalidad clínica futura pueda existir sin identidad verificada, autorización por consultorio, rastro de auditoría inmutable, documentos que no se puedan alterar y controles de privacidad y retención.

## 2. Alcance

| Incluye | No incluye |
|---|---|
| Autenticación con MFA y sesiones revocables | Pantallas de expediente, consulta, nota o receta |
| RBAC (5 roles) + RLS por adscripción | Cobros |
| Bitácora append-only y su exportación | Integraciones externas |
| Mecanismo genérico de finalizar, adenda, snapshot y hash (firma MFA + hash, RN-NOT-010) | FEA / e.firma (hito bloqueante de venta, RN-GEN-008) |
| Retención, respaldos, aviso de privacidad, ARCO e incidentes | |

## 3. Decisiones bloqueantes

| ID | Descripción | Estado | ¿Bloquea el inicio? |
|---|---|---|---|
| DEC-ARQ-005 | Proveedor de identidad y MFA | Pendiente | **Sí** (Sprint 1) |
| DEC-P3-001 | Tenant = médico (frontera de aislamiento) | **Aprobada** (27-10-2026) | No |
| DEC-PEND-006 | Rol de Responsable de cumplimiento en `usuario.tipo` | Resuelta en parte: auditoría por DEC-ARQ-008. Faltan ARCO, aviso, incidentes y acceso de soporte | Parcial (Sprints 4–5) |
| DEC-PEND-001 | Retención contada desde el último acto médico | **Resuelta**: DEC-DOM-006 | No |
| DEC-ARQ-006 | RPO/RTO | Pendiente | Sí (Sprint 4) |
| DEC-P3-007 | Acceso de soporte | Pendiente de aprobación | No (Sprint 4) |
| DEC-DOM-003 | Alcance del médico | **Reemplazada** por DEC-P3-001 | No |

## 4. Reglas de negocio que implementa

RN-GEN-001..004, -006, -007, -009 · RN-ACC-001..004, -006, -008, -009, -011..014 · RN-NOT-007, -008, -010 (mecanismo) · RN-ADE-001..003, -005, -006 · RN-AUD-001..007 · RN-PRIV-001..003, -005, -006, -009 · RN-PAC-005, -011 (cálculo de retención).

## 5. Historias por sprint

| Sprint | Historia | Pri. | RN principales | Requisito | Estado |
|---|---|---|---|---|---|
| 1 | US-C1-01 Acceso individual con MFA | P0 | RN-ACC-001..004 | PRIV-004, SIR-002 | Pendiente |
| 1 | US-C1-02 Autorización por consultorio | P0 | RN-ACC-005..007 | CLIN-002, SIR-002 | Pendiente |
| 1 | US-C1-03 Gestión de adscripciones *(generalizar a usuario–consultorio)* | P0 | RN-ACC-008 | SIR-002 | Pendiente |
| 2 | US-C2-01 Auditoría inmutable | P0 | RN-AUD-001..005 | SIR-002, CLIN-004 | Pendiente |
| 2 | US-C3-03 Tiempo clínico confiable | P0 | RN-GEN-001, -002 | CLIN-001 | Pendiente |
| 3 | US-C3-01 Finalización inmutable | P0 | RN-NOT-007, -008 | CLIN-001, SIR-001 | Pendiente |
| 3 | US-C3-02 Corrección trazable | P0 | RN-ADE-001..005 | CLIN-004 | Pendiente |
| 4 | US-C4-01 Retención clínica | P0 | RN-PAC-005, RN-PRIV-005 | CLIN-003 | Pendiente |
| 4 | US-C4-02 Respaldos y restauración | P0 | RN-PRIV-009 | PRIV-004 | Pendiente |
| 4 | US-C4-03 Aviso de privacidad versionado | P0 | RN-PRIV-001..003 | PRIV-001, -002 | Pendiente |
| 4 | US-C4-05 Gestión de incidentes | P0 | RN-ACC-009 | PRIV-004 | Pendiente |
| 5 | US-C2-02 Consulta/exportación de auditoría | P1 | RN-AUD-006 | SIR-002 | Pendiente |
| 5 | US-C4-04 Solicitudes ARCO | P1 | RN-PRIV-006 | PRIV-003 | Pendiente |

**Ajuste pendiente a las historias originales:** US-C1-01 dice "vinculada a un médico verificado", pero desde la decisión DEC-DOM-002 también hay usuarios administrativos. Reescribir el criterio como "cuenta activa de una persona verificada; si es de tipo Médico, vinculada a un médico verificado". US-C1-03 debe decir usuario–consultorio.

## 6. Pruebas negativas obligatorias (criterio de salida 6)

Por cada endpoint clínico (nota, diagnóstico, tratamiento, historia, receta, adjunto clínico, exportación de expediente):

```gherkin
Escenario: Administrativo no alcanza contenido clínico aunque conozca el ID
  Dado un usuario Administrativo con adscripción activa al consultorio C
  Y una nota clínica N de una consulta en C
  Cuando hace GET/POST/PUT/DELETE sobre /…/N
  Entonces la respuesta es 403
  Y la BD (RLS) no devuelve filas aunque la API fallara
  Y se audita la acción con resultado "denegado" (RN-AUD-005)

Escenario: Un Médico no alcanza pacientes de otro Médico aunque compartan consultorio
  Dado dos Médicos, A y B, adscritos al mismo consultorio C
  Y un paciente P de B
  Cuando A solicita a P, su expediente o cualquier recurso suyo por ID
  Entonces la respuesta es 403 o 404, sin filtrar la existencia del recurso (RN-ACC-011)
  Y la BD (RLS) no devuelve filas aunque la API fallara

Escenario: Un Administrativo no alcanza consultorios sin adscripción
  Dado un Administrativo adscrito solo a C1
  Cuando solicita una cita, o un paciente sin cita ni consulta en C1, por su ID
  Entonces la respuesta es 403 o 404 (RN-ACC-012)
```

## 7. Criterios de salida

- [ ] Las pruebas confirman MFA, alcance por consultorio y negar por defecto para **todos** los roles.
- [ ] Un documento finalizado no se modifica ni se elimina por UI, API ni BD.
- [ ] Los eventos críticos, incluidos los administrativos, son trazables.
- [ ] Hay una restauración exitosa y documentada.
- [ ] La matriz de permisos (`03-roles-y-permisos.md`), la retención, los incidentes y el runbook de respaldo están aprobados.
- [ ] Las pruebas negativas de la sección 6 pasan para cada endpoint clínico.
- [ ] Las filas de la Fase 7 en `trazabilidad.md` tienen prueba y evidencia.
