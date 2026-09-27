# Nueva transacción 1: actualizar el correo de un postulante

Esta es **tu operación** de la segunda etapa del proyecto. Es un **procedimiento almacenado**, no un trigger: la web o SQL Developer lo invoca explícitamente con un documento y un correo nuevo. Modifica únicamente `POSTULANTES.CORREO` del documento indicado. La convocatoria y las solicitudes no cambian. Las operaciones 2 y 3 siguen pendientes para el compañero y el trabajo conjunto.

## Archivos y orden de ejecución

1. Abrir `Database/04_actualizar_correo_postulante.sql` en SQL Developer, conectado al **esquema propietario de `POSTULANTES`**, y ejecutar con **F5**. La instalación reemplaza el procedimiento si ya existe; no recrea tablas ni borra datos.
2. Abrir `Database/05_prueba_actualizar_correo.sql` y ejecutar con **F5**. Debe mostrar `OK: actualizacion, tres rechazos y ROLLBACK verificados.` Utiliza un postulante temporal y revierte su inserción y actualización.
3. En la web, entrar en **Modo Administrador → Proyecto en equipo → Actualizar correo de un postulante**. El formulario se habilita cuando la conexión y el procedimiento son válidos.

GitHub comparte los scripts y el código; **no instala procedimientos en el Oracle de la otra persona**. El compañero deberá ejecutar el script 04 en su propio esquema de práctica. `CREATE OR REPLACE PROCEDURE` es DDL y Oracle hace un commit implícito durante la instalación. Por eso conviene ejecutarlo sin modificaciones propias pendientes en esa sesión.

## Regla de negocio y resultado esperado

| Caso | Resultado |
|---|---|
| Documento existente y correo nuevo válido | Se guarda el correo sin alterar nombre, identidad, experiencias ni solicitudes. |
| Documento vacío o de más de 20 caracteres | Error `ORA-20101`; no se actualiza nada. |
| Correo vacío, de más de 100 caracteres o con formato inválido | Error `ORA-20102`; no se actualiza nada. |
| Correo igual al actual, ignorando mayúsculas y espacios externos | Error `ORA-20103`; no se hace una escritura innecesaria. |
| Documento inexistente | Error `ORA-20104`; no se actualiza otra fila. |
| Dos usuarios intentan editar el mismo postulante | `FOR UPDATE WAIT 5` espera hasta cinco segundos por el bloqueo; si no lo obtiene, Oracle rechaza la operación y la web muestra un aviso para reintentar. |

El nuevo correo se almacena sin espacios externos y en minúsculas. La expresión regular verifica un formato básico y `INSTR(..., '..')` rechaza puntos consecutivos. Esto no comprueba que el buzón exista ni impone unicidad: el esquema actual no tiene una restricción única para `CORREO`.

## Sintaxis y construcciones PL/SQL, una por una

El procedimiento completo y ejecutable está en `Database/04_actualizar_correo_postulante.sql`. Estas son todas sus partes:

1. **`SET SERVEROUTPUT ON`**: directiva del cliente SQL Developer/SQL*Plus para mostrar texto escrito con `DBMS_OUTPUT.PUT_LINE`. No es una instrucción del procedimiento.
2. **`WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK`**: directiva del cliente para detener el script si hay un error SQL y revertir su transacción. Tampoco forma parte del procedimiento almacenado.
3. **`CREATE OR REPLACE PROCEDURE sp_actualizar_correo_postulante (...)`**: crea un objeto ejecutable en Oracle o reemplaza su versión anterior. `sp_actualizar_correo_postulante` es el nombre que llama la aplicación.
4. **`p_doc IN VARCHAR2` y `p_correo_nuevo IN VARCHAR2`**: parámetros de entrada. `IN` significa que el llamador proporciona sus valores y el procedimiento no los devuelve modificados. `VARCHAR2` indica texto de longitud variable. La longitud se controla dentro del cuerpo porque los parámetros PL/SQL no se declaran aquí como columnas `VARCHAR2(20)` y `VARCHAR2(100)`.
5. **`AUTHID DEFINER`**: el procedimiento usa los permisos de su propietario para acceder a `POSTULANTES`. Se instala en el mismo esquema que la tabla.
6. **`IS`**: inicia la sección de declaraciones locales. **`v_correo_actual postulantes.correo%TYPE`** y **`v_correo_limpio postulantes.correo%TYPE`** crean variables con el mismo tipo que la columna `CORREO`; `%TYPE` evita duplicar la definición del tipo.
7. **`BEGIN`**: inicia las instrucciones ejecutables. Todo lo anterior declara el procedimiento y sus variables; desde aquí se validan datos y se modifica la fila.
8. **`IF ... THEN ... END IF;`**: condición PL/SQL. En la primera condición, `TRIM(p_doc)` elimina espacios exteriores, `IS NULL` rechaza el documento vacío y `LENGTH(...) > 20` impide superar la longitud del identificador.
9. **`RAISE_APPLICATION_ERROR(-20101, '...')`**: genera un error controlado con mensaje legible. Los códigos `-20101` a `-20104` identifican reglas distintas y la web los muestra sin exponer errores internos.
10. **`:=`**, **`LOWER`** y **`TRIM`**: `:=` asigna un valor a `v_correo_limpio`; `TRIM` quita espacios externos y `LOWER` pasa el correo a minúsculas antes de comparar y guardar.
11. **`REGEXP_LIKE`**: comprueba que el correo tenga caracteres permitidos, `@`, dominio y terminación alfabética. La expresión entre comillas es un patrón de validación, no una consulta SQL. **`INSTR(v_correo_limpio, '..') > 0`** detecta dos puntos seguidos. **`OR`** agrupa las razones por las que se rechaza el correo.
12. **`SELECT p.correo INTO v_correo_actual FROM postulantes p WHERE ...`**: obtiene el correo actual de **una sola fila**; `INTO` deposita el valor en la variable PL/SQL y `WHERE` limita la búsqueda al documento recibido. Si no hay fila, Oracle genera `NO_DATA_FOUND`.
13. **`FOR UPDATE WAIT 5`**: bloquea esa fila hasta terminar la transacción, esperando como máximo cinco segundos. Evita que dos actualizaciones concurrentes lean simultáneamente el mismo valor y se sobrescriban sin control.
14. **`IF LOWER(TRIM(v_correo_actual)) = v_correo_limpio`**: compara el valor anterior con el nuevo normalizado. Si son equivalentes, `RAISE_APPLICATION_ERROR(-20103, ...)` cancela la actualización.
15. **`UPDATE postulantes p SET p.correo = v_correo_limpio WHERE p.doc_identidad = TRIM(p_doc)`**: cambia **solo la columna `CORREO`** de la fila seleccionada. El `WHERE` es esencial: sin él se cambiarían los correos de todos los postulantes.
16. **`EXCEPTION WHEN NO_DATA_FOUND THEN ...`**: captura únicamente el caso en que el `SELECT ... INTO` no encontró al postulante y lo transforma en `ORA-20104`. Otros errores inesperados se propagan al llamador; no se ocultan.
17. **`END sp_actualizar_correo_postulante;`** cierra la definición PL/SQL. La línea **`/`** indica a SQL Developer/SQL*Plus que ejecute el bloque `CREATE OR REPLACE` ya escrito; no es parte del procedimiento.
18. **`SHOW ERRORS`**, **`USER_ERRORS`** y **`DBMS_OUTPUT.PUT_LINE`** verifican la instalación. `SHOW ERRORS` muestra errores de compilación; la consulta cuenta los errores del objeto; `DBMS_OUTPUT` imprime una confirmación cuando compila correctamente.

