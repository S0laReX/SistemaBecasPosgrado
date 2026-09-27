# Becas de Posgrado: cinco transacciones PL/SQL

Documentación detallada para la entrega: [Memoria técnica, diseño objeto-relacional y explicación de sentencias](docs/Documentacion_Tarea_PL_SQL.md).

Aplicación ASP.NET Core MVC (.NET 10) + Oracle. El panel `/Transacciones` llama a cinco procedimientos de `PKG_BECAS` sobre la base del proyecto anterior.

## Decisión técnica

Conviene aprovechar la web existente. SQL Developer es el cliente desde el que se programa y prueba; Oracle Database ejecuta PL/SQL. No necesitas otra aplicación Oracle. La interfaz recoge datos y ODP.NET invoca procedimientos almacenados. Crear una aplicación alternativa requeriría reconstruir pantallas sin que el enunciado lo exija.

Los procedimientos no confirman internamente: el llamador controla la transacción. En C# se usa BeginTransaction/Commit/Rollback; en SQL Developer, bloques BEGIN con COMMIT y EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE. Una consulta de conteo no sustituye una operación de escritura. Los triggers son opcionales según el enunciado.

## Entregables

| Transacción | Procedimiento | Tarea |
|---|---|---|
| T1 | registrar_candidato | Inserta un objeto T_POSTULANTE e inicializa sus experiencias. |
| T2 | agregar_experiencia | Inserta T_EXPERIENCIA en la colección anidada. |
| T3 | postular | Crea T_SOLICITUD con REF, secuencia y validación de plazo, duplicados y máximo tres. |
| T4 | resolver | Cambia una solicitud pendiente a Aceptada o Rechazada. |
| T5 | cerrar_oferta | Cambia la oferta de Vigente a Cerrada. |

- `Database/01_transacciones.sql`: instalación incremental del paquete y secuencia.
- `Database/02_pruebas.sql`: cinco operaciones, seis errores esperados y reversión de datos temporales.
- `Database/03_cinco_transacciones.sql`: cinco bloques independientes ejecutables, con COMMIT/ROLLBACK, para entregar y exponer.
- Interfaz `/Transacciones`, documentación y evidencias reales de colaboración en GitHub.

## Segunda etapa: tres operaciones en equipo

La vista `/NuevasTransacciones` presenta tres casos de uso. **Las operaciones 1 y 3 ya están implementadas** con procedimientos independientes: `sp_actualizar_correo_postulante` y `sp_transferir_postulacion`. La operación 2 (reprogramar convocatoria) queda para el compañero. La sintaxis y cada construcción PL/SQL están explicadas en [Transacción 1: actualizar correo](docs/Transaccion_1_Actualizar_Correo.md) y [Transacción 3: transferir postulación](docs/Transaccion_3_Transferir_Postulacion.md).

Ejecutar con F5 `Database/04_actualizar_correo_postulante.sql` y `Database/06_transferir_postulacion.sql` en el esquema propietario de las tablas. Después ejecutar las pruebas reversibles `Database/05_prueba_actualizar_correo.sql` y `Database/07_prueba_transferir_postulacion.sql`. En la web, usar **Modo Administrador → Proyecto en equipo**. Los procedimientos no hacen `COMMIT`; la web confirma mediante transacciones de ODP.NET. GitHub no instala los objetos en el Oracle del compañero: él debe ejecutar los scripts 04 y 06 en su entorno.

## SQL Developer: ejecutar

1. Conectarse al esquema propietario de las tablas del proyecto. En el equipo original es SYSTEM en XE. Se requieren POSTULANTES, OFERTAS, SOLICITUDES y sus tipos originales; el panel también consulta referencias a programas.
2. Detener las escrituras durante la primera instalación de la secuencia. Abrir `01_transacciones.sql` y ejecutar con F5. Debe indicar `No errors`.
3. Ejecutar `02_pruebas.sql` con F5 en una sesión sin cambios pendientes. Debe mostrar `OK: cinco operaciones, seis casos de rechazo y rollback verificados.` Se revierten los registros temporales; los valores consumidos de una secuencia no se recuperan.
4. Para exponer, ejecutar `03_cinco_transacciones.sql` en una copia de práctica. Pide un documento nuevo y una oferta existente, Vigente y dentro de plazo. Este script GUARDA cambios y CIERRA la oferta elegida; escoger una oferta de demostración. Introducir documento simple sin comillas: el guion usa sustitución SQL*Plus; la web utiliza parámetros enlazados.

No reimportar `script_becas_full.sql`: es un export histórico de SYSTEM con objetos internos ajenos al trabajo, no un instalador limpio. Los scripts nuevos no borran tablas. El DDL de instalación hace commits implícitos; las pruebas tienen ROLLBACK final, por lo que deben ejecutarse sin trabajo pendiente en esa sesión.

## Ejecutar la interfaz

La conexión original se conserva en `appsettings.Local.json`, ignorado por Git. En otro equipo:

```powershell
Copy-Item appsettings.Local.example.json appsettings.Local.json
# Editar usuario, clave y servicio LOCALMENTE en appsettings.Local.json.
dotnet restore
dotnet run --no-launch-profile --urls http://localhost:5078
```

