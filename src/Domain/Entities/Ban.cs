using MyApp.Domain.Enums;
using MyApp.Domain.ValueObjects;
using MyApp.Domain.Exceptions;

namespace MyApp.Domain.Entities
{
    public sealed class Ban
    {
        public BanId Id { get; private set; }


        public UserId UserId { get; private set; }

        public UserId AdminId { get; private set; }

        public DateTime EndDate { get; private set; }

        public DateTime StartDate { get; private set; }

        public int Duration { get; private set; }

        public string Reason { get; private set; }

        public BanStatus Status { get; private set; } = BanStatus.Activo;

        private Ban() { Reason = null!; }

        public Ban(User user, User admin, int days, string reason)
        {     
            if (user.Id == admin.Id)
                throw new DomainException("You cannot ban yourself."); //lo que dice el mensaje de error

            // Solo se puede banear a alguien con rol menor al propio.
            // Depende del orden del enum UserRole: User < Admin < SysAdmin.
            if (user.Role >= admin.Role)
                throw new UnauthorizedAccessException("You can only ban users with a lower role than yours.");

            if (days <= 0)
                throw new DomainException("Ban duration must be positive.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("Reason is required.");

            Id = BanId.New();
            UserId = user.Id;
            AdminId = admin.Id;
            Duration = days;
            StartDate = DateTime.Today;
            EndDate = StartDate.AddDays(days);
            Reason = reason.Trim();
        }

        public void ModifyBan(int newDays, string newWhy)
        {
            if (newDays != this.Duration)
            {
                if (!ValidateState())
                    throw new DomainException("Only active bans can be modified.");
                if (newDays <= 0)
                    throw new DomainException("Ban duration must be positive.");
                if (string.IsNullOrWhiteSpace(newWhy))
                    throw new DomainException("Reason is required.");

                DateTime newEndDate = this.StartDate.AddDays(newDays);
                DateTime today = DateTime.Today;

                if (newEndDate <= today)    //si se intenta ingresar una nueva duracion que deje el ban en una fecha anterior a hoy
                {
                    this.EndDate = today;   // setea la fecha de fin a la de hoy
                    this.Duration = (today - this.StartDate).Days;  // deja la duracion del ban en la cantidad de dias transcurridos
                    this.Status = BanStatus.Vencido;    // cambia el estado
                }
                else // en caso de dejar una fecha en el futuro solo cambia la fecha de fin y la duracion a la nueva
                {
                    this.EndDate = newEndDate;
                    this.Duration = newDays;
                }
            }

            this.Reason = newWhy.Trim();
        }

        public void Desban()
        {
            if (!ValidateState())
                throw new DomainException("Only active bans can be revoked.");
                
            this.Status = BanStatus.Revocado;
        }

        public bool ValidateState()
        {
            if (this.Status == BanStatus.Activo && DateTime.Today >= this.EndDate)
            {
                this.Status = BanStatus.Vencido;
            }

            return this.Status == BanStatus.Activo;
        }
    }
}