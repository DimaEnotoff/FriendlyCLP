using System;
using System.Collections.Generic;
using System.Linq;

namespace FriendlyCLP
{
    /// <summary>
    /// Argument template mostly useful for types for which TryParse function is defined.
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    public class SimpleArgument<T> : Argument
    {

        public delegate bool TryParseDelegate(string input, out T output);

        private TryParseDelegate TryParse;
        private string ErrorMessageDetails;

        public T Value { get; private set; }

        public override bool Convert(string line, out string errorMessage)
        {
            if (TryParse(line, out T value))
            {
                Value = value;
                errorMessage = null;
                return true;
            }
            errorMessage = "Error parsing argument \"" + Name + "\". " + ErrorMessageDetails;
            return false;
        }

        public SimpleArgument(TryParseDelegate tryParse, string errorMessageDetails, ArgumentAttribute attribute, OptionalAttribute optional) : base(attribute, optional)
        {
            TryParse = tryParse ?? throw new ArgumentNullException(nameof(tryParse));
            ErrorMessageDetails = errorMessageDetails ?? throw new ArgumentNullException(nameof(errorMessageDetails));
        }

    }

    public class IntArgument : SimpleArgument<int>
    {
        public IntArgument(ArgumentAttribute attribute, OptionalAttribute optional) :
            base(int.TryParse, "Integer value expected.", attribute, optional) { }
    }

    public class LongArgument : SimpleArgument<long>
    {
        public LongArgument(ArgumentAttribute attribute, OptionalAttribute optional) :
            base(long.TryParse, "Integer value expected.", attribute, optional) { }
    }

    public class FloatArgument : SimpleArgument<float>
    {
        public FloatArgument(ArgumentAttribute attribute, OptionalAttribute optional) :
            base(float.TryParse, "Real value expected.", attribute, optional)
        { }
    }

    public class DoubleArgument : SimpleArgument<double>
    {
        public DoubleArgument(ArgumentAttribute attribute, OptionalAttribute optional) :
            base(double.TryParse, "Real value expected.", attribute, optional)
        { }
    }

    public class DecimalArgument : SimpleArgument<decimal>
    {
        public DecimalArgument(ArgumentAttribute attribute, OptionalAttribute optional) :
            base(decimal.TryParse, "Real value expected.", attribute, optional)
        { }
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

        public IntNonnegativeArgument(ArgumentAttribute attribute, OptionalAttribute optional) : base(attribute, optional) { }
    }

    /// <summary>
    /// In order to make this argument work properly it should be declared as multisegmented.
    /// </summary>
    public class IntArrayArgument : Argument
    {
        private static readonly char[] ElementsSeparator = new[] { ' ' };
        public int[] Value { get; private set; }

        public override bool Convert(string line, out string errorMessage)
        {
            var elements = line.Split(ElementsSeparator, StringSplitOptions.RemoveEmptyEntries);
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

        public IntArrayArgument(ArgumentAttribute attribute, OptionalAttribute optional) : base(attribute, optional)
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

        public StringArgument(ArgumentAttribute attribute, OptionalAttribute optional) : base(attribute, optional) { }

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

        public CharArgument(ArgumentAttribute attribute, OptionalAttribute optional) : base(attribute, optional) { }

    }

    public class DateTimeArgument : SimpleArgument<DateTime>
    {
        public DateTimeArgument(ArgumentAttribute attribute, OptionalAttribute optional) :
            base(DateTime.TryParse, "Wrong datetime format.", attribute, optional)
        { }
    }


    /// <summary>
    /// Base class for boolean arguments.
    /// </summary>
    public abstract class BoolArgument : Argument
    {

        private readonly HashSet<string> TrueSynonyms;
        private readonly HashSet<string> FalseSynonyms;

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

        /// <summary>
        /// Creates a boolean argument parameter.
        /// </summary>
        /// <param name="attribute">Parameter attribute.</param>
        /// <param name="trueSynonyms">All aliases of <c>true</c> in a given context.</param>
        /// <param name="falseSynonyms">All aliases of <c>false</c> in a given context.</param>
        public BoolArgument(ArgumentAttribute attribute, OptionalAttribute optional, string[] trueSynonyms, string[] falseSynonyms) : base(attribute, optional)
        {
            TrueSynonyms = new HashSet<string>(trueSynonyms);
            FalseSynonyms = new HashSet<string>(falseSynonyms);
        }

    }

    /// <summary>
    /// Boolean true/false (t/f) argument. 
    /// </summary>
    public class BoolTrueFalseArgument : BoolArgument
    {
        public BoolTrueFalseArgument(ArgumentAttribute attribute, OptionalAttribute optional) : base(attribute, optional, new string[] { "t", "true" }, new string[] { "f", "false" }) { }
    }

    /// <summary>
    /// Boolean yes/no (y/n) argument. 
    /// </summary>
    public class BoolYesNoArgument : BoolArgument
    {
        public BoolYesNoArgument(ArgumentAttribute attribute, OptionalAttribute optional) : base(attribute, optional, new string[] { "y", "yes" }, new string[] { "n", "no" }) { }
    }

    /// <summary>
    /// Boolean allowed/forbidden (a/f) argument. 
    /// </summary>
    public class BoolAllowedForbiddenArgument : BoolArgument
    {
        public BoolAllowedForbiddenArgument(ArgumentAttribute attribute, OptionalAttribute optional) : base(attribute, optional,new string[] { "a", "allowed" }, new string[] { "f", "forbidden" }) { }
    }
}
