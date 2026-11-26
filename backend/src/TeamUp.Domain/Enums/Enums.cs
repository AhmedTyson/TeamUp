using System.Text.Json.Serialization;

namespace TeamUp.Domain.Enums;

public enum UserRole { Student, OrgAdmin, PlatformAdmin }

public enum ProficiencyLevel { Beginner, Intermediate, Advanced }

public enum ProjectStatus { Recruiting, InProgress, Completed, Archived }

public enum ApplicationStatus { Pending, Accepted, Rejected, Withdrawn }

public enum ProjectTaskStatus { Todo, InProgress, InReview, Done }

public enum TaskPriority { Low, Medium, High, Urgent }

public enum EventType { Workshop, Hackathon, Session, Competition }

public enum RegistrationStatus { Confirmed, Waitlisted, Cancelled }

public enum VerificationMethod { QRCode, RFID, ManualByOrganizer }

public enum ScoreSourceType { EventAttendance, TaskCompleted, ProjectDelivered, Manual, AttendanceRevocation }

public enum BadgeCriteriaType { PointsThreshold, EventCount, TaskCount }

public enum NotificationType { ApplicationStatus, TaskAssigned, BadgeEarned, EventReminder }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AffiliationType { UniversityStudent, IndependentLearner, FreshGraduate, HighSchoolTech }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrganizationScope { UniversityBranch, FacultyChapter, CrossUniversity, Independent, NationalNetwork }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrganizationUnitType { TechnicalChapter, OperationalCommittee, AffinityGroup, WorkingGroup }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrganizationHierarchyTier { HighBoard, CommitteeLeadership, OrganizingCrew, Participant }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RecruitmentStage { Applied, HrScreening, CommitteeInterview, Accepted, Rejected, Withdrawn }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventDeliveryMode { InPerson, Online, Hybrid }
