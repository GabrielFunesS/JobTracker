namespace JobTracker.App.Domain
{
    public class JobApplication
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string? JobUrl { get; set; }
        public DateTime ApplicationDate { get; set; }

        // Relación con la tabla de Orígenes
        public int OriginId { get; set; }
        public JobOrigin? Origin { get; set; }

        // Columnas del Kanban
        public ApplicationStatus Status { get; set; }

        // Novedades para las estadísticas de fracaso/éxito
        public string? CurrentStage { get; set; }
        public RejectionReason? RejectionReason { get; set; }

        // Datos opcionales
        public decimal? ExpectedSalary { get; set; }
        public string? Notes { get; set; }
        public DateTime? LastFollowUpDate { get; set; }
    }
}
