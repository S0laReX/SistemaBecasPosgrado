# Nueva transacción 3: transferir una postulación pendiente

Esta operación de la segunda etapa queda a cargo del integrante que también implementó la transacción 1. Es un **procedimiento almacenado**, `sp_transferir_postulacion`, no un trigger. Recibe el ID de una solicitud y el ID de la convocatoria de destino. Conserva el mismo registro de `SOLICITUDES`, postulante, resumen y estado; cambia únicamente su atributo `REF_OFERTA`, que es una referencia de Oracle al objeto `T_OFERTA` de destino.

## Instalación y comprobación

1. Conectarse en SQL Developer al esquema propietario de `SOLICITUDES`, `POSTULANTES` y `OFERTAS`.
2. Ejecutar con **F5** `Database/06_transferir_postulacion.sql`. Debe quedar un procedimiento `VALID`. `CREATE OR REPLACE` reemplaza el procedimiento si ya existía y, como DDL, Oracle hace un commit implícito en esa sesión. Ejecutarlo sin cambios propios pendientes.
3. Ejecutar con **F5** `Database/07_prueba_transferir_postulacion.sql`. Debe mostrar `OK: transferencia, cinco rechazos y ROLLBACK verificados.` La prueba crea objetos temporales, comprueba el resultado y revierte todos los datos creados.
4. En la aplicación, usar **Modo Administrador → Proyecto en equipo → Transferir una postulación**. El formulario se habilita cuando el procedimiento está instalado y válido.

La instalación en el Oracle local **no viaja con Git**. La persona que trabaja en la segunda operación tendrá que ejecutar el script 06 en su propia base para usar también la tercera pantalla.

## Reglas de negocio

| Condición | Respuesta |
|---|---|
| IDs nulos, fraccionarios o no positivos | Error `ORA-20201`. |
| La solicitud no existe | Error `ORA-20202`. |
| La solicitud no está `Pendiente` | Error `ORA-20203`; una solicitud ya resuelta no se mueve. |
| La solicitud apunta a un postulante o una oferta inexistentes | Error `ORA-20204`. Las columnas `REF` originales no tienen una restricción referencial que impida referencias colgantes. |
| El destino es la misma oferta actual | Error `ORA-20205`. |
| El destino no existe, no está `Vigente` o está fuera del rango `FECHA_INICIO`–`FECHA_FIN` | Error `ORA-20206`. Se comparan fechas sin hora mediante `TRUNC`. |
| El postulante ya tiene otra solicitud en el destino | Error `ORA-20207`, aunque la otra solicitud tenga otro estado; coincide con la regla de duplicados del paquete original. |
| Todas las reglas se cumplen | Se actualiza solo `REF_OFERTA`. El ID, el `REF_POSTULANTE`, el resumen y el estado no cambian. |

El número de solicitudes de ese postulante permanece igual, por lo que la transferencia no consume un cupo adicional del máximo de tres postulaciones. La convocatoria de origen puede estar cerrada: la regla exige que la **solicitud** siga pendiente y que el **destino** esté abierto. Esta decisión permite trasladar una solicitud pendiente desde una convocatoria cerrada sin reabrirla.

## Sintaxis y construcciones del procedimiento, una por una

El código ejecutable está completo en `Database/06_transferir_postulacion.sql`.

1. **`SET SERVEROUTPUT ON`** permite que SQL Developer muestre `DBMS_OUTPUT.PUT_LINE`. **`WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK`** detiene el script y revierte al producirse un error no controlado. Ambas son directivas del cliente, no sentencias internas del procedimiento.
2. **`CREATE OR REPLACE PROCEDURE sp_transferir_postulacion (...)`** crea o reemplaza un objeto ejecutable. El nombre identifica lo que llama C# y lo que se puede ejecutar desde SQL Developer.
3. **`p_solicitud IN NUMBER` y `p_oferta_destino IN NUMBER`** son parámetros de entrada numéricos. `IN` indica que llegan desde el llamador y no se devuelven modificados.
4. **`AUTHID DEFINER`** hace que el procedimiento acceda a las tablas con los permisos de su propietario. Debe instalarse en el esquema que contiene las tablas de esta práctica.
5. **`IS`** abre las declaraciones. Variables como **`v_estado solicitudes.estado%TYPE`** y **`v_inicio_destino ofertas.fecha_inicio%TYPE`** heredan el tipo de la columna mediante `%TYPE`. **`v_ref_destino REF t_oferta`** almacena una referencia al objeto `T_OFERTA`, no solo su número de ID.
6. **`BEGIN`** inicia la parte ejecutable. El primer **`IF ... THEN ... END IF`** comprueba valores nulos, positivos y enteros. **`TRUNC(p_solicitud)`** elimina la parte decimal del número; la comparación detecta un ID fraccionario.
7. El primer bloque anidado **`BEGIN ... EXCEPTION WHEN NO_DATA_FOUND ... END`** busca la solicitud con **`SELECT s.estado INTO v_estado`**. `INTO` guarda el valor en una variable PL/SQL. **`WHERE s.id_solicitud = p_solicitud`** limita la búsqueda a una fila; **`FOR UPDATE WAIT 5`** la bloquea y espera como máximo cinco segundos si otra transacción la está editando. `NO_DATA_FOUND` se convierte en `ORA-20202`.
8. **`IF v_estado ... <> 'Pendiente'`** impide cambiar solicitudes aceptadas o rechazadas. **`RAISE_APPLICATION_ERROR(-20203, '...')`** envía un código y un mensaje legible al llamador; los códigos `-20201` a `-20207` distinguen cada regla.
9. **`s.ref_postulante.doc_identidad`** y **`s.ref_oferta.id_oferta`** leen atributos de los objetos apuntados por los `REF` originales. El alias `s` es necesario en Oracle para esta notación. Si la referencia no produce un postulante u oferta válidos, la validación devuelve `ORA-20204`.
10. Un segundo **`SELECT ... FOR UPDATE WAIT 5`** bloquea la fila del postulante. Esto serializa las operaciones de un mismo candidato para que dos transferencias concurrentes no aprueben a la vez una solicitud duplicada en el mismo destino.
11. La comparación **`v_oferta_origen = p_oferta_destino`** impide transferir a la oferta actual. Se informa `ORA-20205`.
12. **`SELECT REF(o), o.estado_oferta, o.fecha_inicio, o.fecha_fin INTO ... FROM ofertas o ... FOR UPDATE WAIT 5`** comprueba y bloquea la oferta de destino. **`REF(o)`** devuelve la referencia real al objeto de esa tabla; el procedimiento la guarda en `v_ref_destino`. El bloque anidado diferencia una oferta inexistente de otros errores.
13. **`TRUNC(SYSDATE) NOT BETWEEN TRUNC(v_inicio_destino) AND TRUNC(v_fin_destino)`** comprueba si la fecha de hoy cae fuera de la ventana de postulación. `SYSDATE` es la fecha/hora del servidor Oracle; `TRUNC` elimina la hora; `BETWEEN` incluye ambos extremos.
14. **`SELECT COUNT(*) INTO v_duplicados`** cuenta solicitudes del mismo postulante en la oferta de destino. El filtro **`s.id_solicitud <> p_solicitud`** excluye la solicitud que se está transfiriendo. Si el conteo es mayor que cero, se genera `ORA-20207`.
15. **`UPDATE solicitudes s SET s.ref_oferta = v_ref_destino WHERE s.id_solicitud = p_solicitud`** cambia una sola referencia de una sola solicitud. No elimina ni inserta solicitudes; por eso mantiene su identidad y los demás atributos.
16. **`END sp_transferir_postulacion;`** cierra el procedimiento y **`/`** ordena a SQL Developer/SQL*Plus ejecutar la definición. **`SHOW ERRORS`** y la consulta a **`USER_ERRORS`** comprueban si Oracle lo compiló; `DBMS_OUTPUT.PUT_LINE` imprime la confirmación.

