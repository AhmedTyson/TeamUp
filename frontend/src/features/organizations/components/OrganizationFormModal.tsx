/**
 * @date 2026-11-24
 * @purpose Validated create/edit modal for flexible student activities and open tech communities (Scope, University, City, Website, Recruitment Season).
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useMemo, useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { ORGANIZATION_SCOPES, type OrganizationPayload } from "@/api";
import { Button, ErrorBanner, Input, Modal, Select, Textarea } from "@/components/ui";
import { useUniversityCatalog } from "@/features/catalog/useUniversityCatalogFallback";
import { mapApiErrorsToForm } from "@/lib/formErrors";
import {
  ORGANIZATION_CATEGORIES,
  ORGANIZATION_SCOPE_OPTIONS,
  organizationFormSchema,
  type OrganizationFormValues,
} from "../schemas/organizationSchemas";
import { CategoryInput } from "./CategoryInput";

export interface OrganizationFormModalProps {
  open: boolean;
  onClose: () => void;
  initial?: OrganizationPayload;
  isSubmitting: boolean;
  onSubmitPayload: (payload: OrganizationPayload) => Promise<void>;
}

const DEFAULT_VALUES: OrganizationFormValues = {
  name: "",
  description: "",
  category: "",
  logoUrl: "",
  scope: ORGANIZATION_SCOPES.UniversityBranch,
  universityId: "",
  city: "",
  websiteUrl: "",
  isRecruiting: false,
  currentSeasonName: "",
};

export function OrganizationFormModal({
  open,
  onClose,
  initial,
  isSubmitting,
  onSubmitPayload,
}: OrganizationFormModalProps) {
  const [submitError, setSubmitError] = useState<unknown>(null);
  const { universities } = useUniversityCatalog(open);

  const form = useForm<OrganizationFormValues>({
    resolver: zodResolver(organizationFormSchema),
    defaultValues: DEFAULT_VALUES,
  });

  const watchedScope = useWatch({ control: form.control, name: "scope" });
  const watchedIsRecruiting = useWatch({ control: form.control, name: "isRecruiting" });

  useEffect(() => {
    if (!open) {
      return;
    }
    setSubmitError(null);
    form.reset(
      initial
        ? {
            name: initial.name,
            description: initial.description,
            category: initial.category,
            logoUrl: initial.logoUrl ?? "",
            scope: initial.scope ?? ORGANIZATION_SCOPES.UniversityBranch,
            universityId: initial.universityId ? String(initial.universityId) : "",
            city: initial.city ?? "",
            websiteUrl: initial.websiteUrl ?? "",
            isRecruiting: Boolean(initial.isRecruiting),
            currentSeasonName: initial.currentSeasonName ?? "",
          }
        : DEFAULT_VALUES,
    );
  }, [open, initial, form]);

  // Categories are free text on the backend; the constant list is only offered as suggestions,
  // plus the current value when editing an organization whose category is not in the list.
  const categorySuggestions = useMemo(() => {
    const base = [...ORGANIZATION_CATEGORIES] as string[];
    if (initial?.category && !base.includes(initial.category)) {
      base.unshift(initial.category);
    }
    return base;
  }, [initial?.category]);

  const universityOptions = useMemo(
    () =>
      universities.map((u) => ({
        value: String(u.universityId),
        label: `${u.name} (${u.city})`,
      })),
    [universities],
  );

  const handleValidSubmit = form.handleSubmit(async (values) => {
    setSubmitError(null);
    try {
      const parsedUni = values.universityId ? Number.parseInt(values.universityId, 10) : NaN;
      await onSubmitPayload({
        name: values.name.trim(),
        description: values.description.trim(),
        category: values.category.trim(),
        logoUrl: values.logoUrl?.trim() ? values.logoUrl.trim() : null,
        scope: values.scope,
        universityId: Number.isInteger(parsedUni) && parsedUni > 0 ? parsedUni : null,
        city: values.city?.trim() ? values.city.trim() : null,
        websiteUrl: values.websiteUrl?.trim() ? values.websiteUrl.trim() : null,
        isRecruiting: values.isRecruiting,
        currentSeasonName: values.currentSeasonName?.trim()
          ? values.currentSeasonName.trim()
          : null,
      });
    } catch (error) {
      const mapped = mapApiErrorsToForm(error, form.setError, [
        "name",
        "description",
        "category",
        "logoUrl",
        "city",
        "websiteUrl",
        "currentSeasonName",
      ]);
      if (mapped === 0) {
        setSubmitError(error);
      }
    }
  });

  const showUniversitySelect =
    watchedScope === ORGANIZATION_SCOPES.UniversityBranch ||
    watchedScope === ORGANIZATION_SCOPES.FacultyChapter;

  return (
    <Modal
      open={open}
      onClose={onClose}
      size="lg"
      title={initial ? "Edit organization" : "Create an organization or community"}
      description={
        initial
          ? "Update your chapter scope, campus link, and recruitment status."
          : "Launch a university student branch or open Egyptian tech community."
      }
    >
      <form className="space-y-4" onSubmit={handleValidSubmit} noValidate>
        {Boolean(submitError) && <ErrorBanner error={submitError} />}

        <div className="grid gap-4 sm:grid-cols-2">
          <Input
            label="Organization name"
            required
            maxLength={120}
            placeholder="e.g. IEEE Helwan Student Branch"
            error={form.formState.errors.name?.message}
            {...form.register("name")}
          />

          <CategoryInput
            label="Category"
            required
            maxLength={50}
            suggestions={categorySuggestions}
            error={form.formState.errors.category?.message}
            {...form.register("category")}
          />
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <Select
            label="Activity scope"
            required
            options={[...ORGANIZATION_SCOPE_OPTIONS]}
            error={form.formState.errors.scope?.message}
            {...form.register("scope")}
          />

          {showUniversitySelect ? (
            <Select
              label="Home university"
              placeholder="Optional / All campuses"
              options={universityOptions}
              error={form.formState.errors.universityId?.message}
              {...form.register("universityId")}
            />
          ) : (
            <Input
              label="City / Region"
              maxLength={100}
              placeholder="e.g. Helwan & Maadi, Cairo"
              error={form.formState.errors.city?.message}
              {...form.register("city")}
            />
          )}
        </div>

        <Textarea
          label="Description"
          required
          rows={3}
          maxLength={4000}
          placeholder="Describe your mission, technical chapters, and weekly meetups."
          error={form.formState.errors.description?.message}
          {...form.register("description")}
        />

        <div className="grid gap-4 sm:grid-cols-2">
          <Input
            label="Website URL"
            maxLength={300}
            placeholder="https://ieee-helwan.org"
            error={form.formState.errors.websiteUrl?.message}
            {...form.register("websiteUrl")}
          />
          <Input
            label="Logo URL"
            maxLength={300}
            placeholder="https://example.org/logo.png"
            error={form.formState.errors.logoUrl?.message}
            {...form.register("logoUrl")}
          />
        </div>

        <div className="space-y-3 rounded-xl border border-white/10 bg-[#14151c] p-3.5">
          <label className="flex cursor-pointer items-center gap-2.5 text-sm font-medium text-white">
            <input
              type="checkbox"
              {...form.register("isRecruiting")}
              className="size-4 rounded border-white/25 bg-[#1b1d26] text-[#d3fb52] focus:ring-[#d3fb52]"
            />
            Recruitment season is open right now
          </label>
          {watchedIsRecruiting && (
            <Input
              label="Current season label"
              maxLength={80}
              placeholder="e.g. Season 2026 — Fall Recruitment"
              error={form.formState.errors.currentSeasonName?.message}
              {...form.register("currentSeasonName")}
            />
          )}
        </div>

        <div className="flex justify-end gap-2 border-t border-white/[0.085] pt-4">
          <Button type="button" variant="outline" onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isSubmitting}>
            {initial ? "Save changes" : "Create organization"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
