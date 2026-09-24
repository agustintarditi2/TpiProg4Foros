using System.ComponentModel.DataAnnotations;
using MyApp.Domain.Enums;

namespace MyApp.Domain.Entities
{
    public class Ban
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int AdminId { get; set; }

        public DateTime EndDate { get; set; }
        
        public DateTime StartDate { get; set; }

        [Required]
        public int Duration { get; set; }

        [Required]
        public string Reason { get; set; }

        public BanStatus Status { get; set; } = BanStatus.Activo;


        public Ban(int User, int Admin, int Days, string why)
        {
            UserId = User;
            AdminId = Admin;
            Duration = Days;
            StartDate = DateTime.Today;
            EndDate = StartDate.AddDays(Days);
            Reason = why;
        }

        public void ModifyBan(int newDays, string newWhy)
        {
            if (newDays != this.Duration)
            {
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

            this.Reason = newWhy;
        }

        public void Desban()
        {
            this.Status = BanStatus.Revocado;
        }

        public bool ValidatState()
        {
            if (this.Status == BanStatus.Activo && DateTime.Today >= this.EndDate)
            {
                this.Status = BanStatus.Vencido;
            }

            return this.Status == BanStatus.Activo;
        }
    }
}