using System;

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
            using (MainSource game = new MainSource())
            {
                game.Run();
            }
        }
    }
#endif
}

