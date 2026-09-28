SET SERVEROUTPUT ON;

SAVEPOINT antes_de_pruebas;

-- Prueba 1: caso exitoso (oferta 101, fechas coherentes y futuras)
BEGIN
    sp_reprogramar_convocatoria(101, DATE '2027-05-01', DATE '2027-08-01');
    DBMS_OUTPUT.PUT_LINE('Prueba 1 OK: reprogramacion exitosa.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Prueba 1 FALLO (no debia fallar): ' || SQLERRM);
END;
/

-- Prueba 2: ventana no coherente (inicio despues del fin)
BEGIN
    sp_reprogramar_convocatoria(101, DATE '2027-08-01', DATE '2027-05-01');
    DBMS_OUTPUT.PUT_LINE('Prueba 2 FALLO: debia haber lanzado error -20303.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Prueba 2 OK, error esperado: ' || SQLERRM);
END;
/

-- Prueba 3: fecha de fin ya pasada
BEGIN
    sp_reprogramar_convocatoria(101, DATE '2020-01-01', DATE '2020-02-01');
    DBMS_OUTPUT.PUT_LINE('Prueba 3 FALLO: debia haber lanzado error -20304.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Prueba 3 OK, error esperado: ' || SQLERRM);
END;
/

-- Prueba 4: oferta que no existe
BEGIN
    sp_reprogramar_convocatoria(9999, DATE '2027-05-01', DATE '2027-08-01');
    DBMS_OUTPUT.PUT_LINE('Prueba 4 FALLO: debia haber lanzado error -20305.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Prueba 4 OK, error esperado: ' || SQLERRM);
END;
/

-- Prueba 5: oferta que ya no esta vigente
-- (cerramos la 102 solo dentro de esta prueba, se deshace al final)
BEGIN
    UPDATE ofertas SET estado_oferta = 'Cerrada' WHERE id_oferta = 102;

    sp_reprogramar_convocatoria(102, DATE '2027-05-01', DATE '2027-08-01');
    DBMS_OUTPUT.PUT_LINE('Prueba 5 FALLO: debia haber lanzado error -20306.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Prueba 5 OK, error esperado: ' || SQLERRM);
END;
/

-- Prueba 6: el TRIGGER protege incluso si alguien intenta un UPDATE directo,
-- sin pasar por el procedimiento (oferta 102 sigue "Cerrada" de la prueba anterior)
BEGIN
    UPDATE ofertas
       SET fecha_inicio = DATE '2027-05-01',
           fecha_fin    = DATE '2027-08-01'
     WHERE id_oferta = 102;
    DBMS_OUTPUT.PUT_LINE('Prueba 6 FALLO: el trigger debia haber bloqueado el UPDATE directo.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Prueba 6 OK, el trigger bloqueo el UPDATE directo: ' || SQLERRM);
END;
/

ROLLBACK TO antes_de_pruebas;