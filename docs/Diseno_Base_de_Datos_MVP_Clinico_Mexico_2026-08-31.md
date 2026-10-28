# Diseño de base de datos — MVP clínico ambulatorio México

**Corte:** 31-08-2026 · **Motor:** PostgreSQL (por transacciones ACID, integridad referencial fuerte y Row-Level Security nativo, según `Propuesta_Diseno_Inicial_Fase7_Compliance_MVP`).

Este documento consolida en un esquema físico único lo que hasta ahora vivía repartido en tres documentos: el modelo conceptual (`Modelo_de_Dominio_MVP_Clinico_Mexico`, con la entidad `Cita` y el rol Administrativo ya incorporados), el diseño de la capa de compliance (`Propuesta_Diseno_Inicial_Fase7_Compliance_MVP`) y el esquema de campos de `NotaClínica` (sección 3 del modelo de dominio). No introduce entidades nuevas — traduce lo ya decidido a tablas, tipos y llaves.

## 0. Convenciones generales

- Todas las llaves primarias son `uuid` (`default gen_random_uuid()`; requiere la extensión `pgcrypto`).
- Todo timestamp es `timestamptz`, almacenado en UTC (US-C3-03); la zona horaria de presentación se resuelve en la capa de aplicación a partir de `consultorio.zona_horaria`, nunca en la base de datos.
- **Multi-tenencia (DEC-P3-001):** cada responsable jurídico (médico independiente o consultorio) es un `tenant` propio; no se permite que dos médicos independientes compartan tenant. `tenant_id` se denormaliza en las tablas de mayor volumen (no solo en las raíz) para que las políticas de Row-Level Security no dependan de joins costosos — es el mismo patrón que ya proponía la sección de autorización de `Propuesta_Diseno_Inicial_Fase7_Compliance_MVP`.
- Los campos de texto libre "de baja fricción" (`cita.motivo`, `consulta.motivo`) son `text` nullable — el modelo de dominio (sección 10) exige explícitamente que puedan quedar vacíos o ambiguos sin bloquear ningún flujo.
- Los estados (`estado`) se restringen con `CHECK` a los valores definidos en el modelo de dominio, no con tablas de catálogo separadas — son enumeraciones pequeñas y estables para el MVP.
- El rol de aplicación que usa el backend tiene permisos `INSERT`/`SELECT` pero **no** `UPDATE`/`DELETE` sobre `auditoria` y `documento_version` (ver sección 7 y 8) — es el mecanismo de inmutabilidad a nivel de base de datos, no solo de aplicación.

---

## 1. Identidad y acceso

