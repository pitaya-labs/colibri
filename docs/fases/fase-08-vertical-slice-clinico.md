# Fase 8 — Vertical slice clínico

- **Estado:** 8.0 (prototipo de UI) en curso en colibri-ui · 8 (integración) por iniciar
- **Dueño:** Beto
- **Entregable (según `Ruta.md`):** primer flujo clínico funcional del MVP.

## 1. Objetivo

Un médico y una recepcionista operan un día completo: registrar al paciente, agendarlo o recibirlo sin cita, documentar la consulta, finalizar y corregir la nota, cerrar la consulta y consultar el historial.

## 2. Alcance

| Incluye | No incluye |
|---|---|
| Pacientes: listado, alta, edición de contacto, ficha y expediente | Receta (Fase 9) |
| Agenda: citas, check-in, walk-in | Referencia y resumen clínico |
| Consulta: detalle y cierre | Atención a menores (DEC-P3-002) |
| Nota de evolución: borrador, finalizar, adenda | Cobros |
| Historial del expediente | Historia clínica completa *(se decide si entra; aún no tiene pantalla)* |

## 3. Decisiones bloqueantes

| ID | Descripción | Estado | ¿Bloquea? |
|---|---|---|---|
| Fase 7 | Infraestructura de compliance | Sin construir | **Sí** (para la 8; no para la 8.0) |
| DEC-PRD-001 | ¿Pronóstico obligatorio? | Aprobada en parte: obligatorio; falta su estructura | Sí — US-8-12 (solo la estructura) |
| DEC-PEND-002 | ¿Apellido materno obligatorio? | Pendiente | Sí — US-8-02 |
| DEC-PEND-003 | ¿Quién crea adendas? | Pendiente | Sí — US-8-13 |
| DEC-PEND-004 | ¿Se puede descartar un borrador? | Pendiente | No |
| DEC-PEND-007 | ¿Una nota por consulta? | **Resuelta**: DEC-DOM-007 | No |
| DEC-PEND-008 | ¿Quién cierra la consulta? | Pendiente | Sí — US-8-14 |
| DEC-PEND-009 | Alergias capturadas por el Administrativo | Pendiente | No |
| DEC-UI-001 | Tabla de la agenda: PrimeNG o CdkTable | Pendiente | No |

## 4. Reglas de negocio que implementa

RN-ACC-011..013 · RN-PAC-001..007, -009..011 · RN-CIT-001..007 · RN-CON-001..010 · RN-NOT-001, -004..012 · RN-ADE-004 · RN-PRIV-004, -007.

## 5. Épicas e historias

Estado UI = avance en colibri-ui (8.0) · Estado API = avance de la integración (8).

### Épica E8-1 — Pacientes

#### US-8-01 · Buscar pacientes — P0 · Pantalla P10 · `patients/`

Como **Médico o Administrativo**, quiero buscar pacientes por nombre o apellido para encontrar rápidamente a quien atiendo.

```gherkin
Escenario: Un Médico busca solo entre sus pacientes
  Dado que soy Médico y comparto el consultorio C1 con otro Médico
  Cuando escribo "gonz" en la búsqueda
  Entonces veo solo mis pacientes cuyo nombre o apellido contiene "gonz", nunca los del otro Médico (RN-ACC-011, RN-ACC-013)

Escenario: Un Administrativo busca entre los pacientes de sus consultorios
  Dado que soy Administrativo con adscripción activa solo al consultorio C1
  Cuando escribo "gonz" en la búsqueda
  Entonces veo solo pacientes con cita o consulta en C1, de cualquier Médico que opere ahí (RN-ACC-012)

Escenario: Enlace según el rol
  Dado que soy Administrativo
  Entonces cada fila ofrece "Ver ficha" y nunca "Ver expediente" (RN-PAC-009)
```

Trazabilidad: CLIN-002 · RN-ACC-011..013, RN-PAC-009 · UI: en progreso · API: pendiente

#### US-8-02 · Dar de alta un paciente — P0 · P9 · `patients/create`

Como **Médico o Administrativo**, quiero registrar a un paciente nuevo en menos de dos minutos.

