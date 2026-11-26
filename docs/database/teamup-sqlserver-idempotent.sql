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
    VALUES (N'20261102213307_InitialCreate', N'11.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102223340_AddStudentRefreshToken'
)
BEGIN
    ALTER TABLE [Student] ADD [RefreshToken] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102223340_AddStudentRefreshToken'
)
BEGIN
    ALTER TABLE [Student] ADD [RefreshTokenExpiresAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102223340_AddStudentRefreshToken'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261102223340_AddStudentRefreshToken', N'11.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103165211_AddPasswordResetToken'
)
BEGIN
    ALTER TABLE [Student] ADD [PasswordResetToken] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103165211_AddPasswordResetToken'
)
BEGIN
    ALTER TABLE [Student] ADD [PasswordResetTokenExpiresAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103165211_AddPasswordResetToken'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261103165211_AddPasswordResetToken', N'11.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104180000_AddActivityQueryIndexes'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Organization_Category_Name] ON [Organization] ([Category], [Name]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104180000_AddActivityQueryIndexes'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_OrganizationMember_OrganizationId_Role_JoinedAt] ON [OrganizationMember] ([OrganizationId], [Role], [JoinedAt]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104180000_AddActivityQueryIndexes'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_MembershipApplication_OrganizationId_AppliedAt] ON [MembershipApplication] ([OrganizationId], [AppliedAt]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104180000_AddActivityQueryIndexes'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Event_StartDate_Title] ON [Event] ([StartDate], [Title]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104180000_AddActivityQueryIndexes'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Event_OrganizationId_StartDate] ON [Event] ([OrganizationId], [StartDate]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104180000_AddActivityQueryIndexes'
)
BEGIN
    CREATE INDEX [IX_EventRegistration_EventId_Status_RegisteredAt] ON [EventRegistration] ([EventId], [Status], [RegisteredAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104180000_AddActivityQueryIndexes'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_SessionAttendance_SessionId_CheckInTime] ON [SessionAttendance] ([SessionId], [CheckInTime]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104180000_AddActivityQueryIndexes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261104180000_AddActivityQueryIndexes', N'11.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121203306_AddStrideSecurityHardening'
)
BEGIN
    ALTER TABLE [Student] ADD [SecurityStamp] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121203306_AddStrideSecurityHardening'
)
BEGIN
    ALTER TABLE [Student] ADD [ShowEmailInDirectory] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121203306_AddStrideSecurityHardening'
)
BEGIN
    ALTER TABLE [SessionAttendance] ADD [RecordedByStudentId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121203306_AddStrideSecurityHardening'
)
BEGIN
    ALTER TABLE [Project] ADD [RowVersion] rowversion NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121203306_AddStrideSecurityHardening'
)
BEGIN
    ALTER TABLE [Event] ADD [RowVersion] rowversion NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121203306_AddStrideSecurityHardening'
)
BEGIN
    CREATE TABLE [AuditLog] (
        [AuditLogId] int NOT NULL IDENTITY,
        [ActorStudentId] int NULL,
        [ActorEmail] nvarchar(150) NULL,
        [Action] nvarchar(80) NOT NULL,
        [EntityType] nvarchar(60) NOT NULL,
        [EntityId] nvarchar(64) NOT NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [ClientIp] nvarchar(64) NULL,
        [OccurredAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_AuditLog] PRIMARY KEY ([AuditLogId]),
        CONSTRAINT [FK_AuditLog_Student_ActorStudentId] FOREIGN KEY ([ActorStudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121203306_AddStrideSecurityHardening'
)
BEGIN
    CREATE INDEX [IX_SessionAttendance_RecordedByStudentId] ON [SessionAttendance] ([RecordedByStudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121203306_AddStrideSecurityHardening'
)
BEGIN
    CREATE INDEX [IX_AuditLog_ActorStudentId] ON [AuditLog] ([ActorStudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121203306_AddStrideSecurityHardening'
)
BEGIN
    CREATE INDEX [IX_AuditLog_EntityType_EntityId] ON [AuditLog] ([EntityType], [EntityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121203306_AddStrideSecurityHardening'
)
BEGIN
    CREATE INDEX [IX_AuditLog_OccurredAt] ON [AuditLog] ([OccurredAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121203306_AddStrideSecurityHardening'
)
BEGIN
    ALTER TABLE [SessionAttendance] ADD CONSTRAINT [FK_SessionAttendance_Student_RecordedByStudentId] FOREIGN KEY ([RecordedByStudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121203306_AddStrideSecurityHardening'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261121203306_AddStrideSecurityHardening', N'11.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122162218_AddUniversityLogoUrl'
)
BEGIN
    ALTER TABLE [University] ADD [LogoUrl] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122162218_AddUniversityLogoUrl'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261122162218_AddUniversityLogoUrl', N'11.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124005253_AddAuthAbuseProtection'
)
BEGIN
    ALTER TABLE [Student] ADD [FailedLoginCount] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124005253_AddAuthAbuseProtection'
)
BEGIN
    ALTER TABLE [Student] ADD [LockoutEndUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124005253_AddAuthAbuseProtection'
)
BEGIN
    ALTER TABLE [Student] ADD [PreviousRefreshToken] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124005253_AddAuthAbuseProtection'
)
BEGIN
    ALTER TABLE [Student] ADD [PreviousRefreshTokenRotatedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124005253_AddAuthAbuseProtection'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261124005253_AddAuthAbuseProtection', N'11.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124010511_AddScoreSourceReference'
)
BEGIN
    ALTER TABLE [ScoreTransaction] ADD [SourceReferenceId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124010511_AddScoreSourceReference'
)
BEGIN

    EXEC(N'
    WITH Parsed AS (
        SELECT TransactionId, StudentId, SourceType,
               TRY_CAST(
                   CASE
                       WHEN CHARINDEX(''#'', Reason) > 0
                        AND CHARINDEX('':'', Reason, CHARINDEX(''#'', Reason)) > CHARINDEX(''#'', Reason) + 1
                       THEN SUBSTRING(
                           Reason,
                           CHARINDEX(''#'', Reason) + 1,
                           CHARINDEX('':'', Reason, CHARINDEX(''#'', Reason)) - CHARINDEX(''#'', Reason) - 1)
                   END AS int) AS RefId
        FROM ScoreTransaction
        WHERE SourceType IN (''TaskCompleted'', ''EventAttendance'', ''ProjectDelivered'')
          AND CHARINDEX(''#'', Reason) > 0
          AND CHARINDEX('':'', Reason, CHARINDEX(''#'', Reason)) > CHARINDEX(''#'', Reason) + 1
    ), Ranked AS (
        SELECT TransactionId, RefId,
               ROW_NUMBER() OVER (PARTITION BY StudentId, SourceType, RefId ORDER BY TransactionId) AS rn
        FROM Parsed WHERE RefId IS NOT NULL
    )
    UPDATE t SET SourceReferenceId = r.RefId
    FROM ScoreTransaction t
    JOIN Ranked r ON r.TransactionId = t.TransactionId
    WHERE r.rn = 1;
    ');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124010511_AddScoreSourceReference'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ScoreTransaction_StudentId_SourceType_SourceReferenceId] ON [ScoreTransaction] ([StudentId], [SourceType], [SourceReferenceId]) WHERE [SourceReferenceId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124010511_AddScoreSourceReference'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261124010511_AddScoreSourceReference', N'11.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124013710_AddStudentEmailPreferences'
)
BEGIN
    ALTER TABLE [Student] ADD [EmailNotificationsEnabled] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124013710_AddStudentEmailPreferences'
)
BEGIN
    ALTER TABLE [Student] ADD [ReceiveLeaderboardDigests] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124013710_AddStudentEmailPreferences'
)
BEGIN
    ALTER TABLE [Student] ADD [ReceiveProjectMatchEmails] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124013710_AddStudentEmailPreferences'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261124013710_AddStudentEmailPreferences', N'11.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME(OBJECT_NAME([c].[default_object_id]))
    FROM [sys].[columns] [c]
    WHERE [c].[object_id] = OBJECT_ID(N'[Student]') AND [c].[name] = N'FacultyId';
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Student] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Student] ALTER COLUMN [FacultyId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [Student] ADD [AffiliationType] varchar(30) NOT NULL DEFAULT 'UniversityStudent';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [Student] ADD [City] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [OrganizationMember] ADD [HierarchyTier] varchar(30) NOT NULL DEFAULT 'OrganizingCrew';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [OrganizationMember] ADD [OrganizationUnitId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [Organization] ADD [City] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [Organization] ADD [CoverImageUrl] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [Organization] ADD [CurrentSeasonName] nvarchar(80) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [Organization] ADD [IsRecruiting] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [Organization] ADD [Scope] varchar(30) NOT NULL DEFAULT 'UniversityBranch';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [Organization] ADD [UniversityId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [Organization] ADD [WebsiteUrl] nvarchar(300) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [MembershipApplication] ADD [EvaluationScore] decimal(4,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [MembershipApplication] ADD [InterviewNotes] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [MembershipApplication] ADD [OrganizationUnitId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [MembershipApplication] ADD [ReviewerStudentId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [MembershipApplication] ADD [SeasonName] nvarchar(80) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [MembershipApplication] ADD [Stage] varchar(30) NOT NULL DEFAULT 'Applied';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [Event] ADD [DeliveryMode] varchar(30) NOT NULL DEFAULT 'InPerson';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [Event] ADD [IsPublic] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [Event] ADD [MeetingUrl] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [Event] ADD [OrganizationUnitId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    CREATE TABLE [OrganizationUnit] (
        [OrganizationUnitId] int NOT NULL IDENTITY,
        [OrganizationId] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [Description] nvarchar(600) NULL,
        [UnitType] varchar(30) NOT NULL DEFAULT 'TechnicalChapter',
        [IsRecruiting] bit NOT NULL DEFAULT CAST(0 AS bit),
        [OpenSeats] int NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_OrganizationUnit] PRIMARY KEY ([OrganizationUnitId]),
        CONSTRAINT [CK_OrganizationUnit_UnitType] CHECK ([UnitType] IN ('TechnicalChapter', 'OperationalCommittee', 'AffinityGroup', 'WorkingGroup')),
        CONSTRAINT [FK_OrganizationUnit_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organization] ([OrganizationId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    EXEC(N'ALTER TABLE [Student] ADD CONSTRAINT [CK_Student_AffiliationType] CHECK ([AffiliationType] IN (''UniversityStudent'', ''IndependentLearner'', ''FreshGraduate'', ''HighSchoolTech''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    CREATE INDEX [IX_OrganizationMember_OrganizationUnitId] ON [OrganizationMember] ([OrganizationUnitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    EXEC(N'ALTER TABLE [OrganizationMember] ADD CONSTRAINT [CK_OrganizationMember_HierarchyTier] CHECK ([HierarchyTier] IN (''HighBoard'', ''CommitteeLeadership'', ''OrganizingCrew'', ''Participant''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Organization_Scope_IsRecruiting] ON [Organization] ([Scope], [IsRecruiting]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Organization_UniversityId] ON [Organization] ([UniversityId]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    EXEC(N'ALTER TABLE [Organization] ADD CONSTRAINT [CK_Organization_Scope] CHECK ([Scope] IN (''UniversityBranch'', ''FacultyChapter'', ''CrossUniversity'', ''Independent'', ''NationalNetwork''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_MembershipApplication_OrganizationId_Stage] ON [MembershipApplication] ([OrganizationId], [Stage]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    CREATE INDEX [IX_MembershipApplication_OrganizationUnitId] ON [MembershipApplication] ([OrganizationUnitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    EXEC(N'ALTER TABLE [MembershipApplication] ADD CONSTRAINT [CK_MembershipApplication_Stage] CHECK ([Stage] IN (''Applied'', ''HrScreening'', ''CommitteeInterview'', ''Accepted'', ''Rejected'', ''Withdrawn''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    CREATE INDEX [IX_Event_OrganizationUnitId] ON [Event] ([OrganizationUnitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    EXEC(N'ALTER TABLE [Event] ADD CONSTRAINT [CK_Event_DeliveryMode] CHECK ([DeliveryMode] IN (''InPerson'', ''Online'', ''Hybrid''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_OrganizationUnit_OrganizationId_Code] ON [OrganizationUnit] ([OrganizationId], [Code]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [Event] ADD CONSTRAINT [FK_Event_OrganizationUnit_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [OrganizationUnit] ([OrganizationUnitId]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [MembershipApplication] ADD CONSTRAINT [FK_MembershipApplication_OrganizationUnit_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [OrganizationUnit] ([OrganizationUnitId]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [Organization] ADD CONSTRAINT [FK_Organization_University_UniversityId] FOREIGN KEY ([UniversityId]) REFERENCES [University] ([UniversityId]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    ALTER TABLE [OrganizationMember] ADD CONSTRAINT [FK_OrganizationMember_OrganizationUnit_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [OrganizationUnit] ([OrganizationUnitId]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124175844_AddFlexibleActivitiesAndHierarchy'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261124175844_AddFlexibleActivitiesAndHierarchy', N'11.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124231511_AddOrganizationCreationRequests'
)
BEGIN
    CREATE TABLE [OrganizationCreationRequest] (
        [RequestId] int NOT NULL IDENTITY,
        [RequesterStudentId] int NOT NULL,
        [Name] nvarchar(120) NOT NULL,
        [Description] nvarchar(4000) NOT NULL,
        [Category] nvarchar(50) NOT NULL,
        [Scope] varchar(30) NOT NULL DEFAULT 'UniversityBranch',
        [UniversityId] int NULL,
        [City] nvarchar(100) NULL,
        [LogoUrl] nvarchar(300) NULL,
        [CoverImageUrl] nvarchar(500) NULL,
        [WebsiteUrl] nvarchar(300) NULL,
        [CurrentSeasonName] nvarchar(80) NULL,
        [Justification] nvarchar(1000) NULL,
        [Status] varchar(30) NOT NULL DEFAULT 'Pending',
        [RequestedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [ReviewedAt] datetime2 NULL,
        [ReviewedByStudentId] int NULL,
        [ReviewNotes] nvarchar(1000) NULL,
        [CreatedOrganizationId] int NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_OrganizationCreationRequest] PRIMARY KEY ([RequestId]),
        CONSTRAINT [CK_OrganizationCreationRequest_Scope] CHECK ([Scope] IN ('UniversityBranch', 'FacultyChapter', 'CrossUniversity', 'Independent', 'NationalNetwork')),
        CONSTRAINT [CK_OrganizationCreationRequest_Status] CHECK ([Status] IN ('Pending', 'Accepted', 'Rejected', 'Withdrawn')),
        CONSTRAINT [FK_OrganizationCreationRequest_Organization_CreatedOrganizationId] FOREIGN KEY ([CreatedOrganizationId]) REFERENCES [Organization] ([OrganizationId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrganizationCreationRequest_Student_RequesterStudentId] FOREIGN KEY ([RequesterStudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrganizationCreationRequest_Student_ReviewedByStudentId] FOREIGN KEY ([ReviewedByStudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrganizationCreationRequest_University_UniversityId] FOREIGN KEY ([UniversityId]) REFERENCES [University] ([UniversityId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124231511_AddOrganizationCreationRequests'
)
BEGIN
    CREATE INDEX [IX_OrganizationCreationRequest_CreatedOrganizationId] ON [OrganizationCreationRequest] ([CreatedOrganizationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124231511_AddOrganizationCreationRequests'
)
BEGIN
    CREATE INDEX [IX_OrganizationCreationRequest_RequesterStudentId] ON [OrganizationCreationRequest] ([RequesterStudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124231511_AddOrganizationCreationRequests'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_OrganizationCreationRequest_RequesterStudentId_Name] ON [OrganizationCreationRequest] ([RequesterStudentId], [Name]) WHERE [DeletedAt] IS NULL AND [Status] = ''Pending''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124231511_AddOrganizationCreationRequests'
)
BEGIN
    CREATE INDEX [IX_OrganizationCreationRequest_ReviewedByStudentId] ON [OrganizationCreationRequest] ([ReviewedByStudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124231511_AddOrganizationCreationRequests'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_OrganizationCreationRequest_Status_RequestedAt] ON [OrganizationCreationRequest] ([Status], [RequestedAt]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124231511_AddOrganizationCreationRequests'
)
BEGIN
    CREATE INDEX [IX_OrganizationCreationRequest_UniversityId] ON [OrganizationCreationRequest] ([UniversityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124231511_AddOrganizationCreationRequests'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261124231511_AddOrganizationCreationRequests', N'11.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261125064828_AddStoredImages'
)
BEGIN
    ALTER TABLE [ScoreTransaction] DROP CONSTRAINT [CK_ScoreTransaction_SourceType];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261125064828_AddStoredImages'
)
BEGIN
    CREATE TABLE [StoredImage] (
        [StoredImageId] uniqueidentifier NOT NULL,
        [UploadedByStudentId] int NOT NULL,
        [Content] varbinary(max) NOT NULL,
        [ContentType] nvarchar(50) NOT NULL,
        [SizeBytes] bigint NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_StoredImage] PRIMARY KEY ([StoredImageId]),
        CONSTRAINT [FK_StoredImage_Student_UploadedByStudentId] FOREIGN KEY ([UploadedByStudentId]) REFERENCES [Student] ([StudentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261125064828_AddStoredImages'
)
BEGIN
    EXEC(N'ALTER TABLE [ScoreTransaction] ADD CONSTRAINT [CK_ScoreTransaction_SourceType] CHECK ([SourceType] IN (''EventAttendance'', ''TaskCompleted'', ''ProjectDelivered'', ''Manual'', ''AttendanceRevocation''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261125064828_AddStoredImages'
)
BEGIN
    CREATE INDEX [IX_StoredImage_UploadedByStudentId] ON [StoredImage] ([UploadedByStudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261125064828_AddStoredImages'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261125064828_AddStoredImages', N'11.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261125121701_AddCoversAndSystemImages'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME(OBJECT_NAME([c].[default_object_id]))
    FROM [sys].[columns] [c]
    WHERE [c].[object_id] = OBJECT_ID(N'[StoredImage]') AND [c].[name] = N'UploadedByStudentId';
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [StoredImage] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [StoredImage] ALTER COLUMN [UploadedByStudentId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261125121701_AddCoversAndSystemImages'
)
BEGIN
    ALTER TABLE [Project] ADD [CoverImageUrl] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261125121701_AddCoversAndSystemImages'
)
BEGIN
    ALTER TABLE [Event] ADD [CoverImageUrl] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261125121701_AddCoversAndSystemImages'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261125121701_AddCoversAndSystemImages', N'11.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126180429_HardenCoverImageUrlMaxLength'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME(OBJECT_NAME([c].[default_object_id]))
    FROM [sys].[columns] [c]
    WHERE [c].[object_id] = OBJECT_ID(N'[Project]') AND [c].[name] = N'CoverImageUrl';
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Project] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [Project] ALTER COLUMN [CoverImageUrl] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126180429_HardenCoverImageUrlMaxLength'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME(OBJECT_NAME([c].[default_object_id]))
    FROM [sys].[columns] [c]
    WHERE [c].[object_id] = OBJECT_ID(N'[Event]') AND [c].[name] = N'CoverImageUrl';
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Event] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [Event] ALTER COLUMN [CoverImageUrl] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126180429_HardenCoverImageUrlMaxLength'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261126180429_HardenCoverImageUrlMaxLength', N'11.0.0');
END;

COMMIT;
GO

