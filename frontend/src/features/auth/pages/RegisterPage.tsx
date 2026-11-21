/**
 * @date 2026-11-22
 * @purpose Student registration screen with cached University -> Faculty cascade, academic year picker, and FluentValidation error mapping.
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { Eye, EyeOff, GraduationCap, Lock, Mail, UserPlus } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { Link, useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { ApiClientError } from "@/api";
import { Button, ErrorBanner, Input, Select, type SelectOption } from "@/components/ui";
import { mapApiErrorsToForm } from "@/lib/formErrors";
import { AuthSplitLayout } from "../components/AuthSplitLayout";
import { PasswordStrengthHints } from "../components/PasswordStrengthHints";
import { useAuth } from "../AuthContext";
import { useAcademicCatalogQuery } from "../hooks/useAcademicCatalog";
import { registerSchema, type RegisterFormValues } from "../schemas/authSchemas";

const ACADEMIC_YEAR_OPTIONS: readonly SelectOption[] = [
  { value: "1", label: "Year 1 — Freshman" },
  { value: "2", label: "Year 2 — Sophomore" },
  { value: "3", label: "Year 3 — Junior" },
  { value: "4", label: "Year 4 — Senior" },
  { value: "5", label: "Year 5 — Extended / Masters" },
  { value: "6", label: "Year 6 — Clinical / Postgraduate" },
];

export function RegisterPage() {
  const { register: registerStudent } = useAuth();
  const navigate = useNavigate();

  const [showPassword, setShowPassword] = useState(false);
  const [submitError, setSubmitError] = useState<unknown>(null);
  const [cooldownSeconds, setCooldownSeconds] = useState(0);

  const catalogQuery = useAcademicCatalogQuery();

  const {
    register,
    control,
    handleSubmit,
    setValue,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      firstName: "",
      lastName: "",
      email: "",
      password: "",
      universityId: "",
      facultyId: "",
      academicYear: "1",
    },
  });

  const selectedUniversityId = useWatch({ control, name: "universityId" }) ?? "";
  const watchedPassword = useWatch({ control, name: "password" }) ?? "";

  useEffect(() => {
    if (cooldownSeconds <= 0) {
      return;
    }
    const timer = window.setTimeout(() => {
      setCooldownSeconds((prev) => Math.max(0, prev - 1));
    }, 1000);
    return () => window.clearTimeout(timer);
  }, [cooldownSeconds]);

  const universityOptions = useMemo<SelectOption[]>(() => {
    const items = catalogQuery.data ?? [];
    return items.map((uni) => ({
      value: String(uni.universityId),
      label: `${uni.name} (${uni.city})`,
    }));
  }, [catalogQuery.data]);

  const selectedUniversity = useMemo(() => {
    const parsedId = Number.parseInt(selectedUniversityId, 10);
    if (!Number.isInteger(parsedId) || parsedId <= 0) {
      return undefined;
    }
    return (catalogQuery.data ?? []).find((uni) => uni.universityId === parsedId);
  }, [catalogQuery.data, selectedUniversityId]);

  const facultyOptions = useMemo<SelectOption[]>(() => {
    if (!selectedUniversity) {
      return [];
    }
    return selectedUniversity.faculties.map((faculty) => ({
      value: String(faculty.facultyId),
      label: `${faculty.name} (${faculty.code})`,
    }));
  }, [selectedUniversity]);

  const universityRegistration = register("universityId", {
    onChange: () => {
      setValue("facultyId", "", { shouldValidate: false, shouldDirty: true });
    },
  });

  const onSubmit = async (values: RegisterFormValues) => {
    setSubmitError(null);

    try {
      const result = await registerStudent({
        firstName: values.firstName.trim(),
        lastName: values.lastName.trim(),
        email: values.email.trim(),
        password: values.password,
        academicYear: Number.parseInt(values.academicYear, 10),
        facultyId: Number.parseInt(values.facultyId, 10),
      });

      toast.success(`Welcome to TeamUp, ${result.student.firstName}!`);
      void navigate("/projects", { replace: true });
    } catch (error: unknown) {
      mapApiErrorsToForm(error, setError, [
        "firstName",
        "lastName",
        "email",
        "password",
        "academicYear",
        "facultyId",
      ]);
      if (error instanceof ApiClientError && error.isRateLimited) {
        setCooldownSeconds(error.retryAfterSeconds ?? 30);
      }
      setSubmitError(error);
    }
  };

  return (
    <AuthSplitLayout
      badge="Academic Onboarding"
      headline="Claim your university profile."
      showcaseSubtitle="Connect directly with your university and faculty network to discover teammates, campus clubs, and verified XP milestones."
      title="Create student account"
      description="Select your university and faculty from the verified academic catalog."
      footer={
        <p>
          Already registered?{" "}
          <Link
            to="/auth/login"
            className="font-semibold text-[#d3fb52] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
          >
            Sign in to your account
          </Link>
        </p>
      }
    >
      {catalogQuery.isError && (
        <ErrorBanner
          error={catalogQuery.error}
          title="Unable to load university catalog"
          onRetry={() => {
            void catalogQuery.refetch();
          }}
        />
      )}

      {submitError !== null && (
        <ErrorBanner
          error={submitError}
          retryAfterSeconds={cooldownSeconds > 0 ? cooldownSeconds : undefined}
        />
      )}

      <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
        <div className="grid gap-4 sm:grid-cols-2">
          <Input
            label="First Name"
            autoComplete="given-name"
            placeholder="Nour"
            required
            error={errors.firstName?.message}
            {...register("firstName")}
          />

          <Input
            label="Last Name"
            autoComplete="family-name"
            placeholder="Hassan"
            required
            error={errors.lastName?.message}
            {...register("lastName")}
          />
        </div>

        <Input
          label="University Email"
          type="email"
          autoComplete="email"
          placeholder="nour.hassan@university.edu"
          required
          leftElement={<Mail className="size-4" aria-hidden="true" />}
          error={errors.email?.message}
          {...register("email")}
        />

        <div className="grid gap-4 sm:grid-cols-2">
          <Select
            label="University"
            required
            placeholder={
              catalogQuery.isPending ? "Loading universities…" : "Select your university"
            }
            disabled={catalogQuery.isPending || universityOptions.length === 0}
            options={universityOptions}
            error={errors.universityId?.message}
            {...universityRegistration}
          />

          <Select
            label="Faculty"
            required
            placeholder={
              selectedUniversity
                ? facultyOptions.length > 0
                  ? "Select your faculty"
                  : "No faculties listed"
                : "Select university first"
            }
            disabled={!selectedUniversity || facultyOptions.length === 0}
            options={facultyOptions}
            error={errors.facultyId?.message}
            {...register("facultyId")}
          />
        </div>

        <Select
          label="Academic Year"
          required
          options={ACADEMIC_YEAR_OPTIONS}
          hint="Used to help project leads balance junior and senior squads."
          error={errors.academicYear?.message}
          {...register("academicYear")}
        />

        <Input
          label="Password"
          type={showPassword ? "text" : "password"}
          autoComplete="new-password"
          placeholder="Create a strong password"
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

        <PasswordStrengthHints password={watchedPassword} />

        <div className="flex items-center gap-2 rounded-xl border border-white/8 bg-[#14151c]/70 px-3.5 py-2.5 text-xs text-[#9ca3af]">
          <GraduationCap className="size-4 shrink-0 text-[#d3fb52]" aria-hidden="true" />
          <span>
            New accounts join with the <strong className="text-white">Student</strong> role and an
            empty XP ledger.
          </span>
        </div>

        <Button
          type="submit"
          size="lg"
          isLoading={isSubmitting}
          disabled={cooldownSeconds > 0}
          leftIcon={<UserPlus className="size-4" aria-hidden="true" />}
          className="w-full"
        >
          {cooldownSeconds > 0 ? `Wait ${cooldownSeconds}s to retry` : "Create Account"}
        </Button>
      </form>
    </AuthSplitLayout>
  );
}
