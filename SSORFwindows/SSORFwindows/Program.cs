using System;
using System.Threading;
namespace SSORF
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(1);
#endif
            using (MainSource game = new MainSource())
            {
                game.Run();
            }
        }
    }
#endif
}

