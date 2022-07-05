namespace FriendlyCLP
{
    public class IntArgument : Argument
    {

        public int Value { get; private set; }

        public override bool Convert(string line, out string errorMessage)
        {
            if (int.TryParse(line, out var value))
            {
                Value = value;
                errorMessage = null;
                return true;
            }
            errorMessage = "Error parsing argument \"" + Name + "\". Integer value expected.";
            return false;
        }

        public IntArgument(ArgumentAttribute attribute) : base(attribute) { }

    }

    /// <summary>
    /// Integer argument with additional validation that rejects negative values.
    /// </summary>
    public class IntNonnegativeArgument : IntArgument
    {

        public override bool Validate(out string errorMessage)
        {
            if (Value < 0)
            {
                errorMessage = "Error validating argument \"" + Name + "\". Positive value expected.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        public IntNonnegativeArgument(ArgumentAttribute attribute) : base(attribute) { }
    }

    /// <summary>
    /// In order to make this argument work properly it should be declared as multisegmented.
    /// </summary>
    public class IntArrayArgument : Argument
    {

        public int[] Value { get; private set; }

        public override bool Convert(string line, out string errorMessage)
        {
            var elements = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Value = new int[elements.Length];

            for (int i = 0; i < elements.Length; i++)
            {
                if (!int.TryParse(elements[i], out Value[i]))
                {
                    errorMessage = "Error parsing element #" + (i + 1) + " of \"" + Name + "\" argument. Integer value expected.";
                    return false;
                }
            }
            errorMessage = null;
            return true;
        }

        public override void SetDefault()
        {
            Value = new int[0];
        }

        public IntArrayArgument(ArgumentAttribute attribute) : base(attribute)
        {
            if (!Multisegmented) throw new ArgumentException("Argument of type \"" + typeof(IntArrayArgument).Name + "\" should always be multisegment.");
        }

    }

    /// <summary>
    /// In order to allow spaces in a string it should be declared as multisegmented.
    /// </summary>
    public class StringArgument : Argument
    {

        public string Value { get; private set; }

        public override bool Convert(string line, out string errorMessage)
        {
            Value = line;
            errorMessage = null;
            return true;
        }

        public override void SetDefault() => Value = "";

        public StringArgument(ArgumentAttribute attribute) : base(attribute) { }

    }

    public class CharArgument : Argument
    {

        public char Value { get; private set; }

        public override bool Convert(string line, out string errorMessage)
        {
            if (line.Length == 1)
            {
                Value = line[0];
                errorMessage = null;
                return true;
            }
            errorMessage = "Error parsing argument \"" + Name + "\". Character expected.";
            return false;
        }

        public CharArgument(ArgumentAttribute attribute) : base(attribute) { }

    }

    public class DateTimeArgument : Argument
    {

        public DateTime Value { get; private set; }

        public override bool Convert(string line, out string errorMessage)
        {
            if (DateTime.TryParse(line, out var value))
            {
                Value = value;
                errorMessage = null;
                return true;
            }

            errorMessage = "Error parsing \"" + Name + "\" argument. Wrong datetime format.";
            return false;
        }

        public DateTimeArgument(ArgumentAttribute attribute) : base(attribute) { }

    }

    /// <summary>
    /// Base class for boolean arguments.
    /// </summary>
    public abstract class BoolArgument : Argument
    {

        private readonly HashSet<string> TrueSynonyms;
        private readonly HashSet<string> FalseSynonyms;

        private bool DefaultValue;
        public bool Value { get; private set; }

        public override bool Convert(string line, out string errorMessage)
        {
            var lcLine = line.ToLower();
            if (TrueSynonyms.Contains(lcLine)) { Value = true; }
            else
            {
                if (FalseSynonyms.Contains(lcLine)) { Value = false; }
                else
                {
                    errorMessage = $"Error parsing \"{Name}\" argument. Permissible values are: " + string.Join(", ", TrueSynonyms.ToArray()) + "; or: " + string.Join(", ", FalseSynonyms.ToArray()) + ".";
                    return false;
                }
            }
            errorMessage = null;
            return true;
        }

        public override void SetDefault() => Value = DefaultValue;

        /// <summary>
        /// Creates a boolean argument parameter.
        /// </summary>
        /// <param name="attribute">Parameter attribute.</param>
        /// <param name="trueSynonyms">All aliases of <c>true</c> in a given context.</param>
        /// <param name="falseSynonyms">All aliases of <c>false</c> in a given context.</param>
        public BoolArgument(ArgumentAttribute attribute, string[] trueSynonyms, string[] falseSynonyms, bool defaultValue) : base(attribute)
        {
            TrueSynonyms = new HashSet<string>(trueSynonyms);
            FalseSynonyms = new HashSet<string>(falseSynonyms);
            DefaultValue = defaultValue;
        }

    }

    /// <summary>
    /// Boolean yes/no (y/n) argument. 
    /// True is the default value.
    /// </summary>
    public class BoolYesNoArgument : BoolArgument
    {
        public BoolYesNoArgument(ArgumentAttribute attribute) : base(attribute, new string[] { "y", "yes" }, new string[] { "n", "no" }, true) { }
    }

    /// <summary>
    /// Boolean allowed/forbidden (a/f) argument. 
    /// True is the default value.
    /// </summary>
    public class BoolAllowedForbiddenArgument : BoolArgument
    {
        public BoolAllowedForbiddenArgument(ArgumentAttribute attribute) : base(attribute, new string[] { "a", "allowed" }, new string[] { "f", "forbidden" }, true) { }
    }
}
