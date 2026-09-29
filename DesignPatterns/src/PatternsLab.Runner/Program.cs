using System.Diagnostics;
using PatternsLab.Problems.Builder;
using PatternsLab.Problems.Prototype;
using PatternsLab.Problems.Singleton;

Console.WriteLine("=== SINGLETON ===\n");
var db = new DatabaseService();
var ui = new UiService();
db.Config.Theme = "Dark";
Console.WriteLine("Admin changed theme to Dark.");
ui.Render();
Console.WriteLine($"Same config object? {ReferenceEquals(db.Config, ui.Config)}");
Console.WriteLine($"Times config was loaded from disk: {AppConfig.LoadCount}");

Console.WriteLine("\n=== PROTOTYPE ===\n");
var sw = Stopwatch.StartNew();
EnemyPrototypeRegistry registry = new();
registry.Register("Orc", new Orc());
registry.Register("Elf", new Elf());
List<Enemy> army = new();
for (int i = 1; i <= 5; i++)
{
    Enemy orc = registry.Create("Orc");
    orc.Name = $"Orc-{i}";
    army.Add(orc);
}
Console.WriteLine($"Cloned 5 orcs in {sw.ElapsedMilliseconds} ms");
Enemy original = registry.Create("Orc");
original.Name = "Boss Orc";
Enemy copy = original.Clone();
Console.WriteLine($"Original model id: {original.ModelId}");
Console.WriteLine($"Copy model id:     {copy.ModelId}");
copy.Weapon.Damage = 999;
copy.Abilities.Add("Fire Breath");
Console.WriteLine($"Original weapon damage stays {original.Weapon.Damage}; abilities: {string.Join(", ", original.Abilities)}");

Console.WriteLine("\n=== BUILDER ===\n");
Console.WriteLine(RegistrationCallSites.CreateLiveStudentUgly());
Console.WriteLine(RegistrationCallSites.CreateVideosOnlyUgly());
try
{
    new CourseRegistrationBuilder().ForStudent("test@mail.com").ForCourse("SEF-101").WithAccessMode("LiveGroup").Build();
}
catch (InvalidOperationException error)
{
    Console.WriteLine($"Invalid registration rejected: {error.Message}");
}
