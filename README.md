<img src="resources/logo.png" width = "100">
# ECLP
Easy CommandLine Parser

[![MIT License](https://img.shields.io/apm/l/atomic-design-ui.svg?)](https://github.com/melharfi/ECLP/blob/master/LICENSE)
[![Version](https://badge.fury.io/gh/tterb%2FHyde.svg)](https://github.com/melharfi/ECLP)
![GitHub Release Date](https://img.shields.io/github/release-date/melharfi/ECLP?color=Green)
[![GitHub Release](https://img.shields.io/github/v/release/melharfi/ECLP)](https://github.com/melharfi/ECLP/releases) 
[![PayPal](https://img.shields.io/badge/paypal-donate-yellow.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=VN92ND2CDMX92)
![GitHub language count](https://img.shields.io/github/languages/count/melharfi/ECLP?color=red)

Documentation in progress

This command Line parser use 2 ways of parsing
Eather from a string command or from an array like the one passed to the main method as parameters.

just reference ECLP by nuget or be dowloading the latest release and add it to your references.

ECLP defin 5 types of arguments as Verbs

**Type 1)** Args are arguments could be any referenced primitif type like string, int, bool, float, and char ex "hello 5 true 3,5 T".
Argument is parsed to it's appropriate type, otherwize it return the default value as string
Make attention that positions of arguments is necessary so you can identify them in the Args list by there index.

**Type 2)** Flags are string type arguments without values, prefixed with double dash -- "--verbose --start --friendlyfire".
Value is not parsed and it's a string type.

**Type 3)** Properties are a property with value prefixed with -p "-p driver=steave -p age=30".
Value is parsed to appropriate type, otherwize the default format is 'string'

**Type 4)** Collections is a property with a collection of values separated by Pipe |.
Should be prefixed with -c "-c players=steave|john|clark -c ages=21|15|30" players is the name of the property, and steave,john,clark are a list of object values.

**Type 5)** ExCollections for Extanded Collections, it's a property with a collection of sub properties that have a value.
Should be prefixed with -xc "-xc players=steave:21|john:15|clark:30 -xc adresses=Japan:Tokyo|USA:Washington".
Properties are separated by Pipe |, and sub property name and it's values are separated by double point : .

***How to use ECLP***
add reference using The_Morpher

***C# code***
--------------------------------
string commandLine = "start 5 3.6 true T --verbose --noClip --showStats -p driver=steave -p age=30 -c players=steave|john|clark -xc players=steave:21|john:15|clark:30";

ECLP eCLP = new ECLP(commandLine);
// you can either use the array args as well
CommandResult result = eCLP.Parse();

-------------------------------

The object result store all data parsed.

-For argument you can check the **result.Args**, it's a list of object.

Arguments in the command line are 5 "start 5 3.6 true T" will be parsed as string, int, float, bool and char.

-For flags you can use **result.Flags**, it's a list of strings

Flags are 3 "--verbose --noClip --showStats" in the command line and will be parsed as they are "string", usualy considered as --verbose = true if you want to have more sence.

-For properties you can use **result.Properties**, it's a Dictionnary with string as entry/key and object as value Dictionary<string, object>.

Properties are 2 in the command line "-p driver=steave -p age=30" and will be parsed as driver is the key or name of property and steave it's value, same for the second property.

-For collections you can use **result.Collections**, it's a dictionnary with string as entry/key and array object as value Dictionary<string, object>

Collections in the command  line are 1 "-c players=steave|john|clark" will be parsed as a collection name is players and has 3 values, steave, john and clark.

-For ExtraCollection you can use **result.ExCollection**, it's a dictionnary with string as entry/key and list of array of entry and it's value Dictionary<string, List<KeyValuePair<string, object>>>.

"ExtraCollections are 1 in the command line -xc players=steave:21|john:15|clark:30" and its name is players and it has 3 sub properties, steave is the sub property name and 21 is it's value, and so on.

You can use Link to make a seearch inside each of those collections or use a foreach.

**Event callback**

You can register to a specific event type of verb.

static void Main(string[] args)

        {
        
            string commandLine = "start 5 3.6 true T --verbose --noClip --showStats -p driver=steave -p age=30 -c players=steave|john|clark -xc players=steave:21|john:15|clark:30";

            ECLP eCLP = new ECLP(commandLine);
            
            eCLP.AddedArgs += ECLP_AddedArgs;
            
            eCLP.AddedCollections += ECLP_AddedCollections;
            
            eCLP.AddedExCollections += ECLP_AddedExCollections;
            
            eCLP.AddedFlags += ECLP_AddedFlags;
            
            eCLP.AddedProperties += ECLP_AddedProperties;
            
            CommandResult result = eCLP.Parse();

            Console.ReadKey();
        }

        private static void ECLP_AddedProperties(object sender, The_Morpher.Events.AddedPropertiesEvents e)
        
        {
        
            // To show key
            
            Console.WriteLine(e.Property.Key);

            // to do job when a specific property is triggered
            
            if (e.Property.Key == "driver")
                Console.WriteLine(e.Property.Value);
                
            // this will show steave
            
        }

        private static void ECLP_AddedFlags(object sender, The_Morpher.Events.AddedFlagsEvents e)
        
        {
        
            // To show value
            
            Console.WriteLine(e.Flag);

            if (e.Flag == "--verbose")
                Console.WriteLine("Verbose mode is activated");
                
        }

        private static void ECLP_AddedExCollections(object sender, The_Morpher.Events.AddedExCollectionsEvents e)
        
        {
        
            // To show key, ExCollection's name
            
            Console.WriteLine(e.ExCollections.Key);

            // loop through the sub properties
            
            foreach(KeyValuePair<string, object> sub in e.ExCollections.Value)
            
            {
            
                Console.WriteLine("sub property name is " + sub.Key + " and it's value is " + sub.Value + " and value type is " + sub.Value.GetType());
                
            }
            
        }

        private static void ECLP_AddedCollections(object sender, The_Morpher.Events.AddedCollectionsEvents e)
        
        {
        
            // To show key, Collection name
            
            Console.WriteLine(e.Collections.Key);

            // to loop through sub values
            
            foreach(object o in e.Collections.Value)
            
            {
            
                Console.WriteLine("sub property is " + o + " and its type is " + o.GetType());
                
            }
            
        }

        private static void ECLP_AddedArgs(object sender, The_Morpher.Events.AddedArgsEvents e)
        
        {
        
            Console.WriteLine("Argument is " + e.Arg + " and its type is " + e.Arg.GetType());
            
        }

