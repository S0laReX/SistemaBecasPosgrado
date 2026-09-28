CREATE OR REPLACE PROCEDURE sp_reprogramar_convocatoria (
    p_id_oferta          IN ofertas.id_oferta%TYPE,
    p_nueva_fecha_inicio IN ofertas.fecha_inicio%TYPE,
    p_nueva_fecha_fin    IN ofertas.fecha_fin%TYPE
) AUTHID DEFINER IS

    v_estado_actual ofertas.estado_oferta%TYPE;

BEGIN
    IF p_id_oferta IS NULL OR p_id_oferta <= 0 THEN
        RAISE_APPLICATION_ERROR(-20301, 'El identificador de la oferta no es valido.');
    END IF;

    IF p_nueva_fecha_inicio IS NULL OR p_nueva_fecha_fin IS NULL THEN
        RAISE_APPLICATION_ERROR(-20302, 'Las nuevas fechas de inicio y fin son obligatorias.');
    END IF;

    IF p_nueva_fecha_inicio >= p_nueva_fecha_fin THEN
        RAISE_APPLICATION_ERROR(-20303, 'La ventana de postulacion no es coherente: la fecha de inicio debe ser anterior a la de fin.');
    END IF;

    IF TRUNC(p_nueva_fecha_fin) < TRUNC(SYSDATE) THEN
        RAISE_APPLICATION_ERROR(-20304, 'La fecha de fin no puede ser una fecha ya pasada.');
    END IF;

    BEGIN
        SELECT estado_oferta
          INTO v_estado_actual
          FROM ofertas
         WHERE id_oferta = p_id_oferta
         FOR UPDATE WAIT 5;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20305, 'No existe una oferta con el id indicado.');
    END;

    IF v_estado_actual <> 'Vigente' THEN
        RAISE_APPLICATION_ERROR(-20306, 'Solo se pueden reprogramar ofertas que esten vigentes.');
    END IF;

    UPDATE ofertas
       SET fecha_inicio = p_nueva_fecha_inicio,
           fecha_fin    = p_nueva_fecha_fin
     WHERE id_oferta = p_id_oferta;

END sp_reprogramar_convocatoria;
/

CREATE OR REPLACE TRIGGER trg_oferta_validar_reprogramacion
BEFORE UPDATE OF fecha_inicio, fecha_fin ON ofertas
FOR EACH ROW
BEGIN
    IF :NEW.fecha_inicio >=:NEW.fecha_fin THEN
        RAISE_APPLICATION_ERROR(-20307, 'Trigger: la ventana de postulación no es coherente.');
        END IF;
    IF TRUNC(:NEW.fecha_fin)<TRUNC(SYSDATE) THEN
        RAISE_APPLICATION_ERROR(-20308, 'Trigger: la fecha de fin no puede ser pasada.');
    END IF;
        
    IF :OLD.estado_oferta<> 'Vigente' THEN
        RAISE_APPLICATION_ERROR(-20306, 'Trigger: solo se puede reprogramar ofertas vigentes.');
    END IF;
END;
/
SHOW ERRORS TRIGGER trg_oferta_validar_reprogramacion