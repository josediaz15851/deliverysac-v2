# PRD-001: DeliverySac - Reparto y Trazabilidad de Pedidos.

## Contexto y Problema
En la empresa DeliverySac repartimos con 5 unidades (3 motos lineales y 2 furgonetas). Hoy el administrador asigna clientes y pedidos manualmente, sin visibilidad de dónde están las unidades, qué pedidos se entregaron ni a qué hora. Al final del día solo hay control cuando retornan las unidades: demoras, pérdida de trazabilidad y cero métricas. No tenemos presupuesto ni tiempo para un ERP de reparto; necesitamos algo simple que se haga cargo del control operativo diario.

**Personas:**
* **Administrador de reparto:** Asigna pedidos cada mañana y quiere ver el estado y la hora exacta de actualización sin llamar a nadie.
* **Repartidor:** Necesita ver su ruta en el celular, marcar entregas rápido y sincronizar estados incluso ante intermitencias de señal en ruta.
* **Supervisor:** Consulta el reporte del día y los motivos de no entrega para tomar decisiones operativas.

---

## Objetivos
Que al final del día el 100% de los pedidos asignados tengan un estado final (entregado, no entregado o reprogramado) registrado en el sistema con su respectiva hora y usuario, sin llamadas telefónicas ni dudas.

*Flujo operativo:* Creación de pedido -> Asignación -> Repartidor marca estado (con soporte offline/reintento) -> Administrador visualiza el estado y la hora actualizados.

---

## Requerimientos Funcionales (RF)
* **RF-01:** El sistema debe permitir iniciar sesión con usuario y contraseña según rol (Administrador, Repartidor, Supervisor).
* **RF-02:** El sistema debe permitir al administrador registrar un cliente con nombre y dirección obligatorios.
* **RF-03:** El sistema debe permitir al administrador crear un pedido con cabecera (cliente, fecha de reparto, comentario) y una o más líneas de detalle. *Regla de negocio:* Cada línea requiere producto, cantidad > 0, precio unitario >= 0 y descripción obligatoria. Si no cumple, el sistema rechaza la solicitud respondiendo HTTP 400.
* **RF-04:** El sistema debe calcular automáticamente el monto total del pedido como la suma de cantidad por precio unitario de sus líneas.
* **RF-05:** El sistema debe permitir al administrador asignar un pedido en estado `PENDIENTE` a un repartidor y una unidad en una fecha. *Reglas de negocio:*
  * Un pedido solo puede pertenecer a una asignación activa (si se reasigna, se invalida la anterior automáticamente).
  * La edición de pedidos no es una funcionalidad del sistema: cualquier intento del backend de modificar o reasignar un pedido cuyo estado sea diferente de `PENDIENTE` debe rechazarse respondiendo HTTP 400.
* **RF-06:** El sistema debe mostrar al repartidor únicamente los pedidos asignados a él en la fecha actual, bloqueando estrictamente el acceso a pedidos de otros usuarios (retornando HTTP 403 en intentos no autorizados).
* **RF-07:** El sistema debe mostrar al repartidor, para cada pedido, la cabecera completa y el detalle de sus líneas de productos con precio unitario.
* **RF-08:** El sistema debe permitir al repartidor cambiar el estado de un pedido según la máquina de estados: `PENDIENTE` → `ASIGNADO` → `EN_RUTA` → (`ENTREGADO` | `NO_ENTREGADO` | `REPROGRAMADO`). *Regla de negocio:* Si el estado seleccionado es `NO_ENTREGADO` o `REPROGRAMADO`, el ingreso del motivo del suceso es de carácter obligatorio.
* **RF-09:** El sistema debe registrar de forma automática e inalterable la hora exacta y el ID del usuario responsable cada vez que ocurra un cambio de estado en el pedido.
* **RF-10:** El sistema debe mostrar al administrador y supervisor el listado paginado de los pedidos del día con su estado actual, hora de última modificación y motivo en caso de incidencias.
* **RF-10a:** El sistema debe incluir en el listado de la fecha actual los pedidos con estado `REPROGRAMADO` de días anteriores.
* **RF-10b:** El frontend debe verificar conectividad y refrescar los datos locales cada 5 segundos si hay conexión disponible.
* **RF-11:** El celular del repartidor debe almacenar localmente los cambios de estado si la red 4G se interrumpe momentáneamente, reintentando la sincronización en segundo plano de forma automática al recuperar la conectividad.
* **RF-12:** El sistema debe paginar todos los listados principales utilizando los parámetros de consulta `page` y `size` por defecto.
* **RF-13:** El sistema debe mostrar un indicador visual prominente en el panel del administrador y supervisor cuando no hay conexión disponible.
* **RF-14:** El frontend debe obtener la URL base de la API desde una configuración por entorno (desarrollo vs. producción), sin hardcodear URLs en los componentes. *Aceptación:* Cambiar el puerto o dominio de la API sin editar ningún componente.

