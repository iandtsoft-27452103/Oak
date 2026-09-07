using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Achernar.MakeMove;
using static Achernar.Feature2;
using System.Reflection.Metadata;
using System.Drawing;

namespace Achernar
{
    internal class Test
    {
        public void TestMakeMove()
        {
            Common.Init();
            Hash.IniRand(5489U);
            Hash.IniRandomTable();
            List<Record> records = new List<Record>();
            records = IO.ReadRecordFile("test_records.txt");
            List<int> li = new List<int>();

            for (int i = 0; i < records.Count; i++)
            {
                if (i == 14 || records[i].str_moves.Length > 250)
                {
                    string s = "record_no = " + (i+1).ToString();
                    s += ", ●" + records[i].players[0];
                    s += ", ○" + records[i].players[1];
                    Console.WriteLine(s);
                    li.Add(i);
                }
            }

            Board bt = new Board();
            short color;
            for (int i = 0;i < li.Count;i++)
            {
                Record record = records[li[i]];
                bt.Init();
                color = 0;
                for (short j = 0; j < record.moves.Length; j++)
                {
                    short move = record.moves[j];

                    if (i == 12 && j == 297)
                    {
                        int a = 0;// トリによって自分の駄目を復活させる処理から再開する
                    }

                    Do(ref bt, move, color, (short)(j + 1));
                    if (i == 12 && j == 297)
                    {
                        UnDo(ref bt, move, color, (short)(j + 1));
                        OutBoard(bt);

                        /*int cnt = 0;
                        for (int k = 0; k < 256; k++)
                        {
                            if (bt.dame_sq[1, k].Contains(120))
                            {
                                cnt++;
                            }  
                        }
                        for (int k = 0; k < bt.dame_sq[1,61].Count; k++)
                        {
                            Console.Write(bt.dame_sq[1, 61][k]);
                            Console.Write(",");
                        }*/
                        //Console.WriteLine(cnt);
                        return;
                    }
                    color ^= 1;
                }
            }
        }

