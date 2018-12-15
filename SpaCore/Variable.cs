using System;
using System.Collections.Generic;
using System.Text;

namespace SpaCore
{
    public class Variable
    {
        private SPARTA sparta;

        enum precedence
        {
            DONE, ADD, SUBTRACT, MULTIPLY, DIVIDE, CARAT, MODULO, UNARY,
            NOT
        }

        public Variable(SPARTA sparta)
        {
            this.sparta = sparta;
            
        }
    }
}
