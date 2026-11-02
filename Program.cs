using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using StudentManagementSystem.Utils;

var dataFile = Path.Combine(Directory.GetCurrentDirectory(), "data", "sms-data.json");
var service = new SchoolService(new DataStore(dataFile));

while (true)
{
    Console.WriteLine();
    Console.WriteLine("==============================");
    Console.WriteLine("  STUDENT MANAGEMENT SYSTEM");
    Console.WriteLine("==============================");
    Console.WriteLine(" 1. Add student");
    Console.WriteLine(" 2. View all students");
    Console.WriteLine(" 3. Search students");
    Console.WriteLine(" 4. Update student");
    Console.WriteLine(" 5. Delete student");
    Console.WriteLine(" 6. Add module");
    Console.WriteLine(" 7. View modules");
    Console.WriteLine(" 8. Record a mark");
    Console.WriteLine(" 9. Student report");
    Console.WriteLine("10. Module summary report");
    Console.WriteLine(" 0. Exit");

    var choice = ConsoleHelper.ReadInt("Choose an option: ", 0, 10);
    if (choice == 0)
    {
        Console.WriteLine("Goodbye!");
        break;
    }

    switch (choice)
    {
        case 1: AddStudent(); break;
        case 2: ViewStudents(); break;
        case 3: SearchStudents(); break;
        case 4: UpdateStudent(); break;
        case 5: DeleteStudent(); break;
        case 6: AddModule(); break;
        case 7: ViewModules(); break;
        case 8: RecordMark(); break;
        case 9: StudentReport(); break;
        case 10: ModuleSummary(); break;
    }

    ConsoleHelper.Pause();
}

void AddStudent()
{
    ConsoleHelper.Header("Add student");
    var student = new Student
    {
        StudentNumber = ConsoleHelper.ReadRequired("Student number: "),
        FullName = ConsoleHelper.ReadRequired("Full name: "),
        Email = ConsoleHelper.ReadEmail("Email: "),
        Course = ConsoleHelper.ReadRequired("Course: ")
    };

    Console.WriteLine(service.AddStudent(student)
        ? "Student added."
        : "A student with that number already exists.");
}

void ViewStudents()
{
    ConsoleHelper.Header("All students");
    PrintStudents(service.Students);
}

void SearchStudents()
{
    ConsoleHelper.Header("Search students");
    var term = ConsoleHelper.ReadRequired("Enter a name or student number: ");
    PrintStudents(service.SearchStudents(term).ToList());
}

void UpdateStudent()
{
    ConsoleHelper.Header("Update student");
    var student = PromptForStudent();
    if (student is null) return;

    Console.WriteLine("Enter the new details.");
    service.UpdateStudent(
        student,
        ConsoleHelper.ReadRequired("Full name: "),
        ConsoleHelper.ReadEmail("Email: "),
        ConsoleHelper.ReadRequired("Course: "));
    Console.WriteLine("Student updated.");
}

void DeleteStudent()
{
    ConsoleHelper.Header("Delete student");
    var student = PromptForStudent();
    if (student is null) return;

    if (ConsoleHelper.Confirm($"Delete {student.FullName} and all their marks?"))
    {
        service.DeleteStudent(student);
        Console.WriteLine("Student deleted.");
    }
    else
    {
        Console.WriteLine("Cancelled.");
    }
}

void AddModule()
{
    ConsoleHelper.Header("Add module");
    var module = new Module
    {
        Code = ConsoleHelper.ReadRequired("Module code: ").ToUpperInvariant(),
        Name = ConsoleHelper.ReadRequired("Module name: "),
        Credits = ConsoleHelper.ReadInt("Credits (1-60): ", 1, 60)
    };

    Console.WriteLine(service.AddModule(module)
        ? "Module added."
        : "A module with that code already exists.");
}

void ViewModules()
{
    ConsoleHelper.Header("All modules");
    if (service.Modules.Count == 0)
    {
        Console.WriteLine("No modules found.");
        return;
    }

    Console.WriteLine($"{"Code",-10}{"Name",-35}{"Credits",7}");
    foreach (var m in service.Modules)
        Console.WriteLine($"{m.Code,-10}{m.Name,-35}{m.Credits,7}");
}

void RecordMark()
{
    ConsoleHelper.Header("Record a mark");
    var student = PromptForStudent();
    if (student is null) return;

    if (service.Modules.Count == 0)
    {
        Console.WriteLine("No modules exist yet. Add a module first.");
        return;
    }

    var module = service.FindModule(ConsoleHelper.ReadRequired("Module code: "));
    if (module is null)
    {
        Console.WriteLine("Module not found.");
        return;
    }

    var score = ConsoleHelper.ReadInt("Mark (0-100): ", 0, 100);
    service.RecordMark(student, module, score);
    Console.WriteLine($"Recorded {score}% for {student.FullName} in {module.Name}.");
}

void StudentReport()
{
    ConsoleHelper.Header("Student report");
    var student = PromptForStudent();
    if (student is null) return;

    Console.WriteLine();
    Console.WriteLine($"Name:   {student.FullName} ({student.StudentNumber})");
    Console.WriteLine($"Course: {student.Course}");
    Console.WriteLine($"Email:  {student.Email}");
    Console.WriteLine();

    var marks = service.MarksFor(student);
    if (marks.Count == 0)
    {
        Console.WriteLine("No marks recorded yet.");
        return;
    }

    Console.WriteLine($"{"Module",-35}{"Mark",6}  Result");
    foreach (var mark in marks)
    {
        var name = service.FindModule(mark.ModuleCode)?.Name ?? mark.ModuleCode;
        var result = mark.Score >= SchoolService.PassMark ? "Pass" : "Fail";
        Console.WriteLine($"{name,-35}{mark.Score,5}%  {result}");
    }

    Console.WriteLine();
    Console.WriteLine($"Average: {service.AverageFor(student):F1}%");
}

void ModuleSummary()
{
    ConsoleHelper.Header("Module summary");
    var summaries = service.ModuleSummaries().ToList();
    if (summaries.Count == 0)
    {
        Console.WriteLine("No marks recorded yet.");
        return;
    }

    Console.WriteLine($"{"Module",-30}{"Students",9}{"Avg",8}{"High",6}{"Low",6}{"Pass rate",11}");
    foreach (var s in summaries)
        Console.WriteLine($"{s.Module.Name,-30}{s.Count,9}{s.Average,7:F1}%{s.Highest,6}{s.Lowest,6}{s.PassRate,10:F0}%");
}

Student? PromptForStudent()
{
    if (service.Students.Count == 0)
    {
        Console.WriteLine("No students found. Add a student first.");
        return null;
    }

    var student = service.FindStudent(ConsoleHelper.ReadRequired("Student number: "));
    if (student is null)
        Console.WriteLine("Student not found.");
    return student;
}

void PrintStudents(IReadOnlyList<Student> students)
{
    if (students.Count == 0)
    {
        Console.WriteLine("No students found.");
        return;
    }

    Console.WriteLine($"{"Number",-12}{"Name",-25}{"Course",-25}Email");
    foreach (var s in students)
        Console.WriteLine($"{s.StudentNumber,-12}{s.FullName,-25}{s.Course,-25}{s.Email}");
}