        public void TestEvaluate0()
        {
            //Common.Init();
            //Hash.IniRand(5489U);
            //Hash.IniRandomTable();
            List<Record> records = new List<Record>();
            records = IO.ReadRecordFile("test_records.txt");
            Evaluate eval = new Evaluate();
            //eval.RandomInit();
            //eval.SaveFV();
            //return;
            eval.LoadFV();
            Record record = records[0];
            int limit = record.str_moves.Length;
            List<short> li_b = new List<short>();
            List<short> li_w = new List<short>();
            List<int> li_value = new List<int>();
            Board bt = new Board();
            bt.Init();
            short color;
            short prev_move = 361;

            List<short> bk_li_b0 = new List<short>();
            List<short> bk_li_w0 = new List<short>();
            List<short> bk_li_b1 = new List<short>();
            List<short> bk_li_w1 = new List<short>();
            int error_num = 0;

            color = 0;
            for (short i = 0; i < limit; i++)
            {
                short move = record.moves[i];
                if (i > 0)
                {
                    prev_move = record.moves[i - 1];
                }

                if (i == 49)
                {
                    int aaaaaaa = 0;
                }

                Do(ref bt, move, color, (short)(i + 1));

                if (bt.tori_flag[i])
                {
                    int aaaaa = 0;
                }

                if (i == 49)
                {
                    //li_b.Clear();
                    //li_w.Clear();
                    //break;
                    int mmhe = 0;
                }

                //li_b.Clear();
                //li_w.Clear();
                eval.MakeList(bt, ref li_b, ref li_w);

                /*if (i == 47 || i == 48)
                {
                    for (int j = 0; j < li_b.Count; j++)
                    {
                        bk_li_b0.Add(li_b[j]);
                    }
                    for (int j = 0;j < li_w.Count; j++)
                    {
                        bk_li_w0.Add(li_w[j]);
                    }
                    int xxxxxx = 0;
                }*/

                //if (bt.tori_flag[49] == true)
                /*if (i == 49)
                {
                    for (int j = 0; j < li_b.Count; j++)
                    {
                        bk_li_b1.Add(li_b[j]);
                    }
                    for (int j = 0; j < li_w.Count; j++)
                    {
                        bk_li_w1.Add(li_w[j]);
                    }

                    Console.WriteLine("[ bk_li_b0 ]");
                    for (int j = 0; j < bk_li_b0.Count; j++)
                    {
                        Console.WriteLine(bk_li_b0[j].ToString());
                    }
                    Console.WriteLine("[ bk_li_w0 ]");
                    for (int j = 0; j < bk_li_w0.Count; j++)
                    {
                        Console.WriteLine(bk_li_w0[j].ToString());
                    }
                    Console.WriteLine("[ bk_li_b1 ]");
                    for (int j = 0; j < bk_li_b1.Count; j++)
                    {
                        Console.WriteLine(bk_li_b1[j].ToString());
                    }
                    Console.WriteLine("[ bk_li_w1 ]");
                    for (int j = 0; j < bk_li_w1.Count; j++)
                    {
                        Console.WriteLine(bk_li_w1[j].ToString());
                    }
                    return;
                }*/

                if (i == 47)
                {
                    int ssssccccde = 0;
                }

                Console.WriteLine("ply=" + (i + 1).ToString());
                int value = eval.EvaluateAll(bt);// ply <= 8の時は全計算するのが無難
                li_value.Add(value);
                Console.WriteLine("ply=" + (i + 1).ToString());
                Console.WriteLine("[]");

                if (i > 8)
                {
                    int value2 = eval.EvaluateDiff(bt, color, move, prev_move, (short)li_value[i - 1], ref li_b, ref li_w);
                    int value3 = Math.Abs(value - value2);
                    if (value3 > 1)
                    {
                        Console.WriteLine("Error: " + (i + 1).ToString() + " " + value.ToString() + " " + value2.ToString());
                        error_num++;
                        break;
                    }
                }
                li_b.Clear();
                li_w.Clear();

                if (bt.ply == 49)
                {
                    //break;
                }
                color ^= 1;
            }
            //eval.MakeList(bt, ref li_b, ref li_w);
            //int value = eval.EvaluateAll(bt);
            //int value2 = eval.EvaluateDiff(bt, 1, 134, 171, 23, ref li_b, ref li_w);
            //eval.SaveFV();
            Console.WriteLine("error_num = " + error_num.ToString());
        }


