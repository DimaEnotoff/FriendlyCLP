namespace FriendlyCLP
{
    /// <summary>
    /// Friendly command line processor class that represents a distinct command set.
    /// </summary>
    public sealed class CommandProcessor : ICommandGroup
    {
        private readonly CommandGroup RootCommandGroup;

        public string Description => RootCommandGroup.Description;

        /// <summary>
        /// Creates an instance of Friendly command line processor.
        /// </summary>
        /// <param name="description">Description. To be used in a help article.</param>
        public CommandProcessor(string description)
        {
            RootCommandGroup = new CommandGroup(description);
        }

        /// <summary>
        /// Processes a line from a console user.
        /// </summary>
        /// <param name="line">Raw input of a console user.</param>
        /// <returns>Command result on success, error message on parsing or validation failure.</returns>
        /// <exception cref="Exception">Internal exception.</exception>
        public string ProcessLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return "Please enter a command!";
            switch (RootCommandGroup.Search(line, out var remainder, out var group, out var command))
            {
                case CommandGroup.SearchResult.CommandFound:
                    return command.ParseArgsAndExecute(remainder);
                case CommandGroup.SearchResult.GroupFound:
                    return "Please specify a command within a \"" +group.Names+ "\" group!";
                case CommandGroup.SearchResult.NothingFound:
                    return "Command not found!";
                default:
                    throw new Exception("Internal error. Can not handle this type of search result.");
            }
        }

        /// <summary>
        /// Gets a help article for an element pointed by a path.
        /// </summary>
        /// <param name="path">Path to an element.</param>
        /// <param name="helpArticle">Help article is returned if path is valid.</param>
        /// <returns>True if path is valid, false otherwise.</returns>
        /// <exception cref="Exception">Internal exception.</exception>
        public bool GetHelp(string path, out string helpArticle)
        {
            var result = RootCommandGroup.Search(path, out var remainder, out var group, out var command);

            if (result == CommandGroup.SearchResult.GroupFound)
            {
                helpArticle = string.Join(Environment.NewLine, group.CommandTree);
                return true;
            }

            if (result == CommandGroup.SearchResult.CommandFound)
            {
                if (string.IsNullOrWhiteSpace(remainder))
                {
                    helpArticle = command.HelpArticle;
                    return true;
                }
            }

            helpArticle = null;
            return false;
        }

        /// <summary>
        /// Adds a command at a given path.
        /// </summary>
        /// <param name="path">Path where to add a command.</param>
        /// <param name="command">Command instance. Object implementing <p>ICommand</p> interface.</param>
        /// <exception cref="ArgumentException">Thrown if path is invalid or an element with such name already exists at a given path.</exception>
        public CommandProcessor AddCommand(string path, ICommand command) { 
            RootCommandGroup.AddCommand(path, command);
            return this;
        }

        /// <summary>
        /// Creates a new command group and adds it to the current command group at a given path.
        /// </summary>
        /// <param name="path">Path where to create a command group.</param>
        /// <param name="names">Command group names separated by | (vertical bar sign). They will be used to refer this command group from a command line.
        /// Each name should be alphanumeric lower case with no spaces.</param>
        /// <param name="description">Command group description. To be used in a help article.</param>
        /// <exception cref="ArgumentException">Thrown if path is invalid or an element with such name already exists at a given path.</exception>
        public CommandProcessor AddGroup(string path, string names, string description) {
            RootCommandGroup.AddGroup(path, names, description);
            return this;
        }

        public ICommandGroup AddGroup(string names, string description) => RootCommandGroup.AddGroup(names, description);
        public ICommandGroup AddCommand(ICommand command) => RootCommandGroup.AddCommand(command);
    }

}