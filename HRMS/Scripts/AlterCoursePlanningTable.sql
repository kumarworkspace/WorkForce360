-- =============================================
-- Script: Add Missing Columns to CoursePlanning Table
-- Description: Adds QRCodePath and UploadFilePaths columns
-- =============================================

-- Check if QRCodePath column exists, add if not
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'CoursePlanning'
    AND COLUMN_NAME = 'QRCodePath'
)
BEGIN
    ALTER TABLE [dbo].[CoursePlanning]
    ADD QRCodePath NVARCHAR(500) NULL;

    PRINT 'Column QRCodePath added successfully';
END
ELSE
BEGIN
    PRINT 'Column QRCodePath already exists';
END
GO

-- Check if UploadFilePaths column exists, add if not
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'CoursePlanning'
    AND COLUMN_NAME = 'UploadFilePaths'
)
BEGIN
    ALTER TABLE [dbo].[CoursePlanning]
    ADD UploadFilePaths NVARCHAR(MAX) NULL;

    PRINT 'Column UploadFilePaths added successfully';
END
ELSE
BEGIN
    PRINT 'Column UploadFilePaths already exists';
END
GO

PRINT 'CoursePlanning table updated successfully';
