namespace TestStandApp.Buisness.Logger
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            //using (StreamWriter writer = new StreamWriter("c:\\logs.txt", true))
            //{
            //    writer.WriteLine(message); //TODO
            //}
        }
    }
}
