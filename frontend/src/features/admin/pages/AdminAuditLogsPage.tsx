/**
 * @date 2026-11-22
 * @purpose Filterable, paginated immutable AuditLog viewer with structured field-by-field diff table and Raw JSON snapshot inspection (STRIDE THR-R-01).
 */
import { ClipboardList, Eye, FilterX } from "lucide-react";
import { useState } from "react";
import type { AuditLogItem } from "@/api";
import {
  Badge,
  Button,
  DataTable,
  EmptyState,
  ErrorBanner,
  Input,
  Modal,
  Pagination,
  Select,
  type DataTableColumn,
  type SelectOption,
} from "@/components/ui";
import { cn } from "@/lib/cn";
import { computeJsonDiffEntries, formatDateTime } from "@/lib/formatters";
import { AdminConsoleLayout } from "../components/AdminConsoleLayout";
import { useAuditLogsQuery } from "../hooks/useAdminHooks";

const ENTITY_TYPE_OPTIONS: readonly SelectOption[] = [
  { value: "", label: "All Entity Types" },
  { value: "Student", label: "Student" },
  { value: "University", label: "University" },
  { value: "Faculty", label: "Faculty" },
  { value: "Skill", label: "Skill" },
  { value: "Project", label: "Project" },
  { value: "Organization", label: "Organization" },
  { value: "Event", label: "Event" },
  { value: "Badge", label: "Badge" },
];

function formatJsonValue(raw: string | null): string {
  if (!raw) {
    return "null";
  }
  try {
    return JSON.stringify(JSON.parse(raw), null, 2);
  } catch {
    return raw;
  }
}

