using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Achernar.Record;

namespace Achernar
{
    internal class Position
    {
        public int position_number;
        public int record_number;
        public int ply;
        public List<Position> Alloc(List<Record> records)
        {
            List<Position> positions = new List<Position>();
            int pos_num = 0;
            int rec_num = 0;
            foreach (Record record in records)
            {
                for (int ply = 0; ply < record.str_moves.Length; ply++)
                {
                    Position pos = new Position();
                    pos.position_number = pos_num++;
                    pos.record_number = rec_num;
                    pos.ply = ply;
                    positions.Add(pos);
                }
                rec_num++;
            }
            return positions;
        }
    }
}