---

## Requerimientos No Funcionales (RNF)
* **RNF-01:** El 95% de las peticiones HTTP debe responder en <= 2 segundos bajo condiciones normales de red.
* **RNF-02:** La interfaz del repartidor debe ser responsive y no debe requerir scroll horizontal en pantallas de 320px de ancho (diseño mobile-first), verificado por AC-08.
* **RNF-03:** Las contraseñas de usuario deben almacenarse de forma segura utilizando hash bcrypt con coste >= 10.
* **RNF-04:** Toda comunicación cliente-servidor debe efectuarse mediante HTTPS utilizando TLS 1.2 o superior.
* **RNF-05:** El sistema no debe realizar llamadas a servicios externos de terceros (0 dependencias externas de pago o de terceros: pasarelas, autenticación externa, etc.). Verificable: la lista de dominios externos en la configuración debe estar vacía y todas las peticiones del cliente deben apuntar únicamente a APIs propias.

---

## Criterios de Aceptación (AC)
* **AC-01 (RF-01):** Dado un usuario autenticado con credenciales válidas, cuando inicia sesión, entonces el sistema lo redirige según su rol: ADMINISTRADOR ve el listado de pedidos del día, REPARTIDOR ve su ruta de la fecha actual, SUPERVISOR ve el listado de pedidos del día sin botones de edición ni asignación.
* **AC-02 (RF-02):** Dado un administrador autenticado, cuando registra un cliente con nombre y dirección válidos, entonces el cliente aparece en el listado de clientes del administrador y es recuperable por su ID mediante la API.
* **AC-03 (RF-03):** Dado un administrador que intenta guardar un pedido sin líneas o con cantidades en 0, cuando confirma, entonces el sistema responde HTTP 400 y rechaza la creación.
* **AC-04 (RF-04):** Dado un pedido con líneas creadas, cuando se consulta ese pedido por API, entonces el campo `total` devuelto es exactamente igual a la sumatoria de `cantidad * precioUnitario` de todas sus líneas (verificable con 2 líneas de ejemplo: 2x2.50 + 1x7.00 => total 12.00).
* **AC-05 (RF-05):** Dado un pedido en estado `PENDIENTE`, cuando el administrador lo asigna a un repartidor y unidad, entonces el pedido cambia a estado `ASIGNADO` y aparece reflejado exclusivamente en la ruta de dicho repartidor.
* **AC-06 (RF-05 — Rechazo de edición):** Dado un pedido en estado `ASIGNADO`, `EN_RUTA`, `ENTREGADO`, `NO_ENTREGADO` o `REPROGRAMADO`, cuando el administrador intenta editarlo o reasignarlo, entonces el sistema responde HTTP 400 rechazando la operación.
* **AC-07 (RF-06 — Control de acceso):** Dado un repartidor autenticado que intenta consultar o modificar mediante URL/API el pedido asignado a otro compañero, entonces el sistema deniega el acceso respondiendo con HTTP 403.
* **AC-07a (RF-06 — Aislamiento por rol en API):** Dado un repartidor autenticado, cuando llama directamente por API al endpoint de listado de pedidos del administrador, entonces el sistema responde HTTP 403 y no devuelve datos.
* **AC-08 (RF-07):** Dado un repartidor autenticado con pedidos asignados en un dispositivo con pantalla de 320px, cuando abre el detalle de un pedido, entonces visualiza la cabecera, todas las líneas de productos y sus valores unitarios sin scroll horizontal.
* **AC-09 (RF-08 — Cambio de estado):** Dado un pedido asignado, cuando el repartidor lo marca como `EN_RUTA`, entonces el estado cambia, el sistema registra automáticamente la hora exacta y el ID del usuario, y el administrador lo visualiza actualizado en su listado dentro de 5 segundos.
* **AC-09a (RF-09 — Trazabilidad inalterable):** Dado un pedido con cambios de estado registrados, cuando cualquier rol intenta actualizar la hora o el usuario responsable de un cambio existente mediante API, entonces el sistema responde HTTP 405 (método no permitido), ya que no existe endpoint de edición de la trazabilidad.
* **AC-10 (RF-08 — Cambio a ENTREGADO):** Dado un pedido en estado `EN_RUTA`, cuando el repartidor lo marca como `ENTREGADO`, entonces el estado cambia, se registran hora e ID automáticamente, y el pedido aparece con estado `ENTREGADO` en el listado del administrador dentro de 5 segundos.
* **AC-11 (RF-08 — Motivo obligatorio):** Dado un pedido asignado, cuando el repartidor intenta marcarlo como `NO_ENTREGADO` o `REPROGRAMADO` sin especificar un motivo, entonces el sistema bloquea la acción y exige el texto del motivo.
* **AC-12 (RF-10, RF-10a):** Dado el panel del administrador con conectividad 4G estable y al menos un pedido `REPROGRAMADO` de un día anterior, cuando consulta el listado del día, entonces visualiza los pedidos con estado, hora de última actualización y motivos de incidencias, y el pedido `REPROGRAMADO` de días anteriores figura en el listado; el listado se refresca automáticamente cada 5 segundos.
* **AC-13 (RF-10 — Sin conexión):** Dado el panel del administrador sin conectividad, cuando intenta consultar el listado, entonces visualiza la última versión guardada localmente.
* **AC-14 (RF-08 — Transiciones inválidas):** Dado un pedido en estado `PENDIENTE`, cuando el repartidor intenta cambiar directamente a `ENTREGADO` (saltando ASIGNADO y EN_RUTA), entonces el sistema rechaza la acción respondiendo HTTP 400.
* **AC-15 (RF-08 — Estados finales irreversibles):** Dado un pedido en estado `ENTREGADO`, cuando el repartidor intenta cambiar a cualquier otro estado, entonces el sistema rechaza la acción respondiendo HTTP 400.
* **AC-16 (RF-11):** Dado un repartidor que actualiza el estado de un pedido sin conexión 4G, cuando el celular recupera la señal, entonces el sistema sincroniza automáticamente el cambio pendiente en segundo plano sin intervención del usuario en un plazo máximo de 10 segundos, y el cambio queda reflejado en el panel del administrador.
* **AC-17 (RF-12):** Dados más de 20 pedidos registrados en el sistema, cuando se realiza la petición de listado enviando los parámetros `page` y `size`, entonces el backend devuelve estrictamente un subconjunto paginado respetando los límites solicitados.
* **AC-18 (RF-13 — Indicador de desconexión):** Dado el panel del administrador o supervisor sin conectividad, cuando pierde la conexión, entonces se muestra un banner rojo fijo con el texto "SIN CONEXIÓN" en la pantalla.
* **AC-19 (RF-13 — Recuperación de conexión):** Dado un panel con el banner "SIN CONEXIÓN" visible, cuando el navegador recupera la conexión, entonces el banner desaparece en menos de 2 segundos.
* **AC-20 (RF-14 — Configuración por entorno):** Dado el código fuente del frontend con configuración de desarrollo (localhost:5210) y de producción, cuando se construye con `ng build --configuration production`, entonces el bundle utiliza la URL de producción sin requerir modificación de componentes.
* **AC-21 (RF-14 — URL desde configuración):** Dado un componente del frontend que consume la configuración por entorno, cuando realiza una petición HTTP a la ruta `/api/pedido`, entonces en el panel de red del navegador la petición se resuelve contra la URL base definida en la configuración del entorno activo (desarrollo o producción), sin URLs hardcodeadas en el código del componente.

