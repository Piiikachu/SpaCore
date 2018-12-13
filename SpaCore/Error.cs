using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace SpaCore
{
    public  class Error :Exception
    {
        public static void OutputStackTrace()
        {
            Debug.WriteLine(new string('*', 78));
            StackTrace st = new StackTrace();
            StackFrame[] sfs = st.GetFrames();
            int i = 0;
            foreach(StackFrame sf in sfs)
            {
                
                MethodBase mb = sf.GetMethod();
                Debug.WriteLine("[CALL STACK][{0}]: {1}.{2}", i, mb.DeclaringType.FullName, mb.Name);
                i++;
            }

        }
    }
}