export function AdminAuditLogsPage() {
  const [actionFilter, setActionFilter] = useState("");
  const [entityTypeFilter, setEntityTypeFilter] = useState("");
  const [actorIdInput, setActorIdInput] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [selectedLog, setSelectedLog] = useState<AuditLogItem | null>(null);

  const parsedActorId = Number.parseInt(actorIdInput.trim(), 10);
  const auditLogsQuery = useAuditLogsQuery({
    action: actionFilter.trim() || undefined,
    entityType: entityTypeFilter || undefined,
    actorStudentId: Number.isInteger(parsedActorId) && parsedActorId > 0 ? parsedActorId : undefined,
    page,
    pageSize,
  });

  const clearFilters = () => {
    setActionFilter("");
    setEntityTypeFilter("");
    setActorIdInput("");
    setPage(1);
  };

  const columns: DataTableColumn<AuditLogItem>[] = [
    {
      key: "occurredAt",
      header: "Timestamp",
      cell: (row) => (
        <span className="font-mono text-xs text-[#9ca3af] tabular-nums whitespace-nowrap">
          {formatDateTime(row.occurredAt)}
        </span>
      ),
    },
    {
      key: "action",
      header: "Action",
      cell: (row) => <Badge variant="accent">{row.action}</Badge>,
    },
    {
      key: "target",
      header: "Entity",
      cellClassName: "whitespace-nowrap",
      cell: (row) => (
        <span className="text-xs font-medium text-white">
          {row.entityType} <span className="font-mono text-[#9ca3af]">#{row.entityId}</span>
        </span>
      ),
    },
    {
      key: "actor",
      header: "Administrator",
      cell: (row) => (
        <div className="text-xs">
          <p
            title={row.actorEmail ?? "System"}
            className="max-w-[200px] truncate text-white/90"
          >
            {row.actorEmail ?? "System"}
          </p>
        </div>
      ),
    },
    {
      key: "clientIp",
      header: "Client IP",
      cellClassName: "whitespace-nowrap",
      cell: (row) => (
        <span className="font-mono text-xs text-[#9ca3af]">{row.clientIp ?? "—"}</span>
      ),
    },
    {
      key: "diff",
      header: "Details",
      cellClassName: "whitespace-nowrap",
      cell: (row) => (
        <Button
          size="sm"
          variant="secondary"
          leftIcon={<Eye className="size-3.5" aria-hidden="true" />}
          onClick={() => setSelectedLog(row)}
        >
          Inspect Diff
        </Button>
      ),
    },
  ];

  const structuredEntries = selectedLog
    ? computeJsonDiffEntries(selectedLog.oldValues, selectedLog.newValues)
    : [];

  return (
    <AdminConsoleLayout
      title="Activity Log"
      description="Review administrative actions and changes across universities, faculties, and roles."
      actions={
        <Button
          variant="secondary"
          size="sm"
          leftIcon={<FilterX className="size-3.5" aria-hidden="true" />}
          onClick={clearFilters}
        >
          Reset Filters
        </Button>
      }
    >
      <div className="grid gap-3 rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-4 sm:grid-cols-3">
        <Input
          label="Action"
          placeholder="e.g. RoleChanged, EntityRestored"
          value={actionFilter}
          onChange={(e) => {
            setActionFilter(e.target.value);
            setPage(1);
          }}
        />

        <Select
          label="Entity Type"
          value={entityTypeFilter}
          options={ENTITY_TYPE_OPTIONS}
          onChange={(e) => {
            setEntityTypeFilter(e.target.value);
            setPage(1);
          }}
        />

        <Input
          label="Administrator ID"
          type="number"
          min={1}
          placeholder="e.g. 1"
          value={actorIdInput}
          onChange={(e) => {
            setActorIdInput(e.target.value);
            setPage(1);
          }}
        />
      </div>

      {auditLogsQuery.isError && (
        <ErrorBanner
          error={auditLogsQuery.error}
          onRetry={() => {
            void auditLogsQuery.refetch();
          }}
        />
      )}

      <DataTable
        columns={columns}
        data={auditLogsQuery.data?.items ?? []}
        keyExtractor={(row) => row.auditLogId}
        isLoading={auditLogsQuery.isPending}
        emptyState={
          <EmptyState
            icon={<ClipboardList className="size-6" aria-hidden="true" />}
            title="No audit log entries match your filters"
            description="Clear filters or perform a role/recovery action to inspect recorded events."
            action={
              <Button variant="secondary" onClick={clearFilters}>
                Clear Filters
              </Button>
            }
          />
        }
      />

      {auditLogsQuery.data && (
        <Pagination
          page={auditLogsQuery.data.page}
          totalPages={auditLogsQuery.data.totalPages}
          totalCount={auditLogsQuery.data.totalCount}
          pageSize={auditLogsQuery.data.pageSize}
          pageSizeOptions={[10, 20, 50]}
          onPageChange={setPage}
          onPageSizeChange={(nextSize) => {
            setPageSize(nextSize);
            setPage(1);
          }}
        />
      )}

      <Modal
        open={selectedLog !== null}
        onClose={() => setSelectedLog(null)}
        size="lg"
        title={`Audit Entry #${selectedLog?.auditLogId ?? ""} · ${selectedLog?.action ?? ""}`}
        description={
          selectedLog
            ? `${selectedLog.entityType} #${selectedLog.entityId} · ${formatDateTime(selectedLog.occurredAt)} · IP: ${selectedLog.clientIp ?? "N/A"}`
            : undefined
        }
      >
        {selectedLog && (
          <div className="space-y-5">
            {structuredEntries.length > 0 && (
              <div className="overflow-hidden rounded-xl border border-white/10 bg-[#14151c]">
                <div className="border-b border-white/[0.08] px-3.5 py-2 text-xs font-semibold uppercase tracking-wider text-[#9ca3af]">
                  Field-by-Field Comparison
                </div>
                <div className="overflow-x-auto">
                  <table className="w-full border-collapse text-left text-xs">
                    <thead>
                      <tr className="border-b border-white/[0.08] text-[#9ca3af]">
                        <th className="px-3.5 py-2 font-medium">Property</th>
                        <th className="px-3.5 py-2 font-medium">Before</th>
                        <th className="px-3.5 py-2 font-medium">After</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-white/[0.06]">
                      {structuredEntries.map((entry) => (
                        <tr key={entry.field}>
                          <td className="px-3.5 py-2 font-mono font-semibold text-white">
                            {entry.field}
                          </td>
                          <td
                            className={cn(
                              "break-words px-3.5 py-2 font-mono",
                              entry.changed ? "text-red-300" : "text-[#9ca3af]",
                            )}
                          >
                            {entry.before}
                          </td>
                          <td
                            className={cn(
                              "break-words px-3.5 py-2 font-mono",
                              entry.changed ? "text-emerald-300" : "text-[#9ca3af]",
                            )}
                          >
                            {entry.after}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}

            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-1.5">
                <p className="text-xs font-semibold uppercase tracking-wider text-[#9ca3af]">
                  Previous Values
                </p>
                <pre className="max-h-56 overflow-auto rounded-xl border border-white/10 bg-[#14151c] p-3.5 font-mono text-xs text-red-200/90">
                  {formatJsonValue(selectedLog.oldValues)}
                </pre>
              </div>
              <div className="space-y-1.5">
                <p className="text-xs font-semibold uppercase tracking-wider text-[#9ca3af]">
                  Updated Values
                </p>
                <pre className="max-h-56 overflow-auto rounded-xl border border-white/10 bg-[#14151c] p-3.5 font-mono text-xs text-emerald-200/95">
                  {formatJsonValue(selectedLog.newValues)}
                </pre>
              </div>
            </div>
          </div>
        )}
      </Modal>
    </AdminConsoleLayout>
  );
}
