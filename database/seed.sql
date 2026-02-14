/*
    CalendarTasking seed data
    Prerequisite: database/schema.sql already executed.
*/

USE [CalendarTaskingDb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @UserId INT;
DECLARE @MainCalendarId INT;
DECLARE @PrivateCalendarId INT;

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'ana@example.com')
BEGIN
    INSERT INTO dbo.Users (Email, PasswordHash, FirstName, LastName, TimeZoneId, IsActive)
    VALUES
    (
        'ana@example.com',
        '100000.AQIDBAUGBwgJCgsMDQ4PEA==.oTIQ+V61Mofba8nbqloez0EHoOhnEnMt7NYfUcvpn9A=',
        'Ana',
        'Ilic',
        'Central European Standard Time',
        1
    );
END

SELECT @UserId = UserId FROM dbo.Users WHERE Email = 'ana@example.com';

IF NOT EXISTS (SELECT 1 FROM dbo.Calendars WHERE OwnerUserId = @UserId AND Name = 'Main Calendar')
BEGIN
    INSERT INTO dbo.Calendars (OwnerUserId, Name, Description, ColorHex, IsDefault)
    VALUES (@UserId, 'Main Calendar', 'Default personal calendar', '#2563EB', 1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.Calendars WHERE OwnerUserId = @UserId AND Name = 'Private Classes')
BEGIN
    INSERT INTO dbo.Calendars (OwnerUserId, Name, Description, ColorHex, IsDefault)
    VALUES (@UserId, 'Private Classes', 'Sessions and payments', '#0F766E', 0);
END

SELECT @MainCalendarId = CalendarId FROM dbo.Calendars WHERE OwnerUserId = @UserId AND Name = 'Main Calendar';
SELECT @PrivateCalendarId = CalendarId FROM dbo.Calendars WHERE OwnerUserId = @UserId AND Name = 'Private Classes';

IF NOT EXISTS (SELECT 1 FROM dbo.Events WHERE CalendarId = @MainCalendarId AND Title = 'Study Session')
BEGIN
    INSERT INTO dbo.Events
    (
        CalendarId,
        CreatedByUserId,
        Title,
        Description,
        Location,
        StartUtc,
        EndUtc,
        IsAllDay,
        RepeatType,
        ReminderMinutesBefore,
        Status
    )
    VALUES
    (
        @MainCalendarId,
        @UserId,
        'Study Session',
        'Preparation for QA class.',
        'Home',
        DATEADD(DAY, 1, SYSUTCDATETIME()),
        DATEADD(HOUR, 2, DATEADD(DAY, 1, SYSUTCDATETIME())),
        0,
        'None',
        30,
        'Planned'
    );
END

IF NOT EXISTS (SELECT 1 FROM dbo.Tasks WHERE CalendarId = @MainCalendarId AND Title = 'Write NUnit tests')
BEGIN
    INSERT INTO dbo.Tasks
    (
        CalendarId,
        CreatedByUserId,
        Title,
        Description,
        DueUtc,
        Priority,
        Status,
        ReminderMinutesBefore
    )
    VALUES
    (
        @MainCalendarId,
        @UserId,
        'Write NUnit tests',
        'Create 3 component tests per CRUD endpoint.',
        DATEADD(DAY, 3, SYSUTCDATETIME()),
        'High',
        'Todo',
        60
    );
END

IF NOT EXISTS (SELECT 1 FROM dbo.PrivateClassSessions WHERE CalendarId = @PrivateCalendarId AND StudentName = 'Marko Markovic')
BEGIN
    INSERT INTO dbo.PrivateClassSessions
    (
        CalendarId,
        CreatedByUserId,
        StudentName,
        StudentContact,
        SessionStartUtc,
        SessionEndUtc,
        TopicPlanned,
        TopicDone,
        HomeworkAssigned,
        PriceAmount,
        CurrencyCode,
        IsPaid,
        PaidAtUtc,
        PaymentMethod,
        PaymentNote,
        Status
    )
    VALUES
    (
        @PrivateCalendarId,
        @UserId,
        'Marko Markovic',
        'marko@mail.com',
        DATEADD(HOUR, -6, SYSUTCDATETIME()),
        DATEADD(HOUR, -5, SYSUTCDATETIME()),
        'SQL joins',
        'Inner and left joins',
        'Solve five SQL tasks',
        2000.00,
        'RSD',
        1,
        DATEADD(HOUR, -5, SYSUTCDATETIME()),
        'Cash',
        'Paid right after class',
        'Completed'
    );
END
GO
