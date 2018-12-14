using System;
using System.IO;

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
            string str=null;

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
    }
}