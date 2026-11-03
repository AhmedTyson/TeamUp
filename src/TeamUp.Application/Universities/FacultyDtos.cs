namespace TeamUp.Application.Universities;

public sealed record CreateFacultyRequest(
    string Name,
    string Code
);

public sealed record UpdateFacultyRequest(
    string Name,
    string Code
);

public sealed record FacultyDto(
    int FacultyId,
    string Name,
    string Code,
    int UniversityId,
    string UniversityName,
    int StudentCount
);
