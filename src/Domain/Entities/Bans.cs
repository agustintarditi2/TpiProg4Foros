public class Ban
{
    public int Id {get; set; }
    
    public int UserId {get; set; }

    public int AdminId {get; set; }

    public DateTime EndDate {get; set; }

    public int Duration {get; set; }

    public string Reason {get; set; }

    public string Status {get; set; }
    private static int GenId = 0;


    public Ban (int User, int Admin, int Days, string why)
    {
        Id = GenId;
        GenId++;
        UserId = User;
        AdminId = Admin;
        Duration = Days;
        DateTime today = DateTime.Today;
        EndDate = today.AddDays(Days);
        Reason = why;
        Status = "Activo";
    }
}