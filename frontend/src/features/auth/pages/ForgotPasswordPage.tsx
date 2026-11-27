/**
 * @date 2026-11-22
 * @purpose Opaque password recovery request screen with 3-step recovery lifecycle preview and 429 cooldown enforcement.
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { ArrowRight, CheckCircle2, Clock, Mail, Send } from "lucide-react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import { ApiClientError } from "@/api";
import { Button, ErrorBanner, Input } from "@/components/ui";
import { mapApiErrorsToForm } from "@/lib/formErrors";
import { AuthSplitLayout } from "../components/AuthSplitLayout";
import { useForgotPasswordMutation } from "../hooks/useAcademicCatalog";
import { forgotPasswordSchema, type ForgotPasswordFormValues } from "../schemas/authSchemas";

export function ForgotPasswordPage() {
  const mutation = useForgotPasswordMutation();
  const [submittedResult, setSubmittedResult] = useState<{
    email: string;
    message: string;
  } | null>(null);
  const [cooldownSeconds, setCooldownSeconds] = useState(0);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: { email: "" },
  });

  useEffect(() => {
    if (cooldownSeconds <= 0) {
      return;
    }
    const timer = window.setTimeout(() => {
      setCooldownSeconds((prev) => Math.max(0, prev - 1));
    }, 1000);
    return () => window.clearTimeout(timer);
  }, [cooldownSeconds]);

  const onSubmit = async (values: ForgotPasswordFormValues) => {
    const cleanEmail = values.email.trim();
    try {
      const response = await mutation.mutateAsync({ email: cleanEmail });
      setSubmittedResult({
        email: cleanEmail,
        message:
          response.message ||
          "If an account with that email exists, a recovery link and token have been dispatched.",
      });
      toast.success("Password recovery instructions processed.");
    } catch (error: unknown) {
      mapApiErrorsToForm(error, setError, ["email"]);
      if (error instanceof ApiClientError && error.isRateLimited) {
        setCooldownSeconds(error.retryAfterSeconds ?? 30);
      }
    }
  };

  return (
    <AuthSplitLayout
      eyebrow="Account Recovery"
      headline="Recover access safely."
      showcaseSubtitle="Enter your university email and we'll send you a one-time code to reset your password and get back to your projects."
      heroImageSrc="/images/hero/hero-workshop-hall.jpg"
      heroImageAlt="Students participating in a hands-on engineering workshop in a modern auditorium"
      heroImagePosition="object-center"
      title="Forgot your password?"
      description="Enter your student email address to receive a time-limited reset code."
      footer={
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
          <Link
            to="/auth/login"
            className="font-medium text-white/80 no-underline transition-colors hover:text-[#d3fb52] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
          >
            Back to Sign In
          </Link>
          <Link
            to="/auth/reset-password"
            className="font-semibold text-[#d3fb52] no-underline transition-colors hover:text-[#e6ff8a] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
          >
            Already have a reset token?
          </Link>
        </div>
      }
    >
      <div className="flex items-start gap-3 rounded-2xl border border-white/10 bg-[#14151c]/75 p-4 text-xs leading-relaxed text-[#9ca3af]">
        <Clock className="mt-0.5 size-4 shrink-0 text-[#d3fb52]" aria-hidden="true" />
        <p>
          For account safety, reset tokens expire in <strong className="text-white">15 minutes</strong>{" "}
          and immediately invalidate prior sessions once redeemed.
        </p>
      </div>

      {mutation.isError && (
        <ErrorBanner
          error={mutation.error}
          retryAfterSeconds={cooldownSeconds > 0 ? cooldownSeconds : undefined}
        />
      )}

      {submittedResult ? (
        <div
          role="status"
          className="space-y-4 rounded-2xl border border-emerald-500/35 bg-emerald-500/10 p-5"
        >
          <div className="flex items-start gap-3">
            <CheckCircle2 className="mt-0.5 size-5 shrink-0 text-emerald-400" aria-hidden="true" />
            <div className="space-y-1">
              <p className="text-sm font-semibold text-emerald-100">Recovery request received</p>
              <p className="text-sm text-emerald-100/85">{submittedResult.message}</p>
            </div>
          </div>

          <div className="flex flex-wrap gap-3 pt-1">
            <Link
              to={`/auth/reset-password?email=${encodeURIComponent(submittedResult.email)}`}
              className="inline-flex h-10 items-center gap-2 rounded-xl bg-[#d3fb52] px-4 text-sm font-semibold text-[#14151c] transition-colors hover:bg-[#deff6e] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
            >
              Continue to Reset Password
              <ArrowRight className="size-4" aria-hidden="true" />
            </Link>
            <Button
              variant="secondary"
              onClick={() => {
                setSubmittedResult(null);
              }}
            >
              Try another email
            </Button>
          </div>
        </div>
      ) : (
        <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
          <Input
            label="University Email"
            type="email"
            autoComplete="email"
            placeholder="student@university.edu"
            required
            leftElement={<Mail className="size-4" aria-hidden="true" />}
            error={errors.email?.message}
            {...register("email")}
          />

          <Button
            type="submit"
            size="lg"
            isLoading={mutation.isPending}
            disabled={cooldownSeconds > 0}
            leftIcon={<Send className="size-4" aria-hidden="true" />}
            className="w-full"
          >
            {cooldownSeconds > 0 ? `Wait ${cooldownSeconds}s to retry` : "Send Recovery Instructions"}
          </Button>
        </form>
      )}
    </AuthSplitLayout>
  );
}