```sql
CREATE TABLE tenant (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  nombre_responsable    text NOT NULL,          -- responsable jurídico (persona física o razón social)
  rfc                   text,
  creado_en             timestamptz NOT NULL DEFAULT now(),
  estado                text NOT NULL DEFAULT 'activo'
    CHECK (estado IN ('activo', 'suspendido'))
);

CREATE TABLE medico (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  nombre_completo       text NOT NULL,
  cedula_profesional    text NOT NULL,
  cedula_especialidad   text,
  estado_verificacion   text NOT NULL DEFAULT 'pendiente'
    CHECK (estado_verificacion IN ('pendiente', 'verificado', 'rechazado')),
  creado_en             timestamptz NOT NULL DEFAULT now(),
  UNIQUE (tenant_id, cedula_profesional)
);

CREATE TABLE usuario (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  tipo                  text NOT NULL
    CHECK (tipo IN ('medico', 'administrativo', 'administrador_sistema', 'soporte')),
  medico_id             uuid REFERENCES medico(id),   -- NOT NULL solo si tipo = 'medico', validado por trigger (ver abajo)
  nombre_completo       text NOT NULL,
  correo                text NOT NULL,
  estado                text NOT NULL DEFAULT 'activo'
    CHECK (estado IN ('activo', 'suspendido', 'baja')),
  mfa_habilitado        boolean NOT NULL DEFAULT false,
  ultimo_acceso         timestamptz,
  creado_en             timestamptz NOT NULL DEFAULT now(),
  UNIQUE (tenant_id, correo)
);

-- Un usuario tipo 'medico' debe tener medico_id; ningún otro tipo debe tenerlo.
CREATE FUNCTION fn_validar_usuario_medico() RETURNS trigger AS $$
BEGIN
  IF NEW.tipo = 'medico' AND NEW.medico_id IS NULL THEN
    RAISE EXCEPTION 'usuario tipo medico requiere medico_id';
  ELSIF NEW.tipo <> 'medico' AND NEW.medico_id IS NOT NULL THEN
    RAISE EXCEPTION 'solo un usuario tipo medico puede tener medico_id';
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validar_usuario_medico
  BEFORE INSERT OR UPDATE ON usuario
  FOR EACH ROW EXECUTE FUNCTION fn_validar_usuario_medico();

-- Credenciales MFA separadas de la tabla de usuario, nunca en texto plano ni en logs (US-C1-01).
CREATE TABLE mfa_credencial (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  usuario_id            uuid NOT NULL REFERENCES usuario(id),
  tipo                  text NOT NULL DEFAULT 'totp' CHECK (tipo IN ('totp')),
  secreto_cifrado       bytea NOT NULL,          -- cifrado a nivel de aplicación antes de escribir
  creado_en             timestamptz NOT NULL DEFAULT now(),
  estado                text NOT NULL DEFAULT 'activo' CHECK (estado IN ('activo', 'revocado'))
);

-- Sesiones como filas, no solo JWT — necesario para revocación inmediata (US-C1-01).
CREATE TABLE sesion (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  usuario_id            uuid NOT NULL REFERENCES usuario(id),
  estado                text NOT NULL DEFAULT 'activa' CHECK (estado IN ('activa', 'revocada', 'expirada')),
  creada_en             timestamptz NOT NULL DEFAULT now(),
  expira_en             timestamptz NOT NULL,
  origen_ip             inet,
  user_agent            text
);

CREATE TABLE consultorio (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  nombre                text NOT NULL,
  domicilio             text NOT NULL,
  contacto              text,
  zona_horaria          text NOT NULL DEFAULT 'America/Mexico_City',  -- IANA tz name, US-C3-03
  estado                text NOT NULL DEFAULT 'activo' CHECK (estado IN ('activo', 'inactivo')),
  creado_en             timestamptz NOT NULL DEFAULT now()
);

-- Generalizada de AdscripciónMédico: aplica a médicos y a personal administrativo (sección 10 del modelo de dominio).
CREATE TABLE adscripcion (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  usuario_id            uuid NOT NULL REFERENCES usuario(id),
  consultorio_id        uuid NOT NULL REFERENCES consultorio(id),
  rol                   text NOT NULL CHECK (rol IN ('medico', 'administrativo')),
  vigencia_desde        date NOT NULL,
  vigencia_hasta        date,
  estado                text NOT NULL DEFAULT 'activa' CHECK (estado IN ('activa', 'inactiva')),
  creada_en             timestamptz NOT NULL DEFAULT now(),
  UNIQUE (usuario_id, consultorio_id, vigencia_desde)
);
```

---

## 2. Pacientes y representación

```sql
CREATE TABLE paciente (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  nombre_completo       text NOT NULL,
  fecha_nacimiento      date NOT NULL,
  sexo                  text,
  curp                  text,                    -- validable cuando aplique; NULL si no disponible (SIR-003)
  contacto              text,
  creado_en             timestamptz NOT NULL DEFAULT now(),
  creado_por_usuario_id uuid NOT NULL REFERENCES usuario(id)  -- puede ser Administrativo o Médico
);

CREATE TABLE representante (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  nombre_completo       text NOT NULL,
  contacto              text,
  parentesco_mandato    text NOT NULL,
  evidencia_verificacion text,                    -- referencia al documento probatorio
  creado_en             timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE relacion_representacion (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  paciente_id           uuid NOT NULL REFERENCES paciente(id),
  representante_id      uuid NOT NULL REFERENCES representante(id),
  tipo                  text NOT NULL,
  vigencia_desde        date NOT NULL,
  vigencia_hasta        date,
  evidencia             text NOT NULL,
  estado                text NOT NULL DEFAULT 'pendiente'
    CHECK (estado IN ('pendiente', 'verificada', 'revocada', 'vencida'))
  -- DEC-P3-002: mientras no se apruebe, el flujo de atención a menores permanece deshabilitado en la capa de aplicación.
);
```

