/**
 * @date 2026-11-24
 * @purpose Student self check-in page supporting QR deep links, rotating {step}.{hex} codes, distinct 201/400/409 feedback, and gamification XP sync.
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Camera, CheckCircle2, QrCode } from "lucide-react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useSearchParams } from "react-router-dom";
import { toast } from "sonner";
import { ApiClientError, attendanceApi, type SessionAttendance } from "@/api";
import { Badge, Button, Card, ErrorBanner, Input, StatusPill } from "@/components/ui";
import { useAuth } from "@/features/auth/AuthContext";
import { invalidateGamification } from "@/lib/queryKeys";
import { QrCameraScannerModal } from "../components/QrCameraScannerModal";
import { selfCheckInSchema, type SelfCheckInValues } from "../schemas/eventSchemas";

export function StudentCheckInPage() {
  const [params] = useSearchParams();
  const querySessionId = params.get("sessionId") ?? "";
  const queryCode = params.get("code") ?? "";

  const queryClient = useQueryClient();
  const { user } = useAuth();

  const [verifiedRecord, setVerifiedRecord] = useState<SessionAttendance | null>(null);
  const [submitError, setSubmitError] = useState<unknown>(null);
  const [scannerOpen, setScannerOpen] = useState(false);

  const form = useForm<SelfCheckInValues>({
    resolver: zodResolver(selfCheckInSchema) as never,
    defaultValues: {
      sessionId: querySessionId ? Number(querySessionId) : ("" as unknown as number),
      verificationCode: queryCode,
    },
  });

  useEffect(() => {
    form.reset({
      sessionId: querySessionId ? Number(querySessionId) : ("" as unknown as number),
      verificationCode: queryCode,
    });
    setVerifiedRecord(null);
    setSubmitError(null);
  }, [querySessionId, queryCode, form]);

  const mutation = useMutation({
    mutationFn: (values: SelfCheckInValues) =>
      attendanceApi.selfCheckIn(Number(values.sessionId), values.verificationCode.trim()),
    onSuccess: async (record) => {
      setSubmitError(null);
      setVerifiedRecord(record);
      await invalidateGamification(queryClient, user?.studentId);
      toast.success("Attendance verified! Your XP and badges are being refreshed.");
    },
    onError: (error) => {
      setVerifiedRecord(null);
      setSubmitError(error);
      toast.error(
        error instanceof Error
          ? error.message
          : "Check-in failed. Verify the session code and registration status.",
      );
    },
  });

  const apiError = submitError instanceof ApiClientError ? submitError : null;
  const errorTitle = apiError?.isConflict
    ? "Already checked in or rate-limited"
    : apiError?.isValidationError
      ? "Invalid or expired verification code"
      : "Unable to verify attendance";

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <section className="relative overflow-hidden rounded-[28px] border border-white/10 bg-[radial-gradient(ellipse_at_80%_0%,rgba(83,152,134,0.35),transparent_50%),linear-gradient(135deg,#1b1d26,#14151c)] p-6 shadow-2xl sm:p-9">
        <div className="space-y-3">
          <Badge variant="accent">Student check-in</Badge>
          <h1
            className="text-4xl font-bold uppercase leading-[0.95] text-[#d3fb52] sm:text-5xl"
            style={{ fontFamily: "var(--font-display)" }}
          >
            You made it.
          </h1>
          <p className="text-sm leading-relaxed text-white/70 sm:text-base">
            Scan the session QR code or paste the active verification token to verify your
            attendance and earn campus XP.
          </p>
        </div>
      </section>

      {verifiedRecord && (
        <Card className="border-emerald-500/35 bg-emerald-500/10 p-5">
          <div className="flex items-start gap-3">
            <CheckCircle2 className="mt-0.5 size-5 shrink-0 text-emerald-400" aria-hidden="true" />
            <div className="flex-1 space-y-2">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <h2 className="text-base font-bold text-emerald-200">
                  Attendance verified!
                </h2>
                <StatusPill status={verifiedRecord.verificationMethod} />
              </div>
              <p className="text-sm text-white/85">
                Recorded for{" "}
                <strong>
                  {verifiedRecord.firstName} {verifiedRecord.lastName}
                </strong>{" "}
                (Session #{verifiedRecord.sessionId}) at{" "}
                {new Date(verifiedRecord.checkInTime).toLocaleTimeString()}.
              </p>
              <div className="pt-1">
                <Link
                  to="/gamification"
                  className="text-xs font-semibold text-[#d3fb52] hover:underline"
                >
                  View updated XP & badges →
                </Link>
              </div>
            </div>
          </div>
        </Card>
      )}

      {Boolean(submitError) && (
        <ErrorBanner
          error={submitError}
          variant={apiError?.isConflict ? "warning" : undefined}
          title={errorTitle}
        />
      )}

      <Card className="space-y-4 p-6">
        <form
          className="space-y-4"
          onSubmit={form.handleSubmit((values) => mutation.mutate(values))}
          noValidate
        >
          <Input
            label="Session ID"
            required
            type="number"
            min={1}
            inputMode="numeric"
            placeholder="e.g. 104"
            error={form.formState.errors.sessionId?.message}
            {...form.register("sessionId")}
          />

          <Input
            label="Verification token"
            required
            maxLength={128}
            placeholder="e.g. 29927200.a1b2c3d4e5f60718"
            hint="Rotating session codes include a time step and hexadecimal signature and expire after 60 seconds."
            className="font-mono"
            error={form.formState.errors.verificationCode?.message}
            {...form.register("verificationCode")}
          />

          <div className="flex flex-wrap gap-2">
            <Button
              type="submit"
              className="flex-1"
              isLoading={mutation.isPending}
              leftIcon={<QrCode className="size-4" />}
            >
              Verify attendance
            </Button>
            <Button
              type="button"
              variant="outline"
              leftIcon={<Camera className="size-4" />}
              onClick={() => setScannerOpen(true)}
            >
              Scan QR with Camera
            </Button>
          </div>
        </form>
      </Card>

      <QrCameraScannerModal
        open={scannerOpen}
        onClose={() => setScannerOpen(false)}
        onScanValue={(scannedSessionId, scannedCode) => {
          if (scannedSessionId) {
            form.setValue("sessionId", scannedSessionId, { shouldValidate: true });
          }
          form.setValue("verificationCode", scannedCode, { shouldValidate: true });
          toast.success("QR token captured! Click Verify attendance or submitting automatically…");
          const currentSessionId = scannedSessionId ?? Number(form.getValues("sessionId"));
          if (Number.isInteger(currentSessionId) && currentSessionId > 0 && scannedCode) {
            mutation.mutate({ sessionId: currentSessionId, verificationCode: scannedCode });
          }
        }}
      />
    </div>
  );
}
