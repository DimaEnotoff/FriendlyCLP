using System;

namespace FriendlyCLP
{
    /// <summary>
    /// Base class for all FriendlyCLP command arguments.
    /// </summary>
    public abstract class Argument
    {
        public readonly string Name;
        public readonly string Description;

        public readonly bool Optional;
        public readonly string DefaultValue;

        public readonly bool Multisegmented;

        public readonly int Position;

        /// <summary>
        /// Shows if argument was omitted by a console user. 
        /// Needs to be checked only if argument is optional.
        /// </summary>
        public bool IsOmitted { get; private set; }

        /// <summary>
        /// Creates an argument instance.
        /// Should be called by FriendlyCLP engine only.
        /// </summary>
        /// <param name="attribute">Attribute (annotation) containing required argument parameters.</param>
        /// <param name="optional">Attribute (annotation) only for optional arguments.</param>
        protected Argument(ArgumentAttribute attribute, OptionalAttribute optional)
        {
            Name = attribute.Name;
            Description = attribute.Description;
            Multisegmented = attribute.Multisegmented;
            Position = attribute.Position;
            if (optional != null)
            {
                Optional = true;
                DefaultValue = optional.DefaultValue;
            }
            else { 
                Optional = false;
            }
        }

        /// <summary>
        /// Takes next element from the line and tries to parse it.
        /// Should be called by FriendlyCLP engine only.
        /// </summary>
        /// <param name="line">Part of the raw input of a console user.</param>
        /// <param name="remainder"><c>Line</c> without a consumed element.</param>
        /// <param name="errorMessage">Failure reason. Shown to a console user if parsing fails.</param>
        /// <returns>True if parsing was successfull, false otherwise.</returns>
        internal bool Parse(string line, out string remainder, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(line)) {
                remainder = string.Empty;
                if (Optional)
                {
                    errorMessage = null;
                    IsOmitted = true;
                    if (DefaultValue != null)
                    {
                        return Convert(DefaultValue, out errorMessage) && Validate(out errorMessage);
                    }
                    return true;
                }
                errorMessage = "Argument \"" + Name + "\" is missing!";
                return false;
            }

            IsOmitted = false;

            if (Multisegmented)
            {
                remainder = string.Empty;
                return Convert(line, out errorMessage) && Validate(out errorMessage);
            }

            var lineParts = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            var rawArgument = lineParts[0];
            remainder = lineParts.Length == 2 ? lineParts[1] : string.Empty;

            return Convert(rawArgument, out errorMessage) && Validate(out errorMessage);
        }

        /// <summary>
        /// Converts a raw user input to a value of a particular type.
        /// Should be overwritten for any concrete argument type.
        /// Should be called by FriendlyCLP engine only.
        /// </summary>
        /// <param name="line">Raw input of a console user.</param>
        /// <param name="errorMessage">Failure reason. Shown to a console user if conversion fails.</param>
        /// <returns>True if the conversion was successful, false otherwise.</returns>
        public abstract bool Convert(string line, out string errorMessage);

        /// <summary>
        /// Validates an already converted value.
        /// Just a stub, always returns true by default, should be overridden if validation is needed.
        /// Should be called by FriendlyCLP engine only.
        /// </summary>
        /// <param name="errorMessage">Failure reason. Shown to a console user if validation fails.</param>
        /// <returns>True if the validation was successful, false otherwise.</returns>
        public virtual bool Validate(out string errorMessage)
        {
            errorMessage = null;
            return true;
        }

    }

    /// <summary>
    /// Attribute for annotating FriendlyCLP command arguments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class ArgumentAttribute : Attribute
    {
        public readonly string Name;
        public readonly string Description;
        public readonly bool Multisegmented;
        public readonly int Position;

        /// <summary>
        /// All FriendlyCLP command arguments should be annotated with this attribute.
        /// </summary>
        /// <param name="position">Argument position in an argument sequence.</param>
        /// <param name="name">Short and simple argument name. Should be alphanumeric with no spaces. To be used in a help article.</param>
        /// <param name="description">Argument description. To be used in a help article.</param>
        /// <param name="multisegmented">Specifies if argument can have spaces in it (can be true only for argument on the last position).</param>
        /// <exception cref="ArgumentException">Thrown if attribute params are invalid.</exception>
        public ArgumentAttribute(int position, string name, string description, bool multisegmented = false)
        {

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Invalid argument name (empty or containing whitespaces only).");

            foreach (var symbol in name)
            {
                if (!(char.IsLetterOrDigit(symbol) && char.IsLower(symbol)))
                    throw new ArgumentException("Invalid argument name: \"" + name + "\".");
            }

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("\"" + name + "\" argument description is invalid (empty).");

            if (position < 0)
                throw new ArgumentException("\"" + name + "\" argument position is invalid (negative).");

            Name = name;
            Description = description;
            Position = position;
            Multisegmented = multisegmented;

        }

    }

    /// <summary>
    /// Attribute for annotating optional FriendlyCLP command arguments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class OptionalAttribute : Attribute
    {
        public readonly string DefaultValue;

        /// <summary>
        /// This annotation indicates that argument is optional.
        /// Only arguments at the last position can be optional.
        /// </summary>
        /// <param name="defaultValue">If argument is omitted this string will be parsed and validated as if it was a user input.
        /// If default argument value is not specified or equals null no parsing and validation is done and
        /// argument value will be stale. Before using a value of optional argument with no defaultValue
        /// IsOmitted flag should be always checked.</param>
        public OptionalAttribute(string defaultValue = null)
        {
            DefaultValue = defaultValue;
        }
    }

}