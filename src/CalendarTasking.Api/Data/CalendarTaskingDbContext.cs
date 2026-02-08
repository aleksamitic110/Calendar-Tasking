using CalendarTasking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CalendarTasking.Api.Data;

public class CalendarTaskingDbContext(DbContextOptions<CalendarTaskingDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Calendar> Calendars => Set<Calendar>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<PrivateClassSession> PrivateClassSessions => Set<PrivateClassSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.UserId);

            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();

            entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.TimeZoneId).HasMaxLength(64).IsRequired().HasDefaultValue("UTC");
            entity.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasMany(x => x.OwnedCalendars)
                .WithOne(x => x.OwnerUser)
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.CreatedEvents)
                .WithOne(x => x.CreatedByUser)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(x => x.CreatedTasks)
                .WithOne(x => x.CreatedByUser)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(x => x.CreatedPrivateClassSessions)
                .WithOne(x => x.CreatedByUser)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Calendar>(entity =>
        {
            entity.ToTable("Calendars", table =>
            {
                table.HasCheckConstraint(
                    "CK_Calendars_ColorHex",
                    "LEN([ColorHex]) = 7 AND [ColorHex] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'");
            });

            entity.HasKey(x => x.CalendarId);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.ColorHex).HasMaxLength(7).IsRequired().HasDefaultValue("#2563EB");
            entity.Property(x => x.IsDefault).IsRequired().HasDefaultValue(false);
            entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(x => new { x.OwnerUserId, x.Name }).IsUnique();
            entity.HasIndex(x => new { x.OwnerUserId, x.IsDefault })
                .HasDatabaseName("IX_Calendars_OneDefaultPerOwner")
                .HasFilter("[IsDefault] = 1")
                .IsUnique();

            entity.HasMany(x => x.Events)
                .WithOne(x => x.Calendar)
                .HasForeignKey(x => x.CalendarId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Tasks)
                .WithOne(x => x.Calendar)
                .HasForeignKey(x => x.CalendarId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.PrivateClassSessions)
                .WithOne(x => x.Calendar)
                .HasForeignKey(x => x.CalendarId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events", table =>
            {
                table.HasCheckConstraint("CK_Events_EndAfterStart", "[EndUtc] > [StartUtc]");
                table.HasCheckConstraint("CK_Events_RepeatType", "[RepeatType] IN ('None','Daily','Weekly','Monthly')");
                table.HasCheckConstraint("CK_Events_Status", "[Status] IN ('Planned','Cancelled')");
                table.HasCheckConstraint("CK_Events_ReminderNonNegative", "[ReminderMinutesBefore] IS NULL OR [ReminderMinutesBefore] >= 0");
            });

            entity.HasKey(x => x.EventId);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.Location).HasMaxLength(200);
            entity.Property(x => x.RepeatType).HasMaxLength(20).IsRequired().HasDefaultValue("None");
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Planned");
            entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => new { x.CalendarId, x.StartUtc });
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks", table =>
            {
                table.HasCheckConstraint("CK_Tasks_Priority", "[Priority] IN ('Low','Medium','High')");
                table.HasCheckConstraint("CK_Tasks_Status", "[Status] IN ('Todo','InProgress','Done')");
                table.HasCheckConstraint("CK_Tasks_DoneHasCompletedAt", "[Status] <> 'Done' OR [CompletedAtUtc] IS NOT NULL");
                table.HasCheckConstraint("CK_Tasks_ReminderNonNegative", "[ReminderMinutesBefore] IS NULL OR [ReminderMinutesBefore] >= 0");
            });

            entity.HasKey(x => x.TaskItemId);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.Priority).HasMaxLength(20).IsRequired().HasDefaultValue("Medium");
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Todo");
            entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => new { x.CalendarId, x.Status, x.DueUtc });
        });

        modelBuilder.Entity<PrivateClassSession>(entity =>
        {
            entity.ToTable("PrivateClassSessions", table =>
            {
                table.HasCheckConstraint("CK_PrivateClassSessions_EndAfterStart", "[SessionEndUtc] > [SessionStartUtc]");
                table.HasCheckConstraint("CK_PrivateClassSessions_PriceNonNegative", "[PriceAmount] >= 0");
                table.HasCheckConstraint("CK_PrivateClassSessions_PaidRequiresPaidAt", "[IsPaid] = 0 OR [PaidAtUtc] IS NOT NULL");
                table.HasCheckConstraint("CK_PrivateClassSessions_PaymentMethod", "[PaymentMethod] IS NULL OR [PaymentMethod] IN ('Cash','Card','Transfer')");
                table.HasCheckConstraint("CK_PrivateClassSessions_Status", "[Status] IN ('Scheduled','Completed','Cancelled','NoShow')");
            });

            entity.HasKey(x => x.PrivateClassSessionId);
            entity.Property(x => x.StudentName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.StudentContact).HasMaxLength(120);
            entity.Property(x => x.TopicPlanned).HasMaxLength(500);
            entity.Property(x => x.TopicDone).HasMaxLength(1500);
            entity.Property(x => x.HomeworkAssigned).HasMaxLength(1500);
            entity.Property(x => x.PriceAmount).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired().HasDefaultValue("RSD");
            entity.Property(x => x.PaymentMethod).HasMaxLength(20);
            entity.Property(x => x.PaymentNote).HasMaxLength(500);
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Scheduled");
            entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(x => new { x.CalendarId, x.SessionStartUtc });
            entity.HasIndex(x => new { x.CalendarId, x.IsPaid, x.SessionStartUtc });
        });
    }
}
