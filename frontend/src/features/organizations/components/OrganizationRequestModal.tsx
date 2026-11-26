/**
 * @date 2026-11-25
 * @purpose Modal form for students to propose a new student organization / activity requiring PlatformAdmin approval (auto-promotes requester to OrgAdmin upon acceptance).
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { Info } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { ORGANIZATION_SCOPES, type CreateOrganizationProposalPayload } from "@/api";
import { Button, ErrorBanner, Input, Modal, Select, Textarea } from "@/components/ui";
import { useUniversityCatalog } from "@/features/catalog/useUniversityCatalogFallback";
import { mapApiErrorsToForm } from "@/lib/formErrors";
import {
  ORGANIZATION_CATEGORIES,
  ORGANIZATION_SCOPE_OPTIONS,
  proposalFormSchema,
  type ProposalFormValues,
} from "../schemas/organizationSchemas";
import { CategoryInput } from "./CategoryInput";

export interface OrganizationRequestModalProps {
  open: boolean;
  onClose: () => void;
  isSubmitting: boolean;
  onSubmitProposal: (payload: CreateOrganizationProposalPayload) => Promise<void>;
}

const DEFAULT_VALUES: ProposalFormValues = {
  name: "",
  category: "",
  scope: ORGANIZATION_SCOPES.UniversityBranch,
  universityId: "",
  city: "",
  description: "",
  websiteUrl: "",
  logoUrl: "",
  currentSeasonName: "",
  justification: "",
};

export function OrganizationRequestModal({
  open,
  onClose,
  isSubmitting,
  onSubmitProposal,
}: OrganizationRequestModalProps) {
  const [submitError, setSubmitError] = useState<unknown>(null);
  const { universities } = useUniversityCatalog(open);

  const form = useForm<ProposalFormValues>({
    resolver: zodResolver(proposalFormSchema),
    defaultValues: DEFAULT_VALUES,
  });

  const watchedScope = useWatch({ control: form.control, name: "scope" });

  useEffect(() => {
    if (!open) return;
    setSubmitError(null);
    form.reset(DEFAULT_VALUES);
  }, [open, form]);

  const universityOptions = useMemo(
    () =>
      universities.map((u) => ({
        value: String(u.universityId),
        label: `${u.name} (${u.city})`,
      })),
    [universities],
  );

  const showUniversitySelect =
    watchedScope === ORGANIZATION_SCOPES.UniversityBranch ||
    watchedScope === ORGANIZATION_SCOPES.FacultyChapter;

  const handleValidSubmit = form.handleSubmit(async (values) => {
    setSubmitError(null);
    try {
      const parsedUni = values.universityId ? Number.parseInt(values.universityId, 10) : NaN;
      await onSubmitProposal({
        name: values.name.trim(),
        description: values.description.trim(),
        category: values.category.trim(),
        logoUrl: values.logoUrl?.trim() ? values.logoUrl.trim() : null,
        scope: values.scope,
        universityId: Number.isInteger(parsedUni) && parsedUni > 0 ? parsedUni : null,
        city: values.city?.trim() ? values.city.trim() : null,
        websiteUrl: values.websiteUrl?.trim() ? values.websiteUrl.trim() : null,
        currentSeasonName: values.currentSeasonName?.trim()
          ? values.currentSeasonName.trim()
          : null,
        justification: values.justification?.trim() ? values.justification.trim() : null,
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
        "justification",
      ]);
      if (mapped === 0) {
        setSubmitError(error);
      }
    }
  });

  return (
    <Modal
      open={open}
      onClose={onClose}
      size="lg"
      title="Request a new student organization"
      description="Submit a proposal for Platform Admin review before launching on TeamUp."
    >
      <form className="space-y-4" onSubmit={handleValidSubmit} noValidate>
        <div className="flex items-start gap-2.5 rounded-xl border border-[#d3fb52]/25 bg-[#d3fb52]/[0.07] p-3 text-xs leading-relaxed text-[#e6f99f]">
          <Info className="mt-0.5 size-4 shrink-0 text-[#d3fb52]" aria-hidden="true" />
          <p>
            Requests are reviewed by a Platform Admin. Once approved, your organization goes live and
            you are automatically promoted to <strong className="font-semibold text-white">Organization Admin</strong> &amp; President.
          </p>
        </div>

        {Boolean(submitError) && <ErrorBanner error={submitError} />}

        <div className="grid gap-4 sm:grid-cols-2">
          <Input
            label="Proposed organization name"
            required
            maxLength={120}
            placeholder="e.g. Pixels Helwan Student Activity"
            error={form.formState.errors.name?.message}
            {...form.register("name")}
          />

          <CategoryInput
            label="Category"
            required
            maxLength={50}
            suggestions={ORGANIZATION_CATEGORIES}
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
              placeholder="e.g. Cairo & Maadi"
              error={form.formState.errors.city?.message}
              {...form.register("city")}
            />
          )}
        </div>

        <Textarea
          label="Mission & chapters overview"
          required
          rows={3}
          maxLength={4000}
          placeholder="Describe your planned tracks, workshops, and how students collaborate."
          error={form.formState.errors.description?.message}
          {...form.register("description")}
        />

        <Textarea
          label="Verification & founding plan (for admin review)"
          rows={2}
          maxLength={1000}
          placeholder="Optional: campus authorization details, faculty advisor, or founding High Board team."
          error={form.formState.errors.justification?.message}
          {...form.register("justification")}
        />

        <div className="grid gap-4 sm:grid-cols-2">
          <Input
            label="Website URL"
            maxLength={300}
            placeholder="https://example.org"
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

        <Input
          label="Planned launch season"
          maxLength={80}
          placeholder="e.g. Season 2026 — Launch Cohort"
          error={form.formState.errors.currentSeasonName?.message}
          {...form.register("currentSeasonName")}
        />

        <div className="flex justify-end gap-2 border-t border-white/[0.085] pt-4">
          <Button type="button" variant="outline" onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isSubmitting}>
            Submit request
          </Button>
        </div>
      </form>
    </Modal>
  );
}
