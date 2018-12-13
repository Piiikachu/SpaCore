using System.IO;

namespace SpaCore
{
    public class Input
    {
        private FileStream infile;

        private Variable variable;

        public Input(SPARTA sparta)
        {
            this.infile = sparta.infile;

            variable = new Variable(sparta);

        }
    }
}