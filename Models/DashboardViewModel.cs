namespace StudentAdminPortal.Models;

public class DashboardViewModel
{
    public int TotalStudents { get; set; }

    public int TotalCourses { get; set; }

    public List<Student> RecentStudents { get; set; } = new();
}
