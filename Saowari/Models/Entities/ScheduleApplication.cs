using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    /// <summary>
    /// A Driver or Supervisor applies to the Company Manager for a new schedule.
    /// </summary>
    [Table("ScheduleApplication")]
    public class ScheduleApplication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>The Driver or Supervisor user requesting the schedule.</summary>
        [Required]
        [ForeignKey(nameof(Requester))]
        public int RequesterId { get; set; }

        /// <summary>The company this application belongs to.</summary>
        [Required]
        [ForeignKey(nameof(Company))]
        public int CompanyId { get; set; }

        [Required]
        [ForeignKey(nameof(Route))]
        public int RouteId { get; set; }

        [Required]
        [ForeignKey(nameof(Vehicle))]
        public int VehicleId { get; set; }

        [Required]
        public DateTime DepartureDateTime { get; set; }

        [Required]
        public DateTime ArrivalDateTime { get; set; }

        /// <summary>Pending / Approved / Rejected</summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [MaxLength(500)]
        public string? ManagerRemarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }

        /// <summary>The Schedule created when application is approved.</summary>
        [ForeignKey(nameof(CreatedSchedule))]
        public int? CreatedScheduleId { get; set; }

        public virtual User Requester { get; set; } = null!;
        public virtual Company Company { get; set; } = null!;
        public virtual Route Route { get; set; } = null!;
        public virtual Vehicle Vehicle { get; set; } = null!;
        public virtual Schedule? CreatedSchedule { get; set; }
    }
}
