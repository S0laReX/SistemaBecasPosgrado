-- Ejecutar con F5 en el esquema que contiene POSTULANTES, OFERTAS y SOLICITUDES.
-- No ejecutar el export completo de SYSTEM. Instalacion aditiva, sin borrar datos.
SET SERVEROUTPUT ON
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK

-- Solo se calcula MAX al instalar, con la aplicacion detenida. En ejecucion se usa NEXTVAL.
DECLARE
  n NUMBER;
  existe NUMBER;
BEGIN
  SELECT COUNT(*) INTO existe FROM user_sequences WHERE sequence_name = 'SEQ_BECAS_SOLICITUD';
  IF existe = 0 THEN
    SELECT NVL(MAX(id_solicitud),0)+1 INTO n FROM solicitudes;
    EXECUTE IMMEDIATE 'CREATE SEQUENCE seq_becas_solicitud START WITH ' || TO_CHAR(n,'TM9');
  END IF;
END;
/
CREATE OR REPLACE PACKAGE pkg_becas AS
  PROCEDURE registrar_candidato(p_doc VARCHAR2, p_nombres VARCHAR2, p_apellidos VARCHAR2, p_correo VARCHAR2, p_nivel VARCHAR2);
  PROCEDURE agregar_experiencia(p_doc VARCHAR2, p_empresa VARCHAR2, p_cargo VARCHAR2, p_inicio DATE, p_fin DATE);
  PROCEDURE postular(p_doc VARCHAR2, p_oferta NUMBER, p_resumen VARCHAR2);
  PROCEDURE resolver(p_solicitud NUMBER, p_estado VARCHAR2);
  PROCEDURE cerrar_oferta(p_oferta NUMBER);
