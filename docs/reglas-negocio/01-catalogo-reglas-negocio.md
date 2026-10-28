# Catálogo de reglas de negocio

**Corte:** 22-10-2026 · **Dueño:** Beto · **Estado del documento:** vivo, se actualiza en cada fase.

Este es el documento central de reglas de negocio de Colibrí. Cada regla tiene un ID estable que se cita desde el código, las pruebas, las historias de usuario y la matriz de trazabilidad. Una regla **nunca se borra ni se renumera**: si cambia, se marca como `Reemplazada` y se agrega una nueva con referencia a la anterior.

## Cómo leer este catálogo

| Columna | Significado |
|---|---|
| **ID** | `RN-<DOMINIO>-NNN`. Dominios: GEN, ACC, PAC, CIT, CON, NOT, ADE, RX, AUD, PRIV. |
| **Tipo** | **R** restricción · **V** validación de datos · **D** derivación/cálculo automático · **T** transición de estado · **A** autorización |
| **Origen** | Requisito regulatorio (`CLIN-`, `SIR-`, `PRIV-`, `RX-`, `P3-`), historia (`US-`), decisión (`DEC-`) o sección del modelo de dominio (`MD §n`). |
| **Se aplica en** | Capa donde se hace cumplir: **UI** (solo ayuda visual, nunca suficiente para reglas A/R), **API** (backend), **BD** (constraint, trigger o RLS). |
| **Fase** | Fase de construcción en la que se implementa (ver `../fases/00-roadmap.md`). |
| **Estado** | `Vigente` · `Propuesta` (derivada por ingeniería, falta confirmar) · `Pendiente decisión` (bloqueada por un `DEC-` abierto) · `Reemplazada` |

> **Regla de oro:** toda regla de tipo **A** o **R** debe aplicarse en API (y en BD cuando sea posible). Ocultarla en la UI es una mejora de experiencia, no un control.

---

## RN-GEN — Transversales

| ID | Regla | Tipo | Origen | Se aplica en | Fase | Estado |
|---|---|---|---|---|---|---|
| RN-GEN-001 | Todo timestamp se almacena en UTC (`timestamptz`) y se presenta en la zona horaria del consultorio. | D | US-C3-03 · CLIN-001 | BD · UI | 7 | Vigente |
| RN-GEN-002 | La fecha/hora de finalización, emisión, adenda y auditoría la asigna el servidor; nunca se acepta un timestamp enviado por el cliente. | R | US-C3-03 | API · BD | 7 | Vigente |
| RN-GEN-003 | El autor de un documento clínico es siempre el usuario autenticado; no es un campo capturable. | D | NOM-004 RF-004..006 · CLIN-001 | API | 7 | Vigente |
| RN-GEN-004 | No existe borrado físico de contenido clínico, receta emitida, adenda ni evento de auditoría. Las bajas son lógicas. | R | CLIN-003 · MD §6.7 | API · BD | 7 | Vigente |
| RN-GEN-005 | Cada tenant corresponde a un único responsable jurídico; dos médicos independientes no comparten tenant. | R | DEC-P3-001 | BD | 7 | Reemplazada por RN-GEN-009 (27-10-2026) |
| RN-GEN-006 | Los datos clínicos identificables no se usan para IA, entrenamiento ni analítica propia del producto. | R | DEC-P3-004 · P3-038 | Operación | 7 | Pendiente decisión |
| RN-GEN-007 | Ningún entorno distinto de producción contiene datos clínicos reales. | R | Infra compliance | Operación | 7 | Vigente |
| RN-GEN-008 | El producto no se comercializa (venta o contratación por clientes reales) hasta que la firma electrónica avanzada (e.firma) esté implementada para notas clínicas y adendas. Es condición de salida posterior al MVP, no una mejora opcional. **Prioridad alta.** | R | DEC-ARQ-007 | Operación (release) | Post-MVP | Vigente |
| RN-GEN-009 | La frontera de aislamiento de datos es el **médico**: cada médico es su propio tenant y ningún médico accede a pacientes ni expedientes de otro, aunque compartan consultorio o centro médico. El centro médico es solo una agrupación administrativa de consultorios y personal; no abre acceso entre médicos. | R | DEC-P3-001 | BD (RLS) · API | 7 | Vigente |

