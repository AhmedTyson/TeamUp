/**
 * @date 2026-11-24
 * @purpose Validated create/edit modal for campus events with scoped organization selector and timezone-safe date inputs.
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import {
  EVENT_DELIVERY_MODES,
  EVENT_TYPES,
  type EventPayload,
  type EventSummary,
  type OrganizationSummary,
} from "@/api";
import { Button, ErrorBanner, Input, Modal, Select, Textarea } from "@/components/ui";
import { mapApiErrorsToForm } from "@/lib/formErrors";
import { eventFormSchema, type EventFormValues } from "../schemas/eventSchemas";
import { fromLocalDateTimeInput, toLocalDateTimeInput } from "../utils/attendance";

export interface EventFormModalProps {
  open: boolean;
  onClose: () => void;
  initial?: EventSummary;
  organizations: readonly OrganizationSummary[];
  managedOrgIds: readonly number[];
  isPlatformAdmin: boolean;
  isSubmitting: boolean;
  onSubmitPayload: (payload: EventPayload) => Promise<void>;
}

const EVENT_TYPE_OPTIONS = Object.values(EVENT_TYPES).map((type) => ({
  value: type,
  label: type,
}));

const DELIVERY_MODE_OPTIONS = [
  { value: EVENT_DELIVERY_MODES.InPerson, label: "In-Person Venue" },
  { value: EVENT_DELIVERY_MODES.Hybrid, label: "Hybrid (Campus + Live Stream)" },
  { value: EVENT_DELIVERY_MODES.Online, label: "Online Live Stage" },
];

export function EventFormModal({
  open,
  onClose,
  initial,
  organizations,
  managedOrgIds,
  isPlatformAdmin,
  isSubmitting,
  onSubmitPayload,
}: EventFormModalProps) {
  const [submitError, setSubmitError] = useState<unknown>(null);

  const selectableOrgs = useMemo(() => {
    if (isPlatformAdmin || managedOrgIds.length === 0) {
      return organizations;
    }
    const allowed = new Set(managedOrgIds);
    const filtered = organizations.filter((o) => allowed.has(o.organizationId));
    return filtered.length > 0 ? filtered : organizations;
  }, [organizations, managedOrgIds, isPlatformAdmin]);

  const orgOptions = useMemo(() => {
    const mapped = selectableOrgs.map((org) => ({
      value: String(org.organizationId),
      label: `${org.name} (${org.category})`,
    }));
    if (
      initial &&
      !mapped.some((opt) => Number(opt.value) === initial.organizationId)
    ) {
      mapped.unshift({
        value: String(initial.organizationId),
        label: initial.organizationName,
      });
    }
    return mapped;
  }, [selectableOrgs, initial]);

  const form = useForm<EventFormValues>({
    resolver: zodResolver(eventFormSchema) as never,
    defaultValues: {
      organizationId: "",
      title: "",
      description: "",
      type: EVENT_TYPES.Workshop,
      deliveryMode: EVENT_DELIVERY_MODES.InPerson,
      isPublic: true,
      meetingUrl: "",
      location: "",
      startDate: "",
      endDate: "",
      capacity: 50,
    },
  });

  useEffect(() => {
    if (!open) {
      return;
    }
    setSubmitError(null);
    const defaultOrgId =
      initial?.organizationId ?? selectableOrgs[0]?.organizationId ?? "";
    form.reset(
      initial
        ? {
            organizationId: String(initial.organizationId),
            title: initial.title,
            description: initial.description,
            type: initial.type,
            deliveryMode: initial.deliveryMode ?? EVENT_DELIVERY_MODES.InPerson,
            isPublic: initial.isPublic ?? true,
            meetingUrl: initial.meetingUrl ?? "",
            location: initial.location,
            startDate: toLocalDateTimeInput(initial.startDate),
            endDate: toLocalDateTimeInput(initial.endDate),
            capacity: initial.capacity,
          }
        : {
            organizationId: defaultOrgId ? String(defaultOrgId) : "",
            title: "",
            description: "",
            type: EVENT_TYPES.Workshop,
            deliveryMode: EVENT_DELIVERY_MODES.InPerson,
            isPublic: true,
            meetingUrl: "",
            location: "",
            startDate: "",
            endDate: "",
            capacity: 50,
          },
    );
  }, [open, initial, selectableOrgs, form]);

  const handleValidSubmit = form.handleSubmit(async (values) => {
    setSubmitError(null);
    try {
      await onSubmitPayload({
        title: values.title.trim(),
        description: values.description.trim(),
        type: values.type,
        deliveryMode: values.deliveryMode,
        isPublic: values.isPublic,
        meetingUrl: values.meetingUrl?.trim() ? values.meetingUrl.trim() : null,
        location: values.location.trim(),
        startDate: fromLocalDateTimeInput(values.startDate),
        endDate: fromLocalDateTimeInput(values.endDate),
        capacity: Number(values.capacity),
        ...(initial ? {} : { organizationId: Number(values.organizationId) }),
      });
    } catch (error) {
      const mapped = mapApiErrorsToForm(error, form.setError, [
        "organizationId",
        "title",
        "description",
        "type",
        "deliveryMode",
        "meetingUrl",
        "location",
        "startDate",
        "endDate",
        "capacity",
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
      title={initial ? "Edit event" : "Create a campus event"}
      description={
        initial
          ? "Update schedule, venue, and capacity constraints."
          : "Publish an event for your student organization."
      }
    >
      <form className="space-y-4" onSubmit={handleValidSubmit} noValidate>
        {Boolean(submitError) && <ErrorBanner error={submitError} />}

        <div className="grid gap-4 sm:grid-cols-2">
          <Select
            label="Host organization"
            required
            disabled={Boolean(initial)}
            placeholder="Select an organization"
            options={orgOptions}
            error={form.formState.errors.organizationId?.message}
            {...form.register("organizationId")}
          />

          <Select
            label="Event type"
            required
            options={EVENT_TYPE_OPTIONS}
            error={form.formState.errors.type?.message}
            {...form.register("type")}
          />
        </div>

        <Input
          label="Event title"
          required
          maxLength={150}
          placeholder="e.g. AI & Systems Hands-On Bootcamp"
          error={form.formState.errors.title?.message}
          {...form.register("title")}
        />

        <Textarea
          label="Description"
          required
          rows={3}
          maxLength={2000}
          placeholder="Describe topics, speakers, prerequisites, and expected takeaways."
          error={form.formState.errors.description?.message}
          {...form.register("description")}
        />

        <div className="grid gap-4 sm:grid-cols-2">
          <Input
            label="Location / venue"
            required
            maxLength={200}
            placeholder="Main Auditorium, Hall B"
            error={form.formState.errors.location?.message}
            {...form.register("location")}
          />

          <Input
            label="Seat capacity"
            required
            type="number"
            min={1}
            max={10000}
            error={form.formState.errors.capacity?.message}
            {...form.register("capacity")}
          />
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <Select
            label="Delivery mode"
            options={DELIVERY_MODE_OPTIONS}
            error={form.formState.errors.deliveryMode?.message}
            {...form.register("deliveryMode")}
          />
          <Input
            label="Meeting / Stream URL"
            maxLength={500}
            placeholder="https://meet.teamup.eg/room"
            hint="Revealed only to confirmed registrants & organizers."
            error={form.formState.errors.meetingUrl?.message}
            {...form.register("meetingUrl")}
          />
        </div>

        <label className="flex cursor-pointer items-center gap-2 text-xs font-medium text-white/85">
          <input
            type="checkbox"
            {...form.register("isPublic")}
            className="size-4 rounded border-white/25 bg-[#14151c] text-[#d3fb52] focus:ring-[#d3fb52]"
          />
          List publicly in open event discovery
        </label>

        <div className="grid gap-4 sm:grid-cols-2">
          <Input
            label="Starts (local time)"
            required
            type="datetime-local"
            error={form.formState.errors.startDate?.message}
            {...form.register("startDate")}
          />

          <Input
            label="Ends (local time)"
            required
            type="datetime-local"
            error={form.formState.errors.endDate?.message}
            {...form.register("endDate")}
          />
        </div>

        <div className="flex justify-end gap-2 border-t border-white/[0.085] pt-4">
          <Button type="button" variant="outline" onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isSubmitting}>
            {initial ? "Save changes" : "Create event"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
