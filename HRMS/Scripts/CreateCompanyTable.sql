-- Script to create or verify tbl_company table
-- Run this script in your database to ensure the table exists with correct structure

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_company')
BEGIN
    CREATE TABLE dbo.tbl_company
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Company NVARCHAR(150) NOT NULL,
        CompanyAddress NVARCHAR(250) NULL,
        Description NVARCHAR(500) NULL,
        Logo VARBINARY(MAX) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(100) NULL
    );

    PRINT 'Table tbl_company created successfully';
END
ELSE
BEGIN
    PRINT 'Table tbl_company already exists';
    
    -- Verify structure
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_company') AND name = 'Id' AND is_identity = 1)
    BEGIN
        PRINT 'WARNING: Id column is not IDENTITY. This may cause issues.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_company') AND name = 'Company')
    BEGIN
        PRINT 'WARNING: Company column is missing.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_company') AND name = 'CompanyAddress')
    BEGIN
        PRINT 'WARNING: CompanyAddress column is missing.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_company') AND name = 'Logo')
    BEGIN
        PRINT 'WARNING: Logo column is missing.';
    END
END

