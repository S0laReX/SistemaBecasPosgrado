-- Prueba aislada: crea un postulante temporal, comprueba el procedimiento
-- y revierte TODO el bloque. Ejecutar con F5 luego de 04_actualizar_correo_postulante.sql.
-- No ejecutar con cambios propios pendientes en esta misma sesion.
SET SERVEROUTPUT ON
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK

DECLARE
    v_doc     VARCHAR2(20) := 'QA' || SUBSTR(RAWTOHEX(SYS_GUID()), 1, 18);
    v_correo  VARCHAR2(100);
    v_total   NUMBER;

    PROCEDURE verificar(p_condicion BOOLEAN, p_mensaje VARCHAR2) IS
    BEGIN
        IF p_condicion IS NULL OR NOT p_condicion THEN
            RAISE_APPLICATION_ERROR(-20999, p_mensaje);
        END IF;
    END verificar;
BEGIN
    SAVEPOINT inicio_prueba;
    INSERT INTO postulantes VALUES (
        t_postulante(v_doc, 'Prueba', 'Temporal', 'antes@example.test',
                     'Licenciatura', t_nt_experiencia())
    );

    sp_actualizar_correo_postulante(v_doc, '  DESPUES@example.test  ');
    SELECT correo INTO v_correo FROM postulantes WHERE doc_identidad = v_doc;
    verificar(v_correo = 'despues@example.test', 'No se guardo el correo normalizado.');

    BEGIN
        sp_actualizar_correo_postulante(v_doc, 'DESPUES@example.test');
        RAISE_APPLICATION_ERROR(-20999, 'Se permitio guardar el mismo correo.');
    EXCEPTION WHEN OTHERS THEN
        IF SQLCODE <> -20103 THEN RAISE; END IF;
    END;

    BEGIN
        sp_actualizar_correo_postulante(v_doc, 'correo-invalido');
        RAISE_APPLICATION_ERROR(-20999, 'Se permitio un correo invalido.');
    EXCEPTION WHEN OTHERS THEN
        IF SQLCODE <> -20102 THEN RAISE; END IF;
    END;

    BEGIN
        sp_actualizar_correo_postulante('NO' || SUBSTR(v_doc, 1, 18), 'nuevo@example.test');
        RAISE_APPLICATION_ERROR(-20999, 'Se permitio un postulante inexistente.');
    EXCEPTION WHEN OTHERS THEN
        IF SQLCODE <> -20104 THEN RAISE; END IF;
    END;

    SELECT correo INTO v_correo FROM postulantes WHERE doc_identidad = v_doc;
    verificar(v_correo = 'despues@example.test', 'Una operacion rechazada modifico el correo.');

    ROLLBACK TO inicio_prueba;
    SELECT COUNT(*) INTO v_total FROM postulantes WHERE doc_identidad = v_doc;
    verificar(v_total = 0, 'ROLLBACK no retiro el postulante temporal.');
    DBMS_OUTPUT.PUT_LINE('OK: actualizacion, tres rechazos y ROLLBACK verificados.');
EXCEPTION WHEN OTHERS THEN
    ROLLBACK;
    RAISE;
END;
/
