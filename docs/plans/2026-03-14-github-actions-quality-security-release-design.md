# Diseño: Calidad, Seguridad y Release en GitHub Actions

Fecha: 2026-03-14  
Repositorio: `FamilyFinances`

## 1. Objetivo

Separar responsabilidades de CI/CD para mejorar mantenibilidad y seguridad, reducir consumo de espacio en GitHub y mantener una experiencia clara en PRs.

Objetivos concretos:
- Añadir controles de calidad y seguridad preventivos.
- Mantener `main` con checks obligatorios y `develop` con checks informativos.
- Restringir la generación de ZIP Windows a tags/release.
- Limpiar assets ZIP antes de publicar y conservar solo 2 antiguos (quedarán 3 tras publicar uno nuevo).
- Publicar cobertura en GitHub sin depender de servicios externos en esta fase.

## 2. Estado actual (resumen)

Existe un único workflow (`.github/workflows/ci.yml`) que mezcla:
- Build y test general.
- Reporting gate.
- Empaquetado Windows + smoke test.
- Publicación de release.
- Limpieza de ZIPs de releases antiguas.

Carencias identificadas:
- No hay workflow dedicado de CodeQL.
- No hay workflow de Dependency Review.
- No hay publicación clara de cobertura para PR.
- El empaquetado ocurre más de lo necesario para el caso de uso actual.

## 3. Decisiones de diseño

1. Separar en 4 workflows independientes.
2. Hacer obligatorios en `main` los checks de calidad/seguridad.
3. Mantener los mismos checks en `develop`, pero sin bloqueo de merge.
4. Ejecutar release Windows solo en tags `v*.*.*`.
5. Limpiar ZIPs antiguos antes de publicar release y conservar 2 previos.

## 4. Arquitectura de workflows

```text
PR/push(main,develop) -> ci-quality.yml
PR(main,develop)      -> dependency-review.yml
PR + push + schedule  -> codeql.yml
push tags v*.*.*      -> release-windows.yml
```

### 4.1 `ci-quality.yml`

Trigger:
- `pull_request` hacia `main` y `develop`.
- `push` a `main` y `develop`.

Flujo:
- `dotnet restore`
- `dotnet build -c Release --no-restore`
- `dotnet test -c Release --no-build --collect:"XPlat Code Coverage" --logger "trx;LogFileName=test-results.trx"`
- Publicación de artifacts:
  - `TestResults/**/*.trx`
  - `TestResults/**/coverage.cobertura.xml`
- Publicación de resumen en job summary (resultado de tests y cobertura disponible).

Notas:
- En esta fase no se exige umbral global de cobertura para no introducir fricción inicial.
- El check será obligatorio solo en `main` vía branch protection.

### 4.2 `dependency-review.yml`

Trigger:
- `pull_request` hacia `main` y `develop`.

Flujo:
- `actions/dependency-review-action`.

Política:
- Bloqueante en `main`.
- Informativo en `develop`.

### 4.3 `codeql.yml`

Trigger:
- `pull_request` hacia `main` y `develop`.
- `push` a `main` y `develop`.
- `schedule` semanal.

Flujo:
- `github/codeql-action/init` (lenguaje `csharp`).
- Autobuild o build explícito según necesidad.
- `github/codeql-action/analyze`.

Política:
- Bloqueante en `main`.
- Informativo en `develop`.

### 4.4 `release-windows.yml`

Trigger:
- `push` de tags `v*.*.*`.

Flujo:
1. Build/test mínimos necesarios para release (si aplica en el workflow).
2. **Pre-cleanup** de assets ZIP antiguos.
3. Build distribución Windows.
4. Verificación de contenidos.
5. Smoke test ZIP.
6. Publicación en GitHub Release.

Regla de limpieza:
- Patrón de asset: `FamilyFinances-v*-win-x64.zip`.
- Conservar **2 ZIP antiguos**.
- Publicar el nuevo ZIP después de limpiar.
- Resultado esperado: 2 antiguos + 1 nuevo = 3 assets ZIP máximos de releases recientes.

## 5. Reglas de branch protection

### `main` (obligatorio)
- `ci-quality`
- `dependency-review`
- `codeql`

### `develop` (informativo)
- Los workflows corren, pero no se marcan como required checks.

Implicación:
- Los PR siempre se pueden abrir.
- Solo el merge a `main` queda bloqueado si falla algún required check.

## 6. Estrategia de reducción de espacio

Medidas aprobadas:
- Quitar empaquetado Windows de pushes normales (`main`/`develop`).
- Generar ZIP únicamente en tags de release.
- Limpiar ZIPs de releases antiguas **antes** de publicar.
- Reducir retención de ZIPs históricos en releases de 3 a 2 antiguos.
- Mantener `retention-days` bajo en artifacts temporales de workflow.

Resultado esperado:
- Menor consumo de almacenamiento y minutos de Actions.
- Menor probabilidad de fallos en publish por falta de espacio.

## 7. Cobertura en GitHub y evolución a UI “bonita”

Fase actual (sin servicio externo):
- Cobertura visible en:
  - Artifacts del workflow.
  - Logs y Job Summary del check de CI.

Fase siguiente opcional (UI más rica):
- Integrar Codecov o Coveralls para:
  - vista histórica,
  - diff por PR,
  - comentarios automáticos en PR con cambios de cobertura.

Esta integración se deja explícitamente fuera de esta fase, pero preparada conceptualmente.

## 8. Riesgos y mitigaciones

Riesgo:
- Checks nuevos aumentan tiempo de CI.
Mitigación:
- Separación por workflows, caché de dependencias y scope claro por trigger.

Riesgo:
- Configurar required checks con nombre inestable puede bloquear merges.
Mitigación:
- Fijar nombres estables de jobs antes de activar branch protection.

Riesgo:
- Limpieza de assets borra archivos no deseados.
Mitigación:
- Filtrar por regex exacta del ZIP de distribución.

## 9. Plan de implementación (alto nivel)

1. Crear workflows: `ci-quality.yml`, `dependency-review.yml`, `codeql.yml`, `release-windows.yml`.
2. Ajustar `ci.yml` actual (retirarlo o reducirlo para evitar duplicidad).
3. Activar branch protection en GitHub:
   - `main` required checks.
   - `develop` sin required checks.
4. Validar con PR de prueba:
   - ejecución de checks,
   - artifacts de cobertura,
   - comportamiento de bloqueo en `main`.
5. Validar release de prueba con tag:
   - pre-cleanup correcto,
   - publish correcto,
   - conservación de 2 ZIP antiguos.

## 10. Alcance y no alcance

En alcance:
- Calidad CI, seguridad en PR/código y release Windows optimizado para espacio.

Fuera de alcance:
- Rediseño funcional de la app.
- Umbrales de cobertura estrictos desde el primer día.
- Integración inmediata con plataforma externa de cobertura (Codecov/Coveralls).

