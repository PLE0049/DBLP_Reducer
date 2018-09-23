using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using dblp_reducer_src.Model;

namespace dblp_reducer_src
{
    public class GmlExporter
    {
        
        public bool Export( string fileName , List<Model.Author> AuthorsList , int[,] Matrix, Dictionary<int, int> IndexToNodeDict)
        {
            using (StreamWriter file = new StreamWriter(fileName))
            {
                WriteHeader(file);
                WriteNodes(file, AuthorsList);
                WriteEdge(file, Matrix, IndexToNodeDict);
                WriteFooter(file);
            }

            return true;
        }

        public bool Export(string fileName, List<Model.Author> AuthorsList, int[,] Matrix)
        {
            using (StreamWriter file = new StreamWriter(fileName))
            {
                WriteHeader(file);
                WriteNodes(file, AuthorsList);
                WriteEdge(file, Matrix);
                WriteFooter(file);
            }

            return true;
        }

        private void WriteEdge(StreamWriter file, int[,] Matrix, Dictionary<int, int> IndexToNodeDict)
        {
            for(int i = 0; i < Matrix.GetLength(0); i++)
            {
                for (int j = i; j < Matrix.GetLength(1); j++)
                {
                    if(Matrix[i,j] > 0)
                    {
                        file.WriteLine("  edge");
                        file.WriteLine("  [");
                        file.WriteLine("    source " + IndexToNodeDict[i]);
                        file.WriteLine("    target " + IndexToNodeDict[j]);
                        file.WriteLine("    value " + Matrix[i, j]);
                        file.WriteLine("  ]");
                    }
                }
            }
        }

        private void WriteEdge(StreamWriter file, int[,] Matrix)
        {
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                for (int j = i; j < Matrix.GetLength(1); j++)
                {
                    if (Matrix[i, j] > 0)
                    {
                        file.WriteLine("  edge");
                        file.WriteLine("  [");
                        file.WriteLine("    source " + i);
                        file.WriteLine("    target " + j);
                        file.WriteLine("    value " + Matrix[i, j]);
                        file.WriteLine("  ]");
                    }
                }
            }
        }

        private void WriteNodes(StreamWriter file , List<Model.Author> AuthorsList)
        {
            foreach(Author a in AuthorsList)
            {
                file.WriteLine("  node");
                file.WriteLine("  [");
                file.WriteLine("    id " + a.Id);
                file.WriteLine("    label \"" + a.Name + "\"");
                file.WriteLine("  ]");
            }

        }

        private void WriteHeader(StreamWriter file)
        {
            DateTime dateValue = DateTime.Now;
            file.WriteLine("Creator Jakub Plesnik on "+ dateValue.ToString("f"));
            file.WriteLine("graph");
            file.WriteLine("[");
            file.WriteLine("  directed 0");
        }

        private void WriteFooter(StreamWriter file)
        {
            file.WriteLine("]");
        }
    }
}
