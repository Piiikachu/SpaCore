using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SpaCore
{
    public class Input
    {
        private SPARTA sparta;
        private FileStream infile;

        private Variable variable;


        public Input(SPARTA sparta)
        {
            this.infile = sparta.infile;

            variable = new Variable(sparta);
            this.sparta = sparta;

        }

        public int ExpandArgs(int narg, string[] arg, int mode, out string[] earg)
        {
            string str = null;

            foreach (string s in arg)
            {
                if (s.Contains('*'))
                {
                    str = s.Substring(s.IndexOf('*'));
                    break;
                }

            }
            if (string.IsNullOrEmpty(str))
            {
                earg = arg;
                return narg;
            }
            sparta.DumpMessage("complete input->expand_args: args has char * ");
            earg = arg;
            return narg;


        }
        private string[] args;
        private string command;
        public void File()
        {
            using (StreamReader sr = new StreamReader(infile))
            {
                string str;
                while ((str = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        continue;
                    }
                    if (str.StartsWith("#"))
                    {
                        continue;
                    }
                    if (str.Contains("#"))
                    {
                        str = str.Substring(0, str.IndexOf("#"));
                    }
                    Parse(str);
                    if (!ExcuteCommand())
                    {
                        sparta.DumpMessage(string.Format("Unknown Command:{0}", command));
                    }

                }
            }
        }

        private bool ExcuteCommand()
        {
            bool done = false;
            switch (command)
            {
                case "seed": Seed(); done = true; break;
                case "dimension": Dimension(); done = true; break;
                case "global": Global(); done = true; break;
                case "boundary": Boundry(); done = true; break;
                case "create_box": CreateBox(); done = true; break;
                case "create_grid": CreateGrid(); done = true; break;
                case "balance_grid": BalanceGrid(); done = true; break;
                default:
                    break;
            }



            return done;
        }

        private void BalanceGrid()
        {
            //new BalanceGrid(sparta).Command(args);
        }

        private void CreateGrid()
        {
            new CreateGrid(sparta).Command(args);
        }

        private void CreateBox()
        {
            new CreateBox(sparta).Command(args);
        }

        private void Boundry()
        {
            sparta.domain.SetBoundry(args);
        }

        private void Global()
        {
            sparta.update.Global(args);
        }

        private void Dimension()
        {
            Domain domain = sparta.domain;
            if (args.Length != 1)
            {
                sparta.DumpError("Illegal dimension command");
            }
            if (domain.box_exist)
            {
                sparta.DumpError("Dimension command after simulation box is defined");
            }
            domain.dimension = int.Parse(args[0]);
            if (domain.dimension != 2 && domain.dimension != 3)
            {
                sparta.DumpError("Illegal dimension command");
            }

        }

        private void Seed()
        {
            if (args.Length != 1)
            {
                sparta.DumpError("Illegal seed command");
            }
            int seed = int.Parse(args[0]);
            if (seed <= 0)
            {
                sparta.DumpError("Illegal seed command");
            }
            sparta.update.ranmaster.Init(seed);
        }

        private void Parse(string str)
        {
            string[] commands = Regex.Split(str, @"\s+");
            List<string> list = new List<string>();
            foreach (string s in commands)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    continue;
                }
                list.Add(s);
            }

            command = list[0];
            args = new string[list.Count - 1];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = list[i+1];
            }
        }
    }
}