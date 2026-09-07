using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Achernar.Board;
using static Achernar.Common;
using static Achernar.Evaluate;
using static Achernar.Sort;
using static Achernar.MakeMove;
using static Achernar.IO;

namespace Achernar
{
    internal class Teacher
    {
        public static void MakeTeacherData(int file_number, int num_games)
        {
            //Common.Init();
            //Hash.IniRand(5489U);
            //Hash.IniRandomTable();

            const int ply_start = 8;
            const int ply_limit = 320;

            StreamWriter sw = OpenStreamWriter("teachers" + file_number.ToString() + ".txt");
            List<short> li_moves = new List<short>();
            Board bt = new Board();
            short color;
            for (int i = 0; i < num_games; i++)
            {
                Random r = new Random();
                int limit = r.Next(ply_start + 1, ply_limit);
                li_moves.Clear();
                bt.Init();
                color = 0;
                for (short j = 0; j < limit; j++)
                {
                    List<short> moves = GenMoves(bt, color);
                    Random r2 = new Random();
                    int index = r2.Next(0, moves.Count);
                    short move = moves[index];
                    Do(ref bt, move, color, (short)(j + 1));
                    li_moves.Add(move);
                    color ^= 1;
                }

                string str_out = "";
                for (int j = 0; j < NSquare; j++)
                {
                    short sq = bt.board[j];
                    str_out += sq.ToString();
                }
                int idx = li_moves.Count;
                int counter = 0;
                while (counter < 40)
                {
                    short move;
                    if (idx < 0)
                    {
                        str_out += "," + NSquare.ToString();
                    }
                    else
                    {
                        move = li_moves[--idx];
                        str_out += "," + move.ToString();
                    }
                    counter++;
                }
                str_out += ",";
                str_out += color.ToString();
                sw.WriteLine(str_out);
            }

            sw.Close();
        }

        private static List<short> GenMoves(Board bt, short color)
        {
            List<short> moves = new List<short>();
            for (int i = 0; i < bt.pos_empty.Count; i++)
            {
                short sq = bt.pos_empty[i];

                // ※自殺手と連続コウのチェック処理を入れる
                if (!bt.IsMoveValid(bt, sq, color))
                {
                    continue;
                }
                else
                {
                    moves.Add(sq);
                }
            }
            return moves;
        }
    }
}