## RN-ACC — Identidad, acceso y autorización

| ID | Regla | Tipo | Origen | Se aplica en | Fase | Estado |
|---|---|---|---|---|---|---|
| RN-ACC-001 | Cada usuario tiene una cuenta individual; no existen cuentas compartidas ni genéricas de consultorio. | R | US-C1-01 | API · BD | 7 | Vigente |
| RN-ACC-002 | El segundo factor (MFA/TOTP) es obligatorio antes de crear una sesión. | V | US-C1-01 · PRIV-004 | API | 7 | Vigente |
| RN-ACC-003 | Las sesiones caducan y pueden revocarse de inmediato (sesión persistida en BD, no solo JWT). | R | US-C1-01 | API · BD | 7 | Vigente |
| RN-ACC-004 | Un usuario de tipo Médico debe estar vinculado a un registro de médico con cédula profesional; ningún otro tipo puede tenerlo. | V | US-C1-01 · RX-001 | BD (trigger) | 7 | Vigente |
| RN-ACC-005 | **Negar por defecto:** todo acceso a un recurso exige una adscripción **activa** del usuario al consultorio del recurso. No hay excepción para ningún rol. | A | US-C1-02 | API · BD (RLS) | 7 | Reemplazada por RN-ACC-011 y RN-ACC-012 (27-10-2026) |
| RN-ACC-006 | El rol Administrativo **nunca** crea, lee, edita, finaliza ni exporta contenido clínico (nota, diagnóstico, tratamiento, historia clínica, receta, adjuntos clínicos). Conocer el ID del recurso no cambia nada. | A | MD §6.10 · Infra compliance | API · BD (RLS) | 7 | Vigente |
| RN-ACC-007 | El rol Médico tiene alcance funcional completo **dentro de sus consultorios adscritos activos**. | A | MD §6.11 · DEC-DOM-003 | API · BD | 7 | Reemplazada por RN-ACC-011 (27-10-2026) |
| RN-ACC-008 | Desactivar una adscripción impide accesos futuros, pero no borra ni oculta los registros creados antes. | R | US-C1-03 | API | 7 | Vigente |
| RN-ACC-009 | El acceso de soporte a expedientes es excepcional, autorizado, con expiración obligatoria, y cada acción se marca como acceso extraordinario en auditoría. | A | DEC-P3-007 · US-C4-05 | API | 7 | Pendiente decisión |
| RN-ACC-010 | El consultorio activo es un filtro implícito: la UI nunca muestra pacientes, citas ni consultas de un consultorio sin adscripción vigente. | A | Pantalla 10 · US-C1-02 | UI · API | 8 | Reemplazada por RN-ACC-013 (27-10-2026) |
| RN-ACC-011 | **Negar por defecto (Médico):** un Médico accede únicamente a recursos de su propio tenant (RN-GEN-009). Para operar en un consultorio necesita además una adscripción activa a él. Compartir consultorio o centro con otro médico no concede acceso a los pacientes de ese médico. | A | DEC-P3-001 · US-C1-02 | API · BD (RLS) | 7 | Vigente |
| RN-ACC-012 | **Negar por defecto (Administrativo):** un Administrativo accede a citas, metadatos de consulta y datos generales de paciente solo en los consultorios donde tiene adscripción activa, y dentro de ellos a todos los médicos que operan ahí. Ve a un paciente únicamente si este tiene una cita o consulta en un consultorio de su adscripción. Nunca accede a contenido clínico (RN-ACC-006). | A | DEC-P3-001 | API · BD (RLS) | 7 | Vigente |
| RN-ACC-013 | La UI muestra al Médico solo sus pacientes, en cualquier consultorio donde esté adscrito, y al Administrativo solo lo que permite RN-ACC-012. El consultorio activo es un filtro de trabajo, no el límite de acceso. | A | DEC-P3-001 · Pantalla 10 | UI · API | 8 | Vigente |
| RN-ACC-014 | El acceso de un auditor externo es solo por **concesión temporal de solo lectura**, con folio del requerimiento y autoridad. Perfiles: auditor sanitario (únicamente los expedientes citados en la orden) y auditor técnico (únicamente bitácoras, sin contenido clínico). Solo el médico dueño del tenant puede otorgarla. No hay exportación masiva: la copia certificada la expide el médico. | A | DEC-ARQ-008 | API · BD (RLS) | 7 | Vigente — interpretación pendiente de validar en la Fase 11 |

