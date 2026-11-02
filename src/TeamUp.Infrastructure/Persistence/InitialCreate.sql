IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [Badge] (
        [BadgeId] int NOT NULL IDENTITY,
        [Name] nvarchar(80) NOT NULL,
        [Description] nvarchar(250) NOT NULL,
        [IconUrl] nvarchar(300) NOT NULL,
        [CriteriaType] varchar(30) NOT NULL,
        [CriteriaValue] int NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_Badge] PRIMARY KEY ([BadgeId]),
        CONSTRAINT [CK_Badge_CriteriaType] CHECK ([CriteriaType] IN ('PointsThreshold', 'EventCount', 'TaskCount'))
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [Organization] (
        [OrganizationId] int NOT NULL IDENTITY,
        [Name] nvarchar(120) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Category] nvarchar(50) NOT NULL,
        [LogoUrl] nvarchar(300) NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_Organization] PRIMARY KEY ([OrganizationId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [Skill] (
        [SkillId] int NOT NULL IDENTITY,
        [Name] nvarchar(60) NOT NULL,
        [Category] nvarchar(50) NOT NULL,
        [Description] nvarchar(250) NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_Skill] PRIMARY KEY ([SkillId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [University] (
        [UniversityId] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [City] nvarchar(100) NOT NULL,
        [Country] nvarchar(100) NOT NULL,
        [Website] nvarchar(200) NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_University] PRIMARY KEY ([UniversityId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [Event] (
        [EventId] int NOT NULL IDENTITY,
        [Title] nvarchar(150) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Type] varchar(30) NOT NULL,
        [Location] nvarchar(200) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [Capacity] int NOT NULL,
        [OrganizationId] int NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_Event] PRIMARY KEY ([EventId]),
        CONSTRAINT [CK_Event_Capacity] CHECK ([Capacity] >= 1),
        CONSTRAINT [CK_Event_Dates] CHECK ([EndDate] >= [StartDate]),
        CONSTRAINT [CK_Event_Type] CHECK ([Type] IN ('Workshop', 'Hackathon', 'Session', 'Competition')),
        CONSTRAINT [FK_Event_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organization] ([OrganizationId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [Faculty] (
        [FacultyId] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [UniversityId] int NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_Faculty] PRIMARY KEY ([FacultyId]),
        CONSTRAINT [FK_Faculty_University_UniversityId] FOREIGN KEY ([UniversityId]) REFERENCES [University] ([UniversityId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [EventSession] (
        [SessionId] int NOT NULL IDENTITY,
        [EventId] int NOT NULL,
        [SessionNo] int NOT NULL,
        [Title] nvarchar(120) NOT NULL,
        [StartTime] datetime2 NOT NULL,
        [EndTime] datetime2 NOT NULL,
        [Room] nvarchar(60) NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_EventSession] PRIMARY KEY ([SessionId]),
        CONSTRAINT [CK_EventSession_Times] CHECK ([EndTime] >= [StartTime]),
        CONSTRAINT [FK_EventSession_Event_EventId] FOREIGN KEY ([EventId]) REFERENCES [Event] ([EventId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [Student] (
        [StudentId] int NOT NULL IDENTITY,
        [Email] nvarchar(150) NOT NULL,
        [PasswordHash] nvarchar(256) NOT NULL,
        [FirstName] nvarchar(50) NOT NULL,
        [LastName] nvarchar(50) NOT NULL,
        [AcademicYear] int NOT NULL,
        [Role] varchar(30) NOT NULL DEFAULT 'Student',
        [FacultyId] int NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_Student] PRIMARY KEY ([StudentId]),
        CONSTRAINT [CK_Student_AcademicYear] CHECK ([AcademicYear] BETWEEN 1 AND 7),
        CONSTRAINT [CK_Student_Role] CHECK ([Role] IN ('Student', 'OrgAdmin', 'PlatformAdmin')),
        CONSTRAINT [FK_Student_Faculty_FacultyId] FOREIGN KEY ([FacultyId]) REFERENCES [Faculty] ([FacultyId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [EventRegistration] (
        [EventId] int NOT NULL,
        [StudentId] int NOT NULL,
        [RegisteredAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [Status] varchar(30) NOT NULL DEFAULT 'Confirmed',
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_EventRegistration] PRIMARY KEY ([EventId], [StudentId]),
        CONSTRAINT [CK_EventRegistration_Status] CHECK ([Status] IN ('Confirmed', 'Waitlisted', 'Cancelled')),
        CONSTRAINT [FK_EventRegistration_Event_EventId] FOREIGN KEY ([EventId]) REFERENCES [Event] ([EventId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EventRegistration_Student_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [MembershipApplication] (
        [ApplicationId] int NOT NULL IDENTITY,
        [OrganizationId] int NOT NULL,
        [StudentId] int NOT NULL,
        [Message] nvarchar(600) NULL,
        [Status] varchar(30) NOT NULL DEFAULT 'Pending',
        [AppliedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [ReviewedAt] datetime2 NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_MembershipApplication] PRIMARY KEY ([ApplicationId]),
        CONSTRAINT [CK_MembershipApplication_Status] CHECK ([Status] IN ('Pending', 'Accepted', 'Rejected', 'Withdrawn')),
        CONSTRAINT [FK_MembershipApplication_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organization] ([OrganizationId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MembershipApplication_Student_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [Notification] (
        [NotificationId] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [Title] nvarchar(120) NOT NULL,
        [Message] nvarchar(500) NOT NULL,
        [Type] varchar(30) NOT NULL,
        [IsRead] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_Notification] PRIMARY KEY ([NotificationId]),
        CONSTRAINT [CK_Notification_Type] CHECK ([Type] IN ('ApplicationStatus', 'TaskAssigned', 'BadgeEarned', 'EventReminder')),
        CONSTRAINT [FK_Notification_Student_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [OrganizationMember] (
        [OrganizationId] int NOT NULL,
        [StudentId] int NOT NULL,
        [Role] nvarchar(60) NOT NULL,
        [JoinedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_OrganizationMember] PRIMARY KEY ([OrganizationId], [StudentId]),
        CONSTRAINT [FK_OrganizationMember_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organization] ([OrganizationId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrganizationMember_Student_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [Project] (
        [ProjectId] int NOT NULL IDENTITY,
        [Title] nvarchar(150) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Status] varchar(30) NOT NULL DEFAULT 'Recruiting',
        [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [MaxTeamSize] int NOT NULL,
        [CreatorId] int NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_Project] PRIMARY KEY ([ProjectId]),
        CONSTRAINT [CK_Project_MaxTeamSize] CHECK ([MaxTeamSize] >= 1),
        CONSTRAINT [CK_Project_Status] CHECK ([Status] IN ('Recruiting', 'InProgress', 'Completed', 'Archived')),
        CONSTRAINT [FK_Project_Student_CreatorId] FOREIGN KEY ([CreatorId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [ScoreTransaction] (
        [TransactionId] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [Points] int NOT NULL,
        [Reason] nvarchar(200) NOT NULL,
        [SourceType] varchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_ScoreTransaction] PRIMARY KEY ([TransactionId]),
        CONSTRAINT [CK_ScoreTransaction_SourceType] CHECK ([SourceType] IN ('EventAttendance', 'TaskCompleted', 'ProjectDelivered', 'Manual')),
        CONSTRAINT [FK_ScoreTransaction_Student_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [SessionAttendance] (
        [SessionId] int NOT NULL,
        [StudentId] int NOT NULL,
        [CheckInTime] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [VerificationMethod] varchar(30) NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_SessionAttendance] PRIMARY KEY ([SessionId], [StudentId]),
        CONSTRAINT [CK_SessionAttendance_VerificationMethod] CHECK ([VerificationMethod] IN ('QRCode', 'RFID', 'ManualByOrganizer')),
        CONSTRAINT [FK_SessionAttendance_EventSession_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [EventSession] ([SessionId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SessionAttendance_Student_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [StudentBadge] (
        [StudentId] int NOT NULL,
        [BadgeId] int NOT NULL,
        [AwardedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_StudentBadge] PRIMARY KEY ([StudentId], [BadgeId]),
        CONSTRAINT [FK_StudentBadge_Badge_BadgeId] FOREIGN KEY ([BadgeId]) REFERENCES [Badge] ([BadgeId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StudentBadge_Student_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [StudentInterest] (
        [StudentId] int NOT NULL,
        [Interest] nvarchar(50) NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_StudentInterest] PRIMARY KEY ([StudentId], [Interest]),
        CONSTRAINT [FK_StudentInterest_Student_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [StudentProfile] (
        [ProfileId] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [Bio] nvarchar(1000) NULL,
        [PhotoUrl] nvarchar(300) NULL,
        [GitHubUrl] nvarchar(200) NULL,
        [LinkedInUrl] nvarchar(200) NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_StudentProfile] PRIMARY KEY ([ProfileId]),
        CONSTRAINT [FK_StudentProfile_Student_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [StudentSkill] (
        [StudentId] int NOT NULL,
        [SkillId] int NOT NULL,
        [ProficiencyLevel] varchar(30) NOT NULL,
        [YearsOfExperience] decimal(3,1) NOT NULL DEFAULT 0.0,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_StudentSkill] PRIMARY KEY ([StudentId], [SkillId]),
        CONSTRAINT [CK_StudentSkill_ProficiencyLevel] CHECK ([ProficiencyLevel] IN ('Beginner', 'Intermediate', 'Advanced')),
        CONSTRAINT [CK_StudentSkill_Years] CHECK ([YearsOfExperience] >= 0),
        CONSTRAINT [FK_StudentSkill_Skill_SkillId] FOREIGN KEY ([SkillId]) REFERENCES [Skill] ([SkillId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StudentSkill_Student_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [ProjectApplication] (
        [ApplicationId] int NOT NULL IDENTITY,
        [ProjectId] int NOT NULL,
        [StudentId] int NOT NULL,
        [Message] nvarchar(500) NULL,
        [Status] varchar(30) NOT NULL DEFAULT 'Pending',
        [AppliedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_ProjectApplication] PRIMARY KEY ([ApplicationId]),
        CONSTRAINT [CK_ProjectApplication_Status] CHECK ([Status] IN ('Pending', 'Accepted', 'Rejected', 'Withdrawn')),
        CONSTRAINT [FK_ProjectApplication_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([ProjectId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProjectApplication_Student_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [ProjectMember] (
        [ProjectId] int NOT NULL,
        [StudentId] int NOT NULL,
        [Role] nvarchar(60) NOT NULL,
        [JoinedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_ProjectMember] PRIMARY KEY ([ProjectId], [StudentId]),
        CONSTRAINT [FK_ProjectMember_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([ProjectId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProjectMember_Student_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [ProjectRequiredSkill] (
        [ProjectId] int NOT NULL,
        [SkillId] int NOT NULL,
        [RequiredLevel] varchar(30) NOT NULL,
        [IsMandatory] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_ProjectRequiredSkill] PRIMARY KEY ([ProjectId], [SkillId]),
        CONSTRAINT [CK_ProjectRequiredSkill_RequiredLevel] CHECK ([RequiredLevel] IN ('Beginner', 'Intermediate', 'Advanced')),
        CONSTRAINT [FK_ProjectRequiredSkill_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([ProjectId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProjectRequiredSkill_Skill_SkillId] FOREIGN KEY ([SkillId]) REFERENCES [Skill] ([SkillId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE TABLE [ProjectTask] (
        [TaskId] int NOT NULL IDENTITY,
        [Title] nvarchar(150) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Status] varchar(30) NOT NULL DEFAULT 'Todo',
        [DueDate] datetime2 NULL,
        [Priority] varchar(30) NOT NULL,
        [ProjectId] int NOT NULL,
        [AssignedStudentId] int NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_ProjectTask] PRIMARY KEY ([TaskId]),
        CONSTRAINT [CK_ProjectTask_Priority] CHECK ([Priority] IN ('Low', 'Medium', 'High', 'Urgent')),
        CONSTRAINT [CK_ProjectTask_Status] CHECK ([Status] IN ('Todo', 'InProgress', 'InReview', 'Done')),
        CONSTRAINT [FK_ProjectTask_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([ProjectId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProjectTask_Student_AssignedStudentId] FOREIGN KEY ([AssignedStudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Badge_Name] ON [Badge] ([Name]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Event_OrganizationId] ON [Event] ([OrganizationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Event_StartDate] ON [Event] ([StartDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EventRegistration_StudentId] ON [EventRegistration] ([StudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_EventSession_EventId_SessionNo] ON [EventSession] ([EventId], [SessionNo]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Faculty_UniversityId_Code] ON [Faculty] ([UniversityId], [Code]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_MembershipApplication_OrganizationId_StudentId] ON [MembershipApplication] ([OrganizationId], [StudentId]) WHERE [DeletedAt] IS NULL AND [Status] = ''Pending''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MembershipApplication_StudentId] ON [MembershipApplication] ([StudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notification_StudentId_IsRead_CreatedAt] ON [Notification] ([StudentId], [IsRead], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Organization_Name] ON [Organization] ([Name]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrganizationMember_StudentId] ON [OrganizationMember] ([StudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Project_CreatorId] ON [Project] ([CreatorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ProjectApplication_ProjectId_StudentId] ON [ProjectApplication] ([ProjectId], [StudentId]) WHERE [DeletedAt] IS NULL AND [Status] = ''Pending''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProjectApplication_StudentId] ON [ProjectApplication] ([StudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProjectMember_StudentId] ON [ProjectMember] ([StudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProjectRequiredSkill_SkillId] ON [ProjectRequiredSkill] ([SkillId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProjectTask_AssignedStudentId] ON [ProjectTask] ([AssignedStudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProjectTask_ProjectId] ON [ProjectTask] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ScoreTransaction_StudentId_CreatedAt] ON [ScoreTransaction] ([StudentId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SessionAttendance_StudentId] ON [SessionAttendance] ([StudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Skill_Name] ON [Skill] ([Name]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Student_Email] ON [Student] ([Email]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Student_FacultyId] ON [Student] ([FacultyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StudentBadge_BadgeId] ON [StudentBadge] ([BadgeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_StudentProfile_StudentId] ON [StudentProfile] ([StudentId]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StudentSkill_SkillId] ON [StudentSkill] ([SkillId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_University_Name] ON [University] ([Name]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102213307_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261102213307_InitialCreate', N'10.0.14');
END;

COMMIT;
GO

