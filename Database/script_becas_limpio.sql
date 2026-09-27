-- BASE DE DATOS DE BECAS - EXPORT DEPURADO
-- Fuente: D:/Descargas/script_becas_full.sql
-- Ejecutar con F5 en SQL Developer en un ESQUEMA VACIO de practica.
-- No ejecutar encima de la base existente: crea tipos, tablas y datos.
-- Usa el usuario conectado y su tablespace predeterminado; no crea objetos internos.
-- El original se conserva como respaldo. No contiene DROP ni CREATE USER.
-- Se corrigieron terminadores, orden de dependencias, fechas y REF exportados como texto.
-- Las restricciones PK se declaran una sola vez; Oracle administra OID e indices internos.
-- Las rutinas antiguas compatibles conservan su logica, incluidos COMMIT y MAX+1.
-- Este export no contiene el paquete nuevo PKG_BECAS: se instala por separado
-- con el archivo 01_transacciones.sql del proyecto, despues de esta base.
SET DEFINE OFF
SET SERVEROUTPUT ON
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK

-- 1. TIPOS ACADEMICOS Y SUS COLECCIONES

-- T_ACADEMICO
CREATE OR REPLACE NONEDITIONABLE TYPE "T_ACADEMICO" AS OBJECT (
    nivel_formacion VARCHAR2(100),
    institucion_educativa VARCHAR2(150),
    carrera_especialidad VARCHAR2(100),
    grado_academico VARCHAR2(100),
    anio_egreso NUMBER(4),
    cursos_realizados VARCHAR2(500), -- Podría ser otra tabla anidada, pero lo mantenemos simple
    certificaciones VARCHAR2(500),
    idiomas VARCHAR2(200)
);
/

-- T_CURSO
CREATE OR REPLACE NONEDITIONABLE TYPE "T_CURSO" AS OBJECT (
    id_curso NUMBER,
    nombre_curso VARCHAR2(100),
    creditos NUMBER
);
/

-- T_DIRECCION
CREATE OR REPLACE NONEDITIONABLE TYPE "T_DIRECCION" AS OBJECT (
    pais VARCHAR2(50),
    departamento VARCHAR2(50),
    ciudad_municipio VARCHAR2(50),
    direccion_zona VARCHAR2(150)
);
/

-- T_EXPERIENCIA
CREATE OR REPLACE NONEDITIONABLE TYPE "T_EXPERIENCIA" AS OBJECT (
    empresa VARCHAR2(100),
    cargo VARCHAR2(100),
    fecha_inicio DATE,
    fecha_fin DATE
);
/

-- T_EXPLABORAL
CREATE OR REPLACE NONEDITIONABLE TYPE "T_EXPLABORAL" AS OBJECT (
    empresa VARCHAR2(100),
    cargo VARCHAR2(100),
    area_trabajo VARCHAR2(50),
    fecha_inicio DATE,
    fecha_fin DATE,
    funciones_realizadas VARCHAR2(255)
);
/

-- T_LABORAL
CREATE OR REPLACE NONEDITIONABLE TYPE "T_LABORAL" AS OBJECT (
    empresa VARCHAR2(150),
    cargo_ocupado VARCHAR2(100),
    area_trabajo VARCHAR2(100),
    fecha_inicio DATE,
    fecha_fin DATE,
    funciones_realizadas VARCHAR2(500),
    referencias_laborales VARCHAR2(500),
    habilidades_profesionales VARCHAR2(500)
);
/

-- T_LISTAEXPLABORAL
CREATE OR REPLACE NONEDITIONABLE TYPE "T_LISTAEXPLABORAL" AS TABLE OF T_ExpLaboral;
/

-- T_LISTA_ACADEMICOS
CREATE OR REPLACE NONEDITIONABLE TYPE "T_LISTA_ACADEMICOS" AS TABLE OF t_academico;
/

-- T_LISTA_CURSOS
CREATE OR REPLACE NONEDITIONABLE TYPE "T_LISTA_CURSOS" AS TABLE OF t_curso;
/

-- T_LISTA_LABORALES
CREATE OR REPLACE NONEDITIONABLE TYPE "T_LISTA_LABORALES" AS TABLE OF t_laboral;
/

