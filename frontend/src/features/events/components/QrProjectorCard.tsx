/**
 * @date 2026-11-24
 * @purpose Rotating 60-second QR & verification token projector supporting auto-refresh, on-demand generation, Stage Mode fullscreen, and attendance report.
 */
import { Copy, FileText, Maximize2, QrCode, RefreshCw } from "lucide-react";
import { QRCodeSVG } from "qrcode.react";
import { useCallback, useEffect, useRef, useState } from "react";
import { toast } from "sonner";
import type { SessionAttendance, SessionCheckInCode } from "@/api";
import { Badge, Button, Card } from "@/components/ui";
import { useI18n } from "@/lib/i18n";
import { nextCountdownTick, secondsUntilExpiry } from "../utils/attendance";
import { StageProjectorOverlay } from "./StageProjectorOverlay";

export interface QrProjectorCardProps {
  sessionId: number;
  sessionTitle?: string;
  eventTitle?: string;
  roomLabel?: string | null;
  checkedInCount?: number;
  confirmedCount?: number;
  recentAttendees?: readonly SessionAttendance[];
  isGeneratingCode: boolean;
  isLoadingReport: boolean;
  onGenerateCode: () => Promise<SessionCheckInCode>;
  onOpenReport: () => Promise<void>;
}

export function QrProjectorCard({
  sessionId,
  sessionTitle = `Session #${sessionId}`,
  eventTitle = "Live Check-In",
  roomLabel,
  checkedInCount = 0,
  confirmedCount = 0,
  recentAttendees = [],
  isGeneratingCode,
  isLoadingReport,
  onGenerateCode,
  onOpenReport,
}: QrProjectorCardProps) {
  const { t } = useI18n();
  const [active, setActive] = useState(false);
  const [stageOpen, setStageOpen] = useState(false);
  const [codeData, setCodeData] = useState<SessionCheckInCode | null>(null);
  const [secondsLeft, setSecondsLeft] = useState(0);

  const secondsRef = useRef(0);
  const inFlightRef = useRef(false);

  const refreshCode = useCallback(async () => {
    if (inFlightRef.current) {
      return;
    }
    inFlightRef.current = true;
    try {
      const next = await onGenerateCode();
      const ttl = secondsUntilExpiry(next.expiresAtUtc);
      secondsRef.current = ttl;
      setCodeData(next);
      setSecondsLeft(ttl);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Could not generate check-in code.");
    } finally {
      inFlightRef.current = false;
    }
  }, [onGenerateCode]);

  useEffect(() => {
    setCodeData(null);
    setSecondsLeft(0);
    secondsRef.current = 0;
  }, [sessionId]);

  const hasCode = codeData !== null;

  useEffect(() => {
    if (!active) {
      return;
    }
    void refreshCode();
  }, [active, sessionId, refreshCode]);

  useEffect(() => {
    if (!active && !hasCode) {
      return;
    }

    const timer = window.setInterval(() => {
      if (inFlightRef.current) {
        return;
      }
      const tick = nextCountdownTick(secondsRef.current);
      secondsRef.current = tick.seconds;
      setSecondsLeft(tick.seconds);
      if (tick.shouldRefresh && active) {
        void refreshCode();
      }
    }, 1000);

    return () => window.clearInterval(timer);
  }, [active, hasCode, sessionId, refreshCode]);

  const checkInDeepLink = codeData
    ? `${window.location.origin}/check-in?sessionId=${sessionId}&code=${encodeURIComponent(codeData.code)}`
    : "";

  const copyValue = async (value: string, label: string) => {
    try {
      await navigator.clipboard.writeText(value);
      toast.success(`${label} copied.`);
    } catch {
      toast.error(`Could not copy ${label.toLowerCase()}.`);
    }
  };

  const isExpired = Boolean(codeData && secondsLeft <= 0 && !active);

  return (
    <Card className="p-5 sm:p-6">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div>
          <h2 className="text-lg font-bold text-white">60-second QR / verification projector</h2>
          <p className="text-xs text-white/55">
            Rotating HMAC tokens auto-refresh while active or can be generated on demand.
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Button
            size="sm"
            variant="outline"
            leftIcon={<Maximize2 className="size-3.5" />}
            onClick={() => {
              setActive(true);
              setStageOpen(true);
            }}
          >
            {t("attendance.stageMode")}
          </Button>
          <Button size="sm" onClick={() => setActive((prev) => !prev)}>
            {active ? "Stop projector" : "Start projector"}
          </Button>
        </div>
      </div>

      <div className="mt-5 rounded-2xl bg-white p-5 text-center text-[#14151c]">
        {codeData ? (
          <div className="space-y-3">
            <div className="flex items-center justify-center gap-2">
              <Badge variant={isExpired ? "danger" : "success"}>
                {active ? "Auto-refreshing" : isExpired ? "Expired snapshot" : "Manual snapshot"}
              </Badge>
            </div>

            <div className="mx-auto w-fit rounded-xl border border-black/10 bg-white p-2.5 shadow-sm">
              <QRCodeSVG value={checkInDeepLink} size={184} level="M" />
            </div>

            <div className="space-y-1.5">
              <p className="text-[11px] font-bold uppercase tracking-widest text-[#15733f]">
                Session verification token
              </p>
              <div
                data-testid="projector-token-value"
                className="mx-auto max-w-full rounded-xl border border-black/10 bg-[#f4f7f2] px-3 py-2 font-mono text-base font-bold tracking-wide text-[#14151c] break-all select-all sm:text-lg"
              >
                {codeData.code}
              </div>
            </div>

            <div className="flex flex-wrap items-center justify-center gap-2">
              <button
                type="button"
                onClick={() => void copyValue(codeData.code, "Verification code")}
                className="inline-flex cursor-pointer items-center gap-1.5 rounded-lg border border-black/15 px-2.5 py-1 text-xs font-semibold text-[#14151c] hover:bg-black/5"
              >
                <Copy className="size-3.5" aria-hidden="true" /> Copy code
              </button>
              <button
                type="button"
                onClick={() => void copyValue(checkInDeepLink, "Check-in link")}
                className="inline-flex cursor-pointer items-center gap-1.5 rounded-lg border border-black/15 px-2.5 py-1 text-xs font-semibold text-[#14151c] hover:bg-black/5"
              >
                <Copy className="size-3.5" aria-hidden="true" /> Copy check-in link
              </button>
            </div>

            <p className="text-xs font-medium text-[#4b5563]">
              {secondsLeft > 0 ? `Expires in ${secondsLeft}s` : "Code window expired — generate a fresh code."}
            </p>
            <div className="mx-auto h-2 max-w-sm overflow-hidden rounded-full bg-black/10">
              <div
                className="h-full bg-[#15733f] transition-all duration-500"
                style={{ width: `${Math.min(100, Math.max(0, (secondsLeft / 60) * 100))}%` }}
              />
            </div>
          </div>
        ) : (
          <div className="py-12">
            <QrCode className="mx-auto size-12 text-[#15733f]" aria-hidden="true" />
            <p className="mt-3 text-sm font-semibold">
              {active ? "Generating rotating code…" : "Projector is paused"}
            </p>
            <p className="mt-1 text-xs text-[#4b5563]">
              Start projector for continuous rotation, or click &ldquo;Generate code now&rdquo; for
              a single snapshot.
            </p>
          </div>
        )}
      </div>

      <div className="mt-4 flex flex-wrap gap-2">
        <Button
          variant="outline"
          leftIcon={<RefreshCw className="size-4" />}
          isLoading={isGeneratingCode}
          onClick={() => void refreshCode()}
        >
          Generate code now
        </Button>
        <Button
          variant="secondary"
          leftIcon={<FileText className="size-4" />}
          isLoading={isLoadingReport}
          onClick={() => void onOpenReport()}
        >
          Attendance report
        </Button>
      </div>

      <StageProjectorOverlay
        open={stageOpen}
        onClose={() => setStageOpen(false)}
        sessionTitle={sessionTitle}
        eventTitle={eventTitle}
        roomLabel={roomLabel}
        codeData={codeData}
        checkInDeepLink={checkInDeepLink}
        secondsLeft={secondsLeft}
        checkedInCount={checkedInCount}
        confirmedCount={confirmedCount}
        recentAttendees={recentAttendees}
        onRefreshCode={refreshCode}
      />
    </Card>
  );
}
