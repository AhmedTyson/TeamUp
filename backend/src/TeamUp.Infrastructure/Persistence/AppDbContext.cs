using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Events;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Domain.Common;
using TeamUp.Domain.Entities;

namespace TeamUp.Infrastructure.Persistence;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IDomainEventDispatcher? dispatcher = null) : DbContext(options), IAppDbContext
{
    public DbSet<University> Universities => Set<University>();
    public DbSet<Faculty> Faculties => Set<Faculty>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<StudentInterest> StudentInterests => Set<StudentInterest>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();

    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<StudentSkill> StudentSkills => Set<StudentSkill>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectRequiredSkill> ProjectRequiredSkills => Set<ProjectRequiredSkill>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<ProjectApplication> ProjectApplications => Set<ProjectApplication>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
    public DbSet<MembershipApplication> MembershipApplications => Set<MembershipApplication>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    public DbSet<EventSession> EventSessions => Set<EventSession>();
    public DbSet<SessionAttendance> SessionAttendances => Set<SessionAttendance>();

    public DbSet<ScoreTransaction> ScoreTransactions => Set<ScoreTransaction>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<StudentBadge> StudentBadges => Set<StudentBadge>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(x => x.Entity.DomainEvents.Count > 0)
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        domainEntities.ForEach(x => x.ClearDomainEvents());

        var result = await base.SaveChangesAsync(cancellationToken);

        if (dispatcher is not null && domainEvents.Count > 0)
        {
            await dispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<IDomainEvent>();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entity in modelBuilder.Model.GetEntityTypes().ToList())
        {
            // Singular table names, same as the relational mapping diagram.
            entity.SetTableName(entity.ClrType.Name);

            // Soft delete: hide deleted rows (DeletedAt IS NOT NULL).
            if (typeof(ISoftDelete).IsAssignableFrom(entity.ClrType))
            {
                modelBuilder.Entity(entity.ClrType).HasQueryFilter(SoftDeleteFilter(entity.ClrType));
            }

            // Enums are stored as readable strings, guarded by a CHECK constraint.
            foreach (var prop in entity.GetProperties().Where(p => p.ClrType.IsEnum).ToList())
            {
                var names = Enum.GetNames(prop.ClrType);
                prop.SetMaxLength(30);
                prop.SetIsUnicode(false);
                prop.SetProviderClrType(typeof(string));
                var converterType = typeof(Microsoft.EntityFrameworkCore.Storage.ValueConversion.EnumToStringConverter<>)
                    .MakeGenericType(prop.ClrType);
                prop.SetValueConverter((Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter)
                    Activator.CreateInstance(converterType)!);
                var list = string.Join(", ", names.Select(n => $"'{n}'"));
                var table = entity.GetTableName()!;
                modelBuilder.Entity(entity.ClrType).ToTable(t =>
                    t.HasCheckConstraint($"CK_{table}_{prop.Name}", $"[{prop.Name}] IN ({list})"));
            }
        }

        // Soft delete makes physical cascades meaningless and dangerous: every FK is
        // ON DELETE NO ACTION (Restrict). ON UPDATE is NO ACTION because PKs are immutable.
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;

        if (Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
        {
            modelBuilder.Entity<Project>().Property(p => p.RowVersion).ValueGeneratedNever();
            modelBuilder.Entity<Event>().Property(e => e.RowVersion).ValueGeneratedNever();
        }
    }

    private static LambdaExpression SoftDeleteFilter(Type type)
    {
        var p = Expression.Parameter(type, "e");
        var deletedAt = Expression.Call(typeof(EF), nameof(EF.Property), [typeof(DateTime?)], p, Expression.Constant(nameof(ISoftDelete.DeletedAt)));
        var isNull = Expression.Equal(deletedAt, Expression.Constant(null, typeof(DateTime?)));
        return Expression.Lambda(isNull, p);
    }
}
