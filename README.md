# LuBox
LuBox is a Lua inspired scripting language for embedding in .NET applications.

### Characteristics ###

1. Lightweigth API which is easy to embed.
2. Sandboxed with restricted access to IO and reflection.
3. Based on the Dynamic Language Runtime (DLR).

### Examples ###
**Call CLR Objects**
```csharp
var scriptEngine = new LuScriptEngine();
dynamic environment = scriptEngine.CreateStandardEnvironment();

environment.myVariable = "Hello";
scriptEngine.Execute("myVariable = myVariable:ToUpper()", environment);

Console.WriteLine(environment.myVariable);  // Output: HELLO
```

**Call LuBox Function**
```csharp
var scriptEngine = new LuScriptEngine();
dynamic environment = scriptEngine.CreateStandardEnvironment();

scriptEngine.Execute(@"
    function Add(left, right)
        return left + right
    end", environment);

Console.WriteLine(environment.Add(1, 2));  // Output: 3
```
More examples at [the Wiki](https://github.com/Cooke/LuBox/wiki)

### Performance and memory management ###
Since LuBox is implemented using the DLR the performance is typically close to regular .NET code after the first execution of a script. During the first execution however, or when rebinding due to changed types of variables, reflection is used and the execution time is usally in the order of milliseconds for a small script. 

All objects allocated in a LuBox script are regular CLR objects and are collected by the .NET garbage collector. 

### Feature requests and bugs ###
Please send a pull request with a unit test of the feature requested or the bug found. 
