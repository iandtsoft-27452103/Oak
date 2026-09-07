using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Achernar.MakeMove;
using static Achernar.Common;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch.nn;
//using static Achernar.Feature;
using static Achernar.IO;
using static Achernar.Record;
using static Achernar.Board;
using static Achernar.Layer;
//using static Achernar.Position;
using System.Drawing;
using System.Net.NetworkInformation;

namespace Achernar
{
    internal class MCTS
    {
        public Board BTree;
        public List<Node> NodeList = new List<Node>();
        public int PlayOutCount;
        public float[] RootOutput;
        public const float value_lambda = 0.5f;
        public const int nthr = 30;
        public int TaskNumber;
        public long SearchTimeLimit;// ミリ秒で指定する
        public const long TimeBuffer = 500;
        public Stopwatch sw = new Stopwatch();
        public Queue<string> queue_from_main_thread = new Queue<string>();
        public Queue<torch.Tensor> queue_to_main_thread_p = new Queue<torch.Tensor>();
        public Queue<torch.Tensor> queue_to_main_thread_v = new Queue<torch.Tensor>();
        public TT tt = new TT();
        public bool is_abort;
        public bool is_finished;
        public Feature ft = new Feature();
        public bool[] b_color = { false, true };

        // 後で色々試す
        //public Parameter7 param7 = new Parameter7();
        //public Parameter8 param8 = new Parameter8();
        public Record record;
        public bool is_console_out;

        public List<short> TotalParam(MCTS[] mcts_tasks, ref List<float> f, ref List<int> t, bool use_draw)// ToDo:勝率のリストも返すように変更する
        {
            int value_max, max_index, limit;
            List<int> trial_count_array = new List<int>();
            List<float> win_rate_array = new List<float>();
            List<short> return_moves = new List<short>();
            List<float> return_win_rate_array = new List<float>();
            List<int> return_trial_count_array = new List<int>();

            switch (BTree.RootMoves.Length)
            {
                case 1:
                    limit = 1;
                    break;
                case 2:
                    limit = 2;
                    break;
                default:
                    limit = 3;
                    break;
            }

            if (mcts_tasks.Length > 1)
            {
                for (int i = 1; i < mcts_tasks.Length; i++)
                {
                    for (int j = 0; j < BTree.RootMoves.Length; j++)
                    {
                        mcts_tasks[0].NodeList[j + 1].TrialCount += mcts_tasks[i].NodeList[j + 1].TrialCount;
                        mcts_tasks[0].NodeList[j + 1].WinCount += mcts_tasks[i].NodeList[j + 1].WinCount;
                        mcts_tasks[0].NodeList[j + 1].DrawCount += mcts_tasks[i].NodeList[j + 1].DrawCount;
                        mcts_tasks[0].NodeList[j + 1].LostCount += mcts_tasks[i].NodeList[j + 1].LostCount;
                    }
                }
            }

            for (int i = 0; i < BTree.RootMoves.Length; i++)
            {
                trial_count_array.Add(mcts_tasks[0].NodeList[i + 1].TrialCount);
            }

            for (int i = 0; i < BTree.RootMoves.Length; i++)
            {
                if (use_draw)
                {
                    if (trial_count_array[i] == 0)
                    {
                        win_rate_array.Add(0);
                    }
                    else
                    {
                        win_rate_array.Add((mcts_tasks[0].NodeList[i + 1].WinCount + (float)(mcts_tasks[0].NodeList[i + 1].DrawCount * 0.5)) / trial_count_array[i]);
                    }
                }
                else
                {
                    if (trial_count_array[i] == 0)
                    {
                        win_rate_array.Add(0);
                    }
                    else
                    {
                        win_rate_array.Add(mcts_tasks[0].NodeList[i + 1].WinCount / trial_count_array[i]);
                    }
                }
            }

            // Value Networkの結果を使う場合
            /*for (int i = 0; i < BTree.RootMoves.Length; i++)
            {
                win_rate_array.Add(mcs_tasks[0].NodeList[i + 1].WinRateSum / mcs_tasks[0].NodeList[i + 1].EvalCount);
            }*/

            for (int i = 0; i < limit; i++)
            {
                value_max = trial_count_array.Max();
                max_index = Array.IndexOf(trial_count_array.ToArray(), value_max);
                return_trial_count_array.Add(trial_count_array[max_index]);
                trial_count_array[max_index] = int.MinValue;
                return_moves.Add(BTree.RootMoves[max_index]);
                return_win_rate_array.Add(win_rate_array[max_index]);
                /*if (BTree.RootMoves.Length == 1)
                {
                    return_moves.Add(BTree.RootMoves[max_index]);
                    return_win_rate_array.Add(win_rate_array[max_index]);
                }*/
            }

            f = return_win_rate_array;
            t = return_trial_count_array;

            return return_moves;
        }