-- T_NT_EXPERIENCIA
CREATE OR REPLACE NONEDITIONABLE TYPE "T_NT_EXPERIENCIA" AS TABLE OF t_experiencia;
/

-- T_UNIVERSIDAD
CREATE OR REPLACE NONEDITIONABLE TYPE "T_UNIVERSIDAD" AS OBJECT (
    id_universidad NUMBER,
    nombre VARCHAR2(150),
    pais VARCHAR2(50),
    ciudad VARCHAR2(50),
    direccion VARCHAR2(200),
    telefono VARCHAR2(20)
);
/

-- T_PROGRAMA
CREATE OR REPLACE NONEDITIONABLE TYPE "T_PROGRAMA" AS OBJECT (
    cod_programa VARCHAR2(20),
    nombre VARCHAR2(150),
    descripcion VARCHAR2(500),
    area VARCHAR2(100),
    tipo_programa VARCHAR2(50), -- Especialidad, Maestría, Doctorado
    modalidad VARCHAR2(50),
    ref_universidad REF t_universidad -- Relación usando punteros de objeto (REF)
);
/

-- T_OFERTA
CREATE OR REPLACE NONEDITIONABLE TYPE "T_OFERTA" AS OBJECT (
    id_oferta NUMBER,
    ref_programa REF t_programa,
    fecha_inicio DATE,
    fecha_fin DATE,
    tipo_financiamiento VARCHAR2(100),
    estado_oferta VARCHAR2(20) -- 'Vigente', 'Cerrada'
);
/

-- T_POSTULANTE
CREATE OR REPLACE NONEDITIONABLE TYPE "T_POSTULANTE" AS OBJECT (
    doc_identidad VARCHAR2(20),
    nombres VARCHAR2(100),
    apellidos VARCHAR2(100),
    correo VARCHAR2(100),
    nivel_formacion VARCHAR2(50),
    experiencias t_nt_experiencia -- Aquí anidamos el objeto anterior
);
/

-- T_SEDE
CREATE OR REPLACE NONEDITIONABLE TYPE "T_SEDE" AS OBJECT (
    cod_sede VARCHAR2(20),
    nombre VARCHAR2(150),
    tipo_sede VARCHAR2(50),
    responsable VARCHAR2(100),
    pais VARCHAR2(50),
    departamento VARCHAR2(50),
    municipio VARCHAR2(50),
    direccion VARCHAR2(200)
);
/

-- T_SOLICITUD
CREATE OR REPLACE NONEDITIONABLE TYPE "T_SOLICITUD" AS OBJECT (
    id_solicitud NUMBER,
    ref_postulante REF t_postulante,
    ref_oferta REF t_oferta,
    resumen_interes CLOB, -- Usamos CLOB porque el texto puede ser largo
    estado VARCHAR2(20) -- 'Pendiente', 'Aceptada', 'Rechazada'
);
/

-- 2. TABLAS OBJETO (SEIS TABLAS DEL DOMINIO)
CREATE TABLE UNIVERSIDADES OF T_UNIVERSIDAD (CONSTRAINT PK_UNIVERSIDADES PRIMARY KEY (ID_UNIVERSIDAD));

CREATE TABLE PROGRAMAS OF T_PROGRAMA (CONSTRAINT PK_PROGRAMAS PRIMARY KEY (COD_PROGRAMA));

CREATE TABLE OFERTAS OF T_OFERTA (CONSTRAINT PK_OFERTAS PRIMARY KEY (ID_OFERTA));

CREATE TABLE POSTULANTES OF T_POSTULANTE (CONSTRAINT PK_POSTULANTES PRIMARY KEY (DOC_IDENTIDAD))
NESTED TABLE EXPERIENCIAS STORE AS NT_EXPERIENCIAS_TAB RETURN AS VALUE;

CREATE TABLE SEDES OF T_SEDE (CONSTRAINT PK_SEDES PRIMARY KEY (COD_SEDE));

CREATE TABLE SOLICITUDES OF T_SOLICITUD (CONSTRAINT PK_SOLICITUDES PRIMARY KEY (ID_SOLICITUD));

-- 3. DATOS ORIGINALES, EN ORDEN DE DEPENDENCIAS

