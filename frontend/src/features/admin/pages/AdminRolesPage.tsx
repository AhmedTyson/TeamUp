/**
 * @date 2026-11-22
 * @purpose PlatformAdmin Student Role Governance view with THR-E-02 SecurityStamp revocation notice and THR-E-03 PlatformAdmin guard handling.
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { UserCog, Users } from "lucide-react";
import { useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { toast } from "sonner";
import {
  USER_ROLES,
  type AdminStudentDirectoryItem,
  type UserRole,
} from "@/api";
import {
  Avatar,
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
    label: "PlatformAdmin (System Administrator)",
  },
];

const ROLE_FILTER_PILLS = [
  { id: "ALL", label: "All Roles" },
  { id: USER_ROLES.Student, label: "Students" },
  { id: USER_ROLES.OrgAdmin, label: "Org Admins" },
  { id: USER_ROLES.PlatformAdmin, label: "Platform Admins" },
] as const;

export function AdminRolesPage() {
  const [search, setSearch] = useState("");
  const [roleFilter, setRoleFilter] = useState<string>("ALL");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [modalOpen, setModalOpen] = useState(false);
  const [targetLabel, setTargetLabel] = useState<string>("");
  const [mutationError, setMutationError] = useState<unknown>(null);

  const studentsQuery = useAdminStudentSearchQuery({
    search: search.trim() || undefined,
    page,
    pageSize,
  });

  const filteredStudents =
    roleFilter === "ALL"
      ? (studentsQuery.data?.items ?? [])
      : (studentsQuery.data?.items ?? []).filter((item) => item.role === roleFilter);

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
      setTargetLabel(`${student.firstName} ${student.lastName}`);
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
        `Updated ${updated.firstName} ${updated.lastName} to ${updated.role}.`,
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
        <div className="flex items-center gap-3">
          <Avatar name={`${row.firstName} ${row.lastName}`} size="sm" />
          <div className="space-y-0.5">
            <p className="font-semibold text-white">
              {row.firstName} {row.lastName}
            </p>
            <p className="text-xs text-[#9ca3af]">{row.email ?? "Email private"}</p>
          </div>
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
      description="Search the student directory and manage Organization Admin permissions across campus accounts."
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
      <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
        <SearchInput
          value={search}
          onChange={(value) => {
            setSearch(value);
            setPage(1);
          }}
          placeholder="Search student directory by name or email…"
          containerClassName="w-full sm:w-80 sm:max-w-sm"
        />

        <div className="flex flex-wrap items-center gap-2" role="group" aria-label="Role filter">
          {ROLE_FILTER_PILLS.map((pill) => (
            <button
              key={pill.id}
              type="button"
              onClick={() => setRoleFilter(pill.id)}
              className={`cursor-pointer rounded-full px-3.5 py-1.5 text-xs font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] ${
                roleFilter === pill.id
                  ? "bg-[#d3fb52] text-[#14151c]"
                  : "border border-white/12 bg-[#1b1d26] text-white/75 hover:text-white"
              }`}
            >
              {pill.label}
            </button>
          ))}
        </div>
      </div>

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
        data={filteredStudents}
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
          pageSizeOptions={[10, 20]}
          onPageChange={setPage}
          onPageSizeChange={(nextSize) => {
            setPageSize(nextSize);
            setPage(1);
          }}
        />
      )}

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title={`Update Role · ${targetLabel}`}
        description="Select the platform role for this student account."
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
              title="System Administrator Restriction"
              message="Runtime elevation to PlatformAdmin is restricted by default and must be provisioned by system configuration."
            />
          )}

          <div className="rounded-xl border border-white/10 bg-[#14151c]/75 px-3.5 py-2.5 text-xs text-[#9ca3af]">
            Note: Role changes take effect immediately on the student&apos;s active session.
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
