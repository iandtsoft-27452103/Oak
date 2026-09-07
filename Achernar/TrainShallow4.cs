using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Achernar.IO;
using static Achernar.Record;
using static Achernar.Board;
using static Achernar.Layer;
using static Achernar.Position;
using static Achernar.Common;
using static Achernar.Evaluate;
using System.Drawing;
using TorchSharp;

namespace Achernar
{
    // FVは3石の配置, Bonanza Method
    internal class TrainShallow4
    {
        float[,] param_pb = new float[NSquare, NSquare];
        float[,] param_pw = new float[NSquare, NSquare];
        float[,,] param_bbb = new float[NSquare, NSquare, NSquare];
        float[,,] param_bww = new float[NSquare, NSquare, NSquare];
        float[,,] param_wbb = new float[NSquare, NSquare, NSquare];
        float[,,] param_www = new float[NSquare, NSquare, NSquare];
        Evaluate eval = new Evaluate();
        public const int FV_WINDOW = 256;
        const int STEP_SIZE = 4;
        const float eps = 1e-10f;
        public const int batch_size = 1 * 12;
        public int iteration_limit = 1;
        public const int thread_num = 12;
        public int[] array_prev_value = new int[thread_num];
        //public Evaluate2 obj_eval = new Evaluate2();
        public bool is_load_model = false;

        public struct LearnParse
        {
            public float target;
            public float sum_dT;
            public long num_moves_counted;
            public LearnParse()
            {
                target = 0.0f;
                sum_dT = 0.0f;
                num_moves_counted = 0;
            }
        }
        public void Train(int file_number, bool is_load_model, bool is_load_optim)
        {
            //int batch_size = (int)Math.Pow(2, 20);// 1048576
            //int batch_size = (int)Math.Pow(2, 15);
            //int batch_size = 1572864;
            //int batch_size;
            //Board bt = new Board();
            Random r = new Random();
            file_number = r.Next(0, 3);
            string record_file_name = "records" + file_number.ToString() + ".txt";
            //record_file_name = "test_records.txt";

            Console.WriteLine("Train Start!\n");
            DateTime dt = DateTime.Now;
            Console.WriteLine(dt.ToString() + "\n");

            if (is_load_model)
                eval.LoadFV();

            Console.WriteLine("1. Read Record File.");

            List<Record> records = ReadRecordFile(record_file_name);
            int record_count = records.Count;
            //batch_size = record_count;

            List<Position> positions = new List<Position>();
            //Position obj_pos = new Position();
            //positions = obj_pos.Alloc(records);

            Random rm = new Random();
            positions = positions.OrderBy(x => rm.Next()).ToList();

            LearnParse lp = new LearnParse();


            for (int i = 0; i < iteration_limit; i++)
            {
                Console.WriteLine("2. Start CalcGradients.");
                Task[] tasks = new Task[thread_num];
                //int data_size = teachers.Length / thread_num;
                int data_size = batch_size / thread_num;
                int start_num;
                int end_num;
                // メインスレッドが空いている状態
                for (int j = 0; j < thread_num; j++)
                {
                    start_num = j * data_size;
                    end_num = (j + 1) * data_size;
                    //Position[] positions2 = new Position[end_num - start_num];
                    //positions2 = positions.Skip(start_num).Take(end_num - start_num).ToArray();
                    //CalcGradientsMother(j, teachers2, ref lp, start_num, end_num);
                    //tasks[j] = Task.Factory.StartNew(() => CalcGradientsMother(j + 1, teachers2, ref lp_tree[j], start_num, end_num));
                    tasks[j] = Task.Factory.StartNew(() => CalcGradientsMother(j + 1, records, ref lp, start_num, end_num));
                    //if (j == 1)
                    //Task.WaitAll(tasks[0], tasks[1]);
                    //CalcGradientsMother(j + 1, teachers, ref lp_tree[j], start_num, end_num);
                    if (j == thread_num - 1)
                        Task.WaitAll(tasks);
                }
            }
            UpdateFV(ref eval.fv_pb, param_pb);
            UpdateFV(ref eval.fv_pw, param_pw);
            UpdateFV(ref eval.fv_bbb, param_bbb);
            UpdateFV(ref eval.fv_bww, param_bww);
            UpdateFV(ref eval.fv_wbb, param_wbb);
            UpdateFV(ref eval.fv_www, param_www);

            eval.SaveFV();

            //nt xxx = 0;
            /*for (int i = 0; i < batch_size; i++)
            {
                Position pos = positions[i];
                Record record = records[pos.record_number];
                bt.Init();
                int ply_limit = pos.ply;
                short color = 0;
                for (short j = 0; j < ply_limit; j++)
                {
                    MakeMove.Do(ref bt, record.moves[j], color, (short)(j + 1));
                    color ^= 1;
                }
                Console.WriteLine("i = " + (i + 1).ToString());
            }*/
        }

