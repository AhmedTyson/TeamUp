/**
 * @date 2026-11-22
 * @purpose PlatformAdmin Universities & nested Faculties Master-Detail governance console with database logo preview and URL-synced pagination.
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { Building2, ExternalLink, Pencil, Plus, Trash2 } from "lucide-react";
import { useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { useSearchParams } from "react-router-dom";
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
  UniversityLogo,
  type DataTableColumn,
} from "@/components/ui";
import { cn } from "@/lib/cn";
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

const DEFAULT_PAGE_SIZE = 6;

export function AdminCatalogPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const initialPage = Math.max(1, Number.parseInt(searchParams.get("page") ?? "1", 10) || 1);
  const initialSizeRaw = Number.parseInt(searchParams.get("pageSize") ?? String(DEFAULT_PAGE_SIZE), 10);
  const initialPageSize = [6, 12, 24].includes(initialSizeRaw) ? initialSizeRaw : DEFAULT_PAGE_SIZE;
  const initialSearch = searchParams.get("search") ?? "";

  const [search, setSearch] = useState(initialSearch);
  const [page, setPage] = useState(initialPage);
  const [pageSize, setPageSize] = useState(initialPageSize);
  const [selectedUniversity, setSelectedUniversity] = useState<UniversitySummary | null>(null);
  const [editingUniversity, setEditingUniversity] = useState<UniversitySummary | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [deletingUniversity, setDeletingUniversity] = useState<UniversitySummary | null>(null);
  const [formError, setFormError] = useState<unknown>(null);

  const syncQueryParams = (nextPage: number, nextPageSize: number, nextSearch: string) => {
    const next = new URLSearchParams();
    if (nextPage > 1) next.set("page", String(nextPage));
    if (nextPageSize !== DEFAULT_PAGE_SIZE) next.set("pageSize", String(nextPageSize));
    if (nextSearch.trim()) next.set("search", nextSearch.trim());
    setSearchParams(next, { replace: true });
  };

  const universitiesQuery = useUniversitiesListQuery({
    search: search.trim() || undefined,
    page,
    pageSize,
  });

  const createMutation = useCreateUniversityMutation();
  const updateMutation = useUpdateUniversityMutation();
  const deleteMutation = useDeleteUniversityMutation();

  const {
    register,
    control,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<UniversityFormValues>({
    resolver: zodResolver(universityFormSchema),
    defaultValues: { name: "", city: "", country: "", website: "", logoUrl: "" },
  });

  const watchedName = useWatch({ control, name: "name" }) ?? "";
  const watchedLogoUrl = useWatch({ control, name: "logoUrl" }) ?? "";

  const openCreateModal = () => {
    setEditingUniversity(null);
    setFormError(null);
    reset({ name: "", city: "", country: "", website: "", logoUrl: "" });
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
      logoUrl: uni.logoUrl ?? "",
    });
    setIsModalOpen(true);
  };

  const onSubmit = async (values: UniversityFormValues) => {
    setFormError(null);
    const trimmedLogo = values.logoUrl?.trim() ?? "";
    const payload = {
      name: values.name.trim(),
      city: values.city.trim(),
      country: values.country.trim(),
      website: values.website.trim() || null,
      ...(trimmedLogo ? { logoUrl: trimmedLogo } : editingUniversity ? { logoUrl: null } : {}),
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
      mapApiErrorsToForm(error, setError, ["name", "city", "country", "website", "logoUrl"]);
      setFormError(error);
    }
  };

  const handleConfirmDelete = async () => {
    if (!deletingUniversity) {
      return;
    }
    try {
      await deleteMutation.mutateAsync(deletingUniversity.universityId);
      toast.success(`"${deletingUniversity.name}" moved to archive.`);
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
      cellClassName: "min-w-0",
      cell: (row) => {
        const isSelected = selectedUniversity?.universityId === row.universityId;
        return (
          <div className="flex min-w-0 items-center gap-3">
            <UniversityLogo
              name={row.name}
              logoUrl={row.logoUrl}
              size="md"
              className={cn("shrink-0", isSelected && "ring-2 ring-[#d3fb52]")}
            />
            <div className="min-w-0 space-y-0.5">
              <p title={row.name} className="truncate font-semibold text-white">{row.name}</p>
              <p className="truncate text-xs text-[#9ca3af]">
                {row.city}, {row.country}
              </p>
            </div>
          </div>
        );
      },
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
            className="inline-flex items-center gap-1 text-xs font-medium text-[#d3fb52] no-underline transition-colors hover:text-[#e6ff8a]"
          >
            <span className="max-w-[180px] truncate" title={row.website}>
              {row.website}
            </span>
            <ExternalLink className="size-3 shrink-0" aria-hidden="true" />
          </a>
        ) : (
          <span className="text-xs text-white/45">No public URL</span>
        ),
    },
    {
      key: "facultyCount",
      header: "Faculties",
      cellClassName: "whitespace-nowrap",
      cell: (row) => (
        <Badge variant="default" className="tabular-nums">
          {row.facultyCount}
        </Badge>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      cellClassName: "whitespace-nowrap",
      cell: (row) => {
        const isSelected = selectedUniversity?.universityId === row.universityId;
        return (
          <div className="flex flex-wrap items-center gap-1.5">
            <Button
              size="sm"
              variant={isSelected ? "primary" : "secondary"}
              onClick={() => setSelectedUniversity(isSelected ? null : row)}
            >
              {isSelected ? "Viewing Faculties" : "Manage Faculties"}
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
        );
      },
    },
  ];

  const isSaving = createMutation.isPending || updateMutation.isPending;

  return (
    <AdminConsoleLayout
      title="Academic Catalog Governance"
      description="Manage partner universities, official websites, and academic faculties across the network."
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
            syncQueryParams(1, pageSize, value);
          }}
          placeholder="Search universities by name or city…"
          containerClassName="w-full sm:w-80 sm:max-w-sm"
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

      {/* Master-Detail Split Layout: compresses table to 7/12 columns when Faculty inspector opens */}
      <div className="grid items-start gap-6 lg:grid-cols-12">
        <div
          className={cn(
            "space-y-4 transition-all duration-300",
            selectedUniversity ? "lg:col-span-7" : "lg:col-span-12",
          )}
        >
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
              pageSizeOptions={[6, 12, 24]}
              onPageChange={(nextPage) => {
                setPage(nextPage);
                syncQueryParams(nextPage, pageSize, search);
              }}
              onPageSizeChange={(nextSize) => {
                setPageSize(nextSize);
                setPage(1);
                syncQueryParams(1, nextSize, search);
              }}
            />
          )}
        </div>

        {selectedUniversity && (
          <div className="lg:col-span-5">
            <FacultyManagerPanel
              universityId={selectedUniversity.universityId}
              universityName={selectedUniversity.name}
              universityLogoUrl={selectedUniversity.logoUrl}
              onClose={() => setSelectedUniversity(null)}
            />
          </div>
        )}
      </div>

      <Modal
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingUniversity ? "Edit University" : "Add University"}
        description="Enter the official university name, campus city, public website, and emblem path."
      >
        <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
          {formError !== null && <ErrorBanner error={formError} />}

          <div className="flex items-center gap-3.5 rounded-2xl border border-white/10 bg-[#14151c] p-3.5">
            <UniversityLogo
              name={watchedName || "University"}
              logoUrl={watchedLogoUrl}
              size="lg"
            />
            <div className="min-w-0">
              <p className="truncate text-sm font-semibold text-white">
                {watchedName.trim() || "Official Institutional Emblem"}
              </p>
              <p className="text-xs text-[#9ca3af]">
                Live preview rendered from database logo URL
              </p>
            </div>
          </div>

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
            hint="Optional public website URL (e.g. https://cu.edu.eg)."
            error={errors.website?.message}
            {...register("website")}
          />

          <Input
            label="Logo URL"
            placeholder="https://upload.wikimedia.org/wikipedia/commons/3/3c/Cairo_University_new_logo.png"
            hint="Official public HTTPS university logo image URL."
            error={errors.logoUrl?.message}
            {...register("logoUrl")}
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
        title="Archive university?"
        description={`"${deletingUniversity?.name ?? ""}" will be hidden from student registration. You can restore it anytime from the Archive.`}
        confirmLabel="Archive University"
        isLoading={deleteMutation.isPending}
      />
    </AdminConsoleLayout>
  );
}
