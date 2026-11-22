/**
 * @date 2026-11-22
 * @purpose Accessible live password strength meter with 4-segment visual bar, aria-live tier label, and complexity rule pills.
 */
import { Check, Circle } from "lucide-react";
import { cn } from "@/lib/cn";
import { evaluatePasswordStrength } from "../schemas/authSchemas";

export interface PasswordStrengthHintsProps {
  password: string;
}

const STRENGTH_LABELS = ["Empty", "Weak", "Fair", "Good", "Strong"] as const;

export function PasswordStrengthHints({ password }: PasswordStrengthHintsProps) {
  const strength = evaluatePasswordStrength(password);

  const rules = [
    { label: "8+ characters", met: strength.hasMinLength },
    { label: "Uppercase (A–Z)", met: strength.hasUppercase },
    { label: "Lowercase (a–z)", met: strength.hasLowercase },
    { label: "Number (0–9)", met: strength.hasDigit },
  ] as const;

  const tierColor =
    strength.score <= 1
      ? "bg-red-400"
      : strength.score === 2
        ? "bg-amber-400"
        : strength.score === 3
          ? "bg-sky-400"
          : "bg-[#d3fb52]";

  const labelColor =
    strength.score === 4
      ? "text-[#d3fb52]"
      : strength.score >= 2
        ? "text-white/85"
        : "text-[#9ca3af]";

  return (
    <div className="space-y-3 rounded-2xl border border-white/[0.085] bg-[#14151c]/85 p-3.5">
      <div className="flex items-center justify-between text-xs">
        <span className="font-medium text-white/80">Password strength</span>
        <span
          aria-live="polite"
          className={cn("font-mono text-[11px] font-semibold tabular-nums", labelColor)}
        >
          {STRENGTH_LABELS[strength.score]} · {strength.score}/4
        </span>
      </div>

      <div className="grid grid-cols-4 gap-1.5" aria-hidden="true">
        {[1, 2, 3, 4].map((segment) => (
          <span
            key={segment}
            className={cn(
              "h-1.5 rounded-full transition-colors duration-200",
              strength.score >= segment ? tierColor : "bg-white/10",
            )}
          />
        ))}
      </div>

      <ul className="grid grid-cols-2 gap-1.5" aria-label="Password strength checklist">
        {rules.map((rule) => (
          <li
            key={rule.label}
            className={cn(
              "inline-flex items-center gap-1.5 text-xs transition-colors",
              rule.met ? "text-emerald-300" : "text-[#9ca3af]",
            )}
          >
            {rule.met ? (
              <Check className="size-3.5 shrink-0 text-emerald-400" aria-hidden="true" />
            ) : (
              <Circle className="size-3 shrink-0 text-white/30" aria-hidden="true" />
            )}
            <span>{rule.label}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}
