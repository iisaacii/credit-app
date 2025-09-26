-- === Crear/usar BD ===
IF DB_ID('CreditDB') IS NULL
BEGIN
  CREATE DATABASE CreditDB;
END
GO
USE CreditDB;
GO

-- === Tablas ===
IF OBJECT_ID('dbo.Branches') IS NULL
BEGIN
  CREATE TABLE dbo.Branches (
    BranchId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Address NVARCHAR(255) NULL
  );
END

IF OBJECT_ID('dbo.Clients') IS NULL
BEGIN
  CREATE TABLE dbo.Clients (
    ClientId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Clients_CreatedAt DEFAULT (SYSUTCDATETIME())
  );
END

IF OBJECT_ID('dbo.CreditRequests') IS NULL
BEGIN
  CREATE TABLE dbo.CreditRequests (
    CreditRequestId INT IDENTITY(1,1) PRIMARY KEY,
    ClientId INT NOT NULL,
    BranchId INT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    TermMonths INT NOT NULL,
    Income DECIMAL(18,2) NULL,
    EmploymentMonths INT NULL,
    RequestedAt DATETIME2 NOT NULL CONSTRAINT DF_CreditRequests_RequestedAt DEFAULT (SYSUTCDATETIME()),
    Status NVARCHAR(20) NOT NULL,
    Score DECIMAL(5,2) NULL,
    CONSTRAINT FK_CreditRequests_Clients FOREIGN KEY (ClientId) REFERENCES dbo.Clients(ClientId),
    CONSTRAINT FK_CreditRequests_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(BranchId)
  );
END
GO

-- === Índices útiles (opcionales) ===
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CreditRequests_Status' AND object_id = OBJECT_ID('dbo.CreditRequests'))
  CREATE INDEX IX_CreditRequests_Status ON dbo.CreditRequests(Status);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CreditRequests_RequestedAt' AND object_id = OBJECT_ID('dbo.CreditRequests'))
  CREATE INDEX IX_CreditRequests_RequestedAt ON dbo.CreditRequests(RequestedAt DESC);
GO

-- === Seed de sucursales ===
IF NOT EXISTS (SELECT 1 FROM dbo.Branches)
BEGIN
  INSERT INTO dbo.Branches(Name, Address)
  VALUES (N'Sucursal Norte', N'Av. Norte 123'),
         (N'Sucursal Sur',   N'Av. Sur 456'),
         (N'Sucursal Centro',N'Av. Centro 789');
END
GO

-- === Stored Procedure: inserta solicitud y calcula score/status ===
IF OBJECT_ID('dbo.sp_InsertCreditRequest') IS NOT NULL
  DROP PROCEDURE dbo.sp_InsertCreditRequest;
GO
CREATE PROCEDURE dbo.sp_InsertCreditRequest
  @ClientId INT,
  @BranchId INT = NULL,
  @Amount DECIMAL(18,2),
  @TermMonths INT,
  @Income DECIMAL(18,2) = NULL,
  @EmploymentMonths INT = NULL,
  @OutStatus NVARCHAR(20) OUTPUT,
  @OutScore DECIMAL(5,2) OUTPUT
AS
BEGIN
  SET NOCOUNT ON;

  DECLARE @score DECIMAL(5,2) = 0;

  -- Reglas de scoring (mismas que tu API)
  IF @Income IS NOT NULL
    SET @score += CASE WHEN @Income >= 30000 THEN 50
                       WHEN @Income >= 15000 THEN 30
                       WHEN @Income > 0     THEN 10
                       ELSE 0 END;

  IF @EmploymentMonths IS NOT NULL
    SET @score += CASE
