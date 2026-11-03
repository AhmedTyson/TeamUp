using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TeamUp.Application.Common.Events;
using TeamUp.Domain.Common;
using TeamUp.Domain.Entities;
using TeamUp.Infrastructure.EventDispatching;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.UnitTests;

public record StudentRegisteredDomainEvent(int StudentId, string Email) : IDomainEvent;

public class TestStudentRegisteredHandler : IDomainEventHandler<StudentRegisteredDomainEvent>
{
    public static readonly List<StudentRegisteredDomainEvent> HandledEvents = [];

    public Task HandleAsync(StudentRegisteredDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        HandledEvents.Add(domainEvent);
        return Task.CompletedTask;
    }
}

public class DomainEventTests : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");

    public DomainEventTests()
    {
        _conn.Open();
        TestStudentRegisteredHandler.HandledEvents.Clear();
    }

    public void Dispose() => _conn.Dispose();

    [Fact]
    public void BaseEntity_DomainEventsCollection_CanAddAndClear()
    {
        var uni = new University { Name = "Event Uni", City = "Cairo", Country = "Egypt" };
        var evt = new StudentRegisteredDomainEvent(1, "test@test.com");

        uni.AddDomainEvent(evt);
        Assert.Single(uni.DomainEvents);
        Assert.Equal(evt, uni.DomainEvents.First());

        uni.ClearDomainEvents();
        Assert.Empty(uni.DomainEvents);
    }

    [Fact]
    public async Task SaveChangesAsync_AutomaticallyDispatchesEvents_AndClearsFromEntity()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventHandler<StudentRegisteredDomainEvent>, TestStudentRegisteredHandler>();
        services.AddSingleton<IDomainEventDispatcher>(sp => new DomainEventDispatcher(sp, NullLogger<DomainEventDispatcher>.Instance));

        using var sp = services.BuildServiceProvider();
        var dispatcher = sp.GetRequiredService<IDomainEventDispatcher>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_conn)
            .Options;

        using var db = new AppDbContext(options, dispatcher);
        db.Database.EnsureCreated();

        var uni = new University { Name = "Test Uni For Events", City = "Giza", Country = "Egypt" };
        var evt = new StudentRegisteredDomainEvent(42, "event_student@test.com");
        uni.AddDomainEvent(evt);

        db.Universities.Add(uni);
        await db.SaveChangesAsync();

        Assert.Empty(uni.DomainEvents);
        Assert.Single(TestStudentRegisteredHandler.HandledEvents);
        Assert.Equal(42, TestStudentRegisteredHandler.HandledEvents.First().StudentId);
    }
}
