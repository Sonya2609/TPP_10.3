using System;
namespace Project
{
    public class GameState // Состояние игры
    {
        private int health;
        private bool isGameOver;
        private List<string> inventory;
        private Dictionary<string, bool> worldFlags;
        private List<string> log;
        private int turnCount;
        public GameState(int health, int turnCount)
        {
            this.health = health;
            this.isGameOver = false;
            this.inventory = new List<string>();
            this.worldFlags = new Dictionary<string, bool>();
            this.log = new List<string>();
            this.turnCount = turnCount;
        }
        public int Health
        {
            get { return health; }
            set { health = value; }
        }

        public bool IsGameOver
        {
            get { return isGameOver; }
            set { isGameOver = value; }
        }

        public List<string> Inventory
        {
            get { return inventory; }
            set { inventory = value; }
        }

        public Dictionary<string, bool> WorldFlags
        {
            get { return worldFlags; }
            set { worldFlags = value; }
        }

        public List<string> Log
        {
            get { return log; }
            set { log = value; }
        }

        public int TurnCount
        {
            get { return turnCount; }
            set { turnCount = value; }
        }

        public void AddItem(string item)
        {
            if (!inventory.Contains(item))
                inventory.Add(item);
        }

        public void RemoveItem(string item)
        {
            inventory.Remove(item);
        }

        public bool HasItem(string item)
        {
            return inventory.Contains(item);
        }

        public void SetFlag(string key, bool value)
        {
            worldFlags[key] = value;
        }

        public bool GetFlag(string key)
        {
            return worldFlags.ContainsKey(key) && worldFlags[key];
        }

        public void Damage(int value)
        {
            health -= value;
            if (health < 0) health = 0;
        }

        public void AddLog(string message)
        {
            log.Add(message);
            Console.WriteLine(message);
        }

        public void NextTurn()
        {
            turnCount++;
        }
    }

    public class Game // Основной класс игры
    {
        private GameState state;
        private Dictionary<string, Location> locations;
        private Location currentLocation;
        private Dictionary<string, ICommand> commands;
        public Game(GameState state, Location currentLocation)
        {
            this.state = state;
            this.currentLocation = currentLocation;
            this.locations = new Dictionary<string, Location>();
            this.commands = new Dictionary<string, ICommand>();
        }
        public GameState State
        {
            get { return this.state; }
            set { this.state = value; }
        }

        public Location CurrentLocation
        {
            get { return this.currentLocation; }
            set { this.currentLocation = value; }
        }
        public Dictionary<string, Location> Locations
        {
            get { return locations; }
            set { locations = value; }
        }
        public Dictionary<string, ICommand> Commands
        {
            get { return commands; }
            set { commands = value; }
        }
    //     private void InitWorld()
    //     {
    //     var hall = new Location("Hall", "Вы в холле.");
    //     var storage = new Location("Storage", "Склад.");

    //     hall.Exits["storage"] = "Storage";

    //     hall.Interactables.Add(new Chest("Шкаф", new List<IEffect>
    //     {
    //         new AddItemEffect("Torch")
    //     }));

    //     locations["Hall"] = hall;
    //     locations["Storage"] = storage;

    //     currentLocation = hall;
    // }

    // private void InitCommands()
    // {
    //     commands["look"] = new LookCommand();
    //     commands["go"] = new GoCommand();
    //     commands["interact"] = new InteractCommand();
    //     commands["inv"] = new InventoryCommand();
    // }

    // public void Run()
    // {
    //     while (true)
    //     {
    //         state.NextTurn();

    //         foreach (var e in currentLocation.Events)
    //             e.Check(state);

    //         Console.Write("> ");
    //         var input = Console.ReadLine();
    //         var parts = input.Split(' ', 2);

    //         var cmd = parts[0];
    //         var args = parts.Length > 1 ? parts[1] : "";

    //         if (commands.ContainsKey(cmd))
    //             commands[cmd].Execute(this, args);
    //         else
    //             Console.WriteLine("Неизвестная команда");
    //     }
    // }

    }

    public class Location // Локация
    {
        private string name;
        private string description;
        private List<IInteractable> interactables;
        private List<GameEventBase> locationEvents;
        private Dictionary<string, string> exits;
        public Location(string name, string description)
        {
            this.name = name;
            this.description = description;
            this.interactables = new List<IInteractable>();
            this.locationEvents = new List<GameEventBase>();
            this.exits = exits ?? new Dictionary<string, string>();
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public List<IInteractable> Interactables
        {
            get { return interactables; }
            set { interactables = value; }
        }

        public List<GameEventBase> LocationEvents
        {
            get { return locationEvents; }
            set { locationEvents = value; }
        }

        public Dictionary<string, string> Exits
        {
            get { return exits; }
            set { exits = value; }
        }
    }

    public class Player
    {
        private GameState state;
        public Player(GameState state)
        {
            this.state = state;
        }
        public int Health => state.Health;
    }
    
    
    public interface ICommand // Интерфейс команды пользователя
    {
        void Execute(Game game, string args);
        // game - объект игры для доступа к состоянию и локациям
        // args - аргументы команды (например, "chest" для команды interact)
    }

    public interface IInteractable // Интерфейс всех объектов, с которыми можно взаимодействовать в мире
    {
        string GetName(); // Уникальный идентификатор объекта (для команд игрока)
        string GetDescription(); // Описание объекта при осмотре
        void Interact(GameState state); // Взаимодействие с объектом
        // state - текущее состояние игры
    }

