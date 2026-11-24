/**
 * @date 2026-11-24
 * @purpose Modal form for creating and editing technical chapters, operational committees, affinity groups, and working groups within an organization.
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { ORGANIZATION_UNIT_TYPES, type OrganizationUnit, type OrganizationUnitPayload } from "@/api";
import { Button, ErrorBanner, Input, Modal, Select, Textarea } from "@/components/ui";
import { mapApiErrorsToForm } from "@/lib/formErrors";
import {
  organizationUnitSchema,
  type OrganizationUnitFormValues,
} from "../schemas/organizationSchemas";

export interface OrganizationUnitFormModalProps {
  open: boolean;
  onClose: () => void;
  initial?: OrganizationUnit;
  isSubmitting: boolean;
  onSubmitPayload: (payload: OrganizationUnitPayload) => Promise<void>;
}

const UNIT_TYPE_OPTIONS = [
  { value: ORGANIZATION_UNIT_TYPES.TechnicalChapter, label: "Technical Chapter (CS, RAS, PES, ComSoc)" },
  { value: ORGANIZATION_UNIT_TYPES.OperationalCommittee, label: "Operational Committee (HR, PR, Marketing, OC)" },
  { value: ORGANIZATION_UNIT_TYPES.AffinityGroup, label: "Affinity Group (WIE, SIGHT)" },
  { value: ORGANIZATION_UNIT_TYPES.WorkingGroup, label: "Working Group / Track" },
];

export function OrganizationUnitFormModal({
  open,
  onClose,
  initial,
  isSubmitting,
  onSubmitPayload,
}: OrganizationUnitFormModalProps) {
  const [submitError, setSubmitError] = useState<unknown>(null);

  const form = useForm<OrganizationUnitFormValues>({
    resolver: zodResolver(organizationUnitSchema),
    defaultValues: {
      name: "",
      code: "",
      description: "",
      unitType: ORGANIZATION_UNIT_TYPES.TechnicalChapter,
      isRecruiting: true,
      openSeats: "15",
    },
  });

  useEffect(() => {
    if (!open) {
      return;
    }
    setSubmitError(null);
    form.reset(
      initial
        ? {
            name: initial.name,
            code: initial.code,
            description: initial.description ?? "",
            unitType: initial.unitType,
            isRecruiting: initial.isRecruiting,
            openSeats: initial.openSeats !== null ? String(initial.openSeats) : "",
          }
        : {
            name: "",
            code: "",
            description: "",
            unitType: ORGANIZATION_UNIT_TYPES.TechnicalChapter,
            isRecruiting: true,
            openSeats: "15",
          },
    );
  }, [open, initial, form]);

  const handleValidSubmit = form.handleSubmit(async (values) => {
    setSubmitError(null);
    try {
      const parsedSeats = values.openSeats ? Number.parseInt(values.openSeats, 10) : NaN;
      await onSubmitPayload({
        name: values.name.trim(),
        code: values.code.trim().toUpperCase(),
        description: values.description?.trim() ? values.description.trim() : null,
        unitType: values.unitType,
        isRecruiting: values.isRecruiting,
        openSeats: Number.isInteger(parsedSeats) && parsedSeats >= 0 ? parsedSeats : null,
      });
    } catch (error) {
      const mapped = mapApiErrorsToForm(error, form.setError, [
        "name",
        "code",
        "description",
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
      title={initial ? "Edit chapter / committee" : "Add chapter or committee"}
      description="Structure your student branch into technical chapters and operational committees."
    >
      <form className="space-y-4" onSubmit={handleValidSubmit} noValidate>
        {Boolean(submitError) && <ErrorBanner error={submitError} />}

        <div className="grid gap-4 sm:grid-cols-3">
          <Input
            containerClassName="sm:col-span-2"
            label="Unit name"
            required
            maxLength={100}
            placeholder="e.g. Robotics & Automation"
            error={form.formState.errors.name?.message}
            {...form.register("name")}
          />
          <Input
            label="Short code"
            required
            maxLength={20}
            placeholder="RAS"
            className="font-mono uppercase"
            error={form.formState.errors.code?.message}
            {...form.register("code")}
          />
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <Select
            label="Unit classification"
            options={UNIT_TYPE_OPTIONS}
            error={form.formState.errors.unitType?.message}
            {...form.register("unitType")}
          />
          <Input
            label="Open recruitment seats"
            type="number"
            min={0}
            placeholder="e.g. 15"
            error={form.formState.errors.openSeats?.message}
            {...form.register("openSeats")}
          />
        </div>

        <Textarea
          label="Scope & focus"
          rows={3}
          maxLength={600}
          placeholder="Outline workshops, competitions, or operational responsibilities."
          error={form.formState.errors.description?.message}
          {...form.register("description")}
        />

        <label className="flex cursor-pointer items-center gap-2 text-sm text-white/90">
          <input
            type="checkbox"
            {...form.register("isRecruiting")}
            className="size-4 rounded border-white/25 bg-[#14151c] text-[#d3fb52] focus:ring-[#d3fb52]"
          />
          Accepting new member applications
        </label>

        <div className="flex justify-end gap-2 border-t border-white/[0.085] pt-4">
          <Button type="button" variant="outline" onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isSubmitting}>
            {initial ? "Save changes" : "Add unit"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
