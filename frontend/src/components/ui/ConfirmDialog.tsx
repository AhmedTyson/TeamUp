/**
 * @date 2026-11-21
 * @purpose Confirmation dialog supporting destructive action guards and optional typed-phrase verification.
 */
import { useEffect, useState } from "react";
import { Button } from "./Button";
import { Input } from "./Input";
import { Modal } from "./Modal";

export interface ConfirmDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void | Promise<void>;
  title: string;
  description: string;
  confirmLabel?: string;
  cancelLabel?: string;
  variant?: "danger" | "primary";
  isLoading?: boolean;
  /** Optional string the user must type verbatim before Confirm is enabled (e.g., email for account deletion). */
  confirmTextMatch?: string;
  confirmTextLabel?: string;
}

export function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title,
  description,
  confirmLabel = "Confirm",
  cancelLabel = "Cancel",
  variant = "danger",
  isLoading = false,
  confirmTextMatch,
  confirmTextLabel,
}: ConfirmDialogProps) {
  const [typedValue, setTypedValue] = useState("");

  useEffect(() => {
    if (!open) {
      setTypedValue("");
    }
  }, [open]);

  const requiresMatch = Boolean(confirmTextMatch);
  const isMatched = !requiresMatch || typedValue.trim() === confirmTextMatch;

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={title}
      description={description}
      size="sm"
      footer={
        <>
          <Button variant="ghost" onClick={onClose} disabled={isLoading}>
            {cancelLabel}
          </Button>
          <Button
            variant={variant}
            isLoading={isLoading}
            disabled={!isMatched || isLoading}
            onClick={() => {
              void onConfirm();
            }}
          >
            {confirmLabel}
          </Button>
        </>
      }
    >
      {requiresMatch && confirmTextMatch ? (
        <Input
          label={confirmTextLabel ?? `Type "${confirmTextMatch}" to confirm`}
          value={typedValue}
          onChange={(e) => setTypedValue(e.target.value)}
          placeholder={confirmTextMatch}
          autoComplete="off"
        />
      ) : (
        <p className="text-sm text-white/75">
          Please confirm you want to proceed with this action.
        </p>
      )}
    </Modal>
  );
}
