using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TeamUp.Application.Common.Events;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Domain.Common;
using TeamUp.Domain.Entities;

namespace TeamUp.Infrastructure.Persistence;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IDomainEventDispatcher? dispatcher = null) : DbContext(options), IAppDbContext
{
    private readonly List<IDomainEvent> _deferredDomainEvents = [];
    private bool _deferDomainEvents;

    #region DbSets

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
    public DbSet<OrganizationCreationRequest> OrganizationCreationRequests => Set<OrganizationCreationRequest>();
    public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();
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
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<StoredImage> StoredImages => Set<StoredImage>();

    #endregion

    #region Domain Event Dispatching

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
            if (_deferDomainEvents)
            {
                _deferredDomainEvents.AddRange(domainEvents);
            }
            else
            {
                await dispatcher.DispatchAsync(domainEvents, cancellationToken);
            }
        }

        return result;
    }

    public void BeginDeferringDomainEvents() => _deferDomainEvents = true;

    public async Task DispatchDeferredDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        _deferDomainEvents = false;
        if (dispatcher is null || _deferredDomainEvents.Count == 0)
        {
            _deferredDomainEvents.Clear();
            return;
        }

        var snapshot = _deferredDomainEvents.ToList();
        _deferredDomainEvents.Clear();

        // Runs strictly AFTER the transaction committed: the primary write already succeeded, so a failing
        // side effect (SMTP outage, notification insert, ...) must never turn it into a 500. The dispatcher
        // logs each handler failure; we isolate per event so one bad handler cannot starve the others.
        foreach (var domainEvent in snapshot)
        {
            try
            {
                await dispatcher.DispatchAsync([domainEvent], cancellationToken);
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
            }
        }
    }

    public void ClearDeferredDomainEvents()
    {
        _deferDomainEvents = false;
        _deferredDomainEvents.Clear();
    }

    #endregion

    #region Model Configuration

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
                modelBuilder.Entity(entity.ClrType).HasQueryFilter(QueryFilterNames.SoftDelete, SoftDeleteFilter(entity.ClrType));
            }

            // Enums are stored as readable strings, guarded by a CHECK constraint.
            foreach (var prop in entity.GetProperties().Where(p => p.ClrType.IsEnum).ToList())
            {
                var names = Enum.GetNames(prop.ClrType);
                prop.SetMaxLength(30);
                prop.SetIsUnicode(false);
                prop.SetProviderClrType(typeof(string));
                var converterType = typeof(EnumToStringConverter<>)
                    .MakeGenericType(prop.ClrType);
                prop.SetValueConverter((ValueConverter)Activator.CreateInstance(converterType)!);
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

        if (Database.IsSqlite())
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

    #endregion
}
