using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Achernar.Board;
using static Achernar.Common;
using System.Runtime.Intrinsics.X86;
using System.Reflection;

// 3つの石の組み合わせを評価したが、計算が重すぎるので、しばらく放置する...(^^;)
namespace Achernar
{
    public class Evaluate
    {
        const short Black = 0;
        const short White = 1;
        const short Empty = 2;
        const int fv_scale = 32;
        const string file_name_pb = "fv_pb.bin";
        const string file_name_pw = "fv_pw.bin";
        const string file_name_bbb = "fv_bbb.bin";
        const string file_name_bww = "fv_bww.bin";
        const string file_name_wbb = "fv_wbb.bin";
        const string file_name_www = "fv_www.bin";
        public short[,] fv_pb = new short[NSquare, NSquare];
        public short[,] fv_pw = new short[NSquare, NSquare];
        public short[,,] fv_bbb = new short[NSquare, NSquare, NSquare];
        public short[,,] fv_bww = new short[NSquare, NSquare, NSquare];
        public short[,,] fv_wbb = new short[NSquare, NSquare, NSquare];
        public short[,,] fv_www = new short[NSquare, NSquare, NSquare];

        public void RandomInit()
        {
            Random r = new Random();
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    fv_pb[i, j] = (short)r.Next(64, 128);
                    fv_pw[i, j] = (short)r.Next(64, 128);
                    for (int  k = 0; k < NSquare; k++)
                    {
                        fv_bbb[i, j, k] = (short)r.Next(1, 10);
                        fv_bww[i, j, k] = (short)r.Next(1, 10);
                        fv_wbb[i, j, k] = (short)r.Next(1, 10);
                        fv_www[i, j, k] = (short)r.Next(1, 10);
                    }
                }
            }
        }

        public void SaveFV()
        {
            BinaryWriter bw = new BinaryWriter(File.Open(file_name_pb, FileMode.Create));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    bw.Write(fv_pb[i, j]);
                }
            }
            bw.Close();
            bw = new BinaryWriter(File.Open(file_name_pw, FileMode.Create));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    bw.Write(fv_pw[i, j]);
                }
            }
            bw.Close();
            bw = new BinaryWriter(File.Open(file_name_bbb, FileMode.Create));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    for (int k = 0; k < NSquare; k++)
                    {
                        bw.Write(fv_bbb[i, j, k]);
                    }
                }
            }
            bw.Close();
            bw = new BinaryWriter(File.Open(file_name_bww, FileMode.Create));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    for (int k = 0; k < NSquare; k++)
                    {
                        bw.Write(fv_bww[i, j, k]);
                    }
                }
            }
            bw.Close();
            bw = new BinaryWriter(File.Open(file_name_wbb, FileMode.Create));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    for (int k = 0; k < NSquare; k++)
                    {
                        bw.Write(fv_wbb[i, j, k]);
                    }
                }
            }
            bw.Close();
            bw = new BinaryWriter(File.Open(file_name_www, FileMode.Create));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    for (int k = 0; k < NSquare; k++)
                    {
                        if (fv_www[i, j, k] == 0)
                        {
                            int jjj = 0;
                        }
                        bw.Write(fv_www[i, j, k]);
                    }
                }
            }
            bw.Close();
        }
        public void LoadFV()
        {
            BinaryReader br = new BinaryReader(File.Open(file_name_pb, FileMode.Open));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    fv_pb[i, j] = br.ReadInt16();
                }
            }
            br.Close();
            br = new BinaryReader(File.Open(file_name_pw, FileMode.Open));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    fv_pw[i, j] = br.ReadInt16();
                }
            }
            br.Close();
            br = new BinaryReader(File.Open(file_name_bbb, FileMode.Open));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    for (int k = 0; k < NSquare; k++)
                    {
                        fv_bbb[i, j, k] = br.ReadInt16();
                    }
                }
            }
            br.Close();
            br = new BinaryReader(File.Open(file_name_bww, FileMode.Open));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    for (int k = 0; k < NSquare; k++)
                    {
                        fv_bww[i, j, k] = br.ReadInt16();
                    }
                }
            }
            br.Close();
            br = new BinaryReader(File.Open(file_name_wbb, FileMode.Open));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    for (int k = 0; k < NSquare; k++)
                    {
                        fv_wbb[i, j, k] = br.ReadInt16();
                    }
                }
            }
            br.Close();
            br = new BinaryReader(File.Open(file_name_www, FileMode.Open));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    for (int k = 0; k < NSquare; k++)
                    {
                        fv_www[i, j, k] = br.ReadInt16();
                    }
                }
            }
            br.Close();
        }
        public void MakeList(Board bt, ref List<short> li_b, ref List<short> li_w)
        {
            List<short> temp_li = bt.board.ToList();
            while (temp_li.Contains(Black))
            {
                int index = temp_li.IndexOf(Black);
                temp_li[index] = Empty;
                li_b.Add((short)index);
            }
            while (temp_li.Contains(White))
            {
                int index = temp_li.IndexOf(White);
                temp_li[index] = 0;
                li_w.Add((short)index);
            }
        }
        public int EvaluateAll(Board bt)
        {
            int i, j, k;
            int score = 0;
            short temp_color = 0;

            //string AppPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            //string FilePath = AppPath + "\\debug_log000.txt";
            //StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8);

            //string debug_s = "";

            for (i = 1; i <= bt.ply; i++)
            {
                score += temp_color == 0 ? fv_pb[i - 1, bt.current_moves[i - 1]] : -fv_pw[i - 1, bt.current_moves[i - 1]];
                temp_color ^= 1;
            }

            if (bt.ply <= 8)
            {
                //sw.Close();
                return score / fv_scale;
            }

            List<short> li_b = new List<short>();
            List<short> li_w = new List<short>();

            //sw.WriteLine("bbb\n");
            MakeList(bt, ref li_b, ref li_w);
            for (i = 0; i < li_b.Count; i++)
            {
                for (j = i + 1; j < li_b.Count; j++)
                {
                    for (k = j + 1; k < li_b.Count; k++)
                    {
                        //string s = li_b[i].ToString() + "," + li_b[j].ToString() + "," + li_b[k].ToString();
                        //if (bt.ply == 48)
                            //sw.WriteLine(s);
                        //Console.WriteLine(s);
                        //debug_s += s + " ";
                        score += fv_bbb[li_b[i], li_b[j], li_b[k]];
                    }
                }
            }
            //sw.WriteLine("wbb\n");
            for (i = 0; i < li_w.Count; i++)
            {
                for (j = 0; j < li_b.Count; j++)
                {
                    for (k = j + 1; k < li_b.Count; k++)
                    {
                        //string s = li_w[i].ToString() + "," + li_b[j].ToString() + "," + li_b[k].ToString();
                        //if (bt.ply == 48)
                            //sw.WriteLine(s);
                        //Console.WriteLine(s);
                        //debug_s += s + " ";
                        score += fv_wbb[li_w[i], li_b[j], li_b[k]];
                    }
                }
            }
           // sw.WriteLine("www\n");
            for (i = 0; i < li_w.Count; i++)
            {
                for (j = i + 1; j < li_w.Count; j++)
                {
                    for (k = j + 1; k < li_w.Count; k++)
                    {
                        //string s = li_w[i].ToString() + "," + li_w[j].ToString() + "," + li_w[k].ToString();
                        //if (bt.ply == 48)
                            //sw.WriteLine(s);
                        //Console.WriteLine(s);
                        //debug_s += s + " ";
                        score -= fv_www[li_w[i], li_w[j], li_w[k]];
                    }
                }
            }
            //sw.WriteLine("bww\n");
            for (i = 0; i < li_b.Count; i++)
            {
                for (j = 0; j < li_w.Count; j++)
                {
                    for (k = j + 1; k < li_w.Count; k++)
                    {
                        //string s = li_b[i].ToString() + "," + li_w[j].ToString() + "," + li_w[k].ToString();
                        //if (bt.ply == 48)
                            //sw.WriteLine(s);
                        //Console.WriteLine(s);
                        //debug_s += s + " ";
                        score -= fv_bww[li_b[i], li_w[j], li_w[k]];
                    }
                }
            }

            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine();
            //if (bt.ply == 48)
            //{
               // Console.WriteLine("London!");
                //Console.WriteLine(debug_s);
            //}
            //Console.WriteLine();

            //sw.Close();
            return score / fv_scale;
        }

        public int EvaluateDiff(Board bt, short color, short move, short prev_move, short prev_value, ref List<short> li_b, ref List<short> li_w)
        {
            int i, j, k, l, index;
            int score = prev_value * 32;

            score += color == 0 ? fv_pb[bt.ply - 1, move] : -fv_pw[bt.ply - 1, move];

            string AppPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FilePath = AppPath + "\\debug_log_diff.txt";
            StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8);

            /*if (bt.ply <= 8)
            {
                score += color == 0 ? fv_pb[bt.ply, move] : -fv_pw[bt.ply, move];
                return score / fv_scale;
            }*/

            if (color == 0)
            {
                index = li_b.IndexOf(move);
                for (i = 0; i < index; i++)
                {
                    for (j = i + 1; j < index; j++)
                    {
                        if (li_b[j] == move)
                            continue;
                        string s = li_b[i].ToString() + "," + li_b[j].ToString() + "," + move.ToString();
                        Console.WriteLine(s);
                        if (bt.ply == 49)
                            sw.WriteLine("bbb,"+s);
                        score += fv_bbb[li_b[i], li_b[j], move];
                    }
                }
                for (i = 0; i < index; i++)
                {
                    for (j = index + 1; j < li_b.Count; j++)
                    {
                        if (move == li_b[j])
                            continue;
                        string s = li_b[i].ToString() + "," + move.ToString() + "," + li_b[j].ToString();
                       Console.WriteLine(s);
                        if (bt.ply == 49)
                            sw.WriteLine("bbb,"+s);
                        score += fv_bbb[li_b[i], move, li_b[j]];
                    }
                }
                for (i = index + 1; i < li_b.Count; i++)
                {
                    for (j = i + 1; j < li_b.Count; j++)
                    {
                        if (li_b[i] == li_b[j])
                            continue;
                        string s = move.ToString() + "," + li_b[i].ToString() + "," + li_b[j].ToString();
                        Console.WriteLine(s);
                        if (bt.ply == 49)
                            sw.WriteLine("bbb," + s);
                        score += fv_bbb[move, li_b[i], li_b[j]];
                    }
                }
                for (i = 0; i < li_w.Count; i++)
                {
                    for (j = i + 1; j < li_w.Count; j++)
                    {
                        if (li_w[i] == li_w[j])
                            continue;
                        string s = move.ToString() + "," + li_w[i].ToString() + "," + li_w[j].ToString();
                        Console.WriteLine(s);
                        if (bt.ply == 49)
                            sw.WriteLine("bww," + s);
                        score -= fv_bww[move, li_w[i], li_w[j]];
                    }
                }
                for (i = 0; i < li_w.Count; i++)
                {
                    for (j = 0; j < index; j++)
                    {
                        if (li_b[j] == move)
                            continue;
                        string s = li_w[i].ToString() + "," + li_b[j].ToString() + "," + move.ToString();
                        Console.WriteLine(s);
                        if (bt.ply == 49)
                            sw.WriteLine("wbb," + s);
                        score += fv_wbb[li_w[i], li_b[j], move];
                    }
                    for (j = index; j < li_b.Count; j++)
                    {
                        if (move == li_b[j])
                            continue;
                        string s = li_w[i].ToString() + "," + move.ToString() + "," + li_b[j].ToString();
                        Console.WriteLine(s);
                        if (bt.ply == 49)
                            sw.WriteLine("wbb," + s);
                        score += fv_wbb[li_w[i], move, li_b[j]];
                    }
                }
            }
            else
            {
                index = li_w.IndexOf(move);
                for (i = 0; i < index; i++)
                {
                    for (j = i + 1; j < index; j++)
                    {
                        if (li_w[j] == move)
                            continue;
                        string s = li_w[i].ToString() + "," + li_w[j].ToString() + "," + move.ToString();
                        Console.WriteLine(s);
                        if (bt.ply == 49)
                            sw.WriteLine("www," + s);
                        score -= fv_www[li_w[i], li_w[j], move];
                    }
                }
                for (i = 0; i < index; i++)
                {
                    for (j = index + 1; j < li_w.Count; j++)
                    {
                        if (move == li_w[j])
                            continue;
                        string s = li_w[i].ToString() + "," + move.ToString() + "," + li_w[j].ToString();
                        Console.WriteLine(s);
                        if (bt.ply == 49)
                            sw.WriteLine("www," + s);
                        score -= fv_www[li_w[i], move, li_w[j]];
                    }
                }
                for (i = index + 1; i < li_w.Count; i++)
                {
                    for (j = i + 1; j < li_w.Count; j++)
                    {
                        if (li_w[i] == li_w[j])
                            continue;
                        string s = move.ToString() + "," + li_w[i].ToString() + "," + li_w[j].ToString();
                        Console.WriteLine(s);
                        if (bt.ply == 49)
                            sw.WriteLine("www," + s);
                        score -= fv_www[move, li_w[i], li_w[j]];
                    }
                }
                for (i = 0; i < li_b.Count; i++)
                {
                    for (j = i + 1; j < li_b.Count; j++)
                    {
                        if (li_b[i] == li_b[j])
                            continue;
                        string s = move.ToString() + "," + li_b[i].ToString() + "," + li_b[j].ToString();
                        Console.WriteLine(s);
                        if (bt.ply == 49)
                            sw.WriteLine("wbb," + s);
                        score += fv_wbb[move, li_b[i], li_b[j]];
                    }
                }
                for (i = 0; i < li_b.Count; i++)
                {
                    for (j = 0; j < index; j++)
                    {
                        if (move == li_w[j])
                            continue;
                        string s = li_b[i].ToString() + "," + move.ToString() + "," + li_w[j].ToString();
                        if (bt.ply == 49)
                            sw.WriteLine(s);
                        Console.WriteLine("bww," + s);
                        score -= fv_bww[li_b[i], li_w[j], move];
                    }
                    for (j = index; j < li_w.Count; j++)
                    {
                        if (move == li_w[j])
                            continue;
                        string s = li_b[i].ToString() + "," + move.ToString() + "," + li_w[j].ToString();
                        if (bt.ply == 49)
                            sw.WriteLine("bww," + s);
                        Console.WriteLine(s);
                        score -= fv_bww[li_b[i], move, li_w[j]];
                    }
                }
            }

            if (bt.tori_flag[bt.ply] == true)
            {
                //int opponent_color = color ^ 1;
                int a = 0;
                if (color == 0)
                {
                    for (i = 0; i < 4; i++)
                    {
                        if (bt.removed_seq_num[bt.ply, i] == seq_max)
                        {
                            break;
                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine();
                            //var temp_li_b = li_b.ToList();
                            index = li_b.IndexOf(move);
                            li_b.Remove(move);
                            for (j = 0; j < bt.removed_seq_sq[bt.ply, i].Count; j++)
                            {
                                //score -= fv_wbb[prev_move, ]
                                int index2;
                                int removed_stone = bt.removed_seq_sq[bt.ply, i][j];
                                for (k = 0; k < li_b.Count; k++)
                                {
                                    index2 = li_w.FindIndex(x => x > removed_stone);
                                    for (l = 0; l < index2; l++)
                                    {
                                        score += fv_bww[li_b[k], li_w[l], removed_stone];
                                        a++;
                                    }
                                    //index2 = li_w.FindIndex(x => x < removed_stone);
                                    for (l = index2; l < li_w.Count; l++)
                                    {
                                        score += fv_bww[li_b[k], removed_stone, li_w[l]];
                                        a++;
                                    }
                                }
                                for (k = 0; k < li_b.Count; k++)
                                {
                                    for (l = k + 1; l < li_b.Count; l++)
                                    {
                                        if (li_b[k] == li_b[l])
                                            continue;
                                        string s = removed_stone.ToString() + "," + li_b[k].ToString() + "," + li_b[l].ToString();
                                        Console.WriteLine(s);
                                        if(bt.ply == 49)
                                            sw.WriteLine("wbb," + s);
                                        score -= fv_wbb[removed_stone, li_b[k], li_b[l]];
                                        a++;
                                    }
                                }
                                index2 = li_w.FindIndex(x => x > removed_stone);
                                for (k = index2; k < li_w.Count; k++)
                                {
                                    for (l = k + 1; l < li_w.Count; l++)
                                    {
                                        if (li_w[k] == li_w[l])
                                            continue;
                                        string s = removed_stone.ToString() + "," + li_w[k].ToString() + "," + li_w[l].ToString();
                                        Console.WriteLine(s);
                                        if (bt.ply == 49)
                                            sw.WriteLine("www," + s);
                                        score += fv_www[removed_stone, li_w[k], li_w[l]];
                                        a++;
                                    }
                                }
                                index2 = li_w.FindIndex(x => x < removed_stone);
                                int index3 = li_w.FindIndex(x => x > removed_stone);
                                for (k = index2; k < index3; k++)
                                {
                                    for (l = index3; l < li_w.Count; l++)
                                    {
                                        //if (removed_stone == li_w[l])
                                            //continue;
                                        string s = li_w[k].ToString() + "," + removed_stone.ToString() + "," + li_w[l].ToString();
                                        Console.WriteLine(s);
                                        if (bt.ply == 49)
                                            sw.WriteLine("www," + s);
                                        score += fv_www[li_w[k], removed_stone, li_w[l]];
                                        a++;
                                    }
                                }
                                for (k = index2; k < index3; k++)
                                {
                                    for (l = k + 1; l < index3; l++)
                                    {
                                        //if (removed_stone == li_w[l])
                                            //continue;
                                        string s = li_w[k].ToString() + "," + li_w[l].ToString() + "," + removed_stone.ToString();
                                        Console.WriteLine(s);
                                        if (bt.ply == 49)
                                            sw.WriteLine("www," + s);
                                        score += fv_www[li_w[k], li_w[l], removed_stone];
                                        a++;
                                    }
                                }
                            }
                            li_b.Insert(index, move);
                        }
                    }
                }
                else
                {
                    for (i = 0; i < 4; i++)
                    {
                        if (bt.removed_seq_num[bt.ply, i] == seq_max)
                        {
                            break;
                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine();
                            //var temp_li_b = li_b.ToList();
                            index = li_w.IndexOf(move);
                            li_w.Remove(move);
                            for (j = 0; j < bt.removed_seq_sq[bt.ply, i].Count; j++)
                            {
                                //score -= fv_wbb[prev_move, ]
                                int index2;
                                int removed_stone = bt.removed_seq_sq[bt.ply, i][j];
                                for (k = 0; k < li_w.Count; k++)
                                {
                                    index2 = li_b.FindIndex(x => x > removed_stone);
                                    for (l = 0; l < index2; l++)
                                    {
                                        score -= fv_wbb[li_w[k], li_b[l], removed_stone];
                                        a++;
                                    }
                                    //index2 = li_w.FindIndex(x => x < removed_stone);
                                    for (l = index2; l < li_b.Count; l++)
                                    {
                                        score -= fv_wbb[li_w[k], removed_stone, li_b[l]];
                                        a++;
                                    }
                                }
                                for (k = 0; k < li_w.Count; k++)
                                {
                                    for (l = k + 1; l < li_w.Count; l++)
                                    {
                                        if (li_w[k] == li_w[l])
                                            continue;
                                        string s = removed_stone.ToString() + "," + li_w[k].ToString() + "," + li_w[l].ToString();
                                        Console.WriteLine(s);
                                        if (bt.ply == 49)
                                            sw.WriteLine("bww," + s);
                                        score += fv_bww[removed_stone, li_w[k], li_w[l]];
                                    }
                                }
                                index2 = li_b.FindIndex(x => x > removed_stone);
                                for (k = index2; k < li_b.Count; k++)
                                {
                                    for (l = k + 1; l < li_b.Count; l++)
                                    {
                                        if (li_b[k] == li_b[l])
                                            continue;
                                        string s = removed_stone.ToString() + "," + li_b[k].ToString() + "," + li_b[l].ToString();
                                        Console.WriteLine(s);
                                        if (bt.ply == 49)
                                            sw.WriteLine("bbb," + s);
                                        score -= fv_bbb[removed_stone, li_b[k], li_b[l]];
                                    }
                                }
                                index2 = li_b.FindIndex(x => x < removed_stone);
                                int index3 = li_b.FindIndex(x => x > removed_stone);
                                for (k = index2; k < index3; k++)
                                {
                                    for (l = index3; l < li_b.Count; l++)
                                    {
                                        //if (removed_stone == li_b[l])
                                            //continue;
                                        string s = li_b[k].ToString() + "," + removed_stone.ToString() + "," + li_b[l].ToString();
                                        Console.WriteLine(s);
                                        if (bt.ply == 49)
                                            sw.WriteLine("bbb," + s);
                                        score -= fv_bbb[li_b[k], removed_stone, li_b[l]];
                                    }
                                }
                                for (k = index2; k < index3; k++)
                                {
                                    for (l = k + 1; l < index3; l++)
                                    {
                                        //if (removed_stone == li_b[l])
                                            //continue;
                                        string s = li_b[k].ToString() + "," + li_b[l].ToString() + "," + removed_stone.ToString();
                                        Console.WriteLine(s);
                                        if (bt.ply == 49)
                                            sw.WriteLine("bbb," + s);
                                        score -= fv_bbb[li_b[k], li_b[l], removed_stone];
                                    }
                                }
                            }
                            li_w.Insert(index, move);
                        }
                    }

                }
            }
            sw.Close();
            //score = (prev_value * 32) - score;
            return score / fv_scale;
        }

        public int EvaluateDiff(Board bt, short color, short move, short prev_value, ref List<short> li_b, ref List<short> li_w)
        {
            int i, j, k, l, index;
            int score = prev_value * 32;

            score += color == 0 ? fv_pb[bt.ply - 1, move] : -fv_pw[bt.ply - 1, move];

            string AppPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FilePath = AppPath + "\\debug_log_diff.txt";
            StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8);

            /*if (bt.ply <= 8)
            {
                score += color == 0 ? fv_pb[bt.ply, move] : -fv_pw[bt.ply, move];
                return score / fv_scale;
            }*/

            if (color == 0)
            {
                index = li_b.IndexOf(move);
                for (i = 0; i < index; i++)
                {
                    for (j = i + 1; j < index; j++)
                    {
                        if (li_b[j] == move)
                            continue;
                        //string s = li_b[i].ToString() + "," + li_b[j].ToString() + "," + move.ToString();
                        //Console.WriteLine(s);
                        //if (bt.ply == 49)
                            //sw.WriteLine("bbb," + s);
                        score += fv_bbb[li_b[i], li_b[j], move];
                    }
                }
                for (i = 0; i < index; i++)
                {
                    for (j = index + 1; j < li_b.Count; j++)
                    {
                        if (move == li_b[j])
                            continue;
                        //string s = li_b[i].ToString() + "," + move.ToString() + "," + li_b[j].ToString();
                        //Console.WriteLine(s);
                        //if (bt.ply == 49)
                            //sw.WriteLine("bbb," + s);
                        score += fv_bbb[li_b[i], move, li_b[j]];
                    }
                }
                for (i = index + 1; i < li_b.Count; i++)
                {
                    for (j = i + 1; j < li_b.Count; j++)
                    {
                        if (li_b[i] == li_b[j])
                            continue;
                        //string s = move.ToString() + "," + li_b[i].ToString() + "," + li_b[j].ToString();
                        //Console.WriteLine(s);
                        //if (bt.ply == 49)
                            //sw.WriteLine("bbb," + s);
                        score += fv_bbb[move, li_b[i], li_b[j]];
                    }
                }
                for (i = 0; i < li_w.Count; i++)
                {
                    for (j = i + 1; j < li_w.Count; j++)
                    {
                        if (li_w[i] == li_w[j])
                            continue;
                        //string s = move.ToString() + "," + li_w[i].ToString() + "," + li_w[j].ToString();
                        //Console.WriteLine(s);
                        //if (bt.ply == 49)
                            //sw.WriteLine("bww," + s);
                        score -= fv_bww[move, li_w[i], li_w[j]];
                    }
                }
                for (i = 0; i < li_w.Count; i++)
                {
                    for (j = 0; j < index; j++)
                    {
                        if (li_b[j] == move)
                            continue;
                        //string s = li_w[i].ToString() + "," + li_b[j].ToString() + "," + move.ToString();
                        //Console.WriteLine(s);
                        //if (bt.ply == 49)
                            //sw.WriteLine("wbb," + s);
                        score += fv_wbb[li_w[i], li_b[j], move];
                    }
                    for (j = index; j < li_b.Count; j++)
                    {
                        if (move == li_b[j])
                            continue;
                        //string s = li_w[i].ToString() + "," + move.ToString() + "," + li_b[j].ToString();
                        //Console.WriteLine(s);
                        //if (bt.ply == 49)
                            //sw.WriteLine("wbb," + s);
                        score += fv_wbb[li_w[i], move, li_b[j]];
                    }
                }
            }
            else
            {
                index = li_w.IndexOf(move);
                for (i = 0; i < index; i++)
                {
                    for (j = i + 1; j < index; j++)
                    {
                        if (li_w[j] == move)
                            continue;
                        //string s = li_w[i].ToString() + "," + li_w[j].ToString() + "," + move.ToString();
                        //Console.WriteLine(s);
                        //if (bt.ply == 49)
                            //sw.WriteLine("www," + s);
                        score -= fv_www[li_w[i], li_w[j], move];
                    }
                }
                for (i = 0; i < index; i++)
                {
                    for (j = index + 1; j < li_w.Count; j++)
                    {
                        if (move == li_w[j])
                            continue;
                        //string s = li_w[i].ToString() + "," + move.ToString() + "," + li_w[j].ToString();
                        //Console.WriteLine(s);
                        //if (bt.ply == 49)
                            //sw.WriteLine("www," + s);
                        score -= fv_www[li_w[i], move, li_w[j]];
                    }
                }
                for (i = index + 1; i < li_w.Count; i++)
                {
                    for (j = i + 1; j < li_w.Count; j++)
                    {
                        if (li_w[i] == li_w[j])
                            continue;
                        //string s = move.ToString() + "," + li_w[i].ToString() + "," + li_w[j].ToString();
                        //Console.WriteLine(s);
                        //if (bt.ply == 49)
                            //sw.WriteLine("www," + s);
                        score -= fv_www[move, li_w[i], li_w[j]];
                    }
                }
                for (i = 0; i < li_b.Count; i++)
                {
                    for (j = i + 1; j < li_b.Count; j++)
                    {
                        if (li_b[i] == li_b[j])
                            continue;
                        //string s = move.ToString() + "," + li_b[i].ToString() + "," + li_b[j].ToString();
                        //Console.WriteLine(s);
                        //if (bt.ply == 49)
                            //sw.WriteLine("wbb," + s);
                        score += fv_wbb[move, li_b[i], li_b[j]];
                    }
                }
                for (i = 0; i < li_b.Count; i++)
                {
                    for (j = 0; j < index; j++)
                    {
                        if (move == li_w[j])
                            continue;
                        //string s = li_b[i].ToString() + "," + move.ToString() + "," + li_w[j].ToString();
                        //if (bt.ply == 49)
                            //sw.WriteLine(s);
                        //Console.WriteLine("bww," + s);
                        score -= fv_bww[li_b[i], li_w[j], move];
                    }
                    for (j = index; j < li_w.Count; j++)
                    {
                        if (move == li_w[j])
                            continue;
                        //string s = li_b[i].ToString() + "," + move.ToString() + "," + li_w[j].ToString();
                        //if (bt.ply == 49)
                            //sw.WriteLine("bww," + s);
                        //Console.WriteLine(s);
                        score -= fv_bww[li_b[i], move, li_w[j]];
                    }
                }
            }

            if (bt.tori_flag[bt.ply] == true)
            {
                //int opponent_color = color ^ 1;
                //int a = 0;
                if (color == 0)
                {
                    for (i = 0; i < 4; i++)
                    {
                        if (bt.removed_seq_num[bt.ply, i] == seq_max)
                        {
                            break;
                        }
                        else
                        {
                            //Console.WriteLine();
                            //Console.WriteLine();
                            //Console.WriteLine();
                            //var temp_li_b = li_b.ToList();
                            index = li_b.IndexOf(move);
                            li_b.Remove(move);
                            for (j = 0; j < bt.removed_seq_sq[bt.ply, i].Count; j++)
                            {
                                //score -= fv_wbb[prev_move, ]
                                int index2;
                                int removed_stone = bt.removed_seq_sq[bt.ply, i][j];
                                sw.WriteLine("removed stone = " + removed_stone.ToString());
                                for (k = 0; k < li_b.Count; k++)
                                {
                                    index2 = li_w.FindIndex(x => x > removed_stone);
                                    for (l = 0; l < index2; l++)
                                    {
                                        score += fv_bww[li_b[k], li_w[l], removed_stone];
                                        Console.WriteLine("removed index = 2");
                                        Console.WriteLine("bww," + li_b[k].ToString() + "," + li_w[l].ToString() + "," + removed_stone.ToString());
                                        sw.WriteLine("bww," + li_b[k].ToString() + "," + li_w[l].ToString() + "," + removed_stone.ToString());
                                        //a++;
                                    }
                                    //index2 = li_w.FindIndex(x => x < removed_stone);
                                    if (index2 >= 0)
                                    {
                                        for (l = index2; l < li_w.Count; l++)
                                        {
                                            score += fv_bww[li_b[k], removed_stone, li_w[l]];
                                            Console.WriteLine("removed index = 1");
                                            Console.WriteLine("bww," + li_b[k].ToString() + "," + removed_stone.ToString() + "," + li_w[l].ToString());
                                            sw.WriteLine("bww," + li_b[k].ToString() + "," + removed_stone.ToString() + "," + li_w[l].ToString());
                                            //a++;
                                        }
                                    }
                                }
                                for (k = 0; k < li_b.Count; k++)
                                {
                                    for (l = k + 1; l < li_b.Count; l++)
                                    {
                                        if (li_b[k] == li_b[l])
                                            continue;
                                        //string s = removed_stone.ToString() + "," + li_b[k].ToString() + "," + li_b[l].ToString();
                                        //Console.WriteLine(s);
                                        //if (bt.ply == 49)
                                            //sw.WriteLine("wbb," + s);
                                        score -= fv_wbb[removed_stone, li_b[k], li_b[l]];
                                        Console.WriteLine("removed index = 0");
                                        Console.WriteLine("wbb," + removed_stone.ToString() + "," + li_b[k].ToString() + "," + li_b[l].ToString());
                                        sw.WriteLine("wbb," + removed_stone.ToString() + "," + li_b[k].ToString() + "," + li_b[l].ToString());
                                        //a++;
                                    }
                                }
                                index2 = li_w.FindIndex(x => x > removed_stone);
                                if (index2 >= 0)
                                {
                                    for (k = index2; k < li_w.Count; k++)
                                    {
                                        for (l = k + 1; l < li_w.Count; l++)
                                        {
                                            if (li_w[k] == li_w[l])
                                                continue;
                                            //string s = removed_stone.ToString() + "," + li_w[k].ToString() + "," + li_w[l].ToString();
                                            //Console.WriteLine(s);
                                            //if (bt.ply == 49)
                                            //sw.WriteLine("www," + s);
                                            score += fv_www[removed_stone, li_w[k], li_w[l]];
                                            Console.WriteLine("removed index = 0");
                                            Console.WriteLine("www," + removed_stone.ToString() + "," + li_w[k].ToString() + "," + li_w[l].ToString());
                                            sw.WriteLine("www," + removed_stone.ToString() + "," + li_w[k].ToString() + "," + li_w[l].ToString());
                                            //a++;
                                        }
                                    }
                                }

                                index2 = li_w.FindIndex(x => x < removed_stone);
                                int index3 = li_w.FindIndex(x => x > removed_stone);
                                if (index2 >= 0)
                                {
                                    for (k = index2; k < index3; k++)
                                    {
                                        for (l = index3; l < li_w.Count; l++)
                                        {
                                            //if (removed_stone == li_w[l])
                                            //continue;
                                            //string s = li_w[k].ToString() + "," + removed_stone.ToString() + "," + li_w[l].ToString();
                                            //Console.WriteLine(s);
                                            //if (bt.ply == 49)
                                            //sw.WriteLine("www," + s);
                                            score += fv_www[li_w[k], removed_stone, li_w[l]];
                                            Console.WriteLine("removed index = 1");
                                            Console.WriteLine("www," + li_w[k].ToString() + "," + removed_stone.ToString() + "," + li_w[l].ToString());
                                            sw.WriteLine("www," + li_w[k].ToString() + "," + removed_stone.ToString() + "," + li_w[l].ToString());
                                            //a++;
                                        }
                                    }
                                    for (k = index2; k < index3; k++)
                                    {
                                        for (l = k + 1; l < index3; l++)
                                        {
                                            //if (removed_stone == li_w[l])
                                            //continue;
                                            //string s = li_w[k].ToString() + "," + li_w[l].ToString() + "," + removed_stone.ToString();
                                            //Console.WriteLine(s);
                                            //if (bt.ply == 49)
                                            //sw.WriteLine("www," + s);
                                            score += fv_www[li_w[k], li_w[l], removed_stone];
                                            Console.WriteLine("removed index = 2");                                   
                                            Console.WriteLine("www," + li_w[k].ToString() + "," + li_w[l].ToString() + "," + removed_stone.ToString());
                                            sw.WriteLine("www," + li_w[k].ToString() + "," + li_w[l].ToString() + "," + removed_stone.ToString());
                                            //a++;
                                        }
                                    }
                                }
                            }
                            li_b.Insert(index, move);
                        }
                    }
                }
                else
                {
                    for (i = 0; i < 4; i++)
                    {
                        if (bt.removed_seq_num[bt.ply, i] == seq_max)
                        {
                            break;
                        }
                        else
                        {
                            //Console.WriteLine();
                            //Console.WriteLine();
                            //Console.WriteLine();
                            //var temp_li_b = li_b.ToList();
                            index = li_w.IndexOf(move);
                            li_w.Remove(move);
                            for (j = 0; j < bt.removed_seq_sq[bt.ply, i].Count; j++)
                            {
                                //score -= fv_wbb[prev_move, ]
                                int index2;
                                int removed_stone = bt.removed_seq_sq[bt.ply, i][j];
                                for (k = 0; k < li_w.Count; k++)
                                {
                                    index2 = li_b.FindIndex(x => x > removed_stone);
                                    for (l = 0; l < index2; l++)
                                    {
                                        score -= fv_wbb[li_w[k], li_b[l], removed_stone];
                                        //a++;
                                    }
                                    //index2 = li_w.FindIndex(x => x < removed_stone);
                                    if (index2 >= 0)
                                    {
                                        for (l = index2; l < li_b.Count; l++)
                                        {
                                            score -= fv_wbb[li_w[k], removed_stone, li_b[l]];
                                            //a++;
                                        }
                                    }
                                }
                                for (k = 0; k < li_w.Count; k++)
                                {
                                    for (l = k + 1; l < li_w.Count; l++)
                                    {
                                        if (li_w[k] == li_w[l])
                                            continue;
                                        //string s = removed_stone.ToString() + "," + li_w[k].ToString() + "," + li_w[l].ToString();
                                        //Console.WriteLine(s);
                                        //if (bt.ply == 49)
                                            //sw.WriteLine("bww," + s);
                                        score += fv_bww[removed_stone, li_w[k], li_w[l]];
                                    }
                                }
                                index2 = li_b.FindIndex(x => x > removed_stone);
                                if (index2 >= 0)
                                {
                                    for (k = index2; k < li_b.Count; k++)
                                    {
                                        for (l = k + 1; l < li_b.Count; l++)
                                        {
                                            if (li_b[k] == li_b[l])
                                                continue;
                                            //string s = removed_stone.ToString() + "," + li_b[k].ToString() + "," + li_b[l].ToString();
                                            //Console.WriteLine(s);
                                            //if (bt.ply == 49)
                                            //sw.WriteLine("bbb," + s);
                                            score -= fv_bbb[removed_stone, li_b[k], li_b[l]];
                                        }
                                    }
                                }

                                index2 = li_b.FindIndex(x => x < removed_stone);
                                int index3 = li_b.FindIndex(x => x > removed_stone);
                                if (index2 >= 0)
                                {
                                    for (k = index2; k < index3; k++)
                                    {
                                        for (l = index3; l < li_b.Count; l++)
                                        {
                                            //if (removed_stone == li_b[l])
                                            //continue;
                                            //string s = li_b[k].ToString() + "," + removed_stone.ToString() + "," + li_b[l].ToString();
                                            //Console.WriteLine(s);
                                            //if (bt.ply == 49)
                                            //sw.WriteLine("bbb," + s);
                                            score -= fv_bbb[li_b[k], removed_stone, li_b[l]];
                                        }
                                    }
                                    for (k = index2; k < index3; k++)
                                    {
                                        for (l = k + 1; l < index3; l++)
                                        {
                                            //if (removed_stone == li_b[l])
                                            //continue;
                                            //string s = li_b[k].ToString() + "," + li_b[l].ToString() + "," + removed_stone.ToString();
                                            //Console.WriteLine(s);
                                            //if (bt.ply == 49)
                                            //sw.WriteLine("bbb," + s);
                                            score -= fv_bbb[li_b[k], li_b[l], removed_stone];
                                        }
                                    }
                                }
                            }
                            li_w.Insert(index, move);
                        }
                    }

                }
            }
            sw.Close();
            //score = (prev_value * 32) - score;
            return score / fv_scale;
        }

    }
}
