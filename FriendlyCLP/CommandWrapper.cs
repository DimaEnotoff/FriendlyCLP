using System.Reflection;
using System.Text;

namespace FriendlyCLP
{
    /// <summary>
    /// Internal class of the FriendlyCLP engine that wraps user commands.
    /// </summary>
    internal sealed class CommandWrapper
    {
        private const string Indentation = "   ";
        internal readonly string Name;
        internal readonly string Description;
        private readonly ICommand Command;
        private readonly SortedList<int, Argument> Arguments;

        /// <summary>
        /// Creates a wrapper that facilitates parsing arguments, execution and displaying help of a wrapped command.
        /// </summary>
        /// <param name="command">Command to be wrapped.</param>
        /// <exception cref="ArgumentException">Thrown if a command or its arguments are not properly formatted or do not have a proper annotations.</exception>
        internal CommandWrapper(ICommand command)
        {
            var memberInfo = command.GetType();
            var commandAttribute = (CommandAttribute)memberInfo.GetCustomAttribute(typeof(CommandAttribute), false);
            if (commandAttribute == null)
                throw new ArgumentException("Command should have \"" + typeof(CommandAttribute).Name + "\" attribute.");

            Name = commandAttribute.Name;
            Description = commandAttribute.Description;
            Arguments = new SortedList<int, Argument>();

            var fieldsInfo = memberInfo.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Argument specialArgument = null;

            foreach (var fieldInfo in fieldsInfo)
            {
                if (typeof(Argument).IsAssignableFrom(fieldInfo.FieldType))
                {
                    if (fieldInfo.GetValue(command) != null)
                        throw new ArgumentException("Argument \"" + fieldInfo.Name + "\" in command \"" + Name +
                            "\" should NOT be initialized.");
                    
                    if (!fieldInfo.IsPrivate)
                        throw new ArgumentException("Argument \"" + fieldInfo.Name + "\" in command \"" + Name +
                            "\" should be private.");
                    
                    var ArgumentAttributes = fieldInfo.GetCustomAttribute(typeof(ArgumentAttribute), false) as ArgumentAttribute;

                    if (ArgumentAttributes == null)
                        throw new ArgumentException("Argument \"" + fieldInfo.Name + "\" in command \"" + Name +
                            "\" should have \"" + typeof(ArgumentAttribute).Name + "\" attribute.");

                    var constructorInfo = fieldInfo.FieldType.GetConstructor(new[] { typeof(ArgumentAttribute) });
                    if (constructorInfo == null)
                        throw new ArgumentException("Argument \"" + fieldInfo.Name + "\" in command \"" + Name +
                            "\" should have public constructor that takes \"" + typeof(ArgumentAttribute).Name +
                            "\" parameter.");

                    var argument = (Argument)constructorInfo.Invoke(new object[] { ArgumentAttributes });
                    
                    fieldInfo.SetValue(command, argument);

                    if (argument.Optional || argument.Multisegmented)
                    {
                        if (specialArgument != null)
                            throw new ArgumentException("Command \"" + Name +
                                "\" has two special arguments: \"" + specialArgument.Name + "\" and \"" + argument.Name + "\".");
                        specialArgument = argument;
                    }

                    if (Arguments.TryGetValue(ArgumentAttributes.Position, out var collisionCause))
                        throw new ArgumentException("Argument \"" + fieldInfo.Name + "\" in command \"" + Name +
                            "\" has the same position as \"" + collisionCause.Name + "\" argument.");
                    Arguments.Add(ArgumentAttributes.Position, argument);
                }
            }

            if ((specialArgument != null) && (Arguments.Last().Value != specialArgument)) {
                throw new ArgumentException("Argument \"" + specialArgument.Name + "\" in command \"" + Name +
                    "\" is optional or multiline and should be the last!");
            }

            Arguments.TrimExcess();
            Command = command;
        }

        /// <summary>
        /// Tries to parse all required arguments starting from a given index in the input string.
        /// If parsing was successfull, command is executed.
        /// </summary>
        /// <param name="line">Raw input of a console user.</param>
        /// <param name="index">Index where to start parsing.</param>
        /// <returns>Command result on success, error message on parsing or validation failure.</returns>
        internal string ParseArgsAndExecute(string line, int index)
        {
            foreach (var argument in Arguments.Values)
                if (!argument.Parse(line, ref index, out var errorMessage)) return errorMessage;
            if (!string.IsNullOrWhiteSpace(line[index..line.Length]))
                return "Too many arguments!";
            try {
                return Command.Execute();
            } catch {
                return "Internal error!";
            }
            
        }

        /// <summary>
        /// Constructs a help article.
        /// </summary>
        /// <returns>Help article as a string.</returns>
        internal string Help()
        {
            var result = new StringBuilder();
            result.Append("Command: ").AppendLine(Name);
            result.Append("Description: ").AppendLine(Description);
            result.Append("Usage: ").Append(Name);

            foreach (Argument argument in Arguments.Values)
            {
                result.Append(' ');
                if (argument.Optional) result.Append('[');
                result.Append(argument.Name);
                if (argument.Optional) result.Append(']');
            }

            if (Arguments.Count > 0)
            {
                result.AppendLine().Append(Indentation).AppendLine("Arguments: ");
                foreach (Argument argument in Arguments.Values)
                    result.Append(Indentation).Append(Indentation).Append(argument.Name).Append(": ").
                        Append(argument.Description).AppendLine(argument.Optional ? " (optional)" : "");
            }

            return result.ToString();
        }

    }

    /// <summary>
    /// Attribute for annotating FriendlyCLP commands.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public readonly string Name;
        public readonly string Description;

        /// <summary>
        /// This annotation is required for all FriendlyCLP commands.
        /// </summary>
        /// <param name="name">Command name (alphanumeric with no spaces) that will be used to call this command from a command line.</param>
        /// <param name="description">Command description. To be used in a help article.</param>
        /// <exception cref="ArgumentException">Thrown if attribute params are invalid.</exception>
        public CommandAttribute(string name, string description)
        {

            if (!StringCheck.alphanumericNonempty.IsMatch(name))
                throw new ArgumentException("Invalid command name: \"" + name + "\".");

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("\"" + name + "\" command description is invalid (empty).");

            Name = name;
            Description = description;
        }
    }

}