## RN-PAC — Paciente y expediente

| ID | Regla | Tipo | Origen | Se aplica en | Fase | Estado |
|---|---|---|---|---|---|---|
| RN-PAC-001 | Un paciente tiene **un solo** expediente lógico; todas sus consultas se agregan a él. | R | MD §6.1 · CLIN-002 | BD (UNIQUE) | 8 | Vigente |
| RN-PAC-002 | Alta de paciente: son obligatorios nombre(s), apellido paterno, apellido materno, fecha de nacimiento, sexo registrado y teléfono principal (10 dígitos). | V | Pantalla 9 · NOM-004 DAT-006..009 | UI · API | 8 | Propuesta — ver DEC-PEND-002 |
| RN-PAC-003 | La CURP es opcional; si se captura, se valida su formato (18 caracteres). El sistema nunca la genera. | V | SIR-003 | UI · API | 8 | Vigente |
| RN-PAC-004 | Un paciente con expediente no se elimina; solo puede darse de baja lógica (fuera del MVP). | R | CLIN-003 | API | 8 | Vigente |
| RN-PAC-005 | `retencion_hasta` del expediente es como mínimo **5 años contados desde el último acto médico**, y se recalcula con cada nuevo acto. | D | NOM-004 5.4 · RNF-002 · US-C4-01 · DEC-DOM-006 | API | 7 | Vigente — el detalle está en RN-PAC-011 |
| RN-PAC-006 | La atención a menores está deshabilitada hasta implementar una representación verificable. | R | DEC-P3-002 · P3-031 | API · UI | 8 | Pendiente decisión |
| RN-PAC-007 | "Editar datos de contacto" solo modifica identificación y contacto. La información clínica inicial (alergias) se edita desde el expediente. | A | Pantalla 10 | UI · API | 8 | Vigente |
| RN-PAC-008 | Un Administrativo ve la **ficha administrativa** (identificación, contacto, fechas y motivos de citas y consultas), nunca el expediente clínico. | A | Pantalla 10 · RN-ACC-006 | UI · API | 8 | Reemplazada por RN-PAC-009 (27-10-2026) |
| RN-PAC-009 | Un Administrativo ve la **ficha administrativa**: identificación, contacto, fechas y estado de citas y consultas, y el motivo de la cita. Nunca el expediente clínico ni el motivo de la consulta (RN-CON-010). | A | Pantalla 10 · RN-ACC-006 · DEC-P3-001 | UI · API | 8 | Vigente |
| RN-PAC-010 | Todo paciente pertenece a un médico responsable (su tenant). Cuando el alta la hace un Administrativo, debe elegir al médico responsable. | R | DEC-P3-001 | UI · API · BD | 8 | Propuesta — consecuencia de DEC-P3-001 |
| RN-PAC-011 | `retencion_hasta` se recalcula con cada **acto médico**: finalizar una nota, agregar una adenda, emitir una receta y cerrar una consulta (también si se cierra sin nota). No son actos médicos: crear o reprogramar una cita, el check-in ni las lecturas. Nuevo valor: `GREATEST(retencion_hasta, fecha_del_acto + 5 años)`; nunca retrocede. Se calcula en la capa de dominio, dentro de la misma transacción del acto, **sin trigger de base de datos**. | D | DEC-DOM-006 · NOM-004 5.4 | API | 7 | Vigente |

## RN-CIT — Cita