-- UNIVERSIDADES
INSERT INTO UNIVERSIDADES (ID_UNIVERSIDAD, NOMBRE, PAIS, CIUDAD, DIRECCION, TELEFONO)
VALUES (1, 'Universidad Mayor de San Simón', 'Bolivia', 'Cochabamba', 'Campus Central Las Cuadras', '4123456');
INSERT INTO UNIVERSIDADES (ID_UNIVERSIDAD, NOMBRE, PAIS, CIUDAD, DIRECCION, TELEFONO)
VALUES (2, 'Universidad Mayor de San Andrés', 'Bolivia', 'La Paz', 'Av. Villazón', '2123456');

-- PROGRAMAS
INSERT INTO PROGRAMAS (COD_PROGRAMA, NOMBRE, DESCRIPCION, AREA, TIPO_PROGRAMA, MODALIDAD, REF_UNIVERSIDAD)
VALUES ('PRG-001', 'Maestría en Ingeniería de Sistemas', 'Postgrado en arquitectura de software y BD', 'Tecnología', 'Maestría', 'Presencial', (SELECT REF(u) FROM UNIVERSIDADES u WHERE u.id_universidad=1));
INSERT INTO PROGRAMAS (COD_PROGRAMA, NOMBRE, DESCRIPCION, AREA, TIPO_PROGRAMA, MODALIDAD, REF_UNIVERSIDAD)
VALUES ('PRG-002', 'Especialidad en IoT y Hardware', 'Desarrollo con ESP32, sensores y protocolos', 'Tecnología', 'Especialidad', 'Virtual', (SELECT REF(u) FROM UNIVERSIDADES u WHERE u.id_universidad=1));

-- OFERTAS
INSERT INTO OFERTAS (ID_OFERTA, REF_PROGRAMA, FECHA_INICIO, FECHA_FIN, TIPO_FINANCIAMIENTO, ESTADO_OFERTA)
VALUES (101, (SELECT REF(p) FROM PROGRAMAS p WHERE p.cod_programa='PRG-001'), DATE '2027-02-01', DATE '2028-12-01', 'Autofinanciado', 'Vigente');
INSERT INTO OFERTAS (ID_OFERTA, REF_PROGRAMA, FECHA_INICIO, FECHA_FIN, TIPO_FINANCIAMIENTO, ESTADO_OFERTA)
VALUES (102, (SELECT REF(p) FROM PROGRAMAS p WHERE p.cod_programa='PRG-002'), DATE '2027-03-01', DATE '2027-10-01', 'Beca Completa', 'Vigente');

-- POSTULANTES
INSERT INTO POSTULANTES (DOC_IDENTIDAD, NOMBRES, APELLIDOS, CORREO, NIVEL_FORMACION, EXPERIENCIAS)
VALUES ('1234567', 'Carlos', 'Mendoza', 'carlos.m@email.com', 'Ingeniero de Sistemas', T_NT_EXPERIENCIA(T_EXPERIENCIA('Tech Solutions SRL', 'Desarrollador C# y WPF', DATE '2024-01-01', DATE '2025-12-31'), T_EXPERIENCIA('Proyecto de Transporte', 'Líder de Proyecto IoT', DATE '2026-01-01', NULL)));
INSERT INTO POSTULANTES (DOC_IDENTIDAD, NOMBRES, APELLIDOS, CORREO, NIVEL_FORMACION, EXPERIENCIAS)
VALUES ('7654321', 'Lucia', 'Villarroel', 'lucia.v@email.com', 'Ingeniera de Software', T_NT_EXPERIENCIA(T_EXPERIENCIA('Agencia Web', 'Programadora PHP y SQL', DATE '2023-03-15', DATE '2026-08-30')));

-- SEDES
INSERT INTO SEDES (COD_SEDE, NOMBRE, TIPO_SEDE, RESPONSABLE, PAIS, DEPARTAMENTO, MUNICIPIO, DIRECCION)
VALUES ('SED-CBB-01', 'Campus Central Las Cuadras', 'Principal', 'Ing. Roberto Mendoza', 'Bolivia', 'Cochabamba', 'Cochabamba', 'Av. Sucre y Parque La Torre');
INSERT INTO SEDES (COD_SEDE, NOMBRE, TIPO_SEDE, RESPONSABLE, PAIS, DEPARTAMENTO, MUNICIPIO, DIRECCION)
VALUES ('SED-CBB-02', 'Sede Tecnológica Quillacollo', 'Anexo', 'MSc. Patricia Vargas', 'Bolivia', 'Cochabamba', 'Quillacollo', 'Km 12 Carretera al Occidente');
INSERT INTO SEDES (COD_SEDE, NOMBRE, TIPO_SEDE, RESPONSABLE, PAIS, DEPARTAMENTO, MUNICIPIO, DIRECCION)
VALUES ('SED-LPZ-01', 'Edificio Central Postgrado', 'Principal', 'Dr. Fernando Alarcón', 'Bolivia', 'La Paz', 'La Paz', 'Av. 6 de Agosto esq. JJ Pérez');

