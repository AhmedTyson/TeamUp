// Date: 2026-11-24
// Purpose: Null-safe student academic and open-community affiliation formatting helpers.

using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Common;

public static class AffiliationFormatting
{
    public static string FormatAffiliationLabel(AffiliationType type) => type switch
    {
        AffiliationType.UniversityStudent => "University Student",
        AffiliationType.IndependentLearner => "Independent Learner",
        AffiliationType.FreshGraduate => "Fresh Graduate",
        AffiliationType.HighSchoolTech => "High School Tech",
        _ => "Community Member",
    };

    public static string GetFacultyDisplay(Student student) =>
        student.Faculty?.Name ?? FormatAffiliationLabel(student.AffiliationType);

    public static string GetFacultyCodeDisplay(Student student) =>
        student.Faculty?.Code ?? "IND";

    public static string GetUniversityDisplay(Student student) =>
        student.Faculty?.University?.Name
        ?? (string.IsNullOrWhiteSpace(student.City) ? "Egypt Open Community" : $"{student.City.Trim()} Community");
}
