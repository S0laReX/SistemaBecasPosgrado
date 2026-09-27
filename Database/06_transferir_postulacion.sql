-- Nueva transaccion 3: transferir una solicitud pendiente a otra oferta.
-- Ejecutar con F5 en el esquema propietario de SOLICITUDES, POSTULANTES y OFERTAS.
-- CREATE OR REPLACE es DDL y confirma transacciones pendientes de esta sesion.
-- El procedimiento no hace COMMIT: el llamador confirma o revierte.
SET SERVEROUTPUT ON
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK

CREATE OR REPLACE PROCEDURE sp_transferir_postulacion (
    p_solicitud       IN NUMBER,
    p_oferta_destino  IN NUMBER
) AUTHID DEFINER
IS
    v_estado           solicitudes.estado%TYPE;
    v_doc              postulantes.doc_identidad%TYPE;
    v_oferta_origen    ofertas.id_oferta%TYPE;
    v_estado_destino   ofertas.estado_oferta%TYPE;
    v_inicio_destino   ofertas.fecha_inicio%TYPE;
    v_fin_destino      ofertas.fecha_fin%TYPE;
    v_ref_destino      REF t_oferta;
    v_duplicados       NUMBER;
BEGIN
    IF p_solicitud IS NULL OR p_oferta_destino IS NULL
       OR p_solicitud <= 0 OR p_oferta_destino <= 0
       OR p_solicitud <> TRUNC(p_solicitud)
       OR p_oferta_destino <> TRUNC(p_oferta_destino) THEN
        RAISE_APPLICATION_ERROR(-20201, 'Ingrese IDs enteros positivos de solicitud y oferta.');
    END IF;

    BEGIN
        SELECT s.estado INTO v_estado
          FROM solicitudes s
         WHERE s.id_solicitud = p_solicitud
           FOR UPDATE WAIT 5;
    EXCEPTION WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20202, 'La solicitud no existe.');
    END;

    IF v_estado IS NULL OR v_estado <> 'Pendiente' THEN
        RAISE_APPLICATION_ERROR(-20203, 'Solo se puede transferir una solicitud pendiente.');
    END IF;

    SELECT s.ref_postulante.doc_identidad, s.ref_oferta.id_oferta
      INTO v_doc, v_oferta_origen
      FROM solicitudes s
     WHERE s.id_solicitud = p_solicitud;

    IF v_doc IS NULL OR v_oferta_origen IS NULL THEN
        RAISE_APPLICATION_ERROR(-20204, 'La solicitud tiene una referencia de postulante u oferta no valida.');
    END IF;

    BEGIN
        SELECT p.doc_identidad INTO v_doc
          FROM postulantes p
         WHERE p.doc_identidad = v_doc
           FOR UPDATE WAIT 5;
    EXCEPTION WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20204, 'El postulante de la solicitud no existe.');
    END;

    IF v_oferta_origen = p_oferta_destino THEN
        RAISE_APPLICATION_ERROR(-20205, 'La solicitud ya pertenece a esa oferta.');
    END IF;

    BEGIN
        SELECT REF(o), o.estado_oferta, o.fecha_inicio, o.fecha_fin
          INTO v_ref_destino, v_estado_destino, v_inicio_destino, v_fin_destino
          FROM ofertas o
         WHERE o.id_oferta = p_oferta_destino
           FOR UPDATE WAIT 5;
    EXCEPTION WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20206, 'La oferta de destino no existe.');
    END;

    IF v_estado_destino IS NULL OR v_estado_destino <> 'Vigente'
       OR v_inicio_destino IS NULL OR v_fin_destino IS NULL
       OR TRUNC(SYSDATE) NOT BETWEEN TRUNC(v_inicio_destino) AND TRUNC(v_fin_destino) THEN
        RAISE_APPLICATION_ERROR(-20206, 'La oferta de destino no esta vigente o esta fuera de plazo.');
    END IF;

    SELECT COUNT(*) INTO v_duplicados
      FROM solicitudes s
     WHERE s.ref_postulante.doc_identidad = v_doc
       AND s.ref_oferta.id_oferta = p_oferta_destino
       AND s.id_solicitud <> p_solicitud;

    IF v_duplicados > 0 THEN
        RAISE_APPLICATION_ERROR(-20207, 'El postulante ya tiene una solicitud en la oferta de destino.');
    END IF;

    UPDATE solicitudes s
       SET s.ref_oferta = v_ref_destino
     WHERE s.id_solicitud = p_solicitud;
END sp_transferir_postulacion;
/
SHOW ERRORS PROCEDURE sp_transferir_postulacion

DECLARE
    v_errores NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_errores
      FROM user_errors
     WHERE name = 'SP_TRANSFERIR_POSTULACION';
    IF v_errores > 0 THEN
        RAISE_APPLICATION_ERROR(-20208, 'El procedimiento tiene errores de compilacion. Revise USER_ERRORS.');
    END IF;
    DBMS_OUTPUT.PUT_LINE('SP_TRANSFERIR_POSTULACION instalado correctamente.');
END;
/
