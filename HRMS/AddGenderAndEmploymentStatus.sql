-- Add Gender and EmploymentStatus columns to Staff table if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Staff]') AND name = 'Gender')
BEGIN
    ALTER TABLE [dbo].[Staff]
    ADD [Gender] [nvarchar](20) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Staff]') AND name = 'EmploymentStatus')
BEGIN
    ALTER TABLE [dbo].[Staff]
    ADD [EmploymentStatus] [nvarchar](50) NULL;
END
GO






