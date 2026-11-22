/**
 * @date 2026-11-22
 * @purpose Deterministic date formatting and structured JSON diff utilities for TeamUp tables and audit inspection modals.
 */
const MONTHS = [
  "Jan",
  "Feb",
  "Mar",
  "Apr",
  "May",
  "Jun",
  "Jul",
  "Aug",
  "Sep",
  "Oct",
  "Nov",
  "Dec",
] as const;

export function formatDateTime(iso: string | null | undefined): string {
  if (!iso) {
    return "—";
  }
  const parsed = new Date(iso);
  if (Number.isNaN(parsed.getTime())) {
    return iso;
  }
  const month = MONTHS[parsed.getMonth()];
  const day = String(parsed.getDate()).padStart(2, "0");
  const year = parsed.getFullYear();
  const hours = String(parsed.getHours()).padStart(2, "0");
  const minutes = String(parsed.getMinutes()).padStart(2, "0");
  return `${month} ${day}, ${year} · ${hours}:${minutes}`;
}

export interface JsonDiffEntry {
  field: string;
  before: string;
  after: string;
  changed: boolean;
}

function parseJsonObject(raw: string | null): Record<string, unknown> | null {
  if (!raw || !raw.trim()) {
    return null;
  }
  try {
    const parsed = JSON.parse(raw) as unknown;
    if (parsed && typeof parsed === "object" && !Array.isArray(parsed)) {
      return parsed as Record<string, unknown>;
    }
    return { Value: parsed };
  } catch {
    return { Raw: raw };
  }
}

function stringifyCell(value: unknown): string {
  if (value === undefined) {
    return "—";
  }
  if (value === null) {
    return "null";
  }
  if (typeof value === "string") {
    return value;
  }
  return JSON.stringify(value);
}

export function computeJsonDiffEntries(
  oldJson: string | null,
  newJson: string | null,
): JsonDiffEntry[] {
  const oldObj = parseJsonObject(oldJson) ?? {};
  const newObj = parseJsonObject(newJson) ?? {};
  const keys = Array.from(new Set([...Object.keys(oldObj), ...Object.keys(newObj)]));

  return keys.map((field) => {
    const before = stringifyCell(oldObj[field]);
    const after = stringifyCell(newObj[field]);
    return {
      field,
      before,
      after,
      changed: before !== after,
    };
  });
}