---

## 3. Agenda (Cita)

```sql
CREATE TABLE cita (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  paciente_id           uuid NOT NULL REFERENCES paciente(id),
  consultorio_id        uuid NOT NULL REFERENCES consultorio(id),
  medico_id             uuid REFERENCES medico(id),        -- opcional/preferido
  creada_por_usuario_id uuid NOT NULL REFERENCES usuario(id),  -- Médico o Administrativo
  fecha_hora_programada timestamptz NOT NULL,
  motivo                text,                              -- libre, puede quedar NULL o ambiguo — no se valida clínicamente
  estado                text NOT NULL DEFAULT 'agendada'
    CHECK (estado IN ('agendada', 'confirmada', 'convertida', 'cancelada', 'no_presentado')),
  creada_en             timestamptz NOT NULL DEFAULT now()
);
```

---

## 4. Expediente clínico

```sql
CREATE TABLE expediente (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  paciente_id           uuid NOT NULL UNIQUE REFERENCES paciente(id),  -- un expediente lógico por paciente (regla P0 #1)
  fecha_apertura        timestamptz NOT NULL DEFAULT now(),
  estado                text NOT NULL DEFAULT 'activo' CHECK (estado IN ('activo', 'cerrado')),
  retencion_hasta       date NOT NULL   -- calculado en aplicación: fecha_apertura + 5 años como mínimo (US-C4-01)
);

CREATE TABLE historia_clinica (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  expediente_id         uuid NOT NULL REFERENCES expediente(id),
  version               int NOT NULL DEFAULT 1,
  contenido             jsonb NOT NULL DEFAULT '{}',   -- antecedentes, aparatos y sistemas, exploración (RF-014)
  estado                text NOT NULL DEFAULT 'borrador'
    CHECK (estado IN ('borrador', 'finalizada', 'corregida_por_adenda'))
);

-- Consulta = encuentro clínico real. citaId es opcional (NULL = walk-in sin cita previa).
CREATE TABLE consulta (
  id                        uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id                 uuid NOT NULL REFERENCES tenant(id),
  expediente_id              uuid NOT NULL REFERENCES expediente(id),
  cita_id                    uuid REFERENCES cita(id),          -- NULL si es walk-in
  medico_id                  uuid NOT NULL REFERENCES medico(id),
  consultorio_id              uuid NOT NULL REFERENCES consultorio(id),
  fecha_hora                  timestamptz NOT NULL DEFAULT now(),
  motivo                      text,                              -- heredado de la cita si existe; editable mientras Abierta
  estado                      text NOT NULL DEFAULT 'abierta' CHECK (estado IN ('abierta', 'cerrada')),
  causa_cierre_sin_nota        text,   -- obligatorio si se cierra sin nota finalizada (regla de negocio, sección 5 del modelo de dominio)
  registrada_por_usuario_id    uuid NOT NULL REFERENCES usuario(id),  -- Médico o Administrativo
  creada_en                    timestamptz NOT NULL DEFAULT now()
);

-- Regla dura: no se puede cerrar sin nota finalizada o causa documentada.
CREATE FUNCTION fn_validar_cierre_consulta() RETURNS trigger AS $$
DECLARE
  nota_finalizada boolean;
BEGIN
  IF NEW.estado = 'cerrada' AND OLD.estado <> 'cerrada' THEN
    SELECT EXISTS (
      SELECT 1 FROM nota_clinica
      WHERE consulta_id = NEW.id AND estado IN ('finalizada', 'corregida_por_adenda')
    ) INTO nota_finalizada;
    IF NOT nota_finalizada AND NEW.causa_cierre_sin_nota IS NULL THEN
      RAISE EXCEPTION 'no se puede cerrar la consulta sin nota finalizada o causa documentada';
    END IF;
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validar_cierre_consulta
  BEFORE UPDATE ON consulta
  FOR EACH ROW EXECUTE FUNCTION fn_validar_cierre_consulta();
```