```gherkin
Escenario: Alta mínima
  Dado que capturo nombre(s), apellidos, fecha de nacimiento, sexo y teléfono
  Cuando presiono "Registrar paciente"
  Entonces se crea el paciente y su único expediente (RN-PAC-001)
  Y se calcula retencion_hasta (RN-PAC-005)
  Y se audita "paciente.creado"

Escenario: CURP con formato inválido
  Cuando capturo una CURP de 17 caracteres
  Entonces el campo muestra un error de formato y no se guarda (RN-PAC-003)

Escenario: Alta embebida desde la agenda
  Dado que abrí el alta desde "Agendar cita"
  Cuando registro al paciente
  Entonces vuelvo a la cita con el paciente preseleccionado

Escenario: El Administrativo elige al médico responsable
  Dado que soy Administrativo y doy de alta a un paciente
  Entonces debo elegir al médico responsable, que será el dueño del paciente y su expediente (RN-PAC-010)
```

Trazabilidad: SIR-003 · RN-PAC-001..003, -005 · UI: en progreso · API: pendiente

#### US-8-03 · Editar los datos de contacto — P1 · P9 (modo edición)

```gherkin
Escenario: Solo identificación y contacto
  Dado que abro "Editar datos de contacto" desde el listado
  Entonces solo las secciones 1 y 2 son editables (RN-PAC-007)
  Y se audita "paciente.modificado"
```

#### US-8-04 · Ver el expediente o la ficha — P0

```gherkin
Escenario: Administrativo intenta abrir el expediente por URL
  Dado que soy Administrativo
  Cuando navego directo a la ruta del expediente clínico
  Entonces la ruta no carga (guard) y la API responde 403 (RN-ACC-006)
```

### Épica E8-2 — Agenda

#### US-8-05 · Ver la agenda del día — P0 · P1 · `agenda/`

```gherkin
Escenario: Citas y consultas se distinguen de un vistazo
  Entonces cada fila muestra el badge de tipo (Cita/Consulta) y solo las acciones válidas para su estado (RN-CIT-004)

Escenario: Indicador de nota pendiente
  Dado una consulta abierta con la nota en borrador
  Entonces la fila muestra un indicador no bloqueante "Nota pendiente" (RN-CON-007)
  Y el Administrativo no ve el estado de la nota (RN-ACC-006)
```

UI: en progreso · API: pendiente

#### US-8-06 · Agendar una cita — P0 · P7 · `agenda/create`

```gherkin
Escenario: Motivo vacío permitido
  Dado paciente, consultorio y fecha/hora
  Y el motivo vacío
  Cuando presiono "Agendar cita"
  Entonces se crea la cita en estado "agendada" sin ninguna advertencia (RN-CIT-001, -002)
```

UI: en progreso · API: pendiente

#### US-8-07 · Confirmar, reprogramar, cancelar o marcar no presentado — P1 · P1

```gherkin
Escenario: Transición inválida
  Dado una cita "cancelada"
  Cuando intento registrar la llegada
  Entonces se rechaza (RN-CIT-004)
```

#### US-8-08 · Registrar la llegada desde una cita — P0 · P8 variante A

```gherkin
Escenario: Check-in
  Dado una cita "agendada"
  Cuando confirmo la llegada con la hora real
  Entonces la cita pasa a "convertida" y se crea una consulta "abierta" con cita_id y el motivo heredado, en una sola transacción (RN-CON-002, RN-CIT-005, -006)
  Y el mensaje de confirmación depende del rol

Escenario: Doble check-in
  Cuando intento convertir la misma cita otra vez
  Entonces se rechaza (RN-CIT-006)
```

#### US-8-09 · Registrar un walk-in — P0 · P8 variante B

```gherkin
Escenario: Walk-in sin cita
  Dado un paciente existente o recién dado de alta, y un médico del consultorio
  Cuando presiono "Registrar consulta"
  Entonces se crea una consulta "abierta" con cita_id = null (RN-CON-002)
```

### Épica E8-3 — Consulta y nota clínica

#### US-8-10 · Ver el detalle de una consulta — P0 · P2 · `consultation/detail`

```gherkin
Escenario: Acción según el estado de la nota
  Entonces veo "Iniciar nota", "Continuar nota" o la nota en solo lectura, según el estado
  Y "Cerrar consulta" está deshabilitado si no hay nota finalizada ni causa (RN-CON-004)
```

UI: en progreso · API: pendiente

