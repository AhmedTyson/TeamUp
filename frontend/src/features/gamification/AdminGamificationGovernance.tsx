/**
 * @date 2026-11-22
 * @purpose PlatformAdmin governance console tabs for Badge Catalog CRUD, Manual Badge Awarding, and Manual Score Adjustment.
 */
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Award, Plus, Shield, Trash2, Wrench } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";
import {
  badgesApi,
  gamificationApi,
  invalidateGamification,
  type BadgeCriteriaType,
  type BadgeDto,
  type ScoreSourceType,
} from "@/api";
import {
  Button,
  ConfirmDialog,
  DataTable,
  Input,
  Modal,
  Select,
  Textarea,
  type DataTableColumn,
} from "@/components/ui";
import { isSafeHttpUrl } from "@/lib/urlValidation";
import { queryKeys } from "@/lib/queryKeys";

export function AdminGamificationGovernance() {
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState<"catalog" | "award" | "adjust">("catalog");

  // Badge Catalog Queries & Mutations
  const catalogQuery = useQuery({
    queryKey: queryKeys.badges.catalog,
    queryFn: ({ signal }) => badgesApi.getBadgeCatalog(signal),
  });

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [editingBadge, setEditingBadge] = useState<BadgeDto | null>(null);
  const [deletingBadge, setDeletingBadge] = useState<BadgeDto | null>(null);

  // Form states for Badge
  const [badgeName, setBadgeName] = useState("");
  const [badgeDesc, setBadgeDesc] = useState("");
  const [badgeIcon, setBadgeIcon] = useState("");
  const [criteriaType, setCriteriaType] = useState<BadgeCriteriaType>("PointsThreshold");
  const [criteriaValue, setCriteriaValue] = useState(100);

  // Manual Award states
  const [awardStudentId, setAwardStudentId] = useState<number>(0);
  const [awardBadgeId, setAwardBadgeId] = useState<number>(0);

  // Manual Score Adjustment states
  const [adjustStudentId, setAdjustStudentId] = useState<number>(0);
  const [adjustPoints, setAdjustPoints] = useState<number>(50);
  const [adjustReason, setAdjustReason] = useState("");
  const [adjustSourceType, setAdjustSourceType] = useState<ScoreSourceType>("Manual");

  const createBadgeMutation = useMutation({
    mutationFn: async () => {
      if (!isSafeHttpUrl(badgeIcon)) {
        throw new Error("Badge icon URL must be a valid safe URL starting with http:// or https://");
      }
      return badgesApi.createBadge({
        name: badgeName.trim(),
        description: badgeDesc.trim(),
        iconUrl: badgeIcon.trim(),
        criteriaType,
        criteriaValue: Number(criteriaValue),
      });
    },
    onSuccess: async () => {
      toast.success("Badge created in catalog.");
      setIsCreateModalOpen(false);
      resetBadgeForm();
      await queryClient.invalidateQueries({ queryKey: queryKeys.badges.all });
    },
    onError: (err: unknown) => {
      const msg = err instanceof Error ? err.message : "Failed to create badge.";
      toast.error(msg);
    },
  });

  const updateBadgeMutation = useMutation({
    mutationFn: async () => {
      if (!editingBadge) return;
      if (badgeIcon && !isSafeHttpUrl(badgeIcon)) {
        throw new Error("Badge icon URL must be a valid safe URL starting with http:// or https://");
      }
      return badgesApi.updateBadge(editingBadge.badgeId, {
        name: badgeName.trim(),
        description: badgeDesc.trim(),
        iconUrl: badgeIcon.trim(),
        criteriaType,
        criteriaValue: Number(criteriaValue),
      });
    },
    onSuccess: async () => {
      toast.success("Badge updated.");
      setEditingBadge(null);
      resetBadgeForm();
      await queryClient.invalidateQueries({ queryKey: queryKeys.badges.all });
    },
    onError: (err: unknown) => {
      const msg = err instanceof Error ? err.message : "Failed to update badge.";
      toast.error(msg);
    },
  });

  const deleteBadgeMutation = useMutation({
    mutationFn: async (id: number) => badgesApi.deleteBadge(id),
    onSuccess: async () => {
      toast.success("Badge deleted from catalog.");
      setDeletingBadge(null);
      await queryClient.invalidateQueries({ queryKey: queryKeys.badges.all });
    },
    onError: (err: unknown) => {
      const msg = err instanceof Error ? err.message : "Failed to delete badge.";
      toast.error(msg);
    },
  });

  const manualAwardMutation = useMutation({
    mutationFn: async () => {
      if (!awardStudentId || !awardBadgeId) {
        throw new Error("Student ID and Badge ID are required.");
      }
      return badgesApi.manualAwardBadge(awardStudentId, awardBadgeId);
    },
    onSuccess: async () => {
      toast.success("Badge successfully awarded to student.");
      await invalidateGamification(queryClient, awardStudentId);
      setAwardStudentId(0);
      setAwardBadgeId(0);
    },
    onError: (err: unknown) => {
      const msg = err instanceof Error ? err.message : "Failed to award badge.";
      toast.error(msg);
    },
  });

  const scoreAdjustmentMutation = useMutation({
    mutationFn: async () => {
      if (!adjustStudentId || !adjustReason.trim()) {
        throw new Error("Student ID and adjustment reason are required.");
      }
      return gamificationApi.createScoreTransaction({
        studentId: adjustStudentId,
        points: Number(adjustPoints),
        reason: adjustReason.trim(),
        sourceType: adjustSourceType,
      });
    },
    onSuccess: async () => {
      toast.success("Score adjustment transaction recorded.");
      await invalidateGamification(queryClient, adjustStudentId);
      setAdjustReason("");
      setAdjustStudentId(0);
      setAdjustPoints(50);
    },
    onError: (err: unknown) => {
      const msg = err instanceof Error ? err.message : "Failed to adjust score.";
      toast.error(msg);
    },
  });

  const resetBadgeForm = () => {
    setBadgeName("");
    setBadgeDesc("");
    setBadgeIcon("");
    setCriteriaType("PointsThreshold");
    setCriteriaValue(100);
  };

  const openEditModal = (badge: BadgeDto) => {
    setEditingBadge(badge);
    setBadgeName(badge.name);
    setBadgeDesc(badge.description);
    setBadgeIcon(badge.iconUrl);
    setCriteriaType(badge.criteriaType);
    setCriteriaValue(badge.criteriaValue);
  };

  const badges = catalogQuery.data ?? [];

  const columns: readonly DataTableColumn<BadgeDto>[] = [
    {
      key: "id",
      header: "ID",
      cell: (b) => <span className="font-mono text-xs text-white/50">#{b.badgeId}</span>,
    },
    {
      key: "name",
      header: "Badge Name",
      cell: (b) => (
        <div>
          <p className="font-bold text-white text-xs">{b.name}</p>
          <p className="text-[11px] text-[#9ca3af] truncate max-w-xs">{b.description}</p>
        </div>
      ),
    },
    {
      key: "criteria",
      header: "Unlock Criteria",
      cell: (b) => (
        <span className="font-mono text-xs text-[#d3fb52]">
          {b.criteriaType}: {b.criteriaValue}
        </span>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      cell: (b) => (
        <div className="flex items-center gap-2">
          <Button variant="outline" size="sm" onClick={() => openEditModal(b)}>
            Edit
          </Button>
          <Button variant="danger" size="sm" onClick={() => setDeletingBadge(b)}>
            <Trash2 className="size-3.5" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <section className="space-y-6 rounded-3xl border border-[#d3fb52]/30 bg-[#1b1d26] p-6 shadow-xl">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-white/[0.08] pb-4">
        <div className="flex items-center gap-2">
          <Shield className="size-5 text-[#d3fb52]" aria-hidden="true" />
          <h2
            className="text-base font-bold text-white uppercase tracking-wider"
            style={{ fontFamily: "var(--font-heading)" }}
          >
            Platform Admin Governance Console
          </h2>
        </div>

        {/* Tab Controls */}
        <div className="flex items-center gap-1 rounded-xl border border-white/10 bg-[#14151c] p-1">
          <button
            type="button"
            onClick={() => setActiveTab("catalog")}
            className={`rounded-lg px-3 py-1.5 text-xs font-semibold transition-colors cursor-pointer ${
              activeTab === "catalog"
                ? "bg-[#d3fb52] text-[#14151c]"
                : "text-white/70 hover:text-white"
            }`}
          >
            Badge Catalog
          </button>
          <button
            type="button"
            onClick={() => setActiveTab("award")}
            className={`rounded-lg px-3 py-1.5 text-xs font-semibold transition-colors cursor-pointer ${
              activeTab === "award"
                ? "bg-[#d3fb52] text-[#14151c]"
                : "text-white/70 hover:text-white"
            }`}
          >
            Manual Award Badge
          </button>
          <button
            type="button"
            onClick={() => setActiveTab("adjust")}
            className={`rounded-lg px-3 py-1.5 text-xs font-semibold transition-colors cursor-pointer ${
              activeTab === "adjust"
                ? "bg-[#d3fb52] text-[#14151c]"
                : "text-white/70 hover:text-white"
            }`}
          >
            Score Adjustment
          </button>
        </div>
      </div>

      {/* TAB 1: Badge Catalog */}
      {activeTab === "catalog" && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <p className="text-xs text-[#9ca3af]">
              Manage the global catalog of unlockable campus achievement badges.
            </p>
            <Button
              variant="primary"
              size="sm"
              onClick={() => {
                resetBadgeForm();
                setIsCreateModalOpen(true);
              }}
              className="gap-1"
            >
              <Plus className="size-3.5" />
              Create Badge
            </Button>
          </div>

          <DataTable
            columns={columns}
            data={badges}
            keyExtractor={(b) => b.badgeId}
            caption="Master badge catalog"
          />
        </div>
      )}

      {/* TAB 2: Manual Award Badge */}
      {activeTab === "award" && (
        <form
          onSubmit={(e) => {
            e.preventDefault();
            manualAwardMutation.mutate();
          }}
          className="max-w-md space-y-4"
        >
          <p className="text-xs text-[#9ca3af]">
            Manually award a verified badge to a student builder by their Student ID.
          </p>

          <Input
            label="Student ID"
            type="number"
            value={awardStudentId || ""}
            onChange={(e) => setAwardStudentId(Number(e.target.value))}
            placeholder="e.g. 1"
            required
          />

          <Select
            label="Select Badge to Award"
            value={awardBadgeId ? String(awardBadgeId) : ""}
            onChange={(e) => setAwardBadgeId(Number(e.target.value))}
            options={[
              { label: "Choose a badge...", value: "" },
              ...badges.map((b) => ({ label: `${b.name} (#${b.badgeId})`, value: String(b.badgeId) })),
            ]}
            required
          />

          <Button
            type="submit"
            variant="primary"
            size="md"
            disabled={!awardStudentId || !awardBadgeId || manualAwardMutation.isPending}
            className="w-full gap-2"
          >
            <Award className="size-4" />
            {manualAwardMutation.isPending ? "Awarding..." : "Award Badge to Student"}
          </Button>
        </form>
      )}

      {/* TAB 3: Score Adjustment */}
      {activeTab === "adjust" && (
        <form
          onSubmit={(e) => {
            e.preventDefault();
            scoreAdjustmentMutation.mutate();
          }}
          className="max-w-md space-y-4"
        >
          <p className="text-xs text-[#9ca3af]">
            Issue a manual score transaction to adjust student points or award special contest credits.
          </p>

          <Input
            label="Student ID"
            type="number"
            value={adjustStudentId || ""}
            onChange={(e) => setAdjustStudentId(Number(e.target.value))}
            placeholder="e.g. 1"
            required
          />

          <Input
            label="Points to Credit / Debit"
            type="number"
            value={adjustPoints}
            onChange={(e) => setAdjustPoints(Number(e.target.value))}
            placeholder="e.g. 100 or -50"
            required
          />

          <Select
            label="Source Type"
            value={adjustSourceType}
            onChange={(e) => setAdjustSourceType(e.target.value as ScoreSourceType)}
            options={[
              { label: "Manual Adjustment", value: "Manual" },
              { label: "Event Attendance", value: "EventAttendance" },
              { label: "Task Completed", value: "TaskCompleted" },
              { label: "Project Delivered", value: "ProjectDelivered" },
            ]}
          />

          <Textarea
            label="Audit Reason"
            value={adjustReason}
            onChange={(e) => setAdjustReason(e.target.value)}
            placeholder="e.g. 1st Place Campus Hackathon Winner"
            required
            rows={2}
          />

          <Button
            type="submit"
            variant="primary"
            size="md"
            disabled={!adjustStudentId || !adjustReason.trim() || scoreAdjustmentMutation.isPending}
            className="w-full gap-2"
          >
            <Wrench className="size-4" />
            {scoreAdjustmentMutation.isPending ? "Recording..." : "Record Score Transaction"}
          </Button>
        </form>
      )}

      {/* Create / Edit Badge Modal */}
      {(isCreateModalOpen || editingBadge) && (
        <Modal
          open={true}
          title={editingBadge ? "Edit Badge" : "Create New Badge"}
          onClose={() => {
            setIsCreateModalOpen(false);
            setEditingBadge(null);
            resetBadgeForm();
          }}
        >
          <form
            onSubmit={(e) => {
              e.preventDefault();
              if (editingBadge) {
                updateBadgeMutation.mutate();
              } else {
                createBadgeMutation.mutate();
              }
            }}
            className="space-y-4 pt-2"
          >
            <Input
              label="Badge Name"
              value={badgeName}
              onChange={(e) => setBadgeName(e.target.value)}
              placeholder="e.g. Squad Pioneer"
              required
            />

            <Textarea
              label="Description"
              value={badgeDesc}
              onChange={(e) => setBadgeDesc(e.target.value)}
              placeholder="Describe the milestone needed to unlock..."
              required
              rows={3}
            />

            <Input
              label="Icon URL (https://...)"
              value={badgeIcon}
              onChange={(e) => setBadgeIcon(e.target.value)}
              placeholder="https://example.com/badge.svg"
              required
            />

            <div className="grid grid-cols-2 gap-3">
              <Select
                label="Criteria Type"
                value={criteriaType}
                onChange={(e) => setCriteriaType(e.target.value as BadgeCriteriaType)}
                options={[
                  { label: "Points Threshold", value: "PointsThreshold" },
                  { label: "Event Count", value: "EventCount" },
                  { label: "Task Count", value: "TaskCount" },
                ]}
              />

              <Input
                label="Criteria Threshold"
                type="number"
                value={criteriaValue}
                onChange={(e) => setCriteriaValue(Number(e.target.value))}
                required
              />
            </div>

            <div className="flex justify-end gap-2 pt-2">
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => {
                  setIsCreateModalOpen(false);
                  setEditingBadge(null);
                }}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                variant="primary"
                size="sm"
                disabled={createBadgeMutation.isPending || updateBadgeMutation.isPending}
              >
                {editingBadge ? "Save Changes" : "Create Badge"}
              </Button>
            </div>
          </form>
        </Modal>
      )}

      {/* Delete Confirmation */}
      <ConfirmDialog
        open={Boolean(deletingBadge)}
        title="Delete Badge from Catalog"
        description={`Are you sure you want to delete badge "${deletingBadge?.name}"? This permanently removes the badge criteria.`}
        confirmLabel="Yes, Delete Badge"
        variant="danger"
        isLoading={deleteBadgeMutation.isPending}
        onConfirm={() => {
          if (deletingBadge) deleteBadgeMutation.mutate(deletingBadge.badgeId);
        }}
        onClose={() => setDeletingBadge(null)}
      />
    </section>
  );
}