        public void TestEvaluate()
        {
            //Common.Init();
            //Hash.IniRand(5489U);
            //Hash.IniRandomTable();
            List<Record> records = new List<Record>();
            records = IO.ReadRecordFile("test_records.txt");
            Evaluate eval = new Evaluate();
            //eval.RandomInit();
            eval.LoadFV();
            Record record = records[0];
            int limit = record.str_moves.Length;
            List<short> li_b = new List<short>();
            List<short> li_w = new List<short>();
            List<int> li_value = new List<int>();
            Board bt = new Board();
            bt.Init();
            short color;
            short prev_move = 361;

            List<short> bk_li_b0 = new List<short>();
            List<short> bk_li_w0 = new List<short>();
            List<short> bk_li_b1 = new List<short>();
            List<short> bk_li_w1 = new List<short>();
            int error_num = 0;

            color = 0;
            for (short i = 0; i < limit; i++)
            {
                short move = record.moves[i];
                if (i > 0)
                {
                    prev_move = record.moves[i - 1];
                }

                if (i == 49)
                {
                    int aaaaaaa = 0;
                }

                Do(ref bt, move, color, (short)(i + 1));

                if (bt.tori_flag[i])
                {
                    int aaaaa = 0;
                }

                if (i == 49)
                {
                    //li_b.Clear();
                    //li_w.Clear();
                    //break;
                    int mmhe = 0;
                }

                //li_b.Clear();
                //li_w.Clear();
                eval.MakeList(bt, ref li_b, ref li_w);

                /*if (i == 47 || i == 48)
                {
                    for (int j = 0; j < li_b.Count; j++)
                    {
                        bk_li_b0.Add(li_b[j]);
                    }
                    for (int j = 0;j < li_w.Count; j++)
                    {
                        bk_li_w0.Add(li_w[j]);
                    }
                    int xxxxxx = 0;
                }*/

                //if (bt.tori_flag[49] == true)
                /*if (i == 49)
                {
                    for (int j = 0; j < li_b.Count; j++)
                    {
                        bk_li_b1.Add(li_b[j]);
                    }
                    for (int j = 0; j < li_w.Count; j++)
                    {
                        bk_li_w1.Add(li_w[j]);
                    }

                    Console.WriteLine("[ bk_li_b0 ]");
                    for (int j = 0; j < bk_li_b0.Count; j++)
                    {
                        Console.WriteLine(bk_li_b0[j].ToString());
                    }
                    Console.WriteLine("[ bk_li_w0 ]");
                    for (int j = 0; j < bk_li_w0.Count; j++)
                    {
                        Console.WriteLine(bk_li_w0[j].ToString());
                    }
                    Console.WriteLine("[ bk_li_b1 ]");
                    for (int j = 0; j < bk_li_b1.Count; j++)
                    {
                        Console.WriteLine(bk_li_b1[j].ToString());
                    }
                    Console.WriteLine("[ bk_li_w1 ]");
                    for (int j = 0; j < bk_li_w1.Count; j++)
                    {
                        Console.WriteLine(bk_li_w1[j].ToString());
                    }
                    return;
                }*/

                if (i ==100)
                {
                    int ssssccccde = 0;
                }

                Console.WriteLine("ply=" + (i + 1).ToString());
                int value = eval.EvaluateAll(bt);// ply <= 8の時は全計算するのが無難
                li_value.Add(value);
                Console.WriteLine("ply=" + (i + 1).ToString());
                Console.WriteLine("value = " + value.ToString());

                if (i > 8)
                {
                    int value2 = eval.EvaluateDiff(bt, color, move, (short)li_value[i - 1], ref li_b, ref li_w);
                    int value3 = Math.Abs(value - value2);
                    if (value3 > 1)
                    {
                        Console.WriteLine("Error: " + (i + 1).ToString() + " " + value.ToString() + " " + value2.ToString());
                        error_num++;
                        //break;
                    }
                }
                li_b.Clear();
                li_w.Clear();

                if (bt.ply == 49)
                {
                    //break;
                }
                color ^= 1;
            }
            //eval.MakeList(bt, ref li_b, ref li_w);
            //int value = eval.EvaluateAll(bt);
            //int value2 = eval.EvaluateDiff(bt, 1, 134, 171, 23, ref li_b, ref li_w);
            //eval.SaveFV();
            Console.WriteLine("error_num = " + error_num.ToString());
        }