#### US-8-11 · Documentar la nota en borrador — P0 · P3 · `consultation/clinical-note`

```gherkin
Escenario: Motivo heredado en solo lectura
  Entonces el motivo aparece en el encabezado de contexto, no como campo (RN-NOT-004)

Escenario: Solo el autor edita
  Dado una nota en borrador del médico A
  Cuando el médico B intenta guardarla
  Entonces se rechaza (RN-NOT-006)
```

UI: en progreso · API: pendiente

#### US-8-12 · Finalizar la nota — P0 · P3 → P4

```gherkin
Escenario: Faltan campos obligatorios
  Dado una nota sin diagnóstico
  Cuando presiono "Finalizar nota"
  Entonces se indica qué falta y la nota sigue en borrador (RN-NOT-011)

Escenario: Sin signos vitales no se finaliza
  Dado evolución, diagnóstico con CIE-10, pronóstico, tratamiento y declaración de estudios completos, sin signos vitales
  Cuando presiono "Finalizar nota"
  Entonces se indica que faltan TA, FC, FR y temperatura y la nota sigue en borrador (RN-NOT-011)

Escenario: Finalización completa
  Dado todos los datos obligatorios de RN-NOT-011 completos
  Cuando confirmo la finalización en el diálogo
  Entonces la nota queda "finalizada" con su versión, hash, autor y hora del servidor (RN-NOT-007, RN-NOT-010)
  Y se recalcula la retención del expediente (RN-PAC-011)

Escenario: Intento de edición posterior
  Cuando se envía un PUT sobre la nota finalizada
  Entonces la respuesta es 409, la BD rechaza el cambio y se audita "documento.edicion_rechazada" (RN-NOT-008)
```

#### US-8-13 · Agregar una adenda — P0 · P5

```gherkin
Escenario: Adenda con motivo
  Dado una nota finalizada
  Cuando guardo una adenda con motivo y contenido
  Entonces el original no cambia, la adenda aparece en el historial y la nota pasa a "corregida_por_adenda" (RN-ADE-001, -002)

Escenario: Sin motivo
  Entonces no se puede guardar (RN-ADE-001)
```

#### US-8-14 · Cerrar la consulta — P0 · P2

```gherkin
Escenario: Cierre sin nota requiere causa
  Dado una consulta sin nota finalizada
  Cuando intento cerrarla sin causa
  Entonces se rechaza (RN-CON-004)
```

#### US-8-15 · Consultar el historial del expediente — P0

Como **Médico**, quiero ver en orden cronológico las consultas, las notas finalizadas y las adendas de un paciente.

```gherkin
Escenario: Lectura auditada
  Cuando abro el expediente
  Entonces se audita "expediente.leido" (RN-AUD-001)
```

## 6. Mapa de pantallas a rutas de colibri-ui

| Pantalla | Ruta actual | Componente | Historias |
|---|---|---|---|
| P1 Agenda | `/agenda` | `features/agenda/list` | US-8-05, -07 |
| P7 Agendar cita | `/agenda/create` | `features/agenda/create` | US-8-06 |
| P8 Registrar llegada | — *(modal por crear)* | — | US-8-08, -09 |
| P10 Pacientes | `/patients` | `features/patient/list` | US-8-01 |
| P9 Alta de paciente | `/patients/create` | `features/patient/create-edit` | US-8-02, -03 |
| P2 Detalle de consulta | `/consultation/detail` | `features/consultation/detail` | US-8-10, -14 |
| P3/P4 Nota | `/consultation/clinical-note` | `features/consultation/clinic-note` | US-8-11, -12 |
| P5 Adenda | — *(modal por crear)* | — | US-8-13 |
| P6 Shell | layout | `layout/main` | — |
| Expediente/historial | — *(sin diseño)* | — | US-8-04, -15 |

## 7. Criterios de salida

- [ ] El flujo completo funciona de punta a punta con los roles Médico y Administrativo.
- [ ] Cada RN de esta fase tiene una prueba en `trazabilidad.md`.
- [ ] Cada paso genera su evento de auditoría, verificado en una prueba.
- [ ] Las pruebas negativas del Administrativo sobre P3, P4, P5 y el expediente pasan en API y UI.
- [ ] DEC-PRD-001 y las DEC-PEND bloqueantes están resueltas.
