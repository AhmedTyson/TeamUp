/**
 * @date 2026-11-22
 * @purpose PlatformAdmin Universities & nested Faculties CRUD console with STRIDE THR-T-03 safe URL validation.
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { Building2, ExternalLink, Pencil, Plus, Trash2 } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import type { UniversitySummary } from "@/api";
import {
  Badge,
  Button,
  ConfirmDialog,
  DataTable,
  EmptyState,
  ErrorBanner,
  Input,
  Modal,
  Pagination,
  SearchInput,
  type DataTableColumn,
} from "@/components/ui";
import { mapApiErrorsToForm } from "@/lib/formErrors";
import { isSafeHttpUrl } from "@/lib/urlValidation";
import { AdminConsoleLayout } from "../components/AdminConsoleLayout";
import { FacultyManagerPanel } from "../components/FacultyManagerPanel";
import {
  useCreateUniversityMutation,
  useDeleteUniversityMutation,
  useUniversitiesListQuery,
  useUpdateUniversityMutation,
} from "../hooks/useAdminHooks";
import { universityFormSchema, type UniversityFormValues } from "../schemas/adminSchemas";

export function AdminCatalogPage() {
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [selectedUniversity, setSelectedUniversity] = useState<UniversitySummary | null>(null);
  const [editingUniversity, setEditingUniversity] = useState<UniversitySummary | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [deletingUniversity, setDeletingUniversity] = useState<UniversitySummary | null>(null);
  const [formError, setFormError] = useState<unknown>(null);

  const universitiesQuery = useUniversitiesListQuery({
    search: search.trim() || undefined,
    page,
    pageSize: 10,
  });

  const createMutation = useCreateUniversityMutation();
  const updateMutation = useUpdateUniversityMutation();
  const deleteMutation = useDeleteUniversityMutation();

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<UniversityFormValues>({
    resolver: zodResolver(universityFormSchema),
    defaultValues: { name: "", city: "", country: "", website: "" },
  });

  const openCreateModal = () => {
    setEditingUniversity(null);
    setFormError(null);
    reset({ name: "", city: "", country: "", website: "" });
    setIsModalOpen(true);
  };

  const openEditModal = (uni: UniversitySummary) => {
    setEditingUniversity(uni);
    setFormError(null);
    reset({
      name: uni.name,
      city: uni.city,
      country: uni.country,
      website: uni.website ?? "",
    });
    setIsModalOpen(true);
  };

  const onSubmit = async (values: UniversityFormValues) => {
    setFormError(null);
    const payload = {
      name: values.name.trim(),
      city: values.city.trim(),
      country: values.country.trim(),
      website: values.website.trim() || null,
    };

    try {
      if (editingUniversity) {
        await updateMutation.mutateAsync({ id: editingUniversity.universityId, payload });
        toast.success(`Updated "${payload.name}".`);
      } else {
        await createMutation.mutateAsync(payload);
        toast.success(`Created "${payload.name}".`);
      }
      setIsModalOpen(false);
    } catch (error: unknown) {
      mapApiErrorsToForm(error, setError, ["name", "city", "country", "website"]);
      setFormError(error);
    }
  };

  const handleConfirmDelete = async () => {
    if (!deletingUniversity) {
      return;
    }
    try {
      await deleteMutation.mutateAsync(deletingUniversity.universityId);
      toast.success(`"${deletingUniversity.name}" moved to recycle bin.`);
      if (selectedUniversity?.universityId === deletingUniversity.universityId) {
        setSelectedUniversity(null);
      }
      setDeletingUniversity(null);
    } catch (error: unknown) {
      toast.error(error instanceof Error ? error.message : "Failed to delete university.");
    }
  };

  const columns: DataTableColumn<UniversitySummary>[] = [
    {
      key: "name",
      header: "University",
      cell: (row) => (
        <div className="space-y-0.5">
          <p className="font-semibold text-white">{row.name}</p>
          <p className="text-xs text-[#9ca3af]">
            {row.city}, {row.country}
          </p>
        </div>
      ),
    },
    {
      key: "website",
      header: "Website",
      cell: (row) =>
        row.website && isSafeHttpUrl(row.website) ? (
          <a
            href={row.website}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center gap-1 text-xs font-medium text-[#d3fb52] hover:underline"
          >
            {row.website}
            <ExternalLink className="size-3" aria-hidden="true" />
          </a>
        ) : (
          <span className="text-xs text-white/45">No public URL</span>
        ),
    },
    {
      key: "facultyCount",
      header: "Faculties",
      cell: (row) => <Badge variant="default">{row.facultyCount}</Badge>,
    },
    {
      key: "actions",
      header: "Actions",
      cell: (row) => (
        <div className="flex flex-wrap items-center gap-2">
          <Button
            size="sm"
            variant={
              selectedUniversity?.universityId === row.universityId ? "primary" : "secondary"
            }
            onClick={() => setSelectedUniversity(row)}
          >
            Manage Faculties
          </Button>
          <Button
            size="sm"
            variant="outline"
            leftIcon={<Pencil className="size-3.5" aria-hidden="true" />}
            onClick={() => openEditModal(row)}
          >
            Edit
          </Button>
          <Button
            size="sm"
            variant="danger"
            leftIcon={<Trash2 className="size-3.5" aria-hidden="true" />}
            onClick={() => setDeletingUniversity(row)}
          >
            Delete
          </Button>
        </div>
      ),
    },
  ];

  const isSaving = createMutation.isPending || updateMutation.isPending;

  return (
    <AdminConsoleLayout
      title="Academic Catalog Governance"
      description="Create, update, or soft-delete universities and manage nested faculties used across student registration."
      actions={
        <Button
          leftIcon={<Plus className="size-4" aria-hidden="true" />}
          onClick={openCreateModal}
        >
          Add University
        </Button>
      }
    >
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <SearchInput
          value={search}
          onChange={(value) => {
            setSearch(value);
            setPage(1);
          }}
          placeholder="Search universities by name or city…"
          containerClassName="sm:max-w-sm"
        />
      </div>

      {universitiesQuery.isError && (
        <ErrorBanner
          error={universitiesQuery.error}
          onRetry={() => {
            void universitiesQuery.refetch();
          }}
        />
      )}

      <DataTable
        columns={columns}
        data={universitiesQuery.data?.items ?? []}
        keyExtractor={(row) => row.universityId}
        isLoading={universitiesQuery.isPending}
        emptyState={
          <EmptyState
            icon={<Building2 className="size-6" aria-hidden="true" />}
            title="No universities match your search"
            description="Create a university or clear the current filter to view all academic institutions."
            action={<Button onClick={openCreateModal}>Add University</Button>}
          />
        }
      />

      {universitiesQuery.data && (
        <Pagination
          page={universitiesQuery.data.page}
          totalPages={universitiesQuery.data.totalPages}
          totalCount={universitiesQuery.data.totalCount}
          pageSize={universitiesQuery.data.pageSize}
          onPageChange={setPage}
        />
      )}

      {selectedUniversity && (
        <FacultyManagerPanel
          universityId={selectedUniversity.universityId}
          universityName={selectedUniversity.name}
        />
      )}

      <Modal
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingUniversity ? "Edit University" : "Add University"}
        description="External website URLs must use public http:// or https:// protocols (STRIDE THR-T-03)."
      >
        <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
          {formError !== null && <ErrorBanner error={formError} />}

          <Input
            label="University Name"
            placeholder="Cairo University"
            required
            error={errors.name?.message}
            {...register("name")}
          />

          <div className="grid gap-4 sm:grid-cols-2">
            <Input
              label="City"
              placeholder="Giza"
              required
              error={errors.city?.message}
              {...register("city")}
            />
            <Input
              label="Country"
              placeholder="Egypt"
              required
              error={errors.country?.message}
              {...register("country")}
            />
          </div>

          <Input
            label="Official Website"
            type="url"
            placeholder="https://cu.edu.eg"
            hint="Optional public HTTP/HTTPS URL (localhost and non-HTTP schemes are rejected)."
            error={errors.website?.message}
            {...register("website")}
          />

          <div className="flex justify-end gap-2 pt-2">
            <Button variant="ghost" onClick={() => setIsModalOpen(false)} disabled={isSaving}>
              Cancel
            </Button>
            <Button type="submit" isLoading={isSaving}>
              {editingUniversity ? "Save Changes" : "Create University"}
            </Button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        open={deletingUniversity !== null}
        onClose={() => setDeletingUniversity(null)}
        onConfirm={handleConfirmDelete}
        title="Soft-delete university?"
        description={`"${deletingUniversity?.name ?? ""}" and its catalog visibility will be archived. You can restore it later from /admin/restore.`}
        confirmLabel="Soft Delete"
        isLoading={deleteMutation.isPending}
      />
    </AdminConsoleLayout>
  );
}
