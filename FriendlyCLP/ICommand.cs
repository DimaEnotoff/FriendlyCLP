namespace FriendlyCLP
{
    /// <summary>
    /// Interface that all FriendlyCLP commands should implement.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// This method is called by FriendlyCLP engine when a console user calls this particular command
        /// and all its arguments are successfully parsed and verified.
        /// </summary>
        /// <returns>Command execution result that is returned to a console user.</returns>
        string Execute();
    }
}