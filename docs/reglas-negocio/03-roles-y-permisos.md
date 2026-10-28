# Matriz de roles y permisos

**Corte:** 22-10-2026 · Fuentes: `Infraestructura_Base_de_Compliance_MVP` (autorización mínima), `Modelo_de_Dominio` §6 y §10, historias US-C1-*, US-C2-*, US-C4-*.

## Principios

1. **Negar por defecto (RN-ACC-011, RN-ACC-012).** El Médico accede solo a los datos de su propio tenant (sus pacientes), compartan o no consultorio con otros médicos. El Administrativo accede solo a los consultorios donde tiene adscripción activa y, dentro de ellos, a todos los médicos que operan ahí, sin contenido clínico. Si no hay política explícita que autorice, la acción se rechaza.
2. **Contenido clínico solo para Médico (RN-ACC-006).** La restricción se aplica en la API y en la BD (RLS + validación de `usuario.tipo`), no ocultando botones.
3. **Cada ✅ de esta tabla necesita una prueba positiva, y cada ❌ en recurso clínico, una prueba negativa** a nivel de API (criterio de salida 6 de la Fase 7).

## Roles

| Rol | Clave | Descripción |
|---|---|---|
| Médico | `medico` | Profesional verificado (cédula). Dueño de su tenant: alcance funcional completo sobre **sus** pacientes, en cualquier consultorio donde esté adscrito (RN-ACC-011). Nunca ve pacientes de otros médicos. |
| Administrativo | `administrativo` | Recepción y agenda. Opera lo administrativo; nunca lo clínico. Ve la agenda de todos los médicos de sus consultorios adscritos y a los pacientes con cita o consulta ahí (RN-ACC-012). |
| Administrador de sistema | `administrador_sistema` | Gestiona consultorios, usuarios y adscripciones. Sin acceso clínico. |
| Soporte | `soporte` | Acceso mínimo, temporal y auditado (RN-ACC-009). |
| Responsable de cumplimiento | `responsable_cumplimiento` | **Reemplazado para la auditoría** por DEC-ARQ-008: el médico dueño consulta la auditoría de su tenant y los auditores externos entran por concesión. Sus otras funciones (ARCO, aviso de privacidad, incidentes, acceso de soporte) **siguen sin actor asignado** → DEC-PEND-006. |
| Auditor externo | `auditor_externo` | Acceso temporal de solo lectura por concesión del médico dueño, con folio del requerimiento (RN-ACC-014). Perfiles: sanitario (solo los expedientes citados) y técnico (solo bitácoras). |

Leyenda: ✅ permitido · ❌ denegado (con prueba negativa) · ⏳ solo con acceso extraordinario temporal · 🔸 propuesta, pendiente de confirmar · — no aplica

## Matriz

### Administración de la plataforma

| Recurso / acción | Médico | Administrativo | Adm. sistema | Soporte | Resp. cumplimiento |
|---|---|---|---|---|---|
| Iniciar sesión con MFA | ✅ | ✅ | ✅ | ✅ | ✅ |
| Crear/editar consultorio | ❌ | ❌ | ✅ | ❌ | ❌ |
| Crear/desactivar usuario | ❌ | ❌ | ✅ | ❌ | ❌ |
| Gestionar adscripciones | ❌ | ❌ | ✅ | ❌ | ❌ |
| Revocar sesiones | Las suyas | Las suyas | ✅ | ❌ | ✅ |

### Pacientes

| Recurso / acción | Médico | Administrativo | Adm. sistema | Soporte | Resp. cumplimiento |
|---|---|---|---|---|---|
| Buscar o listar pacientes | ✅ | ✅ | ❌ | ⏳ | ❌ |
| Dar de alta un paciente | ✅ | ✅ | ❌ | ❌ | ❌ |
| Editar identificación y contacto | ✅ | ✅ | ❌ | ❌ | ❌ |
| Ver ficha administrativa | ✅ | ✅ | ❌ | ⏳ | ❌ |
| Ver expediente clínico | ✅ | ❌ | ❌ | ⏳ | ❌ |
| Capturar alergias o información clínica inicial | ✅ | 🔸 solo en el alta (Pantalla 9) | ❌ | ❌ | ❌ |
| Exportar expediente | ✅ (auditado) | ❌ | ❌ | ❌ | 🔸 para atender ARCO |
| Registrar aceptación del aviso de privacidad | ✅ | ✅ | ❌ | ❌ | ❌ |

### Agenda y consulta