    public interface ICondition // Интерфейс условия для проверки состояния игры
    {
        bool IsMet(GameState state); // Проверить, выполняется ли условие состояния
    }

    public interface IEffect // Интерфейс эффекта, изменяющего состояние игры
    {
        void Aply(GameState state); // Применить эффект к состоянию игры
    }

    public abstract class CommandBase : ICommand
    {
        private string name; // Имя команды
        private string description; // Описание команды
        protected CommandBase(string name, string description)
        {
            this.name = name;
            this.description = description;
        }
        public string Name { get{ return name; } }
        public string Description { get{ return description; } }
        public abstract void Execute(Game game, string args); // Основной метод выполнения команды
        protected bool IsGameActive(GameState state) // Проверка, что игра не завершена
        {
            return state.Health > 0 && !state.IsGameOver;
        }
        protected void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ошибка: {message}");
            Console.ResetColor();
        }
        protected void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    public abstract class ConditionBase
    {
        private string description; // Описание условия для отладки (что именно проверяется)
        protected ConditionBase(string description)
        {
            this.description = description;
        }
        public string Description { get{ return description; } }
        public abstract bool IsMet(GameState state); // Проверка выполнения условия
        public override string ToString()
        {
            return $"Condition: {Description}";
        }
    }

    public abstract class EffectBase
    {
        private string message; // Сообщение, которое будет выведено при применении эффекта
        protected EffectBase(string message)
        {
            this.message = message;
        }
        public string Message { get{ return message; } }
        public abstract void Apply(GameState state); // Применить эффект
        protected void LogMessage(GameState state) // Вывод сообщения эффекта (если оно есть)
        {
            if (!string.IsNullOrEmpty(message))
            {
                state.Log.Add(message);
                Console.WriteLine(message);
            }
        }
    }

    public abstract class GameEventBase
    {
        private ICondition triggerCondition; // Условие срабатывания события
        private List<IEffect> effects; // Список эффектов, выполняемых при срабатывании
        private bool isOneTime; // Флаг: удалить событие после первого срабатывания
        private string eventName; // Название события (для логов)
        protected GameEventBase(ICondition triggerCondition, List<IEffect> effects, bool isOneTime, string eventName)
        {
            this.triggerCondition = triggerCondition;
            this.effects = effects;
            this.isOneTime = isOneTime;
            this.eventName = eventName;
        }
        public ICondition TriggerCondition
        { 
            get { return triggerCondition; }
            set { triggerCondition = value; }
        }
        public List<IEffect> Effects
        {
            get { return effects; }
            set { effects = value; }
        }
        public bool IsOneTime
        {
            get { return isOneTime; }
            set { isOneTime = value; }
        }
        public string EventName
        {
            get { return eventName; }
            set { eventName = value; }
        }
    }

    public class HasItemCondition : ConditionBase
    {
        private string item;
        public HasItemCondition(string item) : base($"Наличие предмета: {item}")
        {
            this.item = item;
        }
        public override bool IsMet(GameState state) => state.HasItem(item);
    }
    public class FlagCondition : ConditionBase
    {
        private string key;
        public FlagCondition(string key) : base($"Флаг {key} == true")
        {
            this.key = key;
        }
        public override bool IsMet(GameState state) => state.GetFlag(key);
    }
    public class HealthCondition : ConditionBase
    {
        private int value;
        public HealthCondition(int value) : base($"Здоровье меньше {value}")
        {
            this.value = value;
        }
        public override bool IsMet(GameState state)
        {
            return state.Health < value;
        }
    }
    public class AndCondition : ConditionBase
    {
        private ICondition a;
        private ICondition b;
        public AndCondition(ICondition a, ICondition b) : base("И условие")
        {
            this.a = a;
            this.b = b;
        }
        public override bool IsMet(GameState state)
        {
            return a.IsMet(state) && b.IsMet(state);
        }
    }
    public class OrCondition : ConditionBase
    {
        private ICondition a;
        private ICondition b;
        public OrCondition(ICondition a, ICondition b) : base("ИЛИ условие")
        {
            this.a = a;
            this.b = b;
        }
        public override bool IsMet(GameState state)
        {
            return a.IsMet(state) || b.IsMet(state);
        }
    }
    public class NotCondition : ConditionBase
    {
        private ICondition condition;
        public NotCondition(ICondition condition) : base("НЕ условие")
        {
            this.condition = condition;
        }
        public override bool IsMet(GameState state)
        {
            return !condition.IsMet(state);
        }
    }

    public class AddItemEffect : EffectBase
    {
        private string item;
        public AddItemEffect(string item) : base("НЕ условие")
        {
            this.item = item;
        }
        public override void Apply(GameState state)
        {
            state.AddItem(item);
            state.AddLog($"Получен предмет: {item}");
        }
    }
    public class DamageEffect : EffectBase
    {
        private int damage;
        public DamageEffect(int damage) : base("НЕ условие")
        {
            this.damage = damage;
        }
        public override void Apply(GameState state)
        {
            state.Damage(damage);
            state.AddLog($"Получен урон: {damage}");
        }
    }

    public class SetFlagEffect : EffectBase
    {
        private string key;
        private bool value;
        public SetFlagEffect(string key, bool value) : base("НЕ условие")
        {
            this.key = key;
            this.value = value;
        }
        public override void Apply(GameState state)
        {
         state.SetFlag(key, value);
        }
    }

    public class LogEffect : EffectBase
    {
        public LogEffect(string message) : base(message) { }
        public override void Apply(GameState state)
        {
            state.AddLog(message);
        }
    }


    
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Привет всем");
        }
    }
}