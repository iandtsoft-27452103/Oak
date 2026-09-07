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

namespace Achernar
{
    internal class Evaluate2
    {
        const short Black = 0;
        const short White = 1;
        const short Empty = 2;
        const int fv_scale = 32;
        const string file_name_pb = "fv_pb.bin";
        const string file_name_pw = "fv_pw.bin";
        const string file_name_bb = "fv_bb.bin";
        const string file_name_bw = "fv_bw.bin";
        const string file_name_wb = "fv_wb.bin";
        const string file_name_ww = "fv_ww.bin";
        public short[,] fv_pb = new short[NSquare, NSquare];
        public short[,] fv_pw = new short[NSquare, NSquare];
        public short[,] fv_bb = new short[NSquare, NSquare];
        public short[,] fv_bw = new short[NSquare, NSquare];// bwとwbは相殺してしまうかもしれないが、取りあえず入れておく。
        public short[,] fv_wb = new short[NSquare, NSquare];
        public short[,] fv_ww = new short[NSquare, NSquare];

        public void RandomInit()
        {
            Random r = new Random();
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    fv_pb[i, j] = (short)r.Next(64, 128);
                    fv_pw[i, j] = (short)r.Next(64, 128);
                    fv_bb[i, j] = (short)r.Next(1, 10);
                    fv_bw[i, j] = (short)r.Next(1, 10);
                    fv_wb[i, j] = (short)r.Next(1, 10);
                    fv_ww[i, j] = (short)r.Next(1, 10);
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
            bw = new BinaryWriter(File.Open(file_name_bb, FileMode.Create));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    bw.Write(fv_bb[i, j]);
                }
            }
            bw.Close();
            bw = new BinaryWriter(File.Open(file_name_bw, FileMode.Create));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    bw.Write(fv_bw[i, j]);
                }
            }
            bw.Close();
            bw = new BinaryWriter(File.Open(file_name_wb, FileMode.Create));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    bw.Write(fv_wb[i, j]);
                }
            }
            bw.Close();
            bw = new BinaryWriter(File.Open(file_name_ww, FileMode.Create));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    bw.Write(fv_ww[i, j]);
                }
            }
            bw.Close();
        }
        public void LoadFV()
        {
            int counter = 0;
            BinaryReader br = new BinaryReader(File.Open(file_name_pb, FileMode.Open));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    fv_pb[i, j] = br.ReadInt16();
                    if (fv_pb[i, j] != 0)
                        counter++;
                }
            }
            br.Close();
            br = new BinaryReader(File.Open(file_name_pw, FileMode.Open));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    fv_pw[i, j] = br.ReadInt16();
                    if (fv_pw[i, j] != 0)
                        counter++;
                }
            }
            br.Close();
            br = new BinaryReader(File.Open(file_name_bb, FileMode.Open));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    fv_bb[i, j] = br.ReadInt16();
                    if (fv_bb[i, j] != 0)
                        counter++;
                }
            }
            br.Close();
            br = new BinaryReader(File.Open(file_name_bw, FileMode.Open));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    fv_bw[i, j] = br.ReadInt16();
                    if (fv_bw[i, j] != 0)
                        counter++;
                }
            }
            br.Close();
            br = new BinaryReader(File.Open(file_name_wb, FileMode.Open));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    fv_wb[i, j] = br.ReadInt16();
                    if (fv_wb[i, j] != 0)
                        counter++;
                }
            }
            br.Close();
            br = new BinaryReader(File.Open(file_name_ww, FileMode.Open));
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    fv_ww[i, j] = br.ReadInt16();
                    if (fv_ww[i, j] != 0)
                        counter++;
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
            int i, j;
            int score = 0;
            short temp_color = 0;

            for (i = 1; i <= bt.ply; i++)
            {
                score += (temp_color == 0) ? fv_pb[i - 1, bt.current_moves[i - 1]] : -fv_pw[i - 1, bt.current_moves[i - 1]];
                temp_color ^= 1;
            }

            if (bt.ply <= 8)
            {
                return score / fv_scale;
            }

            List<short> li_b = new List<short>();
            List<short> li_w = new List<short>();

            MakeList(bt, ref li_b, ref li_w);
            for (i = 0; i < li_b.Count; i++)
            {
                for (j = i + 1; j < li_b.Count; j++)
                {
                    score += fv_bb[li_b[i], li_b[j]];
                }
            }

            for (i = 0; i < li_w.Count; i++)
            {
                for (j = 0; j < li_b.Count; j++)
                {
                    score += fv_wb[li_w[i], li_b[j]];
                }
            }

            for (i = 0; i < li_w.Count; i++)
            {
                for (j = i + 1; j < li_w.Count; j++)
                {
                    score -= fv_ww[li_w[i], li_w[j]];
                }
            }

            for (i = 0; i < li_b.Count; i++)
            {
                for (j = 0; j < li_w.Count; j++)
                {
                    score -= fv_bw[li_b[i], li_w[j]];
                }
            }

            return score / fv_scale;
        }

        public int EvaluateDiff(Board bt, short color, short move,  short prev_value, ref List<short> li_b, ref List<short> li_w)
        {
            int i, j, k, index, index2;
            int score = prev_value * 32;

            score += color == 0 ? fv_pb[bt.ply - 1, move] : -fv_pw[bt.ply - 1, move];

            if (color == 0)
            {
                index = li_b.IndexOf(move);
                for (i = 0; i < index; i++)
                {
                    score += fv_bb[li_b[i], move];
                }
                for (i = index + 1; i < li_b.Count; i++)
                {
                    score += fv_bb[move, li_b[i]];
                }
                for (i = 0; i < li_w.Count; i++)
                {
                    score -= fv_bw[move, li_w[i]];
                }
                for (i = 0; i < li_w.Count; i++)
                {
                    score += fv_wb[li_w[i], move];
                }
            }
            else
            {
                index = li_w.IndexOf(move);
                for (i = 0; i < index; i++)
                {
                    score -= fv_ww[li_w[i], move];
                }
                for (i = index + 1; i < li_w.Count; i++)
                {
                    score -= fv_ww[move, li_w[i]];
                }
                for (i = 0; i < li_b.Count; i++)
                {
                    score -= fv_bw[li_b[i], move];
                }
                for (i = 0; i < li_b.Count; i++)
                {
                    score += fv_wb[move, li_b[i]];
                }
            }

            if (bt.tori_flag[bt.ply] == true)
            {
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
                            index = li_b.IndexOf(move);
                            li_b.Remove(move);
                            for (j = 0; j < bt.removed_seq_sq[bt.ply, i].Count; j++)
                            {
                                int removed_stone = bt.removed_seq_sq[bt.ply, i][j];
                                for (k = 0; k < li_b.Count; k++)
                                {
                                    score += fv_bw[li_b[k], removed_stone];
                                }
                                for (k = 0; k < li_b.Count; k++)
                                {
                                    score -= fv_wb[removed_stone, li_b[k]];
                                }
                                index2 = li_w.FindIndex(x => x < removed_stone);
                                if (index2 == -1)
                                {
                                    for (k = 0; k < li_w.Count; k++)
                                    {
                                        score += fv_ww[removed_stone, li_w[k]];
                                    }
                                }
                                else
                                {
                                    for (k = 0; k < index2; k++)
                                    {
                                        score += fv_ww[li_w[k], removed_stone];
                                    }
                                    for (k = index2; k < li_w.Count; k++)
                                    {
                                        score += fv_ww[removed_stone, li_w[k]];
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
                            index = li_w.IndexOf(move);
                            li_w.Remove(move);
                            for (j = 0; j < bt.removed_seq_sq[bt.ply, i].Count; j++)
                            {
                                int removed_stone = bt.removed_seq_sq[bt.ply, i][j];
                                for (k = 0; k < li_w.Count; k++)
                                {
                                    score += fv_bw[removed_stone, li_w[k]];
                                }
                                for (k = 0; k < li_w.Count; k++)
                                {
                                    score -= fv_wb[li_w[k], removed_stone];
                                }
                                index2 = li_b.FindIndex(x => x < removed_stone);
                                if (index2 == -1)
                                {
                                    for (k = 0; k < li_b.Count; k++)
                                    {
                                        score -= fv_bb[removed_stone, li_b[k]];
                                    }
                                }
                                else
                                {
                                    for (k = 0; k < index2; k++)
                                    {
                                        score -= fv_bb[li_b[k], removed_stone];
                                    }
                                    for (k = index2; k < li_b.Count; k++)
                                    {
                                        score -= fv_bb[removed_stone, li_b[k]];
                                    }
                                }
                            }
                            li_w.Insert(index, move);
                        }
                    }

                }
            }

            return score / fv_scale;
        }
    }
}
