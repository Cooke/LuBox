# LuBox
LuBox is a Lua inspired scripting language for embedding in .NET applications.

### The main characteristics of LuBox:

1. Lightweigth API which is easy to embed.
2. Sandboxed with restricted access to IO and reflection.
3. Based on the Dynamic Language Runtime (DLR).

### Examples ###
**Introduction**
```csharp
var scriptEngine = new LuScriptEngine();
scriptEngine.Globals.hello = "Hello";
scriptEngine.Execute("hello = hello:ToUpper()");
Console.WriteLine(scriptEngine.Globals.hello); // Output: HELLO
```

**Events**
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
Foo foo = new Foo();
scriptEngine.Globals.foo = foo;
scriptEngine.Globals.console = Console.Out;
scriptEngine.Execute(
  @"
  function handleBar(text)
      console:WriteLine(text)
  end
  foo.BarFired:Add(handleBar)");
foo.FireBar(); // Output: HELLO
```