END pkg_becas;
/
CREATE OR REPLACE PACKAGE BODY pkg_becas AS
  PROCEDURE registrar_candidato(p_doc VARCHAR2, p_nombres VARCHAR2, p_apellidos VARCHAR2, p_correo VARCHAR2, p_nivel VARCHAR2) IS
  BEGIN
    IF TRIM(p_doc) IS NULL OR TRIM(p_nombres) IS NULL OR TRIM(p_apellidos) IS NULL OR TRIM(p_nivel) IS NULL
       OR p_correo IS NULL OR NOT REGEXP_LIKE(p_correo, '^[^ @]+@[^ @]+\.[^ @]+$') THEN
      RAISE_APPLICATION_ERROR(-20001,'Complete los datos del candidato y un correo valido.');
    END IF;
    INSERT INTO postulantes VALUES(t_postulante(TRIM(p_doc),TRIM(p_nombres),TRIM(p_apellidos),TRIM(p_correo),TRIM(p_nivel),t_nt_experiencia()));
  EXCEPTION WHEN DUP_VAL_ON_INDEX THEN
    RAISE_APPLICATION_ERROR(-20002,'Ya existe un candidato con ese documento.');
  END;

  PROCEDURE agregar_experiencia(p_doc VARCHAR2, p_empresa VARCHAR2, p_cargo VARCHAR2, p_inicio DATE, p_fin DATE) IS
    v_doc VARCHAR2(20);
  BEGIN
    IF TRIM(p_empresa) IS NULL OR TRIM(p_cargo) IS NULL OR p_inicio IS NULL OR p_fin < p_inicio THEN
      RAISE_APPLICATION_ERROR(-20003,'Revise empresa, cargo y fechas de experiencia.');
    END IF;
    SELECT doc_identidad INTO v_doc FROM postulantes WHERE doc_identidad = p_doc FOR UPDATE WAIT 5;
    UPDATE postulantes p SET p.experiencias = t_nt_experiencia() WHERE p.doc_identidad=p_doc AND p.experiencias IS NULL;
    INSERT INTO TABLE(SELECT p.experiencias FROM postulantes p WHERE p.doc_identidad=p_doc)
      VALUES(t_experiencia(TRIM(p_empresa),TRIM(p_cargo),p_inicio,p_fin));
  EXCEPTION WHEN NO_DATA_FOUND THEN RAISE_APPLICATION_ERROR(-20004,'El candidato no existe.');
  END;

  PROCEDURE postular(p_doc VARCHAR2, p_oferta NUMBER, p_resumen VARCHAR2) IS
    v_doc VARCHAR2(20); v_estado VARCHAR2(20); v_inicio DATE; v_fin DATE; n NUMBER;
    rp REF t_postulante; ro REF t_oferta;
  BEGIN
    IF TRIM(p_resumen) IS NULL OR LENGTH(p_resumen)>2000 THEN
      RAISE_APPLICATION_ERROR(-20005,'Ingrese una motivacion de hasta 2000 caracteres.');
    END IF;
    -- Serializa postulaciones del mismo candidato y el cierre de la oferta.
    SELECT doc_identidad INTO v_doc FROM postulantes WHERE doc_identidad=p_doc FOR UPDATE WAIT 5;
    SELECT estado_oferta,fecha_inicio,fecha_fin INTO v_estado,v_inicio,v_fin FROM ofertas WHERE id_oferta=p_oferta FOR UPDATE WAIT 5;
    IF v_estado IS NULL OR v_estado <> 'Vigente' OR v_inicio IS NULL OR v_fin IS NULL OR TRUNC(SYSDATE) NOT BETWEEN TRUNC(v_inicio) AND TRUNC(v_fin) THEN
      RAISE_APPLICATION_ERROR(-20006,'La oferta no esta vigente o esta fuera de plazo.');
    END IF;
    SELECT COUNT(*) INTO n FROM solicitudes s WHERE s.ref_postulante.doc_identidad=p_doc AND s.ref_oferta.id_oferta=p_oferta;
    IF n>0 THEN RAISE_APPLICATION_ERROR(-20007,'El candidato ya postulo a esta oferta.'); END IF;
    SELECT COUNT(*) INTO n FROM solicitudes s WHERE s.ref_postulante.doc_identidad=p_doc;
    IF n>=3 THEN RAISE_APPLICATION_ERROR(-20008,'El candidato alcanzo el limite de tres postulaciones.'); END IF;
    SELECT REF(p) INTO rp FROM postulantes p WHERE p.doc_identidad=p_doc;
    SELECT REF(o) INTO ro FROM ofertas o WHERE o.id_oferta=p_oferta;
    INSERT INTO solicitudes VALUES(t_solicitud(seq_becas_solicitud.NEXTVAL,rp,ro,p_resumen,'Pendiente'));
  EXCEPTION WHEN NO_DATA_FOUND THEN RAISE_APPLICATION_ERROR(-20009,'No existe el candidato o la oferta.');
  END;

  PROCEDURE resolver(p_solicitud NUMBER, p_estado VARCHAR2) IS
    v_estado VARCHAR2(20);
  BEGIN
    IF p_estado IS NULL OR p_estado NOT IN ('Aceptada','Rechazada') THEN
      RAISE_APPLICATION_ERROR(-20010,'Seleccione Aceptada o Rechazada.');
    END IF;
    SELECT estado INTO v_estado FROM solicitudes WHERE id_solicitud=p_solicitud FOR UPDATE WAIT 5;
    IF v_estado IS NULL OR v_estado<>'Pendiente' THEN RAISE_APPLICATION_ERROR(-20011,'Solo se pueden resolver solicitudes pendientes.'); END IF;
    UPDATE solicitudes SET estado=p_estado WHERE id_solicitud=p_solicitud;
  EXCEPTION WHEN NO_DATA_FOUND THEN RAISE_APPLICATION_ERROR(-20012,'La solicitud no existe.');
  END;

  PROCEDURE cerrar_oferta(p_oferta NUMBER) IS
    v_estado VARCHAR2(20);
  BEGIN
    SELECT estado_oferta INTO v_estado FROM ofertas WHERE id_oferta=p_oferta FOR UPDATE WAIT 5;
    IF v_estado IS NULL OR v_estado<>'Vigente' THEN RAISE_APPLICATION_ERROR(-20013,'Solo se puede cerrar una oferta vigente.'); END IF;
    UPDATE ofertas SET estado_oferta='Cerrada' WHERE id_oferta=p_oferta;
  EXCEPTION WHEN NO_DATA_FOUND THEN RAISE_APPLICATION_ERROR(-20014,'La oferta no existe.');
  END;
END pkg_becas;
/
SHOW ERRORS PACKAGE BODY pkg_becas
DECLARE n NUMBER;
BEGIN
  SELECT COUNT(*) INTO n FROM user_errors WHERE name='PKG_BECAS';
  IF n>0 THEN RAISE_APPLICATION_ERROR(-20090,'PKG_BECAS tiene errores de compilacion. Consulte USER_ERRORS.'); END IF;
END;
/
-- COMMIT/ROLLBACK pertenecen al llamador: interfaz web o bloques de demostracion.
