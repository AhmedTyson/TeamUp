/**
 * @date 2026-11-21
 * @purpose Accessible Modal dialog with Escape key dismissal, backdrop click close, and Handshake elevated dark surface.
 */
import { X } from "lucide-react";
import { useEffect, useId, type MouseEvent, type ReactNode } from "react";
import { createPortal } from "react-dom";
import { cn } from "@/lib/cn";

export type ModalSize = "sm" | "md" | "lg" | "xl";

export interface ModalProps {
  open: boolean;
  onClose: () => void;
  title?: string;
  description?: string;
  size?: ModalSize;
  footer?: ReactNode;
  children: ReactNode;
  closeOnBackdropClick?: boolean;
  className?: string;
}

const SIZE_CLASSES: Record<ModalSize, string> = {
  sm: "max-w-md",
  md: "max-w-lg",
  lg: "max-w-2xl",
  xl: "max-w-4xl",
};

export function Modal({
  open,
  onClose,
  title,
  description,
  size = "md",
  footer,
  children,
  closeOnBackdropClick = true,
  className,
}: ModalProps) {
  const titleId = useId();
  const descriptionId = useId();

  useEffect(() => {
    if (!open) {
      return;
    }

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        event.stopPropagation();
        onClose();
      }
    };

    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    window.addEventListener("keydown", handleKeyDown);

    return () => {
      document.body.style.overflow = previousOverflow;
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [open, onClose]);

  if (!open) {
    return null;
  }

  const handleBackdropClick = (event: MouseEvent<HTMLDivElement>) => {
    if (closeOnBackdropClick && event.target === event.currentTarget) {
      onClose();
    }
  };

  const content = (
    <div
      role="presentation"
      onClick={handleBackdropClick}
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-3 backdrop-blur-sm sm:p-4"
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby={title ? titleId : undefined}
        aria-describedby={description ? descriptionId : undefined}
        className={cn(
          "relative flex max-h-[90dvh] w-full flex-col overflow-hidden rounded-2xl border border-white/12 bg-[#1b1d26] text-white shadow-2xl shadow-black/70",
          SIZE_CLASSES[size],
          className,
        )}
      >
        {(title || description) && (
          <div className="flex items-start justify-between gap-4 border-b border-white/[0.085] px-4 py-4 sm:px-6 sm:py-5">
            <div className="min-w-0 space-y-1">
              {title && (
                <h2
                  id={titleId}
                  className="text-lg font-semibold tracking-tight text-white"
                  style={{ fontFamily: "var(--font-heading)" }}
                >
                  {title}
                </h2>
              )}
              {description && (
                <p id={descriptionId} className="text-sm text-[#9ca3af]">
                  {description}
                </p>
              )}
            </div>

            <button
              type="button"
              onClick={onClose}
              aria-label="Close modal"
              className="inline-flex size-9 shrink-0 cursor-pointer items-center justify-center rounded-lg text-white/60 transition-colors hover:bg-white/10 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
            >
              <X className="size-4" aria-hidden="true" />
            </button>
          </div>
        )}

        <div className="flex-1 overflow-y-auto px-4 py-4 sm:px-6 sm:py-5">{children}</div>

        {footer && (
          <div className="flex flex-wrap items-center justify-end gap-2.5 border-t border-white/[0.085] bg-[#14151c]/50 px-4 py-3.5 sm:px-6 sm:py-4">
            {footer}
          </div>
        )}
      </div>
    </div>
  );

  return typeof document !== "undefined" ? createPortal(content, document.body) : content;
}
