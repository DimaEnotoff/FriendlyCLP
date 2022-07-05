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
        public readonly bool Multisegmented;

        public readonly int Position;

        /// <summary>
        /// Shows if argument was ommited by a console user. Useful only for optional arguments.
        /// </summary>
        public bool IsOmmited { get; private set; }

        /// <summary>
        /// Creates an argument instance.
        /// Should be called by FriendlyCLP engine only.
        /// </summary>
        /// <param name="attribute">Attribute (annotation) containing required argument parameters.</param>
        protected Argument(ArgumentAttribute attribute)
        {
            Name = attribute.Name;
            Description = attribute.Description;
            Optional = attribute.Optional;
            Multisegmented = attribute.Multisegmented;
            Position = attribute.Position;
        }

        /// <summary>
        /// Tries to find boundaries and parse argument starting from a given index in the input string.
        /// Should be called by FriendlyCLP engine only.
        /// </summary>
        /// <param name="line">Raw input of a console user.</param>
        /// <param name="index">Index that separates parsed part of the line from unparsed.</param>
        /// <param name="errorMessage">Failure reason. Shown to a console user if parsing fails.</param>
        /// <returns>True if parsing was successfull, false otherwise.</returns>
        internal bool Parse(string line, ref int index, out string errorMessage)
        {
            while (index < line.Length && line[index] == ' ') index++;

            if (index == line.Length)
            {
                if (Optional)
                {
                    errorMessage = null;
                    IsOmmited = true;
                    SetDefault();
                    return true;
                }
                errorMessage = "Argument \"" + Name + "\" is missing!";
                return false;
            }

            IsOmmited = false;

            if (Multisegmented)
                return Convert(line[index..(index = line.Length)], out errorMessage) && Validate(out errorMessage);

            var delimeterIndex = line.IndexOf(" ", index);
            delimeterIndex = delimeterIndex == -1 ? line.Length : delimeterIndex;
            return Convert(line[index..(index = delimeterIndex)], out errorMessage) && Validate(out errorMessage);
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

        /// <summary>
        /// Sets a default value if argument was ommited.
        /// Just a stub, does nothing by default, should be overridden if needed.
        /// Should be called by FriendlyCLP engine only.
        /// </summary>
        public virtual void SetDefault() { }
    }

    /// <summary>
    /// Attribute for annotating FriendlyCLP command arguments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class ArgumentAttribute : Attribute
    {
        public readonly string Name;
        public readonly string Description;
        public readonly bool Optional;
        public readonly bool Multisegmented;
        public readonly int Position;

        /// <summary>
        /// This annotation is required for all FriendlyCLP command arguments.
        /// </summary>
        /// <param name="position">Argument position in an argument sequence.</param>
        /// <param name="name">Short and simple argument name. Should be alphanumeric with no spaces. To be used in a help article.</param>
        /// <param name="description">Argument description. To be used in a help article.</param>
        /// <param name="optional">Specifies if argument can be ommited (only for argument on the last position). </param>
        /// <param name="multisegmented">Specifies if argument can have spaces in it (only for argument on the last position).</param>
        /// <exception cref="ArgumentException">Thrown if attribute params are invalid.</exception>
        public ArgumentAttribute(int position, string name, string description, bool optional = false, bool multisegmented = false)
        {
            if (!StringCheck.alphanumericNonempty.IsMatch(name))
                throw new ArgumentException("Invalid argument name: \"" + name + "\".");

            if (position < 0)
                throw new ArgumentException("\"" + name + "\" argument position is invalid (negative).");

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("\"" + name + "\" argument description is invalid (empty).");

            Name = name;
            Description = description;
            Position = position;
            Optional = optional;
            Multisegmented = multisegmented;

        }

    }

}