---

## 5. Documentación clínica

```sql
-- Contenido propio de la nota (borrador, mutable). Al finalizar, se congela un snapshot en
-- documento_version (sección 7) y este registro deja de ser editable por reglas de aplicación
-- + el trigger genérico de inmutabilidad descrito en la Propuesta de Fase 7.
CREATE TABLE nota_clinica (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  consulta_id           uuid NOT NULL REFERENCES consulta(id),
  tipo                  text NOT NULL DEFAULT 'evolucion',
  evolucion             text,                       -- DAT-034, obligatorio para finalizar
  signos_vitales        jsonb,                      -- DAT-035, condicional: {ta, fc, fr, temperatura, peso, talla}
  pronostico            text,                       -- DAT-038 — decisión de obligatoriedad aún pendiente (ver modelo de dominio, sección 3)
  tratamiento_resumen   text,                       -- narrativa corta; el detalle estructurado vive en `tratamiento`
  estado                text NOT NULL DEFAULT 'borrador'
    CHECK (estado IN ('borrador', 'finalizada', 'corregida_por_adenda')),
  autor_id              uuid NOT NULL REFERENCES usuario(id),   -- siempre un usuario tipo 'medico'
  creada_en             timestamptz NOT NULL DEFAULT now(),
  finalizada_en         timestamptz,
  version_actual        int NOT NULL DEFAULT 1
);

CREATE TABLE diagnostico (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  nota_id               uuid NOT NULL REFERENCES nota_clinica(id),
  catalogo_codigo       text,
  descripcion           text NOT NULL,
  tipo                  text   -- p. ej. 'principal' | 'secundario', a definir con codificación clínica (Fase 10)
);

CREATE TABLE tratamiento (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  nota_id               uuid NOT NULL REFERENCES nota_clinica(id),
  indicaciones          text NOT NULL,
  vigencia              text     -- no necesariamente farmacológico
);

CREATE TABLE estudio_adjunto (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  expediente_id         uuid NOT NULL REFERENCES expediente(id),
  consulta_id           uuid REFERENCES consulta(id),   -- opcional
  tipo                  text NOT NULL,
  referencia_segura     text NOT NULL,   -- ruta al almacén de objetos cifrado, nunca el archivo en la fila
  autor_id              uuid NOT NULL REFERENCES usuario(id),
  estado                text NOT NULL DEFAULT 'activo' CHECK (estado IN ('activo', 'reemplazado')),
  creado_en             timestamptz NOT NULL DEFAULT now()
);

-- Vínculo N:M entre una nota y los adjuntos que cita como "estudios relevantes" (DAT-036).
CREATE TABLE nota_estudio (
  nota_id               uuid NOT NULL REFERENCES nota_clinica(id),
  estudio_id            uuid NOT NULL REFERENCES estudio_adjunto(id),
  PRIMARY KEY (nota_id, estudio_id)
);

CREATE TABLE referencia (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  consulta_id           uuid NOT NULL REFERENCES consulta(id),
  destinatario          text NOT NULL,
  motivo                text NOT NULL,
  indicaciones          text,
  estado                text NOT NULL DEFAULT 'emitida' CHECK (estado IN ('emitida', 'atendida', 'cancelada'))
);
```

---

## 6. Receta

