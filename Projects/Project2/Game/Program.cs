using System;
namespace Project
{
    public class Game // Основной класс игры
    {
        private GameState state;
        private Location currentLocation;
        public Game(GameState state, Location currentLocation)
        {
            this.state = state;
            this.currentLocation = currentLocation;
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
    }

    public class GameState // Состояние игры
    {
        private int health;
        private bool isGameOver;
        private List<string> inventory;
        private Dictionary<string, bool> worldFlags;
        private List<string> log;
        private int turnCount;
        public GameState(int health, List<string> inventory, Dictionary<string, bool> worldFlags, int turnCount)
        {
            this.health = health;
            this.isGameOver = false;
            this.inventory = inventory ?? new List<string>();
            this.worldFlags = worldFlags ?? new Dictionary<string, bool>();
            this.log = new List<string>();
            this.turnCount = turnCount;
        }
        public int Health
        {
            get { return this.health; }
            set { this.health = value; }
        }

        public bool IsGameOver
        {
            get { return this.isGameOver; }
            set { this.isGameOver = value; }
        }

        public List<string> Inventory
        {
            get { return this.inventory; }
            set { this.inventory = value; }
        }

        public Dictionary<string, bool> WorldFlags
        {
            get { return this.worldFlags; }
            set { this.worldFlags = value; }
        }

        public List<string> Log
        {
            get { return this.log; }
            set { this.log = value; }
        }

        public int TurnCount
        {
            get { return this.turnCount; }
            set { this.turnCount = value; }
        }
    }

    public class Location // Локация
    {
        private string name;
        private string description;
        private List<IInteractable> interactables;
        private List<GameEventBase> locationEvents;
        private Dictionary<string, string> exits;
        public Location()
        {
            this.name = string.Empty;
            this.description = string.Empty;
            this.interactables = new List<IInteractable>();
            this.locationEvents = new List<GameEventBase>();
            this.exits = new Dictionary<string, string>();
        }
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
            get { return this.name; }
            set { this.name = value; }
        }

        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        public List<IInteractable> Interactables
        {
            get { return this.interactables; }
            set { this.interactables = value; }
        }

        public List<GameEventBase> LocationEvents
        {
            get { return this.locationEvents; }
            set { this.locationEvents = value; }
        }

        public Dictionary<string, string> Exits
        {
            get { return this.exits; }
            set { this.exits = value; }
        }
    }
    
    
    interface ICommand // Интерфейс команды пользователя
    {
        void Execute(Game game, string args);
        // game - объект игры для доступа к состоянию и локациям
        // args - аргументы команды (например, "chest" для команды interact)
    }

    interface IInteractable // Интерфейс всех объектов, с которыми можно взаимодействовать в мире
    {
        string GetName(); // Уникальный идентификатор объекта (для команд игрока)
        string GetDescription(); // Описание объекта при осмотре
        void Interact(GameState state); // Взаимодействие с объектом
        // state - текущее состояние игры
    }

    interface ICondition // Интерфейс условия для проверки состояния игры
    {
        bool IsMet(GameState state); // Проверить, выполняется ли условие состояния
    }

    interface IEffect // Интерфейс эффекта, изменяющего состояние игры
    {
        void Aply(GameState state); // Применить эффект к состоянию игры
    }

    abstract class CommandBase : ICommand
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

    abstract class ConditionBase
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

    abstract class EffectBase
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

    abstract class GameEventBase
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


    
    class Program
    {
        static void Main(string[] args)
        {
            int n = int.Parse(Console.ReadLine());
            Console.WriteLine($"В ВВОДЕ N == {n}");
            //solution(n);
        }
    }
}
