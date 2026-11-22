/**
 * @date 2026-11-22
 * @purpose Nested faculty CRUD inspector panel displayed side-by-side in the AdminCatalogPage Master-Detail split layout.
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { Pencil, Plus, Trash2, X } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import type { FacultyDetail } from "@/api";
import {
  Badge,
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  ConfirmDialog,
  EmptyState,
  ErrorBanner,
  Input,
  Modal,
  Skeleton,
  UniversityLogo,
} from "@/components/ui";
import { mapApiErrorsToForm } from "@/lib/formErrors";
import {
  useCreateFacultyMutation,
  useDeleteFacultyMutation,
  useUniversityFacultiesQuery,
  useUpdateFacultyMutation,
} from "../hooks/useAdminHooks";
import { facultyFormSchema, type FacultyFormValues } from "../schemas/adminSchemas";

export interface FacultyManagerPanelProps {
  universityId: number;
  universityName: string;
  universityLogoUrl?: string | null;
  onClose?: () => void;
}

export function FacultyManagerPanel({
  universityId,
  universityName,
  universityLogoUrl,
  onClose,
}: FacultyManagerPanelProps) {
  const facultiesQuery = useUniversityFacultiesQuery(universityId);
  const createMutation = useCreateFacultyMutation();
  const updateMutation = useUpdateFacultyMutation();
  const deleteMutation = useDeleteFacultyMutation();

  const [editingFaculty, setEditingFaculty] = useState<FacultyDetail | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [deletingFaculty, setDeletingFaculty] = useState<FacultyDetail | null>(null);
  const [formError, setFormError] = useState<unknown>(null);

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<FacultyFormValues>({
    resolver: zodResolver(facultyFormSchema),
    defaultValues: { name: "", code: "" },
  });

  const openCreateModal = () => {
    setEditingFaculty(null);
    setFormError(null);
    reset({ name: "", code: "" });
    setIsModalOpen(true);
  };

  const openEditModal = (faculty: FacultyDetail) => {
    setEditingFaculty(faculty);
    setFormError(null);
    reset({ name: faculty.name, code: faculty.code });
    setIsModalOpen(true);
  };

  const onSubmit = async (values: FacultyFormValues) => {
    setFormError(null);
    const payload = { name: values.name.trim(), code: values.code.trim().toUpperCase() };

    try {
      if (editingFaculty) {
        await updateMutation.mutateAsync({
          facultyId: editingFaculty.facultyId,
          universityId,
          payload,
        });
        toast.success(`Faculty "${payload.name}" updated.`);
      } else {
        await createMutation.mutateAsync({ universityId, payload });
        toast.success(`Faculty "${payload.name}" added to ${universityName}.`);
      }
      setIsModalOpen(false);
    } catch (error: unknown) {
      mapApiErrorsToForm(error, setError, ["name", "code"]);
      setFormError(error);
    }
  };

  const handleConfirmDelete = async () => {
    if (!deletingFaculty) {
      return;
    }
    try {
      await deleteMutation.mutateAsync({
        facultyId: deletingFaculty.facultyId,
        universityId,
      });
      toast.success(`Faculty "${deletingFaculty.name}" moved to archive.`);
      setDeletingFaculty(null);
    } catch (error: unknown) {
      toast.error(error instanceof Error ? error.message : "Failed to delete faculty.");
    }
  };

  const isSaving = createMutation.isPending || updateMutation.isPending;

  return (
    <Card variant="elevated" className="lg:sticky lg:top-20">
      <CardHeader className="flex flex-row items-start justify-between gap-3 border-b border-white/[0.085]">
        <div className="space-y-1">
          <CardTitle className="flex items-center gap-2.5 text-base">
            <UniversityLogo name={universityName} logoUrl={universityLogoUrl} size="sm" />
            <span>Faculties · {universityName}</span>
          </CardTitle>
          <CardDescription className="text-xs">
            Add or edit accredited faculties and academic codes for this university.
          </CardDescription>
        </div>

        <div className="flex shrink-0 items-center gap-1.5">
          <Button
            size="sm"
            leftIcon={<Plus className="size-3.5" aria-hidden="true" />}
            onClick={openCreateModal}
          >
            Add Faculty
          </Button>
          {onClose && (
            <button
              type="button"
              onClick={onClose}
              aria-label="Close faculty inspector"
              className="inline-flex size-8 cursor-pointer items-center justify-center rounded-lg border border-white/10 bg-[#14151c] text-white/65 transition-colors hover:border-white/25 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
            >
              <X className="size-4" aria-hidden="true" />
            </button>
          )}
        </div>
      </CardHeader>

      <CardContent className="space-y-3 pt-4">
        {facultiesQuery.isError && (
          <ErrorBanner
            error={facultiesQuery.error}
            onRetry={() => {
              void facultiesQuery.refetch();
            }}
          />
        )}

        {facultiesQuery.isPending ? (
          <div className="space-y-2">
            <Skeleton className="h-14 w-full" />
            <Skeleton className="h-14 w-full" />
          </div>
        ) : (facultiesQuery.data?.length ?? 0) === 0 ? (
          <EmptyState
            title="No faculties listed yet"
            description="Add the first faculty so students from this university can register."
            action={
              <Button size="sm" onClick={openCreateModal}>
                Add Faculty
              </Button>
            }
          />
        ) : (
          <div className="divide-y divide-white/[0.08] rounded-xl border border-white/[0.085] bg-[#14151c]/75">
            {facultiesQuery.data?.map((faculty) => (
              <div
                key={faculty.facultyId}
                className="flex flex-col justify-between gap-2.5 px-3.5 py-3 sm:flex-row sm:items-center"
              >
                <div className="min-w-0 space-y-0.5">
                  <div className="flex flex-wrap items-center gap-2">
                    <span className="truncate text-sm font-semibold text-white">
                      {faculty.name}
                    </span>
                    <Badge variant="accent" size="sm">
                      {faculty.code}
                    </Badge>
                  </div>
                  <p className="text-xs text-[#9ca3af] tabular-nums">
                    {faculty.studentCount} enrolled student(s)
                  </p>
                </div>

                <div className="flex shrink-0 items-center gap-1.5">
                  <Button
                    variant="secondary"
                    size="sm"
                    leftIcon={<Pencil className="size-3" aria-hidden="true" />}
                    onClick={() => openEditModal(faculty)}
                  >
                    Edit
                  </Button>
                  <Button
                    variant="danger"
                    size="sm"
                    leftIcon={<Trash2 className="size-3" aria-hidden="true" />}
                    onClick={() => setDeletingFaculty(faculty)}
                  >
                    Delete
                  </Button>
                </div>
              </div>
            ))}
          </div>
        )}
      </CardContent>

      <Modal
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingFaculty ? "Edit Faculty" : "Add Faculty"}
        description={`Parent university: ${universityName}`}
      >
        <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
          {formError !== null && <ErrorBanner error={formError} />}

          <Input
            label="Faculty Name"
            placeholder="Faculty of Computers and Artificial Intelligence"
            required
            error={errors.name?.message}
            {...register("name")}
          />

          <Input
            label="Academic Code"
            placeholder="FCAI"
            hint="Alphanumeric, dashes, or underscores (max 20 chars)."
            required
            error={errors.code?.message}
            {...register("code")}
          />

          <div className="flex justify-end gap-2 pt-2">
            <Button variant="ghost" onClick={() => setIsModalOpen(false)} disabled={isSaving}>
              Cancel
            </Button>
            <Button type="submit" isLoading={isSaving}>
              {editingFaculty ? "Save Faculty" : "Create Faculty"}
            </Button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        open={deletingFaculty !== null}
        onClose={() => setDeletingFaculty(null)}
        onConfirm={handleConfirmDelete}
        title="Archive faculty?"
        description={`"${deletingFaculty?.name ?? ""}" will be hidden from student registration. You can restore it anytime from the Archive.`}
        confirmLabel="Archive Faculty"
        isLoading={deleteMutation.isPending}
      />
    </Card>
  );
}
