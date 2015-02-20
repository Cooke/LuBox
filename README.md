# LuBox
LuBox is a Lua inspired scripting language for embedding in .NET applications.

### The main characteristics of LuBox:

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

**Handle CLR Events**
```csharp
public class Foo
{
    public event Action<string> BarFired;

    public void FireBar()
    {
        BarFired("BAR!");
    }
}
```
```csharp
var scriptEngine = new LuScriptEngine();
dynamic environment = scriptEngine.CreateStandardEnvironment();

var foo = new Foo();
environment.foo = foo;
environment.console = Console.Out;

scriptEngine.Execute(@"
    function handleBar(text)
        console:WriteLine(text)
    end

    foo.BarFired:Add(handleBar)", environment);

 foo.FireBar(); // Output: BAR!
```

**Iterate CLR Enumerable**
```csharp
var scriptEngine = new LuScriptEngine();
dynamic environment = scriptEngine.CreateStandardEnvironment();

environment.list = new List<int> { 1, 2, 3, 4, 5 };
scriptEngine.Execute(@"
    sum = 0
    for i in iter(list) do
        sum = sum + i
    end
    ", environment);

Console.WriteLine(environment.sum); // 15
```
