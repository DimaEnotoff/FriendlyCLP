## Friendly Command Line Processor

Friendly CLP is a library that facilitates rapid development of command line interfaces with focus on convenience of users who access them from a mobile phone.


### Mobile friendly

Slash, colon, dot and other special characters that are commonly used in most CLIs are cumbersome to type on mobile phone keyboard. To address this issue Friendly CLP uses spaces to separate command groups, commands and parameters from each other.


### Other features

- Easy to add commands and organise them in groups
- Easy to add parameters, most commonly used data types are readily available
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
_ICommand_ interface has _Execute_ method that should contain a commands payload.  
_Command_ annotation has _name_ and _description_ fields. _Name_ is used to call the command from a command line, so it should be concise and should contain no spaces. _Description_ is a brief command description that will be shown in a help article.

```C#
    [Command("dst", "Display sample text.")]
    private class DisplaySampleTextCommand : ICommand
    {
        public string Execute() => "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut ...";
    }
```

#### 3. Create a root command group

Command groups serve multiple purposes. They organise commands in a tree like structure, parse user input, call corresponding commands if input is valid or display error message if not. Following code creates an instance of a command group class that represets a root of a command tree. It can contain other command groups or commands directly. Multiple command trees can be used in the same application. They can be used, for example, to proccess commands issued by users with different authorization levels.

```C#
    CommandGroup rootCommandGroup = new CommandGroup("My CLI");
```

#### 4. Add the command to the root group

Command should be added to a command group so that Friendly CLP engine can find it.

```C#
    rootCommandGroup.AddCommand("", new DisplaySampleTextCommand());
```

#### 5. Process user input

`ProcessLine` method of a command group takes user input, parses it and calls corresponding command if input is valid or returns meaningful error message if not.

```C#
    while (true) Console.WriteLine(rootCommandGroup.ProcessLine(Console.ReadLine()));
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
