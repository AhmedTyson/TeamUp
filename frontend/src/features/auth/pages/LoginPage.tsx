/**
 * @date 2026-11-22
 * @purpose Student sign-in screen with focusable error summary, session-expired alerts, 429 cooldown timer, and route-specific role scope preview.
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { CheckCircle2, Eye, EyeOff, KeyRound, Lock, LogIn, Mail, Shield } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useLocation, useNavigate, useSearchParams } from "react-router-dom";
import { toast } from "sonner";
import { ApiClientError } from "@/api";
import { Button, ErrorBanner, Input } from "@/components/ui";
import { mapApiErrorsToForm } from "@/lib/formErrors";
import { AuthSplitLayout } from "../components/AuthSplitLayout";
import { useAuth } from "../AuthContext";
import { loginSchema, type LoginFormValues } from "../schemas/authSchemas";

interface RedirectLocationState {
  from?: {
    pathname?: string;
    search?: string;
  };
}

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();

  const [showPassword, setShowPassword] = useState(false);
  const [submitError, setSubmitError] = useState<unknown>(null);
  const [cooldownSeconds, setCooldownSeconds] = useState(0);
  const errorRegionRef = useRef<HTMLDivElement>(null);

  const reason = searchParams.get("reason");
  const resetStatus = searchParams.get("reset");

  const {
    register,
    handleSubmit,
    setValue,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
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

  const onSubmit = async (values: LoginFormValues) => {
    setSubmitError(null);

    try {
      const result = await login({
        email: values.email.trim(),
        password: values.password,
      });

      toast.success(`Welcome back, ${result.student.firstName}!`);

      const state = location.state as RedirectLocationState | null;
      const fromPath = state?.from?.pathname;
      const fromSearch = state?.from?.search ?? "";
      const safeRedirect =
        fromPath && fromPath.startsWith("/") && !fromPath.startsWith("/auth")
          ? `${fromPath}${fromSearch}`
          : "/projects";

      void navigate(safeRedirect, { replace: true });
    } catch (error: unknown) {
      mapApiErrorsToForm(error, setError, ["email", "password"]);
      if (error instanceof ApiClientError && error.isRateLimited) {
        setCooldownSeconds(error.retryAfterSeconds ?? 30);
      }
      setSubmitError(error);
      requestAnimationFrame(() => {
        errorRegionRef.current?.focus();
      });
    }
  };

  const applyDemoCredentials = (email: string) => {
    const demoPassword =
      (import.meta.env.VITE_DEV_SEED_PASSWORD as string | undefined) || "Adm!0RK8zvFIO2BDTX";
    setValue("email", email, { shouldDirty: true, shouldValidate: true });
    setValue("password", demoPassword, { shouldDirty: true });
  };

  const devQuickFill = import.meta.env.DEV ? (
    <div className="flex flex-wrap items-center justify-between gap-2 rounded-2xl border border-white/[0.08] bg-[#14151c]/70 px-3.5 py-2.5">
      <span className="inline-flex items-center gap-1.5 text-[11px] font-medium text-[#9ca3af]">
        <KeyRound className="size-3.5 text-[#d3fb52]" aria-hidden="true" />
        Demo quick-fill
      </span>
      <div className="flex flex-wrap gap-2">
        <button
          type="button"
          onClick={() => applyDemoCredentials("admin@teamup.local")}
          className="inline-flex cursor-pointer items-center gap-1 rounded-lg border border-white/12 bg-[#1b1d26] px-2.5 py-1 text-xs font-medium text-white transition-colors hover:border-[#d3fb52]/50 hover:text-[#d3fb52]"
        >
          <Shield className="size-3" aria-hidden="true" />
          PlatformAdmin
        </button>
        <button
          type="button"
          onClick={() => applyDemoCredentials("nour@cu.edu.eg")}
          className="inline-flex cursor-pointer items-center gap-1 rounded-lg border border-white/12 bg-[#1b1d26] px-2.5 py-1 text-xs font-medium text-white transition-colors hover:border-[#d3fb52]/50 hover:text-[#d3fb52]"
        >
          OrgAdmin Demo
        </button>
      </div>
    </div>
  ) : null;

  return (
    <AuthSplitLayout
      eyebrow="Student Portal"
      headline="Step back into your campus squad."
      showcaseSubtitle="Pick up active Kanban tasks, review organization invitations, and join upcoming event sessions with your university account."
      heroImageSrc="/images/hero/hero-squad-lab.jpg"
      heroImageAlt="University students collaborating around laptops in a sunlit innovation lab"
      heroImagePosition="object-center"
      title="Sign in to TeamUp"
      description="Use your registered student email and password to continue."
      footer={
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
          <span>
            New to TeamUp?{" "}
            <Link
              to="/auth/register"
              className="font-semibold text-[#d3fb52] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
            >
              Create a student account
            </Link>
          </span>
          <Link
            to="/auth/forgot-password"
            className="font-medium text-white/80 hover:text-[#d3fb52] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
          >
            Forgot password?
          </Link>
        </div>
      }
    >
      {reason === "session_expired" && (
        <ErrorBanner
          variant="warning"
          title="Session expired"
          message="Your session has expired. Please sign in again to continue."
        />
      )}

      {resetStatus === "success" && (
        <div
          role="status"
          className="flex items-start gap-3 rounded-2xl border border-emerald-500/35 bg-emerald-500/10 p-4 text-sm text-emerald-200"
        >
          <CheckCircle2 className="mt-0.5 size-5 shrink-0 text-emerald-400" aria-hidden="true" />
          <div>
            <p className="font-semibold text-emerald-100">Password updated</p>
            <p className="text-emerald-200/85">
              Your credentials were reset cleanly. Sign in with your new password below.
            </p>
          </div>
        </div>
      )}

      {submitError !== null && (
        <div ref={errorRegionRef} tabIndex={-1} className="focus:outline-none">
          <ErrorBanner
            error={submitError}
            retryAfterSeconds={cooldownSeconds > 0 ? cooldownSeconds : undefined}
          />
        </div>
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
          label="Password"
          type={showPassword ? "text" : "password"}
          autoComplete="current-password"
          placeholder="Enter your password"
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
          error={errors.password?.message}
          {...register("password")}
        />

        <div className="pt-1">
          <Button
            type="submit"
            size="lg"
            isLoading={isSubmitting}
            disabled={cooldownSeconds > 0}
            leftIcon={<LogIn className="size-4" aria-hidden="true" />}
            className="w-full"
          >
            {cooldownSeconds > 0 ? `Wait ${cooldownSeconds}s to retry` : "Sign In"}
          </Button>
        </div>
      </form>
      {devQuickFill}
    </AuthSplitLayout>
  );
}
