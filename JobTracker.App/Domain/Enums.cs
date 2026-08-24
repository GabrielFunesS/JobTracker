namespace JobTracker.App.Domain
{
    public enum ApplicationStatus
    {
        Enviado,
        Entrevistas,
        Oferta,
        Rechazado,
        Caducado
    }

    public enum RejectionReason
    {
        Ghosting,
        FiltroRRHH,
        PruebaTecnica,
        EntrevistaCliente,
        OfertaInsuficiente,
        PosicionCancelada
    }
}
