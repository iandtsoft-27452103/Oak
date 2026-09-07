using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Achernar
{
    public class TT2
    {
        public Dictionary<ulong, int> value = new Dictionary<ulong, int>();
        public Dictionary<ulong, short> color = new Dictionary<ulong, short>();
        //public Dictionary<ulong, bool> is_check = new Dictionary<ulong, bool>();
        public Dictionary<ulong, short> move = new Dictionary<ulong, short>();// 要不要を後で精査する。
        public void Store(ulong k, int v, short c, short m)
        {
            value[k] = v;
            color[k] = c;
            move[k] = m;
        }
    }
}
