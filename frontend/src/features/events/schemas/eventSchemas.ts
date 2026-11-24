/**
 * @date 2026-11-24
 * @purpose Zod validation schemas aligned with backend EventValidators, EventSessionValidators, and SelfCheckInRequestValidator.
 */
import { EVENT_DELIVERY_MODES, EVENT_TYPES } from "@/api";
import { optionalSafeUrlSchema } from "@/lib/urlValidation";
import { z } from "@/lib/zodCspConfig";

const EVENT_TYPE_VALUES = [
  EVENT_TYPES.Workshop,
  EVENT_TYPES.Hackathon,
  EVENT_TYPES.Session,
  EVENT_TYPES.Competition,
] as const;

const DELIVERY_MODE_VALUES = [
  EVENT_DELIVERY_MODES.InPerson,
  EVENT_DELIVERY_MODES.Online,
  EVENT_DELIVERY_MODES.Hybrid,
] as const;

export const eventFormSchema = z
  .object({
    organizationId: z.string().min(1, "Please select an organization."),
    title: z
      .string()
      .trim()
      .min(2, "Title must be at least 2 characters.")
      .max(150, "Title cannot exceed 150 characters."),
    description: z
      .string()
      .trim()
      .min(10, "Description must be at least 10 characters.")
      .max(2000, "Description cannot exceed 2000 characters."),
    type: z.enum(EVENT_TYPE_VALUES),
    deliveryMode: z.enum(DELIVERY_MODE_VALUES).default(EVENT_DELIVERY_MODES.InPerson),
    isPublic: z.boolean().default(true),
    meetingUrl: optionalSafeUrlSchema.refine(
      (val) => !val || val.length <= 500,
      "Meeting URL cannot exceed 500 characters.",
    ),
    location: z
      .string()
      .trim()
      .min(2, "Location must be at least 2 characters.")
      .max(200, "Location cannot exceed 200 characters."),
    startDate: z.string().min(1, "Start date and time are required."),
    endDate: z.string().min(1, "End date and time are required."),
    capacity: z.coerce
      .number()
      .int("Capacity must be a whole number.")
      .min(1, "Capacity must be at least 1.")
      .max(10000, "Capacity cannot exceed 10,000."),
  })
  .refine(
    (data) => {
      const start = Date.parse(data.startDate);
      const end = Date.parse(data.endDate);
      return !Number.isNaN(start) && !Number.isNaN(end) && end >= start;
    },
    {
      path: ["endDate"],
      message: "End date & time must be on or after the start date & time.",
    },
  );

export type EventFormValues = z.infer<typeof eventFormSchema>;

export const sessionFormSchema = z
  .object({
    sessionNo: z.coerce
      .number()
      .int("Session number must be a whole number.")
      .min(1, "Session number must be at least 1.")
      .max(500, "Session number cannot exceed 500."),
    title: z
      .string()
      .trim()
      .min(2, "Session title must be at least 2 characters.")
      .max(150, "Session title cannot exceed 150 characters."),
    startTime: z.string().min(1, "Start time is required."),
    endTime: z.string().min(1, "End time is required."),
    room: z
      .string()
      .trim()
      .max(60, "Room name cannot exceed 60 characters.")
      .optional()
      .or(z.literal("")),
  })
  .refine(
    (data) => {
      const start = Date.parse(data.startTime);
      const end = Date.parse(data.endTime);
      return !Number.isNaN(start) && !Number.isNaN(end) && end >= start;
    },
    {
      path: ["endTime"],
      message: "Session end time must be on or after the start time.",
    },
  );

export type SessionFormValues = z.infer<typeof sessionFormSchema>;

export const selfCheckInSchema = z.object({
  sessionId: z.coerce
    .number()
    .int("Enter a valid positive session ID.")
    .min(1, "Enter a valid positive session ID."),
  verificationCode: z
    .string()
    .trim()
    .min(1, "Enter the rotating session verification code.")
    .max(128, "Verification code cannot exceed 128 characters."),
});

export type SelfCheckInValues = z.infer<typeof selfCheckInSchema>;
