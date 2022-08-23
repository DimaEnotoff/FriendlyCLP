# Friendly Command Line Processor

**Friendly CLP** is a library that facilitates rapid development of command line interfaces with focus on convenience of users who access them from a mobile phone. The library was used in production to build debugging/engineering menus for self service kiosks and other automation systems.

## Mobile friendly

Slash, colon, dot and other special characters that are commonly used in most CLIs are cumbersome to type on a mobile phone keyboard. To address this issue **Friendly CLP** uses spaces to separate command groups, commands and arguments from each other. To make it even more convenient for mobile phone users commands are not case sensitive.

## Other features

- Easy to add commands and organize them in tree like structure
- Easy to add command arguments
  - Commonly used argument data types are readily available
  - Creating a custom data type arguments is straightforward
- All required parsing is done automatically, payload code is called only if all arguments are valid
- Meaningful error messages are given if parsing fails
- Help articles are generated automatically

## Quick start

### 1. Importing the library

```C#
using FriendlyCLP;
```

### 2. Creating a command

**Friendly CLP** command is a class that implements `ICommand` interface and has `Command` annotation.

`ICommand` interface has an `Execute` method that must contain a command payload and must return a result as a string.

`Command` annotation has following arguments:
- `names` is a group of names (aliases) of a command that will be used to call it from a command line. Each name should be concise and contain no spaces. Number of names is not limited, but it is reasonable to have a full name and short abbreviation. Vertical bar sign `|` is used to separate one name from another.  
- `description` is a brief command description that will be shown in a help article.

```C#
[Command("displaysampletext|dst", "display sample text")]
private class DisplaySampleTextCommand : ICommand
{
    public string Execute() => "Lorem ipsum dolor sit amet ...";
}
```


### 3. Creating a Command Processor instance

`CommandProcessor` is a class that contains commands in a tree-like structure, parses user input, calls a corresponding command if input is valid or displays error message if not. Multiple `CommandProcessor` instances, containing different command trees, can be used in the same application. They can be used, for example, to process commands issued by users with different authorization levels.

```C#
var CommandProcessor = new CommandProcessor("My CLI");
```

### 4. Adding the command to the CommandProcessor 

In order to make a command invokable from a particular `CommandProcessor` instance it must be added to it.

```C#
CommandProcessor.AddCommand(new DisplaySampleTextCommand());
```

### 5. Processing user input

`ProcessLine` method of a `CommandProcessor` instance takes user input, parses it and calls a corresponding command if input is valid or returns a meaningful error message if not.

```C#
while (true) {
    var request = Console.ReadLine();
    var response = MyCommandSet.CommandProcessor.ProcessLine(request);
    Console.WriteLine(response);
}
```

**This example makes the simplest working command line interface with just one command with no arguments.**

Usage example:
```
dst
Lorem ipsum dolor sit amet ...
dst abc
Too many arguments!
xyz
Command not found!
```

## Going further

### Adding arguments of common types

Arguments are just annotated properties of a command class. Properties must be private and must not be instantiated.

The library has some premade argument property types that represent commonly used data types. An argument property type must be one of those in the list.
- IntArgument
- LongArgument
- FloatArgument
- DoubleArgument
- DecimalArgument
- IntNonnegativeArgument
- IntArrayArgument
- StringArgument
- CharArgument
- DateTimeArgument
- BoolTrueFalseArgument _This type recognises true as "true" or "t" and false as "false" or "f"._
- BoolYesNoArgument _This type recognises true as "yes" or "y" and false as "no" or "n"._
- BoolAllowedForbiddenArgument _This type recognises true as "allowed" or "a" and false as "forbidden" or "f"._

All of these classes have a property called `Value`, through which the actual value of parsed user input is exposed.

Arguments must be annotated by an `Argument` annotation that has following arguments:
- `position` - a sort key for all arguments within a command.
- `name` - an argument name that will be shown in a help article.
- `description` - a brief argument description that will be shown in a help article.
- `multisegmented` will be explained a little later, should be _false_ for now.