        private void CalcGradientsMother(int thread_id, List<Record> records, ref LearnParse lp, int start_num, int end_num)
        {
            Board bt = new Board();
            int counter = 0;
            int prev_value = 0;
            for (int i = start_num; i < end_num; i++)
            {
                Record record = new Record();
                bt.Init();
                lock (records)
                {
                    record = records[i];
                    Console.WriteLine("record number = " + (counter + 1).ToString());
                }
                short color = 0;
                for (int j = 0; j < record.moves.Length; j++)
                {
                    //MakeMove.Do(ref bt, record.moves[j], color, (short)(j + 1));
                    CalcGradients(ref lp, ref bt, record.moves[j], color, (short)(j + 1), ref prev_value);
                    color ^= 1;
                }
                counter++;
            }
        }

        private void CalcGradients(ref LearnParse lp, ref Board bt, short record_move, short color, short ply, ref int p_v)
        {
            long nc = 0;
            int prev_value = 0;
            int record_value = 0;
            int value = 0;
            float target, dT, sum_dT;

            target = 0.0f;
            sum_dT = 0.0f;
            dT = 0.0f;

            if (ply != 1)
            {
                //prev_value = (color == 0) ? -eval.EvaluateAll(bt) : eval.EvaluateAll(bt);
                prev_value = p_v;
            }

            MakeMove.Do(ref bt, record_move, color, ply);

            //record_value = (color == 0) ? obj_eval.EvaluateAll(bt) : -obj_eval.EvaluateAll(bt);
            List<short> li_b = new List<short>();
            List<short> li_w = new List<short>();
            eval.MakeList(bt, ref li_b, ref li_w);
            record_value = (color == 0) ? eval.EvaluateDiff(bt, color, record_move, (short)prev_value, ref li_b, ref li_w) : -eval.EvaluateDiff(bt, color, record_move, (short)prev_value, ref li_b, ref li_w);
            p_v = record_value;
            if (color == 1)
                p_v = -p_v;

            MakeMove.UnDo(ref bt, record_move, color, ply);
            List<short> legal_move_list = new List<short>();
            for (short i = 0; i < bt.pos_empty.Count; i++)
            {
                if (bt.IsMoveValid(bt, i, color))
                    legal_move_list.Add(i);
            }
            nc += legal_move_list.Count;
            for (int i = 0; i < legal_move_list.Count; i++)
            {
                short move = legal_move_list[i];
                if (move == record_move)
                {
                    continue;
                }
                MakeMove.Do(ref bt, move, color, ply);
                //value = (color == 0) ? eval.EvaluateAll(bt) : -eval.EvaluateAll(bt);
                li_b = new List<short>();
                li_w = new List<short>();
                eval.MakeList(bt, ref li_b, ref li_w);
                int temp_value = (color == 0) ? eval.EvaluateDiff(bt, color, move, (short)prev_value, ref li_b, ref li_w) : -eval.EvaluateDiff(bt, color, move, (short)prev_value, ref li_b, ref li_w);
                target += Sigmoid(value - record_value);
                dT = (color == 0) ? dSigmoid(value - record_value) : -dSigmoid(value - record_value);
                sum_dT += dT;
                IncParam(ref bt, ref lp, -dT);
                MakeMove.UnDo(ref bt, move, color, ply);
            }
            MakeMove.Do(ref bt, record_move, color, ply);
            lp.num_moves_counted += nc;
            lp.sum_dT += sum_dT;
            lp.target += target;
            IncParam(ref bt, ref lp, sum_dT);
        }

        private float Sigmoid(float x)
        {
            const float delta = (float)(FV_WINDOW / 7.0);
            float d;

            if (x < -FV_WINDOW) { x = -FV_WINDOW; }
            else if (x > FV_WINDOW) { x = FV_WINDOW; }
            d = 1.0f / (1.0f + (float)Math.Exp(-x / delta));
            return d;
        }

        private float dSigmoid(float x)
        {
            const float delta = (float)(FV_WINDOW / 7.0);
            float dd, dn, dtemp, dret;

            if (x <= -FV_WINDOW) { dret = 0.0f; }
            else if (x >= FV_WINDOW) { dret = 0.0f; }
            else
            {
                dn = (float)(Math.Exp(-x / delta));
                dtemp = dn + 1.0f;
                dd = delta * dtemp * dtemp;
                dret = dn / dd;
            }

            return dret;
        }

