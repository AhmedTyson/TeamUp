/**
 * @date 2026-11-21
 * @purpose Helper mapping backend FluentValidation ApiClientError.errors[] onto react-hook-form fields.
 */
import type { FieldValues, Path, UseFormSetError } from "react-hook-form";
import { ApiClientError } from "@/api/client";

function toCamelCase(field: string): string {
  if (!field) {
    return field;
  }
  return field.charAt(0).toLowerCase() + field.slice(1);
}

/**
 * Applies field-level validation errors from an ApiClientError onto a react-hook-form instance.
 * Returns the number of field errors mapped (0 if the error only has a top-level message).
 */
export function mapApiErrorsToForm<TFieldValues extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<TFieldValues>,
  allowedFields?: readonly Path<TFieldValues>[],
): number {
  if (!(error instanceof ApiClientError) || error.errors.length === 0) {
    return 0;
  }

  const normalizedLookup = allowedFields
    ? new Map(allowedFields.map((name) => [name.toLowerCase(), name]))
    : null;

  let mapped = 0;

  for (const detail of error.errors) {
    if (!detail.field || !detail.message) {
      continue;
    }

    const camel = toCamelCase(detail.field.trim());
    let targetField: Path<TFieldValues> | undefined;

    if (normalizedLookup) {
      targetField =
        normalizedLookup.get(detail.field.trim().toLowerCase()) ??
        normalizedLookup.get(camel.toLowerCase());
    } else {
      targetField = camel as Path<TFieldValues>;
    }

    if (targetField) {
      setError(targetField, { type: "server", message: detail.message });
      mapped += 1;
    }
  }

  return mapped;
}
