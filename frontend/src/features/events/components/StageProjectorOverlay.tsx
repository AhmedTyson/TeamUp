/**
 * @date 2026-11-24
 * @purpose Fullscreen Stage Projector overlay displaying giant rotating QR code, countdown bar, and live attendee counter for lecture halls and bootcamps.
 */
import { Maximize2, RefreshCw, Users, X } from "lucide-react";
import { QRCodeSVG } from "qrcode.react";
import { useEffect } from "react";
import { createPortal } from "react-dom";
import type { SessionAttendance, SessionCheckInCode } from "@/api";
import { Badge, Button } from "@/components/ui";

export interface StageProjectorOverlayProps {
  open: boolean;
  onClose: () => void;
  sessionTitle: string;
  eventTitle: string;
  roomLabel?: string | null;
  codeData: SessionCheckInCode | null;
  checkInDeepLink: string;
  secondsLeft: number;
  checkedInCount: number;
  confirmedCount: number;
  recentAttendees?: readonly SessionAttendance[];
  onRefreshCode: () => Promise<void>;
}

export function StageProjectorOverlay({
  open,
  onClose,
  sessionTitle,
  eventTitle,
  roomLabel,
  codeData,
  checkInDeepLink,
  secondsLeft,
  checkedInCount,
  confirmedCount,
  recentAttendees = [],
  onRefreshCode,
}: StageProjectorOverlayProps) {
  useEffect(() => {
    if (!open) {
      return;
    }
    const handleKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        onClose();
      }
    };
    window.addEventListener("keydown", handleKey);
    return () => window.removeEventListener("keydown", handleKey);
  }, [open, onClose]);

  if (!open || typeof document === "undefined") {
    return null;
  }

  return createPortal(
    <div
      role="dialog"
      aria-modal="true"
      aria-label="Auditorium stage QR projector"
      className="fixed inset-0 z-[100] flex flex-col justify-between overflow-y-auto bg-[#0e1015] p-6 text-white sm:p-10"
    >
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-white/10 pb-4">
        <div>
          <div className="flex items-center gap-2">
            <Badge variant="accent">LIVE STAGE CHECK-IN</Badge>
            {roomLabel && <span className="font-mono text-xs text-white/60">{roomLabel}</span>}
          </div>
          <h2 className="mt-1 text-2xl font-bold text-white sm:text-3xl">{eventTitle}</h2>
          <p className="text-sm text-[#d3fb52]">{sessionTitle}</p>
        </div>

        <div className="flex items-center gap-3">
          <div className="rounded-2xl border border-white/15 bg-[#1b1d26] px-4 py-2 text-end">
            <p className="text-[11px] uppercase tracking-wider text-white/55">Checked In</p>
            <p className="font-mono text-xl font-bold text-[#d3fb52]">
              {checkedInCount} / {Math.max(checkedInCount, confirmedCount)}
            </p>
          </div>
          <Button
            type="button"
            variant="outline"
            leftIcon={<X className="size-4" />}
            onClick={onClose}
          >
            Exit Stage (Esc)
          </Button>
        </div>
      </div>

      <div className="my-auto grid items-center gap-8 py-6 lg:grid-cols-[1.15fr_0.85fr]">
        <div className="mx-auto flex w-full max-w-lg flex-col items-center rounded-[32px] bg-white p-8 text-center text-[#14151c] shadow-2xl">
          {codeData ? (
            <>
              <div className="rounded-2xl border border-black/10 bg-white p-4 shadow-sm">
                <QRCodeSVG value={checkInDeepLink} size={280} level="M" />
              </div>
              <p className="mt-5 text-xs font-bold uppercase tracking-[0.18em] text-[#15733f]">
                Scan with TeamUp Camera or enter verification code
              </p>
              <div className="mt-2 w-full rounded-xl border border-black/10 bg-[#f4f7f2] px-4 py-3 font-mono text-2xl font-black tracking-wide text-[#14151c] break-all">
                {codeData.code}
              </div>
              <div className="mt-4 w-full space-y-1.5">
                <div className="flex justify-between text-xs font-semibold text-[#4b5563]">
                  <span>Rotating HMAC Window</span>
                  <span className="font-mono">{secondsLeft}s remaining</span>
                </div>
                <div className="h-2.5 w-full overflow-hidden rounded-full bg-black/10">
                  <div
                    className="h-full bg-[#15733f] transition-all duration-500"
                    style={{ width: `${Math.min(100, Math.max(0, (secondsLeft / 60) * 100))}%` }}
                  />
                </div>
              </div>
            </>
          ) : (
            <div className="py-16">
              <Maximize2 className="mx-auto size-12 text-[#15733f]" aria-hidden="true" />
              <p className="mt-3 text-base font-bold">Generating rotating stage code…</p>
              <Button
                className="mt-4"
                leftIcon={<RefreshCw className="size-4" />}
                onClick={() => void onRefreshCode()}
              >
                Generate code now
              </Button>
            </div>
          )}
        </div>

        <div className="mx-auto w-full max-w-md space-y-4 rounded-3xl border border-white/10 bg-[#1b1d26] p-6">
          <div className="flex items-center justify-between">
            <h3 className="flex items-center gap-2 text-base font-bold text-white">
              <Users className="size-4 text-[#d3fb52]" aria-hidden="true" />
              Recent live check-ins
            </h3>
            <span className="font-mono text-xs text-emerald-400">LIVE SYNC</span>
          </div>

          {recentAttendees.length > 0 ? (
            <ul className="space-y-2.5">
              {recentAttendees.slice(-6).reverse().map((att) => (
                <li
                  key={att.studentId}
                  className="flex items-center justify-between rounded-xl border border-white/10 bg-[#14151c] px-4 py-2.5 text-sm"
                >
                  <span className="font-semibold text-white">
                    {att.firstName} {att.lastName}
                  </span>
                  <span className="font-mono text-xs text-white/60">
                    {new Date(att.checkInTime).toLocaleTimeString()}
                  </span>
                </li>
              ))}
            </ul>
          ) : (
            <p className="py-8 text-center text-sm text-white/50">
              Waiting for the first student scan…
            </p>
          )}
        </div>
      </div>

      <p className="text-center font-mono text-xs text-white/45">
        TeamUp Auditorium Stage Mode · Anti-relay HMAC verification · Press Esc to exit
      </p>
    </div>,
    document.body,
  );
}
