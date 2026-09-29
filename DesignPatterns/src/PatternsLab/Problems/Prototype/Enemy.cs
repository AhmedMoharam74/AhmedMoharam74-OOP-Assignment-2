namespace PatternsLab.Problems.Prototype;

public sealed class Weapon
{
    public string Name { get; set; } = "";
    public int Damage { get; set; }
    public Weapon DeepCopy() => new() { Name = Name, Damage = Damage };
}

public abstract class Enemy
{
    private readonly string _modelData;
    public string Name { get; set; } = "";
    public int Health { get; set; }
    public Weapon Weapon { get; set; } = new();
    public List<string> Abilities { get; set; } = new();
    public string ModelId => _modelData;

    protected Enemy()
    {
        Console.WriteLine("   ...loading 3D model (slow)...");
        Thread.Sleep(500);
        _modelData = "MODEL_" + Guid.NewGuid().ToString("N")[..6];
    }

    protected Enemy(Enemy source)
    {
        _modelData = source._modelData;
        Name = source.Name;
        Health = source.Health;
        Weapon = source.Weapon.DeepCopy();
        Abilities = new List<string>(source.Abilities);
    }

    public abstract Enemy Clone();
}

public sealed class Orc : Enemy
{
    public Orc() { Name = "Orc"; Health = 100; Weapon = new Weapon { Name = "Axe", Damage = 25 }; Abilities.Add("Rage"); }
    private Orc(Orc source) : base(source) { }
    public override Enemy Clone() => new Orc(this);
}

public sealed class Elf : Enemy
{
    public Elf() { Name = "Elf"; Health = 70; Weapon = new Weapon { Name = "Bow", Damage = 18 }; Abilities.Add("Stealth"); }
    private Elf(Elf source) : base(source) { }
    public override Enemy Clone() => new Elf(this);
}

public sealed class EnemyPrototypeRegistry
{
    private readonly Dictionary<string, Enemy> _prototypes = new(StringComparer.OrdinalIgnoreCase);
    public void Register(string name, Enemy prototype) => _prototypes[name] = prototype;
    public Enemy Create(string name) => _prototypes.TryGetValue(name, out var prototype)
        ? prototype.Clone()
        : throw new KeyNotFoundException($"No enemy prototype named '{name}'.");
}

// Kept as a small compatibility entry point; cloning is polymorphic, with no type switch.
public static class EnemyCopyHelper
{
    public static Enemy CopyEnemy(Enemy enemy) => enemy.Clone();
}