-- SOLICITUDES
INSERT INTO SOLICITUDES (ID_SOLICITUD, REF_POSTULANTE, REF_OFERTA, ESTADO)
VALUES (1, (SELECT REF(p) FROM POSTULANTES p WHERE p.doc_identidad='1234567'), (SELECT REF(o) FROM OFERTAS o WHERE o.id_oferta=101), 'Aceptada');
INSERT INTO SOLICITUDES (ID_SOLICITUD, REF_POSTULANTE, REF_OFERTA, ESTADO)
VALUES (2, (SELECT REF(p) FROM POSTULANTES p WHERE p.doc_identidad='1234567'), (SELECT REF(o) FROM OFERTAS o WHERE o.id_oferta=102), 'Aceptada');
INSERT INTO SOLICITUDES (ID_SOLICITUD, REF_POSTULANTE, REF_OFERTA, ESTADO)
VALUES (3, (SELECT REF(p) FROM POSTULANTES p WHERE p.doc_identidad='7654321'), (SELECT REF(o) FROM OFERTAS o WHERE o.id_oferta=101), 'Pendiente');
INSERT INTO SOLICITUDES (ID_SOLICITUD, REF_POSTULANTE, REF_OFERTA, ESTADO)
VALUES (4, (SELECT REF(p) FROM POSTULANTES p WHERE p.doc_identidad='1234567'), (SELECT REF(o) FROM OFERTAS o WHERE o.id_oferta=101), 'Pendiente');

-- El export original no incluye RESUMEN_INTERES en sus INSERT de SOLICITUDES.
-- Se conserva esa ausencia como NULL; no se inventa texto ni se eliminan duplicados.
COMMIT;

-- 4. SECUENCIAS ACADEMICAS
CREATE SEQUENCE SEQ_OFERTA START WITH 103 INCREMENT BY 1 NOCYCLE CACHE 20;
CREATE SEQUENCE SEQ_POSTULACION START WITH 1 INCREMENT BY 1 NOCYCLE CACHE 20;
CREATE SEQUENCE SEQ_POSTULANTE START WITH 1 INCREMENT BY 1 NOCYCLE CACHE 20;
CREATE SEQUENCE SEQ_PROG START WITH 1 INCREMENT BY 1 NOCYCLE CACHE 20;
CREATE SEQUENCE SEQ_SEDE START WITH 1 INCREMENT BY 1 NOCYCLE CACHE 20;
CREATE SEQUENCE SEQ_UNIV START WITH 3 INCREMENT BY 1 NOCYCLE CACHE 20;

-- 5. PROCEDIMIENTOS Y FUNCIONES ORIGINALES COMPATIBLES

CREATE OR REPLACE NONEDITIONABLE PROCEDURE "SP_AGREGAR_EXPERIENCIA" (
    p_doc IN VARCHAR2, 
    p_empresa IN VARCHAR2, 
    p_cargo IN VARCHAR2, 
    p_inicio IN DATE, 
    p_fin IN DATE
) IS
BEGIN
    -- Usamos la función TABLE() para hacer el INSERT directamente en la colección del usuario
    INSERT INTO TABLE(SELECT p.experiencias FROM postulantes p WHERE p.doc_identidad = p_doc)
    VALUES (t_experiencia(p_empresa, p_cargo, p_inicio, p_fin));

    COMMIT;
END;
/
SHOW ERRORS PROCEDURE SP_AGREGAR_EXPERIENCIA

