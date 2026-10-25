using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StudentAdminPortal.Data;
using StudentAdminPortal.Models;

namespace StudentAdminPortal.Controllers;

// Every action here requires a logged-in administrator.
[Authorize]
public class StudentsController : Controller
{
    private readonly StudentRepository _students;

    public StudentsController(StudentRepository students)
    {
        _students = students;
    }

    // GET: /Students (optional ?search= term searches name or email)
    public async Task<IActionResult> Index(string? search)
    {
        ViewData["CurrentSearch"] = search;
        var students = string.IsNullOrWhiteSpace(search)
            ? await _students.GetAllAsync()
            : await _students.SearchAsync(search.Trim());
        return View(students);
    }

    // GET: /Students/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var student = await _students.GetByIdAsync(id);
        if (student is null)
        {
            return NotFound();
        }
        return View(student);
    }

    // GET: /Students/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Students/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Student student)
    {
        if (!ModelState.IsValid)
        {
            return View(student);
        }
        if (await _students.EmailExistsAsync(student.Email))
        {
            ModelState.AddModelError(nameof(student.Email), "This email is already registered.");
            return View(student);
        }
        try
        {
            await _students.AddAsync(student);
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            // Unique-index violation (race between the check above and the insert).
            ModelState.AddModelError(nameof(student.Email), "This email is already registered.");
            return View(student);
        }
        TempData["SuccessMessage"] = $"Student {student.FullName} was registered successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Students/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var student = await _students.GetByIdAsync(id);
        if (student is null)
        {
            return NotFound();
        }
        return View(student);
    }

    // POST: /Students/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Student student)
    {
        if (id != student.Id)
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            return View(student);
        }
        if (await _students.EmailExistsAsync(student.Email, excludeId: student.Id))
        {
            ModelState.AddModelError(nameof(student.Email), "This email is already registered.");
            return View(student);
        }
        try
        {
            var updated = await _students.UpdateAsync(student);
            if (!updated)
            {
                return NotFound();
            }
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            ModelState.AddModelError(nameof(student.Email), "This email is already registered.");
            return View(student);
        }
        TempData["SuccessMessage"] = $"Student {student.FullName} was updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Students/Delete/5 (confirmation page)
    public async Task<IActionResult> Delete(int id)
    {
        var student = await _students.GetByIdAsync(id);
        if (student is null)
        {
            return NotFound();
        }
        return View(student);
    }

    // POST: /Students/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _students.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }
        TempData["SuccessMessage"] = "Student was deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
