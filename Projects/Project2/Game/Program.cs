using System;
using System.Collections.Generic;
using System.Linq;

namespace Project
{
    #region Interfaces
    public interface ICommand
    {
        void Execute(Game game, string args);
        string Name { get; }
        string Description { get; }
    }

    public interface IInteractable
    {
        string GetName();
        string GetDescription();
        void Interact(GameState state);
    }

    public interface ICondition
    {
        bool IsMet(GameState state);
    }

    public interface IEffect
    {
        void Apply(GameState state);
    }
    #endregion

    #region Abstract Classes
    public abstract class CommandBase : ICommand
    {
        private string name;
        private string description;

        protected CommandBase(string name, string description)
        {
            this.name = name;
            this.description = description;
        }

        public string Name => name;
        public string Description => description;
        public abstract void Execute(Game game, string args);

        protected bool IsGameActive(GameState state) => state.Health > 0 && !state.IsGameOver;

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

    public abstract class ConditionBase : ICondition
    {
        private string description;
        protected ConditionBase(string description) { this.description = description; }
        public string Description => description;
        public abstract bool IsMet(GameState state);
        public override string ToString() => $"Condition: {Description}";
    }

    public abstract class EffectBase : IEffect
    {
        protected string message;
        protected EffectBase(string message) { this.message = message; }
        public string Message => message;
        public abstract void Apply(GameState state);

        protected void LogMessage(GameState state)
        {
            if (!string.IsNullOrEmpty(message))
                state.AddLog(message);
        }
    }

    public abstract class GameEventBase
    {
        private ICondition triggerCondition;
        private List<IEffect> effects;
        private bool isOneTime;
        private string eventName;

        protected GameEventBase(ICondition triggerCondition, List<IEffect> effects, bool isOneTime, string eventName)
        {
            this.triggerCondition = triggerCondition;
            this.effects = effects;
            this.isOneTime = isOneTime;
            this.eventName = eventName;
        }

        public ICondition TriggerCondition { get { return triggerCondition; } set { triggerCondition = value; } }
        public List<IEffect> Effects { get { return effects; } set { effects = value; } }
        public bool IsOneTime { get { return isOneTime; } set { isOneTime = value; } }
        public string EventName { get { return eventName; } set { eventName = value; } }

        public virtual bool CheckAndApply(GameState state)
        {
            if (TriggerCondition.IsMet(state))
            {
                foreach (var effect in Effects) effect.Apply(state);
                return IsOneTime;
            }
            return false;
        }
    }

    public class OnEnterLocationEvent : GameEventBase
    {
        public OnEnterLocationEvent(ICondition condition, List<IEffect> effects, bool isOneTime, string name)
            : base(condition, effects, isOneTime, name) { }
    }

    public class OnTurnEvent : GameEventBase
    {
        public OnTurnEvent(ICondition condition, List<IEffect> effects, bool isOneTime, string name)
            : base(condition, effects, isOneTime, name) { }
    }
    #endregion

    #region State & Core
    public class GameState
    {
        private int health;
        private bool isGameOver;
        private List<string> inventory;
        private Dictionary<string, bool> worldFlags;
        private List<string> log;
        private int turnCount;
        private List<Quest> quests;

        // Свойство для принудительной смены локации через эффекты
        public string NextLocationName { get; set; }
        public Action<Location> OnLocationChanged { get; set; }

        public GameState(int health, int turnCount)
        {
            this.health = health;
            this.isGameOver = false;
            this.inventory = new List<string>();
            this.worldFlags = new Dictionary<string, bool>();
            this.log = new List<string>();
            this.turnCount = turnCount;
            this.quests = new List<Quest>();
            this.NextLocationName = null;
        }

