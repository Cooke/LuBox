namespace LuBox.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var nuEngine = new NuEngine();
            nuEngine.Evaluate<int>("local temp = 1 + 2");
            System.Console.ReadLine();
        }
    }
}
