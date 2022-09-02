using FriendlyCLP;
using System;
using System.Linq;

namespace CommandSetExample
{

    // This is an example of how Friendly CLP commands can be organised.
    public class CommandSet
    {
        public readonly CommandProcessor CommandProcessor = new CommandProcessor("Test command set");

        /// <summary>
        /// Help command is not integrated to the CommandProcessor class in order to allow its customization.
        /// </summary>
        [Command("help|h", "show help")]
        private class HelpCommand : ICommand
        {
            private readonly CommandProcessor CommandProcessor;

            [Argument(0, "path", "path to a command or a command group", true)]
            [Optional(defaultValue: "")]
            private readonly StringArgument Path;

            public HelpCommand(CommandProcessor commandProcessor)
            {
                CommandProcessor = commandProcessor ?? throw new ArgumentNullException(nameof(commandProcessor));
            }

            public string Execute()
            {
                if (CommandProcessor.GetHelp(Path.Value, out var helpArticle))
                {
                    if (Path.IsOmitted)
                        helpArticle += Environment.NewLine + "In order to get help on a particular command please type \"help pathToCommand commandName\" or \"h pathToCommand commandName\".";
                    return helpArticle;
                }
                else
                    return "Nothing found. Please type \"help\" or \"h\" with no arguments.";
            }
        }

        /// <summary>
        /// Simple command without arguments.
        /// </summary>
        [Command("displaysampletext|dst", "display sample text")]
        private class DisplaySampleTextCommand : ICommand
        {
            public string Execute() => "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut ...";
        }

        /// <summary>
        /// Command takes one compulsory argument.
        /// </summary>
        [Command("countchars|cc", "count characters in a word")]
        private class CountCharactersCommand : ICommand
        {
            [Argument(0, "word", "word to count characters in", multisegmented: false)]
            private readonly StringArgument Word;
            public string Execute() => Word.Value.Length.ToString();
        }

        /// <summary>
        /// Command takes one multisegmented (that can contain spaces) compulsory argument.
        /// Multisegmented argument should always be the last one.
        /// </summary>
        [Command("countwords|cw", "count words")]
        private class CountWordsCommand : ICommand
        {
            [Argument(0, "text", "text to count words in", multisegmented: true)]
            private readonly StringArgument Text;

            public string Execute() => Text.Value.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries).Length.ToString();
        }

        /// <summary>
        /// Command takes two arguments, one compulsory and one optional.
        /// Optional argument should always be the last one.
        /// </summary>
        [Command("removechar|rc", "remove character in a word")]
        private class RemoveCharacterCommand : ICommand
        {
            [Argument(0, "word", "word to remove character from", multisegmented: false)]
            private readonly StringArgument Word;
            [Argument(1, "char", "character to be removed", multisegmented: false)]
            [Optional]
            private readonly CharArgument Char;
            public string Execute()
            {
                if (Char.IsOmitted) //Checking if user omitted this argument
                    return Word.Value;
                return Word.Value.Replace(Char.Value.ToString(), "");
            }
        }

        /// <summary>
        /// Invalid defaultValue example
        /// </summary>
        [Command("repw", "repeat word X number of times")]
        class RepeatWordCommand : ICommand
        {

            [Argument(0, "word", "word to repeat", multisegmented: false)]
            private readonly StringArgument Word;

            [Argument(1, "times", "number of times to repeat", multisegmented: false)]
            [Optional(defaultValue: "one")]
            private readonly IntArgument Times;

            public string Execute()
            {
                return string.Concat(Enumerable.Repeat(Word.Value + ' ', Times.Value));
            }
        }


        /// <summary>
        /// Multisegment argument example
        /// </summary>
        [Command("repp", "repeat phrase X number of times")]
        class RepeatPhraseCommand : ICommand
        {

            [Argument(0, "times", "number of times to repeat", multisegmented: false)]
            private readonly IntArgument Times;

            [Argument(1, "phrase", "phrase to repeat", multisegmented: true)]
            private readonly StringArgument Phrase;

            public string Execute()
            {
                return string.Concat(Enumerable.Repeat(Phrase.Value + ' ', Times.Value));
            }
        }

        /// <summary>
        /// Argument with extra verification
        /// </summary>
        private class IntDivisorArgument : IntArgument
        {
            public IntDivisorArgument(ArgumentAttribute attribute, OptionalAttribute optional) : base(attribute, optional) { }

            public override bool Validate(out string errorMessage)
            {
                if (Value != 0)
                {
                    errorMessage = null;
                    return true;
                }
                errorMessage = "Divisor can not be zero!";
                return false;
            }

        }

