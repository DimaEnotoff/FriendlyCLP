using System;
using System.Collections.Generic;
using System.Linq;
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
        internal readonly string Names;
        internal readonly string [] NameList;
        internal readonly string Description;
        private readonly ICommand Command;
        private readonly SortedList<int, Argument> Arguments;

        private string CachedHelpArticle;

        internal string HelpArticle => CachedHelpArticle ?? (CachedHelpArticle = CreateHelpArticle());

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

            Names = commandAttribute.Names;
            NameList = commandAttribute.NameList;
            Description = commandAttribute.Description;
            Arguments = new SortedList<int, Argument>();

            var fieldsInfo = memberInfo.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Argument specialArgument = null;

            foreach (var fieldInfo in fieldsInfo)
            {
                if (typeof(Argument).IsAssignableFrom(fieldInfo.FieldType))
                {
                    if (fieldInfo.GetValue(command) != null)
                        throw new ArgumentException("Argument \"" + fieldInfo.Name + "\" in command \"" + Names +
                            "\" should NOT be initialized.");
                    
                    if (!fieldInfo.IsPrivate)
                        throw new ArgumentException("Argument \"" + fieldInfo.Name + "\" in command \"" + Names +
                            "\" should be private.");
                    
                    var argumentAttribute = fieldInfo.GetCustomAttribute(typeof(ArgumentAttribute), false) as ArgumentAttribute;
                    var optionalAttribute = fieldInfo.GetCustomAttribute(typeof(OptionalAttribute), false) as OptionalAttribute;

                    if (argumentAttribute == null)
                        throw new ArgumentException("Argument \"" + fieldInfo.Name + "\" in command \"" + Names +
                            "\" should have \"" + typeof(ArgumentAttribute).Name + "\" attribute.");

                    var constructorInfo = fieldInfo.FieldType.GetConstructor(new[] { typeof(ArgumentAttribute), typeof(OptionalAttribute) });
                    if (constructorInfo == null)
                        throw new ArgumentException("Argument \"" + fieldInfo.Name + "\" in command \"" + Names +
                            "\" should have public constructor that takes \"" + typeof(ArgumentAttribute).Name + "\" and \""
                            + typeof(OptionalAttribute).Name + "\" parameter.");

                    var argument = (Argument)constructorInfo.Invoke(new object[] { argumentAttribute, optionalAttribute });
                    
                    fieldInfo.SetValue(command, argument);

                    if (argument.Optional || argument.Multisegmented)
                    {
                        if (specialArgument != null)
                            throw new ArgumentException("Command \"" + Names +
                                "\" has two special arguments: \"" + specialArgument.Name + "\" and \"" + argument.Name + "\".");
                        specialArgument = argument;
                    }

                    if (Arguments.TryGetValue(argumentAttribute.Position, out var collisionCause))
                        throw new ArgumentException("Argument \"" + fieldInfo.Name + "\" in command \"" + Names +
                            "\" has the same position as \"" + collisionCause.Name + "\" argument.");
                    Arguments.Add(argumentAttribute.Position, argument);
                }
            }

            if ((specialArgument != null) && (Arguments.Last().Value != specialArgument)) {
                throw new ArgumentException("Argument \"" + specialArgument.Name + "\" in command \"" + Names +
                    "\" is optional or multiline and should be the last!");
            }

            Arguments.TrimExcess();
            Command = command;
        }

        /// <summary>
        /// Tries to parse all arguments of the command.
        /// If parsing was successfull, command is executed.
        /// </summary>
        /// <param name="line">Part of raw input of a console user.</param>
        /// <returns>Command result on success, error message on parsing or validation failure.</returns>
        internal string ParseArgsAndExecute(string line)
        {
            string remainder = line;
            foreach (var argument in Arguments.Values)
                if (!argument.Parse(remainder, out remainder, out var errorMessage)) return errorMessage;
            
            if (!string.IsNullOrWhiteSpace(remainder))
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
        private string CreateHelpArticle()
        {
            var result = new StringBuilder();
            result.Append("Command: ").AppendLine(Names);
            result.Append("Description: ").AppendLine(Description);
            result.Append("Usage: ").Append(Names);

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
                {
                    result.Append(Indentation).Append(Indentation).Append(argument.Name).Append(": ").
                        Append(argument.Description);
                    if (argument.Optional) {
                        result.Append(" (optional");
                        if (argument.DefaultValue != null) {
                            result.Append(", default: \"").Append(argument.DefaultValue).Append('\"');
                        }
                        result.Append(')');
                    }
                    result.AppendLine();
                }
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
        private const string NamesSeparator = "|";

        public readonly string Names;
        public readonly string[] NameList;
        public readonly string Description;

        /// <summary>
        /// This annotation is required for all FriendlyCLP commands.
        /// </summary>
        /// <param name="names">Command names separated by | (vertical bar sign). They will be used to call this command from a command line.
        /// Each name should be alphanumeric lower case with no spaces.</param>
        /// <param name="description">Command description. To be used in a help article.</param>
        /// <exception cref="ArgumentException">Thrown if attribute params are invalid.</exception>
        public CommandAttribute(string names, string description)
        {
            NameList = names.Split(NamesSeparator, StringSplitOptions.RemoveEmptyEntries);

            if (NameList.Length == 0)
                throw new ArgumentException("Invalid command names" + (names.Length == 0 ? " (empty)" : ": \"" + names + "\"") + ".");

            foreach (var name in NameList)
            {
                foreach (var symbol in name)
                {
                    if (!char.IsLetterOrDigit(symbol) || !char.IsLower(symbol))
                        throw new ArgumentException("Invalid command name: \"" + name + "\".");
                }
            }

            Names = string.Join(NamesSeparator, NameList);

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("\"" + Names + "\" command description is invalid (empty).");

            Description = description;
        }
    }

}