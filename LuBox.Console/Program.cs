namespace LuBox.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var nuEngine = new LuScriptEngine();
            nuEngine.Evaluate<int>("local temp = 1 + 2");
            System.Console.ReadLine();
        }
    }
}