        public void TestEvaluate2()
        {
            //Common.Init();
            //Hash.IniRand(5489U);
            //Hash.IniRandomTable();
            List<Record> records = new List<Record>();
            records = IO.ReadRecordFile("test_records.txt");
            Evaluate2 eval = new Evaluate2();
            //eval.RandomInit();
            eval.LoadFV();
            Record record = records[0];
            int limit = record.str_moves.Length;
            List<short> li_b = new List<short>();
            List<short> li_w = new List<short>();
            List<int> li_value = new List<int>();
            Board bt = new Board();
            bt.Init();
            short color;
            short prev_move = 361;

            List<short> bk_li_b0 = new List<short>();
            List<short> bk_li_w0 = new List<short>();
            List<short> bk_li_b1 = new List<short>();
            List<short> bk_li_w1 = new List<short>();

            color = 0;
            for (short i = 0; i < limit; i++)
            {
                short move = record.moves[i];
                if (i > 0)
                {
                    prev_move = record.moves[i - 1];
                }

                Do(ref bt, move, color, (short)(i + 1));

                eval.MakeList(bt, ref li_b, ref li_w);

                Console.WriteLine("ply=" + (i + 1).ToString());
                int record_value = eval.EvaluateAll(bt);// ply <= 8の時は全計算するのが無難
                if (color == 1)
                    record_value = -record_value;
                li_value.Add(record_value);
                Console.WriteLine("ply=" + (i + 1).ToString() + ", record_value=" + record_value.ToString());

                /*UnDo(ref bt, move, color, (short)(i + 1));

                li_b.Clear();
                li_w.Clear();

                List<short> legal_move_list = new List<short>();
                for (short j = 0; j < bt.pos_empty.Count; j++)
                {
                    if (bt.IsMoveValid(bt, i, color) && j != move)
                        legal_move_list.Add(j);
                }

                List<double> values = new List<double>();
                int rank = 1;
                int max_value = record_value;
                bool flag = false;
                for (short j = 0; j < legal_move_list.Count; j++)
                {
                    short m = legal_move_list[j];
                    if (move == m)
                    {
                        continue;
                    }
                    Do(ref bt, m, color, (short)(i + 1));
                    int value = eval.EvaluateAll(bt);
                    if (color == 1)
                        value = -value;
                    values.Add(value);
                    if (flag == false && value > record_value)
                    {
                        rank++;
                        max_value = value;
                        flag = true;
                    }

                    if (flag == true && value > max_value)
                    {
                        rank++;
                        max_value = value;
                    }

                    UnDo(ref bt, m, color, (short)(i + 1));
                    li_b.Clear();
                    li_w.Clear();
                }*/
                //Console.WriteLine("ply=" + (i + 1).ToString() + ", record_rank = " + rank.ToString() + ", max_value = " + max_value.ToString());
                //Do(ref bt, move, color, (short)(i + 1));
                color ^= 1;
            }
        }

        public void TestEvaluate3()
        {
            //Common.Init();
            //Hash.IniRand(5489U);
            //Hash.IniRandomTable();
            List<Record> records = new List<Record>();
            records = IO.ReadRecordFile("test_records.txt");
            Evaluate3 eval = new Evaluate3();
            //eval.RandomInit();
            eval.LoadFV();
            Record record = records[0];
            int limit = record.str_moves.Length;
            List<short> li_b = new List<short>();
            List<short> li_w = new List<short>();
            List<double> li_value = new List<double>();
            Board bt = new Board();
            bt.Init();
            short color;
            short prev_move = 361;

            List<short> bk_li_b0 = new List<short>();
            List<short> bk_li_w0 = new List<short>();
            List<short> bk_li_b1 = new List<short>();
            List<short> bk_li_w1 = new List<short>();

            color = 0;
            for (short i = 0; i < limit; i++)
            {
                short move = record.moves[i];
                if (i > 0)
                {
                    prev_move = record.moves[i - 1];
                }

                Do(ref bt, move, color, (short)(i + 1));

                eval.MakeList(bt, ref li_b, ref li_w);

                Console.WriteLine("ply=" + (i + 1).ToString());
                double record_value = eval.EvaluateAll(bt);// ply <= 8の時は全計算するのが無難
                if (color == 1)
                    record_value = -record_value;
                li_value.Add(record_value);
                Console.WriteLine("ply=" + (i + 1).ToString() + ", record_value=" + record_value.ToString());

                /*UnDo(ref bt, move, color, (short)(i + 1));

                li_b.Clear();
                li_w.Clear();

                List<short> legal_move_list = new List<short>();
                for (short j = 0; j < bt.pos_empty.Count; j++)
                {
                    if (bt.IsMoveValid(bt, i, color) && j != move)
                        legal_move_list.Add(j);
                }

                List<double> values = new List<double>();
                int rank = 1;
                int max_value = record_value;
                bool flag = false;
                for (short j = 0; j < legal_move_list.Count; j++)
                {
                    short m = legal_move_list[j];
                    if (move == m)
                    {
                        continue;
                    }
                    Do(ref bt, m, color, (short)(i + 1));
                    int value = eval.EvaluateAll(bt);
                    if (color == 1)
                        value = -value;
                    values.Add(value);
                    if (flag == false && value > record_value)
                    {
                        rank++;
                        max_value = value;
                        flag = true;
                    }

                    if (flag == true && value > max_value)
                    {
                        rank++;
                        max_value = value;
                    }

                    UnDo(ref bt, m, color, (short)(i + 1));
                    li_b.Clear();
                    li_w.Clear();
                }*/
                //Console.WriteLine("ply=" + (i + 1).ToString() + ", record_rank = " + rank.ToString() + ", max_value = " + max_value.ToString());
                //Do(ref bt, move, color, (short)(i + 1));
                color ^= 1;
            }
        }


