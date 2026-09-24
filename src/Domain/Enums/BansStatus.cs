namespace MyApp.Domain.Enums
{
    public enum BanStatus
    {
        Activo = 0,     // el estado inicial de todos los bans
        Vencido = 1,    // el ban alcanzo su fecha limite
        Revocado = 2   // por si un admin levanta el ban antes de tiempo
    }
}