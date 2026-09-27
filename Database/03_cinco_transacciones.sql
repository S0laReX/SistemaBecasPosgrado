-- Entrega: cinco transacciones PL/SQL independientes con COMMIT y ROLLBACK.
-- Ejecutar en una copia de practica. Crea datos DEMO persistentes.
-- Preparacion: elegir un documento NUEVO y una oferta REAL, vigente y dentro de plazo.
-- F5 en SQL Developer. El script pregunta los valores.
SET SERVEROUTPUT ON
SET DEFINE ON
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
ACCEPT documento CHAR PROMPT 'Documento nuevo para la demostracion: '
ACCEPT oferta NUMBER PROMPT 'ID de oferta de practica vigente (se cerrara al final): '
VARIABLE solicitud NUMBER

-- T1: Registrar candidato.
BEGIN
  pkg_becas.registrar_candidato('&documento','Estudiante','Demostracion','estudiante@example.test','Licenciatura');
  COMMIT;
  DBMS_OUTPUT.PUT_LINE('T1 confirmada: candidato registrado.');
EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/
-- T2: Insertar experiencia en una tabla anidada.
BEGIN
  pkg_becas.agregar_experiencia('&documento','Empresa de practica','Analista',DATE '2025-01-01',NULL);
  COMMIT;
  DBMS_OUTPUT.PUT_LINE('T2 confirmada: experiencia agregada.');
EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/
-- T3: Crear solicitud usando REF, validaciones y secuencia.
BEGIN
  pkg_becas.postular('&documento',&oferta,'Deseo profundizar mi formacion profesional.');
  SELECT s.id_solicitud INTO :solicitud FROM solicitudes s
    WHERE s.ref_postulante.doc_identidad='&documento' AND s.ref_oferta.id_oferta=&oferta;
  COMMIT;
  DBMS_OUTPUT.PUT_LINE('T3 confirmada: solicitud ' || :solicitud);
EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/
-- T4: Resolver solicitud bloqueando la fila durante la decision.
BEGIN
  pkg_becas.resolver(:solicitud,'Aceptada');
  COMMIT;
  DBMS_OUTPUT.PUT_LINE('T4 confirmada: solicitud aceptada.');
EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/
-- T5: Cerrar oferta sin alterar las solicitudes existentes.
BEGIN
  pkg_becas.cerrar_oferta(&oferta);
  COMMIT;
  DBMS_OUTPUT.PUT_LINE('T5 confirmada: oferta cerrada.');
EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/
SELECT s.id_solicitud,s.estado,s.ref_postulante.doc_identidad documento,s.ref_oferta.estado_oferta estado_oferta
FROM solicitudes s WHERE s.id_solicitud=:solicitud;
UNDEFINE documento
UNDEFINE oferta
