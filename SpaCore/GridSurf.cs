using System;
using System.Collections.Generic;
using System.Text;

namespace SpaCore
{
    public partial class Grid
    {
        public void AllocateSurfArrays()
        {
            csurfs = new List<int>();
            csplits = new List<int>();
            csubs = new List<int>();
        }

    }
}
