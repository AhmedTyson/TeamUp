using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.UnitTests;

public sealed class SoftDeleteTests : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");

    public SoftDeleteTests()
    {
        _conn.Open();
        using var db = NewDb();
        db.Database.EnsureCreated();
    }

    private AppDbContext NewDb() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(_conn)
        .AddInterceptors(new SoftDeleteInterceptor())
        .Options);

    public void Dispose() => _conn.Dispose();

    private static University NewUniversity(string name = "Cairo University") =>
        new() { Name = name, City = "Giza", Country = "Egypt" };

    [Fact]
    public void Remove_SetsFlags_AndKeepsRow()
    {
        using var db = NewDb();
        var u = NewUniversity();
        db.Universities.Add(u);
        db.SaveChanges();

        db.Universities.Remove(u);
        db.SaveChanges();

        var raw = db.Universities.IgnoreQueryFilters().Single(x => x.UniversityId == u.UniversityId);
        Assert.True(raw.IsDeleted);
        Assert.NotNull(raw.DeletedAt);
    }

    [Fact]
    public void DeletedRow_IsHiddenByDefault_AndVisibleWithIgnoreQueryFilters()
    {
        using var db = NewDb();
        var u = NewUniversity();
        db.Universities.Add(u);
        db.SaveChanges();
        db.Universities.Remove(u);
        db.SaveChanges();

        using var fresh = NewDb();
        Assert.Empty(fresh.Universities.ToList());
        Assert.Single(fresh.Universities.IgnoreQueryFilters().ToList());
    }

    [Fact]
    public void Remove_OnlyWritesSoftDeleteColumns()
    {
        using var db = NewDb();
        var u = NewUniversity();
        db.Universities.Add(u);
        db.SaveChanges();

        db.Universities.Remove(u);
        u.Name = "Tampered";            // must NOT be persisted by the delete
        db.SaveChanges();

        using var fresh = NewDb();
        Assert.Equal("Cairo University", fresh.Universities.IgnoreQueryFilters().Single().Name);
    }

    [Fact]
    public void UniqueName_CanBeReused_AfterSoftDelete()
    {
        using var db = NewDb();
        var first = NewUniversity("Same Name");
        db.Universities.Add(first);
        db.SaveChanges();
        db.Universities.Remove(first);
        db.SaveChanges();

        db.Universities.Add(NewUniversity("Same Name"));
        db.SaveChanges();

        Assert.Single(db.Universities.ToList());
    }

    [Fact]
    public void UniqueName_StillEnforced_ForLiveRows()
    {
        using var db = NewDb();
        db.Universities.Add(NewUniversity("Dup"));
        db.SaveChanges();
        db.Universities.Add(NewUniversity("Dup"));
        Assert.Throws<DbUpdateException>(() => db.SaveChanges());
    }

    [Fact]
    public void AppendOnly_ScoreTransaction_CannotBeDeleted()
    {
        using var db = NewDb();
        var uni = NewUniversity();
        uni.Faculties.Add(new Faculty { Name = "F", Code = "F1" });
        db.Universities.Add(uni);
        db.SaveChanges();
        var st = new Student
        {
            Email = "a@b.c", PasswordHash = "x", FirstName = "A", LastName = "B", AcademicYear = 1,
            FacultyId = uni.Faculties.First().FacultyId,
        };
        db.Students.Add(st);
        db.SaveChanges();
        var tx = new ScoreTransaction
        {
            StudentId = st.StudentId, Points = 10, Reason = "r", SourceType = ScoreSourceType.Manual,
            CreatedAt = DateTime.UtcNow,
        };
        db.ScoreTransactions.Add(tx);
        db.SaveChanges();

        db.ScoreTransactions.Remove(tx);
        Assert.Throws<InvalidOperationException>(() => db.SaveChanges());
    }
}
