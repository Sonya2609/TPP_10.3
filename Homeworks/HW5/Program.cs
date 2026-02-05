abstract class Character : IDamageable, IHealable
{
    protected string Name;
    protected int Health;
    public abstract void Attack();
    public void Move()
    {
        Console.WriteLine($"{Name} двигается.");
    }
    public void TakeDamage(int damage)
    {
        Health -= damage;
        Console.WriteLine($"{Name} получил {damage} урона. Осталось {Health} здоровья");
    }
    public void TakeHeal(int heal)
    {
        Health += heal;
        Console.WriteLine($"{Name} получил {heal} восстановления. Осталось {Health} здоровья");
    }
}

class Warrior : Character
{
    public Warrior(string name)
    {
        this.Name = name;
        Health = 100;
    }
    
    public override void Attack()
    {
        Console.WriteLine($"{Name} с мечом идёт в атаку!");
    }
}

class Mage : Character
{
    public Mage(string name)
    {
        this.Name = name;
        Health = 100;
    }
    
    public override void Attack()
    {
        Console.WriteLine($"{Name} начинает заколдовывать врага!");
    }
}


interface IDamageable
{
    void TakeDamage(int damage);
}

interface IHealable
{
    void TakeHeal(int heal);
}

class Program
{
    static void Main(string[] args)
    {
        Character[] characters = new Character[2];
        characters[0] = new Warrior("Вася");
        characters[1] = new Mage("Петя");
        foreach (Character character in characters)
        {
            character.Attack();
        }
    }
}