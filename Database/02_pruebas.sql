-- Pruebas de integracion: datos aislados y ROLLBACK final. Ejecutar con F5.
SET SERVEROUTPUT ON
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
DECLARE
  doc VARCHAR2(20) := 'QA' || SUBSTR(RAWTOHEX(SYS_GUID()),1,18);
  oferta NUMBER; solicitud NUMBER; n NUMBER; estado VARCHAR2(20);
  PROCEDURE verificar(ok BOOLEAN, mensaje VARCHAR2) IS
  BEGIN IF ok IS NULL OR NOT ok THEN RAISE_APPLICATION_ERROR(-20999,mensaje); END IF; END;
BEGIN
  SAVEPOINT inicio_prueba;
  -- IDs negativos reservados para esta prueba; nunca altera ofertas existentes.
  oferta := -TO_NUMBER(SUBSTR(RAWTOHEX(SYS_GUID()),1,10),'XXXXXXXXXX');
  INSERT INTO ofertas VALUES(t_oferta(oferta,NULL,SYSDATE-1,SYSDATE+1,'Prueba automatizada','Vigente'));
  pkg_becas.registrar_candidato(doc,'Prueba','Temporal','qa@example.test','Licenciatura');
  SELECT COUNT(*) INTO n FROM postulantes WHERE doc_identidad=doc;
  verificar(n=1,'No se registro el candidato');
  BEGIN
    pkg_becas.registrar_candidato(doc,'Duplicado','Temporal','qa@example.test','Licenciatura');
    RAISE_APPLICATION_ERROR(-20999,'Se permitio candidato duplicado');
  EXCEPTION WHEN OTHERS THEN IF SQLCODE<>-20002 THEN RAISE; END IF; END;
  pkg_becas.agregar_experiencia(doc,'Empresa QA','Analista',SYSDATE-30,NULL);
  SELECT COUNT(*) INTO n FROM TABLE(SELECT p.experiencias FROM postulantes p WHERE p.doc_identidad=doc);
  verificar(n=1,'No se registro la experiencia');
  BEGIN
    pkg_becas.agregar_experiencia(doc,'Empresa QA','Analista',SYSDATE,SYSDATE-1);
    RAISE_APPLICATION_ERROR(-20999,'Se permitieron fechas invalidas');
  EXCEPTION WHEN OTHERS THEN IF SQLCODE<>-20003 THEN RAISE; END IF; END;
  pkg_becas.postular(doc,oferta,'Motivacion de prueba');
  SELECT s.id_solicitud INTO solicitud FROM solicitudes s WHERE s.ref_postulante.doc_identidad=doc AND s.ref_oferta.id_oferta=oferta;
  BEGIN
    pkg_becas.postular(doc,oferta,'Duplicada');
    RAISE_APPLICATION_ERROR(-20999,'Se permitio postulacion duplicada');
  EXCEPTION WHEN OTHERS THEN IF SQLCODE<>-20007 THEN RAISE; END IF; END;
  pkg_becas.resolver(solicitud,'Aceptada');
  SELECT s.estado INTO estado FROM solicitudes s WHERE s.id_solicitud=solicitud;
  verificar(estado='Aceptada','No se resolvio la solicitud');
  BEGIN
    pkg_becas.resolver(solicitud,'Rechazada');
    RAISE_APPLICATION_ERROR(-20999,'Se modifico una resolucion final');
  EXCEPTION WHEN OTHERS THEN IF SQLCODE<>-20011 THEN RAISE; END IF; END;
  pkg_becas.cerrar_oferta(oferta);
  SELECT o.estado_oferta INTO estado FROM ofertas o WHERE o.id_oferta=oferta;
  verificar(estado='Cerrada','No se cerro la oferta');
  BEGIN
    pkg_becas.postular(doc,oferta,'Oferta cerrada');
    RAISE_APPLICATION_ERROR(-20999,'Se permitio postular a oferta cerrada');
  EXCEPTION WHEN OTHERS THEN IF SQLCODE<>-20006 THEN RAISE; END IF; END;
  -- Mas ofertas temporales para comprobar el limite de tres.
  FOR i IN 1..3 LOOP
    INSERT INTO ofertas VALUES(t_oferta(oferta-i,NULL,SYSDATE-1,SYSDATE+1,'Prueba limite','Vigente'));
  END LOOP;
  pkg_becas.postular(doc,oferta-1,'Segunda');
  pkg_becas.postular(doc,oferta-2,'Tercera');
  BEGIN
    pkg_becas.postular(doc,oferta-3,'Cuarta');
    RAISE_APPLICATION_ERROR(-20999,'Se permitio una cuarta postulacion');
  EXCEPTION WHEN OTHERS THEN IF SQLCODE<>-20008 THEN RAISE; END IF; END;
  ROLLBACK TO inicio_prueba;
  SELECT COUNT(*) INTO n FROM postulantes WHERE doc_identidad=doc;
  verificar(n=0,'ROLLBACK no revirtio el candidato');
  SELECT COUNT(*) INTO n FROM ofertas o WHERE o.id_oferta BETWEEN oferta-3 AND oferta;
  verificar(n=0,'ROLLBACK no revirtio las ofertas');
  DBMS_OUTPUT.PUT_LINE('OK: cinco operaciones, seis casos de rechazo y rollback verificados.');
  ROLLBACK;
EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/