**No hay `COMMIT` dentro del procedimiento.** La transacción comprende el bloqueo y el `UPDATE`, pero el llamador decide su resultado final. En la aplicación, `OracleConnection.BeginTransaction()` inicia el trabajo, `ExecuteNonQuery()` llama el procedimiento, `Commit()` confirma el cambio y `Rollback()` lo revierte ante una excepción. Los dos valores viajan como parámetros enlazados, no concatenados en SQL.

## Sintaxis de la prueba reversible

En `Database/05_prueba_actualizar_correo.sql`, **`DECLARE`** define variables y un procedimiento auxiliar `verificar`. **`SYS_GUID()`** genera un documento temporal difícil de repetir. **`SAVEPOINT inicio_prueba`** marca el punto al que se volverá. **`INSERT INTO postulantes VALUES (t_postulante(..., t_nt_experiencia()))`** crea un objeto del mismo tipo que utiliza la base; `t_nt_experiencia()` inicializa la colección de experiencias vacía.

La prueba llama al procedimiento con un correo válido y consulta el resultado con `SELECT ... INTO`. Después utiliza tres bloques anidados `BEGIN ... EXCEPTION WHEN OTHERS` para provocar, por separado, correo repetido, correo inválido y documento inexistente. **`SQLCODE`** comprueba que cada fallo corresponda al código esperado; `RAISE` vuelve a lanzar cualquier error distinto. Finalmente, **`ROLLBACK TO inicio_prueba`** revierte la actualización y el postulante temporal, y otra consulta comprueba que ya no existe. El `ROLLBACK` del manejador exterior protege la prueba si falla antes de llegar al final.

## Ejecución manual y relación con la interfaz

Para una demostración que **sí guarde** un cambio en un postulante de práctica:

```sql
BEGIN
    sp_actualizar_correo_postulante('DOCUMENTO_DE_PRACTICA', 'nuevo@example.test');
    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END;
/
```

Aquí `BEGIN ... END` es un **bloque anónimo** que invoca al procedimiento; no crea otro objeto. `COMMIT` confirma si todo terminó bien; `ROLLBACK` revierte si ocurre un error; `RAISE` comunica ese error a SQL Developer. Para practicar sin conservar cambios, utilice el script 05.

En la web, `Models/CorreoPostulanteViewModel.cs` valida los campos antes de llamar a Oracle. `Controllers/NuevasTransaccionesController.cs` verifica el token antifalsificación, llama al repositorio y presenta errores de negocio comprensibles. `Repositories/CorreoPostulanteRepository.cs` enlaza los parámetros y controla `COMMIT`/`ROLLBACK`. La validación decisiva sigue en Oracle, por lo que también se aplica a llamadas directas desde SQL Developer.

**Límite del proyecto académico:** el «Modo Administrador» actual es una cookie de presentación, no un inicio de sesión real. No publique esta función en Internet ni permita modificar correos de terceros sin autenticación y autorización de identidad.

## Comprobación realizada

El procedimiento se instaló y quedó `VALID` en Oracle XE local. Se ejecutó el script de prueba con actualización, tres rechazos y reversión. También se envió el formulario web con un postulante temporal, se verificó el `COMMIT` desde otra conexión y se retiró el registro temporal. La compilación .NET terminó sin errores.

## Fuentes oficiales

- [Oracle: desarrollo de subprogramas almacenados](https://docs.oracle.com/en/database/oracle/oracle-database/18/tdddg/stored-subprograms-packages.html).
- [Oracle: excepciones y `RAISE_APPLICATION_ERROR`](https://docs.oracle.com/en/database/oracle/oracle-database/18/lnpls/plsql-error-handling.html).
- [Oracle: `OracleCommand` y transacciones](https://docs.oracle.com/en/database/oracle/oracle-database/26/odpnt/featOraCommand.html).
