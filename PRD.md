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
  * No se pueden asignar ni editar pedidos cuyos estados sean diferentes de `PENDIENTE`; el backend debe rechazar cualquier intento de modificación si el estado ya avanzó, respondiendo HTTP 400.
* **RF-06:** El sistema debe mostrar al repartidor únicamente los pedidos asignados a él en la fecha actual, bloqueando estrictamente el acceso a pedidos de otros usuarios (retornando HTTP 403 en intentos no autorizados).
* **RF-07:** El sistema debe mostrar al repartidor, para cada pedido, la cabecera completa y el detalle de sus líneas de productos con precio unitario.
* **RF-08:** El sistema debe permitir al repartidor cambiar el estado de un pedido según la máquina de estados: `PENDIENTE` → `ASIGNADO` → `EN_RUTA` → (`ENTREGADO` | `NO_ENTREGADO` | `REPROGRAMADO`). *Regla de negocio:* Si el estado seleccionado es `NO_ENTREGADO` o `REPROGRAMADO`, el ingreso del motivo del suceso es de carácter obligatorio.
* **RF-09:** El sistema debe registrar de forma automática e inalterable la hora exacta y el ID del usuario responsable cada vez que ocurra un cambio de estado en el pedido.
* **RF-10:** El sistema debe mostrar al administrador y supervisor el listado paginado de los pedidos del día con su estado actual, hora de última modificación y motivo en caso de incidencias. El frontend debe verificar conectividad y refrescar los datos locales cada 5 segundos si hay conexión disponible.
* **RF-11:** El celular del repartidor debe almacenar localmente los cambios de estado si la red 4G se interrumpe momentáneamente, reintentando la sincronización en segundo plano de forma automática al recuperar la conectividad.
* **RF-12:** El sistema debe paginar todos los listados principales utilizando los parámetros de consulta `page` y `size` por defecto.
* **RF-13:** El sistema debe mostrar un indicador visual prominente en el panel del administrador y supervisor cuando no hay conexión disponible.
* **RF-14:** El frontend debe obtener la URL base de la API desde un archivo de configuración por entorno (development vs. production), sin hardcodear URLs en los componentes. *Regla de negocio:* 
  * Usar `environment.ts` (desarrollo) y `environment.prod.ts` (producción)
  * Crear `ConfigService` inyectable que expone `apiUrl`
  * Todos los componentes usan `this.config.apiUrl` en lugar de valores fijos
  * Aceptancia: Cambiar puerto o dominio sin editar componentes

---

## Requerimientos No Funcionales (RNF)
* **RNF-01:** El 95% de las peticiones HTTP debe responder en <= 2 segundos bajo condiciones normales de red.
* **RNF-02:** La interfaz del repartidor debe ser responsive, adaptada y totalmente usable desde pantallas de 320px de ancho (diseño mobile-first).
* **RNF-03:** Las contraseñas de usuario deben almacenarse de forma segura utilizando hash bcrypt con coste >= 10.
* **RNF-04:** Toda comunicación cliente-servidor debe efectuarse mediante HTTPS utilizando TLS 1.2 o superior.
* **RNF-05:** El sistema debe funcionar utilizando únicamente APIs propias; no depende de servicios externos de terceros (pasarelas de pago, autenticación externa, etc.).

---

