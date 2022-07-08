## Friendly Command Line Processor

Friendly CLP is a library that facilitates rapid development of command line interfaces with focus on convenience of those who access them from a mobile phone.


### Mobile friendly

Slash, colon, dot and other special characters that are commonly used in most CLIs are cumbersome to access on mobile phone keyboards. To address this issue Friendly CLP uses spaces to separate command groups, commands and parameters from each other.


### Other features

- Easy to add commands and organise them in groups
- Easy to add parameters, some commonly used types are readily available
- All required parsing is done automatically, payload code is called only if command line is valid
- Meaningful error messages are given if parsing fails
- Help articles are generated automatically


### Quick start

#### 1. Import the library

```C#
    using FriendlyCLP;
```

#### 2. Add a command

Friendly CLP command is a class that implements `ICommand` and has `Command` annotation.  
_ICommand_ has only one method _Execute_, it schould contain payload code.
_Command_ annotation has _name_ and description of the command.
_Name_ should be concise because it is used to call a command.

```C#
    [Command("dst", "Display sample text.")]
    private class DisplaySampleTextCommand : ICommand
    {
        public string Execute() => "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut ...";
    }
```

#### 3. Create a root command group

This creates an instance of a command group class that represets a root of a command tree. It can contain other command groups or commands directly.
Multiple command trees can be used in the same application. They can be used to proccess commands issued by users with different authorization levels, for example.

```C#
    CommandGroup rootCommandGroup = new CommandGroup("My CLI");
```

#### 4. Add new command to the root group

```C#
    rootCommandGroup.AddCommand("", new DisplaySampleTextCommand());
```

#### 5. Process user input

```C#
    while (true) Console.WriteLine(rootCommandGroup.ProcessLine(Console.ReadLine()));
```


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
