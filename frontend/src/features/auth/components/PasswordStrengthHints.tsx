/**
 * @date 2026-11-22
 * @purpose Accessible live password strength checklist reflecting TeamUp FluentValidation password complexity rules.
 */
import { Check, Circle } from "lucide-react";
import { cn } from "@/lib/cn";
import { evaluatePasswordStrength } from "../schemas/authSchemas";

export interface PasswordStrengthHintsProps {
  password: string;
}

export function PasswordStrengthHints({ password }: PasswordStrengthHintsProps) {
  const strength = evaluatePasswordStrength(password);

  const rules = [
    { label: "8+ characters", met: strength.hasMinLength },
    { label: "Uppercase (A–Z)", met: strength.hasUppercase },
    { label: "Lowercase (a–z)", met: strength.hasLowercase },
    { label: "Number (0–9)", met: strength.hasDigit },
  ] as const;

  return (
    <div className="rounded-xl border border-white/8 bg-[#14151c]/80 p-3">
      <div className="mb-2 flex items-center justify-between text-xs">
        <span className="font-medium text-white/75">Password requirements</span>
        <span className="font-mono text-[11px] tabular-nums text-[#9ca3af]">
          {strength.score}/4 met
        </span>
      </div>
      <ul className="grid grid-cols-2 gap-1.5" aria-label="Password strength checklist">
        {rules.map((rule) => (
          <li
            key={rule.label}
            className={cn(
              "inline-flex items-center gap-1.5 text-xs transition-colors",
              rule.met ? "text-[#d3fb52]" : "text-[#9ca3af]",
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
