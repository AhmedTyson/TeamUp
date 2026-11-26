/**
 * @date 2026-11-25
 * @purpose Free-text organization category input with datalist suggestions. The backend accepts any category,
 * so the UI must not limit organizations to a compiled-in list; ORGANIZATION_CATEGORIES are suggestions only.
 */
import { useId, type ComponentProps } from "react";
import { Input } from "@/components/ui";
import { ORGANIZATION_CATEGORIES } from "../schemas/organizationSchemas";

type CategoryInputProps = ComponentProps<typeof Input> & {
  suggestions?: readonly string[];
};

export function CategoryInput({ suggestions = ORGANIZATION_CATEGORIES, ...props }: CategoryInputProps) {
  const listId = useId();
  return (
    <>
      <Input list={listId} placeholder="e.g. Technology, or your own" {...props} />
      <datalist id={listId}>
        {suggestions.map((category) => (
          <option key={category} value={category} />
        ))}
      </datalist>
    </>
  );
}
