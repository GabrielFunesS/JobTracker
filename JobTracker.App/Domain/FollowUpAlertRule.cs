namespace JobTracker.App.Domain
{
    /// <summary>
    /// Regla pura de detección de follow-up (US-001): decide si una postulación
    /// debe generar una alerta. Sin I/O, sin reloj: «hoy» siempre es un parámetro (D-2).
    /// </summary>
    public static class FollowUpAlertRule
    {
        /// <summary>
        /// Umbral de días hábiles transcurridos desde la fecha de referencia
        /// a partir del cual se alerta (R2: 6 o más; 5 transcurridos aún no).
        /// </summary>
        private const int BusinessDayThreshold = 6;

        /// <summary>
        /// Devuelve true si la postulación debe generar una alerta:
        /// R1 — solo estado «Enviado»;
        /// R4 — fecha de referencia = LastFollowUpDate si existe, si no ApplicationDate;
        /// R3 — se cuentan solo días de lunes a viernes, posteriores a la
        ///      referencia e incluyendo «hoy»;
        /// R2 — alerta con 6 o más días hábiles;
        /// R5 — el reinicio (nueva fecha de seguimiento o salida de «Enviado»)
        ///      se deriva de R4 y R1, sin estado adicional (D-4).
        /// </summary>
        public static bool ShouldAlert(JobApplication application, DateTime today)
        {
            if (application.Status != ApplicationStatus.Enviado)
                return false;

            var referenceDate = application.LastFollowUpDate ?? application.ApplicationDate;

            return CountBusinessDaysAfter(referenceDate, today) >= BusinessDayThreshold;
        }

        /// <summary>
        /// Cuenta los días hábiles (L-V) estrictamente posteriores a <paramref name="from"/>
        /// hasta e incluyendo <paramref name="to"/> (convenio D-1). Si <paramref name="to"/>
        /// no es posterior, el conteo es 0.
        /// </summary>
        private static int CountBusinessDaysAfter(DateTime from, DateTime to)
        {
            var count = 0;
            for (var date = from.Date.AddDays(1); date <= to.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
                    count++;
            }

            return count;
        }
    }
}