| ID | Regla | Tipo | Origen | Se aplica en | Fase | Estado |
|---|---|---|---|---|---|---|
| RN-CIT-001 | Una cita requiere paciente, consultorio y fecha/hora programada. El médico es opcional (preferido). | V | MD §2 | API · BD | 8 | Vigente |
| RN-CIT-002 | El motivo de la cita es texto libre opcional. No se valida clínicamente y nunca bloquea ni genera advertencia. | V | MD §10.2 | UI · API | 8 | Vigente |
| RN-CIT-003 | Pueden crear, confirmar, reprogramar y cancelar citas los usuarios Médico o Administrativo con adscripción activa al consultorio. | A | MD §5 | API | 8 | Vigente |
| RN-CIT-004 | Solo se permiten las transiciones definidas en `02-ciclos-de-vida.md § Cita`. Los estados `convertida`, `cancelada` y `no_presentado` son finales. | T | MD §5 | API | 8 | Vigente |
| RN-CIT-005 | Al convertirse en consulta, el motivo de la cita se copia a la consulta. | D | MD §10.2 | API | 8 | Vigente |
| RN-CIT-006 | Una cita se convierte en una sola consulta (1:1). | R | Derivada | BD (UNIQUE `consulta.cita_id`) | 8 | Propuesta |
| RN-CIT-007 | La cita no es un documento clínico: no le aplica inmutabilidad, pero sus cambios se auditan. | R | MD §10.2 | API | 8 | Vigente |

## RN-CON — Consulta

| ID | Regla | Tipo | Origen | Se aplica en | Fase | Estado |
|---|---|---|---|---|---|---|
| RN-CON-001 | Una consulta requiere médico, consultorio, expediente del paciente y fecha/hora. | V | MD §6.2 | API · BD | 8 | Vigente |
| RN-CON-002 | Una consulta nace de dos maneras: por check-in de una cita (`cita_id`) o como walk-in (`cita_id = null`). La pueden registrar Médico o Administrativo. | T | MD §10.1 | API | 8 | Vigente |
| RN-CON-003 | El motivo de la consulta es editable mientras esté `abierta`; no bloquea la creación si llega vacío. | V | MD §6.2 · §10.2 | API | 8 | Vigente |
| RN-CON-004 | Una consulta **no puede cerrarse** sin una nota `finalizada` o sin una causa documentada (`causa_cierre_sin_nota`). | T | MD §5 | API · BD (trigger) | 8 | Vigente |
| RN-CON-005 | Que el paciente se retire físicamente no cambia ningún estado de forma automática; la nota puede quedar en borrador después de que el paciente se fue. | R | MD §8 | API | 8 | Vigente |
| RN-CON-006 | Solo un Médico puede iniciar o editar la nota clínica de una consulta. | A | MD §5 | API · BD (RLS) | 8 | Vigente |
| RN-CON-007 | La agenda muestra un indicador **no bloqueante** de consultas con nota pendiente de finalizar. No hay bloqueo por antigüedad del borrador. | D | MD §8 · DEC-DOM-004 | UI | 8 | Propuesta (validar en piloto) |
| RN-CON-008 | En el check-in, la hora real de llegada es editable y puede diferir de la hora agendada. | V | Pantalla 8 | UI · API | 8 | Vigente |
| RN-CON-009 | Solo un Médico puede cerrar una consulta, y cerrarla sin nota exige capturar la causa. | A | Derivada | API | 8 | Propuesta — ver DEC-PEND-008 |
| RN-CON-010 | El motivo de la cita es visible para el Administrativo porque él lo captura. El motivo de la consulta, una vez editado por el médico, es contenido clínico y no se expone al Administrativo. | A | DEC-P3-001 | API · UI | 8 | Propuesta — falta definir el walk-in (motivo capturado en el check-in) |

## RN-NOT — Nota clínica (y por extensión historia clínica)