CREATE OR REPLACE NONEDITIONABLE PROCEDURE "SP_CREAR_POSTULANTE" (
    p_doc IN VARCHAR2, 
    p_nombres IN VARCHAR2, 
    p_apellidos IN VARCHAR2,
    p_correo IN VARCHAR2, 
    p_nivel IN VARCHAR2
) IS
BEGIN
    -- Nota: t_nt_experiencia() inicializa la tabla anidada para este usuario
    INSERT INTO postulantes VALUES (
        t_postulante(p_doc, p_nombres, p_apellidos, p_correo, p_nivel, t_nt_experiencia())
    );
    COMMIT;
END;
/
SHOW ERRORS PROCEDURE SP_CREAR_POSTULANTE

CREATE OR REPLACE NONEDITIONABLE FUNCTION "FN_ACEPTAR_SOLICITUD" (
    p_id_solicitud IN NUMBER
) RETURN VARCHAR2 IS
    v_estado_actual VARCHAR2(20);
BEGIN
    SELECT estado INTO v_estado_actual 
    FROM solicitudes 
    WHERE id_solicitud = p_id_solicitud 
    FOR UPDATE;

    IF v_estado_actual = 'Aceptada' THEN
        RETURN 'Aviso: La solicitud ya se encontraba aceptada previamente.';
    END IF;

    UPDATE solicitudes 
    SET estado = 'Aceptada' 
    WHERE id_solicitud = p_id_solicitud;

    COMMIT;
    RETURN 'Éxito: Solicitud ' || p_id_solicitud || ' aceptada correctamente.';
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN 'Error: No se encontró ninguna solicitud con el ID proporcionado.';
    WHEN OTHERS THEN
        ROLLBACK;
        RETURN 'Error inesperado: ' || SQLERRM;
END;
/
SHOW ERRORS FUNCTION FN_ACEPTAR_SOLICITUD

CREATE OR REPLACE NONEDITIONABLE FUNCTION "FN_CONTAR_ACEPTADOS" (
    p_id_oferta IN NUMBER
) RETURN NUMBER IS
    v_cantidad_aceptados NUMBER := 0;
    v_existe NUMBER;
BEGIN
    SELECT 1 INTO v_existe FROM ofertas WHERE id_oferta = p_id_oferta;

    SELECT COUNT(*) INTO v_cantidad_aceptados
    FROM solicitudes s
    WHERE s.ref_oferta.id_oferta = p_id_oferta
      AND s.estado = 'Aceptada';

    RETURN v_cantidad_aceptados;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20002, 'Error: La oferta indicada no existe.');
    WHEN OTHERS THEN
        RAISE;
END;
/
SHOW ERRORS FUNCTION FN_CONTAR_ACEPTADOS

CREATE OR REPLACE NONEDITIONABLE FUNCTION "FN_REGISTRAR_SOLICITUD" (
    p_doc_identidad IN VARCHAR2,
    p_id_oferta IN NUMBER,
    p_resumen IN CLOB
) RETURN VARCHAR2 IS
    v_ref_postulante REF t_postulante;
    v_ref_oferta REF t_oferta;
    v_conteo NUMBER;
    v_id_solicitud NUMBER;
BEGIN
    SELECT REF(p) INTO v_ref_postulante FROM postulantes p WHERE doc_identidad = p_doc_identidad;
    SELECT REF(o) INTO v_ref_oferta FROM ofertas o WHERE id_oferta = p_id_oferta;

    SELECT COUNT(*) INTO v_conteo 
    FROM solicitudes s 
    WHERE s.ref_postulante.doc_identidad = p_doc_identidad;

    IF v_conteo >= 3 THEN
        RAISE_APPLICATION_ERROR(-20001, 'El candidato ya alcanzó el límite máximo de 3 postulaciones.');
    END IF;

    SELECT NVL(MAX(id_solicitud), 0) + 1 INTO v_id_solicitud FROM solicitudes;

    INSERT INTO solicitudes VALUES (
        t_solicitud(v_id_solicitud, v_ref_postulante, v_ref_oferta, p_resumen, 'Pendiente')
    );
    COMMIT;
    RETURN 'Éxito: Solicitud ' || v_id_solicitud || ' registrada.';
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN 'Error: El documento de identidad o la oferta no existen en el sistema.';
    WHEN OTHERS THEN
        ROLLBACK;
        RETURN 'Error inesperado: ' || SQLERRM;
