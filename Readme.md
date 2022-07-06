## Friendly Command Line Processor

Friendly CLP is a library that facilitates rapid development of command line interfaces. Especially those that are accessed via mobile phones.
 

#### Mobile friendly

Slashes, dot and other special characters that are commonly used in most CLIs are hard to access on mobile phone keyboards. To address this issue Friendly CLP uses spaces only. They are used to separate command groups, commands and parameters from each other.


### Features

- Easy to add commands and organise them in groups
- Easy to add parameters, some commonly used types are readily available
- Does all required parsing and calls payload code only if all parameters are valid
- Gives meaningful error messages if parsing fails
- Builds help articles automatically

#### Commands

Friendly CLP command is a class that implements *ICommand* and has *Command* annotation.
*ICommand* has only one method - *Execute*, that contains payload code.
*Command* annotation has name and description of the command.

    [Command("dst", "Display sample text.")]
    private class DisplaySampleTextCommand : ICommand
    {
        public string Execute() => "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut ...";
    }

