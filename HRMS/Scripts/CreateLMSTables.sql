-- =============================================
-- Script: Create LMS Tables and Stored Procedures
-- Description: Creates tables for Learning Management System
-- =============================================

-- Create LMSCourses table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LMSCourses' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LMSCourses] (
        [CourseId] INT IDENTITY(1,1) PRIMARY KEY,
        [Code] NVARCHAR(50) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX),
        [CourseCategoryId] INT,
        [SkillId] INT,
        [DurationHours] DECIMAL(5,2),
        [DifficultyLevel] NVARCHAR(20), -- Beginner, Intermediate, Advanced
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedBy] INT,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT,
        [UpdatedDate] DATETIME2,
        [TenantId] INT NOT NULL,
        CONSTRAINT [FK_LMSCourses_CourseCategory] FOREIGN KEY ([CourseCategoryId]) REFERENCES [MasterDropdown]([Id]),
        CONSTRAINT [FK_LMSCourses_Skill] FOREIGN KEY ([SkillId]) REFERENCES [LMSSkills]([SkillId]),
        CONSTRAINT [FK_LMSCourses_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [Tenant]([TenantId])
    );
    PRINT 'LMSCourses table created successfully';
END
ELSE
BEGIN
    PRINT 'LMSCourses table already exists';
END
GO

-- Create LMSModules table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LMSModules' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LMSModules] (
        [ModuleId] INT IDENTITY(1,1) PRIMARY KEY,
        [CourseId] INT NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX),
        [ContentType] NVARCHAR(20) NOT NULL, -- Video, PDF, Quiz, Document
        [ContentUrl] NVARCHAR(500),
        [VideoUrl] NVARCHAR(500),
        [DocumentPath] NVARCHAR(500),
        [QuizId] INT,
        [OrderIndex] INT NOT NULL DEFAULT 0,
        [DurationMinutes] INT,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedBy] INT,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT,
        [UpdatedDate] DATETIME2,
        [TenantId] INT NOT NULL,
        CONSTRAINT [FK_LMSModules_Course] FOREIGN KEY ([CourseId]) REFERENCES [LMSCourses]([CourseId]),
        CONSTRAINT [FK_LMSModules_Quiz] FOREIGN KEY ([QuizId]) REFERENCES [LMSQuizzes]([QuizId]),
        CONSTRAINT [FK_LMSModules_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [Tenant]([TenantId])
    );
    PRINT 'LMSModules table created successfully';
END
ELSE
BEGIN
    PRINT 'LMSModules table already exists';
END
GO

-- Create LMSEnrollments table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LMSEnrollments' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LMSEnrollments] (
        [EnrollmentId] INT IDENTITY(1,1) PRIMARY KEY,
        [CourseId] INT NOT NULL,
        [StaffId] INT NOT NULL,
        [EnrollmentDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CompletionDate] DATETIME2,
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'Enrolled', -- Enrolled, InProgress, Completed, Withdrawn
        [ProgressPercentage] DECIMAL(5,2) DEFAULT 0,
        [CertificateId] INT,
        [IsRecommended] BIT DEFAULT 0,
        [CreatedBy] INT,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT,
        [UpdatedDate] DATETIME2,
        [TenantId] INT NOT NULL,
        CONSTRAINT [FK_LMSEnrollments_Course] FOREIGN KEY ([CourseId]) REFERENCES [LMSCourses]([CourseId]),
        CONSTRAINT [FK_LMSEnrollments_Staff] FOREIGN KEY ([StaffId]) REFERENCES [Staff]([StaffId]),
        CONSTRAINT [FK_LMSEnrollments_Certificate] FOREIGN KEY ([CertificateId]) REFERENCES [LMSCertificates]([CertificateId]),
        CONSTRAINT [FK_LMSEnrollments_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [Tenant]([TenantId])
    );
    PRINT 'LMSEnrollments table created successfully';
END
ELSE
BEGIN
    PRINT 'LMSEnrollments table already exists';
END
GO

-- Create LMSProgress table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LMSProgress' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LMSProgress] (
        [ProgressId] INT IDENTITY(1,1) PRIMARY KEY,
        [EnrollmentId] INT NOT NULL,
        [ModuleId] INT NOT NULL,
        [IsCompleted] BIT NOT NULL DEFAULT 0,
        [CompletionDate] DATETIME2,
        [TimeSpentMinutes] INT DEFAULT 0,
        [LastAccessedDate] DATETIME2,
        [Score] DECIMAL(5,2),
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate] DATETIME2,
        [TenantId] INT NOT NULL,
        CONSTRAINT [FK_LMSProgress_Enrollment] FOREIGN KEY ([EnrollmentId]) REFERENCES [LMSEnrollments]([EnrollmentId]),
        CONSTRAINT [FK_LMSProgress_Module] FOREIGN KEY ([ModuleId]) REFERENCES [LMSModules]([ModuleId]),
        CONSTRAINT [FK_LMSProgress_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [Tenant]([TenantId])
    );
    PRINT 'LMSProgress table created successfully';
