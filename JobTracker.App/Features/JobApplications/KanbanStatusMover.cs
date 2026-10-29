using JobTracker.App.Domain;

namespace JobTracker.App.Features.JobApplications
{
    /// <summary>
    /// Cambio de estado de una postulación desde el tablero (BRIEF-001).
    /// Usa la misma operación que el formulario —<c>UpdateJobApplicationAsync</c>—,
    /// así que la persistencia es idéntica por los dos caminos y se puede probar
    /// sin UI: misma columna no escribe, y un destino fuera del enum se rechaza.
    /// </summary>
    public class KanbanStatusMover
    {
        private readonly IJobTrackerService _service;

        public KanbanStatusMover(IJobTrackerService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Mueve <paramref name="application"/> a <paramref name="target"/>.
        /// Devuelve <c>true</c> si escribió; <c>false</c> si ya estaba en ese estado.
        /// Lanza <see cref="ArgumentException"/> si el destino no está definido.
        /// </summary>
        public async Task<bool> MoveAsync(JobApplication application, ApplicationStatus target)
        {
            ArgumentNullException.ThrowIfNull(application);

            if (!Enum.IsDefined(target))
                throw new ArgumentException(
                    $"Estado no definido: {(int)target}.", nameof(target));

            if (application.Status == target)
                return false;

            application.Status = target;
            await _service.UpdateJobApplicationAsync(application);
            return true;
        }
    }
}
