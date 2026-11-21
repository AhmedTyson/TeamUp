using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Events;
using TeamUp.Application.Organizations;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.IntegrationTests;

public sealed class EventsAndAttendanceTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private HttpClient Client(int studentId, string role)
    {
        var client = factory.CreateClient();
        var token = factory.CreateToken(studentId, $"user{studentId}@test.com", role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static readonly System.Text.Json.JsonSerializerOptions JsonOpts = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private static async Task<T> Data<T>(HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<BaseResponse<T>>(JsonOpts);
        Assert.NotNull(envelope);
        Assert.True(envelope.Success);
        Assert.NotNull(envelope.Data);
        return envelope.Data;
    }


    [Fact]
    public async Task EventManagementEndpoints_EnforceAuthenticationAndOrganizerRole()
    {
        using var anonymous = factory.CreateClient();
        var anonymousResponse = await anonymous.GetAsync("/api/v1/events");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        using var student = Client(303, "Student");
        var createEvent = new CreateEventRequest("Forbidden Event", "Description", EventType.Workshop,
            "Campus", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, 1);
        var createResponse = await student.PostAsJsonAsync("/api/v1/events", createEvent);
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);

        var attendanceResponse = await student.GetAsync("/api/v1/sessions/1/attendance");
        Assert.Equal(HttpStatusCode.Forbidden, attendanceResponse.StatusCode);
    }

    [Fact]
    public async Task EventRegistrationWaitlistAndSessionAttendance_WorkEndToEnd()
    {
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            foreach (var id in new[] { 301, 302 })
            {
                db.Students.Add(new Student
                {
                    StudentId = id,
                    Email = $"event{id}@test.com",
                    PasswordHash = "test-hash",
                    FirstName = $"Student{id}",
                    LastName = "Test",
                    AcademicYear = 2,
                    FacultyId = factory.SeededFacultyId
                });
            }
            await db.SaveChangesAsync();
        }

        using var orgAdmin = Client(900, "OrgAdmin");
        var orgRequest = new CreateOrganizationRequest($"EventOrg_{Guid.NewGuid():N}", "Events host", "Technology", null);
        var orgResponse = await orgAdmin.PostAsJsonAsync("/api/v1/organizations", orgRequest);
        Assert.Equal(HttpStatusCode.Created, orgResponse.StatusCode);
        var organization = await Data<OrganizationDetailDto>(orgResponse);

        var eventStart = DateTime.UtcNow.Date.AddDays(5);
        var eventRequest = new CreateEventRequest("TeamUp Workshop", "Workshop description", EventType.Workshop,
            "Helwan University", eventStart, eventStart.AddDays(1), 1, organization.OrganizationId);
        var eventResponse = await orgAdmin.PostAsJsonAsync("/api/v1/events", eventRequest);
        Assert.Equal(HttpStatusCode.Created, eventResponse.StatusCode);
        var createdEvent = await Data<EventDto>(eventResponse);

        var sessionRequest = new CreateEventSessionRequest(1, "Opening", eventStart.AddHours(1), eventStart.AddHours(2), "Hall A");
        var sessionResponse = await orgAdmin.PostAsJsonAsync($"/api/v1/events/{createdEvent.EventId}/sessions", sessionRequest);
        Assert.Equal(HttpStatusCode.Created, sessionResponse.StatusCode);
        var session = await Data<EventSessionDto>(sessionResponse);

        using var firstStudent = Client(301, "Student");
        using var secondStudent = Client(302, "Student");
        var firstRegistration = await firstStudent.PostAsync($"/api/v1/events/{createdEvent.EventId}/registrations", null);
        Assert.Equal(HttpStatusCode.Created, firstRegistration.StatusCode);
        var first = await Data<EventRegistrationDto>(firstRegistration);
        Assert.Equal(RegistrationStatus.Confirmed, first.Status);

        var secondRegistration = await secondStudent.PostAsync($"/api/v1/events/{createdEvent.EventId}/registrations", null);
        Assert.Equal(HttpStatusCode.Created, secondRegistration.StatusCode);
        var second = await Data<EventRegistrationDto>(secondRegistration);
        Assert.Equal(RegistrationStatus.Waitlisted, second.Status);

        var cancelFirst = await firstStudent.DeleteAsync($"/api/v1/events/{createdEvent.EventId}/registrations");
        Assert.Equal(HttpStatusCode.NoContent, cancelFirst.StatusCode);

        var registrationsResponse = await orgAdmin.GetAsync($"/api/v1/events/{createdEvent.EventId}/registrations");
        var registrations = await Data<List<EventRegistrationDto>>(registrationsResponse);
        Assert.Contains(registrations, x => x.StudentId == 302 && x.Status == RegistrationStatus.Confirmed);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cancelled = await db.EventRegistrations.IgnoreQueryFilters()
                .SingleAsync(x => x.EventId == createdEvent.EventId && x.StudentId == 301);
            Assert.NotNull(cancelled.DeletedAt);
            Assert.Equal(RegistrationStatus.Cancelled, cancelled.Status);
        }

        var reRegister = await firstStudent.PostAsync($"/api/v1/events/{createdEvent.EventId}/registrations", null);
        Assert.Equal(HttpStatusCode.Created, reRegister.StatusCode);
        var reRegistered = await Data<EventRegistrationDto>(reRegister);
        Assert.Equal(RegistrationStatus.Waitlisted, reRegistered.Status);

        var attendanceRequest = new CreateAttendanceRequest(302, VerificationMethod.ManualByOrganizer);
        var checkInResponse = await orgAdmin.PostAsJsonAsync($"/api/v1/sessions/{session.SessionId}/attendance", attendanceRequest);
        Assert.Equal(HttpStatusCode.Created, checkInResponse.StatusCode);

        var duplicateCheckIn = await orgAdmin.PostAsJsonAsync($"/api/v1/sessions/{session.SessionId}/attendance", attendanceRequest);
        Assert.Equal(HttpStatusCode.Conflict, duplicateCheckIn.StatusCode);

        var revokeResponse = await orgAdmin.DeleteAsync($"/api/v1/sessions/{session.SessionId}/attendance/302");
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        var deleteEvent = await orgAdmin.DeleteAsync($"/api/v1/events/{createdEvent.EventId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteEvent.StatusCode);
        var deletedSessions = await orgAdmin.GetAsync($"/api/v1/events/{createdEvent.EventId}/sessions");
        Assert.Equal(HttpStatusCode.NotFound, deletedSessions.StatusCode);
    }
}
