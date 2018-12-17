using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SpaCore
{
    internal class ReadSurf
    {
        private SPARTA sparta;

        private string filename;
        private FileStream fp;

        private int dim;
        private int npoint, nline;

        enum PartFlag { NONE, CHECK, KEEP };
        public ReadSurf(SPARTA sparta)
        {
            this.sparta = sparta;
        }

        public class Point
        {
            public double[] x;
            public Point()
            {
                x = new double[3];
            }
        }
        public class Line
        {
            public int type, mask;
            public int p1, p2;
        }
        public List<Point> pts;
        public List<Line> lines;


        string keyword;

        public void Command(string[] arg)
        {
            Grid grid = sparta.grid;
            Surf surf = sparta.surf;
            Domain domain = sparta.domain;
            Particle particle = sparta.particle;
            int narg = arg.Length;
            filename = arg[0];

            double[] origion = new double[3];
            if (!grid.exist)
            {
                sparta.DumpError("Cannot read_surf before grid is defined");
            }
            surf.exist = true;
            dim = domain.dimension;
            if (narg < 1)
            {
                sparta.DumpError("Illegal read_surf command");
            }
            Console.WriteLine("Reading surf file ...\n");
            try
            {
                fp = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception e)
            {

                throw e;
            }
            double time1 = Timer.getTime();
            Header();

            pts = new List<Point>(npoint);
            lines = new List<Line>(nline);

            ParseKeyword(true);

            ParseKeyword(false);
            PartFlag pf = PartFlag.NONE;
            bool grouparg = false;

            int iarg = 1;
            while (iarg < narg)
            {
                switch (arg[iarg])
                {

                    default:
                        sparta.DumpError(string.Format("ReadSurf->Command: complete args {0}", arg[iarg]));
                        break;
                }
            }

            if (particle.exist && pf == PartFlag.NONE)
            {
                sparta.DumpError("Using read_surf particle none when particles exist");
            }

            //if (grouparg)
            //{
            //    int igroup=surf.FindGroup
            //}
            // extent of surfs after geometric transformations
            // compute sizes of smallest surface elements

            double[,] extent = new double[3, 2];
            extent[0, 0] = extent[1, 0] = extent[2, 0] = double.MaxValue;
            extent[0, 1] = extent[1, 1] = extent[2, 1] = -double.MaxValue;

            foreach (Point p in pts)
            {
                extent[0, 0] = Math.Min(extent[0, 0], p.x[0]);
                extent[0, 1] = Math.Max(extent[0, 1], p.x[0]);
                extent[1, 0] = Math.Min(extent[1, 0], p.x[1]);
                extent[1, 1] = Math.Max(extent[1, 1], p.x[1]);
                extent[2, 0] = Math.Min(extent[2, 0], p.x[2]);
                extent[2, 1] = Math.Max(extent[2, 1], p.x[2]);
            }

            double minlen=0, minarea=0;
            if (dim == 2)
            {
                minlen = ShortestLine();
            }
            if (dim == 3)
            {
                sparta.DumpError("Readsurf->Command: complete dimension3");
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("  {0:G6} {1:G6} xlo xhi\n", extent[0, 0], extent[0, 1]);
            sb.AppendFormat("  {0:G6} {1:G6} ylo yhi\n", extent[1, 0], extent[1, 1]);
            sb.AppendFormat("  {0:G6} {1:G6} zlo zhi\n", extent[2, 0], extent[2, 1]);
            if (dim==2)
            {
                sb.AppendFormat("  {0:G6} min line length\n", minlen);
            }
            else
            {
                sparta.DumpError("Readsurf->Command: complete dimension3");
            }
            sparta.DumpMessage(sb.ToString());

            if (dim == 2)
            {
                surf.nline = nline;
                surf.Grow();
                List<Surf.Line> newlines = surf.lines;

                int m = 0;
                foreach (Line line in lines)
                {
                    Surf.Line l = new Surf.Line();
                    l.id = m + 1;  // check for overflow when allow distributed surfs
                    l.type = line.type;
                    l.mask = line.mask;
                    l.isc = l.isr = -1;
                    Array.Copy(pts[line.p1].x, l.p1,  3);
                    Array.Copy(pts[line.p2].x, l.p2,  3);
                    newlines.Add(l);
                    m++;
                }


            }
            else if (dim==3)
            {
                sparta.DumpError("Readsurf->Command: complete dimension3");
            }


            if (dim==2)
            {
                surf.ComputeLineNormal();
            }
            else
            {
                sparta.DumpError("Readsurf->Command: complete dimension3");
            }

            if (dim == 2)
            {
                surf.CheckPointInside();
            }
            else
            {
                sparta.DumpError("Readsurf->Command: complete dimension3");
            }

            double time2 = Timer.getTime();

            if (particle.exist)
            {
                particle.Sort();
            }

            surf.SetupSurf();
            //todo:not necessary
            grid.UnsetNeighbors();
            grid.RemoveGhosts();

            if (particle.exist&&grid.nsplitlocal!=0)
            {
                throw new NotImplementedException();

            }

            grid.ClearSurf();

            double time3 = Timer.getTime();
            //todo:continue
            if (dim==2)
            {
                surf.CheckWatertight2d(nline);
                CheckNeighborNorm2d();
            }

            double time4 = Timer.getTime();
            grid.SurftoGrid(true);

            throw new NotImplementedException();
        }

        private void CheckNeighborNorm2d()
        {
            sparta.DumpMessage("ReadSurf->CheckNeighborNorm2d: this is the function that checks if the surf is infinitely thin");

            //throw new NotImplementedException();
        }

        private double ShortestLine()
        {
            double len = double.MaxValue;
            foreach (Line l in lines)
            {
                len = Math.Min(len, Surf.LineSize(pts[l.p1].x, pts[l.p2].x));
            }
            return len;
        }

        private void GrowSurf()
        {
            throw new NotImplementedException();
        }

        private void ParseKeyword(bool first)
        {
            using (fp = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                StreamReader sr = new StreamReader(fp);
                string line;
                bool startflag = false;
                if (!first)
                {
                    if (dim == 2)
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.Trim().Equals("Lines"))
                            {
                                keyword = line.Trim();
                                startflag = true;
                                continue;
                            }
                            if (startflag)
                            {
                                startflag = ReadLines(line.Trim());
                            }
                        }
                        if (!keyword.Equals("Lines"))
                        {
                            sparta.DumpError("Read_surf did not find lines section of surf file");
                        }
                        if (lines.Count != nline)
                        {
                            sparta.DumpError("Incorrect line number in surf file");
                        }
                        else
                        {
                            sparta.DumpMessage(string.Format("  {0} lines\n", nline));
                        }

                    }
                    else if (dim == 3)
                    {
                        sparta.DumpError("ReadSurf->ParseKeyword: complete dimension=3");
                    }
                }
                else if (first)
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Contains("Points"))
                        {
                            keyword = "Points";
                            startflag = true;
                            continue;
                        }
                        if (startflag)
                        {
                            startflag = ReadPoints(line);
                        }
                    }
                    if (!keyword.Equals("Points"))
                    {
                        sparta.DumpError("Read_surf did not find points section of surf file");
                    }
                    if (pts.Count != npoint)
                    {
                        sparta.DumpError("Incorrect point number in surf file");
                    }
                    else
                    {
                        sparta.DumpMessage(string.Format("  {0} points\n", npoint));
                    }
                }
            }
        }

        private bool ReadPoints(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return true;
            }
            if (str.Contains("Lines") || str.Contains("Triangles"))
            {
                return false;
            }
            string[] words = str.Split();
            if (dim == 2 && words.Length != 3)
            {
                sparta.DumpError("Incorrect point format in surf file");
            }
            if (dim == 3 && words.Length != 4)
            {
                sparta.DumpError("Incorrect point format in surf file");
            }
            double[] x = new double[3];
            x[0] = double.Parse(words[1]);
            x[1] = double.Parse(words[2]);
            if (dim == 3)
            {
                x[2] = double.Parse(words[3]);
            }
            else
            {
                x[2] = 0.0;
            }
            Point pt = new Point();
            pt.x = x;
            pts.Add(pt);

            return true;

        }

        private bool ReadLines(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return true;
            }
            string[] words = str.Split();
            int type, p1, p2;

            if (words.Length != 3)
            {
                sparta.DumpError("Incorrect line format in surf file");
            }
            type = 1;
            p1 = int.Parse(words[1]);
            p2 = int.Parse(words[2]);
            if (p1 < 1 || p1 > npoint || p2 < 1 || p2 > npoint || p1 == p2)
            {
                sparta.DumpError("Invalid point index in line");
            }
            Line line = new Line();
            line.type = type;
            line.mask = 1;
            line.p1 = p1 - 1;
            line.p2 = p2 - 1;
            lines.Add(line);
            return true;
        }

        private void Header()
        {
            StreamReader sr = new StreamReader(fp);

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                if (line.StartsWith("#"))
                {
                    continue;
                }
                if (line.EndsWith("points"))
                {
                    npoint = ParseInt(line);
                }
                if (line.EndsWith("lines"))
                {
                    nline = ParseInt(line);
                    break;
                }
            }

        }

        private int ParseInt(string line)
        {
            string[] strs = Regex.Split(line, @"\s+");
            return int.Parse(strs[0]);
        }
    }
}