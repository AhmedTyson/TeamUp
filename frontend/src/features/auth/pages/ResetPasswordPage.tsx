/**
 * @date 2026-11-22
 * @purpose Password reset screen supporting ?email=&token= query prefill, visual strength bar, and SecurityStamp rotation preview.
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { Eye, EyeOff, KeyRound, Lock, Mail, ShieldCheck } from "lucide-react";
import { useEffect, useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { toast } from "sonner";
import { ApiClientError } from "@/api";
import { Button, ErrorBanner, Input } from "@/components/ui";
import { mapApiErrorsToForm } from "@/lib/formErrors";
import { AuthSplitLayout } from "../components/AuthSplitLayout";
import { PasswordStrengthHints } from "../components/PasswordStrengthHints";
import { useResetPasswordMutation } from "../hooks/useAcademicCatalog";
import { resetPasswordSchema, type ResetPasswordFormValues } from "../schemas/authSchemas";

export function ResetPasswordPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const mutation = useResetPasswordMutation();

  const [showPassword, setShowPassword] = useState(false);
  const [cooldownSeconds, setCooldownSeconds] = useState(0);

  const initialEmail = searchParams.get("email") ?? "";
  const initialToken = searchParams.get("token") ?? "";

  const {
    register,
    control,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: {
      email: initialEmail,
      token: initialToken,
      newPassword: "",
      confirmPassword: "",
    },
  });

  const watchedNewPassword = useWatch({ control, name: "newPassword" }) ?? "";

  useEffect(() => {
    if (cooldownSeconds <= 0) {
      return;
    }
    const timer = window.setTimeout(() => {
      setCooldownSeconds((prev) => Math.max(0, prev - 1));
    }, 1000);
    return () => window.clearTimeout(timer);
  }, [cooldownSeconds]);

  const onSubmit = async (values: ResetPasswordFormValues) => {
    try {
      const message = await mutation.mutateAsync({
        email: values.email.trim(),
        token: values.token.trim(),
        newPassword: values.newPassword,
      });

      toast.success(message);
      void navigate("/auth/login?reset=success", { replace: true });
    } catch (error: unknown) {
      mapApiErrorsToForm(error, setError, ["email", "token", "newPassword"]);
      if (error instanceof ApiClientError && error.isRateLimited) {
        setCooldownSeconds(error.retryAfterSeconds ?? 30);
      }
    }
  };

  return (
    <AuthSplitLayout
      eyebrow="Security Update"
      headline="Set a new password."
      showcaseSubtitle="Choose a strong password for your university account to get back to your teams, clubs, and workshops."
      heroImageSrc="/images/hero/hero-demo-day.jpg"
      heroImageAlt="Student teammates showcasing their engineering project prototype at Demo Day"
      heroImagePosition="object-center"
      title="Reset your password"
      description="Enter your email, 15-minute recovery token, and your new password."
      footer={
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
          <Link
            to="/auth/forgot-password"
            className="font-medium text-white/80 no-underline transition-colors hover:text-[#d3fb52] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
          >
            Request a new token
          </Link>
          <Link
            to="/auth/login"
            className="font-semibold text-[#d3fb52] no-underline transition-colors hover:text-[#e6ff8a] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
          >
            Return to Sign In
          </Link>
        </div>
      }
    >
      {mutation.isError && (
        <ErrorBanner
          error={mutation.error}
          retryAfterSeconds={cooldownSeconds > 0 ? cooldownSeconds : undefined}
        />
      )}

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

        <Input
          label="Recovery Token"
          autoComplete="one-time-code"
          placeholder="Paste the token from your recovery email"
          required
          leftElement={<KeyRound className="size-4" aria-hidden="true" />}
          error={errors.token?.message}
          {...register("token")}
        />

        <Input
          label="New Password"
          type={showPassword ? "text" : "password"}
          autoComplete="new-password"
          placeholder="Enter a strong new password"
          required
          leftElement={<Lock className="size-4" aria-hidden="true" />}
          rightElement={
            <button
              type="button"
              onClick={() => setShowPassword((prev) => !prev)}
              aria-label={showPassword ? "Hide password" : "Show password"}
              className="inline-flex size-7 cursor-pointer items-center justify-center rounded-lg text-white/60 transition-colors hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
            >
              {showPassword ? (
                <EyeOff className="size-4" aria-hidden="true" />
              ) : (
                <Eye className="size-4" aria-hidden="true" />
              )}
            </button>
          }
          error={errors.newPassword?.message}
          {...register("newPassword")}
        />

        <PasswordStrengthHints password={watchedNewPassword} />

        <Input
          label="Confirm New Password"
          type={showPassword ? "text" : "password"}
          autoComplete="new-password"
          placeholder="Re-enter your new password"
          required
          leftElement={<Lock className="size-4" aria-hidden="true" />}
          error={errors.confirmPassword?.message}
          {...register("confirmPassword")}
        />

        <Button
          type="submit"
          size="lg"
          isLoading={mutation.isPending}
          disabled={cooldownSeconds > 0}
          leftIcon={<ShieldCheck className="size-4" aria-hidden="true" />}
          className="w-full"
        >
          {cooldownSeconds > 0 ? `Wait ${cooldownSeconds}s to retry` : "Update Password"}
        </Button>
      </form>
    </AuthSplitLayout>
  );
}
