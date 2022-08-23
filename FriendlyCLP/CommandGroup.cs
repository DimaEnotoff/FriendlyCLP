namespace FriendlyCLP
{

    /// <summary>
    /// Interface that alowes to add nested commands and command groups to this command group.
    /// </summary>
    public interface ICommandGroup {

        /// <summary>
        /// Creates a new command group and adds it to the current command group.
        /// Command groups form tree like structure.
        /// </summary>
        /// <param name="names">Command group names separated by | (vertical bar sign). They will be used to refer this command group from a command line.
        /// Each name should be alphanumeric lower case with no spaces.</param>
        /// <param name="description">Command group description. To be used in a help article.</param>
        /// <returns>Newly created command group.</returns>
        /// <exception cref="ArgumentException">Thrown if group already contains other element with the same name.</exception>
        ICommandGroup AddGroup(string names, string description);

        /// <summary>
        /// Adds command to the current command group.
        /// </summary>
        /// <param name="command">Command that implements <p>ICommand</p> interface.</param>
        /// <returns>Current command group, to facilitate method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if group already contains other element with the same name.</exception>
        public ICommandGroup AddCommand(ICommand command);
    }

    /// <summary>
    /// Internal class of the FriendlyCLP engine.
    /// Command group is used to organize commands in a tree like structure.
    /// Command group can contain commands and other command groups.
    /// </summary>
    internal class CommandGroup: ICommandGroup
    {
        private const string NamesSeparator = "|";
        internal readonly string Names;
        internal readonly string[] NameList;
        internal readonly string Description;
        private readonly Dictionary<string, CommandGroup> Groups = new Dictionary<string, CommandGroup>();
        private readonly Dictionary<string, CommandWrapper> Commands = new Dictionary<string, CommandWrapper>();

        private List<string> CachedCommandTree;

        /// <summary>
        /// Pseudographics representation of a command tree contained in this command group.
        /// </summary>
        internal IReadOnlyList<string> CommandTree => CachedCommandTree ?? (CachedCommandTree = CreateCommandTree());

        /// <summary>
        /// Creates a new command group.
        /// </summary>
        /// <param name="names">Command group names separated by | (vertical bar sign). They will be used to refer this command group from a command line.
        /// Each name should be alphanumeric lower case with no spaces.</param>
        /// <param name="description">Command group description. To be used in a help article.</param>
        internal CommandGroup(string names, string description)
        {
            NameList = names.Split(NamesSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (NameList.Length == 0)
                throw new ArgumentException("Invalid command group names" + (names.Length == 0 ? " (empty)" : ": \"" + names + "\"") + ".");

            foreach (var name in NameList)
            {
                foreach (var symbol in name)
                {
                    if (!(char.IsLetterOrDigit(symbol) && char.IsLower(symbol)))
                        throw new ArgumentException("Invalid command group name: \"" + name + "\".");
                }
            }

            Names = string.Join(NamesSeparator, NameList);

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("\"" + Names + "\" command description is invalid (empty).");

            Description = description;
        }

        /// <summary>
        /// Creates a root command group.
        /// Root group is different from regular one by the absence of a name.
        /// </summary>
        /// <param name="description">Root command group description. To be used in a help article.</param>
        internal CommandGroup(string description)
        {
            NameList = null;
            Names = null;

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Root command group description is invalid (empty).");

            Description = description;
        }

        public ICommandGroup AddGroup(string names, string description) {
            var newGroup = new CommandGroup(names, description);
            foreach (var name in newGroup.NameList) {
                if (Commands.ContainsKey(name) || Groups.ContainsKey(name))
                    throw new ArgumentException("Can not add group. Child element named \"" + name + "\" already exists in \"" + Names + "\" group.");
                Groups.Add(name, newGroup);
            }
            return newGroup;
        }

        /// <summary>
        /// Creates a new command group and adds it to the current command group at a given path.
        /// </summary>
        /// <param name="path">Path where to create a command group.</param>
        /// <param name="names">Command group names separated by | (vertical bar sign). They will be used to refer this command group from a command line.
        /// Each name should be alphanumeric lower case with no spaces.</param>        
        /// <param name="description">Command group description. To be used in a help article.</param>
        /// <exception cref="ArgumentException">Thrown if path is invalid or an element with such name already exists at a given path.</exception>
        internal void AddGroup(string path, string names, string description)
        {
            switch (Search(path, out _, out var group, out _))
            {
                case SearchResult.GroupFound:
                    group.AddGroup(names, description);
                    break;
                case SearchResult.CommandFound:
                    throw new ArgumentException("Can not add group \"" + names + "\" at a given path \"" + path + "\". " +
                        "Path points to an existing command, but should point to an existing group.");
                case SearchResult.NothingFound:
                    throw new ArgumentException("Can not add group \"" + names + "\" at a given path \"" + path + "\". " +
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
        internal void AddCommand(CommandWrapper wrappedCommand) {
            foreach (var name in wrappedCommand.NameList) {
                if (Commands.ContainsKey(name) || Groups.ContainsKey(name))
                    throw new ArgumentException("Can not add command. One of child element names \"" + name + "\" already exists in \"" + Names + "\" group.");
                Commands.Add(name, wrappedCommand);
            }
        }

        public ICommandGroup AddCommand(ICommand command)
        {
            AddCommand(new CommandWrapper(command));
            return this;
        }

        /// <summary>
        /// Wraps command and adds it at a given path.
        /// </summary>
        /// <param name="path">Path where to add a command.</param>
        /// <param name="command">Command that implements <p>ICommand</p> interface.</param>
        /// <exception cref="ArgumentException">Thrown if path is invalid or an element with such name already exists at a given path.</exception>
        internal void AddCommand(string path, ICommand command)
        {
            var wrappedCommand = new CommandWrapper(command);
            switch (Search(path, out _, out var group, out _))
            {
                case SearchResult.GroupFound: 
                    group.AddCommand(wrappedCommand);
                    break;
                case SearchResult.CommandFound:
                    throw new ArgumentException("Can not add command \"" + wrappedCommand.Names + "\" at a given path \"" + path + "\". " +
                        "Path points to an existing command, but should point to an existing group.");
                case SearchResult.NothingFound:
                    throw new ArgumentException("Can not add command \"" + wrappedCommand.Names + "\" at a given path \"" + path + "\". " +
                        "Path is invalid.");
                default:
                    throw new Exception("Internal error. Can not handle this type of search result.");
            }
        }

        internal protected enum SearchResult { GroupFound, CommandFound, NothingFound }

        /// <summary>
        /// Parses a command line and searches for mentioned elements.
        /// </summary>
        /// <param name="line">Part of the raw input of a console user.</param>
        /// <param name="remainder"><c>Line</c> without consumed element.</param>
        /// <param name="group">Returns command group if path points at a command group.</param>
        /// <param name="command">Returns command if path poins at a command.</param>
        /// <returns>If path points to existing element <c>GroupFound</c> or <c>CommandFound</c> is returned.
        /// If path is invalid <c>NothingFound</c> is returned.</returns>
        internal protected SearchResult Search(string line, out string remainder, out CommandGroup group, out CommandWrapper command)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                group = this;
                command = null;
                remainder = string.Empty;
                return SearchResult.GroupFound;
            }

            var lineParts = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            var nextElementName = lineParts[0].ToLowerInvariant();
            remainder = lineParts.Length == 2 ? lineParts[1] : string.Empty;

            if (Groups.TryGetValue(nextElementName, out var childGroup))
                return childGroup.Search(remainder, out remainder, out group, out command);

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
        /// Creates pseudographics representation of a command tree.
        /// </summary>
        /// <returns>Command tree as a multiline text.</returns>
        private List<string> CreateCommandTree()
        {
            List<string> result = new List<string> { Names == null ? Description : "<" + Names + ": " + Description + ">" };
            
            var uniqueGroups = new HashSet<CommandGroup>(Groups.Values);
            var groupCount = uniqueGroups.Count;
            foreach (var group in Groups.Values)
            {
                if (uniqueGroups.Contains(group)) {
                    var lastItem = --groupCount == 0 && Commands.Count == 0;
                    var childGroupTree = group.CreateCommandTree();

                    for (int i = 0; i < childGroupTree.Count; i++)
                    {
                        var prefix = i == 0 ? lastItem ? "└──" : "├──" : lastItem ? "   " : "│  ";
                        result.Add(prefix + childGroupTree[i]);
                    }
                    uniqueGroups.Remove(group);
                }
            }

            var uniqueCommands = new HashSet<CommandWrapper>(Commands.Values);
            var commandCount = uniqueCommands.Count;

            foreach (var command in Commands.Values)
            {
                if (uniqueCommands.Contains(command)) {
                    var lastItem = --commandCount == 0;
                    var prefix = lastItem ? "└──" : "├──";
                    result.Add(prefix + "\"" + command.Names + "\" - " + command.Description);
                    uniqueCommands.Remove(command);
                }
            }

            return result;
        }

    }

}