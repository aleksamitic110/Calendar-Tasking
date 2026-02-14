/*
    CalendarTasking SQL schema
    Run in SQL Server Management Studio.
*/

IF DB_ID('CalendarTaskingDb') IS NULL
BEGIN
    CREATE DATABASE [CalendarTaskingDb];
END
GO

USE [CalendarTaskingDb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.PrivateClassSessions', 'U') IS NOT NULL DROP TABLE dbo.PrivateClassSessions;
IF OBJECT_ID('dbo.Tasks', 'U') IS NOT NULL DROP TABLE dbo.Tasks;
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.Calendars', 'U') IS NOT NULL DROP TABLE dbo.Calendars;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users
(
    UserId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(80) NOT NULL,
    LastName NVARCHAR(80) NOT NULL,
    TimeZoneId NVARCHAR(64) NOT NULL CONSTRAINT DF_Users_TimeZoneId DEFAULT ('UTC'),
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT ((1)),
    CreatedAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Users_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc DATETIME2(7) NULL,
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
GO

CREATE TABLE dbo.Calendars
(
    CalendarId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Calendars PRIMARY KEY,
    OwnerUserId INT NOT NULL,
    Name NVARCHAR(120) NOT NULL,
    Description NVARCHAR(500) NULL,
    ColorHex CHAR(7) NOT NULL CONSTRAINT DF_Calendars_ColorHex DEFAULT ('#2563EB'),
    IsDefault BIT NOT NULL CONSTRAINT DF_Calendars_IsDefault DEFAULT ((0)),
    CreatedAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Calendars_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc DATETIME2(7) NULL,
    CONSTRAINT FK_Calendars_Users_OwnerUserId
        FOREIGN KEY (OwnerUserId)
        REFERENCES dbo.Users (UserId)
        ON DELETE CASCADE,
    CONSTRAINT CK_Calendars_ColorHex
        CHECK (LEN(ColorHex) = 7 AND ColorHex LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]')
);
GO

CREATE UNIQUE INDEX UX_Calendars_OwnerUserId_Name ON dbo.Calendars (OwnerUserId, Name);
CREATE UNIQUE INDEX UX_Calendars_OneDefaultPerOwner ON dbo.Calendars (OwnerUserId, IsDefault) WHERE IsDefault = 1;
GO

CREATE TABLE dbo.Events
(
    EventId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Events PRIMARY KEY,
    CalendarId INT NOT NULL,
    CreatedByUserId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Location NVARCHAR(200) NULL,
    StartUtc DATETIME2(7) NOT NULL,
    EndUtc DATETIME2(7) NOT NULL,
    IsAllDay BIT NOT NULL CONSTRAINT DF_Events_IsAllDay DEFAULT ((0)),
    RepeatType NVARCHAR(20) NOT NULL CONSTRAINT DF_Events_RepeatType DEFAULT ('None'),
    ReminderMinutesBefore INT NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Events_Status DEFAULT ('Planned'),
    CreatedAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Events_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc DATETIME2(7) NULL,
    CONSTRAINT FK_Events_Calendars_CalendarId
        FOREIGN KEY (CalendarId)
        REFERENCES dbo.Calendars (CalendarId)
        ON DELETE CASCADE,
    CONSTRAINT FK_Events_Users_CreatedByUserId
        FOREIGN KEY (CreatedByUserId)
        REFERENCES dbo.Users (UserId),
    CONSTRAINT CK_Events_EndAfterStart CHECK (EndUtc > StartUtc),
    CONSTRAINT CK_Events_RepeatType CHECK (RepeatType IN ('None','Daily','Weekly','Monthly')),
    CONSTRAINT CK_Events_Status CHECK (Status IN ('Planned','Cancelled')),
    CONSTRAINT CK_Events_ReminderNonNegative CHECK (ReminderMinutesBefore IS NULL OR ReminderMinutesBefore >= 0)
);
GO

CREATE INDEX IX_Events_CalendarId_StartUtc ON dbo.Events (CalendarId, StartUtc);
GO

CREATE TABLE dbo.Tasks
(
    TaskItemId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tasks PRIMARY KEY,
    CalendarId INT NOT NULL,
    CreatedByUserId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    DueUtc DATETIME2(7) NULL,
    Priority NVARCHAR(20) NOT NULL CONSTRAINT DF_Tasks_Priority DEFAULT ('Medium'),
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Tasks_Status DEFAULT ('Todo'),
    CompletedAtUtc DATETIME2(7) NULL,
    ReminderMinutesBefore INT NULL,
    CreatedAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Tasks_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc DATETIME2(7) NULL,
    CONSTRAINT FK_Tasks_Calendars_CalendarId
        FOREIGN KEY (CalendarId)
        REFERENCES dbo.Calendars (CalendarId)
        ON DELETE CASCADE,
    CONSTRAINT FK_Tasks_Users_CreatedByUserId
        FOREIGN KEY (CreatedByUserId)
        REFERENCES dbo.Users (UserId),
    CONSTRAINT CK_Tasks_Priority CHECK (Priority IN ('Low','Medium','High')),
    CONSTRAINT CK_Tasks_Status CHECK (Status IN ('Todo','InProgress','Done')),
    CONSTRAINT CK_Tasks_DoneHasCompletedAt CHECK (Status <> 'Done' OR CompletedAtUtc IS NOT NULL),
    CONSTRAINT CK_Tasks_ReminderNonNegative CHECK (ReminderMinutesBefore IS NULL OR ReminderMinutesBefore >= 0)
);
GO

CREATE INDEX IX_Tasks_CalendarId_Status_DueUtc ON dbo.Tasks (CalendarId, Status, DueUtc);
GO

CREATE TABLE dbo.PrivateClassSessions
(
    PrivateClassSessionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PrivateClassSessions PRIMARY KEY,
    CalendarId INT NOT NULL,
    CreatedByUserId INT NOT NULL,
    StudentName NVARCHAR(120) NOT NULL,
    StudentContact NVARCHAR(120) NULL,
    SessionStartUtc DATETIME2(7) NOT NULL,
    SessionEndUtc DATETIME2(7) NOT NULL,
    TopicPlanned NVARCHAR(500) NULL,
    TopicDone NVARCHAR(1500) NULL,
    HomeworkAssigned NVARCHAR(1500) NULL,
    PriceAmount DECIMAL(10,2) NOT NULL,
    CurrencyCode CHAR(3) NOT NULL CONSTRAINT DF_PrivateClassSessions_CurrencyCode DEFAULT ('RSD'),
    IsPaid BIT NOT NULL CONSTRAINT DF_PrivateClassSessions_IsPaid DEFAULT ((0)),
    PaidAtUtc DATETIME2(7) NULL,
    PaymentMethod NVARCHAR(20) NULL,
    PaymentNote NVARCHAR(500) NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_PrivateClassSessions_Status DEFAULT ('Scheduled'),
    CreatedAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_PrivateClassSessions_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc DATETIME2(7) NULL,
    CONSTRAINT FK_PrivateClassSessions_Calendars_CalendarId
        FOREIGN KEY (CalendarId)
        REFERENCES dbo.Calendars (CalendarId)
        ON DELETE CASCADE,
    CONSTRAINT FK_PrivateClassSessions_Users_CreatedByUserId
        FOREIGN KEY (CreatedByUserId)
        REFERENCES dbo.Users (UserId),
    CONSTRAINT CK_PrivateClassSessions_EndAfterStart CHECK (SessionEndUtc > SessionStartUtc),
    CONSTRAINT CK_PrivateClassSessions_PriceNonNegative CHECK (PriceAmount >= 0),
    CONSTRAINT CK_PrivateClassSessions_PaidRequiresPaidAt CHECK (IsPaid = 0 OR PaidAtUtc IS NOT NULL),
    CONSTRAINT CK_PrivateClassSessions_PaymentMethod CHECK (PaymentMethod IS NULL OR PaymentMethod IN ('Cash','Card','Transfer')),
    CONSTRAINT CK_PrivateClassSessions_Status CHECK (Status IN ('Scheduled','Completed','Cancelled','NoShow'))
);
GO

CREATE INDEX IX_PrivateClassSessions_CalendarId_SessionStartUtc ON dbo.PrivateClassSessions (CalendarId, SessionStartUtc);
CREATE INDEX IX_PrivateClassSessions_CalendarId_IsPaid_SessionStartUtc ON dbo.PrivateClassSessions (CalendarId, IsPaid, SessionStartUtc);
GO