        public void TestTransProbs()
        {
            //Common.Init();
            //Hash.IniRand(5489U);
            //Hash.IniRandomTable();
            List<Record> records = new List<Record>();
            records = IO.ReadRecordFile("test_records.txt");
            Feature2 f2 = new Feature2();
            f2.LoadFV();
            Record record = records[0];
            int limit = record.str_moves.Length;
            List<short> li_b = new List<short>();
            List<short> li_w = new List<short>();
            List<int> li_value = new List<int>();
            Board bt = new Board();
            bt.Init();
            short color;
            short prev_move = 361;

            List<short> bk_li_b0 = new List<short>();
            List<short> bk_li_w0 = new List<short>();
            List<short> bk_li_b1 = new List<short>();
            List<short> bk_li_w1 = new List<short>();

            color = 0;
            for (short i = 0; i < limit; i++)
            {
                short move = record.moves[i];
                if (i > 0)
                {
                    prev_move = record.moves[i - 1];
                }

                Do(ref bt, move, color, (short)(i + 1));
                
                List<int> li = f2.MakeInputFeature(bt, color);
                double record_value = f2.Calc(li);

                UnDo(ref bt, move, color, (short)(i + 1));

                li.Clear();

                List<short> legal_move_list = new List<short>();
                for (short j = 0; j < bt.pos_empty.Count; j++)
                {
                    if (bt.IsMoveValid(bt, i, color) && j != move)
                        legal_move_list.Add(j);
                }

                List<double> values = new List<double>();
                int rank = 1;
                for (short j = 0; j < legal_move_list.Count; j++)
                {
                    short m = legal_move_list[j];
                    if (move == m)
                    {
                        continue;
                    }
                    Do(ref bt, m, color, (short)(i + 1));
                    li = f2.MakeInputFeature(bt, color);
                    double value = f2.Calc(li);
                    values.Add(value);
                    if (value > record_value)
                    {
                        rank++;
                    }
                    li.Clear();
                    UnDo(ref bt, m, color, (short)(i + 1));
                }

                Console.WriteLine("ply=" + (i + 1).ToString() + ", record_rank = " + rank.ToString());

                color ^= 1;
            }
        }

        private void OutBoard(Board bt)
        {
            string str_out = "";
            int cnt = 0;
            for (int i = 0; i < Common.NSquare; i++)
            {
                switch(bt.board[i])
                {
                    case 0:
                        str_out += "○ ";
                        break;
                    case 1:
                        str_out += "● ";
                        break;
                    case 2:
                        str_out += "+ ";
                        break;
                }
                cnt++;
                if (cnt == Common.NSide)
                {
                    str_out += "\n";
                    cnt = 0;
                }
            }

            str_out = str_out + "\n";
            str_out += "黒のアゲハマ" + bt.agehama[0].ToString() + "\n";
            str_out = str_out + "\n";
            str_out += "白のアゲハマ" + bt.agehama[1].ToString() + "\n";

            Console.WriteLine(str_out);
        }
    }
}
