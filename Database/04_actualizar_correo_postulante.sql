-- Nueva transaccion 1 (integrante de interfaz): actualizar el correo de un postulante existente.
-- Ejecutar con F5 en SQL Developer, conectado al esquema propietario de POSTULANTES.
-- La instalacion (CREATE OR REPLACE) hace commit implicito en Oracle.
-- El procedimiento NO hace COMMIT: la web o el bloque que lo invoca decide.
SET SERVEROUTPUT ON
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK

CREATE OR REPLACE PROCEDURE sp_actualizar_correo_postulante (
    p_doc           IN VARCHAR2,
    p_correo_nuevo  IN VARCHAR2
) AUTHID DEFINER
IS
    v_correo_actual  postulantes.correo%TYPE;
    v_correo_limpio  postulantes.correo%TYPE;
BEGIN
    IF TRIM(p_doc) IS NULL OR LENGTH(TRIM(p_doc)) > 20 THEN
        RAISE_APPLICATION_ERROR(-20101, 'Ingrese un documento de hasta 20 caracteres.');
    END IF;

    v_correo_limpio := LOWER(TRIM(p_correo_nuevo));
    IF v_correo_limpio IS NULL
       OR LENGTH(v_correo_limpio) > 100
       OR NOT REGEXP_LIKE(v_correo_limpio,
           '^[a-z0-9][a-z0-9._%+-]*@[a-z0-9][a-z0-9.-]*[.][a-z]{2,}$')
       OR INSTR(v_correo_limpio, '..') > 0 THEN
        RAISE_APPLICATION_ERROR(-20102, 'Ingrese un correo valido de hasta 100 caracteres.');
    END IF;

    SELECT p.correo
      INTO v_correo_actual
      FROM postulantes p
     WHERE p.doc_identidad = TRIM(p_doc)
       FOR UPDATE WAIT 5;

    IF LOWER(TRIM(v_correo_actual)) = v_correo_limpio THEN
        RAISE_APPLICATION_ERROR(-20103, 'El nuevo correo es igual al correo actual.');
    END IF;

    UPDATE postulantes p
       SET p.correo = v_correo_limpio
     WHERE p.doc_identidad = TRIM(p_doc);

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20104, 'No existe un postulante con ese documento.');
END sp_actualizar_correo_postulante;
/
SHOW ERRORS PROCEDURE sp_actualizar_correo_postulante

DECLARE
    v_errores NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_errores
      FROM user_errors
     WHERE name = 'SP_ACTUALIZAR_CORREO_POSTULANTE';
    IF v_errores > 0 THEN
        RAISE_APPLICATION_ERROR(-20105, 'El procedimiento tiene errores de compilacion. Revise USER_ERRORS.');
    END IF;
    DBMS_OUTPUT.PUT_LINE('SP_ACTUALIZAR_CORREO_POSTULANTE instalado correctamente.');
END;
/
