## Friendly Command Line Processor

Friendly CLP is a library that facilitates rapid development of command line interfaces with focus on convenience of users who access them from a mobile phone. The library was used in production to build debugging/engineering menus for self service kiosks and other automation systems.


### Mobile friendly

Slash, colon, dot and other special characters that are commonly used in most CLIs are cumbersome to type on mobile phone keyboard. To address this issue Friendly CLP uses spaces to separate command groups, commands and parameters from each other.


### Other features

- Easy to add commands and organise them in groups
- Easy to add command parameters, most commonly used data types are readily available, creating a custom parameter data type is straightforward
- All required parsing is done automatically, payload code is called only if command line is valid
- Meaningful error messages are given if parsing fails
- Help articles are generated automatically


### Quick start

#### 1. Import the library

```C#
    using FriendlyCLP;
```

#### 2. Create a command

Friendly CLP command is a class that implements `ICommand` interface and has `Command` annotation.  
_ICommand_ interface has _Execute_ method that should contain a command payload.  
_Command_ annotation has _name_ and _description_ fields. _Name_ is a name or an abbriviation of a command that will be used to call it from a command line, so it should be concise and should contain no spaces. _Description_ is a brief command description that will be shown in a help article.

```C#
    [Command("dst", "Display sample text.")]
    private class DisplaySampleTextCommand : ICommand
    {
        public string Execute() => "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed ...";
    }
```

#### 3. Create a Command Processor instance

`CommandProcessor` is a class that contains commands in a tree like structure, parses user input, calls corresponding command if input is valid or display error message if not. Multiple _CommandProcessor_ instances, containing different command trees can be used in the same application. They can be used, for example, to proccess commands issued by users with different authorization levels.

```C#
    CommandProcessor CommandProcessor = new CommandProcessor("My CLI");    
```

#### 4. Add the command to the _CommandProcessor_ 

In order to make a command invokable from a particular _CommandProcessor_ instance it should be added to it. One command can be added to several _CommandProcessor_ instances.

```C#
    CommandProcessor.AddCommand("", new DisplaySampleTextCommand());
```

#### 5. Process user input

`ProcessLine` method of a _CommandProcessor_ instance takes user input, parses it and calls a corresponding command if input is valid or returns meaningful error message if not.

```C#
    while (true) Console.WriteLine(CommandProcessor.ProcessLine(Console.ReadLine()));
```

This example makes the simplest command line interface with just one command with no parameters.

### Going further

#### Adding arguments



```C#
    [Command("div", "Divide two integer values.")]
    class DivideCommand : ICommand
    {

        [Argument(0, "dvd", "Dividend", optional: false, multisegmented: false)]
        private readonly IntArgument Divident;

        [Argument(1, "dvs", "Divisor", optional: false, multisegmented: false)]
        private readonly IntArgument Divisor;

        public string Execute()
        {
            if (Divisor.Value == 0) return "Can not divide by zero.";
            return ((float)Divident.Value / Divisor.Value).ToString();
        }
    }
```

#### Arguments

Arguments are classes that extend abstract `Argument` class. Concrete argument implementation has to expose a property through which actual parsed value will be consumed. 

Convert Validate SetDefault



Friendly console test, type "help", to show command list!
help
Friendly console test
├──<tu: Some useful text utils.>
│  ├──<mt: Calculate various string metrics.>
│  │  ├──"cc" - Count characters in a word.
│  │  └──"cw" - Count words.
│  ├──"dst" - Display sample text.
│  └──"rc" - Remove character in a word.
├──<calc: Do some calculus.>
│  ├──"add" - Add arbitrary number of values.
│  └──"div" - Divide two integer values.
├──"scdt" - Show current date time.
└──"help" - Show help.
In order to get help on a particular command please type "help pathToCommand commandName".
