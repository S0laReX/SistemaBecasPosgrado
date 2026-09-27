-- Prueba reversible de la transaccion 3. Ejecutar con F5 despues del script 06.
-- Use una sesion sin cambios propios pendientes; el bloque termina con ROLLBACK.
SET SERVEROUTPUT ON
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK

DECLARE
    v_doc              VARCHAR2(20) := 'QA' || SUBSTR(RAWTOHEX(SYS_GUID()), 1, 18);
    v_base             NUMBER := 700000000 + TO_NUMBER(SUBSTR(RAWTOHEX(SYS_GUID()), 1, 6), 'XXXXXX');
    v_origen           NUMBER;
    v_destino          NUMBER;
    v_solicitud        NUMBER;
    v_duplicada        NUMBER;
    v_ref_postulante   REF t_postulante;
    v_ref_origen       REF t_oferta;
    v_ref_destino      REF t_oferta;
    v_oferta_actual    NUMBER;
    v_estado_actual    VARCHAR2(20);
    v_doc_actual       VARCHAR2(20);
    v_total            NUMBER;

    PROCEDURE verificar(p_condicion BOOLEAN, p_mensaje VARCHAR2) IS
    BEGIN
        IF p_condicion IS NULL OR NOT p_condicion THEN
            RAISE_APPLICATION_ERROR(-20999, p_mensaje);
        END IF;
    END verificar;
BEGIN
    v_origen    := v_base;
    v_destino   := v_base + 1;
    v_solicitud := v_base + 2;
    v_duplicada := v_base + 3;
    SAVEPOINT inicio_prueba;

    INSERT INTO postulantes VALUES (
        t_postulante(v_doc, 'Prueba', 'Temporal', 'transferencia@example.test',
                     'Licenciatura', t_nt_experiencia())
    );
    INSERT INTO ofertas VALUES (t_oferta(v_origen, NULL, TRUNC(SYSDATE)-2,
        TRUNC(SYSDATE)+2, 'Prueba', 'Vigente'));
    INSERT INTO ofertas VALUES (t_oferta(v_destino, NULL, TRUNC(SYSDATE)-2,
        TRUNC(SYSDATE)+2, 'Prueba', 'Vigente'));
    SELECT REF(p) INTO v_ref_postulante FROM postulantes p WHERE p.doc_identidad = v_doc;
    SELECT REF(o) INTO v_ref_origen FROM ofertas o WHERE o.id_oferta = v_origen;
    SELECT REF(o) INTO v_ref_destino FROM ofertas o WHERE o.id_oferta = v_destino;
    INSERT INTO solicitudes VALUES (t_solicitud(v_solicitud, v_ref_postulante,
        v_ref_origen, 'Motivacion temporal', 'Pendiente'));

    sp_transferir_postulacion(v_solicitud, v_destino);
    SELECT s.ref_oferta.id_oferta, s.ref_postulante.doc_identidad, s.estado
      INTO v_oferta_actual, v_doc_actual, v_estado_actual
      FROM solicitudes s WHERE s.id_solicitud = v_solicitud;
    verificar(v_oferta_actual = v_destino AND v_doc_actual = v_doc
             AND v_estado_actual = 'Pendiente',
             'La transferencia altero datos adicionales o no cambio la oferta.');

    BEGIN
        sp_transferir_postulacion(v_solicitud, v_destino);
        RAISE_APPLICATION_ERROR(-20999, 'Se permitio la misma oferta de destino.');
    EXCEPTION WHEN OTHERS THEN
        IF SQLCODE <> -20205 THEN RAISE; END IF;
    END;

    sp_transferir_postulacion(v_solicitud, v_origen);
    UPDATE ofertas SET estado_oferta = 'Cerrada' WHERE id_oferta = v_destino;
    BEGIN
        sp_transferir_postulacion(v_solicitud, v_destino);
        RAISE_APPLICATION_ERROR(-20999, 'Se permitio una oferta cerrada.');
    EXCEPTION WHEN OTHERS THEN
        IF SQLCODE <> -20206 THEN RAISE; END IF;
    END;

    UPDATE ofertas SET estado_oferta = 'Vigente', fecha_fin = TRUNC(SYSDATE)-1
     WHERE id_oferta = v_destino;
    BEGIN
        sp_transferir_postulacion(v_solicitud, v_destino);
        RAISE_APPLICATION_ERROR(-20999, 'Se permitio una oferta fuera de plazo.');
    EXCEPTION WHEN OTHERS THEN
        IF SQLCODE <> -20206 THEN RAISE; END IF;
    END;

    UPDATE ofertas SET fecha_fin = TRUNC(SYSDATE)+2 WHERE id_oferta = v_destino;
    INSERT INTO solicitudes VALUES (t_solicitud(v_duplicada, v_ref_postulante,
        v_ref_destino, 'Otra solicitud', 'Pendiente'));
    BEGIN
        sp_transferir_postulacion(v_solicitud, v_destino);
        RAISE_APPLICATION_ERROR(-20999, 'Se permitio una solicitud duplicada.');
    EXCEPTION WHEN OTHERS THEN
        IF SQLCODE <> -20207 THEN RAISE; END IF;
    END;

    UPDATE solicitudes SET estado = 'Aceptada' WHERE id_solicitud = v_solicitud;
    BEGIN
        sp_transferir_postulacion(v_solicitud, v_destino);
        RAISE_APPLICATION_ERROR(-20999, 'Se permitio transferir una solicitud aceptada.');
    EXCEPTION WHEN OTHERS THEN
        IF SQLCODE <> -20203 THEN RAISE; END IF;
    END;

    SELECT s.ref_oferta.id_oferta INTO v_oferta_actual
      FROM solicitudes s WHERE s.id_solicitud = v_solicitud;
    verificar(v_oferta_actual = v_origen, 'Un caso rechazado modifico la oferta.');

    ROLLBACK TO inicio_prueba;
    SELECT COUNT(*) INTO v_total FROM solicitudes WHERE id_solicitud IN (v_solicitud, v_duplicada);
    verificar(v_total = 0, 'ROLLBACK no retiro las solicitudes temporales.');
    DBMS_OUTPUT.PUT_LINE('OK: transferencia, cinco rechazos y ROLLBACK verificados.');
    ROLLBACK;
EXCEPTION WHEN OTHERS THEN
    ROLLBACK;
    RAISE;
END;
/
