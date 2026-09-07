using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using static Achernar.Common;
using static Achernar.Board;

namespace Achernar
{
    internal class ParameterBase
    {
        public int input_dim;
        public int output_dim;
        public int weight_dim_num;
        public int bias_dim_num;

        public void Init(int[] param)
        {
            input_dim = param[0];
            output_dim = param[1];
            weight_dim_num = param[2];
            bias_dim_num = param[3];
        }
    }

    // 直前の相手の手に対応する手かどうかを判別するパラメータ
    //input = 361*2, prev_moveの画像1枚とcurrent_moveの画像1枚
    // output = 1, score
    internal class Parameter0 : ParameterBase
    {
        public double[] weight = new double[2 * 361];
        public double bias = new double();
        public string file_name = "model_playout_param_0.txt";

        public void LoadParam()
        {
            string AppPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FilePath = AppPath + "\\" + file_name;
            string line;
            StreamReader sr = new StreamReader(FilePath, Encoding.UTF8);
            while ((line = sr.ReadLine()) != null)
            {
                string[] s = line.Split(',');
                switch (s[0])
                {
                    case "w":
                        weight[int.Parse(s[1])] = double.Parse(s[2]);
                        break;
                    case "b":
                        bias = double.Parse(s[1]);
                        break;
                }
            }
            sr.Close();
        }

        public bool IsRespond(short prev_move, short current_move)
        {
            short file_prev, rank_prev, file_cur, rank_cur, dist_file, dist_rank;
            file_prev = FileTable[prev_move];
            rank_prev = RankTable[prev_move];
            file_cur = FileTable[current_move];
            rank_cur = RankTable[current_move];
            dist_file = (short)Math.Abs(file_prev - file_cur);
            dist_rank = (short)Math.Abs(rank_prev - rank_cur);
            if (dist_file <= 2 && dist_rank <= 2)
                return true;
            return false;
        }

        public double MatMul(double[] x)
        {
            double v = 0.0f;
            for (int i = 0; i < x.Length; i++)
                v += x[i] * weight[i];
            v += bias;
            v = 1 / (1 + Math.Exp(-v));
            return v;
        }
    }

    //当たりから逃げる手かどうかを判別するパラメータ
    //input = 361, 対象になる連のビットボード1枚(19x19)
    //output = 1, score
    internal class Parameter1 : ParameterBase
    {
        public double[] weight = new double[361];
        public double bias = new double();
        public string file_name = "model_playout_param_1.txt";

        public void LoadParam()
        {
            string AppPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FilePath = AppPath + "\\" + file_name;
            string line;
            StreamReader sr = new StreamReader(FilePath, Encoding.UTF8);
            while ((line = sr.ReadLine()) != null)
            {
                string[] s = line.Split(',');
                switch (s[0])
                {
                    case "w":
                        weight[int.Parse(s[1])] = double.Parse(s[2]);
                        break;
                    case "b":
                        bias = double.Parse(s[1]);
                        break;
                }
            }
            sr.Close();
        }

        public bool IsEscape(Board board, short move, short color, ref string s_num)
        {
            List<short> seq_number_list = new List<short>();
            for (int i = 0; i < PosCrossTable[move].Count; i++)
            {
                short seq_number = board.seq_number_table[color, PosCrossTable[move][i]];
                if (seq_number != seq_max)
                    seq_number_list.Add(seq_number);
            }

            if (seq_number_list.Count == 0)
            {
                s_num = "None";
                return false;
            }
            else
            {
                for (int i = 0; i < seq_number_list.Count; i++)
                {
                    int cnt = board.dame_sq[color, seq_number_list[i]].Count;
                    if (cnt == 1)
                    {
                        s_num = seq_number_list[i].ToString();
                        return true;
                    }
                }
            }

            s_num = "None";
            return false;
        }

        public double MatMul(double[] x)
        {
            double v = 0.0f;
            for (int i = 0; i < x.Length; i++)
                v += x[i] * weight[i];
            v += bias;
            v = 1 / (1 + Math.Exp(-v));
            return v;
        }
    }