        private void IncParam(ref Board bt, ref LearnParse lp, float dinc)
        {
            int i, j, k;
            short temp_color = 0;
            const int fv_scale = 32;
            float p = dinc / fv_scale;

            for (i = 1; i <= bt.ply; i++)
            {
                if (temp_color == 0)
                {
                    param_pb[i - 1, bt.current_moves[i - 1]] += p;
                }
                else
                {
                    param_pw[i - 1, bt.current_moves[i - 1]] -= p;
                }
                temp_color ^= 1;
            }

            List<short> li_b = new List<short>();
            List<short> li_w = new List<short>();

            eval.MakeList(bt, ref li_b, ref li_w);
            for (i = 0; i < li_b.Count; i++)
            {
                for (j = i + 1; j < li_b.Count; j++)
                {
                    for (k = j + 1; k < li_b.Count; k++)
                    {
                        param_bbb[li_b[i], li_b[j], li_b[k]] += p;
                    }
                }
            }

            for (i = 0; i < li_w.Count; i++)
            {
                for (j = 0; j < li_b.Count; j++)
                {
                    for (k = 0; k < li_b.Count; k++)
                    {
                        param_wbb[li_w[i], li_b[j], li_b[k]] += p;
                    }
                }
            }

            for (i = 0; i < li_w.Count; i++)
            {
                for (j = i + 1; j < li_w.Count; j++)
                {
                    for (k = j + 1; k < li_w.Count; k++)
                    {
                        param_www[li_w[i], li_w[j], li_w[k]] -= p;
                    }
                }
            }

            for (i = 0; i < li_b.Count; i++)
            {
                for (j = 0; j < li_w.Count; j++)
                {
                    for (k = 0; k < li_w.Count; k++)
                    {
                        param_bww[li_b[i], li_w[j], li_w[k]] -= p;
                    }
                }
            }

        }

        /*private void UpdateLoop(ref LearnParse lp)
        {
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    Update.UpdateParam(ref eval.fv_pb[i, j], param_pb[i, j]);
                    Update.UpdateParam(ref eval.fv_pw[i, j], param_pw[i, j]);
                    Update.UpdateParam(ref eval.fv_bb[i, j], param_bb[i, j]);
                    Update.UpdateParam(ref eval.fv_bw[i, j], param_bw[i, j]);
                    Update.UpdateParam(ref eval.fv_wb[i, j], param_wb[i, j]);
                    Update.UpdateParam(ref eval.fv_ww[i, j], param_ww[i, j]);
                }
            }
        }*/

        public static class Update
        {
            public static uint urand = 0;
            public static uint uc = 31;
            public const float FV_PENALTY = 0.00625f;
            public const short SHRT_MAX = 32767;
            public const short SHRT_MIN = -32768;

            public static void UpdateParam(ref short pv, double dv)
            {
                int v, istep;

                istep = BRand();
                istep += BRand();
                v = pv;

                if (v > 0) { dv -= FV_PENALTY; }
                else if (v < 0) { dv += FV_PENALTY; }

                if (dv >= 0.0 && v <= SHRT_MAX - istep) { v += istep; }
                else if (dv <= 0.0 && v >= SHRT_MIN + istep) { v -= istep; }
                else { Console.WriteLine("A fvcoef parameter is out of bounce."); }

                pv = (short)v;
            }

            public static int BRand()
            {
                uint uret;

                if (uc == 31)
                {
                    urand = Hash.Rand32();
                    uc = 0;
                }
                else { uc += 1; }
                uret = urand & 1U;
                urand >>= 1;
                return (int)uret;
            }
        }


        private void UpdateFV(ref short[,] fv, float[,] pfv)
        {
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    float g = pfv[i, j];
                    float h = g * g;
                    short x = (short)Math.Round(STEP_SIZE * g / (Math.Sqrt(h) + eps));
                    fv[i, j] -= x;
                }
            }
        }

        private void UpdateFV(ref short[,,] fv, float[,,] pfv)
        {
            for (int i = 0; i < NSquare; i++)
            {
                for (int j = 0; j < NSquare; j++)
                {
                    for (int k = 0; k < NSquare; k++)
                    {
                        float g = pfv[i, j, k];
                        float h = g * g;
                        short x = (short)Math.Round(STEP_SIZE * g / (Math.Sqrt(h) + eps));
                        fv[i, j, k] -= x;
                        if (g > 0)
                        {
                            int jjjjj = 0;
                        }
                    }
                }
            }
        }
    }
}