Simple example of using standard type arguments in a command:
```C#
[Command("removechar|rc", "remove character in a word")]
private class RemoveCharacterCommand : ICommand
{
    [Argument(0, "word", "word to remove character from", multisegmented: false)]
    private readonly StringArgument Word;
    [Argument(1, "char", "character to be removed", multisegmented: false)]
    private readonly CharArgument Char;
    public string Execute()
    {
        return Word.Value.Replace(Char.Value.ToString(), "");
    }
}
```

Usage example:
```
rc abracadabra a
brcdbr
```

If wrong input is provided, the library will automatically generate appropriate error messages:
```
rc
Argument "word" is missing!
rc abracadabra
Argument "char" is missing!
rc abracadabra ab
Error parsing argument "char". Character expected.
```

### Special arguments

Last argument of a command (that has the highest position number) can be **special**. There are two types of special arguments: _optional_ and _multisegmented_. These types are not mutually exclusive. Same argument can be multi segmented and optional at the same time.

#### Optional

Optional argument is an argument that can be omitted. In order to make the argument optional it must be annotated with `Optional` annotation.
If the optional argument is omitted, its `IsOmitted` property is set to true, and `Value` property is left stale.
 `IsOmitted` flag should be always checked for optional arguments to prevent a command from consuming stale `Value` property value.

Simple example that shows usage of an optional argument:
```C#
[Command("removechar|rc", "remove character in a word")]
private class RemoveCharacterCommand : ICommand
{
    [Argument(0, "word", "word to remove character from", multisegmented: false)]
    private readonly StringArgument Word;
    [Argument(1, "char", "character to be removed", multisegmented: false)]
    [Optional]
    private readonly CharArgument Char;
    public string Execute()
    {
        if (Char.IsOmitted) //Checking if user omitted this argument
            return Word.Value;
        return Word.Value.Replace(Char.Value.ToString(), "");
    }
}
```

Usage example:
```
rc abracadabra a
brcdbr
rc abracadabra
abracadabra
```

##### Default

It is possible to set a default value of an optional argument by setting `defaultValue` argument of `Optional` annotation. `defaultValue` is a string that will be used instead of user input when the argument is omitted.

```C#
[Argument(0, "country", "country of residence", multisegmented: false)]
[Optional(defaultValue: "Canada")]
private readonly StringArgument Country;
```

`defaultValue` is not checked for validity and in case an invalid value is provided, omitting the argument will cause parsing or validation error.

Example of a wrong default value leading to an error on argument omission:
```C#
[Command("repw", "repeat word X number of times")]
class RepeatWordCommand : ICommand
{
    [Argument(0, "word", "word to repeat", multisegmented: false)]
    private readonly StringArgument Word;

    [Argument(1, "times", "number of times to repeat", multisegmented: false)]
    [Optional (defaultValue: "two times")] // Invalid default value
    private readonly IntArgument Times;

    public string Execute()
    {
        return string.Concat(Enumerable.Repeat(Word.Value + ' ', Times.Value));
    }
}
```

Execution example:
```
repw Wow 5
Wow Wow Wow Wow Wow
repw Wow
Error parsing argument "times". Integer value expected.
```

#### Multi segmented

**Friendly CLP** uses spaces as separators between command groups, commands and arguments. If an argument is expected to have spaces inside, it must be marked as multi segmented and must be at the last position in the command. To mark an argument as multi segmented `multisegmented` argument of `Argument` annotation must be set to _true_.

```C#
[Command("repp", "repeat phrase X number of times")]
class RepeatPhraseCommand : ICommand
{
    [Argument(0, "times", "number of times to repeat", multisegmented: false)]
    private readonly IntArgument Times;

    [Argument(1, "phrase", "phrase to repeat", multisegmented: true)]
    private readonly StringArgument Phrase;

    public string Execute()
    {
        return string.Concat(Enumerable.Repeat(Phrase.Value + ' ', Times.Value));
    }
}
```

Usage example:
```
tu repp 3 the bird is the word
the bird is the word the bird is the word the bird is the word
```

