/**
 * @date 2026-11-24
 * @purpose Validated create/edit modal for event sessions with timezone-safe datetime-local conversion.
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import type { EventSession, SessionPayload } from "@/api";
import { Button, ErrorBanner, Input, Modal } from "@/components/ui";
import { mapApiErrorsToForm } from "@/lib/formErrors";
import { sessionFormSchema, type SessionFormValues } from "../schemas/eventSchemas";
import { fromLocalDateTimeInput, toLocalDateTimeInput } from "../utils/attendance";

export interface SessionFormModalProps {
  open: boolean;
  onClose: () => void;
  initial?: EventSession;
  nextSessionNo: number;
  parentStartIso: string;
  parentEndIso: string;
  isSubmitting: boolean;
  onSubmitPayload: (payload: SessionPayload) => Promise<void>;
}

export function SessionFormModal({
  open,
  onClose,
  initial,
  nextSessionNo,
  parentStartIso,
  parentEndIso,
  isSubmitting,
  onSubmitPayload,
}: SessionFormModalProps) {
  const [submitError, setSubmitError] = useState<unknown>(null);

  const form = useForm<SessionFormValues>({
    resolver: zodResolver(sessionFormSchema) as never,
    defaultValues: {
      sessionNo: nextSessionNo,
      title: "",
      startTime: "",
      endTime: "",
      room: "",
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
            sessionNo: initial.sessionNo,
            title: initial.title,
            startTime: toLocalDateTimeInput(initial.startTime),
            endTime: toLocalDateTimeInput(initial.endTime),
            room: initial.room ?? "",
          }
        : {
            sessionNo: nextSessionNo,
            title: "",
            startTime: toLocalDateTimeInput(parentStartIso),
            endTime: toLocalDateTimeInput(parentEndIso),
            room: "",
          },
    );
  }, [open, initial, nextSessionNo, parentStartIso, parentEndIso, form]);

  const handleValidSubmit = form.handleSubmit(async (values) => {
    setSubmitError(null);
    try {
      await onSubmitPayload({
        sessionNo: Number(values.sessionNo),
        title: values.title.trim(),
        startTime: fromLocalDateTimeInput(values.startTime),
        endTime: fromLocalDateTimeInput(values.endTime),
        room: values.room?.trim() ? values.room.trim() : null,
      });
    } catch (error) {
      const mapped = mapApiErrorsToForm(error, form.setError, [
        "sessionNo",
        "title",
        "startTime",
        "endTime",
        "room",
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
      title={initial ? "Edit session" : "Add event session"}
      description={`Session times must fall within ${new Date(parentStartIso).toLocaleString()} – ${new Date(parentEndIso).toLocaleString()}.`}
    >
      <form className="space-y-4" onSubmit={handleValidSubmit} noValidate>
        {Boolean(submitError) && <ErrorBanner error={submitError} />}

        <div className="grid gap-4 sm:grid-cols-2">
          <Input
            label="Session number"
            required
            type="number"
            min={1}
            max={500}
            error={form.formState.errors.sessionNo?.message}
            {...form.register("sessionNo")}
          />

          <Input
            label="Room"
            maxLength={60}
            placeholder="Hall B-204 (optional)"
            error={form.formState.errors.room?.message}
            {...form.register("room")}
          />
        </div>

        <Input
          label="Session title"
          required
          maxLength={150}
          placeholder="Opening Keynote & Live Check-In"
          error={form.formState.errors.title?.message}
          {...form.register("title")}
        />

        <div className="grid gap-4 sm:grid-cols-2">
          <Input
            label="Starts (local time)"
            required
            type="datetime-local"
            error={form.formState.errors.startTime?.message}
            {...form.register("startTime")}
          />

          <Input
            label="Ends (local time)"
            required
            type="datetime-local"
            error={form.formState.errors.endTime?.message}
            {...form.register("endTime")}
          />
        </div>

        <div className="flex justify-end gap-2 border-t border-white/[0.085] pt-4">
          <Button type="button" variant="outline" onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isSubmitting}>
            {initial ? "Save session" : "Add session"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