---

## Fuera de Alcance
Reportes PDF/Excel · cierre de día automatizado · registro público de usuarios · GPS o ubicación en tiempo real por mapa · auditoría avanzada de bases de datos · arquitectura multiempresa/multi-sucursal · aplicación móvil nativa (se emplea PWA/Web móvil) · notificaciones push masivas · chat interno · integraciones con ERP, WhatsApp o pasarelas de terceros · edición de pedidos ya creados (solo se permite crear, asignar y cambiar estado).

---

## Riesgos y Dependencias
* **Riesgo:** Intermitencia o pérdida de señal 4G mientras el repartidor se desplaza en moto.
  * *Mitigación:* Mecanismo de cola local en el almacenamiento del navegador móvil que reintenta automáticamente el envío del cambio de estado al recuperar la conexión (implementado en RF-11 y verificado en AC-16).
* **Riesgo:** Que un administrador intente editar un pedido ya en curso (`ASIGNADO`, `EN_RUTA`, etc.), alterando la trazabilidad operativa.
  * *Mitigación:* Validación estricta en el backend que rechaza cualquier edición si el estado actual es diferente de `PENDIENTE`, respondiendo HTTP 400 (regla incorporada en RF-05 y verificada en AC-06).
* **Dependencia:** Estabilidad básica de la red móvil celular (4G/3G) para las sincronizaciones periódicas en ruta.
* **Dependencia:** Disponibilidad del servidor backend durante el horario operativo de reparto (06:00 a 20:00 horas).

---

## Nota de Decisiones Técnicas (fuera del alcance funcional del PRD)
Derivadas de RF-14, para el equipo de desarrollo (no son requerimientos):
* `environment.ts` (desarrollo) y `environment.prod.ts` (producción) como fuentes de la URL base.
* `ConfigService` inyectable que expone `apiUrl`; todos los componentes consumen `this.config.apiUrl` en lugar de valores fijos.