| ID | Regla | Tipo | Origen | Se aplica en | Fase | Estado |
|---|---|---|---|---|---|---|
| RN-NOT-001 | Cada consulta tiene una nota de evolución; las correcciones posteriores se hacen con adendas, no con notas nuevas. La regla vive en el dominio; `UNIQUE (consulta_id, tipo)` es respaldo declarativo, no trigger. | R | Pantalla 2 · DEC-DOM-007 | API · BD (UNIQUE) | 8 | Vigente |
| RN-NOT-002 | Para finalizar la nota se requiere: evolución/padecimiento actual, al menos un diagnóstico, tratamiento/indicaciones y pronóstico. | V | NOM-004 DAT-034..040 | UI · API | 8 | Reemplazada por RN-NOT-011 (27-10-2026) |
| RN-NOT-003 | Los signos vitales y los estudios relevantes son condicionales y nunca bloquean la finalización. | V | DAT-035 · DAT-036 | UI · API | 8 | Reemplazada por RN-NOT-011 (27-10-2026) |
| RN-NOT-004 | El motivo **no** es un campo de la nota: se muestra heredado de la consulta, en solo lectura. | R | MD §3 | UI | 8 | Vigente |
| RN-NOT-005 | "Resumen clínico" no es un campo de la nota; es un documento distinto que se genera a partir de notas finalizadas. | R | MD §2 (nota) | UI | 8 | Vigente |
| RN-NOT-006 | Solo el autor puede editar el borrador de una nota. | A | MD §5 | API | 8 | Vigente |
| RN-NOT-007 | Finalizar es irreversible: fija un snapshot canónico, el número de versión, un hash SHA-256, el autor y la fecha/hora del servidor. La UI pide confirmación explícita antes de hacerlo. | T | US-C3-01 · SIR-001 | UI · API · BD | 7 (mecanismo) · 8 (pantalla) | Vigente |
| RN-NOT-008 | Cualquier intento de editar o borrar una nota finalizada se rechaza (en UI, API y BD) y queda auditado. | R | US-C3-01 · CLIN-004 | API · BD | 7 | Vigente |
| RN-NOT-009 | Una nota solo puede iniciarse mientras su consulta esté `abierta`. | V | Derivada | API | 8 | Propuesta |
| RN-NOT-010 | En el MVP la finalización se respalda con sesión autenticada con MFA, confirmación explícita, hash SHA-256 y hora del servidor; no se usa e.firma. El mecanismo se registra en `firma_constancia.mecanismo` y es extensible a `fea_sat` y `fea_psc`. | R | DEC-ARQ-007 | API · BD | 7 | Vigente — transitoria hasta cumplir RN-GEN-008 |
| RN-NOT-011 | Para finalizar una nota se requiere, completo: (1) **encabezado**: nombre completo del paciente, edad, sexo, número de expediente, fecha/hora y médico con cédula (los asigna el sistema, RN-GEN-003); (2) evolución y actualización del cuadro clínico; (3) **signos vitales**: TA, FC, FR y temperatura, sin excepción (peso, talla e IMC son complementarios y opcionales); (4) **declaración de estudios**: el médico elige entre «con estudios» (resumen de hallazgos y origen) y «sin estudios» (genera la leyenda «No se presentan ni aportan estudios auxiliares en esta consulta.»), sin valor preseleccionado; (5) al menos un diagnóstico, todos con **código CIE-10**, y al menos uno marcado principal; (6) plan de estudio y/o tratamiento e indicaciones; (7) **pronóstico**, sin valor preseleccionado (estructura pendiente: DEC-PRD-001); (8) respaldo de firma según RN-NOT-010. La validación vive en el dominio. | V | DEC-DOM-008 · DEC-PRD-001 · DEC-PRD-002 · NOM-004 DAT-034..040 | UI · API | 8 | Vigente — numerales de la norma por validar en la Fase 11 |
| RN-NOT-012 | Un estudio adjunto de origen externo registra su origen (externo o aportado por el paciente), la fecha real del estudio, la institución emisora y un hash SHA-256; queda vinculado a la nota donde se analizó y no puede reemplazarse ni borrarse. | R | DEC-DOM-008 | API · BD | 8 | Vigente — numerales de la norma por validar en la Fase 11 |

