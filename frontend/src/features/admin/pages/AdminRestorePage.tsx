/**
 * @date 2026-11-22
 * @purpose Universal 8-entity Soft-Delete Recycle Bin allowing PlatformAdmins to filter and restore deleted records cleanly.
 */
import { RotateCcw, Trash2 } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";
import {
  RESTORABLE_ENTITY_TYPES,
  type RestorableEntityType,
  type SoftDeletedItem,
} from "@/api";
import {
  Badge,
  Button,
  DataTable,
  EmptyState,
  ErrorBanner,
  type DataTableColumn,
} from "@/components/ui";
import { cn } from "@/lib/cn";
import { AdminConsoleLayout } from "../components/AdminConsoleLayout";
import { useRestoreEntityMutation, useSoftDeletedQuery } from "../hooks/useAdminHooks";

type FilterType = "all" | RestorableEntityType;

export function AdminRestorePage() {
  const [selectedType, setSelectedType] = useState<FilterType>("all");
  const [restoringKey, setRestoringKey] = useState<string | null>(null);
  const [restoreError, setRestoreError] = useState<unknown>(null);

  const softDeletedQuery = useSoftDeletedQuery(selectedType);
  const restoreMutation = useRestoreEntityMutation();

  const handleRestore = async (item: SoftDeletedItem) => {
    const key = `${item.entityType}-${item.id}`;
    setRestoringKey(key);
    setRestoreError(null);

    try {
      const message = await restoreMutation.mutateAsync({
        entityType: item.entityType,
        id: item.id,
      });
      toast.success(message);
    } catch (error: unknown) {
      setRestoreError(error);
    } finally {
      setRestoringKey(null);
    }
  };

  const columns: DataTableColumn<SoftDeletedItem>[] = [
    {
      key: "entityType",
      header: "Entity Type",
      cell: (row) => <Badge variant="warning">{row.entityType}</Badge>,
    },
    {
      key: "id",
      header: "ID",
      cell: (row) => <span className="font-mono text-xs tabular-nums">#{row.id}</span>,
    },
    {
      key: "displayName",
      header: "Record",
      cell: (row) => <span className="font-medium text-white">{row.displayName}</span>,
    },
    {
      key: "deletedAt",
      header: "Deleted At",
      cell: (row) => (
        <span className="font-mono text-xs text-[#9ca3af] tabular-nums">
          {new Date(row.deletedAt).toLocaleString()}
        </span>
      ),
    },
    {
      key: "actions",
      header: "Action",
      cell: (row) => {
        const key = `${row.entityType}-${row.id}`;
        const isCurrent = restoringKey === key;
        return (
          <Button
            size="sm"
            variant="primary"
            isLoading={isCurrent}
            disabled={restoringKey !== null && !isCurrent}
            leftIcon={<RotateCcw className="size-3.5" aria-hidden="true" />}
            onClick={() => {
              void handleRestore(row);
            }}
          >
            Restore
          </Button>
        );
      },
    },
  ];

  return (
    <AdminConsoleLayout
      title="Soft-Delete Recycle Bin"
      description="Inspect and restore soft-deleted records across all 8 platform entity types with automatic cache invalidation."
    >
      <div className="flex flex-wrap items-center gap-2" role="group" aria-label="Entity type filters">
        <button
          type="button"
          onClick={() => setSelectedType("all")}
          className={cn(
            "cursor-pointer rounded-full px-3 py-1.5 text-xs font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
            selectedType === "all"
              ? "bg-[#d3fb52] text-[#14151c]"
              : "border border-white/12 bg-[#1b1d26] text-white/75 hover:text-white",
          )}
        >
          All Entities
        </button>

        {Object.values(RESTORABLE_ENTITY_TYPES).map((type) => (
          <button
            key={type}
            type="button"
            onClick={() => setSelectedType(type)}
            className={cn(
              "cursor-pointer rounded-full px-3 py-1.5 text-xs font-semibold capitalize transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
              selectedType === type
                ? "bg-[#d3fb52] text-[#14151c]"
                : "border border-white/12 bg-[#1b1d26] text-white/75 hover:text-white",
            )}
          >
            {type}
          </button>
        ))}
      </div>

      {softDeletedQuery.isError && (
        <ErrorBanner
          error={softDeletedQuery.error}
          onRetry={() => {
            void softDeletedQuery.refetch();
          }}
        />
      )}

      {restoreError !== null && <ErrorBanner error={restoreError} />}

      <DataTable
        columns={columns}
        data={softDeletedQuery.data ?? []}
        keyExtractor={(row) => `${row.entityType}-${row.id}`}
        isLoading={softDeletedQuery.isPending}
        emptyState={
          <EmptyState
            icon={<Trash2 className="size-6" aria-hidden="true" />}
            title="Recycle bin is empty"
            description="No soft-deleted records match the active filter."
          />
        }
      />
    </AdminConsoleLayout>
  );
}
