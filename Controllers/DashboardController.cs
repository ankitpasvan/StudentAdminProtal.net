using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAdminPortal.Data;
using StudentAdminPortal.Models;

namespace StudentAdminPortal.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly StudentRepository _students;

    public DashboardController(StudentRepository students)
    {
        _students = students;
    }

    public async Task<IActionResult> Index()
    {
        var (totalStudents, totalCourses) = await _students.GetStatisticsAsync();
        var model = new DashboardViewModel
        {
            TotalStudents = totalStudents,
            TotalCourses = totalCourses,
            RecentStudents = await _students.GetRecentAsync(5)
        };
        return View(model);
    }
}