## RN-ADE — Correcciones (adendas)

| ID | Regla | Tipo | Origen | Se aplica en | Fase | Estado |
|---|---|---|---|---|---|---|
| RN-ADE-001 | Una corrección se registra como una adenda nueva: autor, fecha/hora del servidor, **motivo obligatorio**, contenido y vínculo al documento finalizado. | V | US-C3-02 · CLIN-004 | API | 7 | Vigente |
| RN-ADE-002 | El documento original permanece inalterado; solo cambia su marcador de estado a `corregida_por_adenda`. | R | US-C3-02 | API · BD | 7 | Vigente |
| RN-ADE-003 | Una adenda es append-only: no se edita ni se elimina. | R | Diseño BD §7 | BD (REVOKE) | 7 | Vigente |
| RN-ADE-004 | Solo puede crear adendas un Médico con adscripción activa. | A | Derivada | API | 8 | Pendiente decisión — ver DEC-PEND-003 |
| RN-ADE-005 | La cadena de versiones y adendas puede consultarse y exportarse. | D | US-C3-02 | API | 7 | Vigente |
| RN-ADE-006 | Las adendas se firman con el mismo mecanismo que la nota (RN-NOT-010 en el MVP; e.firma después, según RN-GEN-008). | R | DEC-ARQ-007 | API | 7 | Vigente |

## RN-RX — Receta (Fase 9)

| ID | Regla | Tipo | Origen | Se aplica en | Fase | Estado |
|---|---|---|---|---|---|---|
| RN-RX-001 | Una receta solo se emite desde una consulta con nota clínica finalizada. | T | MD §6.3 | API · BD (trigger) | 9 | Vigente |
| RN-RX-002 | La receta exige paciente, médico con cédula, consultorio con domicilio y fecha; cada línea exige genérico, presentación, concentración, dosis, vía, frecuencia, duración y cantidad. | V | RX-001 · MD §6.4 | API · BD | 9 | Vigente |
| RN-RX-003 | La denominación distintiva es opcional; los resurtimientos son explícitos (por defecto 0). | V | RX-002 | API · BD | 9 | Vigente |
| RN-RX-004 | Se deniega cualquier línea de un medicamento controlado (estupefacientes, psicotrópicos II–IV, cannabis, especiales) desde el catálogo, la API o una importación. | R | RX-005 · MD §6.5 | API · BD (CHECK) | 9 | Vigente |
| RN-RX-005 | En los antibióticos, la cantidad y la duración son obligatorias. | V | RX-004 · MD §6.6 | API | 9 | Vigente |
| RN-RX-006 | La receta emitida es un snapshot inmutable con hash. | R | RX-007 · SIR-001 | API · BD | 9 | Vigente |
| RN-RX-007 | Anular conserva el motivo; sustituir enlaza la receta nueva con la anterior. Nunca se borra. | T | RX-007 | API · BD | 9 | Vigente |
| RN-RX-008 | La firma es autógrafa sobre la impresión. El sistema registra la creación, la fijación, la generación y la impresión; no declara dispensación ni validez electrónica universal. | R | RX-003 · MD §6.9 | API · UI | 9 | Vigente |
| RN-RX-009 | El estado "dispensada" no existe en el MVP. | R | MD §6.6 | BD (CHECK) | 9 | Vigente |
| RN-RX-010 | La receta se entrega al paciente; no se envía directamente a farmacias. | R | DEC-P3-003 | — | 9 | Pendiente decisión |
| RN-RX-011 | Solo un médico con `estado_verificacion = verificado` puede emitir recetas. | A | RX-001 | API | 9 | Vigente |

## RN-AUD — Auditoría

