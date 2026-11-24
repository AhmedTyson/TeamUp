/**
 * @date 2026-11-24
 * @purpose Organizer manual / RFID check-in panel built with TeamUp UI Select/Input primitives and security downgrade guidance.
 */
import { UserCheck } from "lucide-react";
import { useState, type FormEvent } from "react";
import { toast } from "sonner";
import {
  VERIFICATION_METHODS,
  type EventRegistration,
  type ManualAttendancePayload,
  type VerificationMethod,
} from "@/api";
import { Button, Card, Input, Select } from "@/components/ui";

export interface ManualCheckInCardProps {
  pendingConfirmedRegistrants: readonly EventRegistration[];
  isSubmitting: boolean;
  onSubmitCheckIn: (payload: ManualAttendancePayload) => Promise<void>;
}

const METHOD_OPTIONS = Object.values(VERIFICATION_METHODS).map((method) => ({
  value: method,
  label:
    method === VERIFICATION_METHODS.ManualByOrganizer
      ? "Manual by organizer"
      : method === VERIFICATION_METHODS.QRCode
        ? "QR verification code"
        : "RFID / campus card",
}));

export function ManualCheckInCard({
  pendingConfirmedRegistrants,
  isSubmitting,
  onSubmitCheckIn,
}: ManualCheckInCardProps) {
  const [studentId, setStudentId] = useState("");
  const [method, setMethod] = useState<VerificationMethod>(
    VERIFICATION_METHODS.ManualByOrganizer,
  );
  const [verificationCode, setVerificationCode] = useState("");

  const registrantOptions = pendingConfirmedRegistrants.map((reg) => ({
    value: String(reg.studentId),
    label: `${reg.firstName} ${reg.lastName} (#${reg.studentId})`,
  }));

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!studentId) {
      return;
    }
    try {
      await onSubmitCheckIn({
        studentId: Number(studentId),
        verificationMethod: method,
        verificationCode: verificationCode.trim() || null,
      });
      setStudentId("");
      setVerificationCode("");
      toast.success("Attendance recorded and student XP refreshed.");
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Could not record attendance.");
    }
  };

  return (
    <Card className="p-5 sm:p-6">
      <h2 className="text-lg font-bold text-white">Manual / RFID check-in</h2>
      <p className="mt-1 text-xs text-white/55">
        Verify confirmed registrants directly at the venue desk.
      </p>

      <form className="mt-4 space-y-4" onSubmit={handleSubmit}>
        <Select
          label="Confirmed registrant"
          required
          placeholder={
            registrantOptions.length > 0
              ? "Select confirmed student"
              : "All confirmed registrants checked in"
          }
          options={registrantOptions}
          value={studentId}
          onChange={(e) => setStudentId(e.target.value)}
        />

        <Select
          label="Verification method"
          options={METHOD_OPTIONS}
          value={method}
          onChange={(e) => setMethod(e.target.value as VerificationMethod)}
          hint={
            method === VERIFICATION_METHODS.QRCode && !verificationCode.trim()
              ? "Security note: submitting QRCode without a rotating code is recorded as ManualByOrganizer."
              : undefined
          }
        />

        <Input
          label="Verification code (optional)"
          maxLength={128}
          placeholder="Paste active session code if verifying via QR"
          value={verificationCode}
          onChange={(e) => setVerificationCode(e.target.value)}
        />

        <Button
          type="submit"
          isLoading={isSubmitting}
          disabled={!studentId || isSubmitting}
          leftIcon={<UserCheck className="size-4" />}
        >
          Record attendance
        </Button>
      </form>
    </Card>
  );
}