| Recurso / acción | Médico | Administrativo | Adm. sistema | Soporte | Resp. cumplimiento |
|---|---|---|---|---|---|
| Ver agenda | ✅ | ✅ | ❌ | ❌ | ❌ |
| Crear, confirmar, reprogramar o cancelar cita | ✅ | ✅ | ❌ | ❌ | ❌ |
| Marcar no presentado | ✅ | ✅ | ❌ | ❌ | ❌ |
| Registrar llegada o walk-in (crear consulta) | ✅ | ✅ | ❌ | ❌ | ❌ |
| Editar el motivo de una consulta abierta | ✅ | 🔸 | ❌ | ❌ | ❌ |
| Ver datos administrativos de la consulta | ✅ | ✅ | ❌ | ❌ | ❌ |
| Cerrar consulta | ✅ | 🔸 ❌ | ❌ | ❌ | ❌ |

### Contenido clínico

| Recurso / acción | Médico | Administrativo | Adm. sistema | Soporte | Resp. cumplimiento |
|---|---|---|---|---|---|
| Crear o editar el borrador de una nota | ✅ (solo el autor edita) | ❌ | ❌ | ❌ | ❌ |
| Leer una nota | ✅ | ❌ | ❌ | ⏳ | ❌ |
| Finalizar una nota | ✅ (autor) | ❌ | ❌ | ❌ | ❌ |
| Agregar una adenda | ✅ (DEC-PEND-003) | ❌ | ❌ | ❌ | ❌ |
| Historia clínica (crear/leer/finalizar) | ✅ | ❌ | ❌ | ⏳ (lectura) | ❌ |
| Diagnóstico y tratamiento | ✅ | ❌ | ❌ | ❌ | ❌ |
| Subir o leer estudios y adjuntos clínicos | ✅ | 🔸 ❌ | ❌ | ⏳ | ❌ |
| Crear, emitir, imprimir, anular o sustituir receta | ✅ (verificado) | ❌ | ❌ | ❌ | ❌ |

### Cumplimiento

| Recurso / acción | Médico | Administrativo | Adm. sistema | Soporte | Resp. cumplimiento |
|---|---|---|---|---|---|
| Consultar o exportar auditoría | ✅ (solo su tenant) | ❌ | ❌ | ❌ | 🔸 por definir (DEC-PEND-006) |
| Publicar una versión del aviso de privacidad | ❌ | ❌ | 🔸 | ❌ | ✅ |
| Registrar o resolver una solicitud ARCO | ❌ | 🔸 solo registrar | ❌ | ❌ | ✅ |
| Registrar o gestionar un incidente | ❌ | ❌ | ❌ | ❌ | ✅ |
| Otorgar acceso extraordinario a soporte | ❌ | ❌ | ❌ | ❌ | ✅ |
| Cobros (cuando se diseñen) | ✅ | ✅ | ❌ | ❌ | ❌ |

### Auditor externo (RN-ACC-014, DEC-ARQ-008)

| Recurso / acción | Auditor sanitario | Auditor técnico |
|---|---|---|
| Leer los expedientes citados en la orden | ✅ (solo lectura, con vigencia y folio) | ❌ |
| Buscar o listar pacientes fuera de la orden | ❌ | ❌ |
| Consultar bitácoras de auditoría | ❌ | ✅ (sin contenido clínico) |
| Escribir, editar, borrar o agregar adendas | ❌ | ❌ |
| Exportar en bloque | ❌ | ❌ |
| Quién otorga la concesión | Solo el médico dueño del tenant | Solo el médico dueño del tenant |

Toda lectura del auditor se audita ligada a su concesión (RN-AUD-007). La disociación de datos para comités queda fuera del MVP.

## Reglas de presentación por rol (UI)

Estas reglas no sustituyen a la autorización de la API; evitan que la interfaz sugiera un acceso que no existe.

| Pantalla | Médico | Administrativo |
|---|---|---|
| P1 Agenda | Una consulta abierta ofrece "Ver detalle" | "Ver detalle" muestra solo los datos administrativos del encuentro |
| P8 Registrar llegada | Al confirmar: "Consulta creada. Dr(a). X puede iniciar la nota clínica cuando esté listo." | Al confirmar: "Consulta registrada. El paciente aparece ahora en la agenda del médico." Sin acceso a "Iniciar nota" |
| P10 Pacientes | El enlace de la fila dice **"Ver expediente"** | El enlace dice **"Ver ficha"**. El menú solo ofrece "Agendar cita" y "Editar datos de contacto" |
| P6 Shell | Navegación completa | Sin la entrada "Auditoría"; ninguna ruta clínica en el menú |

## Implementación sugerida en colibri-ui

- Guards de ruta (`canMatch`) por rol para `consultation/*/clinic-note`. Si el rol es Administrativo, la ruta no se carga.
- Un servicio `PermissionService.can(accion, recurso)` que lea la matriz anterior (claves como `nota.leer` o `cita.crear`), para no dispersar `if (rol === ...)` por los componentes.
- Las pruebas de UI usan los mismos IDs de acción que la API, para que la matriz sea la única fuente.
