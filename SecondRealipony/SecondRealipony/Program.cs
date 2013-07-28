using System;

namespace SecondRealipony
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SRController game = new SRController())
            {
                game.Run();
            }
        }
    }
#endif
}

