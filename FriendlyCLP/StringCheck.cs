using System.Text.RegularExpressions;

namespace FriendlyCLP
{
    internal static class StringCheck
    {
        public static Regex alphanumericNonempty = new Regex("^[a-zA-Z0-9]+$");
    }

}