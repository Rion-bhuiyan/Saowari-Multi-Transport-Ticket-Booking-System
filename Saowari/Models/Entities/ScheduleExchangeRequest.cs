using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    /// <summary>
    /// A Driver or Supervisor requests to swap their schedule with another Driver/Supervisor.
    /// Workflow: Requester creates → Peer accepts/rejects → Manager approves/rejects → Schedules swapped.
    /// </summary>
    [Table("ScheduleExchangeRequest")]
    public class ScheduleExchangeRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Requester))]
        public int RequesterId { get; set; }

        [Required]
        [ForeignKey(nameof(RequesterSchedule))]
        public int RequesterScheduleId { get; set; }

        [Required]
        [ForeignKey(nameof(TargetUser))]
        public int TargetUserId { get; set; }

        [Required]
        [ForeignKey(nameof(TargetSchedule))]
        public int TargetScheduleId { get; set; }

        /// <summary>
        /// Overall status: Pending → AcceptedByPeer / RejectedByPeer → Approved / Rejected (by Manager)
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [MaxLength(500)]
        public string? ManagerRemarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PeerRespondedAt { get; set; }

        public DateTime? ManagerRespondedAt { get; set; }

        public virtual User Requester { get; set; } = null!;
        public virtual Schedule RequesterSchedule { get; set; } = null!;
        public virtual User TargetUser { get; set; } = null!;
        public virtual Schedule TargetSchedule { get; set; } = null!;
    }
}
