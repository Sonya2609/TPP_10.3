using System;
namespace ConsoleApp1
{
    interface ICommand // Интерфейс команды пользователя
    {
        void Executet(Game game, string args);
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
