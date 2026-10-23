-- ==========================================================================================
-- ARCHIVO: Script_BaseDatos.sql
-- PROYECTO: e-Solutions - Evaluación Técnica Desarrollador .NET
-- AUTOR: Jonathan
-- DESCRIPCIÓN: Creación de base de datos, tabla de configuración de canales, procedimiento
--              almacenado y datos iniciales para canales AB, CJ y canales de prueba.
-- ==========================================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'eSolutionsPagos')
BEGIN
    CREATE DATABASE eSolutionsPagos;
END
GO

USE eSolutionsPagos;
GO

-- 1. Tabla de Configuración de Canales
IF OBJECT_ID(N'dbo.CanalConfiguracion', N'U') IS NOT NULL
    DROP TABLE dbo.CanalConfiguracion;
GO

CREATE TABLE dbo.CanalConfiguracion (
    CanalCodigo       VARCHAR(10)    NOT NULL,
    CanalNombre       VARCHAR(100)   NOT NULL,
    ServicioUrl       VARCHAR(255)   NOT NULL,
    ApiKey            VARCHAR(100)   NOT NULL,
    MuestraMenu       BIT            NOT NULL DEFAULT 1,
    PaginaDestino     VARCHAR(150)   NOT NULL,
    TimeoutSegundos   INT            NOT NULL DEFAULT 10,
    Activo            BIT            NOT NULL DEFAULT 1,
    FechaCreacion     DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_CanalConfiguracion PRIMARY KEY CLUSTERED (CanalCodigo)
);
GO

-- 2. Procedimiento Almacenado para Consultar Configuración por Canal
IF OBJECT_ID(N'dbo.sp_ObtenerConfiguracionCanal', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ObtenerConfiguracionCanal;
GO

CREATE PROCEDURE dbo.sp_ObtenerConfiguracionCanal
    @CanalCodigo VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        CanalCodigo,
        CanalNombre,
        ServicioUrl,
        ApiKey,
        MuestraMenu,
        PaginaDestino,
        TimeoutSegundos,
        Activo
    FROM dbo.CanalConfiguracion
    WHERE CanalCodigo = @CanalCodigo
      AND Activo = 1;
END
GO

-- 3. Inserción de Datos Iniciales para Canales AB, CJ y canales de prueba
-- Nota: La URL del servicio puede ser una URL absoluta (ej: http://localhost:PORT/api/empresas) 
-- o relativa al mismo host (ej: ~/SimuladorEmpresas.ashx?canal=AB).
-- El servicio simulado integrado en la solución responde tanto en ashx como en endpoint simulado.

INSERT INTO dbo.CanalConfiguracion 
    (CanalCodigo, CanalNombre, ServicioUrl, ApiKey, MuestraMenu, PaginaDestino, TimeoutSegundos, Activo)
VALUES
    -- Canal AB: Menú activo (1), redirige a PagoEmpresa.aspx
    ('AB', 'Canal Autoservicio Bancario (AB)', '~/SimuladorEmpresas.ashx?canal=AB', 'KEY-SEC-AB-2026', 1, 'PagoEmpresa.aspx', 10, 1),
    
    -- Canal CJ: Menú desactivado (0), redirige a DetalleEmpresa.aspx
    ('CJ', 'Canal Cajas y Ventanilla (CJ)', '~/SimuladorEmpresas.ashx?canal=CJ', 'KEY-SEC-CJ-2026', 0, 'DetalleEmpresa.aspx', 8, 1),
    
    -- Canal de prueba: Simula API Key inválida (debe arrojar 401 controlado)
    ('TEST401', 'Canal Prueba Llave Inválida', '~/SimuladorEmpresas.ashx?canal=AB', 'LLAVE-INVALIDA-XYZ', 1, 'PagoEmpresa.aspx', 5, 1),

    -- Canal de prueba: Simula Timeout (demora de 10s con timeout configurado a 2s)
    ('TESTTO', 'Canal Prueba Timeout Excedido', '~/SimuladorEmpresas.ashx?canal=AB&delay=10000', 'KEY-SEC-AB-2026', 1, 'PagoEmpresa.aspx', 2, 1);
GO

-- ==========================================================================================
-- EJEMPLO DE CÓMO AGREGAR UN NUEVO CANAL SIN TOCAR CÓDIGO NI RECOMPILAR:
-- ==========================================================================================
/*
INSERT INTO dbo.CanalConfiguracion 
    (CanalCodigo, CanalNombre, ServicioUrl, ApiKey, MuestraMenu, PaginaDestino, TimeoutSegundos, Activo)
VALUES
    ('WEB', 'Canal Web Portal Clientes', '~/SimuladorEmpresas.ashx?canal=WEB', 'KEY-SEC-WEB-2026', 1, 'PagoEmpresa.aspx', 15, 1);
*/

PRINT 'Base de datos eSolutionsPagos y objetos configurados exitosamente.';
GO
