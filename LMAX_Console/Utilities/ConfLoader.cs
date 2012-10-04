using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Utilites
{
    class ConfLoader
    {
        public Dictionary<String, String> loadConfFromFile(String FileName)
        {
            Dictionary<String, String> config = new Dictionary<String, String>();
            String line = null;
            StreamReader sr = new StreamReader(FileName);
            String key = null;
            String val = null;

            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                
                if (line.StartsWith("#")) continue;
                String[] par = line.Split('=');
                if (par != null && par.Length == 2)
                {
                    key = par[0].Trim();
                    val = par[1].Trim();
                }
                if (key.Length != 0 && val.Length != 0) 
                {
                    config.Add(key, val);
                }
                key = "";
            }
            return config;
        }
    }
}