Abrir http://localhost:5078. También se acepta la variable `ConnectionStrings__OracleDB`, con prioridad sobre el archivo local. Los cambios hechos en la interfaz se guardan en Oracle. El panel muestra ofertas con estado Vigente y PL/SQL verifica además sus fechas; no se modifican automáticamente para pasar la prueba.

## Trabajo en pareja con GitHub

Remoto: https://github.com/S0laReX/SistemaBecasPosgrado

| Integrante | Desarrollo propuesto | Revisión cruzada |
|---|---|---|
| Integrante de interfaz | Nuevas operaciones 1 y 3: correo y transferencia, formularios y explicación | Revisar la operación 2. |
| Compañero | Nueva operación 2: reprogramar convocatoria | Revisar la operación 1. |
| Ambos | Integración final del proyecto | Probar y explicar las tres operaciones. |

1. El propietario invita la cuenta del compañero en Settings → Collaborators. No compartir contraseñas ni tokens.
2. Cada integrante clona el repositorio y configura una copia Oracle de práctica. GitHub comparte código/scripts, NO sincroniza bases de datos. No hace falta exponer el Oracle local a Internet.
3. Conforme al acuerdo del equipo, trabajar en la rama **`master` ya existente**; no crear ramas nuevas. Antes de empezar, ejecutar `git pull origin master` con el árbol de trabajo limpio.
4. Cada integrante confirma sus cambios con su propia cuenta de Git y un mensaje descriptivo. Coordinar las modificaciones a archivos compartidos como `_Layout.cshtml` y `Program.cs` para evitar conflictos.
5. Antes de subir, compilar, revisar `git status`, confirmar los archivos propios y ejecutar `git pull --rebase origin master` si otro integrante publicó antes. Resolver cualquier conflicto antes de `git push origin master`. Nunca usar `--force` para compartir esta rama.
6. La plantilla de PR y el workflow de compilación siguen disponibles si al final deciden integrar `master` a la rama `main` existente. El workflow no prueba Oracle porque no tiene una base configurada.

```powershell
git status
git add Database/06_transferir_postulacion.sql Database/07_prueba_transferir_postulacion.sql Controllers/NuevasTransaccionesController.cs Models/CorreoPostulanteViewModel.cs Models/TransferenciaPostulacionViewModel.cs Repositories/TransferenciaPostulacionRepository.cs Views/NuevasTransacciones/Index.cshtml Program.cs docs/Transaccion_1_Actualizar_Correo.md docs/Transaccion_3_Transferir_Postulacion.md README.md
git commit -m "Implementa transferencia de postulaciones pendientes"
git pull --rebase origin master
git push origin master
```

Adaptar los archivos para cada integrante. Preparar código no demuestra colaboración: entregar el historial de commits y revisiones que realmente hayan ocurrido. No se han enviado invitaciones desde esta implementación.

## Defensa de la tarea

- Mostrar los objetos, las referencias REF y la colección anidada.
- Ejecutar T1–T5 en SQL Developer y consultar los registros resultantes.
- Provocar duplicado, fechas inválidas y cuarta postulación; explicar el error y la reversión.
- Ejecutar las pruebas para demostrar ROLLBACK, incluido que la secuencia no retrocede.
- Ejecutar una operación desde la web y señalar el procedimiento al que llama C#.
- Mostrar aportes y revisión cruzada en GitHub, con los nombres de ambos estudiantes.

## Validación y límites

Verificado localmente: paquete y cuerpo compilados en Oracle XE; cinco operaciones, seis rechazos y rollback; compilación .NET 10 sin errores, con advertencias de nulabilidad heredadas. Los datos temporales de las pruebas SQL se revierten.

También se verificó en el navegador el diseño, el rechazo de un candidato inexistente conservando los campos y el registro exitoso de un candidato temporal. Se confirmó ese COMMIT desde una segunda sesión Oracle y se retiró el registro temporal. El registro de eventos se dirige a consola/debug para no depender de permisos sobre el Event Log de Windows.

El paquete bloquea al candidato para serializar postulaciones y bloquea la oferta para coordinar postulación/cierre. Estas garantías requieren usar el paquete: las funciones, CRUD y rutas anteriores permanecen y pueden escribir por otras vías. Usar `/Transacciones` para la entrega. El proyecto académico no incorpora autenticación ni roles de evaluación y no está listo para publicarse en Internet. Para despliegue, migrar a esquema dedicado con permisos mínimos; el código heredado contiene referencias a SYSTEM que habría que adaptar.

La configuración original tenía una clave en un archivo versionado. Se retiró del archivo actual, pero sigue pudiendo existir en el historial: cambiarla antes de compartir/publicar y revisar el historial con el propietario. No se ha reescrito el historial.

## Fuentes

- [OracleCommand y procedimientos almacenados](https://docs.oracle.com/en/database/oracle/oracle-database/26/odpnt/featOraCommand.html).
- [Oracle: PL/SQL, SAVEPOINT y ROLLBACK](https://docs.oracle.com/en/database/oracle/oracle-database/18/lnpls/static-sql.html).
- [SQL Developer y ejecución de scripts](https://docs.oracle.com/database/sql-developer-17.2/RPTUG/sql-developer-concepts-usage.htm).
- [GitHub: ramas y pull requests](https://docs.github.com/en/pull-requests/reference/pull-requests).
- [GitHub: revisión de pull requests](https://docs.github.com/en/pull-requests/reference/pull-request-reviews).
