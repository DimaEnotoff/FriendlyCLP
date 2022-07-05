namespace FriendlyCLP
{
    /// <summary>
    /// Command group is used to organize commands in a tree like structure.
    /// Command group can contain commands and other command groups.
    /// </summary>
    public class CommandGroup
    {
        private readonly string Name;
        private readonly string Description;
        private readonly Dictionary<string, CommandGroup> Groups = new Dictionary<string, CommandGroup>();
        private readonly Dictionary<string, CommandWrapper> Commands = new Dictionary<string, CommandWrapper>();

        /// <summary>
        /// Creates a top level command group that contains all other command groups and commands.
        /// </summary>
        /// <param name="description">Command group description. To be used in a help article.</param>
        public CommandGroup(string description)
        {
            Name = null;
            Description = description;
        }

        /// <summary>
        /// Creates a new command group.
        /// </summary>
        /// <param name="name">Command group name (alphanumeric with no spaces) that will be used to refer this command group in a command line.</param>
        /// <param name="description">Command group description. To be used in a help article.</param>
        private CommandGroup(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Creates a new command group and adds it to the current command group.
        /// </summary>
        /// <param name="name">Command group name (alphanumeric with no spaces) that will be used to refer this command group in a command line.</param>
        /// <param name="description">Command group description. To be used in a help article.</param>
        /// <exception cref="ArgumentException">Thrown if group already contains other element with the same name.</exception>
        private void AddGroup(string name, string description) {
            if (Commands.ContainsKey(name) || Groups.ContainsKey(name))
                throw new ArgumentException("Can not add group. Child element named \"" + name + "\" already exists in \"" + Name + "\" group.");
            Groups.Add(name, new CommandGroup(name, description));
        }


        /// <summary>
        /// Creates a new command group and adds it to the current command group at a given path.
        /// </summary>
        /// <param name="path">Path where to create a command group.</param>
        /// <param name="name">Command group name (alphanumeric with no spaces) that will be used to refer this command group in a command line.</param>
        /// <param name="description">Command group description. To be used in a help article.</param>
        /// <exception cref="ArgumentException">Thrown if path is invalid or an element with such name already exists at a given path.</exception>
        /// <exception cref="Exception">Internal exception.</exception>
        public void AddGroup(string path, string name, string description)
        {
            var index = 0;
            switch (Search(path, ref index, out var group, out _))
            {
                case SearchResult.GroupFound:
                    group.AddGroup(name, description);
                    break;
                case SearchResult.CommandFound:
                    throw new ArgumentException("Can not add group \"" + name + "\" at a given path \"" + path + "\". " +
                        "Path points to an existing command, but should point to an existing group.");
                case SearchResult.NothingFound:
                    throw new ArgumentException("Can not add group \"" + name + "\" at a given path \"" + path + "\". " +
                        "Path is invalid.");
                default:
                    throw new Exception("Internal error. Can not handle this type of search result.");
            }
        }

        /// <summary>
        /// Adds wrapped command to the current command group.
        /// </summary>
        /// <param name="wrappedCommand">Wrapped command.</param>
        /// <exception cref="ArgumentException">Thrown if group already contains other element with the same name.</exception>
        private void AddCommand(CommandWrapper wrappedCommand) {
            if (Commands.ContainsKey(wrappedCommand.Name) || Groups.ContainsKey(wrappedCommand.Name))
                throw new ArgumentException("Can not add command. Child element named \"" + wrappedCommand.Name + "\" already exists in \"" + Name + "\" group.");
            Commands.Add(wrappedCommand.Name, wrappedCommand);
        }

        /// <summary>
        /// Adds a command at a given path.
        /// </summary>
        /// <param name="path">Path where to add a command.</param>
        /// <param name="command">Command.</param>
        /// <exception cref="ArgumentException">Thrown if path is invalid or an element with such name already exists at a given path.</exception>
        /// <exception cref="Exception">Internal exception.</exception>
        public void AddCommand(string path, ICommand command)
        {
            var index = 0;
            var wrappedCommand = new CommandWrapper(command);
            switch (Search(path, ref index, out var group, out _))
            {
                case SearchResult.GroupFound: 
                    group.AddCommand(wrappedCommand);
                    break;
                case SearchResult.CommandFound:
                    throw new ArgumentException("Can not add command \"" + wrappedCommand.Name + "\" at a given path \"" + path + "\". " +
                        "Path points to an existing command, but should point to an existing group.");
                case SearchResult.NothingFound:
                    throw new ArgumentException("Can not add command \"" + wrappedCommand.Name + "\" at a given path \"" + path + "\". " +
                        "Path is invalid.");
                default:
                    throw new Exception("Internal error. Can not handle this type of search result.");
            }
        }

        private enum SearchResult { GroupFound, CommandFound, NothingFound }

        /// <summary>
        /// Parses a command line and searches for mentioned elements.
        /// </summary>
        /// <param name="line">Raw input of a console user.</param>
        /// <param name="index">Index where to start parsing.</param>
        /// <param name="group">Returns command group if path points at a command group.</param>
        /// <param name="command">Returns command if path poins at a command.</param>
        /// <returns>If path points to existing element <c>GroupFound</c> or <c>CommandFound</c> is returned.
        /// If path is invalid <c>NothingFound</c> is returned.</returns>
        private SearchResult Search(string line, ref int index, out CommandGroup group, out CommandWrapper command)
        {
            while (index < line.Length && line[index] == ' ') index++;

            if (index == line.Length)
            {
                group = this;
                command = null;
                return SearchResult.GroupFound;
            }

            var delimeterIndex = line.IndexOf(" ", index);
            delimeterIndex = delimeterIndex == -1 ? line.Length : delimeterIndex;
            var nextElementName = line[index..(index = delimeterIndex)];

            if (Groups.TryGetValue(nextElementName, out var childGroup))
                return childGroup.Search(line, ref index, out group, out command);

            if (Commands.TryGetValue(nextElementName, out command))
            {
                group = null;
                return SearchResult.CommandFound;
            }

            group = null;
            command = null;
            return SearchResult.NothingFound;
        }

        /// <summary>
        /// Processes a line from a console user.
        /// </summary>
        /// <param name="line">Raw input of a console user.</param>
        /// <returns>Command result on success, error message on parsing or validation failure.</returns>
        /// <exception cref="Exception">Internal exception.</exception>
        public string ProcessLine(string line)
        {
            var index = 0;
            switch (Search(line, ref index, out _, out var command))
            {
                case SearchResult.CommandFound:
                    return command.ParseArgsAndExecute(line, index);
                case SearchResult.GroupFound:
                    return "Please specify a command within a group!";
                case SearchResult.NothingFound:
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
        public bool getHelp(string path, out string helpArticle)
        {
            var index = 0;
            switch (Search(path, ref index, out var group, out var command))
            {
                case SearchResult.GroupFound:
                    helpArticle = string.Join(Environment.NewLine, group.getTree());
                    return true;
                case SearchResult.CommandFound:
                    helpArticle = command.Help();
                    return true;
                case SearchResult.NothingFound:
                    helpArticle = null;
                    return false;
                default:
                    throw new Exception("Internal error. Can not handle this type of search result.");
            }
        }

        /// <summary>
        /// Gets pseudographics representation of a command tree.
        /// </summary>
        /// <returns>Command tree as a multiline text.</returns>
        public List<string> getTree()
        {
            List<string> result = new List<string> { Name == null ? Description : "<" + Name + ": " + Description + ">" };
            var groupCount = 0;
            foreach (var group in Groups.Values)
            {
                var lastItem = ++groupCount == Groups.Count && Commands.Count == 0;
                var childGroupTree = group.getTree();

                for (int i = 0; i < childGroupTree.Count; i++)
                {
                    var prefix = i == 0 ? lastItem ? "└──" : "├──" : lastItem ? "   " : "│  ";
                    result.Add(prefix + childGroupTree[i]);
                }
            }
            var commandCount = 0;
            foreach (var command in Commands.Values)
            {
                var lastItem = ++commandCount == Commands.Count;
                var prefix = lastItem ? "└──" : "├──";
                result.Add(prefix + "\"" + command.Name + "\" - " + command.Description);
            }
            return result;
        }

    }

}