```sql
CREATE TABLE receta (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  consulta_id           uuid NOT NULL REFERENCES consulta(id),
  nota_id               uuid NOT NULL REFERENCES nota_clinica(id),
  paciente_id           uuid NOT NULL REFERENCES paciente(id),
  medico_id             uuid NOT NULL REFERENCES medico(id),
  estado                text NOT NULL DEFAULT 'borrador'
    CHECK (estado IN ('borrador', 'lista_para_emitir', 'emitida', 'anulada', 'sustituida', 'vencida')),
  emitida_en            timestamptz,
  version               int NOT NULL DEFAULT 1,
  hash_sha256           text,
  receta_sustituye_id   uuid REFERENCES receta(id)   -- enlaza la nueva receta con la que sustituye
);

-- Regla P0 #3: solo se puede emitir receta si la nota está finalizada.
CREATE FUNCTION fn_validar_emision_receta() RETURNS trigger AS $$
DECLARE
  nota_estado text;
BEGIN
  IF NEW.estado = 'emitida' AND (OLD.estado IS NULL OR OLD.estado <> 'emitida') THEN
    SELECT estado INTO nota_estado FROM nota_clinica WHERE id = NEW.nota_id;
    IF nota_estado NOT IN ('finalizada', 'corregida_por_adenda') THEN
      RAISE EXCEPTION 'no se puede emitir receta sin nota clínica finalizada';
    END IF;
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validar_emision_receta
  BEFORE INSERT OR UPDATE ON receta
  FOR EACH ROW EXECUTE FUNCTION fn_validar_emision_receta();

CREATE TABLE linea_receta (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  receta_id             uuid NOT NULL REFERENCES receta(id),
  generico              text NOT NULL,
  distintivo            text,
  presentacion          text NOT NULL,
  concentracion         text NOT NULL,
  dosis                 text NOT NULL,
  via                   text NOT NULL,
  frecuencia            text NOT NULL,
  duracion              text NOT NULL,
  cantidad              text NOT NULL,
  resurtimientos        int NOT NULL DEFAULT 0,
  clasificacion         text NOT NULL DEFAULT 'ordinario'
    CHECK (clasificacion NOT IN ('estupefaciente', 'psicotropico_ii_iv', 'cannabis', 'especial'))
    -- CHECK deniega por catálogo cualquier categoría controlada (RX-005); el catálogo real de
    -- clasificaciones permitidas se valida además en aplicación contra el listado vigente.
);
```

---

## 7. Integridad documental transversal

Genérica para `nota`, `historia` y `receta` — un solo mecanismo, no uno por tipo de documento (US-C3-01).

```sql
CREATE TABLE documento_version (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  documento_tipo        text NOT NULL CHECK (documento_tipo IN ('nota', 'historia', 'receta')),
  documento_id          uuid NOT NULL,
  numero_version        int NOT NULL,
  contenido_snapshot    jsonb NOT NULL,
  hash_sha256           text NOT NULL,
  autor_id              uuid NOT NULL REFERENCES usuario(id),
  finalizada_en         timestamptz NOT NULL DEFAULT now(),  -- tiempo de servidor, nunca del cliente
  estado                text NOT NULL
    CHECK (estado IN ('finalizado', 'corregido_por_adenda', 'anulado', 'sustituido')),
  UNIQUE (documento_tipo, documento_id, numero_version)
);

CREATE TABLE adenda (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  documento_tipo        text NOT NULL CHECK (documento_tipo IN ('nota', 'historia', 'receta')),
  documento_id           uuid NOT NULL,
  autor_id               uuid NOT NULL REFERENCES usuario(id),
  motivo                 text NOT NULL,
  contenido               jsonb NOT NULL,
  creada_en               timestamptz NOT NULL DEFAULT now()
  -- Sin ciclo de vida propio: es un evento append-only (ver conversación sobre adendas).
  -- No existe UPDATE/DELETE de aplicación sobre esta tabla — ver revocación de permisos abajo.
);

CREATE TABLE firma_constancia (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  documento_tipo        text NOT NULL CHECK (documento_tipo IN ('nota', 'historia', 'receta')),
  documento_id          uuid NOT NULL,
  autor_id              uuid NOT NULL REFERENCES usuario(id),
  mecanismo             text NOT NULL DEFAULT 'autografa_sobre_impresion'
    CHECK (mecanismo IN ('autografa_sobre_impresion')),  -- FEA fuera de alcance hasta validación jurídica (RX-003)
  fecha_hora            timestamptz NOT NULL DEFAULT now(),
  evidencia             text  -- referencia a constancia de impresión/generación, no el documento firmado en sí
);

-- Inmutabilidad reforzada a nivel de base de datos: el rol de aplicación no puede modificar
-- ni borrar versiones ni adendas ya escritas, incluso ante un bug en el backend.
REVOKE UPDATE, DELETE ON documento_version FROM app_role;
REVOKE UPDATE, DELETE ON adenda FROM app_role;
```

