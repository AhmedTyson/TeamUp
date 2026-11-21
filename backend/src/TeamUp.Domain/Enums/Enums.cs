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

public enum ScoreSourceType { EventAttendance, TaskCompleted, ProjectDelivered, Manual }

public enum BadgeCriteriaType { PointsThreshold, EventCount, TaskCount }

public enum NotificationType { ApplicationStatus, TaskAssigned, BadgeEarned, EventReminder }
