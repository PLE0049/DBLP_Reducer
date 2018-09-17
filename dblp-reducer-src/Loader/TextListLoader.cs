using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dblp_reducer_src.Loader
{
    public static class TextListLoader
    {
        public static List<string> LoadAuthors(string fileName)
        {
            List<string> AuthorsName = new List<string>();
            var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line != "" && line[0] != '/')
                        AuthorsName.Add(line);
                }
            }

            return AuthorsName;
        }
    }
}