END
ELSE
BEGIN
    PRINT 'LMSProgress table already exists';
END
GO

-- Create LMSQuizzes table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LMSQuizzes' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LMSQuizzes] (
        [QuizId] INT IDENTITY(1,1) PRIMARY KEY,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX),
        [CourseId] INT,
        [ModuleId] INT,
        [PassingScore] DECIMAL(5,2) NOT NULL DEFAULT 60.00,
        [TotalQuestions] INT NOT NULL DEFAULT 0,
        [DurationMinutes] INT,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedBy] INT,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT,
        [UpdatedDate] DATETIME2,
        [TenantId] INT NOT NULL,
        CONSTRAINT [FK_LMSQuizzes_Course] FOREIGN KEY ([CourseId]) REFERENCES [LMSCourses]([CourseId]),
        CONSTRAINT [FK_LMSQuizzes_Module] FOREIGN KEY ([ModuleId]) REFERENCES [LMSModules]([ModuleId]),
        CONSTRAINT [FK_LMSQuizzes_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [Tenant]([TenantId])
    );
    PRINT 'LMSQuizzes table created successfully';
END
ELSE
BEGIN
    PRINT 'LMSQuizzes table already exists';
END
GO

-- Create LMSQuizQuestions table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LMSQuizQuestions' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LMSQuizQuestions] (
        [QuestionId] INT IDENTITY(1,1) PRIMARY KEY,
        [QuizId] INT NOT NULL,
        [QuestionText] NVARCHAR(MAX) NOT NULL,
        [QuestionType] NVARCHAR(20) NOT NULL, -- MultipleChoice, TrueFalse, ShortAnswer
        [Options] NVARCHAR(MAX), -- JSON array for multiple choice options
        [CorrectAnswer] NVARCHAR(MAX) NOT NULL,
        [Points] INT NOT NULL DEFAULT 1,
        [OrderIndex] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedBy] INT,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT,
        [UpdatedDate] DATETIME2,
        [TenantId] INT NOT NULL,
        CONSTRAINT [FK_LMSQuizQuestions_Quiz] FOREIGN KEY ([QuizId]) REFERENCES [LMSQuizzes]([QuizId]),
        CONSTRAINT [FK_LMSQuizQuestions_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [Tenant]([TenantId])
    );
    PRINT 'LMSQuizQuestions table created successfully';
END
ELSE
BEGIN
    PRINT 'LMSQuizQuestions table already exists';
END
GO

-- Create LMSQuizAttempts table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LMSQuizAttempts' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LMSQuizAttempts] (
        [AttemptId] INT IDENTITY(1,1) PRIMARY KEY,
        [EnrollmentId] INT NOT NULL,
        [QuizId] INT NOT NULL,
        [StartTime] DATETIME2 NOT NULL,
        [EndTime] DATETIME2,
        [Score] DECIMAL(5,2),
        [IsPassed] BIT,
        [Answers] NVARCHAR(MAX), -- JSON of answers
        [TimeSpentMinutes] INT,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [TenantId] INT NOT NULL,
        CONSTRAINT [FK_LMSQuizAttempts_Enrollment] FOREIGN KEY ([EnrollmentId]) REFERENCES [LMSEnrollments]([EnrollmentId]),
        CONSTRAINT [FK_LMSQuizAttempts_Quiz] FOREIGN KEY ([QuizId]) REFERENCES [LMSQuizzes]([QuizId]),
        CONSTRAINT [FK_LMSQuizAttempts_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [Tenant]([TenantId])
    );
    PRINT 'LMSQuizAttempts table created successfully';
END
ELSE
BEGIN
    PRINT 'LMSQuizAttempts table already exists';
END
GO

