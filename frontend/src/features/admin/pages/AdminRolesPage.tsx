/**
 * @date 2026-11-22
 * @purpose PlatformAdmin Student Role Governance view with THR-E-02 SecurityStamp revocation notice and THR-E-03 PlatformAdmin guard handling.
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { ShieldAlert, UserCog, Users } from "lucide-react";
import { useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { toast } from "sonner";
import {
  USER_ROLES,
  type AdminStudentDirectoryItem,
  type UserRole,
} from "@/api";
import {
  Badge,
  Button,
  DataTable,
  EmptyState,
  ErrorBanner,
  Input,
  Modal,
  Pagination,
  SearchInput,
  Select,
  type DataTableColumn,
  type SelectOption,
} from "@/components/ui";
import { AdminConsoleLayout } from "../components/AdminConsoleLayout";
import {
  useAdminStudentSearchQuery,
  useUpdateStudentRoleMutation,
} from "../hooks/useAdminHooks";
import { roleUpdateSchema, type RoleUpdateFormValues } from "../schemas/adminSchemas";

const ROLE_OPTIONS: readonly SelectOption[] = [
  { value: USER_ROLES.Student, label: "Student (Standard Campus Member)" },
  { value: USER_ROLES.OrgAdmin, label: "OrgAdmin (Can Create & Manage Organizations)" },
  {
    value: USER_ROLES.PlatformAdmin,
    label: "PlatformAdmin (Guarded by THR-E-03 Server Policy)",
  },
];

export function AdminRolesPage() {
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [targetLabel, setTargetLabel] = useState<string>("");
  const [mutationError, setMutationError] = useState<unknown>(null);

  const studentsQuery = useAdminStudentSearchQuery({
    search: search.trim() || undefined,
    page,
    pageSize: 10,
  });

  const updateRoleMutation = useUpdateStudentRoleMutation();

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<RoleUpdateFormValues>({
    resolver: zodResolver(roleUpdateSchema),
    defaultValues: { studentId: "", role: USER_ROLES.Student },
  });

  const selectedRole = useWatch({ control, name: "role" });

  const openModalForStudent = (student?: AdminStudentDirectoryItem) => {
    setMutationError(null);
    if (student) {
      setTargetLabel(`${student.firstName} ${student.lastName} (#${student.studentId})`);
      reset({ studentId: String(student.studentId), role: student.role });
    } else {
      setTargetLabel("Target Student Account");
      reset({ studentId: "", role: USER_ROLES.OrgAdmin });
    }
    setModalOpen(true);
  };

  const onSubmit = async (values: RoleUpdateFormValues) => {
    setMutationError(null);
    const parsedId = Number.parseInt(values.studentId, 10);

    try {
      const updated = await updateRoleMutation.mutateAsync({
        studentId: parsedId,
        role: values.role as UserRole,
      });
      toast.success(
        `Updated ${updated.firstName} ${updated.lastName} to ${updated.role}. Active tokens revoked.`,
      );
      setModalOpen(false);
    } catch (error: unknown) {
      setMutationError(error);
    }
  };

  const columns: DataTableColumn<AdminStudentDirectoryItem>[] = [
    {
      key: "student",
      header: "Student",
      cell: (row) => (
        <div className="space-y-0.5">
          <p className="font-semibold text-white">
            {row.firstName} {row.lastName}{" "}
            <span className="font-mono text-xs text-[#9ca3af]">#{row.studentId}</span>
          </p>
          <p className="text-xs text-[#9ca3af]">{row.email ?? "Email private"}</p>
        </div>
      ),
    },
    {
      key: "affiliation",
      header: "Academic Affiliation",
      cell: (row) => (
        <div className="text-xs">
          <p className="text-white/90">{row.universityName}</p>
          <p className="text-[#9ca3af]">
            {row.facultyName} · Year {row.academicYear}
          </p>
        </div>
      ),
    },
    {
      key: "role",
      header: "Current Role",
      cell: (row) => (
        <Badge
          variant={
            row.role === USER_ROLES.PlatformAdmin
              ? "accent"
              : row.role === USER_ROLES.OrgAdmin
                ? "info"
                : "default"
          }
        >
          {row.role}
        </Badge>
      ),
    },
    {
      key: "action",
      header: "Governance",
      cell: (row) => (
        <Button
          size="sm"
          variant="secondary"
          leftIcon={<UserCog className="size-3.5" aria-hidden="true" />}
          onClick={() => openModalForStudent(row)}
        >
          Change Role
        </Button>
      ),
    },
  ];

  return (
    <AdminConsoleLayout
      title="Student Role Governance"
      description="Promote or demote Student and OrgAdmin roles. Changing a role immediately rotates the user's SecurityStamp and revokes active JWTs."
      actions={
        <Button
          variant="secondary"
          leftIcon={<UserCog className="size-4" aria-hidden="true" />}
          onClick={() => openModalForStudent()}
        >
          Update by Student ID
        </Button>
      }
    >
      <div className="flex items-start gap-3 rounded-2xl border border-amber-500/35 bg-amber-500/10 p-4 text-xs text-amber-100">
        <ShieldAlert className="mt-0.5 size-5 shrink-0 text-amber-300" aria-hidden="true" />
        <div className="space-y-1">
          <p className="font-semibold">
            Security Stamp Revocation (THR-E-02) &amp; Elevation Guard (THR-E-03)
          </p>
          <p className="text-amber-100/80">
            Updating any student&apos;s role immediately invalidates their active access and refresh
            tokens. Runtime promotion to <strong className="text-white">PlatformAdmin</strong> is
            restricted by default on the API and returns <code>403 Forbidden</code> unless bootstrap
            overrides are explicitly enabled.
          </p>
        </div>
      </div>

      <SearchInput
        value={search}
        onChange={(value) => {
          setSearch(value);
          setPage(1);
        }}
        placeholder="Search student directory by name or email…"
        containerClassName="sm:max-w-sm"
      />

      {studentsQuery.isError && (
        <ErrorBanner
          error={studentsQuery.error}
          onRetry={() => {
            void studentsQuery.refetch();
          }}
        />
      )}

      <DataTable
        columns={columns}
        data={studentsQuery.data?.items ?? []}
        keyExtractor={(row) => row.studentId}
        isLoading={studentsQuery.isPending}
        emptyState={
          <EmptyState
            icon={<Users className="size-6" aria-hidden="true" />}
            title="No students found"
            description="Adjust your search query or update a role directly using a known Student ID."
            action={
              <Button variant="secondary" onClick={() => openModalForStudent()}>
                Update by Student ID
              </Button>
            }
          />
        }
      />

      {studentsQuery.data && (
        <Pagination
          page={studentsQuery.data.page}
          totalPages={studentsQuery.data.totalPages}
          totalCount={studentsQuery.data.totalCount}
          pageSize={studentsQuery.data.pageSize}
          onPageChange={setPage}
        />
      )}

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title={`Update Role · ${targetLabel}`}
        description="Assign a new platform role. Target user sessions will be revoked immediately."
      >
        <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
          {mutationError !== null && <ErrorBanner error={mutationError} />}

          <Input
            label="Student ID"
            type="number"
            min={1}
            required
            error={errors.studentId?.message}
            {...register("studentId")}
          />

          <Select
            label="Target Role"
            required
            options={ROLE_OPTIONS}
            error={errors.role?.message}
            {...register("role")}
          />

          {selectedRole === USER_ROLES.PlatformAdmin && (
            <ErrorBanner
              variant="warning"
              title="THR-E-03 PlatformAdmin Elevation Guard"
              message="Runtime promotion to PlatformAdmin is rejected by default on the server (403 Forbidden) and must normally be provisioned via bootstrap seeders."
            />
          )}

          <div className="rounded-xl border border-white/10 bg-[#14151c]/75 px-3.5 py-2.5 text-xs text-[#9ca3af]">
            Warning: Applying this change rotates the user&apos;s SecurityStamp, revoking all
            active access and refresh tokens immediately.
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <Button
              variant="ghost"
              onClick={() => setModalOpen(false)}
              disabled={updateRoleMutation.isPending}
            >
              Cancel
            </Button>
            <Button type="submit" isLoading={updateRoleMutation.isPending}>
              Confirm Role Update
            </Button>
          </div>
        </form>
      </Modal>
    </AdminConsoleLayout>
  );
}
