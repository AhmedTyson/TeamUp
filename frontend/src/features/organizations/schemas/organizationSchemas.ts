/**
 * @date 2026-11-24
 * @purpose Zod validation schemas and Egyptian ecosystem constants aligned with OrganizationValidators.
 */
import { ORGANIZATION_SCOPES, ORGANIZATION_UNIT_TYPES } from "@/api";
import { optionalSafeAssetOrUrlSchema, optionalSafeUrlSchema } from "@/lib/urlValidation";
import { z } from "@/lib/zodCspConfig";

export const ORGANIZATION_CATEGORIES = [
  "Engineering",
  "Technology",
  "AI & Robotics",
  "Entrepreneurship",
  "Leadership",
  "Technical",
  "Developer",
  "Robotics",
  "Design",
  "Business",
  "Science",
  "Other",
] as const;

export const ORGANIZATION_SCOPE_VALUES = [
  ORGANIZATION_SCOPES.UniversityBranch,
  ORGANIZATION_SCOPES.FacultyChapter,
  ORGANIZATION_SCOPES.CrossUniversity,
  ORGANIZATION_SCOPES.Independent,
  ORGANIZATION_SCOPES.NationalNetwork,
] as const;

export const ORGANIZATION_SCOPE_OPTIONS = [
  { value: ORGANIZATION_SCOPES.UniversityBranch, label: "University Student Branch (e.g. IEEE SB)" },
  { value: ORGANIZATION_SCOPES.FacultyChapter, label: "Faculty Chapter (e.g. FCAI / BIS Club)" },
  { value: ORGANIZATION_SCOPES.CrossUniversity, label: "Cross-University Initiative (e.g. STP Egypt)" },
  { value: ORGANIZATION_SCOPES.Independent, label: "Open / Independent Community (e.g. GDG)" },
  { value: ORGANIZATION_SCOPES.NationalNetwork, label: "National Network" },
] as const;

export const ORGANIZATION_UNIT_TYPE_VALUES = [
  ORGANIZATION_UNIT_TYPES.TechnicalChapter,
  ORGANIZATION_UNIT_TYPES.OperationalCommittee,
  ORGANIZATION_UNIT_TYPES.AffinityGroup,
  ORGANIZATION_UNIT_TYPES.WorkingGroup,
] as const;

export const organizationFormSchema = z.object({
  name: z
    .string()
    .trim()
    .min(2, "Organization name must be at least 2 characters.")
    .max(120, "Organization name cannot exceed 120 characters."),
  description: z
    .string()
    .trim()
    .min(10, "Description must be at least 10 characters.")
    .max(4000, "Description cannot exceed 4000 characters."),
  category: z
    .string()
    .trim()
    .min(2, "Category must be at least 2 characters.")
    .max(50, "Category cannot exceed 50 characters."),
  logoUrl: optionalSafeAssetOrUrlSchema.refine(
    (val) => !val || val.length <= 300,
    "Logo URL cannot exceed 300 characters.",
  ),
  scope: z.enum(ORGANIZATION_SCOPE_VALUES),
  universityId: z.string().trim().optional().or(z.literal("")),
  city: z.string().trim().max(100, "City cannot exceed 100 characters.").optional().or(z.literal("")),
  websiteUrl: optionalSafeUrlSchema.refine(
    (val) => !val || val.length <= 300,
    "Website URL cannot exceed 300 characters.",
  ),
  isRecruiting: z.boolean(),
  currentSeasonName: z
    .string()
    .trim()
    .max(80, "Season name cannot exceed 80 characters.")
    .optional()
    .or(z.literal("")),
});

export type OrganizationFormValues = z.infer<typeof organizationFormSchema>;

export const organizationUnitSchema = z.object({
  name: z
    .string()
    .trim()
    .min(2, "Unit name must be at least 2 characters.")
    .max(100, "Unit name cannot exceed 100 characters."),
  code: z
    .string()
    .trim()
    .min(2, "Code must be 2-20 uppercase characters.")
    .max(20, "Code cannot exceed 20 characters."),
  description: z
    .string()
    .trim()
    .max(600, "Description cannot exceed 600 characters.")
    .optional()
    .or(z.literal("")),
  unitType: z.enum(ORGANIZATION_UNIT_TYPE_VALUES),
  isRecruiting: z.boolean(),
  openSeats: z
    .string()
    .trim()
    .optional()
    .or(z.literal("")),
});

export type OrganizationUnitFormValues = z.infer<typeof organizationUnitSchema>;

export const membershipApplicationSchema = z.object({
  organizationUnitId: z.string().trim().optional().or(z.literal("")),
  message: z
    .string()
    .trim()
    .max(600, "Application message cannot exceed 600 characters.")
    .optional()
    .or(z.literal("")),
});

export type MembershipApplicationValues = z.infer<typeof membershipApplicationSchema>;

export const proposalFormSchema = z.object({
  name: z.string().trim().min(2, "Organization name is required.").max(120),
  category: z.string().trim().min(2, "Category is required.").max(50),
  scope: z.enum(ORGANIZATION_SCOPE_VALUES),
  universityId: z.string().optional(),
  city: z.string().trim().max(100).optional(),
  description: z
    .string()
    .trim()
    .min(10, "Please describe the club mission (at least 10 characters).")
    .max(4000),
  websiteUrl: optionalSafeUrlSchema.refine(
    (v) => !v || v.length <= 300,
    "Website URL cannot exceed 300 characters.",
  ),
  logoUrl: optionalSafeAssetOrUrlSchema.refine(
    (v) => !v || v.length <= 300,
    "Logo URL cannot exceed 300 characters.",
  ),
  currentSeasonName: z.string().trim().max(80).optional(),
  justification: z.string().trim().max(1000).optional(),
});

export type ProposalFormValues = z.infer<typeof proposalFormSchema>;
