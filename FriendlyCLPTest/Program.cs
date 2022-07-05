using FriendlyCLP;

class Program
{
    private const string _appName = "Friendly console test";
    public const string WellcomeMessage = $"{_appName}, type \"help\", to show command list!";

    private static readonly CommandGroup consoleRoot = new CommandGroup(_appName);

    [Command("help", "Show help.")]
    private class HelpCommand : ICommand
    {
        private readonly CommandGroup CLProcessor;

        [Argument(0, "path", "Path to a command or a command group.", true, true)]
        private readonly StringArgument Path;
        
        public HelpCommand(CommandGroup cLProcessor)
        {
            CLProcessor = cLProcessor ?? throw new ArgumentNullException(nameof(cLProcessor));
        }

        public string Execute()
        {
            if (CLProcessor.getHelp(Path.Value, out var helpArticle))
            {
                if (Path.IsOmmited)
                    helpArticle += Environment.NewLine + "In order to get help on a particular command please type \"help pathToCommand commandName\".";
                return helpArticle;
            }
            else
                return Environment.NewLine + "Nothing found. Please type \"help\" with no arguments.";
        }
    }

    /// <summary>
    /// Simple command without arguments.
    /// </summary>
    [Command("dst", "Display sample text.")]
    private class DisplaySampleTextCommand : ICommand
    {
        public string Execute() => "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut ...";
    }

    /// <summary>
    /// Command takes one compulsory argument.
    /// </summary>
    [Command("cc", "Count characters in a word.")]
    private class CountCharactersCommand : ICommand
    {
        [Argument(0, "word", "Word to count characters in.", optional: false, multisegmented: false)]
        private readonly StringArgument Word;
        public string Execute() => Word.Value.Length.ToString();
    }

    /// <summary>
    /// Command takes one multisegmented (that can contain spaces) compulsory argument.
    /// Multisegmented argument should always be the last one.
    /// </summary>
    [Command("cw", "Count words.")]
    private class CountWordsCommand : ICommand
    {
        [Argument(0, "text", "Text to count words in.", optional: false, multisegmented: true)]
        private readonly StringArgument Text;

        public string Execute() => Text.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Count().ToString();
    }

    /// <summary>
    /// Command takes two arguments, one compulsory and one optional.
    /// Optional argument should always be the last one.
    /// </summary>
    [Command("rc", "Remove character in a word.")]
    private class RemoveCharacterCommand : ICommand
    {
        [Argument(0, "word", "Word to remove character from.", optional: false, multisegmented: false)]
        private readonly StringArgument Word;
        [Argument(1, "char", "Character to be removed.", optional: true, multisegmented: false)]
        private readonly StringArgument Char;
        public string Execute() {
            if (Char.IsOmmited) //Checking if user ommited this argument
                return Word.Value;
            return Word.Value.Replace(Char.Value, "");
        }
    }

    /// <summary>
    /// Command takes two compulsory integer arguments.
    /// </summary>
    [Command("div", "Divide two integer values.")]
    class DivideCommand : ICommand
    {

        [Argument(0, "dvd", "Dividend", optional: false, multisegmented: false)]
        private readonly IntArgument Divident;

        [Argument(1, "dvs", "Divisor", optional: false, multisegmented: false)]
        private readonly IntArgument Divisor;

        public string Execute()
        {
            if (Divisor.Value == 0) return "Can not divide by zero.";
            return ((float)Divident.Value / Divisor.Value).ToString();
        }
    }

    /// <summary>
    /// Command takes arbitrary number of integer arguments including zero.
    /// </summary>
    [Command("add", "Add arbitrary number of values.")]
    class AddArrayCommand : ICommand
    {

        [Argument(0, "values", "Array of values, split by spaces", optional: true, multisegmented: true)]
        private readonly IntArrayArgument Array;

        public string Execute()
        {
            checked
            {
                try
                {
                    var sum = 0;
                    foreach (var value in Array.Value)
                    {
                        sum += value;
                    }
                    return sum.ToString();
                }
                catch (OverflowException)
                {
                    return "Can not compute, sum is too big.";
                }
            }
        }
    }

    /// <summary>
    /// Custom argument example.
    /// </summary>
    public class TimeTypeArgument : Argument
    {
        public enum TimeType { DATE, TIME, FULL }
        public TimeType Value { get; private set; }

        public override bool Convert(string line, out string errorMessage)
        {
            switch (line.ToLower())
            {
                case "t":
                case "time":
                    Value = TimeType.TIME;
                    errorMessage = null;
                    return true;
                case "d":
                case "date":
                    Value = TimeType.DATE;
                    errorMessage = null;
                    return true;
                case "f":
                case "full":
                    Value = TimeType.FULL;
                    errorMessage = null;
                    return true;
                default:
                    errorMessage = "Error parsing \"" + Name + "\" argument. Only d/t/f or date/time/full values are expected.";
                    return false;
            }
        }

        public override void SetDefault()
        {
            Value = TimeType.FULL;
        }

        public TimeTypeArgument(ArgumentAttribute attribute) : base(attribute) { }

    }

    /// <summary>
    /// Command consuming a custom argument.
    /// </summary>
    [Command("scdt", "Show current date time.")]
    class ShowDateTimeCommand : ICommand
    {
        [Argument(0, "format", "Date time format: d/t/f or date/time/full.", optional: true, multisegmented: false)]
        private readonly TimeTypeArgument Type;

        public string Execute()
        {
            switch (Type.Value)
            {
                case TimeTypeArgument.TimeType.DATE:
                    return DateTime.Now.ToShortDateString();
                case TimeTypeArgument.TimeType.TIME:
                    return DateTime.Now.ToShortTimeString();
                case TimeTypeArgument.TimeType.FULL:
                    return DateTime.Now.ToString();
                default:
                    return "Internal error.";
            }
        }
    }

    static void Main(string[] args)
    {
        consoleRoot.AddGroup("", "tu", "Some useful text utils.");
        consoleRoot.AddGroup("tu", "mt", "Calculate various string metrics.");
        consoleRoot.AddGroup("", "calc", "Do some calculus.");
        consoleRoot.AddCommand("tu", new DisplaySampleTextCommand());
        consoleRoot.AddCommand("tu", new RemoveCharacterCommand());
        consoleRoot.AddCommand("tu mt", new CountCharactersCommand());
        consoleRoot.AddCommand("tu mt", new CountWordsCommand());
        consoleRoot.AddCommand("calc", new AddArrayCommand());
        consoleRoot.AddCommand("calc", new DivideCommand());
        consoleRoot.AddCommand("", new ShowDateTimeCommand());
        consoleRoot.AddCommand("", new HelpCommand(consoleRoot));
        Console.WriteLine(WellcomeMessage);
        while (true) Console.WriteLine(consoleRoot.ProcessLine(Console.ReadLine()));
    }
}