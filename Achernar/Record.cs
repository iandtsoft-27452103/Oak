using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Achernar
{
    internal class Record
    {
        public string[] players = new string[2];
        public string[] str_moves;
        public short[] moves;
        public int winner;
        public int ply;
    }
}
