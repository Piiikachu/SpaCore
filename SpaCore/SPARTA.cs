using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpaCore
{
    public class SPARTA
    {

        public const string VERSION = "dotnet core sparta 0.1 by Adidas";

        public Input input;

        public Particle particle;
        public Update update;
        public Domain domain;
        public Modify modify;
        public Grid grid;
        public Surf surf;
        public Collide collide;
        public React react;
        public Output output;
        public Timer timer;


        public FileStream infile;
        private FileStream logfile;

        public StreamWriter sw;

        public void DumpMessage(string message)
        {
            sw.WriteLine(message);
            Console.WriteLine(message);
        }
    

        public SPARTA(string[] args)
        {
            if (args.Length!=0)
            {
                if (args[0].Equals("-in"))
                {

                    try
                    {
                        infile = new FileStream(args[1], FileMode.Open, FileAccess.Read,FileShare.Read);

                    }
                    catch (FileNotFoundException e)
                    {
                        throw e;
                    }
                }
            }

            try
            {
                logfile = new FileStream("log.sparta", FileMode.Create, FileAccess.Write,FileShare.ReadWrite);
                sw = new StreamWriter(logfile);
            }
            catch (Exception e)
            {
                throw e;
            }

            DumpMessage(VERSION);

            input = new Input(this);

            Create();
            //PostCreate();
        }

        private void PostCreate()
        {
            throw new NotImplementedException();
        }

        private void Create()
        {
            update = new Update(this);
            particle = new Particle(this);
            domain = new Domain(this);
            grid = new Grid(this);
            surf = new Surf(this);

            collide = null;
            react = null;

            modify = new Modify(this);
            output = new Output(this);
            timer = new Timer(this);
            DumpMessage("create(): done.");
        }
    }
}