        public int Health { get { return health; } set { health = value; if (health < 0) health = 0; } }
        public bool IsGameOver { get { return isGameOver; } set { isGameOver = value; } }
        public List<string> Inventory { get { return inventory; } set { inventory = value; } }
        public Dictionary<string, bool> WorldFlags { get { return worldFlags; } set { worldFlags = value; } }
        public List<string> Log { get { return log; } set { log = value; } }
        public int TurnCount { get { return turnCount; } set { turnCount = value; } }
        public List<Quest> Quests { get { return quests; } set { quests = value; } }

        public void AddItem(string item) { if (!inventory.Contains(item)) inventory.Add(item); }
        public void RemoveItem(string item) { inventory.Remove(item); }
        public bool HasItem(string item) { return inventory.Contains(item); }
        public void SetFlag(string key, bool value) { worldFlags[key] = value; }
        public bool GetFlag(string key) { return worldFlags.ContainsKey(key) && worldFlags[key]; }

        public void Damage(int value)
        {
            Health -= value;
            if (Health <= 0) { Health = 0; IsGameOver = true; }
        }
        public void Heal(int value) { Health += value; if (Health > 100) Health = 100; }
        public void AddLog(string message) { log.Add(message); Console.WriteLine(message); }
        public void NextTurn() { turnCount++; }
    }

    public class Game
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
            this.state.OnLocationChanged = HandleLocationEnter;
        }

        public GameState State { get { return this.state; } set { this.state = value; } }
        public Location CurrentLocation { get { return this.currentLocation; } set { this.currentLocation = value; } }
        public Dictionary<string, Location> Locations { get { return locations; } set { locations = value; } }
        public Dictionary<string, ICommand> Commands { get { return commands; } set { commands = value; } }