### Adding custom type arguments

In case standard argument types do not meet requirements, custom argument types can be easily made. Argument type is a class that extends `Argument` or `SimpleArgument` class.

### Extending SimpleArgument class

This approach is suitable for data types for which following function is defined:
```C#
delegate bool TryParseDelegate(string input, out T output);
```
A conversion function should be provided to the constructor alongside with a custom error message that will be displayed if parsing goes wrong.

```C#
    public class CustomTypeArgument : SimpleArgument<CustomType>
    {
        public IntArgument(ArgumentAttribute attribute, OptionalAttribute optional) :
            base(CustomType.TryParse, "CustomType value expected.", attribute, optional) { }
    }
```

Types derived from `SimpleArgument` will expose a parsed value through the `Value` property.

### Extending Argument class

This is the most general and the most flexible approach.
A custom argument type class should extend the `Argument` class and override the abstract `Convert` method.
A concrete argument implementation has to expose a property or properties through which the actual parsed value will be consumed. There are no restrictions concerning a property name, however all readily available in the library argument classes have property called `Value`.

This example shows how to make a custom argument type that represents _DateTime_ display format.

```C#
private class DateTimeDisplayFormatArgument : Argument
{
    public enum DateTimeDisplayFormat { DATEONLY, TIMEONLY, FULL }
    public DateTimeDisplayFormat Value { get; private set; }

    public override bool Convert(string line, out string errorMessage)
    {
        switch (line.ToLower())
        {
            case "t":
            case "time":
                Value = DateTimeDisplayFormat.TIMEONLY;
                errorMessage = null;
                return true;
            case "d":
            case "date":
                Value = DateTimeDisplayFormat.DATEONLY;
                errorMessage = null;
                return true;
            case "f":
            case "full":
                Value = DateTimeDisplayFormat.FULL;
                errorMessage = null;
                return true;
            default:
                errorMessage = "Error parsing \"" + Name + "\" argument. Only date/time/full or d/t/f values are expected.";
                return false;
        }
    }
        
    public DateTimeDisplayFormatArgument(ArgumentAttribute attribute, OptionalAttribute optional) : base(attribute, optional) { }
        
}
```

### Argument validation

After successful parsing of a user input by the `Convert` method, the `Validate` method is called. It performs an additional check on the already parsed value. By default it does nothing. To make it work it must be overridden in a child class. If validation fails, the `Execute` method of the command is not called.

For example, if only odd integer numbers should be accepted following validated argument data type could be created based on `IntArgument` data type.

```C#
private class OddIntArgument : IntArgument {
    public OddIntArgument(ArgumentAttribute attribute, OptionalAttribute optional) : base(attribute, optional) { }

    public override bool Validate(out string errorMessage)
    {
        if (Value % 2 != 0)
        {
            errorMessage = null;
            return true;
        }
        errorMessage = "Odd integer value expected.";
        return false;
    }
}
```

### Treelike command structure

Commands are organized in a treelike structure for convenience.

Related commands can be grouped together with command groups. Command groups have their names and descriptions. They can contain commands and nested command groups. Nesting depth is not limited.

There are two ways to create a hierarchy.

Using group handles:
```C#
var CommandProcessor = new CommandProcessor("My CLI");
var calcGroup = CommandProcessor.AddGroup("calc", "do some calculus");
var trGroup = calcGroup.AddGroup("tr", "trigonometric functions");
calcGroup.AddCommand(new AdditionCommand())
    .AddCommand(new SubstractionCommand())
    .AddCommand(new DivisionCommand())
    .AddCommand(new MultiplicationCommand());
trGroup.AddCommand(new SinCommand())
    .AddCommand(new CosCommand())
    .AddCommand(new TgCommand())
    .AddCommand(new CtgCommand());
```