## Criterios de Aceptación (AC)
* **AC-01 (RF-01):** Dado un usuario con credenciales válidas, cuando ingresa usuario y contraseña, entonces accede correctamente a su panel según su rol asignado.
* **AC-02 (RF-02):** Dado un administrador autenticado, cuando registra un cliente con nombre y dirección válidos, entonces el cliente se guarda exitosamente en la base de datos.
* **AC-03 (RF-03):** Dado un administrador que intenta guardar un pedido sin líneas o con cantidades en 0, cuando confirma, entonces el sistema responde HTTP 400 y rechaza la creación.
* **AC-04 (RF-04):** Dado un conjunto de líneas de detalle en un pedido, cuando el sistema procesa la creación o consulta, entonces calcula automáticamente el monto total como la suma aritmética de cantidad por precio unitario sin intervención manual.
* **AC-05 (RF-05):** Dado un pedido en estado `PENDIENTE`, cuando el administrador lo asigna a un repartidor y unidad, entonces el pedido cambia a estado `ASIGNADO` y aparece reflejado exclusivamente en la ruta de dicho repartidor.
* **AC-06 (RF-05 — Rechazo de edición):** Dado un pedido en estado `ASIGNADO`, `EN_RUTA`, `ENTREGADO`, `NO_ENTREGADO` o `REPROGRAMADO`, cuando el administrador intenta editarlo o reasignarlo, entonces el sistema responde HTTP 400 rechazando la operación.
* **AC-07 (RF-06 — Control de acceso):** Dado un repartidor autenticado que intenta consultar o modificar mediante URL/API el pedido asignado a otro compañero, entonces el sistema deniega el acceso respondiendo con HTTP 403.
* **AC-08 (RF-07):** Dado un repartidor autenticado con pedidos asignados en un dispositivo con pantalla de 320px, cuando abre el detalle de un pedido, entonces visualiza sin scroll horizontal la cabecera, todas las líneas de productos y sus valores unitarios en orden legible.
* **AC-09 (RF-08 — Cambio de estado):** Dado un pedido asignado, cuando el repartidor lo marca como `EN_RUTA`, entonces el estado cambia, el sistema registra automáticamente la hora exacta y el ID del usuario, y el administrador lo visualiza actualizado en su listado dentro de 5 segundos.
* **AC-10 (RF-08 — Cambio a ENTREGADO):** Dado un pedido en estado `EN_RUTA`, cuando el repartidor lo marca como `ENTREGADO`, entonces el estado cambia, se registran hora e ID automáticamente, y el pedido aparece con estado `ENTREGADO` en el listado del administrador dentro de 5 segundos.
* **AC-11 (RF-08 — Motivo obligatorio):** Dado un pedido asignado, cuando el repartidor intenta marcarlo como `NO_ENTREGADO` o `REPROGRAMADO` sin especificar un motivo, entonces el sistema bloquea la acción y exige el texto del motivo.
* **AC-12 (RF-10):** Dado el panel del administrador con conectividad 4G estable, cuando consulta el listado del día, entonces visualiza los pedidos con estado, hora de última actualización y motivos de incidencias; el listado se refresca automáticamente cada 5 segundos.
* **AC-13 (RF-10 — Sin conexión):** Dado el panel del administrador sin conectividad, cuando intenta consultar el listado, entonces visualiza la última versión guardada localmente.
* **AC-14 (RF-08 — Transiciones inválidas):** Dado un pedido en estado `PENDIENTE`, cuando el repartidor intenta cambiar directamente a `ENTREGADO` (saltando ASIGNADO y EN_RUTA), entonces el sistema rechaza la acción respondiendo HTTP 400.
* **AC-15 (RF-08 — Estados finales irreversibles):** Dado un pedido en estado `ENTREGADO`, cuando el repartidor intenta cambiar a cualquier otro estado, entonces el sistema rechaza la acción respondiendo HTTP 400.
* **AC-16 (RF-11):** Dado un repartidor que actualiza el estado de un pedido sin conexión 4G, cuando el celular recupera la señal, entonces el sistema sincroniza automáticamente el cambio pendiente en segundo plano y lo refleja en el panel del administrador.
* **AC-17 (RF-12):** Dados más de 20 pedidos registrados en el sistema, cuando se realiza la petición de listado enviando los parámetros `page` y `size`, entonces el backend devuelve estrictamente un subconjunto paginado respetando los límites solicitados.
* **AC-18 (RF-13 — Indicador de desconexión):** Dado el panel del administrador o supervisor sin conectividad, cuando pierde la conexión, entonces muestra un indicador visual prominente (color rojo, ícono de desconexión o banner) en la pantalla.
* **AC-19 (RF-13 — Recuperación de conexión):** Dado un panel sin indicador de desconexión activo, cuando el navegador recupera la conexión, entonces desaparece el indicador en menos de 2 segundos.
* **AC-20 (RF-14 — Configuración por entorno):** Dado el código fuente del frontend con `environment.ts` (localhost:5210) y `environment.prod.ts` (producción), cuando se construye con `ng build --configuration production`, entonces el bundle utiliza la URL de producción sin requerimiento de modificación de componentes.
* **AC-21 (RF-14 — ConfigService):** Dado un componente que inyecta `ConfigService`, cuando realiza una petición HTTP a `${this.config.apiUrl}/api/pedido`, entonces la URL se resuelve correctamente según el entorno sin valores hardcodeados.

---

## Fuera de Alcance
Reportes PDF/Excel · cierre de día automatizado · registro público de usuarios · GPS o ubicación en tiempo real por mapa · auditoría avanzada de bases de datos · arquitectura multiempresa/multi-sucursal · aplicación móvil nativa (se emplea PWA/Web móvil) · notificaciones push masivas · chat interno · integraciones con ERP, WhatsApp o pasarelas de terceros.

---

## Riesgos y Dependencias
* **Riesgo:** Intermitencia o pérdida de señal 4G mientras el repartidor se desplaza en moto.
  * *Mitigación:* Mecanismo de cola local en el almacenamiento del navegador móvil que reintenta automáticamente el envío del cambio de estado al recuperar la conexión (implementado en RF-11 y verificado en AC-14).
* **Riesgo:** Que un administrador intente editar un pedido ya en curso (`ASIGNADO`, `EN_RUTA`, etc.), alterando la trazabilidad operativa.
  * *Mitigación:* Validación estricta en el backend que rechaza cualquier edición si el estado actual es diferente de `PENDIENTE`, respondiendo HTTP 400 (regla incorporada en RF-05 y verificada en AC-06).
* **Dependencia:** Estabilidad básica de la red móvil celular (4G/3G) para las sincronizaciones periódicas en ruta.
* **Dependencia:** Disponibilidad del servidor backend durante el horario operativo de reparto (06:00 a 20:00 horas).