        public short GetBestMove(MCTS mcts, bool use_draw, ref float win_rate)
        {
            int value_max, max_index;
            List<int> trial_count_array = new List<int>();
            List<float> win_rate_array = new List<float>();
            float f = 0f;
            //List<short> return_moves = new List<short>();
            //List<float> return_win_rate_array = new List<float>();
            //List<int> return_trial_count_array = new List<int>();

            for (int i = 0; i < BTree.RootMoves.Length; i++)
            {
                trial_count_array.Add(mcts.NodeList[i + 1].TrialCount);
            }

            for (int i = 0; i < BTree.RootMoves.Length; i++)
            {
                if (use_draw)
                {
                    if (trial_count_array[i] == 0)
                    {
                        win_rate_array.Add(0);
                    }
                    else
                    {
                        f = (mcts.NodeList[i + 1].WinCount + (float)(mcts.NodeList[i + 1].DrawCount * 0.5)) / trial_count_array[i];
                        win_rate_array.Add(f);
                    }
                }
                else
                {
                    if (trial_count_array[i] == 0)
                    {
                        win_rate_array.Add(0);
                    }
                    else
                    {
                        f = (float)mcts.NodeList[i + 1].WinCount / (float)trial_count_array[i];
                        win_rate_array.Add(f);
                    }
                }
            }

            value_max = trial_count_array.Max();
            max_index = Array.IndexOf(trial_count_array.ToArray(), value_max);
            win_rate = win_rate_array[max_index];
            //return_trial_count_array.Add(trial_count_array[max_index]);
            //trial_count_array[max_index] = int.MinValue;
            //return_moves.Add(BTree.RootMoves[max_index]);
            //return_win_rate_array.Add(win_rate_array[max_index]);
            return BTree.RootMoves[max_index];
        }

        public void Root()
        {
            float[] result_array = new float[BTree.RootMoves.Length];

            NodeList.Clear();

            Node root_node = new Node();
            NodeList.Add(root_node);

            for (int i = 0; i < BTree.RootMoves.Length; i++)// 要対応：RootMovesは全合法手を展開するか否か？
            {
                Node n = new Node();
                n.color = BTree.RootColor;
                short m = BTree.RootMoves[i];
                n.ParentIndex = 0;
                n.ThisIndex = i + 1;
                n.move = m;
                NodeList[0].ChildIndexes.Add(i + 1);
                n.PolicyResult = RootOutput[i];
                result_array[i] = RootOutput[i];
                NodeList.Add(n);
            }

            // Softmax関数はPython側でかけた方がよさそう
            while (true)
            {
                long elapsed = sw.ElapsedMilliseconds;
                if (SearchTimeLimit < elapsed)
                    break;

                if (is_abort)
                    break;

                int t, i;
                float[] ucb1_array = new float[BTree.RootMoves.Length];
                t = 0;
                i = 1;
                while (i < BTree.RootMoves.Length)
                    t += NodeList[i++].PlayoutCount;
                i = 1;
                while (i < BTree.RootMoves.Length)
                {
                    float u = NodeList[i].PolicyResult * (float)Math.Sqrt(t) / (NodeList[i].PlayoutCount + 1);
                    float q = 0.0F;
                    if (NodeList[i].EvalCount > 0 && NodeList[i].PlayoutCount > 0)
                        q = (float)((1 - value_lambda) * (NodeList[i].WinRateSum / NodeList[i].EvalCount) + value_lambda * (NodeList[i].WinCount / NodeList[i].PlayoutCount));
                    ucb1_array[i - 1] = u + q;
                    i++;
                }

                int max_index = Array.IndexOf(ucb1_array, ucb1_array.Max()) + 1;
                Do(ref BTree, BTree.RootMoves[max_index - 1], BTree.RootColor, 1);

                if (NodeList[max_index].IsLeaf)
                {
                    elapsed = sw.ElapsedMilliseconds;
                    if (SearchTimeLimit < (elapsed + TimeBuffer))
                        break;
                    if (is_abort)
                        break;

                    if (NodeList[max_index].TrialCount >= nthr)
                    {
                        ExpandNode((short)(BTree.RootColor ^ 1), max_index, (short)(BTree.ply + 1));
                        if (is_abort)
                            break;
                    }
                    else
                    {
                        Board bt = new Board();
                        bt.Init();
                        bt = bt.DeepCopy(BTree, false);
                        //BoardTreeAlloc(ref bt);
                        //bt = bt.DeepCopy(BTree, false);
                        int result = PlayOut(ref bt, (short)(BTree.RootColor ^ 1), max_index, (short)(BTree.ply + 1));
                        EvalNode(max_index, (short)(BTree.RootColor ^ 1));
                        if (is_abort)
                            break;
                        UpdateParam(max_index, result);
                    }
                }
                else
                {
                    elapsed = sw.ElapsedMilliseconds;
                    if (SearchTimeLimit < elapsed + TimeBuffer)
                        break;
                    if (is_abort)
                        break;
                    DescendNode((short)(BTree.RootColor ^ 1), max_index, nthr, (short)(BTree.ply + 1));
                }

                UnDo(ref BTree, BTree.RootMoves[max_index - 1], BTree.RootColor, 1);
            }
        }