        private void InitWorld()
        {
            var hall = new Location("Hall", "Вы очнулись в жилом отсеке станции. Воздух спёрт, свет мигает.");
            var storage = new Location("Storage", "Тёмный склад. Везде стеллажи и коробки. Пахнет машинным маслом.");
            var darkCorridor = new Location("DarkCorridor", "Длинный коридор без освещения. Холодно и тихо.");
            var generatorRoom = new Location("GeneratorRoom", "Комната резервного генератора. В центре стоит распределительный щит.");
            var exit = new Location("Exit", "Гермодверь в ангар. Спасательный челнок готов к отстыковке.");

            hall.Exits["south"] = "Storage";
            darkCorridor.Exits["east"] = "GeneratorRoom";

            var hallLocker = new Chest("Шкафчик", new List<IEffect> { new AddItemEffect("Torch", "Вы нашли фонарик в шкафчике.") });
            hall.Interactables.Add(hallLocker);

            var storageChest = new Chest("Ящик с инструментами", new List<IEffect> { new AddItemEffect("Wrench", "Вы нашли гаечный ключ.") });
            storage.Interactables.Add(storageChest);

            var storageKeyBox = new Chest("Стеллаж", new List<IEffect> { new AddItemEffect("Key", "Вы нашли ключ-карту от двери.") });
            storage.Interactables.Add(storageKeyBox);

            var trap = new Trap("Растяжка", new FlagCondition("TrapArmed"),
                new List<IEffect> {
                    new DamageEffect(15, "Вы задели растяжку! Осколок вонзился в ногу."),
                    new SetFlagEffect("TrapArmed", false, "Ловушка сработала и больше не опасна.")
                });
            state.SetFlag("TrapArmed", true);
            storage.Interactables.Add(trap);

            darkCorridor.LocationEvents.Add(new OnEnterLocationEvent(
                new NotCondition(new HasItemCondition("Torch")),
                new List<IEffect> { new DamageEffect(20, "Тьма обжигает кожу. Вы теряете здоровье, пробираясь вслепую.") },
                false, "DarknessDamage"));

            var darkCorridorFloor = new Chest("Пол в коридоре", new List<IEffect> { new AddItemEffect("Fuse", "Вы подобрали предохранитель.") });
            darkCorridor.Interactables.Add(darkCorridorFloor);

            var mainDoor = new Door("Гермодверь", new HasItemCondition("Key"),
                new List<IEffect> { new AddExitEffect(hall, "south", "DarkCorridor", "Дверь с шипением открылась. Путь в тёмный коридор свободен.") },
                new List<IEffect> { new LogEffect("Дверь заблокирована. Требуется ключ-карта.") });
            hall.Interactables.Add(mainDoor);

            var generatorPanel = new Chest("Распределительный щит",
                new AndCondition(new HasItemCondition("Fuse"), new HasItemCondition("Wrench")),
                new List<IEffect> {
                    new RemoveItemEffect("Fuse"),
                    new RemoveItemEffect("Wrench"),
                    new SetFlagEffect("GeneratorOn", true, "Вы вставили предохранитель и затянули болты. Генератор ожил!"),
                    new LogEffect("Питание восстановлено. Гермодвери разблокированы."),
                    new AddExitEffect(exit, "west", "Exit", "Свет загорелся, и дверь шлюза отъехала в сторону.")
                },
                new List<IEffect> { new LogEffect("Для запуска нужен предохранитель и гаечный ключ.") });
            generatorRoom.Interactables.Add(generatorPanel);

            generatorRoom.Interactables.Add(new Chest("Аптечка на стене", new List<IEffect> { new HealEffect(40, "Вы использовали аптечку. Стало легче дышать.") }));

            var exitTerminal = new Terminal("Пульт шлюза",
                new Dictionary<string, (ICondition, List<IEffect>)> {
                    { "activate", (new FlagCondition("GeneratorOn"), new List<IEffect> {
                        new ChangeLocationEffect("Credits", "Шлюз открыт. Вы садитесь в челнок. Спасение близко!"),
                        new SetFlagEffect("GameWon", true)
                    })}
                });
            exit.Interactables.Add(exitTerminal);
            exit.LocationEvents.Add(new OnEnterLocationEvent(
                new FlagCondition("GameWon"),
                new List<IEffect> { new LogEffect("Поздравляем! Вы выбрались со станции!") },
                true, "Victory"));

            locations.Add("Hall", hall);
            locations.Add("Storage", storage);
            locations.Add("DarkCorridor", darkCorridor);
            locations.Add("GeneratorRoom", generatorRoom);
            locations.Add("Exit", exit);
            locations.Add("Credits", new Location("Credits", "Титры. Конец."));

            currentLocation = hall;
            state.Quests.Add(CreatePowerQuest());
            state.Quests.Add(CreateEscapeQuest());
        }

        private void InitCommands()
        {
            commands["help"] = new HelpCommand();
            commands["look"] = new LookCommand();
            commands["go"] = new GoCommand();
            commands["interact"] = new InteractCommand();
            commands["inv"] = new InventoryCommand();
            commands["status"] = new StatusCommand();
        }

        public void Run()
        {
            InitWorld();
            InitCommands();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== STATION PROGER-67 ===");
            Console.ResetColor();
            Console.WriteLine("Введите 'help' для списка команд.\n");

            HandleLocationEnter(currentLocation);

            while (!state.IsGameOver && currentLocation.Name != "Credits")
            {
                state.NextTurn();
                foreach (var evt in currentLocation.LocationEvents)
                {
                    if (evt is OnTurnEvent turnEvent) turnEvent.CheckAndApply(state);
                }
                CheckQuestUpdates();

                Console.Write("\n> ");
                var input = Console.ReadLine()?.Trim().ToLower() ?? "";
                if (string.IsNullOrEmpty(input)) continue;

                var parts = input.Split(new[] { ' ' }, 2);
                var cmdName = parts[0];
                var args = parts.Length > 1 ? parts[1] : "";

                if (commands.ContainsKey(cmdName))
                {
                    commands[cmdName].Execute(this, args);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Неизвестная команда. Введите 'help'.");
                    Console.ResetColor();
                }

                // Обработка принудительной смены локации из эффектов
                if (!string.IsNullOrEmpty(state.NextLocationName))
                {
                    if (locations.ContainsKey(state.NextLocationName))
                    {
                        currentLocation = locations[state.NextLocationName];
                        HandleLocationEnter(currentLocation);
                    }
                    state.NextLocationName = null; // Сброс после обработки
                }

                if (state.IsGameOver && currentLocation.Name != "Credits")
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("\n💀 ВАШ СИГНАЛ ЖИЗНИ УГАС. ИГРА ОКОНЧЕНА.");
                    Console.ResetColor();
                    break;
                }
            }

