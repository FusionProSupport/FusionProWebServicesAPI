using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace FPWebServicesDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() < 4)
            {
                Console.Write("Usage: Template Group InputFile OutputFolder\n");
                return;
            }
            Console.Write(Launcher.ComposeJob(args[0], args[1], args[2], args[3], false));
        }
    }
}