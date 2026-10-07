---
name: task-commit
description: Verifica build de frontend y backend antes de commitear. Se usa después de terminar una subtarea compilable.
---

# Task Commit

Cuando termines una subtarea:

1. **Verifica que compila (backend)**
   ```
   cd API
   dotnet build
   dotnet test
   ```
   Si falla, arreglá primero. NO continúes.

2. **Verifica que compila (frontend)**
   ```
   cd web
   npm run build
   ```
   Si falla, arreglá primero.

3. **Stagea solo archivos de la subtarea** (NUNCA bin/, obj/, dist/, node_modules/)
   ```
   git add <archivos específicos>
   ```
   **CRÍTICO:** 
   - NO `git add .` — stagea solo lo que tocaste
   - NUNCA subas: `bin/`, `obj/`, `dist/`, `node_modules/`
   - Si accidentalmente stagiaste, usa: `git restore --staged <archivo>`
   - Si ya están en git, usa: `git rm -r --cached <carpeta>`

4. **Commitea usando skill conventional-commit**
   
   El mensaje debe ser: `tipo(scope): descripción`
   - Ejemplo: `feat(api): crear modelo usuario + dbcontext`
   - Ejemplo: `feat(web): crear authservice`
   - Usa el skill `conventional-commit` para redactarlo

Una subtarea = un commit. Compilable, testeable, limpio.