    // 取る手かどうかを判別するパラメータ
    // input = 361, 対象になる相手の連のビットボード1枚(19x19)
    // output = 1, score
    internal class Parameter2 : Parameter1
    {
        public Parameter2()
        {
            file_name = "model_playout_param_2.txt";
        }

        public bool IsCapture(Board board, short move, short color, ref string s_num)
        {
            List<short> seq_number_list = new List<short>();
            for (int i = 0; i < PosCrossTable[move].Count; i++)
            {
                short seq_number = board.seq_number_table[color ^ 1, PosCrossTable[move][i]];
                if (seq_number != seq_max)
                    seq_number_list.Add(seq_number);
            }

            if (seq_number_list.Count == 0)
            {
                s_num = "None";
                return false;
            }
            else
            {
                for (int i = 0; i < seq_number_list.Count; i++)
                {
                    int cnt = board.dame_sq[color ^ 1, seq_number_list[i]].Count;
                    if (cnt == 1)
                    {
                        s_num = seq_number_list[i].ToString();
                        return true;
                    }
                }
            }

            s_num = "None";
            return false;
        }
    }

    internal class Parameter3 : ParameterBase
    {
        public double[] weight = new double[8];
        public double bias = new double();
        public string file_name = "model_playout_param_3.txt";

        public void LoadParam()
        {
            string AppPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FilePath = AppPath + "\\" + file_name;
            string line;
            StreamReader sr = new StreamReader(FilePath, Encoding.UTF8);
            while ((line = sr.ReadLine()) != null)
            {
                string[] s = line.Split(',');
                switch (s[0])
                {
                    case "w":
                        weight[int.Parse(s[1])] = double.Parse(s[2]);
                        break;
                    case "b":
                        bias = double.Parse(s[1]);
                        break;
                }
            }
            sr.Close();
        }

        public double MatMul(double[] x)
        {
            double v = 0.0f;
            for (int i = 0; i < x.Length; i++)
                v += x[i] * weight[i];
            v += bias;
            v = 1 / (1 + Math.Exp(-v));
            return v;
        }
    }

    internal class Parameter4 : ParameterBase
    {
        public double[,] weight = new double[8, 65535];
        public double[] bias = new double[65535];
        public string file_name = "model_playout_param_4.txt";
        private const int size_v = 65535;
        public void LoadParam()
        {
            string AppPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FilePath = AppPath + "\\" + file_name;
            string line;
            StreamReader sr = new StreamReader(FilePath, Encoding.UTF8);
            while ((line = sr.ReadLine()) != null)
            {
                string[] s = line.Split(',');
                switch (s[0])
                {
                    case "w":
                        weight[int.Parse(s[1]), int.Parse(s[2])] = double.Parse(s[3]);
                        break;
                    case "b":
                        bias[int.Parse(s[1])] = double.Parse(s[2]);
                        break;
                }
            }
            sr.Close();
        }

        public double[] MatMul(double[] x)
        {
            double[] v = new double[size_v];
            for (int i = 0; i < x.Length; i++)
                for (int j = 0; weight.Length > j; j++)
                    v[j] = x[i] * weight[i, j];

            for (int i = 0; i < size_v; i++)
            {
                v[i] += bias[i];
                v[i] = 1 / (1 + (double)Math.Exp(-v[i]));
            }
            return v;
        }
    }

    internal class Parameter5 : Parameter4
    {
        public Parameter5()
        {
            file_name = "model_playout_param_5.txt";
        }
    }

    internal class Parameter6 : Parameter4
    {
        public Parameter6()
        {
            file_name = "model_playout_param_6.txt";
        }
    }

    // 当該局面での勝率を返す
    // input = 19x19の画像20枚、黒石の位置 = 1枚, 白石の位置 = 1枚, 空白 = 1枚, n手前までの手の位置 = 16枚, 手番 = 1枚（黒番：全部ゼロ埋め, 白番：全部1埋め）
    // output = 1, 勝率 スカラー値
    internal class Parameter7 : ParameterBase
    {
        public double[] weight = new double[361*20];
        public double bias = new double();
        public string file_name = "model_log3_v.txt";

