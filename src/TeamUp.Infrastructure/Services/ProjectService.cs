using System.Data;
using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Projects;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Infrastructure.Services;

public sealed class ProjectService(AppDbContext db) : IProjectService
{
    public async Task<PagedResult<ProjectListItemDto>> GetProjectsAsync(string? keyword, string? status, int? skillId, PageRequest page, CancellationToken ct)
    {
        var query = db.Projects.AsNoTracking().Include(p => p.Creator).Include(p => p.Members).AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim();
            query = query.Where(p => p.Title.Contains(term) || p.Description.Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<ProjectStatus>(status, true, out var parsed) || !Enum.IsDefined(parsed))
                throw new BadRequestException("Invalid project status.");
            query = query.Where(p => p.Status == parsed);
        }
        if (skillId.HasValue)
            query = query.Where(p => db.ProjectRequiredSkills.Any(s => s.ProjectId == p.ProjectId && s.SkillId == skillId.Value));

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(p => p.CreatedAt).Skip(page.Skip).Take(page.SafePageSize)
            .Select(p => new ProjectListItemDto(p.ProjectId, p.Title, p.Description, p.Status, p.MaxTeamSize,
                p.Members.Count(m => m.DeletedAt == null), p.CreatorId, p.Creator.FirstName + " " + p.Creator.LastName, p.CreatedAt))
            .ToListAsync(ct);
        return new PagedResult<ProjectListItemDto>(items, page.SafePage, page.SafePageSize, total);
    }

    public async Task<ProjectDetailDto> CreateProjectAsync(int studentId, CreateProjectRequest request, CancellationToken ct)
    {
        ValidateText(request.Title, 150, "Title");
        if (string.IsNullOrWhiteSpace(request.Description)) throw new BadRequestException("Description is required.");
        if (request.MaxTeamSize < 1) throw new BadRequestException("MaxTeamSize must be at least 1.");
        await EnsureStudentAsync(studentId, ct);
        var project = new Project { Title = request.Title.Trim(), Description = request.Description.Trim(), MaxTeamSize = request.MaxTeamSize, CreatorId = studentId, Status = ProjectStatus.Recruiting };
        project.Members.Add(new ProjectMember { StudentId = studentId, Role = "Team Lead" });
        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);
        return await GetProjectAsync(project.ProjectId, ct);
    }

    public async Task<ProjectDetailDto> GetProjectAsync(int projectId, CancellationToken ct)
    {
        var project = await db.Projects.AsNoTracking().Include(p => p.Creator).Include(p => p.Members).ThenInclude(m => m.Student)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId, ct) ?? throw new NotFoundException("Project not found.");
        var skills = await GetRequiredSkillsAsync(projectId, ct);
        var members = project.Members.Where(m => m.DeletedAt == null).Select(m => new ProjectMemberDto(m.StudentId, m.Student.FirstName, m.Student.LastName, m.Role, m.JoinedAt)).ToList();
        return new ProjectDetailDto(project.ProjectId, project.Title, project.Description, project.Status, project.MaxTeamSize,
            members.Count, project.CreatorId, project.Creator.FirstName + " " + project.Creator.LastName, project.CreatedAt, members, skills);
    }

    public async Task<ProjectDetailDto> UpdateProjectAsync(int studentId, int projectId, UpdateProjectRequest request, CancellationToken ct)
    {
        var project = await GetProjectEntityAsync(projectId, ct);
        EnsureCreator(project, studentId);
        if (request.Title is not null) { ValidateText(request.Title, 150, "Title"); project.Title = request.Title.Trim(); }
        if (request.Description is not null) { if (string.IsNullOrWhiteSpace(request.Description)) throw new BadRequestException("Description cannot be empty."); project.Description = request.Description.Trim(); }
        if (request.MaxTeamSize.HasValue)
        {
            if (request.MaxTeamSize.Value < 1) throw new BadRequestException("MaxTeamSize must be at least 1.");
            var currentSize = await db.ProjectMembers.CountAsync(m => m.ProjectId == projectId, ct);
            if (request.MaxTeamSize.Value < currentSize) throw new ConflictException("MaxTeamSize cannot be lower than the current team size.");
            project.MaxTeamSize = request.MaxTeamSize.Value;
        }
        if (request.Status.HasValue)
        {
            if (!Enum.IsDefined(request.Status.Value)) throw new BadRequestException("Invalid project status.");
            project.Status = request.Status.Value;
        }
        await db.SaveChangesAsync(ct);
        return await GetProjectAsync(projectId, ct);
    }

    public async Task DeleteProjectAsync(int studentId, int projectId, CancellationToken ct)
    {
        var project = await GetProjectEntityAsync(projectId, ct);
        EnsureCreator(project, studentId);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var now = DateTime.UtcNow;
        var tasks = await db.ProjectTasks.Where(t => t.ProjectId == projectId).ToListAsync(ct);
        foreach (var task in tasks) task.DeletedAt = now;
        project.DeletedAt = now;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<ProjectSkillDto>> GetRequiredSkillsAsync(int projectId, CancellationToken ct)
    {
        await EnsureProjectExistsAsync(projectId, ct);
        return await db.ProjectRequiredSkills.AsNoTracking().Where(x => x.ProjectId == projectId).Include(x => x.Skill)
            .OrderBy(x => x.Skill.Name).Select(x => new ProjectSkillDto(x.SkillId, x.Skill.Name, x.Skill.Category, x.RequiredLevel, x.IsMandatory)).ToListAsync(ct);
    }

    public async Task UpsertRequiredSkillAsync(int studentId, int projectId, int skillId, UpsertRequiredSkillRequest request, CancellationToken ct)
    {
        var project = await GetProjectEntityAsync(projectId, ct); EnsureCreator(project, studentId);
        if (!Enum.IsDefined(request.RequiredLevel)) throw new BadRequestException("Invalid required proficiency level.");
        if (!await db.Skills.AnyAsync(s => s.SkillId == skillId, ct)) throw new NotFoundException("Skill not found.");
        var item = await db.ProjectRequiredSkills.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.ProjectId == projectId && x.SkillId == skillId, ct);
        if (item is null) db.ProjectRequiredSkills.Add(new ProjectRequiredSkill { ProjectId = projectId, SkillId = skillId, RequiredLevel = request.RequiredLevel, IsMandatory = request.IsMandatory });
        else { item.DeletedAt = null; item.RequiredLevel = request.RequiredLevel; item.IsMandatory = request.IsMandatory; }
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveRequiredSkillAsync(int studentId, int projectId, int skillId, CancellationToken ct)
    {
        var project = await GetProjectEntityAsync(projectId, ct); EnsureCreator(project, studentId);
        var item = await db.ProjectRequiredSkills.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.SkillId == skillId, ct)
            ?? throw new NotFoundException("Required skill not found.");
        item.DeletedAt = DateTime.UtcNow; await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ProjectMemberDto>> GetMembersAsync(int projectId, CancellationToken ct)
    {
        await EnsureProjectExistsAsync(projectId, ct);
        return await db.ProjectMembers.AsNoTracking().Where(m => m.ProjectId == projectId).Include(m => m.Student)
            .OrderBy(m => m.JoinedAt).Select(m => new ProjectMemberDto(m.StudentId, m.Student.FirstName, m.Student.LastName, m.Role, m.JoinedAt)).ToListAsync(ct);
    }

    public async Task UpdateMemberRoleAsync(int studentId, int projectId, int memberStudentId, UpdateProjectMemberRequest request, CancellationToken ct)
    {
        var project = await GetProjectEntityAsync(projectId, ct); EnsureCreator(project, studentId);
        ValidateText(request.Role, 60, "Role");
        var member = await db.ProjectMembers.FirstOrDefaultAsync(m => m.ProjectId == projectId && m.StudentId == memberStudentId, ct)
            ?? throw new NotFoundException("Project member not found.");
        member.Role = request.Role.Trim(); await db.SaveChangesAsync(ct);
    }

    public async Task RemoveMemberAsync(int studentId, int projectId, int memberStudentId, CancellationToken ct)
    {
        var project = await GetProjectEntityAsync(projectId, ct);
        if (studentId != project.CreatorId && studentId != memberStudentId) throw new UnauthorizedException("Only the project creator or the member themselves can remove this member.");
        if (memberStudentId == project.CreatorId) throw new ConflictException("The project creator cannot leave or be removed from their own project.");
        var member = await db.ProjectMembers.FirstOrDefaultAsync(m => m.ProjectId == projectId && m.StudentId == memberStudentId, ct)
            ?? throw new NotFoundException("Project member not found.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        member.DeletedAt = DateTime.UtcNow;
        var assignedTasks = await db.ProjectTasks.Where(t => t.ProjectId == projectId && t.AssignedStudentId == memberStudentId).ToListAsync(ct);
        foreach (var task in assignedTasks) task.AssignedStudentId = null;
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<ProjectApplicationDto>> GetApplicationsAsync(int studentId, int projectId, CancellationToken ct)
    {
        var project = await GetProjectEntityAsync(projectId, ct); EnsureCreator(project, studentId);
        var applications = await db.ProjectApplications.AsNoTracking().Where(a => a.ProjectId == projectId).Include(a => a.Student)
            .OrderByDescending(a => a.AppliedAt).ToListAsync(ct);
        return applications.Select(ToApplicationDto).ToList();
    }

    public async Task<ProjectApplicationDto> ApplyAsync(int studentId, int projectId, CreateProjectApplicationRequest request, CancellationToken ct)
    {
        var project = await GetProjectEntityAsync(projectId, ct);
        await EnsureStudentAsync(studentId, ct);
        if (project.CreatorId == studentId || await db.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.StudentId == studentId, ct))
            throw new ConflictException("You are already part of this project.");
        if (project.Status != ProjectStatus.Recruiting) throw new ConflictException("This project is not recruiting.");
        if (await db.ProjectApplications.AnyAsync(a => a.ProjectId == projectId && a.StudentId == studentId && a.Status == ApplicationStatus.Pending, ct))
            throw new ConflictException("You already have a pending application for this project.");
        if (request.Message?.Length > 500) throw new BadRequestException("Application message cannot exceed 500 characters.");
        var application = new ProjectApplication { ProjectId = projectId, StudentId = studentId, Message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim() };
        db.ProjectApplications.Add(application); await db.SaveChangesAsync(ct);
        await db.Entry(application).Reference(a => a.Student).LoadAsync(ct);
        return ToApplicationDto(application);
    }

    public async Task<ProjectApplicationDto> ReviewApplicationAsync(int studentId, int projectId, int applicationId, ReviewProjectApplicationRequest request, CancellationToken ct)
    {
        var project = await GetProjectEntityAsync(projectId, ct); EnsureCreator(project, studentId);
        if (request.Status is not (ApplicationStatus.Accepted or ApplicationStatus.Rejected)) throw new BadRequestException("Application status must be Accepted or Rejected.");
        var application = await db.ProjectApplications.Include(a => a.Student).FirstOrDefaultAsync(a => a.ApplicationId == applicationId && a.ProjectId == projectId, ct)
            ?? throw new NotFoundException("Application not found.");
        if (application.Status != ApplicationStatus.Pending) throw new ConflictException("Only pending applications can be reviewed.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (request.Status == ApplicationStatus.Accepted)
        {
            if (project.Status != ProjectStatus.Recruiting) throw new ConflictException("This project is not recruiting.");
            var memberCount = await db.ProjectMembers.CountAsync(m => m.ProjectId == projectId, ct);
            if (memberCount >= project.MaxTeamSize) throw new ConflictException("The project has reached its maximum team size.");
            var existingMembership = await db.ProjectMembers.IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.StudentId == application.StudentId, ct);
            if (existingMembership is not null && existingMembership.DeletedAt is null)
                throw new ConflictException("Applicant is already a project member.");
            if (existingMembership is null)
                db.ProjectMembers.Add(new ProjectMember { ProjectId = projectId, StudentId = application.StudentId, Role = "Member" });
            else
            {
                existingMembership.DeletedAt = null;
                existingMembership.Role = "Member";
                existingMembership.JoinedAt = DateTime.UtcNow;
            }
        }
        application.Status = request.Status;
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return ToApplicationDto(application);
    }

    public async Task WithdrawApplicationAsync(int studentId, int projectId, int applicationId, CancellationToken ct)
    {
        var application = await db.ProjectApplications.FirstOrDefaultAsync(a => a.ApplicationId == applicationId && a.ProjectId == projectId, ct)
            ?? throw new NotFoundException("Application not found.");
        if (application.StudentId != studentId) throw new UnauthorizedException("You can only withdraw your own application.");
        if (application.Status != ApplicationStatus.Pending) throw new ConflictException("Only pending applications can be withdrawn.");
        application.Status = ApplicationStatus.Withdrawn;
        application.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ProjectTaskDto>> GetProjectTasksAsync(int studentId, int projectId, CancellationToken ct)
    {
        await EnsureMemberAsync(studentId, projectId, ct);
        var tasks = await db.ProjectTasks.AsNoTracking().Where(t => t.ProjectId == projectId).Include(t => t.AssignedStudent)
            .OrderBy(t => t.DueDate).ThenBy(t => t.TaskId).ToListAsync(ct);
        return tasks.Select(ToTaskDto).ToList();
    }

    public async Task<ProjectTaskDto> CreateTaskAsync(int studentId, int projectId, CreateProjectTaskRequest request, CancellationToken ct)
    {
        await EnsureMemberAsync(studentId, projectId, ct); ValidateText(request.Title, 150, "Title");
        if (!Enum.IsDefined(request.Priority)) throw new BadRequestException("Invalid task priority.");
        if (request.Description?.Length > 1000) throw new BadRequestException("Description cannot exceed 1000 characters.");
        var task = new ProjectTask { ProjectId = projectId, Title = request.Title.Trim(), Description = request.Description, DueDate = request.DueDate, Priority = request.Priority };
        db.ProjectTasks.Add(task); await db.SaveChangesAsync(ct); return ToTaskDto(task);
    }

    public async Task<ProjectTaskDto> GetTaskAsync(int studentId, int taskId, CancellationToken ct)
    {
        var task = await db.ProjectTasks.AsNoTracking().Include(t => t.AssignedStudent).FirstOrDefaultAsync(t => t.TaskId == taskId, ct)
            ?? throw new NotFoundException("Task not found.");
        await EnsureMemberAsync(studentId, task.ProjectId, ct); return ToTaskDto(task);
    }

    public async Task<ProjectTaskDto> UpdateTaskAsync(int studentId, int taskId, UpdateProjectTaskRequest request, CancellationToken ct)
    {
        var task = await db.ProjectTasks.Include(t => t.AssignedStudent).FirstOrDefaultAsync(t => t.TaskId == taskId, ct)
            ?? throw new NotFoundException("Task not found.");
        await EnsureMemberAsync(studentId, task.ProjectId, ct);
        if (request.Title is not null) { ValidateText(request.Title, 150, "Title"); task.Title = request.Title.Trim(); }
        if (request.Description is not null) { if (request.Description.Length > 1000) throw new BadRequestException("Description cannot exceed 1000 characters."); task.Description = request.Description; }
        if (request.Status.HasValue)
        {
            if (!Enum.IsDefined(request.Status.Value)) throw new BadRequestException("Invalid task status.");
            task.Status = request.Status.Value;
        }
        if (request.Priority.HasValue)
        {
            if (!Enum.IsDefined(request.Priority.Value)) throw new BadRequestException("Invalid task priority.");
            task.Priority = request.Priority.Value;
        }
        if (request.DueDate.HasValue) task.DueDate = request.DueDate.Value;
        await db.SaveChangesAsync(ct); return ToTaskDto(task);
    }

    public async Task AssignTaskAsync(int studentId, int taskId, AssignTaskRequest request, CancellationToken ct)
    {
        var task = await db.ProjectTasks.FirstOrDefaultAsync(t => t.TaskId == taskId, ct) ?? throw new NotFoundException("Task not found.");
        var project = await GetProjectEntityAsync(task.ProjectId, ct); EnsureCreator(project, studentId);
        if (!await db.ProjectMembers.AnyAsync(m => m.ProjectId == task.ProjectId && m.StudentId == request.StudentId, ct))
            throw new BadRequestException("Task can only be assigned to a confirmed project member.");
        task.AssignedStudentId = request.StudentId; await db.SaveChangesAsync(ct);
    }

    public async Task UnassignTaskAsync(int studentId, int taskId, CancellationToken ct)
    {
        var task = await db.ProjectTasks.FirstOrDefaultAsync(t => t.TaskId == taskId, ct) ?? throw new NotFoundException("Task not found.");
        var project = await GetProjectEntityAsync(task.ProjectId, ct); EnsureCreator(project, studentId);
        task.AssignedStudentId = null; await db.SaveChangesAsync(ct);
    }

    public async Task DeleteTaskAsync(int studentId, int taskId, CancellationToken ct)
    {
        var task = await db.ProjectTasks.FirstOrDefaultAsync(t => t.TaskId == taskId, ct) ?? throw new NotFoundException("Task not found.");
        var project = await GetProjectEntityAsync(task.ProjectId, ct); EnsureCreator(project, studentId);
        task.DeletedAt = DateTime.UtcNow; await db.SaveChangesAsync(ct);
    }

    private async Task<Project> GetProjectEntityAsync(int id, CancellationToken ct) => await db.Projects.FirstOrDefaultAsync(p => p.ProjectId == id, ct) ?? throw new NotFoundException("Project not found.");
    private async Task EnsureProjectExistsAsync(int id, CancellationToken ct) { if (!await db.Projects.AnyAsync(p => p.ProjectId == id, ct)) throw new NotFoundException("Project not found."); }
    private async Task EnsureStudentAsync(int id, CancellationToken ct) { if (!await db.Students.AnyAsync(s => s.StudentId == id, ct)) throw new NotFoundException("Student not found."); }
    private async Task EnsureMemberAsync(int studentId, int projectId, CancellationToken ct)
    {
        await EnsureProjectExistsAsync(projectId, ct);
        if (!await db.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.StudentId == studentId, ct)) throw new UnauthorizedException("You must be a confirmed project member to access its tasks.");
    }
    private static void EnsureCreator(Project project, int studentId) { if (project.CreatorId != studentId) throw new UnauthorizedException("Only the project creator can perform this action."); }
    private static void ValidateText(string value, int max, string field) { if (string.IsNullOrWhiteSpace(value)) throw new BadRequestException($"{field} is required."); if (value.Trim().Length > max) throw new BadRequestException($"{field} cannot exceed {max} characters."); }
    private static ProjectApplicationDto ToApplicationDto(ProjectApplication a) => new(a.ApplicationId, a.ProjectId, a.StudentId, a.Student.FirstName + " " + a.Student.LastName, a.Message, a.Status, a.AppliedAt);
    private static ProjectTaskDto ToTaskDto(ProjectTask t) => new(t.TaskId, t.Title, t.Description, t.Status, t.Priority, t.DueDate, t.ProjectId, t.AssignedStudentId, t.AssignedStudent is null ? null : t.AssignedStudent.FirstName + " " + t.AssignedStudent.LastName);
}