        private void ExpandNode(short color, int parent_index, short ply)
        {
            int current_index;
            bool flag = false;

            if (!tt.PolicyMoves.ContainsKey(BTree.CurrentHash))
            {
                // トランスポジションテーブルにデータがなかった場合

                // メインスレッドにPolicy Networkのリクエストを投げる
                //string str_throw = SFEN.ToSFEN(BTree, color);
                //string str_throw = Board.BoardToString(BTree, "p", color);// ※後で盤面を文字列にする処理を追加する
                //str_throw = "p," + str_throw;
                var x = ft.MakeInputFeature(ref BTree, b_color[color]);
                queue_to_main_thread_p.Enqueue(x);

                // メインスレッドから結果を受信するまで待機する
                while (queue_from_main_thread.Count == 0) { Thread.Sleep(1); if (is_abort) { return; } }

                string str_receive = queue_from_main_thread.Dequeue(); //データのフォーマットは"SQ0 0.65,SQ1 0.25,SQ2 0.1"といった感じを想定

                string[] s = str_receive.Split(',');
                List<short> moves = new List<short>();

                for (int i = 0; i < s.Length; i += 2)
                {
                    Node n = new Node();
                    n.color = color;
                    string[] s2 = s[i].Split(" ");
                    //Move m = CSA.CSA2Move(BTree, s2[0]);
                    short m = short.Parse(s2[0]);// ※後で変換処理を追加する

                    // ※自殺手と連続コウのチェック処理を入れる
                    if (!BTree.IsMoveValid(BTree, m, color))
                        continue;

                    Do(ref BTree, m, color, ply);

                    n.ThisIndex = NodeList.Count;
                    n.ParentIndex = NodeList[parent_index].ThisIndex;
                    n.move = m;
                    moves.Add(m);

                    //Do(ref BTree, color, m);

                    NodeList[parent_index].ChildIndexes.Add(n.ThisIndex);
                    n.PolicyResult = float.Parse(s2[1]);// SoftMax関数はPython側でかける
                    tt.PolicyDict.TryAdd(BTree.CurrentHash, n.PolicyResult);
                    NodeList.Add(n);
                    current_index = n.ThisIndex;
                    Board bt = new Board();
                    bt.Init();
                    bt.DeepCopy(BTree, false);
                    //BoardTreeAlloc(ref bt);// ※後で元に戻す
                    //bt = bt.DeepCopy(BTree, false);// ※後で元に戻す
                    int result = PlayOut(ref bt, (short)(color ^ 1), current_index, (short)(ply + 1));
                    EvalNode(current_index, (short)(color ^ 1));
                    UpdateParam(current_index, result);

                    UnDo(ref BTree, m, color, ply);

                    flag = true;
                }
                //tt.PolicyMoves.Add(BTree.CurrentHash, moves);
                tt.PolicyMoves.TryAdd(BTree.CurrentHash, moves);
            }
            else
            {
                // トランスポジションテーブルにデータがあった場合

                List<short> moves = new List<short>();
                tt.PolicyMoves.TryGetValue(BTree.CurrentHash, out moves);

                if (moves.Count != 0)
                    NodeList[parent_index].IsLeaf = false;

                for (int i = 0; i < moves.Count; i++)
                {
                    Node n = new Node();
                    n.color = color;
                    short m = moves[i];
                    n.ThisIndex = NodeList.Count;
                    n.ParentIndex = NodeList[parent_index].ThisIndex;
                    n.move = m;

                    Do(ref BTree, m, color, ply);

                    NodeList[parent_index].ChildIndexes.Add(n.ThisIndex);
                    tt.PolicyDict.TryGetValue(BTree.CurrentHash, out float v);
                    n.PolicyResult = v;
                    NodeList.Add(n);
                    current_index = n.ThisIndex;
                    Board bt = new Board();
                    bt.Init();
                    bt.DeepCopy(BTree, false);
                    //BoardTreeAlloc(ref bt);// ※後で元に戻す
                    //bt = bt.DeepCopy(BTree, false);// ※後で元に戻す
                    int result = PlayOut(ref bt, (short)(color ^ 1), current_index, (short)(ply + 1));
                    EvalNode(current_index, (short)(color ^ 1));
                    UpdateParam(current_index, result);

                    UnDo(ref BTree, m, color, ply);

                    flag = true;
                }
            }

            if (flag == true)
                NodeList[parent_index].IsLeaf = false;
        }

        private int PlayOut(ref Board bt, short start_color, int node_index, short temp_ply)
        {
            const int ply_max = 361 + 1;
            const int input_dim = 361 * 20;
            //const int legal_move_size = 6;
            double[] input_feature = new double[input_dim];
            double[] vector = new double[NSquare];
            int result = 2;// 初期値は引き分け
            short ply = temp_ply;
            short color = start_color;
            int pass_cnt = 0;

            //try
            {
                while (ply < ply_max)
                {
                    /*if (ply == ply_max - 1)
                    {
                        param8.MakeFeature(BTree, ref input_feature, color);
                        vector = param8.MatMul(input_feature);

                        double win_rate = param7.MatMul(input_feature);
                        double win_rate = 0.0;
                        if (win_rate < 0.5)
                        {
                            if (color != bt.RootColor)
                            {
                                result = 0;// root手番の勝ち
                            }
                            else
                            {
                                result = 1;// 相手の勝ち
                            }
                            break;
                        }
                    }*/

                    List<short> legal_move_list = new List<short>();
                    //List<int> score_list = new List<int>();

                    /*for (int i = 0; i < legal_move_size; i++)
                    {
                        double d = vector.Max();
                        short sq = (short)Array.IndexOf(vector, d);
                        if (bt.IsMoveValid(bt, sq, color))
                            legal_move_list.Add(sq);
                    }*/
                    for (short i = 0; i < bt.pos_empty.Count; i++)
                    {
                        if (bt.IsMoveValid(bt, i, color))
                            legal_move_list.Add(i);
                    }

                    if (ply == ply_max - 1 && legal_move_list.Count > 0)
                    {
                        if (CountTexture(ref bt) == 0)
                        {
                            if (bt.RootColor == 0)
                            {
                                result = 0;// root手番の勝ち
                            }
                            else
                            {
                                result = 1;// 相手の勝ち
                            }
                        }
                        else
                        {
                            if (bt.RootColor == 0)
                            {
                                result = 1;// 相手の勝ち
                            }
                            else
                            {
                                result = 0;// root手番の勝ち
                            }
                        }
                        break;
                    }

                    if (pass_cnt < 2)
                    {
                        if (legal_move_list.Count == 0)
                        {
                            pass_cnt++;
                            continue;
                        }
                    }
                    else
                    {
                        if (CountTexture(ref bt) == 0)
                        {
                            if (bt.RootColor == 0)
                            {
                                result = 0;// root手番の勝ち
                            }
                            else
                            {
                                result = 1;// 相手の勝ち
                            }
                        }
                        else
                        {
                            if (bt.RootColor == 0)
                            {
                                result = 1;// 相手の勝ち
                            }
                            else
                            {
                                result = 0;// root手番の勝ち
                            }
                        }
                        break;
                    }

                        /*if (legal_move_list.Count == 0)
                        {
                            if (color != bt.RootColor)
                            {
                                result = 0;// root手番の勝ち
                            }
                            else
                            {
                                result = 1;// 相手の勝ち
                            }
                            break;
                        }*/



                    Random r = new Random();
                    int n = r.Next(legal_move_list.Count);
                    Do(ref bt, legal_move_list[n], color, ply);

                    color ^= 1;
                    ply++;
                }

                NodeList[node_index].PlayoutCount++;
                PlayOutCount++;
            }
            /*catch (Exception e)
            {
                result = 2;
            }*/

            return result;
        }

        private int CountTexture(ref Board bt)
        {
            int black_count = 0;
            int white_count = 0;
            for (int i = 0; i < NSquare; i++)
            {
                int limit = PosCrossTable[i].Count();
                int count = 0;
                for (int j = 0; j < limit; j++)
                {
                    int sq = PosCrossTable[i][j];
                    if (bt.seq_number_table[0, sq] != seq_max)
                    {
                        count++;
                    }
                }
                if (count == limit)
                {
                    black_count++;
                }
            }
            for (int i = 0; i < NSquare; i++)
            {
                int limit = PosCrossTable[i].Count();
                int count = 0;
                for (int j = 0; j < limit; j++)
                {
                    int sq = PosCrossTable[i][j];
                    if (bt.seq_number_table[1, sq] != seq_max)
                    {
                        count++;
                    }
                }
                if (count == limit)
                {
                    white_count++;
                }
            }
            if (black_count > white_count + 7)
            {
                return 0;// black win
            }
            return 1;// white win
        }

        private void EvalNode(int node_index, short color)
        {
            if (tt.ValueDict.TryGetValue(BTree.CurrentHash, out float f))
            {
                // トランスポジションテーブルにデータがあった場合
                NodeList[node_index].WinRateSum = 1 - f;
                NodeList[node_index].LostRateSum = f;
                NodeList[node_index].EvalCount++;
                return;
            }

            // メインスレッドにValue Networkのリクエストを投げる
            // string str_throw = Board.BoardToString(BTree, "v", color);
            //str_throw = "v," + str_throw;
            var x = ft.MakeInputFeature(ref BTree, b_color[color]);
            queue_to_main_thread_v.Enqueue(x);

            // メインスレッドから結果を受信するまで待機する
            while (queue_from_main_thread.Count == 0) { Thread.Sleep(1); if (is_abort) { return; } }

            string str_receive = queue_from_main_thread.Dequeue(); //データのフォーマットは"0.65"といった感じを想定

            float v = float.Parse(str_receive);

            // 手番側の勝率で学習させてある。1手指した後の手番の勝率が返ってくるので反転する。
            NodeList[node_index].WinRateSum = 1 - v;
            NodeList[node_index].LostRateSum = v;
            NodeList[node_index].EvalCount++;// ToDo:要るかどうか分からないので要精査

            // トランスポジションテーブルに保存する
            tt.ValueDict.TryAdd(BTree.CurrentHash, v);
        }

        private void UpdateParam(int node_index, int result)
        {
            NodeList[node_index].TrialCount += 1;
            if (result == 0)
            {
                if (NodeList[node_index].color == (int)BTree.RootColor)
                {
                    NodeList[node_index].WinCount += 1;
                }
                else
                {
                    NodeList[node_index].LostCount += 1;
                }
            }
            else if (result == 1)
            {
                if (NodeList[node_index].color == (int)BTree.RootColor)
                {
                    NodeList[node_index].LostCount += 1;
                }
                else
                {
                    NodeList[node_index].WinCount += 1;
                }
            }
            else
            {
                NodeList[node_index].DrawCount += 1;
            }
            Node current_node = NodeList[node_index];
            float delta;
            float delta2;
            // 手番側の勝率で学習させてある
            if (current_node.color == (int)BTree.RootColor)
            {
                delta = NodeList[node_index].WinRateSum;
                delta2 = NodeList[node_index].LostRateSum;
            }
            else
            {
                delta = NodeList[node_index].LostRateSum;
                delta2 = NodeList[node_index].WinRateSum;
            }
            if (current_node.ParentIndex == 0)
                return;
            while (true)
            {
                int index = current_node.ParentIndex;
                current_node = NodeList[index];
                NodeList[index].TrialCount += 1;
                NodeList[index].PlayoutCount += 1;
                if (result == 0)
                {
                    if (NodeList[index].color == (int)BTree.RootColor)
                    {
                        NodeList[index].WinCount += 1;
                    }
                    else
                    {
                        NodeList[index].LostCount += 1;
                    }
                }
                else if (result == 1)
                {
                    if (NodeList[index].color == (int)BTree.RootColor)
                    {
                        NodeList[index].LostCount += 1;
                    }
                    else
                    {
                        NodeList[index].WinCount += 1;
                    }
                }
                else
                {
                    NodeList[index].DrawCount += 1;
                }

                if (current_node.color == (int)BTree.RootColor)
                {
                    NodeList[index].WinRateSum += delta;
                    NodeList[index].LostRateSum += delta2;
                }
                else
                {
                    NodeList[index].WinRateSum += delta2;
                    NodeList[index].LostRateSum += delta;
                }

                NodeList[index].EvalCount += 1;
                if (current_node.ParentIndex == 0)
                    break;
            }
        }

        void DescendNode(short color, int node_index, int nthr, short ply)
        {
            if (NodeList[node_index].ChildIndexes.Count == 0)
                return;

            int idx = new int();
            bool flag = false;
            while (true)
            {
                long elapsed = sw.ElapsedMilliseconds;
                if (SearchTimeLimit < elapsed + TimeBuffer)
                    break;
                if (is_abort)
                    break;
                int t, i;
                float[] ucb1_array = new float[NodeList[node_index].ChildIndexes.Count];
                t = 0;
                i = 0;
                while (i < NodeList[node_index].ChildIndexes.Count)
                {
                    idx = NodeList[node_index].ChildIndexes[i];
                    t += NodeList[idx].PlayoutCount;
                    i++;
                }

                i = 0;
                while (i < NodeList[node_index].ChildIndexes.Count)
                {
                    idx = NodeList[node_index].ChildIndexes[i];
                    float u = NodeList[idx].PolicyResult * (float)Math.Sqrt(t) / (NodeList[idx].PlayoutCount + 1);
                    float q = 0.0F;
                    if (NodeList[idx].EvalCount > 0 && NodeList[idx].PlayoutCount > 0)
                        q = (float)((1 - value_lambda) * (NodeList[idx].WinRateSum / NodeList[idx].EvalCount) + value_lambda * (NodeList[idx].WinCount / NodeList[idx].PlayoutCount));
                    ucb1_array[i++] = u + q;
                }

                int max_index = Array.IndexOf(ucb1_array, ucb1_array.Max());
                idx = NodeList[node_index].ChildIndexes[max_index];
                Do(ref BTree, NodeList[idx].move, color, ply);

                if (NodeList[idx].IsLeaf)
                {
                    elapsed = sw.ElapsedMilliseconds;
                    if (SearchTimeLimit < elapsed + TimeBuffer)
                    {
                        flag = true;
                        goto end;
                    }

                    if (is_abort)
                        break;

                    if (NodeList[idx].TrialCount >= nthr)
                    {
                        ExpandNode((short)(color ^ 1), idx, (short)(ply + 1));
                        if (is_abort)
                            break;
                    }
                    else
                    {
                        Board bt = new Board();
                        bt.Init();
                        bt.DeepCopy(BTree, false);
                        int result = PlayOut(ref bt, (short)(color ^ 1), idx, (short)(ply + 1));
                        EvalNode(idx, (short)(color ^ 1));
                        if (is_abort)
                            break;
                        UnDo(ref BTree, NodeList[idx].move, color, ply);
                        AscendNode((short)(color ^ 1), idx, result, ply);
                        return;
                    }
                }
                else
                {
                    elapsed = sw.ElapsedMilliseconds;
                    if (SearchTimeLimit < elapsed + TimeBuffer)
                    {
                        flag = true;
                        goto end;
                    }

                    if (is_abort)
                        break;

                    DescendNode((short)(color ^ 1), idx, nthr, (short)(ply + 1));
                    return;
                }

                UnDo(ref BTree, NodeList[idx].move, (short)color, (short)ply);

            end:

                if (flag)
                {
                    Node current_node = NodeList[idx];
                    short temp_color = (short)(color ^ 1);
                    short temp_ply = (short)(ply - 1);// バグかもしれないので要チェック
                    while (true)
                    {
                        int index = current_node.ParentIndex;
                        current_node = NodeList[index];
                        if (current_node.ParentIndex == 0)
                            break;
                        UnDo(ref BTree, current_node.move, temp_color, temp_ply);
                        temp_color ^= 1;
                    }
                    break;
                }
            }
        }