        public void LoadParam()
        {
            string AppPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FilePath = AppPath + "\\" + file_name;
            string line;
            StreamReader sr = new StreamReader(FilePath, Encoding.UTF8);
            while ((line = sr.ReadLine()) != null)
            {
                string[] s = line.Split(',');
                switch (s[0])
                {
                    case "w":
                        weight[int.Parse(s[1])] = double.Parse(s[2]);
                        break;
                    case "b":
                        bias = double.Parse(s[1]);
                        break;
                }
            }
            sr.Close();
        }

        public double MatMul(double[] x)
        {
            double v = 0.0f;
            for (int i = 0; i < x.Length; i++)
                v += x[i] * weight[i];
            v += bias;
            v = 1 / (1 + Math.Exp(-v));
            return v;
        }
    }

    // 当該局面での手の点数を返す
    // input = 19x19の画像20枚、黒石の位置 = 1枚, 白石の位置 = 1枚, 空白 = 1枚, n手前までの手の位置 = 16枚, 手番 = 1枚（黒番：全部ゼロ埋め, 白番：全部1埋め）
    internal class Parameter8 : ParameterBase
    {
        public double[,] weight = new double[361 * 20, 361];
        public double[] bias = new double[361];
        public string file_name = "model_log3.txt";
        private const int size_v = 361;
        private const int size_prev_moves = 16;
        private const int image_count = 20;

        public void LoadParam()
        {
            string AppPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FilePath = AppPath + "\\" + file_name;
            string line;
            StreamReader sr = new StreamReader(FilePath, Encoding.UTF8);
            while ((line = sr.ReadLine()) != null)
            {
                string[] s = line.Split(',');
                switch (s[0])
                {
                    case "w":
                        weight[int.Parse(s[1]), int.Parse(s[2])] = double.Parse(s[3]);
                        break;
                    case "b":
                        bias[int.Parse(s[1])] = double.Parse(s[2]);
                        break;
                }
            }
            sr.Close();
        }

        public double[] MatMul(double[] x)
        {
            double[] v = new double[size_v];
            double[] exp = new double[size_v];
            for (int i = 0; i < x.Length; i++)
                for (int j = 0; weight.Length > j; j++)
                    v[j] = x[i] * weight[i, j];

            double exp_sum = 0d;
            for (int i = 0; i < size_v; i++)
            {
                v[i] += bias[i];
                exp[i] = Math.Exp(v[i]);
                exp_sum += exp[i];
            }

            double[] ret_val = new double[size_v];
            for (int i = 0; i < size_v; i++)
                ret_val[i] = exp[i] / exp_sum;

            return ret_val;
        }

        public void MakeFeature(Board bt, ref double[] input_v, short color)
        {
            int index, counter;
            short[] prev_moves = new short[size_v];

            index = bt.current_moves.Count - 1;
            counter = 0;
            while (index >= 0)
                prev_moves[counter++] = bt.current_moves[index--];

            if (counter < size_prev_moves)
                prev_moves[counter] = NSquare;

            // 盤面
            for (int i = 0; i < NSquare; i++)
            {
                switch(bt.board[i])
                {
                    // 黒石
                    case 0:
                        input_v[i] = 1.0d;
                        break;
                    // 白石
                    case 1:
                        input_v[i + 361] = 1.0d;
                        break;
                    // 空白
                    case 2:
                        input_v[i + (361 * 2)] = 1.0d;
                        break;
                }
            }

            // 16手前までの位置
            index = NSquare * 2;
            for (int i = 0; i < image_count; i++)
            {
                if (prev_moves[i] == NSquare)
                    break;
                input_v[index + prev_moves[i]] = 1.0d;
                index += NSquare;
            }

            // 手番
            if (color == 1)
            {
                index = NSquare * (image_count - 1);
                while (index < NSquare * image_count)
                    input_v[index++] = 1.0d;
            }
        }
    }
}