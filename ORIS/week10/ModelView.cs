using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORIS.week10
{
    public class ModelView
    {
        public string FilePath { get; private set; }

        public Dictionary<string, string> Properties { get; private set; }

        public ModelView(string filePath, Dictionary<string, string> properties)
        {
            FilePath = filePath;
            Properties = properties;
        }
    }
}