-- Create LMSCertificates table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LMSCertificates' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LMSCertificates] (
        [CertificateId] INT IDENTITY(1,1) PRIMARY KEY,
        [EnrollmentId] INT NOT NULL,
        [CertificateNumber] NVARCHAR(50) NOT NULL UNIQUE,
        [IssueDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ExpiryDate] DATETIME2,
        [CertificateUrl] NVARCHAR(500),
        [QRCode] NVARCHAR(MAX),
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedBy] INT,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT,
        [UpdatedDate] DATETIME2,
        [TenantId] INT NOT NULL,
        CONSTRAINT [FK_LMSCertificates_Enrollment] FOREIGN KEY ([EnrollmentId]) REFERENCES [LMSEnrollments]([EnrollmentId]),
        CONSTRAINT [FK_LMSCertificates_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [Tenant]([TenantId])
    );
    PRINT 'LMSCertificates table created successfully';
END
ELSE
BEGIN
    PRINT 'LMSCertificates table already exists';
END
GO

-- Create LMSSkills table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LMSSkills' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LMSSkills] (
        [SkillId] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(MAX),
        [Category] NVARCHAR(50),
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedBy] INT,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT,
        [UpdatedDate] DATETIME2,
        [TenantId] INT NOT NULL,
        CONSTRAINT [FK_LMSSkills_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [Tenant]([TenantId])
    );
    PRINT 'LMSSkills table created successfully';
END
ELSE
BEGIN
    PRINT 'LMSSkills table already exists';
END
GO

-- Create LMSEmployeeSkills table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LMSEmployeeSkills' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LMSEmployeeSkills] (
        [EmployeeSkillId] INT IDENTITY(1,1) PRIMARY KEY,
        [StaffId] INT NOT NULL,
        [SkillId] INT NOT NULL,
        [ProficiencyLevel] NVARCHAR(20), -- Beginner, Intermediate, Advanced, Expert
        [LastAssessedDate] DATETIME2,
        [NextAssessmentDate] DATETIME2,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedBy] INT,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT,
        [UpdatedDate] DATETIME2,
        [TenantId] INT NOT NULL,
        CONSTRAINT [FK_LMSEmployeeSkills_Staff] FOREIGN KEY ([StaffId]) REFERENCES [Staff]([StaffId]),
        CONSTRAINT [FK_LMSEmployeeSkills_Skill] FOREIGN KEY ([SkillId]) REFERENCES [LMSSkills]([SkillId]),
        CONSTRAINT [FK_LMSEmployeeSkills_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [Tenant]([TenantId])
    );
    PRINT 'LMSEmployeeSkills table created successfully';
END
ELSE
BEGIN
    PRINT 'LMSEmployeeSkills table already exists';
END
GO

-- Create LMSRecommendations table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LMSRecommendations' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LMSRecommendations] (
        [RecommendationId] INT IDENTITY(1,1) PRIMARY KEY,
        [StaffId] INT NOT NULL,
        [CourseId] INT NOT NULL,
        [RecommendationReason] NVARCHAR(MAX),
        [RecommendationScore] DECIMAL(5,2),
        [IsAccepted] BIT,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [TenantId] INT NOT NULL,
        CONSTRAINT [FK_LMSRecommendations_Staff] FOREIGN KEY ([StaffId]) REFERENCES [Staff]([StaffId]),
        CONSTRAINT [FK_LMSRecommendations_Course] FOREIGN KEY ([CourseId]) REFERENCES [LMSCourses]([CourseId]),
        CONSTRAINT [FK_LMSRecommendations_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [Tenant]([TenantId])
    );
    PRINT 'LMSRecommendations table created successfully';
END
ELSE
BEGIN
    PRINT 'LMSRecommendations table already exists';
END
GO

-- Create indexes for better performance
CREATE INDEX IX_LMSCourses_TenantId ON LMSCourses(TenantId);
CREATE INDEX IX_LMSCourses_IsActive ON LMSCourses(IsActive);
CREATE INDEX IX_LMSModules_CourseId ON LMSModules(CourseId);
CREATE INDEX IX_LMSEnrollments_StaffId ON LMSEnrollments(StaffId);
CREATE INDEX IX_LMSEnrollments_CourseId ON LMSEnrollments(CourseId);
CREATE INDEX IX_LMSProgress_EnrollmentId ON LMSProgress(EnrollmentId);
CREATE INDEX IX_LMSQuizAttempts_EnrollmentId ON LMSQuizAttempts(EnrollmentId);
CREATE INDEX IX_LMSCertificates_EnrollmentId ON LMSCertificates(EnrollmentId);
CREATE INDEX IX_LMSEmployeeSkills_StaffId ON LMSEmployeeSkills(StaffId);
CREATE INDEX IX_LMSRecommendations_StaffId ON LMSRecommendations(StaffId);

PRINT 'LMS tables and indexes created successfully';