            if (currentLocation.Name == "Credits" && state.GetFlag("GameWon"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n🎉 ВЫЖИВАНИЕ ЗАВЕРШЕНО УСПЕШНО. СПАСИБО ЗА ИГРУ!");
                Console.ResetColor();
            }
        }

        public void HandleLocationEnter(Location location)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n📍 {location.Name}");
            Console.ResetColor();
            Console.WriteLine(location.Description);

            foreach (var evt in location.LocationEvents)
            {
                if (evt is OnEnterLocationEvent enterEvent) enterEvent.CheckAndApply(state);
            }
            CheckQuestUpdates();
        }

        private void CheckQuestUpdates()
        {
            foreach (var quest in state.Quests) quest.Check(state);
        }

        private Quest CreatePowerQuest()
        {
            var q = new Quest("Аварийное питание", "Запустить резервный генератор.");
            q.AddStage("Найти гаечный ключ", new HasItemCondition("Wrench"), "Гаечный ключ найден! Теперь можно открутить панель.");
            q.AddStage("Найти предохранитель", new HasItemCondition("Fuse"), "Предохранитель цел! Генератор почти готов.");
            q.AddStage("Включить генератор", new FlagCondition("GeneratorOn"), "Гул генератора разносится по станции. Питание восстановлено!");
            return q;
        }

        private Quest CreateEscapeQuest()
        {
            var q = new Quest("Последний рубеж", "Добраться до аварийного выхода.");
            q.AddStage("Пройти тёмный коридор", new AndCondition(new FlagCondition("GeneratorOn"), new HasItemCondition("Torch")), "Коридор освещён. Вы добрались до генераторной.");
            q.AddStage("Открыть шлюз", new FlagCondition("GeneratorOn"), "Шлюз разблокирован.");
            q.AddStage("Достичь выхода", new FlagCondition("GameWon"), "Вы выбрались! КВЕСТ ВЫПОЛНЕН.");
            return q;
        }
    }

    public class Location
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
            this.exits = new Dictionary<string, string>();
        }

        public string Name { get { return name; } set { name = value; } }
        public string Description { get { return description; } set { description = value; } }
        public List<IInteractable> Interactables { get { return interactables; } set { interactables = value; } }
        public List<GameEventBase> LocationEvents { get { return locationEvents; } set { locationEvents = value; } }
        public Dictionary<string, string> Exits { get { return exits; } set { exits = value; } }
    }

    public class Player
    {
        private GameState state;
        public Player(GameState state) { this.state = state; }
        public int Health => state.Health;
    }
    #endregion

    #region Conditions
    public class HasItemCondition : ConditionBase
    {
        private string item;
        public HasItemCondition(string item) : base($"Наличие предмета: {item}") { this.item = item; }
        public override bool IsMet(GameState state) => state.HasItem(item);
    }
    public class FlagCondition : ConditionBase
    {
        private string key;
        public FlagCondition(string key) : base($"Флаг {key} == true") { this.key = key; }
        public override bool IsMet(GameState state) => state.GetFlag(key);
    }
    public class HealthCondition : ConditionBase
    {
        private int value;
        public HealthCondition(int value) : base($"Здоровье меньше {value}") { this.value = value; }
        public override bool IsMet(GameState state) => state.Health < value;
    }
    public class AndCondition : ConditionBase
    {
        private ICondition a, b;
        public AndCondition(ICondition a, ICondition b) : base("И условие") { this.a = a; this.b = b; }
        public override bool IsMet(GameState state) => a.IsMet(state) && b.IsMet(state);
    }
    public class OrCondition : ConditionBase
    {
        private ICondition a, b;
        public OrCondition(ICondition a, ICondition b) : base("ИЛИ условие") { this.a = a; this.b = b; }
        public override bool IsMet(GameState state) => a.IsMet(state) || b.IsMet(state);
    }
    public class NotCondition : ConditionBase
    {
        private ICondition condition;
        public NotCondition(ICondition condition) : base("НЕ условие") { this.condition = condition; }
        public override bool IsMet(GameState state) => !condition.IsMet(state);
    }
    #endregion

    #region Effects
    public class AddItemEffect : EffectBase
    {
        private string item;
        public AddItemEffect(string item, string message = null) : base(message ?? $"Получен предмет: {item}") { this.item = item; }
        public override void Apply(GameState state) { state.AddItem(item); LogMessage(state); }
    }
    public class RemoveItemEffect : EffectBase
    {
        private string item;
        public RemoveItemEffect(string item, string message = null) : base(message ?? $"Предмет {item} израсходован.") { this.item = item; }
        public override void Apply(GameState state) { state.RemoveItem(item); LogMessage(state); }
    }
    public class SetFlagEffect : EffectBase
    {
        private string key;
        private bool value;
        public SetFlagEffect(string key, bool value, string message = null) : base(message) { this.key = key; this.value = value; }
        public override void Apply(GameState state) { state.SetFlag(key, value); LogMessage(state); }
    }
    public class DamageEffect : EffectBase
    {
        private int damage;
        public DamageEffect(int damage, string message = null) : base(message ?? $"Получен урон: {damage}") { this.damage = damage; }
        public override void Apply(GameState state) { state.Damage(damage); LogMessage(state); }
    }
    public class HealEffect : EffectBase
    {
        private int value;
        public HealEffect(int value, string message = null) : base(message ?? $"Восстановлено {value} здоровья.") { this.value = value; }
        public override void Apply(GameState state) { state.Heal(value); LogMessage(state); }
    }
    public class LogEffect : EffectBase
    {
        public LogEffect(string message) : base(message) { }
        public override void Apply(GameState state) { LogMessage(state); }
    }
    public class AddExitEffect : EffectBase
    {
        private Location targetLocation;
        private string directionName, locationKey;
        public AddExitEffect(Location loc, string dir, string locKey, string message = null) : base(message ?? "Открыт новый проход.")
        { this.targetLocation = loc; this.directionName = dir; this.locationKey = locKey; }
        public override void Apply(GameState state)
        { if (targetLocation != null && !targetLocation.Exits.ContainsKey(directionName)) targetLocation.Exits[directionName] = locationKey; LogMessage(state); }
    }
    public class ChangeLocationEffect : EffectBase
    {
        private string targetLocationName;
        public ChangeLocationEffect(string locationName, string message = null) : base(message) { this.targetLocationName = locationName; }
        public override void Apply(GameState state)
        {
            LogMessage(state);
            // Исправлено: используем string-свойство вместо SetFlag, так как флаги принимают только bool
            state.NextLocationName = targetLocationName;
        }
    }
    #endregion

    #region Interactables
    public class Chest : IInteractable
    {
        private string name;
        private List<IEffect> effectsOnOpen, onFail;
        private ICondition openCondition;
        private bool isOpened;

        public Chest(string name, List<IEffect> effects) : this(name, null, effects, new List<IEffect>()) { }
        public Chest(string name, List<IEffect> effects, ICondition condition) : this(name, condition, effects, new List<IEffect>()) { }
        public Chest(string name, ICondition condition, List<IEffect> onSuccess, List<IEffect> onFail)
        {
            this.name = name;
            this.openCondition = condition;
            this.effectsOnOpen = onSuccess;
            this.onFail = onFail;
            this.isOpened = false;
        }

        public string GetName() => name;
        public string GetDescription() => isOpened ? "Уже открыт и пуст." : "Закрытый контейнер.";
        public void Interact(GameState state)
        {
            if (isOpened) { state.AddLog("Вы уже всё забрали отсюда."); return; }
            if (openCondition != null && !openCondition.IsMet(state))
            {
                foreach (var eff in onFail) eff.Apply(state);
                return;
            }
            foreach (var eff in effectsOnOpen) eff.Apply(state);
            isOpened = true;
        }
    }

    public class Door : IInteractable
    {
        private string name;
        private ICondition openCondition;
        private List<IEffect> onSuccess, onFail;

        public Door(string name, ICondition condition, List<IEffect> success, List<IEffect> fail)
        { this.name = name; this.openCondition = condition; this.onSuccess = success; this.onFail = fail; }

        public string GetName() => name;
        public string GetDescription() => "Массивная дверь.";
        public void Interact(GameState state)
        {
            if (openCondition.IsMet(state)) { foreach (var eff in onSuccess) eff.Apply(state); }
            else { foreach (var eff in onFail) eff.Apply(state); }
        }
    }

    public class Terminal : IInteractable
    {
        private string name;
        private Dictionary<string, (ICondition Condition, List<IEffect> Effects)> dialogOptions;

        public Terminal(string name, Dictionary<string, (ICondition, List<IEffect>)> options)
        { this.name = name; this.dialogOptions = options; }

        public string GetName() => name;
        public string GetDescription() => "Рабочий терминал с мигающим курсором.";
        public void Interact(GameState state)
        {
            foreach (var kvp in dialogOptions)
            {
                if (kvp.Value.Condition.IsMet(state))
                {
                    foreach (var eff in kvp.Value.Effects) eff.Apply(state);
                    state.AddLog($"Терминал принял команду '{kvp.Key}'.");
                    return;
                }
            }
            state.AddLog("Терминал требует выполнения дополнительных условий.");
        }
    }

    public class Trap : IInteractable
    {
        private string name;
        private ICondition triggerCondition;
        private List<IEffect> trapEffects;
        private bool isTriggered;

        public Trap(string name, ICondition condition, List<IEffect> effects)
        { this.name = name; this.triggerCondition = condition; this.trapEffects = effects; this.isTriggered = false; }

        public string GetName() => name;
        public string GetDescription() => isTriggered ? "Обезвреженная растяжка." : "Осторожно! Натянутая проволока.";
        public void Interact(GameState state)
        {
            if (isTriggered) return;
            if (triggerCondition.IsMet(state))
            {
                foreach (var eff in trapEffects) eff.Apply(state);
                isTriggered = true;
            }
        }
    }
    #endregion

    #region Commands
    public class HelpCommand : CommandBase
    {
        public HelpCommand() : base("help", "Список команд") { }
        public override void Execute(Game game, string args)
        {
            Console.WriteLine("\n=== ДОСТУПНЫЕ КОМАНДЫ ===");
            foreach (var cmd in game.Commands.Values) Console.WriteLine($"- {cmd.Name}: {cmd.Description}");
            Console.WriteLine("=========================");
        }
    }
    public class LookCommand : CommandBase
    {
        public LookCommand() : base("look", "Осмотр локации") { }
        public override void Execute(Game game, string args)
        {
            Console.WriteLine("\n" + game.CurrentLocation.Description);
            if (game.CurrentLocation.Interactables.Count > 0)
            { Console.WriteLine("Объекты:"); foreach (var obj in game.CurrentLocation.Interactables) Console.WriteLine($"- {obj.GetName()}: {obj.GetDescription()}"); }
            if (game.CurrentLocation.Exits.Count > 0)
            { Console.WriteLine("Выходы:"); foreach (var ex in game.CurrentLocation.Exits) Console.WriteLine($"- {ex.Key} -> {ex.Value}"); }
        }
    }
    public class GoCommand : CommandBase
    {
        public GoCommand() : base("go", "Переход (go <направление>)") { }
        public override void Execute(Game game, string args)
        {
            if (string.IsNullOrEmpty(args)) { PrintError("Укажите направление. Пример: go south"); return; }
            var dir = args.Trim().ToLower();
            if (game.CurrentLocation.Exits.TryGetValue(dir, out string targetName))
            {
                if (game.Locations.TryGetValue(targetName, out Location target))
                {
                    game.CurrentLocation = target;
                    game.HandleLocationEnter(target);
                }
            }
            else PrintError("Туда пройти нельзя.");
        }
    }
    public class InteractCommand : CommandBase
    {
        public InteractCommand() : base("interact", "Взаимодействие (interact <объект>)") { }
        public override void Execute(Game game, string args)
        {
            if (string.IsNullOrEmpty(args)) { PrintError("Укажите объект. Пример: interact шкафчик"); return; }
            var target = args.Trim().ToLower();
            var obj = game.CurrentLocation.Interactables.FirstOrDefault(o => o.GetName().ToLower().Contains(target));
            if (obj != null) obj.Interact(game.State);
            else PrintError("Такого объекта здесь нет.");
        }
    }
    public class InventoryCommand : CommandBase
    {
        public InventoryCommand() : base("inv", "Просмотр инвентаря") { }
        public override void Execute(Game game, string args)
        {
            if (game.State.Inventory.Count == 0) Console.WriteLine("Инвентарь пуст.");
            else { Console.WriteLine("Ваш инвентарь:"); foreach (var item in game.State.Inventory) Console.WriteLine($"- {item}"); }
        }
    }
    public class StatusCommand : CommandBase
    {
        public StatusCommand() : base("status", "Статус игрока") { }
        public override void Execute(Game game, string args)
        {
            Console.WriteLine($"❤️ Здоровье: {game.State.Health}/100");
            Console.WriteLine($"🚶 Ход: {game.State.TurnCount}");
            if (game.State.Log.Count > 0)
            { Console.WriteLine("📜 Последние события:"); foreach (var msg in game.State.Log.Skip(Math.Max(0, game.State.Log.Count - 3))) Console.WriteLine($"  {msg}"); }
        }
    }
    #endregion

    #region Quests
    public class QuestStage
    {
        public string Name { get; set; }
        public ICondition Condition { get; set; }
        public string CompleteMessage { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class Quest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public List<QuestStage> Stages { get; set; } = new List<QuestStage>();

        public Quest(string name, string description) { Name = name; Description = description; }
        public void AddStage(string name, ICondition condition, string completeMsg)
        { Stages.Add(new QuestStage { Name = name, Condition = condition, CompleteMessage = completeMsg, IsCompleted = false }); }

        public void Check(GameState state)
        {
            if (IsCompleted) return;
            foreach (var stage in Stages)
            {
                if (!stage.IsCompleted && stage.Condition.IsMet(state))
                {
                    stage.IsCompleted = true;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"📌 КВЕСТ '{Name}' ОБНОВЛЁН: Этап '{stage.Name}' выполнен! {stage.CompleteMessage}");
                    Console.ResetColor();
                }
            }
            if (Stages.All(s => s.IsCompleted))
            {
                IsCompleted = true;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ КВЕСТ '{Name}' ЗАВЕРШЁН!");
                Console.ResetColor();
            }
        }
    }
    #endregion

    class Program
    {
        static void Main(string[] args)
        {
            var gameState = new GameState(100, 0);
            var startLocation = new Location("Hall", "");
            var game = new Game(gameState, startLocation);
            game.Run();
        }
    }
}