---

## 8. Auditoría

```sql
CREATE TABLE auditoria (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL,
  actor_id              uuid NOT NULL REFERENCES usuario(id),
  accion                text NOT NULL,          -- p. ej. 'auth.login', 'cita.creada', 'nota.finalizada'
  recurso_tipo          text NOT NULL,
  recurso_id            uuid,
  resultado             text NOT NULL CHECK (resultado IN ('exito', 'denegado', 'error')),
  timestamp_utc         timestamptz NOT NULL DEFAULT now(),
  correlacion_id        uuid NOT NULL,
  origen                text,                   -- IP, user-agent, sesión — nunca contraseñas/tokens
  metadata              jsonb                   -- nunca contenido clínico completo
);

REVOKE UPDATE, DELETE ON auditoria FROM app_role;

CREATE INDEX idx_auditoria_tenant_tiempo ON auditoria (tenant_id, timestamp_utc DESC);
CREATE INDEX idx_auditoria_actor ON auditoria (actor_id);
CREATE INDEX idx_auditoria_recurso ON auditoria (recurso_tipo, recurso_id);
```

---

## 9. Privacidad, retención y continuidad

```sql
CREATE TABLE solicitud_eliminacion (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  expediente_id         uuid NOT NULL REFERENCES expediente(id),
  solicitante            text NOT NULL,
  motivo                 text NOT NULL,
  fecha_solicitud         timestamptz NOT NULL DEFAULT now(),
  estado                  text NOT NULL DEFAULT 'recibida'
    CHECK (estado IN ('recibida', 'evaluada', 'denegada_por_retencion', 'resuelta')),
  resolucion              text,
  resuelta_en             timestamptz
);

CREATE TABLE aviso_privacidad (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  version               text NOT NULL,
  idioma                text NOT NULL DEFAULT 'es-MX',
  contenido             text NOT NULL,
  vigente_desde          timestamptz NOT NULL,
  vigente_hasta           timestamptz,
  UNIQUE (tenant_id, version, idioma)
);

CREATE TABLE aceptacion_aviso (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  aviso_privacidad_id   uuid NOT NULL REFERENCES aviso_privacidad(id),
  sujeto_id             uuid NOT NULL,          -- paciente_id o representante_id
  sujeto_tipo           text NOT NULL CHECK (sujeto_tipo IN ('paciente', 'representante')),
  fecha_hora            timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE solicitud_arco (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  titular_id            uuid NOT NULL REFERENCES paciente(id),
  representante_id      uuid REFERENCES representante(id),
  derecho               text NOT NULL CHECK (derecho IN ('acceso', 'rectificacion', 'cancelacion', 'oposicion')),
  evidencia_identidad    text NOT NULL,
  fecha_recepcion         timestamptz NOT NULL DEFAULT now(),
  plazo_limite            date NOT NULL,
  estado                  text NOT NULL DEFAULT 'recibida'
    CHECK (estado IN ('recibida', 'en_proceso', 'resuelta', 'denegada')),
  resolucion               text,
  resuelta_en               timestamptz
);

CREATE TABLE incidente_seguridad (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id             uuid NOT NULL REFERENCES tenant(id),
  detectado_en          timestamptz NOT NULL DEFAULT now(),
  responsable_id         uuid NOT NULL REFERENCES usuario(id),
  descripcion             text NOT NULL,
  estado                  text NOT NULL DEFAULT 'detectado'
    CHECK (estado IN ('detectado', 'contenido', 'investigado', 'evaluando_notificacion', 'cerrado')),
  linea_tiempo             jsonb NOT NULL DEFAULT '[]',
  acceso_extraordinario_otorgado boolean NOT NULL DEFAULT false,
  acceso_expira_en         timestamptz
);

-- Evidencia de ejecución de respaldos (US-C4-02) — no el respaldo en sí, que vive fuera de la BD primaria.
CREATE TABLE respaldo_ejecucion (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  iniciado_en           timestamptz NOT NULL DEFAULT now(),
  finalizado_en          timestamptz,
  resultado               text CHECK (resultado IN ('exito', 'fallo')),
  tamano_bytes             bigint,
  checksum                 text,
  ubicacion_externa        text  -- referencia, no la ruta completa con credenciales
);
```

