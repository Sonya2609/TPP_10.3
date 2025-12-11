class Student
{
    protected string name;
    protected int age;
    protected double group;
    public Student()
    {
        name = "София";
        age = 16;
        group = 10.3;
    }
    public Student(string name, int age, double group)
    {
        this.name = name;
        this.age = age;
        this.group = group;
    }
    public string Name
    {
        get { return name; }
        set { name = value; }
    }
    public int Age
    {
        get { return age; }
        set { age = value; }
    }
    public double Group
    {
        get { return group; }
        set { group = value; }
    }
    public void Study()
    {
        Console.WriteLine($"Студент по имени {this.name}, которому {this.age} лет, учится в группе {this.group}.");
    }
}

class Master : Student
{
    public Master() : base()
    {
    }
    public Master(string name, int age, double group) : base(name, age, group)
    {
    }
    public void Defend()
    {
        Console.WriteLine($"Магистр {this.name} успешно защитил свой диплом.");
    }
}
class Bacheolor : Student
{
    public Bacheolor() : base()
    {
    }
    public Bacheolor(string name, int age, double group) : base(name, age, group)
    {
    }
    public void Pass()
    {
        Console.WriteLine($"Бакалавр {this.name} успешно сдал экзамен.");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Student I = new Student();
        I.Study();

        Master magistr = new Master("Егор", 22, 1);
        magistr.Defend();

        Bacheolor bakalavr = new Bacheolor("Вася", 26, 6);
        bakalavr.Pass();
    }
    }