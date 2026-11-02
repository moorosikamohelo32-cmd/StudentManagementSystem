using StudentManagementSystem.Models;

namespace StudentManagementSystem.Services;

/// <summary>Business logic for students, modules and marks.</summary>
public class SchoolService
{
    public const int PassMark = 50;

    private readonly AppData _data;
    private readonly DataStore _store;

    public SchoolService(DataStore store)
    {
        _store = store;
        _data = store.Load();
    }

    public IReadOnlyList<Student> Students => _data.Students.OrderBy(s => s.StudentNumber).ToList();
    public IReadOnlyList<Module> Modules => _data.Modules.OrderBy(m => m.Code).ToList();

    // ---------- Students ----------

    public Student? FindStudent(string studentNumber) =>
        _data.Students.FirstOrDefault(s => s.StudentNumber.Equals(studentNumber, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<Student> SearchStudents(string term) =>
        _data.Students.Where(s =>
            s.StudentNumber.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            s.FullName.Contains(term, StringComparison.OrdinalIgnoreCase));

    public bool AddStudent(Student student)
    {
        if (FindStudent(student.StudentNumber) is not null)
            return false;

        _data.Students.Add(student);
        _store.Save(_data);
        return true;
    }

    public void UpdateStudent(Student student, string fullName, string email, string course)
    {
        student.FullName = fullName;
        student.Email = email;
        student.Course = course;
        _store.Save(_data);
    }

    public void DeleteStudent(Student student)
    {
        _data.Students.Remove(student);
        _data.Marks.RemoveAll(m => m.StudentNumber == student.StudentNumber);
        _store.Save(_data);
    }

    // ---------- Modules ----------

    public Module? FindModule(string code) =>
        _data.Modules.FirstOrDefault(m => m.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    public bool AddModule(Module module)
    {
        if (FindModule(module.Code) is not null)
            return false;

        _data.Modules.Add(module);
        _store.Save(_data);
        return true;
    }

    // ---------- Marks ----------

    /// <summary>Adds a mark, or replaces the existing one for that student and module.</summary>
    public void RecordMark(Student student, Module module, int score)
    {
        var existing = _data.Marks.FirstOrDefault(m =>
            m.StudentNumber == student.StudentNumber && m.ModuleCode == module.Code);

        if (existing is null)
            _data.Marks.Add(new Mark { StudentNumber = student.StudentNumber, ModuleCode = module.Code, Score = score });
        else
            existing.Score = score;

        _store.Save(_data);
    }

    public IReadOnlyList<Mark> MarksFor(Student student) =>
        _data.Marks.Where(m => m.StudentNumber == student.StudentNumber).OrderBy(m => m.ModuleCode).ToList();

    public double? AverageFor(Student student)
    {
        var marks = MarksFor(student);
        return marks.Count == 0 ? null : marks.Average(m => m.Score);
    }

    public IEnumerable<(Module Module, int Count, double Average, int Highest, int Lowest, double PassRate)> ModuleSummaries()
    {
        foreach (var module in Modules)
        {
            var scores = _data.Marks.Where(m => m.ModuleCode == module.Code).Select(m => m.Score).ToList();
            if (scores.Count == 0)
                continue;

            var passRate = scores.Count(s => s >= PassMark) * 100.0 / scores.Count;
            yield return (module, scores.Count, scores.Average(), scores.Max(), scores.Min(), passRate);
        }
    }
}
