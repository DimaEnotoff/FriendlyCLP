using FriendlyCLP;

class Program
{
    private const string _appName = "Friendly console test";
    public const string WellcomeMessage = $"{_appName}, type \"help\", to show command list!";

    private static readonly FCLP CLProcessor = new FCLP(_appName);

    [Command("help", "Show help.")]
    private class HelpCommand : ICommand
    {
        private readonly FCLP CLProcessor;

        [Argument(0, "path", "Path to a command or a command group.", true, true)]
        private readonly StringArgument Path;
        
        public HelpCommand(FCLP clProcessor)
        {
            CLProcessor = clProcessor ?? throw new ArgumentNullException(nameof(clProcessor));
        }

        public string Execute()
        {
            if (CLProcessor.GetHelp(Path.Value, out var helpArticle))
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
        CLProcessor.AddGroup("", "tu", "Some useful text utils.")
        .AddGroup("tu", "mt", "Calculate various string metrics.")
        .AddGroup("", "calc", "Do some calculus.")
        .AddCommand("tu", new DisplaySampleTextCommand())
        .AddCommand("tu", new RemoveCharacterCommand())
        .AddCommand("tu mt", new CountCharactersCommand())
        .AddCommand("tu mt", new CountWordsCommand())
        .AddCommand("calc", new AddArrayCommand())
        .AddCommand("calc", new DivideCommand())
        .AddCommand("", new ShowDateTimeCommand())
        .AddCommand("", new HelpCommand(CLProcessor));
        Console.WriteLine(WellcomeMessage);
        while (true) Console.WriteLine(CLProcessor.ProcessLine(Console.ReadLine()));
    }
}