**El procedimiento no contiene `COMMIT` ni `ROLLBACK`.** El llamador posee la transacción. La web inicia `OracleConnection.BeginTransaction()`, llama el procedimiento con parámetros enlazados, ejecuta `Commit()` si termina bien y `Rollback()` ante errores. Los bloqueos se liberan cuando termina esa transacción. El filtro `RequiereModoAdmin` y el token antifalsificación protegen el flujo de demostración en la interfaz; el modo administrador actual es solo una cookie y **no equivale a autenticación real**.

## Cómo funciona la prueba reversible

`Database/07_prueba_transferir_postulacion.sql` comienza con **`DECLARE`**, variables y un procedimiento auxiliar `verificar`. **`SYS_GUID()`** produce valores temporales difíciles de repetir. **`SAVEPOINT inicio_prueba`** marca el inicio de los cambios de prueba. Los constructores **`t_postulante(...)`**, **`t_oferta(...)`** y **`t_solicitud(...)`** insertan objetos compatibles con las tablas del proyecto; **`REF(p)`** y **`REF(o)`** crean las relaciones reales.

El primer llamado transfiere la solicitud y una consulta verifica destino, postulante y estado. Después, bloques anidados `BEGIN ... EXCEPTION WHEN OTHERS` provocan y comprueban cinco rechazos: mismo destino, destino cerrado, destino fuera de plazo, duplicado y solicitud aceptada. **`SQLCODE`** identifica el error esperado; **`RAISE`** propaga cualquier fallo diferente. La prueba confirma que los rechazos no cambiaron el destino y finalmente ejecuta **`ROLLBACK TO inicio_prueba`**. Un conteo comprueba que las solicitudes temporales desaparecieron; el `ROLLBACK` exterior cubre una falla inesperada.

## Ejecución manual que sí guarda

Con IDs de una **copia de práctica**, una solicitud pendiente y una oferta de destino vigente:

```sql
BEGIN
    sp_transferir_postulacion(501, 102);
    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END;
/
```

Este es un **bloque anónimo**: `BEGIN` invoca el procedimiento, `COMMIT` confirma si funciona, `ROLLBACK` revierte ante una excepción y `RAISE` deja visible el error. Para practicar sin conservar cambios, ejecutar el script 07 en vez de usar IDs reales.

## Comprobación realizada

El procedimiento se instaló y quedó `VALID` en Oracle XE local. Pasó la prueba reversible de transferencia, cinco rechazos y `ROLLBACK`. La web mostró el error para una solicitud inexistente conservando los IDs escritos. Con objetos temporales, el formulario hizo un `COMMIT`; otra conexión confirmó el nuevo `REF_OFERTA` y que el postulante y el estado no cambiaron. Después se retiraron esos objetos temporales. La compilación .NET terminó sin errores.

## Fuentes oficiales

- [Oracle: referencias `REF` en tablas de objetos](https://docs.oracle.com/en/database/oracle/oracle-database/21/adobj/Sql-object-types-and-references.html).
- [Oracle: `SELECT ... FOR UPDATE` y control de transacciones](https://docs.oracle.com/en/database/oracle/oracle-database/21/lnpls/static-sql.html).
- [Oracle: excepciones PL/SQL](https://docs.oracle.com/en/database/oracle/oracle-database/18/lnpls/plsql-error-handling.html).