END;
/
SHOW ERRORS FUNCTION FN_REGISTRAR_SOLICITUD

-- Verificacion de compilacion de los objetos incluidos.
DECLARE v_errores NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_errores FROM user_errors WHERE attribute = 'ERROR'
    AND name IN ('T_ACADEMICO', 'T_CURSO', 'T_DIRECCION', 'T_EXPERIENCIA', 'T_EXPLABORAL', 'T_LABORAL', 'T_LISTAEXPLABORAL', 'T_LISTA_ACADEMICOS', 'T_LISTA_CURSOS', 'T_LISTA_LABORALES', 'T_NT_EXPERIENCIA', 'T_UNIVERSIDAD', 'T_PROGRAMA', 'T_OFERTA', 'T_POSTULANTE', 'T_SEDE', 'T_SOLICITUD', 'SP_AGREGAR_EXPERIENCIA', 'SP_CREAR_POSTULANTE', 'FN_ACEPTAR_SOLICITUD', 'FN_CONTAR_ACEPTADOS', 'FN_REGISTRAR_SOLICITUD');
  IF v_errores > 0 THEN RAISE_APPLICATION_ERROR(-20090, 'Existen errores de compilacion. Consultar USER_ERRORS.'); END IF;
END;
/

-- 6. ARCHIVO HISTORICO COMENTADO - NO SE EJECUTA
-- Estas funciones SON del proyecto, pero corresponden a otro modelo de datos.
-- FN_CREAR_OFERTA usa ID_PROGRAMA/ID_SEDE, inexistentes en la tabla objeto OFERTAS.
-- FN_REGISTRAR_POSTULACION usa POSTULACIONES, tabla que no existe en este export.
--
-- CREATE OR REPLACE NONEDITIONABLE FUNCTION "FN_CREAR_OFERTA" (
--     p_id_programa IN NUMBER,
--     p_id_sede IN NUMBER,
--     p_fecha_inicio IN DATE,
--     p_fecha_fin IN DATE,
--     p_financiamiento IN VARCHAR2
-- ) RETURN VARCHAR2 IS
--     e_fechas_invalidas EXCEPTION;
-- BEGIN
--     IF p_fecha_inicio >= p_fecha_fin THEN
--         RAISE e_fechas_invalidas;
--     END IF;
-- 
--     INSERT INTO Ofertas (id_programa, id_sede, fecha_inicio, fecha_fin, tipo_financiamiento)
--     VALUES (p_id_programa, p_id_sede, p_fecha_inicio, p_fecha_fin, p_financiamiento);
-- 
--     COMMIT;
--     RETURN 'Éxito: Oferta creada e insertada en planificación.';
-- EXCEPTION
--     WHEN e_fechas_invalidas THEN
--         ROLLBACK;
--         RETURN 'Error de validación: La fecha de inicio debe ser anterior a la de finalización.';
--     WHEN OTHERS THEN
--         ROLLBACK;
--         RETURN 'Error inesperado: ' || SQLERRM;
-- END;
--
-- CREATE OR REPLACE NONEDITIONABLE FUNCTION "FN_REGISTRAR_POSTULACION" (
--     p_id_postulante IN NUMBER,
--     p_id_oferta IN NUMBER,
--     p_resumen_interes IN CLOB
-- ) RETURN VARCHAR2 IS
--     v_cantidad NUMBER;
--     e_limite EXCEPTION;
-- BEGIN
--     SELECT COUNT(*) INTO v_cantidad
--     FROM Postulaciones
--     WHERE id_postulante = p_id_postulante;
-- 
--     IF v_cantidad >= 3 THEN
--         RAISE e_limite;
--     END IF;
-- 
--     INSERT INTO Postulaciones (id_postulante, id_oferta, resumen_interes)
--     VALUES (p_id_postulante, p_id_oferta, p_resumen_interes);
-- 
--     COMMIT;
--     RETURN 'Éxito: Postulación registrada correctamente.';
-- EXCEPTION
--     WHEN e_limite THEN
--         ROLLBACK;
--         RETURN 'Error: El candidato ya alcanzó el límite máximo de 3 programas.';
--     WHEN OTHERS THEN
--         ROLLBACK;
--         RETURN 'Error inesperado: ' || SQLERRM;
-- END;

-- Fin del export depurado.
