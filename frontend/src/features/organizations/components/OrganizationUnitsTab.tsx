/**
 * @date 2026-11-24
 * @purpose Chapters, Operational Committees, and Affinity Groups showcase & management tab inside OrganizationDetailPage.
 */
import { Cpu, Layers, Pencil, Plus, Trash2, Users } from "lucide-react";
import { useMemo, useState } from "react";
import { toast } from "sonner";
import {
  ORGANIZATION_UNIT_TYPES,
  type OrganizationUnit,
  type OrganizationUnitPayload,
} from "@/api";
import { Badge, Button, Card, ConfirmDialog, EmptyState } from "@/components/ui";
import { OrganizationUnitFormModal } from "./OrganizationUnitFormModal";

export interface OrganizationUnitsTabProps {
  units: readonly OrganizationUnit[];
  canManage: boolean;
  isBusy: boolean;
  onCreateUnit: (payload: OrganizationUnitPayload) => Promise<unknown>;
  onUpdateUnit: (unitId: number, payload: OrganizationUnitPayload) => Promise<unknown>;
  onDeleteUnit: (unitId: number) => Promise<unknown>;
  onApplyToUnit?: (unitId: number) => void;
}

const SECTION_META = [
  {
    key: "technical",
    title: "Technical Chapters & Affinity Groups",
    subtitle: "Domain tracks, societies, and hands-on engineering labs",
    types: new Set<string>([
      ORGANIZATION_UNIT_TYPES.TechnicalChapter,
      ORGANIZATION_UNIT_TYPES.AffinityGroup,
      ORGANIZATION_UNIT_TYPES.WorkingGroup,
    ]),
  },
  {
    key: "operational",
    title: "Operational Committees",
    subtitle: "Cross-cutting HR, PR & Fundraising, Marketing, and Logistics teams",
    types: new Set<string>([ORGANIZATION_UNIT_TYPES.OperationalCommittee]),
  },
];

