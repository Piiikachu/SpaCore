namespace SpaCore
{
    public class CreateBox
    {
        private SPARTA sparta;

        public CreateBox(SPARTA sparta)
        {
            this.sparta = sparta;
        }
        public void Command(string[] arg)
        {
            int narg = arg.Length;
            Domain domain = sparta.domain;
            if (domain.box_exist)
            {
                sparta.DumpError("Cannot create_box after simulation box is defined");
            }

            domain.box_exist = true;

            if (narg != 6)
            {
                sparta.DumpError("Illegal create_box command");
            }

            domain.boxlo[0] = double.Parse(arg[0]);
            domain.boxhi[0] = double.Parse(arg[1]);
            domain.boxlo[1] = double.Parse(arg[2]);
            domain.boxhi[1] = double.Parse(arg[3]);
            domain.boxlo[2] = double.Parse(arg[4]);
            domain.boxhi[2] = double.Parse(arg[5]);

            if (domain.dimension == 2)
            {
                if (domain.boxlo[2] >= 0 || domain.boxhi[2] <= 0)
                {
                    sparta.DumpError("Create_box z box bounds must straddle 0.0 for 2d simulations");
                }
            }

            if (domain.axisymmetric && domain.boxlo[1] != 0.0)
            {
                sparta.DumpError("Box ylo must be 0.0 for axi-symmetric model");
            }

            sparta.update.ntimestep = 0;

            domain.PrintBox("Created ");
            domain.SetInitialBox();
            domain.SetGlobalBox();

        }
    }
}