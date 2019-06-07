﻿using System;

namespace Bellight.Core.Misc
{
    public class ConsoleStaticLogProvider : IStaticLogProvider
    {
        public void Error(string message)
        {
            Console.WriteLine("{0:u} [ERROR]: {1}", DateTime.Now, message);
        }

        public void Error(Exception ex)
        {
            Console.WriteLine("{0:u} [ERROR]: {1} - Stack trace: {2}",
                DateTime.Now,
                ex.Message, 
                ex.StackTrace);
        }

        public void Error(Exception ex, string messageTemplate)
        {
            Console.WriteLine("{0:u} [ERROR]: {1} - Stack trace: {2}",
                DateTime.Now,
                string.Format(messageTemplate, ex), 
                ex.StackTrace);
        }
    }
}