And using paths:
```C#
var CommandProcessor = new CommandProcessor("My CLI");
CommandProcessor.AddGroup("", "calc", "do some calculus")
    .AddGroup("calc", "tr", "trigonometric functions")
    .AddCommand("calc", new AdditionCommand())
    .AddCommand("calc", new SubstractionCommand())
    .AddCommand("calc", new DivisionCommand())
    .AddCommand("calc", new MultiplicationCommand())
    .AddCommand("calc tr", new SinCommand())
    .AddCommand("calc tr", new CosCommand())
    .AddCommand("calc tr", new TgCommand())
    .AddCommand("calc tr", new CtgCommand());
```
Disadvantage of this approach is that changing a group name requires modifying all paths in which this group was mentioned.

### Help command

The `CommandProcessor` automatically builds and provides help articles for command groups and commands. It exposes these articles through the following method:
```C#
public bool GetHelp(string path, out string helpArticle)
```
Method returns _true_ if `path` points to an existing element. Empty path is also considered valid, it points to the root of the command tree.

If `path` points to a command group, including root, the command tree of this group is returned.

Full tree, empty `path`:
```
Test command set
├──<textutils|tu: some useful text utils>
│  ├──<metrics|metr|mt: calculate various string metrics>
│  │  ├──"countchars|cc" - count characters in a word
│  │  └──"countwords|cw" - count words
│  ├──"displaysampletext|dst" - display sample text
│  ├──"removechar|rc" - remove character in a word
│  ├──"repw" - repeat word X number of times
│  └──"repp" - repeat phrase X number of times
├──<fr: frequently used commands>
│  ├──"displaysampletext|dst" - display sample text
│  └──"countchars|cc" - count characters in a word
├──<calc: do some calculus>
│  ├──"add" - add arbitrary number of values
│  └──"divide|div" - divide two integer values
├──"showdatetime|sdt" - show current date and time
└──"help|h" - show help
```

Subtree, `path` = “textutils metrics”:
```
<metrics|metr|mt: calculate various string metrics>
├──"countchars|cc" - count characters in a word
└──"countwords|cw" - count words
```

If `path` points to a command, help article is returned:
```
Command: removechar|rc
Description: remove character in a word
Usage: removechar|rc word [char]
   Arguments:
      word: word to remove character from
      char: character to be removed (optional)
```

In order to provide a help article to a user, the `GetHelp` method should be wrapped with a help command. Help command is not built-in in `CommandProcessor` to allow its customization.

Here is an example of a help command:
```C#
[Command("help|h", "show help")]
private class HelpCommand : ICommand
{
    private readonly CommandProcessor CommandProcessor;

    [Argument(0, "path", "path to a command or a command group", true)]
    [Optional(defaultValue: "")]
    private readonly StringArgument Path;

    public HelpCommand(CommandProcessor commandProcessor)
    {
        CommandProcessor = commandProcessor ?? throw new ArgumentNullException(nameof(commandProcessor));
    }

    public string Execute()
    {
        if (CommandProcessor.GetHelp(Path.Value, out var helpArticle))
        {
            if (Path.IsOmitted)
                helpArticle += Environment.NewLine + "In order to get help on a particular command please type \"help pathToCommand commandName\" or \"h pathToCommand commandName\".";
            return helpArticle;
        }
        else
            return "Nothing found. Please type \"help\" or \"h\" with no arguments.";
    }
}
```

Note that the help command requires a `CommandProcessor` instance to be passed as a parameter.
```C#
CommandProcessor.AddCommand(new HelpCommand(CommandProcessor));
```

## Configuration errors

**Friendly CLP** detects common configuration errors including: invalid names, overlapping command names and argument positions, missing annotations, annotating wrong entities, declaring non last argument as _multisegmented_ or _optional_ etc. In case such configuration error is found an exception will be thrown.

## Example projects

There are three example projects:
- CommandSetExample 
- ConsoleExample
- TelegramBotExample

**CommandSetExample** is a command set that is specially made to show most **Friendly CLP** features.

**ConsoleExample** is a simple console application that allows to interact with the command set example.

**TelegramBotExample** connects the command set example to a Telegram bot with the help of the _Telegram.Bot_ library. In order to run this example API key from the BotFather is needed. 

Enjoy!  
:raccoon:
