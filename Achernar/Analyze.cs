using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorchSharp;
using static Achernar.Common;
using static Achernar.MakeMove;

namespace Achernar
{
    internal class Analyze
    {
        public static void AnalyzeRecord(string read_file_name, string out_file_name, int task_num, string[] str_header, int thinking_time)
        {
            List<Record> records = new List<Record>();
            records = IO.ReadRecordFile(read_file_name);
            StreamWriter sw = IO.OpenStreamWriter(out_file_name);

            //int policy_contains_count = 0;

            sw.WriteLine("対局日：" + str_header[1] + "\n");
            sw.WriteLine("棋戦名：" + str_header[2] + "\n");
            sw.WriteLine("黒番：" + records[0].players[0] + "\n");
            sw.WriteLine("白番：" + records[0].players[1] + "\n");

            Board board = new Board();
            board.Init();
            Prediction prediction = new Prediction();
            prediction.InitPolicy();
            prediction.InitValue();
            Feature ft = new Feature();
            var d = torch.device(DeviceType.CUDA);
            float[] policy_result = new float[4];
            Controller controller = new Controller();
            //Communicate cm = new Communicate();
            //cm.Boot();

            //try
            {
                int color_out = 0;
                int[] correct_count = new int[2];
                correct_count[0] = 0;
                correct_count[1] = 0;
                int[] correct_count_within3 = new int[2];
                correct_count_within3[0] = 0;
                correct_count_within3[1] = 0;
                int[] color_count = new int[2];
                color_count[0] = 0;
                color_count[1] = 0;
                string str_out = "";

                for (int i = 0; i < records[0].str_moves.Count(); i++)
                {
                    //if (i == 10)
                        //break;

                    //board = new Board();
                    board.Init();
                    board.RootMoves = new short[4];
                    int limit = i;
                    short color = 0;
                    string str_color;

                    if (color_out == 0)
                    {
                        str_color = "●";
                    }
                    else
                    {
                        str_color = "○";
                    }

                    str_out = "";

                    string str_pro_move = (FileTable[records[0].moves[i]] + 1).ToString() + "-" + (RankTable[records[0].moves[i]] + 1).ToString();

                    Console.WriteLine("ply = " + (i + 1).ToString());
                    str_out += "ply=" + (i + 1).ToString();
                    str_out += "   ";
                    str_out += "pro =";
                    str_out += str_color;
                    str_out += str_pro_move;
                    str_out += ",   ";
                    str_out += "com =";

                    for (short j = 0; j < limit; j++)
                    {
                        Do(ref board, records[0].moves[j], color, j);
                        color ^= 1;
                    }

                    {
                        List<short> m = new List<short>();
                        List<float> f = new List<float>();
                        List<int> t = new List<int>();

                        //string str_to_python = "p," + Board.BoardToString(board, "p", color);
                        //cm.ThrowRequest(str_to_python);
                        //string s = cm.ReceiveResponse();
                        bool b_color = false;
                        if (color == 1)
                            b_color = true;
                        var x = ft.MakeInputFeature(ref board, b_color);
                        x = x.reshape(1, 44, 19, 19).to(d);
                        var y = prediction.ExecPolicy(x);
                        int count = 0;
                        board.RootMoves[0] = board.RootMoves[1] = board.RootMoves[2] = board.RootMoves[3] = NSquare;
                        policy_result[0] = policy_result[1] = policy_result[2] = policy_result[3] = float.MinValue;
                        torch.Tensor y2 = torch.sigmoid(y[0][1].reshape(1, 361));
                        short y3;
                        while (count < 4)
                        {
                            y3 = torch.argmax(y2).ToInt16();
                            if (board.pos_empty.Contains(y3))
                            {
                                board.RootMoves[count] = y3;
                                policy_result[count++] = y2[0][y3].ToSingle();
                                y2[0][y3] = float.MinValue;
                            }
                        }

                        float policy_sum = policy_result.Sum();
                        for (int j = 0; j < 4; j++)
                        {
                            policy_result[j] /= policy_sum;
                        }

                        board.RootColor = color;
                        //string s = "";
                        //board.SetRootPos(ref board, s, color, ref policy_result);

                        controller.ReceiveLoop(ref board, ref prediction, task_num, ref policy_result, ref m, ref f, ref t, thinking_time);// fの大きい順に並べ替える

                        int o = t.Sum();
                        Console.WriteLine("trial_counts = " + o.ToString());

                        //if (str_mate_pv == "")
                        {
                            List<short> moves = new List<short>();
                            List<int> trial_counts = new List<int>();
                            List<float> win_rates = new List<float>();

                            for (int j = 0; j < m.Count; j++)
                            {
                                int value_max = t.Max();
                                if (value_max < 0)
                                    break;
                                trial_counts.Add(value_max);
                                int index = Array.IndexOf(t.ToArray(), value_max);
                                t[index] = int.MinValue;
                                moves.Add(m[index]);
                                win_rates.Add(f[index]);
                            }

                            for (int j = 0; j < moves.Count; j++)
                            {
                                //string str_move = CSA.Move2CSA(moves[j]);
                                string str_move = (FileTable[moves[j]] + 1).ToString() + "-" + (RankTable[moves[j]] + 1).ToString();
                                if (j == 0)
                                {
                                    str_out += str_color;
                                    str_out += str_move;
                                    str_out += "   ";
                                }

                                if (str_pro_move == str_move)
                                {
                                    str_out += "result= ○ ";
                                    correct_count_within3[color]++;
                                    if (j == 0)
                                        correct_count[color]++;
                                }
                                else
                                {
                                    str_out += "result= × ";
                                }

                                str_out += "  ";
                                str_out += "候補手" + (j + 1).ToString() + "：" + str_color + str_move;
                                str_out += " 訪問回数 " + trial_counts[j].ToString();
                                str_out += "  ";
                                str_out += " 勝率 " + win_rates[j].ToString("P", CultureInfo.InvariantCulture);
                                if (j != moves.Count - 1)
                                    str_out += ",   ";
                            }
                            sw.WriteLine(str_out);
                        }
                    }

                    color_count[color]++;
                    color_out ^= 1;
                }

                float v;

                str_out = "\n";
                str_out += "黒番一致率：" + correct_count[0].ToString() + " / " + color_count[0].ToString();
                v = (float)((float)correct_count[0] / (float)color_count[0]);
                str_out += " " + v.ToString("P", CultureInfo.InvariantCulture);
                str_out += "\n\n";
                str_out += "白番一致率：" + correct_count[1].ToString() + " / " + color_count[1].ToString();
                v = (float)((float)correct_count[1] / (float)color_count[1]);
                str_out += " " + v.ToString("P", CultureInfo.InvariantCulture);
                str_out += "\n\n";
                str_out += "全体一致率： " + (correct_count[0] + correct_count[1]).ToString() + " / " + records[0].str_moves.Count().ToString();
                v = (float)((float)(correct_count[0] + correct_count[1]) / (float)records[0].str_moves.Count());
                str_out += " " + v.ToString("P", CultureInfo.InvariantCulture);
                str_out += "\n\n";
                str_out += "候補手3位以内の率： " + (correct_count_within3[0] + correct_count_within3[1]).ToString() + " / " + records[0].str_moves.Count().ToString();
                v = (float)((float)(correct_count_within3[0] + correct_count_within3[1]) / (float)records[0].str_moves.Count());
                str_out += " " + v.ToString("P", CultureInfo.InvariantCulture);
                str_out += "\n\n";
                str_out += "解析解析エンジン名：Achernar Ver.1.0.2";// ToDo: ソフト名を考える。
                sw.WriteLine(str_out);
            }
            //catch (Exception ex)
            //{
            //sw.WriteLine(ex.ToString());
            //}

            //Console.WriteLine("policy_contains_count = " + policy_contains_count.ToString());

            //cm.Quit();
            //cm.Dispose();
            sw.Close();
        }
    }
}