        void AscendNode(short color, int node_index, int result, short ply)// パラメータ plyは要らないか？
        {
            NodeList[node_index].TrialCount += 1;
            if (result == 0)
            {
                if (NodeList[node_index].color == (int)BTree.RootColor)
                {
                    NodeList[node_index].WinCount += 1;
                }
                else
                {
                    NodeList[node_index].LostCount += 1;
                }
            }
            else if (result == 1)
            {
                if (NodeList[node_index].color == (int)BTree.RootColor)
                {
                    NodeList[node_index].LostCount += 1;
                }
                else
                {
                    NodeList[node_index].WinCount += 1;
                }
            }
            else
            {
                NodeList[node_index].DrawCount += 1;
            }
            Node current_node = NodeList[node_index];
            float delta;
            float delta2;
            // 手番側の勝率で学習させてある
            if (current_node.color == (int)BTree.RootColor)
            {
                delta = NodeList[node_index].WinRateSum;
                delta2 = NodeList[node_index].LostRateSum;
            }
            else
            {
                delta = NodeList[node_index].LostRateSum;
                delta2 = NodeList[node_index].WinRateSum;
            }
            if (current_node.ParentIndex == 0)
                return;
            short temp_color = color;
            short temp_ply = (short)(ply - 1);// バグかもしれないので要チェック
            while (true)
            {
                int index = current_node.ParentIndex;
                current_node = NodeList[index];
                NodeList[index].TrialCount += 1;
                NodeList[index].PlayoutCount += 1;
                if (result == 0)
                {
                    if (NodeList[index].color == (int)BTree.RootColor)
                    {
                        NodeList[index].WinCount += 1;
                    }
                    else
                    {
                        NodeList[index].LostCount += 1;
                    }
                }
                else if (result == 1)
                {
                    if (NodeList[index].color == (int)BTree.RootColor)
                    {
                        NodeList[index].LostCount += 1;
                    }
                    else
                    {
                        NodeList[index].WinCount += 1;
                    }
                }
                else
                {
                    NodeList[index].DrawCount += 1;
                }

                if (current_node.color == (int)BTree.RootColor)
                {
                    NodeList[index].WinRateSum += delta;
                    NodeList[index].LostRateSum += delta2;
                }
                else
                {
                    NodeList[index].WinRateSum += delta2;
                    NodeList[index].LostRateSum += delta;
                }

                NodeList[index].EvalCount += 1;
                if (current_node.ParentIndex == 0)
                    break;
                UnDo(ref BTree, current_node.move, temp_color, temp_ply);
                temp_color ^= 1;
                temp_ply -= 1;
            }
        }

        private void WriteRecord()
        {
            record.moves = new short[BTree.ply];
            record.str_moves = new string[BTree.ply];
            record.players[0] = "Achernar";
            record.players[1] = "Achernar";
            for (int i = 0; i < BTree.current_moves.Count; i++)
            {
                record.moves[i] = BTree.current_moves[i];
                record.str_moves[i] = ShortToStr(BTree.current_moves[i]);
            }
            record.ply = BTree.ply;
        }

        private void OutBoard(Board bt)
        {
            string str_out = "";
            int cnt = 0;
            for (int i = 0; i < Common.NSquare; i++)
            {
                switch (bt.board[i])
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
