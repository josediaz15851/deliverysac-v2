# AGENTS.md

## Propósito
DeliverySac: sistema web de reparto y trazabilidad de pedidos. El administrador asigna pedidos a repartidores y unidades, los repartidores marcan estados desde el celular (con soporte offline/reintento) y el administrador/supervisor consultan el estado y la hora de cada cambio. Ver `SESSION02/PRD.md`.

## Stack
- Backend: .NET 8 (LTS) + EF Core + SQL Server Express/LocalDB
- Frontend: Angular 17/18/19 (última), npm, PWA/mobile-first (>= 320px) con cola offline (RF-11)
- Tests: xUnit (`dotnet test`)

## Cómo correr
- Instalar: `dotnet restore` en la raíz/API y `npm install` en el frontend
- Levantar backend: `dotnet run` en la carpeta de la API
- Levantar frontend: `ng serve` en el frontend
- Tests: `dotnet test`

## Qué NO hacer
- No editar ni reasignar pedidos cuyo estado no sea `PENDIENTE`; rechazar con HTTP 400 (RF-05).
- No alterar la hora ni el ID de usuario registrados en los cambios de estado: son automáticos e inalterables (RF-09).
- No depender de pasarelas o servicios pagos de terceros (RNF-05).