using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceStateless
{
    class Program
    {
        static void Main(string[] args)
        {
            var processor = new Processor("ResourceOne");            

            processor.Print();
            processor.Start("Article1", 5);
            processor.Print();
            processor.Done();
            processor.Print();
            processor.Start("Article2", 3);
            processor.Print();
            Console.WriteLine("Sleep 4000");
            System.Threading.Thread.Sleep(4000);
            processor.PlannedWaitStart();
            processor.Print();
            processor.PlannedWaitDone();
            processor.Print();
            processor.Start("Article3", 6);
            processor.Print();
            processor.Done();
            processor.Print();

            //Console.WriteLine(processor.ToDotGraph());

            Console.WriteLine("Press any key...");
            Console.ReadKey(true);


            var processorEvent = new Processor("ResourceEvents");            

            processorEvent.Print();
            processorEvent.Start("Article1", 5);
            processorEvent.Print();
            processorEvent.Done();
            processorEvent.Print();
            processorEvent.Start("Article2", 3);
            processorEvent.Print();
            Console.WriteLine("Sleep 4000");
            System.Threading.Thread.Sleep(4000);
            processorEvent.PlannedWaitStart();
            processorEvent.Print();
            processorEvent.PlannedWaitDone();
            processorEvent.Print();
            processorEvent.Start("Article3", 6);
            processorEvent.Print();
            processorEvent.Done();
            processorEvent.Print();

            //Console.WriteLine(processor.ToDotGraph());

            Console.WriteLine("Press any key...");
            Console.ReadKey(true);
        }
    }
}
