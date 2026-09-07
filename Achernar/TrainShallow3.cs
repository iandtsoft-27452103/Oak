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
using static Achernar.Feature2;

namespace Achernar
{
    // 簡易的なロジスティック回帰のPolicyだが、まったくうまく行っていない。
    internal class TrainShallow3
    {
        double[] param = new double[NSquare*6+2];
        public const int FV_WINDOW = 256;
        const double lr = 0.0008;
        const float eps = 1e-10f;
        public int iteration_limit = 1;
        public int thread_num = 12;
        //public Evaluate2 obj_eval = new Evaluate2();
        public bool is_load_model = false;
        const int num_prev_moves = 4;
        public Feature2 f2 = new Feature2();
        public struct LearnParse
        {
            public double target;
            public double sum_dT;
            public long num_moves_counted;
            public LearnParse()
            {
                target = 0.0;
                sum_dT = 0.0;
                num_moves_counted = 0;
            }
        }

        public void Train(int file_number, bool is_load_model)
        {
            //int batch_size = (int)Math.Pow(2, 20);// 1048576
            //int batch_size = (int)Math.Pow(2, 15);
            //int batch_size = 1572864;
            int batch_size;
            //Board bt = new Board();
            string record_file_name = "records" + file_number.ToString() + ".txt";
            record_file_name = "records.txt";

            Console.WriteLine("Train Start!\n");
            DateTime dt = DateTime.Now;
            Console.WriteLine(dt.ToString() + "\n");

            if (is_load_model)
                f2.LoadFV();

            Console.WriteLine("1. Read Record File.");

            List<Record> records = ReadRecordFile(record_file_name);
            int record_count = records.Count;
            batch_size = record_count;

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
                //int data_size = batch_size / thread_num;
                int data_size = 16;
                int limit = 600;
                int start_num;
                int end_num;
                // メインスレッドが空いている状態
                //for (int j = 0; j < thread_num; j++)
                for (int j = 0; j < limit; j++)
                {
                    param = new double[NSquare * 6 + 2];
                    start_num = j * data_size;
                    end_num = (j + 1) * data_size;
                    CalcGradientsMother(j + 1, records, ref lp, start_num, end_num);
                    double loss = lp.target / lp.num_moves_counted;
                    Console.WriteLine("loss = " + loss.ToString());
                    UpdateFV();
                    //tasks[j] = Task.Factory.StartNew(() => CalcGradientsMother(j + 1, records, ref lp, start_num, end_num));
                    //if (j == thread_num - 1)
                    //Task.WaitAll(tasks);
                }
            }
            f2.SaveFV();
        }

        // 仮置きだが、Bonanza風の変形を入れていない。
        private double Sigmoid(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-x));
        }

        // 仮置きだが、、Bonanza風の変形を入れていない。
        private double dSigmoid(double x)
        {
            return Sigmoid(x) * (1.0f - Sigmoid(x));
        }

        private double BCEWithLogits(double y, double t)
        {
            return (-t * Math.Log(Sigmoid(y / 600.0f)) - (1 - t) * Math.Log(1 - Sigmoid(y / 600.0f)));
        }

        private void CalcGradientsMother(int thread_id, List<Record> records, ref LearnParse lp, int start_num, int end_num)
        {
            Board bt = new Board();
            int counter = 0;
            for (int i = start_num; i < end_num; i++)
            {
                Record record = new Record();
                bt.Init();
                //lock (records)
                {
                    record = records[i];
                    //Console.WriteLine("record number = " + (counter + 1).ToString() + " / " + (end_num - start_num).ToString());
                }
                short color = 0;
                for (int j = 0; j < record.moves.Length; j++)
                {
                    //MakeMove.Do(ref bt, record.moves[j], color, (short)(j + 1));
                    CalcGradients(ref lp, ref bt, record.moves[j], color, (short)(j + 1));
                    color ^= 1;
                }
                counter++;
            }
        }

        private void CalcGradients(ref LearnParse lp, ref Board bt, short t, short color, short ply)
        {
            double target, dT;
            double record_y = 0.0;
            double other_y = 0.0;
            double sum_dT = 0.0;

            target = 0.0f;
            List<int> li_t = new List<int>();
            MakeMove.Do(ref bt, t, color, ply);
            li_t = f2.MakeInputFeature(bt, color);
            record_y = f2.Calc(li_t);
            MakeMove.UnDo(ref bt, t, color, ply);
            //li_t.Clear();
            List<short> legal_move_list = new List<short>();
            for (short i = 0; i < bt.pos_empty.Count; i++)
            {
                if (bt.IsMoveValid(bt, i, color) && i != t)
                    legal_move_list.Add(i);
            }
            //nc += legal_move_list.Count;
            for (int i = 0; i < legal_move_list.Count; i++)
            {
                short move = legal_move_list[i];
                if (move == t)
                {
                    continue;
                }
                MakeMove.Do(ref bt, move, color, ply);
                List<int> li = new List<int>();
                li = f2.MakeInputFeature(bt, color);
                other_y = f2.Calc(li);
                target += Sigmoid(other_y - record_y);
                dT = (color == 0) ? dSigmoid(other_y - record_y) : -dSigmoid(other_y - record_y);
                sum_dT += dT;
                IncParam(li, -dT);
                li.Clear();
                MakeMove.UnDo(ref bt, move, color, ply);
            }
            MakeMove.Do(ref bt, t, color, ply);
            lp.num_moves_counted += legal_move_list.Count;
            lp.sum_dT += sum_dT;
            lp.target += target;
            IncParam(li_t, sum_dT);
        }

        public void IncParam(List<int> list, double p)
        {
            for (int i = 0; i < list.Count; i++)
            {
                param[list[i]] += p;
            }
        }

        private void UpdateFV()
        {
            for (int i = 0; i < f2.fv.Length; i++)
            {
                double g = param[i];
                double h = g * g;
                double x = lr * g / (Math.Sqrt(h) + eps);
                f2.fv[i] -= x;
            }
        }
    }
}
