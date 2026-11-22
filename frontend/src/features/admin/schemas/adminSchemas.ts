/**
 * @date 2026-11-22
 * @purpose Zod schemas for PlatformAdmin catalog CRUD and role governance forms mirroring backend FluentValidation + STRIDE THR-T-03 rules.
 */
import { z } from "zod";
import { USER_ROLES } from "@/api";
import { isSafeAssetOrHttpUrl, isSafeHttpUrl } from "@/lib/urlValidation";

export const universityFormSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "University name is required.")
    .max(150, "University name must not exceed 150 characters."),
  city: z
    .string()
    .trim()
    .min(1, "City is required.")
    .max(100, "City must not exceed 100 characters."),
  country: z
    .string()
    .trim()
    .min(1, "Country is required.")
    .max(100, "Country must not exceed 100 characters."),
  website: z
    .string()
    .trim()
    .max(200, "Website must not exceed 200 characters.")
    .refine(
      (val) => val.length === 0 || isSafeHttpUrl(val),
      "Website must be a valid public HTTP or HTTPS URL.",
    ),
  logoUrl: z
    .string()
    .trim()
    .max(500, "Logo URL must not exceed 500 characters.")
    .refine(
      (val) => val.length === 0 || isSafeAssetOrHttpUrl(val),
      "Logo URL must be a valid relative asset path (/logos/...) or public HTTP/HTTPS URL.",
    )
    .optional(),
});

export type UniversityFormValues = z.infer<typeof universityFormSchema>;

export const facultyFormSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Faculty name is required.")
    .max(150, "Faculty name must not exceed 150 characters."),
  code: z
    .string()
    .trim()
    .min(1, "Faculty academic code is required.")
    .max(20, "Faculty code must not exceed 20 characters.")
    .regex(
      /^[A-Za-z0-9_-]+$/,
      "Faculty code may only contain letters, numbers, underscores, and dashes.",
    ),
});

export type FacultyFormValues = z.infer<typeof facultyFormSchema>;

export const roleUpdateSchema = z.object({
  studentId: z
    .string()
    .trim()
    .refine((value) => {
      const parsed = Number.parseInt(value, 10);
      return Number.isInteger(parsed) && parsed > 0;
    }, "A valid positive Student ID is required."),
  role: z.enum([USER_ROLES.Student, USER_ROLES.OrgAdmin, USER_ROLES.PlatformAdmin]),
});

export type RoleUpdateFormValues = z.infer<typeof roleUpdateSchema>;
