using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Achernar
{
    internal class SelfPlay
    {

        public void SelfPlayWrapper(int task_num, int game_num, int thinking_time, bool is_console_out)
        {
            Communicate cm = new Communicate();
            cm.Boot();
            Controller controller = new Controller();
            controller.SelfPlayReceiveLoop(this, task_num, game_num, cm, thinking_time, is_console_out);
            cm.Quit();
            cm.Dispose();
        }

        public void MCTSInit(ref MCTS mcts, ref Board bt, ref Record record, int thinking_time, int task_number, bool is_console_out)
        {
            mcts = new MCTS();
            mcts.TaskNumber = task_number;
            mcts.BTree = bt.DeepCopy(bt, false);
            mcts.is_abort = false;
            mcts.is_finished = false;
            mcts.SearchTimeLimit = thinking_time * 1000;
            mcts.record = record;
            //mcts.param7.LoadParam();
            mcts.is_console_out = is_console_out;
        }

        public void PrepareStartGame(ref MCTS mcts, List<Book> books, int ply_book_limit, ref Communicate cm)
        {
            // 定石レコードから定石を選び、指定局面まで再生する。
            Random r = new Random();
            int record_number = r.Next(books.Count);
            r = new Random();
            int n = r.Next(0, ply_book_limit);
            //int n = 4;
            short color = 0;
            short ply = 1;
            for (short j = 0; j < n; j++)
            {
                MakeMove.Do(ref mcts.BTree, books[record_number].moves[j], color, ply++);
                color ^= 1;
            }

            // Policy Networkを実行する
            string str_to_python = "p," + Board.BoardToString(mcts.BTree, "p", color);
            cm.ThrowRequest(str_to_python);
            string s = cm.ReceiveResponse();
            float[] policy_result = new float[1];
            mcts.BTree.SetRootPos(ref mcts.BTree, s, color, ref policy_result);
            mcts.RootOutput = new float[policy_result.Length];
            mcts.RootOutput = policy_result;
        }

        public void StartGame(ref MCTS mcts, ref Task task)
        {
            //task = Task.Run(mcts.Game);//※実装後に戻す
        }

        public void WriteRecord(MCTS mcts, int task_index)
        {
            const string cm = ",";
            string file_name = "self_play_record" + task_index.ToString() + ".txt";
            string str_out = "";
            StreamWriter sw = IO.OpenStreamWriter(file_name, true);
            str_out += mcts.record.players[0] + cm;
            str_out += mcts.record.players[1] + cm;
            if (mcts.record.winner == 0)
            {
                str_out += "B+Resign" + cm;
            }
            else
            {
                str_out += "W+Resign" + cm;
            }

            for (int i = 0; i < mcts.record.str_moves.Length; i++)
            {
                str_out += mcts.record.str_moves[i];
                if (i != mcts.record.str_moves.Length - 1)
                {
                    str_out += cm;
                }
            }

            sw.WriteLine(str_out);
            sw.Close();
        }
    }
}