export function OrganizationUnitsTab({
  units,
  canManage,
  isBusy,
  onCreateUnit,
  onUpdateUnit,
  onDeleteUnit,
  onApplyToUnit,
}: OrganizationUnitsTabProps) {
  const [modalOpen, setModalOpen] = useState(false);
  const [editingUnit, setEditingUnit] = useState<OrganizationUnit | undefined>(undefined);
  const [deleteTarget, setDeleteTarget] = useState<OrganizationUnit | null>(null);

  const grouped = useMemo(
    () =>
      SECTION_META.map((sec) => ({
        ...sec,
        items: units.filter((u) => sec.types.has(u.unitType)),
      })).filter((sec) => sec.items.length > 0),
    [units],
  );

  return (
    <div className="space-y-6 pt-2">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-base font-semibold text-white">Chapters &amp; Operational Structure</h2>
          <p className="text-xs text-white/60">
            Explore specialized technical societies and operational committees inside this activity.
          </p>
        </div>
        {canManage && (
          <Button
            size="sm"
            leftIcon={<Plus className="size-3.5" />}
            onClick={() => {
              setEditingUnit(undefined);
              setModalOpen(true);
            }}
          >
            Add chapter / committee
          </Button>
        )}
      </div>

      {units.length === 0 ? (
        <EmptyState
          icon={<Layers className="size-6" />}
          title="No chapters or committees configured yet"
          description="Branch admins can organize technical chapters (CS, RAS, PES) and committees (HR, PR, OC) here."
        />
      ) : (
        <div className="space-y-6">
          {grouped.map((section) => (
            <section key={section.key} className="space-y-3">
              <div className="flex flex-wrap items-baseline justify-between gap-2 border-b border-white/10 pb-2">
                <div>
                  <h3 className="text-sm font-bold uppercase tracking-wider text-[#d3fb52]">
                    {section.title}
                  </h3>
                  <p className="text-xs text-white/55">{section.subtitle}</p>
                </div>
                <span className="font-mono text-xs text-white/50">{section.items.length} units</span>
              </div>

              <div className="grid gap-3.5 sm:grid-cols-2 xl:grid-cols-3">
                {section.items.map((unit) => (
                  <Card
                    key={unit.organizationUnitId}
                    className="flex flex-col justify-between gap-3 bg-[#14151c] p-4 transition-colors hover:border-[#d3fb52]/30"
                  >
                    <div className="space-y-2">
                      <div className="flex items-start justify-between gap-2">
                        <div className="inline-flex items-center gap-2">
                          <span className="inline-flex size-8 items-center justify-center rounded-lg border border-[#d3fb52]/30 bg-[#d3fb52]/10 font-mono text-xs font-bold text-[#d3fb52]">
                            {unit.code}
                          </span>
                          <div className="min-w-0">
                            <p className="truncate font-semibold text-white">{unit.name}</p>
                            <p className="text-[11px] text-white/50">{unit.unitType}</p>
                          </div>
                        </div>
                        {unit.isRecruiting && (
                          <Badge variant="success" size="sm">
                            Recruiting
                          </Badge>
                        )}
                      </div>

                      {unit.description && (
                        <p className="line-clamp-3 text-xs leading-relaxed text-white/70">
                          {unit.description}
                        </p>
                      )}
                    </div>

                    <div className="space-y-3 border-t border-white/10 pt-3">
                      <div className="flex items-center justify-between text-xs text-white/60">
                        <span className="inline-flex items-center gap-1.5">
                          <Users className="size-3.5" aria-hidden="true" />
                          {unit.memberCount} crew
                        </span>
                        {unit.openSeats !== null && (
                          <span className="inline-flex items-center gap-1 font-mono text-[#d3fb52]">
                            <Cpu className="size-3" aria-hidden="true" />
                            {unit.openSeats} seats
                          </span>
                        )}
                      </div>

                      <div className="flex flex-wrap items-center justify-between gap-2">
                        {onApplyToUnit && unit.isRecruiting && (
                          <Button
                            size="sm"
                            variant="outline"
                            onClick={() => onApplyToUnit(unit.organizationUnitId)}
                          >
                            Apply to {unit.code}
                          </Button>
                        )}
                        {canManage && (
                          <div className="ms-auto flex items-center gap-1.5">
                            <Button
                              size="sm"
                              variant="ghost"
                              leftIcon={<Pencil className="size-3" />}
                              disabled={isBusy}
                              onClick={() => {
                                setEditingUnit(unit);
                                setModalOpen(true);
                              }}
                            >
                              Edit
                            </Button>
                            <Button
                              size="sm"
                              variant="danger"
                              leftIcon={<Trash2 className="size-3" />}
                              disabled={isBusy}
                              onClick={() => setDeleteTarget(unit)}
                            >
                              Delete
                            </Button>
                          </div>
                        )}
                      </div>
                    </div>
                  </Card>
                ))}
              </div>
            </section>
          ))}
        </div>
      )}

      <OrganizationUnitFormModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        initial={editingUnit}
        isSubmitting={isBusy}
        onSubmitPayload={async (payload) => {
          if (editingUnit) {
            await onUpdateUnit(editingUnit.organizationUnitId, payload);
            toast.success(`${payload.code} updated.`);
          } else {
            await onCreateUnit(payload);
            toast.success(`${payload.code} created.`);
          }
          setModalOpen(false);
        }}
      />

      <ConfirmDialog
        open={deleteTarget !== null}
        onClose={() => setDeleteTarget(null)}
        isLoading={isBusy}
        title="Delete chapter / committee?"
        description={
          deleteTarget
            ? `Removing "${deleteTarget.name}" unassigns its members back to organization-wide crew.`
            : ""
        }
        confirmLabel="Delete unit"
        onConfirm={async () => {
          if (!deleteTarget) {
            return;
          }
          try {
            await onDeleteUnit(deleteTarget.organizationUnitId);
            toast.success(`${deleteTarget.code} deleted.`);
            setDeleteTarget(null);
          } catch (error) {
            toast.error(error instanceof Error ? error.message : "Could not delete unit.");
          }
        }}
      />
    </div>
  );
}
