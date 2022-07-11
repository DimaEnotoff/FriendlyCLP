namespace FriendlyCLP
{
    /// <summary>
    /// Friendly command line processor class that represents a distinct command set.
    /// </summary>
    public sealed class FCLP
    {
        private readonly CommandGroup RootCommandGroup;

        public string Description => RootCommandGroup.Name;

        /// <summary>
        /// Creates an instance of Friendly command line processor.
        /// </summary>
        /// <param name="description">Description. To be used in a help article.</param>
        public FCLP(string description)
        {
            RootCommandGroup = new CommandGroup(null, description);
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
            var index = 0;
            switch (RootCommandGroup.Search(line, ref index, out var group, out var command))
            {
                case CommandGroup.SearchResult.CommandFound:
                    return command.ParseArgsAndExecute(line, index);
                case CommandGroup.SearchResult.GroupFound:
                    return "Please specify a command within a \"" +group.Name+ "\" group!";
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
            var index = 0;
            var result = RootCommandGroup.Search(path, ref index, out var group, out var command);

            if (result == CommandGroup.SearchResult.GroupFound)
            {
                helpArticle = string.Join(Environment.NewLine, group.getTree());
                return true;
            }

            if (result == CommandGroup.SearchResult.CommandFound)
            {
                while (index < path.Length && path[index] == ' ') index++;
                if (index == path.Length)
                {
                    helpArticle = command.Help();
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
        /// <param name="command">Command.</param>
        /// <exception cref="ArgumentException">Thrown if path is invalid or an element with such name already exists at a given path.</exception>
        public FCLP AddCommand(string path, ICommand command) { 
            RootCommandGroup.AddCommand(path, command);
            return this;
        }

        /// <summary>
        /// Creates a new command group and adds it to the current command group at a given path.
        /// </summary>
        /// <param name="path">Path where to create a command group.</param>
        /// <param name="name">Command group name (alphanumeric with no spaces) that will be used to refer this command group in a command line.</param>
        /// <param name="description">Command group description. To be used in a help article.</param>
        /// <exception cref="ArgumentException">Thrown if path is invalid or an element with such name already exists at a given path.</exception>
        public FCLP AddGroup(string path, string name, string description) {
            RootCommandGroup.AddGroup(path, name, description);
            return this;
        }
    }

}