        /// <summary>
        /// Command takes two compulsory integer arguments.
        /// </summary>
        [Command("divide|div", "divide two integer values")]
        class DivideCommand : ICommand
        {

            [Argument(0, "dvd", "dividend", multisegmented: false)]
            private readonly IntArgument Divident;

            [Argument(1, "dvs", "divisor", multisegmented: false)]
            private readonly IntDivisorArgument Divisor;

            public string Execute() => ((float)Divident.Value / Divisor.Value).ToString();
        }

        /// <summary>
        /// Command takes arbitrary number of integer arguments including zero.
        /// </summary>
        [Command("add", "add arbitrary number of values")]
        class AddArrayCommand : ICommand
        {
            [Argument(0, "values", "array of values, split by spaces", multisegmented: true)]
            [Optional(defaultValue: "")]
            private readonly IntArrayArgument Array;
            public string Execute()
            {
                checked
                {
                    try
                    {
                        var sum = 0;
                        foreach (var value in Array.Value)
                            sum += value;
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
        private class DateTimeDisplayFormatArgument : Argument
        {
            public enum DateTimeDisplayFormat { DATEONLY, TIMEONLY, FULL }
            public DateTimeDisplayFormat Value { get; private set; }

            public override bool Convert(string line, out string errorMessage)
            {
                switch (line.ToLower())
                {
                    case "t":
                    case "time":
                        Value = DateTimeDisplayFormat.TIMEONLY;
                        errorMessage = null;
                        return true;
                    case "d":
                    case "date":
                        Value = DateTimeDisplayFormat.DATEONLY;
                        errorMessage = null;
                        return true;
                    case "f":
                    case "full":
                        Value = DateTimeDisplayFormat.FULL;
                        errorMessage = null;
                        return true;
                    default:
                        errorMessage = "Error parsing \"" + Name + "\" argument. Only date/time/full or d/t/f values are expected.";
                        return false;
                }
            }

            public DateTimeDisplayFormatArgument(ArgumentAttribute attribute, OptionalAttribute optional) : base(attribute, optional) { }

        }

        /// <summary>
        /// Command that consumes a custom argument.
        /// </summary>
        [Command("showdatetime|sdt", "show current date and time")]
        class ShowDateTimeCommand : ICommand
        {
            [Argument(0, "format", "date time format: date/time/full or d/t/f", multisegmented: false)]
            [Optional(defaultValue: "full")]
            private readonly DateTimeDisplayFormatArgument DisplayFormat;

            public string Execute()
            {
                switch (DisplayFormat.Value)
                {
                    case DateTimeDisplayFormatArgument.DateTimeDisplayFormat.DATEONLY:
                        return DateTime.Now.ToShortDateString();
                    case DateTimeDisplayFormatArgument.DateTimeDisplayFormat.TIMEONLY:
                        return DateTime.Now.ToShortTimeString();
                    case DateTimeDisplayFormatArgument.DateTimeDisplayFormat.FULL:
                        return DateTime.Now.ToString();
                    default:
                        return "Internal error.";
                }
            }
        }



        public CommandSet()
        {
            CommandProcessor.AddGroup("", "textutils|tu", "some useful text utils")
                // Number of command and command group aliases is not limited.
                .AddGroup("tu", "metrics|metr|mt", "calculate various string metrics")
                .AddGroup("", "fr", "frequently used commands")

                // Previously created groups can be referred by any combination of valid aliases.
                .AddCommand("tu", new DisplaySampleTextCommand())
                .AddCommand("textutils", new RemoveCharacterCommand())
                .AddCommand("tu mt", new CountCharactersCommand())
                .AddCommand("textutils mt", new CountWordsCommand())
                .AddCommand("textutils", new RepeatWordCommand())
                .AddCommand("tu", new RepeatPhraseCommand())

                // Same commands can be added at multiple locations in a tree like command heirarchy.
                .AddCommand("fr", new DisplaySampleTextCommand())
                .AddCommand("fr", new CountCharactersCommand())
                .AddCommand("", new ShowDateTimeCommand())
                .AddCommand("", new HelpCommand(CommandProcessor));

            // "Safe" way of adding commands and groups.
            // In this case changing group name will not require modifying paths in which this group was mentioned.
            var calcGroup = CommandProcessor.AddGroup("calc", "do some calculus");
            calcGroup.AddCommand(new AddArrayCommand()).AddCommand(new DivideCommand());

        }
    }
}