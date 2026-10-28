# Definition of Done

Aplica a toda historia de las fases 7 en adelante. Una fase se cierra solo cuando todas sus historias P0 cumplen esto **y** se cumplen los criterios de salida de su documento de fase.

## Historia "Hecha"

### Reglas y diseño
- [ ] Todas las RN que toca la historia existen en el catálogo, con su estado `Vigente` (no `Pendiente decisión`).
- [ ] Si la historia cambia estados, `02-ciclos-de-vida.md` está actualizado.
- [ ] Si cambia quién puede hacer algo, `03-roles-y-permisos.md` está actualizado.

### Implementación
- [ ] Las reglas **A** y **R** se aplican en la API, y en la BD cuando hay mecanismo (constraint, trigger, RLS, REVOKE).
- [ ] Los timestamps clínicos y de auditoría vienen del servidor (RN-GEN-002).
- [ ] Ningún log ni evento de auditoría contiene secretos ni contenido clínico completo (RN-AUD-003).
- [ ] Las acciones relevantes emiten su evento de auditoría.

### Pruebas
- [ ] Prueba positiva por cada criterio de aceptación.
- [ ] Prueba negativa por cada regla A/R (otro rol, otro consultorio, estado inválido).
- [ ] El nombre de cada prueba incluye el ID de la RN.
- [ ] Las pruebas corren en CI y pasan.

### UI (si aplica)
- [ ] Coincide con la pantalla de referencia (P1–P10), o la diferencia está registrada en el registro de decisiones.
- [ ] Contraste WCAG 2.2 AA y funciona en 1024 px o más.
- [ ] Para el rol Administrativo, no aparece ningún elemento que sugiera acceso clínico.
- [ ] Solo datos simulados; nada real (RN-GEN-007).

### Documentación
- [ ] La fila en `trazabilidad.md` tiene la prueba y la evidencia.
- [ ] El estado de la historia está actualizado en su documento de fase.

## Fase "Cerrada"

- [ ] Todas las historias P0 cumplen lo anterior.
- [ ] Los criterios de salida del documento de fase están marcados.
- [ ] No quedan DEC bloqueantes abiertas para la fase siguiente, o están identificadas en el roadmap.
- [ ] Hay una entrada en `bitacora-fases.md`.
- [ ] `00-roadmap.md` refleja el nuevo estado.
