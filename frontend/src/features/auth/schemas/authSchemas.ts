/**
 * @date 2026-11-22
 * @purpose Zod validation schemas and password strength evaluator matching backend FluentValidation rules.
 */
import { z } from "zod";

export const loginSchema = z.object({
  email: z
    .string()
    .trim()
    .min(1, "Email address is required.")
    .email("A valid email address is required."),
  password: z.string().min(1, "Password is required."),
});

export type LoginFormValues = z.infer<typeof loginSchema>;

export const registerSchema = z.object({
  firstName: z
    .string()
    .trim()
    .min(1, "First name is required.")
    .max(50, "First name must not exceed 50 characters."),
  lastName: z
    .string()
    .trim()
    .min(1, "Last name is required.")
    .max(50, "Last name must not exceed 50 characters."),
  email: z
    .string()
    .trim()
    .min(1, "Email address is required.")
    .email("A valid email address is required.")
    .max(150, "Email must not exceed 150 characters."),
  password: z
    .string()
    .min(8, "Password must be at least 8 characters long.")
    .regex(/[A-Z]/, "Password must contain at least one uppercase letter.")
    .regex(/[a-z]/, "Password must contain at least one lowercase letter.")
    .regex(/[0-9]/, "Password must contain at least one digit."),
  universityId: z
    .string()
    .trim()
    .refine((value) => {
      const parsed = Number.parseInt(value, 10);
      return Number.isInteger(parsed) && parsed > 0;
    }, "Please select your university."),
  facultyId: z
    .string()
    .trim()
    .refine((value) => {
      const parsed = Number.parseInt(value, 10);
      return Number.isInteger(parsed) && parsed > 0;
    }, "Please select your faculty."),
  academicYear: z
    .string()
    .trim()
    .refine((value) => {
      const parsed = Number.parseInt(value, 10);
      return Number.isInteger(parsed) && parsed >= 1 && parsed <= 6;
    }, "Academic year must be between 1 and 6."),
});

export type RegisterFormValues = z.infer<typeof registerSchema>;

export const forgotPasswordSchema = z.object({
  email: z
    .string()
    .trim()
    .min(1, "Email address is required.")
    .email("A valid email address is required."),
});

export type ForgotPasswordFormValues = z.infer<typeof forgotPasswordSchema>;

export const resetPasswordSchema = z
  .object({
    email: z
      .string()
      .trim()
      .min(1, "Email address is required.")
      .email("A valid email address is required."),
    token: z.string().trim().min(1, "Password reset token is required."),
    newPassword: z
      .string()
      .min(8, "New password must be at least 8 characters long.")
      .regex(/[A-Z]/, "New password must contain at least one uppercase letter.")
      .regex(/[a-z]/, "New password must contain at least one lowercase letter.")
      .regex(/[0-9]/, "New password must contain at least one digit."),
    confirmPassword: z.string().min(1, "Please confirm your new password."),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    path: ["confirmPassword"],
    message: "Passwords do not match.",
  });

export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>;

export interface PasswordStrengthResult {
  hasMinLength: boolean;
  hasUppercase: boolean;
  hasLowercase: boolean;
  hasDigit: boolean;
  score: number;
}

export function evaluatePasswordStrength(password: string): PasswordStrengthResult {
  const hasMinLength = password.length >= 8;
  const hasUppercase = /[A-Z]/.test(password);
  const hasLowercase = /[a-z]/.test(password);
  const hasDigit = /[0-9]/.test(password);
  const score = [hasMinLength, hasUppercase, hasLowercase, hasDigit].filter(Boolean).length;

  return {
    hasMinLength,
    hasUppercase,
    hasLowercase,
    hasDigit,
    score,
  };
}