| ID | Regla | Tipo | Origen | Se aplica en | Fase | Estado |
|---|---|---|---|---|---|---|
| RN-AUD-001 | Se auditan como mínimo: sesión, MFA, fallos de autenticación, lectura/creación/exportación de expediente, perfil/adscripción, citas, alta y edición de pacientes, ciclo de notas, ciclo de recetas, adjuntos, respaldos y permisos. | D | Infra compliance · SIR-002 | API (middleware) | 7 | Vigente |
| RN-AUD-002 | Cada evento contiene actor, acción, recurso, resultado, timestamp UTC, correlación y origen/sesión. | V | US-C2-01 | API · BD | 7 | Vigente |
| RN-AUD-003 | La bitácora nunca guarda contraseñas, tokens ni contenido clínico completo. | R | US-C2-01 | API | 7 | Vigente |
| RN-AUD-004 | La bitácora es append-only: el rol de aplicación no tiene `UPDATE` ni `DELETE`. | R | US-C2-01 | BD (REVOKE + trigger) | 7 | Vigente |
| RN-AUD-005 | Las denegaciones de autorización y los intentos de editar documentos finalizados se auditan con resultado `denegado`. | D | US-C1-02 · US-C3-01 | API | 7 | Vigente |
| RN-AUD-006 | Solo un rol autorizado consulta y exporta la bitácora, y cada exportación genera su propio evento. | A | US-C2-02 | API | 7 | Vigente |
| RN-AUD-007 | Todo evento generado bajo una concesión de acceso la referencia (`concesion_id`), de modo que cada lectura de un auditor queda ligada a su folio de requerimiento. | D | DEC-ARQ-008 | API · BD | 7 | Vigente |

## RN-PRIV — Privacidad, retención y continuidad

| ID | Regla | Tipo | Origen | Se aplica en | Fase | Estado |
|---|---|---|---|---|---|---|
| RN-PRIV-001 | El aviso de privacidad tiene versión, vigencia e idioma. Publicar una versión nueva cierra la anterior, sin borrarla. | R | US-C4-03 · PRIV-002 | API · BD | 7 | Vigente |
| RN-PRIV-002 | La aceptación o puesta a disposición del aviso registra versión, sujeto y fecha/hora. | D | US-C4-03 | API | 7 | Vigente |
| RN-PRIV-003 | No se usa aviso de privacidad corto. | R | DEC-P3-005 | — | 7 | Pendiente decisión |
| RN-PRIV-004 | El consentimiento para datos sensibles se documenta en la atención ordinaria y se mantiene separado del consentimiento informado médico. | V | P3-004 · P3-005 | API | 8 | Vigente |
| RN-PRIV-005 | Una solicitud de eliminación se registra y se resuelve conforme a la política; nunca ejecuta un borrado mientras dure la retención. | R | US-C4-01 · P3-017 | API | 7 | Vigente |
| RN-PRIV-006 | Una solicitud ARCO registra identidad, derecho, evidencia, fecha de recepción, plazo, estado y resolución. | V | US-C4-04 · PRIV-003 | API | 7 | Vigente |
| RN-PRIV-007 | Exportar un expediente requiere autorización explícita y queda auditado. | A | SIR-004 | API | 8 | Vigente |
| RN-PRIV-008 | El sistema no almacena datos de tarjeta; el proveedor de pagos los recibe directamente. | R | DEC-P3-006 | — | Futuro | Pendiente decisión |
| RN-PRIV-009 | Los respaldos se hacen automáticamente, cifrados, fuera del entorno primario, con una restauración de prueba documentada cada trimestre. | R | US-C4-02 · PRIV-004 | Operación | 7 | Vigente |

---

## Cómo agregar o cambiar una regla

1. Asigna el siguiente número libre del dominio. Nunca reutilices un ID.
2. Llena **Origen**. Si la regla no viene de una norma, cita la decisión (`DEC-`) que la justifica; si esa decisión no existe, regístrala primero en `04-registro-decisiones.md`.
3. Si la regla cambia un estado, actualiza también `02-ciclos-de-vida.md`. Si cambia quién puede hacer algo, actualiza `03-roles-y-permisos.md`.
4. Agrega la fila correspondiente en `../fases/trazabilidad.md` con la prueba que la verifica.
5. En el código, referencia el ID en el mensaje de error o en el nombre de la prueba (ej. `it('RN-CON-004: rechaza cerrar consulta sin nota finalizada')`).