---

## 10. Índices recomendados (además de los ya señalados por `UNIQUE`)

```sql
CREATE INDEX idx_adscripcion_usuario_activa ON adscripcion (usuario_id, consultorio_id) WHERE estado = 'activa';
CREATE INDEX idx_cita_consultorio_fecha ON cita (consultorio_id, fecha_hora_programada);
CREATE INDEX idx_cita_estado ON cita (tenant_id, estado);
CREATE INDEX idx_consulta_expediente ON consulta (expediente_id);
CREATE INDEX idx_consulta_consultorio_fecha ON consulta (consultorio_id, fecha_hora);
CREATE INDEX idx_nota_clinica_consulta ON nota_clinica (consulta_id);
CREATE INDEX idx_documento_version_lookup ON documento_version (documento_tipo, documento_id);
CREATE INDEX idx_receta_consulta ON receta (consulta_id);
CREATE INDEX idx_paciente_tenant_nombre ON paciente (tenant_id, nombre_completo);
```

---

## 11. Row-Level Security — patrón de referencia

El diseño de Fase 7 exige "negar por defecto" para todos los roles, no solo médico (`Infraestructura_Base_de_Compliance`, sección de Autorización mínima). El patrón se repite igual en cada tabla clínica: filtrar por `tenant_id` y, cuando aplica, por adscripción activa al `consultorio_id` del recurso.

```sql
ALTER TABLE consulta ENABLE ROW LEVEL SECURITY;

CREATE POLICY consulta_por_adscripcion ON consulta
  USING (
    consultorio_id IN (
      SELECT consultorio_id FROM adscripcion
      WHERE usuario_id = current_setting('app.usuario_id')::uuid
        AND estado = 'activa'
    )
  );

-- El mismo patrón se replica en cita, nota_clinica (vía consulta_id), receta, estudio_adjunto, etc.
-- nota_clinica, diagnostico, tratamiento y receta, además, deben negar el acceso completo a
-- cualquier usuario cuyo tipo no sea 'medico' — esa segunda capa se resuelve con una política
-- adicional que valida usuario.tipo = 'medico', no solo la adscripción al consultorio.
```

---

## 12. Qué no cubre este diseño todavía

- El catálogo de medicamentos/genéricos que valida `linea_receta.generico` y `clasificacion` contra COFEPRIS (RX-002, RX-005) — se modela como tabla de referencia externa, pendiente de Fase 9.
- El detalle de `pagos` — explícitamente fuera de alcance hasta que se defina ese flujo.
- Particionamiento o estrategia de escalamiento de `auditoria` a largo plazo (crecerá más rápido que cualquier otra tabla) — suficiente para el volumen de un MVP de consultorio pequeño, pero revisar antes de crecer a múltiples tenants grandes.
- Migraciones/versionado del esquema en sí (Flyway/Prisma Migrate/etc.) — es una decisión de tooling, no de modelo.
