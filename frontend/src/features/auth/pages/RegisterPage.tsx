/**
 * @date 2026-11-22
 * @purpose Student onboarding registration screen with live University -> Faculty catalog cascade, Campus Credential Card preview, and focusable error summary.
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { Building2, ChevronDown, Eye, EyeOff, GraduationCap, Lock, Mail, UserPlus } from "lucide-react";
import { useEffect, useMemo, useRef, useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { Link, useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { ApiClientError } from "@/api";
import { Avatar, Badge, Button, ErrorBanner, Input, Select, UniversityLogo, type SelectOption } from "@/components/ui";
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
  const [emailPrefsOpen, setEmailPrefsOpen] = useState(false);
  const [submitError, setSubmitError] = useState<unknown>(null);
  const [cooldownSeconds, setCooldownSeconds] = useState(0);
  const errorRegionRef = useRef<HTMLDivElement>(null);

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
      emailNotificationsEnabled: true,
      receiveLeaderboardDigests: true,
      receiveProjectMatchEmails: true,
    },
  });

  const watchedFirstName = useWatch({ control, name: "firstName" }) ?? "";
  const watchedLastName = useWatch({ control, name: "lastName" }) ?? "";
  const watchedEmail = useWatch({ control, name: "email" }) ?? "";
  const selectedUniversityId = useWatch({ control, name: "universityId" }) ?? "";
  const selectedFacultyId = useWatch({ control, name: "facultyId" }) ?? "";
  const watchedAcademicYear = useWatch({ control, name: "academicYear" }) ?? "1";
  const watchedPassword = useWatch({ control, name: "password" }) ?? "";
  const watchedEmailNotificationsEnabled =
    useWatch({
      control,
      name: "emailNotificationsEnabled",
    }) ?? true;
  const watchedReceiveLeaderboardDigests =
    useWatch({
      control,
      name: "receiveLeaderboardDigests",
    }) ?? true;
  const watchedReceiveProjectMatchEmails =
    useWatch({
      control,
      name: "receiveProjectMatchEmails",
    }) ?? true;

  const activeDigestCount =
    (watchedReceiveLeaderboardDigests ? 1 : 0) + (watchedReceiveProjectMatchEmails ? 1 : 0);
  const emailPrefsSummary = !watchedEmailNotificationsEnabled
    ? "Paused (security only)"
    : activeDigestCount > 0
      ? `${activeDigestCount} of 2 digests`
      : "Account only";

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

  const selectedFaculty = useMemo(() => {
    const parsedId = Number.parseInt(selectedFacultyId, 10);
    if (!selectedUniversity || !Number.isInteger(parsedId) || parsedId <= 0) {
      return undefined;
    }
    return selectedUniversity.faculties.find((f) => f.facultyId === parsedId);
  }, [selectedUniversity, selectedFacultyId]);

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
        emailNotificationsEnabled: values.emailNotificationsEnabled,
        receiveLeaderboardDigests:
          values.emailNotificationsEnabled && values.receiveLeaderboardDigests,
        receiveProjectMatchEmails:
          values.emailNotificationsEnabled && values.receiveProjectMatchEmails,
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
      requestAnimationFrame(() => {
        errorRegionRef.current?.focus();
      });
    }
  };

  const fullNamePreview =
    [watchedFirstName.trim(), watchedLastName.trim()].filter(Boolean).join(" ") ||
    "Your Student Name";

  const credentialCardPreview = (
    <div className="rounded-2xl border border-white/15 bg-[#14151c]/88 p-4 shadow-2xl shadow-black/50 backdrop-blur-xl">
      <div className="flex items-center justify-between gap-2 border-b border-white/[0.08] pb-3">
        <span className="font-mono text-[11px] uppercase tracking-wider text-white/75">
          Campus Pass Preview
        </span>
        <Badge variant="accent" size="sm">
          Student · Yr {watchedAcademicYear}
        </Badge>
      </div>

      <div className="mt-3.5 flex items-center justify-between gap-3">
        <div className="flex min-w-0 items-center gap-3">
          <Avatar name={fullNamePreview} size="md" />
          <div className="min-w-0">
            <p className="truncate text-sm font-semibold text-white">{fullNamePreview}</p>
            <p className="truncate text-xs text-white/70">
              {watchedEmail.trim() || "student@university.edu"}
            </p>
          </div>
        </div>
        {selectedUniversity && (
          <UniversityLogo
            name={selectedUniversity.name}
            logoUrl={selectedUniversity.logoUrl}
            size="md"
            className="ring-2 ring-[#d3fb52]/40"
          />
        )}
      </div>

      <div className="mt-3.5 space-y-1.5 rounded-xl border border-white/[0.08] bg-[#1b1d26]/85 p-3 text-xs">
        <div className="flex items-center justify-between gap-2">
          <span className="inline-flex shrink-0 items-center gap-1.5 text-[#9ca3af]">
            {selectedUniversity ? (
              <UniversityLogo
                name={selectedUniversity.name}
                logoUrl={selectedUniversity.logoUrl}
                size="sm"
                className="size-5 rounded-md"
              />
            ) : (
              <Building2 className="size-3.5 text-emerald-400" aria-hidden="true" />
            )}
            University
          </span>
          <span className="min-w-0 truncate text-right font-medium text-white">
            {selectedUniversity ? selectedUniversity.name : "Select university"}
          </span>
        </div>
        <div className="flex items-center justify-between gap-2">
          <span className="inline-flex shrink-0 items-center gap-1.5 text-[#9ca3af]">
            <GraduationCap className="size-3.5 text-[#d3fb52]" aria-hidden="true" />
            Faculty
          </span>
          <span className="min-w-0 truncate text-right font-medium text-white">
            {selectedFaculty ? `${selectedFaculty.code} · ${selectedFaculty.name}` : "Pending"}
          </span>
        </div>
      </div>
    </div>
  );

  return (
    <AuthSplitLayout
      eyebrow="Campus Onboarding"
      headline="Claim your university profile."
      showcaseSubtitle="Connect directly with your university and faculty network to discover teammates, campus clubs, and student activities."
      heroImageSrc="/images/hero/hero-club-meetup.jpg"
      heroImageAlt="University student club members brainstorming outdoors around hardware and laptops"
      heroImagePosition="object-center"
      previewSlot={credentialCardPreview}
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
        <div ref={errorRegionRef} tabIndex={-1} className="focus:outline-none">
          <ErrorBanner
            error={submitError}
            retryAfterSeconds={cooldownSeconds > 0 ? cooldownSeconds : undefined}
          />
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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

        <div className="rounded-xl border border-white/[0.08] bg-[#14151c]/65 overflow-hidden">
          <div className="flex flex-wrap items-center justify-between gap-2 p-3.5">
            <label className="flex flex-1 min-w-0 items-start gap-2.5 cursor-pointer text-xs text-white/90">
              <input
                type="checkbox"
                {...register("emailNotificationsEnabled")}
                className="mt-0.5 size-4 shrink-0 rounded border-white/20 bg-[#14151c] text-[#d3fb52] focus:ring-[#d3fb52]"
              />
              <span className="min-w-0">
                <span className="block font-semibold text-white">
                  Send me email notifications
                </span>
                <span className="block text-[11px] text-[#9ca3af]">
                  Account updates, event notices, and optional digests
                </span>
              </span>
            </label>

            <button
              type="button"
              onClick={() => setEmailPrefsOpen((open) => !open)}
              aria-expanded={emailPrefsOpen}
              aria-controls="register-email-prefs-panel"
              aria-label="Customize email notification topics"
              className="inline-flex shrink-0 cursor-pointer items-center gap-1.5 rounded-lg border border-white/10 bg-[#1b1d26] px-2.5 py-1.5 text-[11px] font-semibold text-white/85 transition-colors hover:border-[#d3fb52]/40 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
            >
              <span className={watchedEmailNotificationsEnabled ? "text-[#d3fb52]" : "text-[#9ca3af]"}>
                {emailPrefsSummary}
              </span>
              <ChevronDown
                className={`size-3.5 transition-transform duration-200 ${
                  emailPrefsOpen ? "rotate-180 text-[#d3fb52]" : "text-white/70"
                }`}
                aria-hidden="true"
              />
            </button>
          </div>

          <div
            id="register-email-prefs-panel"
            hidden={!emailPrefsOpen}
            className="space-y-2.5 border-t border-white/[0.08] bg-[#101118]/75 px-3.5 py-3"
          >
            <label
              className={`flex items-start gap-2.5 pl-6 text-xs ${
                watchedEmailNotificationsEnabled
                  ? "cursor-pointer text-white/80"
                  : "cursor-not-allowed text-white/40"
              }`}
            >
              <input
                type="checkbox"
                disabled={!watchedEmailNotificationsEnabled}
                {...register("receiveLeaderboardDigests")}
                className="mt-0.5 size-3.5 shrink-0 rounded border-white/20 bg-[#14151c] text-[#d3fb52] focus:ring-[#d3fb52]"
              />
              <span>Leaderboard standings &amp; campus news digest</span>
            </label>

            <label
              className={`flex items-start gap-2.5 pl-6 text-xs ${
                watchedEmailNotificationsEnabled
                  ? "cursor-pointer text-white/80"
                  : "cursor-not-allowed text-white/40"
              }`}
            >
              <input
                type="checkbox"
                disabled={!watchedEmailNotificationsEnabled}
                {...register("receiveProjectMatchEmails")}
                className="mt-0.5 size-3.5 shrink-0 rounded border-white/20 bg-[#14151c] text-[#d3fb52] focus:ring-[#d3fb52]"
              />
              <span>Recruiting project recommendations matching my skills</span>
            </label>

            <p className="pl-6 text-[11px] leading-relaxed text-[#9ca3af]">
              Essential security messages such as password resets are always delivered. Manage your
              visibility and notification choices anytime in Profile Studio.
            </p>
          </div>
        </div>

        <p className="text-[11px] leading-relaxed text-[#9ca3af]">
          Your university email is private by default (THR-I-01) and never shown to other students
          unless you opt in. Read our{" "}
          <Link
            to="/privacy"
            className="font-semibold text-[#d3fb52] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
          >
            Privacy &amp; Policy
          </Link>
          .
        </p>

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
