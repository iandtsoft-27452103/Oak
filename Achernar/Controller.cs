using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tensorboard;
using TorchSharp;
using TorchSharp.Modules;
using static Achernar.Board;
//using static Achernar.Position;
using static Achernar.Common;
//using static Achernar.Feature;
using static Achernar.IO;
using static Achernar.Layer;
using static Achernar.Record;
using static System.Net.Mime.MediaTypeNames;
using static TorchSharp.torch.nn;

namespace Achernar
{
    internal class Controller
    {
        //public MoveFeature mf;
        //public Mate3Ply m3p;
        //public TT tt;

        //public Controller()
        //{
            //mf = new MoveFeature();
            //mf.LoadFeature();
            //mf.ProgressInit();
            //Console.WriteLine(mf.move_feature.Max());
            //m3p = new Mate3Ply();
            //tt = new TT();
        //}

        public void ReceiveLoop(ref Board bt, Communicate cm, int task_num, ref float[] root_policy_result, ref List<short> move, ref List<float> f, ref List<int> t, int thinking_time)
        {
            MCTS[] mcts_array = new MCTS[task_num];
            //Mate[] mate_array = new Mate[mate_task_num];
            //bool[] is_mate = new bool[mate_task_num];
            Task[] tasks = new Task[task_num];
            //Task[] mate_tasks = new Task[mate_task_num];

            try
            {
                for (int i = 0; i < task_num; i++)
                {
                    mcts_array[i] = new MCTS();
                    mcts_array[i].BTree = bt.DeepCopy(bt, true);//※実装後に戻す
                    //mcts_array[i].tt = tt; // ToDo: 探索終了後のTTを保存しておくと良いかも
                    mcts_array[i].is_abort = false;
                    mcts_array[i].is_finished = false;
                    mcts_array[i].RootOutput = root_policy_result;
                    mcts_array[i].SearchTimeLimit = thinking_time * 1000;
                    mcts_array[i].sw.Start();
                    //mcts_array[i].param7.LoadParam();
                    //mcts_array[i].param8.LoadParam();
                    tasks[i] = Task.Run(mcts_array[i].Root);//※実装後に戻す
                }

                while (true)
                {
                    if (IsCompleted(ref tasks, task_num))
                    {
                        break;
                    }

                    //ConfirmQueue(mcts_array, task_num, cm);
                }

                List<short> moves = new List<short>();
                List<float> win_rates = new List<float>();
                List<int> trial_counts = new List<int>();

                moves = mcts_array[0].TotalParam(mcts_array, ref win_rates, ref trial_counts, true);//※実装後に戻す

                move = moves;
                f = win_rates;
                t = trial_counts;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void ReceiveLoop(ref Board bt, ref Prediction prediction, int task_num, ref float[] root_policy_result, ref List<short> move, ref List<float> f, ref List<int> t, int thinking_time)
        {
            MCTS[] mcts_array = new MCTS[task_num];
            //Mate[] mate_array = new Mate[mate_task_num];
            //bool[] is_mate = new bool[mate_task_num];
            Task[] tasks = new Task[task_num];
            //Task[] mate_tasks = new Task[mate_task_num];

            try
            {
                for (int i = 0; i < task_num; i++)
                {
                    mcts_array[i] = new MCTS();
                    mcts_array[i].BTree = bt.DeepCopy(bt, true);//※実装後に戻す
                    //mcts_array[i].tt = tt; // ToDo: 探索終了後のTTを保存しておくと良いかも
                    mcts_array[i].is_abort = false;
                    mcts_array[i].is_finished = false;
                    mcts_array[i].RootOutput = root_policy_result;
                    mcts_array[i].SearchTimeLimit = thinking_time * 1000;
                    mcts_array[i].sw.Start();
                    //mcts_array[i].param7.LoadParam();
                    //mcts_array[i].param8.LoadParam();
                    tasks[i] = Task.Run(mcts_array[i].Root);//※実装後に戻す
                }

                while (true)
                {
                    if (IsCompleted(ref tasks, task_num))
                    {
                        break;
                    }

                    ConfirmQueue(mcts_array, task_num, ref prediction);
                }

                List<short> moves = new List<short>();
                List<float> win_rates = new List<float>();
                List<int> trial_counts = new List<int>();

                moves = mcts_array[0].TotalParam(mcts_array, ref win_rates, ref trial_counts, true);//※実装後に戻す

                move = moves;
                f = win_rates;
                t = trial_counts;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SelfPlayReceiveLoop(SelfPlay sp, int task_num, int game_num,Communicate cm, int thinking_time, bool is_console_out)
        {
            const int ply_book_limit = 16;
            const string read_file_name = "records.txt";
            List<Book> books = new List<Book>();
            books = IO.ReadBookFile(read_file_name, ply_book_limit);
            MCTS[] mcts_array = new MCTS[task_num];
            Task[] tasks = new Task[task_num];
            Board bt = new Board();
            bt.Init();

            // (1) 盤面を初期局面で初期化する
            // (2) 定石を選択し、指定局面まで局面更新する
            // (3) Policy Networkを実行し、RootOutputに保存する
            // (4) 探索パラメータを初期化する

            int[] game_cnt = new int[task_num];
            bool[] is_completed = new bool[task_num];
            Record[] record = new Record[task_num];
            bool flag = false;
            //int index = task_num;
            int index = int.MaxValue;

            // タスクの初期化処理
            for (int i = 0; i < task_num; i++)
            {
                record[i] = new Record();
                record[i].winner = 0;
                record[i].players = new string[2];
                record[i].players[0] = "";
                record[i].players[1] = "";
                sp.MCTSInit(ref mcts_array[i], ref bt, ref record[i], thinking_time, i, is_console_out);
            }
                
                //mcts_array[i].RootOutput = root_policy_result;
                //mcts_array[i].sw.Start();
                //tasks[i] = Task.Run(mcts_array[i].Root);//※実装後に戻す

            // 対局待ちのループ
            while (true)
            {
                if (!flag)
                {
                    flag = true;
                    for (int i = 0; i < task_num; i++)
                    {
                        sp.PrepareStartGame(ref mcts_array[i], books, ply_book_limit, ref cm);
                        sp.StartGame(ref mcts_array[i], ref tasks[i]);
                        Console.WriteLine("task" + i.ToString() + " Game 1 is Started.");
                    }                      
                }
                else
                {
                    if (is_completed[index])
                    {
                        is_completed[index] = false;
                        game_cnt[index]++;

                        // 棋譜を書き出す
                        sp.WriteRecord(mcts_array[index], index);
                        
                        if (game_cnt[index] < game_num)
                        {
                            record[index] = new Record();
                            record[index].winner = 0;
                            record[index].players = new string[2];
                            record[index].players[0] = "";
                            record[index].players[1] = "";
                            bt = new Board();
                            bt.Init();
                            sp.MCTSInit(ref mcts_array[index], ref bt, ref record[index], thinking_time, index, is_console_out);
                            sp.PrepareStartGame(ref mcts_array[index], books, ply_book_limit, ref cm);
                            sp.StartGame(ref mcts_array[index], ref tasks[index]);
                            Console.WriteLine("task" + index.ToString() + " Game " + (game_cnt[index] + 1).ToString() + " is Started.");
                            index = int.MaxValue;
                            //break;// 2022.11.22 修正
                        }
                    }

                    int counter = 0;
                    for (int i = 0; i < task_num; i++)
                    {
                        if (game_cnt[i] == game_num)
                            counter++;
                    }

                    if (counter == task_num)
                        break;
                }

                // 対局のループ
                while (true)
                {
                    index = IsCompleted(ref tasks, task_num, ref is_completed, game_cnt, game_num);
                    if (index != int.MaxValue)
                        break;

                    //ConfirmQueue(mcts_array, task_num, cm);
                }
            }

            /*while (true)
            {
                if (IsCompleted(ref tasks, task_num))
                {
                    break;
                }

                ConfirmQueue(mcts_array, task_num, cm);
            }*/
        }

        private bool IsCompleted(ref Task[] tasks, int task_num)
        {
            bool iret = false;
            int counter = 0;
            for (int i = 0; i < task_num; i++)
            {
                if (tasks[i].Status == TaskStatus.RanToCompletion)
                    counter++;
            }

            if (counter == task_num)
                iret = true;

            return iret;
        }

        private int IsCompleted(ref Task[] tasks, int task_num, ref bool[] flag, int[] game_cnt, int game_num)
        {
            int index = int.MaxValue;
            for (int i = 0; i < task_num; i++)
            {
                if (tasks[i].Status == TaskStatus.RanToCompletion && game_cnt[i] < game_num)
                {
                    flag[i] = true;
                    index = i;
                }
            }

            return index;
        }

        /*private void ConfirmQueue(MCTS[] mcts_array, int task_num, Communicate com)
        {
            try
            {
                string[] str = new string[task_num];
                List<int> index_array = new List<int>();
                for (int i = 0; i < task_num; i++)
                {
                    if (mcts_array[i].queue_to_main_thread.Count != 0)
                    {
                        str[i] = mcts_array[i].queue_to_main_thread.Dequeue();
                        index_array.Add(i);
                        //task_number[i] = i;
                    }
                }

                List<string> str_p = new List<string>();
                List<int> p_index_array = new List<int>();
                List<string> str_v = new List<string>();
                List<int> v_index_array = new List<int>();

                for (int i = 0; i < str.Length; i++)
                {
                    //Console.WriteLine("confirm_queue_loop");
                    if (str[i] != "" && str[i] != null)
                    {
                        string[] s = str[i].Split(',');
                        if (s[0] == "p")
                        {
                            str_p.Add(s[1]);
                            p_index_array.Add(i);
                        }
                        else
                        {
                            str_v.Add(s[1]);
                            v_index_array.Add(i);
                        }
                    }
                }

                // ToDo: PythonにPolicy NetworkまたはValue Networkの実行を投げる処理を書く
                if (str_p.Count != 0)
                {
                    string s = "p,";
                    for (int i = 0; i < str_p.Count; i++)
                    {
                        s += str_p[i];
                        if (i != str_p.Count - 1)
                            s += ",";
                    }
                    com.ThrowRequest(s);
                    string s_ret = com.ReceiveResponse();
                    string[] s_array = s_ret.Split(":");
                    for (int i = 0; i < str_p.Count; i++)
                        mcts_array[p_index_array[i]].queue_from_main_thread.Enqueue(s_array[i]);

                    for (int i = 0; i < str_p.Count; i++)
                        while (mcts_array[p_index_array[i]].queue_from_main_thread.Count > 0) { Thread.Sleep(1); }
                }

                if (str_v.Count != 0)
                {
                    string s = "v,";
                    for (int i = 0; i < str_v.Count; i++)
                    {
                        s += str_v[i];
                        if (i != str_v.Count - 1)
                            s += ",";
                    }
                    com.ThrowRequest(s);
                    string s_ret = com.ReceiveResponse();
                    string[] s_array = s_ret.Split(",");
                    for (int i = 0; i < str_v.Count; i++)
                        mcts_array[v_index_array[i]].queue_from_main_thread.Enqueue(s_array[i]);
                    for (int i = 0; i < str_v.Count; i++)
                        while (mcts_array[v_index_array[i]].queue_from_main_thread.Count > 0) { Thread.Sleep(1); }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }*/

        // TorchSharp用
        private void ConfirmQueue(MCTS[] mcts_array, int task_num, ref Prediction prediction)
        {
            try
            {
                string[] str = new string[task_num];
                List<torch.Tensor> xp = new List<torch.Tensor>();
                List<torch.Tensor> xv = new List<torch.Tensor>();
                List<int> index_array = new List<int>();
                var d = torch.device(DeviceType.CUDA);
                for (int i = 0; i < task_num; i++)
                {
                    if (mcts_array[i].queue_to_main_thread_p.Count != 0)
                    {
                        //str[i] = mcts_array[i].queue_to_main_thread_p.Dequeue();
                        //x[i] = mcts_array[i].queue_to_main_thread_p.Dequeue();
                        xp.Add(mcts_array[i].queue_to_main_thread_p.Dequeue());
                        str[i] = "p";
                        index_array.Add(i);
                        //task_number[i] = i;
                    }
                    if (mcts_array[i].queue_to_main_thread_v.Count != 0)
                    {
                        //str[i] = mcts_array[i].queue_to_main_thread_p.Dequeue();
                        xv.Add(mcts_array[i].queue_to_main_thread_v.Dequeue());
                        str[i] = "v";
                        index_array.Add(i);
                        //task_number[i] = i;
                    }
                }

                List<string> str_p = new List<string>();
                List<int> p_index_array = new List<int>();
                List<string> str_v = new List<string>();
                List<int> v_index_array = new List<int>();

                for (int i = 0; i < task_num; i++)
                {
                    //Console.WriteLine("confirm_queue_loop");
                    //if (str[i] != "" && str[i] != null)
                    if (index_array.Contains(i))
                    {
                        //string[] s = str[i].Split(',');
                        if (str[i] == "p")
                        {
                            str_p.Add(str[i]);
                            p_index_array.Add(i);
                        }
                        else
                        {
                            str_v.Add(str[i]);
                            v_index_array.Add(i);
                        }
                    }
                }

                if (str_p.Count != 0)
                {
                    if (str_p.Count > 1)
                    {
                        int xxx = 0;
                    }
                    /*string s = "p,";
                    for (int i = 0; i < str_p.Count; i++)
                    {
                        s += str_p[i];
                        if (i != str_p.Count - 1)
                            s += ",";
                    }*/
                    //com.ThrowRequest(s);
                    //string s_ret = com.ReceiveResponse();
                    var x = torch.zeros(str_p.Count, 44, 19, 19).to(d);
                    for (int i = 0; i < str_p.Count; i++)
                    {
                        x[i] = xp[i].reshape(44, 19, 19).to(d);
                    }
                    var y = prediction.ExecPolicy(x);
                    string s_ret = "";
                    for (int i = 0; i < str_p.Count; i++)
                    {
                        torch.Tensor y2 = torch.sigmoid(y[i][1].reshape(1, 361));
                        short y3;
                        int count = 0;
                        while (count < 4)
                        {
                            y3 = torch.argmax(y2).ToInt16();
                            s_ret += y3.ToString() + " " + y2[0][y3].ToSingle().ToString();
                            if (count < 3)
                            {
                                s_ret += ",";
                            }
                            //policy_result[count++] = y2[0][y3].ToSingle();
                            y2[0][y3] = float.MinValue;
                            count++;// 仮の処置
                        }
                        if (i < str_p.Count - 1)
                        {
                            s_ret += ":";
                        }
                        /*float policy_sum = policy_result.Sum();
                        for (int j = 0; j < 4; j++)
                        {
                            policy_result[j] /= policy_sum;
                        }*/
                    }
                    string[] s_array = s_ret.Split(":");
                    for (int i = 0; i < str_p.Count; i++)
                        mcts_array[p_index_array[i]].queue_from_main_thread.Enqueue(s_array[i]);

                    for (int i = 0; i < str_p.Count; i++)
                        while (mcts_array[p_index_array[i]].queue_from_main_thread.Count > 0) { Thread.Sleep(1); }
                }

                if (str_v.Count != 0)
                {
                    /*string s = "v,";
                    for (int i = 0; i < str_v.Count; i++)
                    {
                        s += str_v[i];
                        if (i != str_v.Count - 1)
                            s += ",";
                    }*/
                    //com.ThrowRequest(s);
                    //string s_ret = com.ReceiveResponse();
                    var x = torch.zeros(str_v.Count, 44, 19, 19).to(d);
                    for (int i = 0; i < str_v.Count; i++)
                    {
                        x[i] = xv[i].reshape(44, 19, 19).to(d);
                    }

                    if (str_v.Count > 1)
                    {
                        int xxx = 0;
                    }

                    var y = prediction.ExecValue(x);
                    //var y2 = y[0].ToSingle();
                    string s_ret = "";
                    for (int i = 0; i < str_v.Count; i++)
                    {
                        s_ret += y[i][0].ToSingle().ToString();
                        if (i != str_v.Count - 1)
                            s_ret += ",";
                    }
                    string[] s_array = s_ret.Split(",");
                    for (int i = 0; i < str_v.Count; i++)
                        mcts_array[v_index_array[i]].queue_from_main_thread.Enqueue(s_array[i]);
                    for (int i = 0; i < str_v.Count; i++)
                        while (mcts_array[v_index_array[i]].queue_from_main_thread.Count > 0) { Thread.Sleep(1); }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
