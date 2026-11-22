/**
 * @date 2026-11-22
 * @purpose Zod validation schemas for Student Account and Extended Portfolio Studio (STRIDE THR-T-03 safe URLs).
 */
import { z } from "zod";

const safeHttpUrlSchema = z
  .string()
  .trim()
  .optional()
  .or(z.literal(""))
  .refine(
    (val) => {
      if (!val || val.length === 0) return true;
      try {
        const parsed = new URL(val);
        return parsed.protocol === "http:" || parsed.protocol === "https:";
      } catch {
        return false;
      }
    },
    { message: "Must be a valid safe URL starting with http:// or https://" },
  );

export const updateAccountSchema = z.object({
  firstName: z.string().trim().min(2, "First name must be at least 2 characters."),
  lastName: z.string().trim().min(2, "Last name must be at least 2 characters."),
  academicYear: z
    .number({ message: "Academic year must be an integer." })
    .int("Academic year must be an integer.")
    .min(1, "Academic year must be between 1 and 6.")
    .max(6, "Academic year must be between 1 and 6."),
  showEmailInDirectory: z.boolean(),
});

export type UpdateAccountFormValues = z.infer<typeof updateAccountSchema>;

export const upsertProfileSchema = z.object({
  bio: z.string().max(500, "Bio cannot exceed 500 characters.").optional().or(z.literal("")),
  photoUrl: safeHttpUrlSchema,
  gitHubUrl: safeHttpUrlSchema,
  linkedInUrl: safeHttpUrlSchema,
});

export type UpsertProfileFormValues = z.infer<typeof upsertProfileSchema>;
