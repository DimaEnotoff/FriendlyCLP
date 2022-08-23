using CommandSetExample;

// This simple CLI application shows how Friendly CLP library can be used to process user commands.

// Creating an instance of a Friendly CLP command set.
var MyCommandSet = new CommandSet();

Console.WriteLine("Friendly command line processor test console, type \"help\", to show command list!");

while (true) {
    
    // Processing an incoming user command with a Friendly CLP command processor.
    var response = MyCommandSet.CommandProcessor.ProcessLine(Console.ReadLine());
    Console.WriteLine(response);

}