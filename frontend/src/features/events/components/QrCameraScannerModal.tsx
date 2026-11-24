/**
 * @date 2026-11-24
 * @purpose In-app camera viewfinder QR scanner modal powered by navigator.mediaDevices + BarcodeDetector with graceful fallback notice for unsupported browsers.
 */
import { Camera, XCircle } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { Button, Modal } from "@/components/ui";

export interface QrCameraScannerModalProps {
  open: boolean;
  onClose: () => void;
  onScanValue: (sessionId: number | null, code: string) => void;
}

interface BarcodeDetectorLike {
  detect(source: HTMLVideoElement): Promise<Array<{ rawValue?: string }>>;
}

type BarcodeDetectorCtor = new (options?: { formats?: string[] }) => BarcodeDetectorLike;

export function parseScannedCheckInPayload(raw: string): {
  sessionId: number | null;
  code: string;
} {
  const trimmed = raw.trim();
  try {
    if (trimmed.includes("://") || trimmed.startsWith("/check-in")) {
      const url = new URL(trimmed, window.location.origin);
      const sessionParam = Number.parseInt(url.searchParams.get("sessionId") ?? "", 10);
      const codeParam = url.searchParams.get("code") ?? "";
      if (codeParam) {
        return {
          sessionId: Number.isInteger(sessionParam) && sessionParam > 0 ? sessionParam : null,
          code: codeParam,
        };
      }
    }
  } catch {
    // Not a URL — fall back to raw token
  }
  return { sessionId: null, code: trimmed };
}

export function QrCameraScannerModal({
  open,
  onClose,
  onScanValue,
}: QrCameraScannerModalProps) {
  const videoRef = useRef<HTMLVideoElement | null>(null);
  const [cameraError, setCameraError] = useState<string | null>(null);

  useEffect(() => {
    if (!open) {
      return;
    }
    let active = true;
    let stream: MediaStream | null = null;
    let rafTimer: number | null = null;

    const startScanner = async () => {
      setCameraError(null);
      if (!navigator.mediaDevices?.getUserMedia) {
        setCameraError("Camera access is not available in this browser or context. Enter the verification code manually below.");
        return;
      }

      const Detector = (window as unknown as { BarcodeDetector?: BarcodeDetectorCtor }).BarcodeDetector;
      if (!Detector) {
        setCameraError("In-browser BarcodeDetector is not supported here. Scan with your phone's camera app to open the check-in link, or paste the token manually.");
        return;
      }

      try {
        stream = await navigator.mediaDevices.getUserMedia({
          video: { facingMode: "environment" },
        });
        if (!active) {
          stream.getTracks().forEach((t) => t.stop());
          return;
        }
        if (videoRef.current) {
          videoRef.current.srcObject = stream;
          await videoRef.current.play();
        }

        const detector = new Detector({ formats: ["qr_code"] });
        const scanLoop = async () => {
          if (!active || !videoRef.current) {
            return;
          }
          try {
            if (videoRef.current.readyState >= 2) {
              const results = await detector.detect(videoRef.current);
              const raw = results[0]?.rawValue?.trim();
              if (raw) {
                const parsed = parseScannedCheckInPayload(raw);
                onScanValue(parsed.sessionId, parsed.code);
                onClose();
                return;
              }
            }
          } catch {
            // Continue polling frames
          }
          rafTimer = window.setTimeout(() => void scanLoop(), 250);
        };
        void scanLoop();
      } catch {
        setCameraError("Could not access camera. Check camera permissions or paste the verification code manually.");
      }
    };

    void startScanner();

    return () => {
      active = false;
      if (rafTimer !== null) {
        window.clearTimeout(rafTimer);
      }
      if (stream) {
        stream.getTracks().forEach((t) => t.stop());
      }
    };
  }, [open, onClose, onScanValue]);

  return (
    <Modal
      open={open}
      onClose={onClose}
      title="Scan Session QR Code"
      description="Point your rear camera at the stage projector QR code to auto-fill and verify attendance."
    >
      <div className="space-y-4">
        {cameraError ? (
          <div className="flex items-start gap-3 rounded-2xl border border-amber-400/35 bg-amber-400/10 p-4 text-sm text-amber-100">
            <XCircle className="mt-0.5 size-5 shrink-0 text-amber-300" aria-hidden="true" />
            <p>{cameraError}</p>
          </div>
        ) : (
          <div className="relative aspect-square w-full overflow-hidden rounded-2xl border border-[#d3fb52]/35 bg-black">
            <video
              ref={videoRef}
              playsInline
              muted
              className="h-full w-full object-cover"
            />
            <div
              aria-hidden="true"
              className="pointer-events-none absolute inset-10 rounded-2xl border-2 border-[#d3fb52]/80 shadow-[0_0_0_9999px_rgba(0,0,0,0.45)]"
            />
            <div className="absolute bottom-3 inset-x-0 text-center">
              <span className="inline-flex items-center gap-1.5 rounded-full bg-black/75 px-3 py-1 text-xs font-medium text-[#d3fb52]">
                <Camera className="size-3.5" aria-hidden="true" />
                Align projector QR inside frame
              </span>
            </div>
          </div>
        )}

        <div className="flex justify-end">
          <Button variant="outline" onClick={onClose}>
            Close scanner
          </Button>
        </div>
      </div>
    </Modal>
  );
}
