using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tensorboard;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch.nn;
//using static Achernar.Feature;
using static Achernar.IO;
using static Achernar.Record;
using static Achernar.Board;
using static Achernar.Layer;
//using static Achernar.Position;
using static Achernar.Common;
using static TorchSharp.torch.optim;

namespace Achernar
{
    // 標準的なロジスティック回帰の学習。αβ法で使えるか？
    internal class TrainDeep2
    {

        // 猛烈に精度が悪い。
        public void TrainPolicy(int file_number, bool is_load_model, bool is_load_optim, int batch_size)
        {
            Feature3 ft = new Feature3();
            Board bt = new Board();
            string record_file_name = "records" + file_number.ToString() + ".txt";

            List<Record> records = ReadRecordFile(record_file_name);
            int record_count = records.Count;

            List<Position> positions = new List<Position>();
            Position obj_pos = new Position();
            positions = obj_pos.Alloc(records);

            Random rm = new Random();
            positions = positions.OrderBy(x => rm.Next()).ToList();

            const int input_num = 12;
            const int num_channels = 64;
            const int output_num = 2;
            const double lr = 0.003;
            const double mt = 0.9;
            const double wd = 0.0001;
            const string model_file_name = "model_logreg0.pth";
            const string optimizer_file_name = "optimizer_logreg0.pth";
            const int console_out_threshold = 10;
            bool[] flg_color = { false, true };
            var d = torch.device(DeviceType.CUDA);
            //var input_layer = Conv2d(input_num, num_channels, 5, 1, 2, device: d);
            var input_layer = Linear(input_num * NSquare, num_channels, device: d);
            //ar output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            var output_layer = Linear(num_channels, NSquare, device: d);
            var seq = Sequential(("input_layer", input_layer), ("input_relu", ReLU()), ("output_layer", output_layer), ("output_softmax", Softmax(1)));

            if (is_load_model)
            {
                seq.load(model_file_name);
            }

            seq = seq.cuda();
            var optimizer = torch.optim.SGD(seq.parameters(), learningRate: lr, momentum: mt, weight_decay: wd, nesterov: true);

            if (is_load_optim)
            {
                optimizer.load_state_dict(optimizer_file_name);
            }

            int pos_index = 0;
            int iteration_num = positions.Count() / batch_size;
            iteration_num = 2000;// 後でコメント化する。
            double sum_loss = 0;
            int ith_out = 0;

            using var x0 = torch.zeros(batch_size, input_num, NSquare, dtype: torch.float32, device: d);
            using var t0 = torch.zeros(batch_size, NSquare, dtype: torch.float32, device: d);
            Console.WriteLine("Train Start!\n");
            DateTime dt = DateTime.Now;
            Console.WriteLine(dt.ToString() + "\n");
            for (int ith = 0; ith < iteration_num; ith++)
            {
                for (int i = 0; i < batch_size; i++)
                {
                    bt.Init();
                    Position pos = positions[pos_index++];
                    int record_no = pos.record_number;
                    int limit = pos.ply;
                    Record record = records[record_no];
                    short color = 0;
                    for (short j = 0; j < limit; j++)
                    {
                        short move = record.moves[j];
                        MakeMove.Do(ref bt, move, color, (short)(j + 1));
                        color ^= 1;
                    }
                    x0[i] = ft.MakeInputFeature(ref bt, flg_color[color]);
                    var temp_t = torch.zeros(1, NSquare, dtype: torch.float32, device: d);
                    temp_t[0][record.moves[limit]] = 1.0f;
                    temp_t = temp_t.reshape(NSquare);
                    t0[i] = temp_t;
                }
                var x = x0.reshape(batch_size, input_num * NSquare);
                var t = t0.reshape(batch_size, NSquare);
                x.cuda();
                t.cuda();
                seq.train();
                using var eval = seq.forward(x);
                using var output = functional.cross_entropy(eval, t);
                double loss = output.ToDouble();
                sum_loss += loss;
                string s = output.ToString();
                optimizer.zero_grad();
                output.backward();
                optimizer.step();
                ith_out++;
                if (ith_out == console_out_threshold)
                {
                    double avg_loss = sum_loss / (ith + 1);
                    Console.WriteLine("ith = " + (ith + 1).ToString() + ", " + "avg_loss = " + avg_loss.ToString() + "\n");
                    ith_out = 0;
                }
            }
            Console.WriteLine("Train End!\n");
            dt = DateTime.Now;
            Console.WriteLine(dt.ToString() + "\n");
            seq.save(model_file_name);
            optimizer.save_state_dict(optimizer_file_name);
        }

        // 通常のValue Network型のロジスティック回帰
        public void TrainValue(int file_number, bool is_load_model, bool is_load_optim, int batch_size)
        {
            Feature3 ft = new Feature3();
            Board bt = new Board();
            string record_file_name = "records" + file_number.ToString() + ".txt";

            List<Record> records = ReadRecordFile(record_file_name);
            int record_count = records.Count;

            List<Position> positions = new List<Position>();
            Position obj_pos = new Position();
            positions = obj_pos.Alloc(records);

            Random rm = new Random();
            positions = positions.OrderBy(x => rm.Next()).ToList();

            const int input_num = 12;
            const int num_channels = 64;
            const int output_num = 2;
            const double lr = 0.003;
            const double mt = 0.9;
            const double wd = 0.0001;
            const string model_file_name = "model_logreg0_v.pth";
            const string optimizer_file_name = "optimizer_logreg0_v.pth";
            const int console_out_threshold = 10;
            bool[] flg_color = { false, true };
            var d = torch.device(DeviceType.CUDA);
            //var input_layer = Conv2d(input_num, num_channels, 5, 1, 2, device: d);
            var input_layer = Linear(input_num * NSquare, num_channels, device: d);
            //ar output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            var output_layer = Linear(num_channels, 1, device: d);
            var seq = Sequential(("input_layer", input_layer), ("input_relu", ReLU()), ("output_layer", output_layer), ("output_sigmoid", Sigmoid()));

            if (is_load_model)
            {
                seq.load(model_file_name);
            }

            seq = seq.cuda();
            var optimizer = torch.optim.SGD(seq.parameters(), learningRate: lr, momentum: mt, weight_decay: wd, nesterov: true);

            if (is_load_optim)
            {
                optimizer.load_state_dict(optimizer_file_name);
            }

            int pos_index = 0;
            int iteration_num = positions.Count() / batch_size;
            iteration_num = 1000;// 後でコメント化する。
            double sum_loss = 0;
            int ith_out = 0;

            using var x0 = torch.zeros(batch_size, input_num, NSquare, dtype: torch.float32, device: d);
            using var t0 = torch.zeros(batch_size, 1, dtype: torch.float32, device: d);
            Console.WriteLine("Train Start!\n");
            DateTime dt = DateTime.Now;
            Console.WriteLine(dt.ToString() + "\n");
            for (int ith = 0; ith < iteration_num; ith++)
            {
                for (int i = 0; i < batch_size; i++)
                {
                    bt.Init();
                    Position pos = positions[pos_index++];
                    int record_no = pos.record_number;
                    int limit = pos.ply;
                    Record record = records[record_no];
                    short color = 0;
                    for (short j = 0; j < limit; j++)
                    {
                        short move = record.moves[j];
                        MakeMove.Do(ref bt, move, color, (short)(j + 1));
                        color ^= 1;
                    }
                    x0[i] = ft.MakeInputFeature(ref bt, flg_color[color]);
                    if (color == 0)
                    {
                        if (record.winner == 0)
                        {
                            t0[i] = 1.0f;
                        }
                        else
                        {
                            t0[i] = 0.0f;
                        }
                    }
                    else
                    {
                        if (record.winner == 1)
                        {
                            t0[i] = 1.0f;
                        }
                        else
                        {
                            t0[i] = 0.0f;
                        }
                    }
                }
                var x = x0.reshape(batch_size, input_num*NSquare);
                var t = t0.reshape(batch_size, 1);
                x.cuda();
                t.cuda();
                seq.train();
                using var eval = seq.forward(x);
                using var output = functional.binary_cross_entropy(eval, t);
                double loss = output.ToDouble();
                sum_loss += loss;
                string s = output.ToString();
                optimizer.zero_grad();
                output.backward();
                optimizer.step();
                ith_out++;
                if (ith_out == console_out_threshold)
                {
                    double avg_loss = sum_loss / (ith + 1);
                    Console.WriteLine("ith = " + (ith + 1).ToString() + ", " + "avg_loss = " + avg_loss.ToString() + "\n");
                    ith_out = 0;
                }
            }
            Console.WriteLine("Train End!\n");
            dt = DateTime.Now;
            Console.WriteLine(dt.ToString() + "\n");
            seq.save(model_file_name);
            optimizer.save_state_dict(optimizer_file_name);
        }

        // Bonanza Method型のロジスティック回帰
        public void TrainValue2(int file_number, bool is_load_model, bool is_load_optim, int batch_size)
        {
            Feature3 ft = new Feature3();
            Board bt = new Board();
            string record_file_name = "records" + file_number.ToString() + ".txt";

            List<Record> records = ReadRecordFile(record_file_name);
            int record_count = records.Count;

            List<Position> positions = new List<Position>();
            Position obj_pos = new Position();
            positions = obj_pos.Alloc(records);

            Random rm = new Random();
            positions = positions.OrderBy(x => rm.Next()).ToList();

            const int input_num = 12;
            const int num_channels = 64;
            const int output_num = 2;
            const double lr = 0.003;
            const double mt = 0.9;
            const double wd = 0.0001;
            const string model_file_name = "model_logreg1_v.pth";
            const string optimizer_file_name = "optimizer_logreg1_v.pth";
            const int console_out_threshold = 10;
            bool[] flg_color = { false, true };
            var d = torch.device(DeviceType.CUDA);
            //var input_layer = Conv2d(input_num, num_channels, 5, 1, 2, device: d);
            var input_layer = Linear(input_num * NSquare, num_channels, device: d);
            //ar output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            var output_layer = Linear(num_channels, 1, device: d);
            var seq = Sequential(("input_layer", input_layer), ("input_relu", ReLU()), ("output_layer", output_layer), ("output_sigmoid", Sigmoid()));

            if (is_load_model)
            {
                seq.load(model_file_name);
            }

            seq = seq.cuda();
            var optimizer = torch.optim.SGD(seq.parameters(), learningRate: lr, momentum: mt, weight_decay: wd, nesterov: true);

            if (is_load_optim)
            {
                optimizer.load_state_dict(optimizer_file_name);
            }

            int pos_index = 0;
            int iteration_num = positions.Count() / batch_size;
            iteration_num = 8000;// 後でコメント化する。
            double sum_loss = 0;
            int ith_out = 0;

            Console.WriteLine("Train Start!\n");
            DateTime dt = DateTime.Now;
            Console.WriteLine(dt.ToString() + "\n");
            for (int ith = 0; ith < iteration_num; ith++)
            {
                //for (int i = 0; i < batch_size; i++)
                {
                    bt.Init();
                    Position pos = positions[pos_index++];
                    int record_no = pos.record_number;
                    int limit = pos.ply;
                    Record record = records[record_no];
                    short color = 0;
                    int teacher_index = 0;
                    for (short j = 0; j < limit; j++)
                    {
                        short move = record.moves[j];

                        if (j == limit - 1)
                        {
                            List<short> move_list = new List<short>();
                            for (int k = 0; k < bt.pos_empty.Count; k++)
                            {
                                if (bt.IsMoveValid(bt, bt.pos_empty[k], color))
                                    move_list.Add(bt.pos_empty[k]);
                            }
                            using var x0 = torch.zeros(move_list.Count, input_num, NSquare, dtype: torch.float32, device: d);
                            using var t0 = torch.zeros(move_list.Count, 1, dtype: torch.float32, device: d);
                            for (int k = 0; k < move_list.Count; k++)
                            {
                                short m = move_list[k];
                                MakeMove.Do(ref bt, m, color, (short)(j + 1));
                                x0[k] = ft.MakeInputFeature(ref bt, flg_color[color ^ 1]);
                                MakeMove.UnDo(ref bt, m, color, (short)(j + 1));
                            }
          

                            List<double> scores = new List<double>();
                            for (int k = 0; k < move_list.Count; k++)
                            {
                                short m = move_list[k];
                                if (m == move)
                                {
                                    teacher_index = k;
                                    break;
                                }
                            }
                            for (int k = 0; k < move_list.Count; k++)
                            {
                                if (k == teacher_index)
                                {
                                    t0[k] = 1.0f;
                                }
                                else
                                {
                                    t0[k] = 0.0f;
                                }
                            }

                            var x = x0.reshape(move_list.Count, input_num * NSquare);
                            var t = t0.reshape(move_list.Count, 1);
                            x.cuda();
                            t.cuda();
                            seq.train();
                            using var eval = seq.forward(x);
                            using var output = functional.binary_cross_entropy(eval, t);
                            double loss = output.ToDouble();
                            sum_loss += loss;
                            string s = output.ToString();
                            optimizer.zero_grad();
                            output.backward();
                            optimizer.step();
                            break;
                        }

                        MakeMove.Do(ref bt, move, color, (short)(j + 1));
                        color ^= 1;
                    }

                }
                ith_out++;
                if (ith_out == console_out_threshold)
                {
                    double avg_loss = sum_loss / (ith + 1);
                    Console.WriteLine("ith = " + (ith + 1).ToString() + ", " + "avg_loss = " + avg_loss.ToString() + "\n");
                    ith_out = 0;
                }
            }
            Console.WriteLine("Train End!\n");
            dt = DateTime.Now;
            Console.WriteLine(dt.ToString() + "\n");
            seq.save(model_file_name);
            optimizer.save_state_dict(optimizer_file_name);
        }

        // Conv1DのValue Network
        public void TrainValue3(int file_number, bool is_load_model, bool is_load_optim, int batch_size)
        {
            Feature3 ft = new Feature3();
            Board bt = new Board();
            string record_file_name = "records" + file_number.ToString() + ".txt";

            List<Record> records = ReadRecordFile(record_file_name);
            int record_count = records.Count;

            List<Position> positions = new List<Position>();
            Position obj_pos = new Position();
            positions = obj_pos.Alloc(records);

            Random rm = new Random();
            positions = positions.OrderBy(x => rm.Next()).ToList();

            const int input_num = 12;
            const int num_channels = 64;
            const int output_num = 1;
            const double lr = 0.003;
            const double mt = 0.9;
            const double wd = 0.0001;
            const string model_file_name = "model_conv1d_v.pth";
            const string optimizer_file_name = "optimizer_conv1d_v.pth";
            const int console_out_threshold = 10;
            bool[] flg_color = { false, true };
            var d = torch.device(DeviceType.CUDA);
            //var input_layer = Conv2d(input_num, num_channels, 5, 1, 2, device: d);
            //var input_layer = Linear(input_num * NSquare, num_channels, device: d);
            var input_layer = Conv1d(input_num, num_channels, 3, 1, 1, device: d);
            var middle_layer = Conv1d(num_channels, num_channels, 3, 1, 1, device: d);
            //var output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            var flatten = Flatten();
            var output_layer = Linear(num_channels * NSquare, 1, device: d);
            var seq = Sequential(("input_layer", input_layer), ("input_relu", ReLU()), ("middle_layer", middle_layer), ("middle_relu", ReLU()), ("flatten", flatten), ("output_layer", output_layer), ("output_sigmoid", Sigmoid()));

            if (is_load_model)
            {
                seq.load(model_file_name);
            }

            seq = seq.cuda();
            var optimizer = torch.optim.SGD(seq.parameters(), learningRate: lr, momentum: mt, weight_decay: wd, nesterov: true);

            if (is_load_optim)
            {
                optimizer.load_state_dict(optimizer_file_name);
            }

            int pos_index = 0;
            int iteration_num = positions.Count() / batch_size;
            iteration_num = 2000;// 後でコメント化する。
            double sum_loss = 0;
            int ith_out = 0;

            using var x0 = torch.zeros(batch_size, 12, NSquare, dtype: torch.float32, device: d);
            using var t0 = torch.zeros(batch_size, 1, dtype: torch.float32, device: d);
            Console.WriteLine("Train Start!\n");
            DateTime dt = DateTime.Now;
            Console.WriteLine(dt.ToString() + "\n");
            for (int ith = 0; ith < iteration_num; ith++)
            {
                for (int i = 0; i < batch_size; i++)
                {
                    bt.Init();
                    Position pos = positions[pos_index++];
                    int record_no = pos.record_number;
                    int limit = pos.ply;
                    Record record = records[record_no];
                    short color = 0;
                    for (short j = 0; j < limit; j++)
                    {
                        short move = record.moves[j];
                        MakeMove.Do(ref bt, move, color, (short)(j + 1));
                        color ^= 1;
                    }
                    x0[i] = ft.MakeInputFeature(ref bt, flg_color[color]);
                    if (color == 0)
                    {
                        if (record.winner == 0)
                        {
                            t0[i] = 1.0f;
                        }
                        else
                        {
                            t0[i] = 0.0f;
                        }
                    }
                    else
                    {
                        if (record.winner == 1)
                        {
                            t0[i] = 1.0f;
                        }
                        else
                        {
                            t0[i] = 0.0f;
                        }
                    }
                }
                var x = x0.reshape(batch_size, input_num, NSquare);
                var t = t0.reshape(batch_size, 1);
                x.cuda();
                t.cuda();
                seq.train();
                //var aaa = input_layer.forward(x);
                //var ccc = Flatten().forward(aaa);
                //var bbb = output_layer.forward(ccc);
                using var eval = seq.forward(x);
                //using var output = functional.binary_cross_entropy(eval, t);
                using var output = functional.mse_loss(eval, t);
                double loss = output.ToDouble();
                sum_loss += loss;
                string s = output.ToString();
                optimizer.zero_grad();
                output.backward();
                optimizer.step();
                ith_out++;
                if (ith_out == console_out_threshold)
                {
                    double avg_loss = sum_loss / (ith + 1);
                    Console.WriteLine("ith = " + (ith + 1).ToString() + ", " + "avg_loss = " + avg_loss.ToString() + "\n");
                    ith_out = 0;
                }
            }
            Console.WriteLine("Train End!\n");
            dt = DateTime.Now;
            Console.WriteLine(dt.ToString() + "\n");
            seq.save(model_file_name);
            optimizer.save_state_dict(optimizer_file_name);
        }

        // Conv2DのValue Network
        public void TrainValue4(int file_number, bool is_load_model, bool is_load_optim, int batch_size)
        {
            Feature3 ft = new Feature3();
            Board bt = new Board();
            string record_file_name = "records" + file_number.ToString() + ".txt";

            List<Record> records = ReadRecordFile(record_file_name);
            int record_count = records.Count;

            List<Position> positions = new List<Position>();
            Position obj_pos = new Position();
            positions = obj_pos.Alloc(records);

            Random rm = new Random();
            positions = positions.OrderBy(x => rm.Next()).ToList();

            const int input_num = 12;
            const int num_channels = 64;
            const int output_num = 1;
            const double lr = 0.003;
            const double mt = 0.9;
            const double wd = 0.0001;
            const string model_file_name = "model_conv2d_v.pth";
            const string optimizer_file_name = "optimizer_conv2d_v.pth";
            const int console_out_threshold = 10;
            bool[] flg_color = { false, true };
            var d = torch.device(DeviceType.CUDA);
            //var input_layer = Conv2d(input_num, num_channels, 5, 1, 2, device: d);
            //var input_layer = Linear(input_num * NSquare, num_channels, device: d);
            var input_layer = Conv2d(input_num, num_channels, 3, 1, 1, device: d);
            var middle_layer = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            //var output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            var flatten = Flatten();
            var output_layer = Linear(num_channels * NSquare, 1, device: d);
            var seq = Sequential(("input_layer", input_layer), ("input_relu", ReLU()), ("middle_layer", middle_layer), ("middle_relu", ReLU()), ("flatten", flatten), ("output_layer", output_layer), ("output_sigmoid", Sigmoid()));

            if (is_load_model)
            {
                seq.load(model_file_name);
            }

            seq = seq.cuda();
            var optimizer = torch.optim.SGD(seq.parameters(), learningRate: lr, momentum: mt, weight_decay: wd, nesterov: true);

            if (is_load_optim)
            {
                optimizer.load_state_dict(optimizer_file_name);
            }

            int pos_index = 0;
            int iteration_num = positions.Count() / batch_size;
            iteration_num = 2000;// 後でコメント化する。
            double sum_loss = 0;
            int ith_out = 0;

            using var x0 = torch.zeros(batch_size, 12, NSquare, dtype: torch.float32, device: d);
            using var t0 = torch.zeros(batch_size, 1, dtype: torch.float32, device: d);
            Console.WriteLine("Train Start!\n");
            DateTime dt = DateTime.Now;
            Console.WriteLine(dt.ToString() + "\n");
            for (int ith = 0; ith < iteration_num; ith++)
            {
                for (int i = 0; i < batch_size; i++)
                {
                    bt.Init();
                    Position pos = positions[pos_index++];
                    int record_no = pos.record_number;
                    int limit = pos.ply;
                    Record record = records[record_no];
                    short color = 0;
                    for (short j = 0; j < limit; j++)
                    {
                        short move = record.moves[j];
                        MakeMove.Do(ref bt, move, color, (short)(j + 1));
                        color ^= 1;
                    }
                    x0[i] = ft.MakeInputFeature(ref bt, flg_color[color]);
                    if (color == 0)
                    {
                        if (record.winner == 0)
                        {
                            t0[i] = 1.0f;
                        }
                        else
                        {
                            t0[i] = 0.0f;
                        }
                    }
                    else
                    {
                        if (record.winner == 1)
                        {
                            t0[i] = 1.0f;
                        }
                        else
                        {
                            t0[i] = 0.0f;
                        }
                    }
                }
                var x = x0.reshape(batch_size, input_num, 19, 19);
                var t = t0.reshape(batch_size, 1);
                x.cuda();
                t.cuda();
                seq.train();
                //var aaa = input_layer.forward(x);
                //var ccc = Flatten().forward(aaa);
                //var bbb = output_layer.forward(ccc);
                using var eval = seq.forward(x);
                //using var output = functional.binary_cross_entropy(eval, t);
                using var output = functional.mse_loss(eval, t);
                double loss = output.ToDouble();
                sum_loss += loss;
                string s = output.ToString();
                optimizer.zero_grad();
                output.backward();
                optimizer.step();
                ith_out++;
                if (ith_out == console_out_threshold)
                {
                    double avg_loss = sum_loss / (ith + 1);
                    Console.WriteLine("ith = " + (ith + 1).ToString() + ", " + "avg_loss = " + avg_loss.ToString() + "\n");
                    ith_out = 0;
                }
            }
            Console.WriteLine("Train End!\n");
            dt = DateTime.Now;
            Console.WriteLine(dt.ToString() + "\n");
            seq.save(model_file_name);
            optimizer.save_state_dict(optimizer_file_name);
        }


        public void PredictPolicy()
        {
            Feature3 ft = new Feature3();
            Board bt = new Board();
            string record_file_name = "test_records.txt";

            List<Record> records = ReadRecordFile(record_file_name);
            int record_count = records.Count;

            const int input_num = 12;
            const int num_channels = 64;
            const int output_num = 2;
            const string model_file_name = "model_logreg0.pth";
            bool[] flg_color = { false, true };
            var d = torch.device(DeviceType.CUDA);
            //var input_layer = Conv2d(input_num, num_channels, 5, 1, 2, device: d);
            var input_layer = Linear(input_num * NSquare, num_channels, device: d);
            //ar output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            var output_layer = Linear(num_channels, NSquare, device: d);
            var seq = Sequential(("input_layer", input_layer), ("input_relu", ReLU()), ("output_layer", output_layer), ("output_softmax", Softmax(0)));

            seq = seq.cuda();
            seq.eval();

            seq.load(model_file_name);

            seq = seq.cuda();
            seq.eval();

            int move_count_total = 0;
            int acc_total = 0;

            //for (int i = 0; i < records.Count; i++)
            for (int i = 0; i < 40; i++)
            {
                bt.Init();
                Record record = records[i];

                Console.WriteLine("[ Record " + (i + 1).ToString() + "]");

                int acc_count = 0;

                short color = 0;
                for (int j = 0; j < record.str_moves.Length; j++)
                {
                    short move = record.moves[j];
                    var x = torch.zeros(1, input_num, NSide * NSide, dtype: torch.float32, device: d);
                    x[0] = ft.MakeInputFeature(ref bt, flg_color[color]);
                    x = x.reshape(1, input_num*NSquare);
                    x.cuda();

                    var eval = seq.forward(x);

                    eval = eval.reshape(1, NSquare);

                    List<short> move_list = new List<short>();
                    for (int k = 0; k < bt.pos_empty.Count; k++)
                    {
                        if (bt.IsMoveValid(bt, bt.pos_empty[k], color))
                            move_list.Add(bt.pos_empty[k]);
                    }

                    List<double> scores = new List<double>();
                    int teacher_index = 0;
                    double teacher_score = 0.0;
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        short m = move_list[k];
                        using var t = eval[0][m];
                        string s = t.ToSingle().ToString();
                        scores.Add(t.ToDouble());
                        if (move == m)
                        {
                            teacher_index = k;
                            teacher_score = t.ToDouble();
                        }
                    }

                    int teacher_move_rank = 1;
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        if (k == teacher_index)
                        {
                            continue;
                        }
                        if (scores[k] > teacher_score)
                        {
                            teacher_move_rank++;
                        }
                    }

                    if (teacher_move_rank == 1)
                    {
                        acc_count++;
                        acc_total++;
                    }

                    MakeMove.Do(ref bt, move, color, (short)j);
                    color ^= 1;
                }

                move_count_total += record.str_moves.Length;

                double acc_rate = ((double)acc_count / (double)record.str_moves.Length);
                Console.WriteLine(acc_count.ToString() + " / " + record.str_moves.Length.ToString());
                Console.WriteLine(acc_rate.ToString());
                Console.WriteLine();
            }

            Console.WriteLine("[ Total Accuracy ]");

            double acc_rate_total = ((double)acc_total / (double)move_count_total);
            Console.WriteLine(acc_total.ToString() + " / " + move_count_total.ToString());
            Console.WriteLine(acc_rate_total.ToString());
            Console.WriteLine();
        }
        public void PredictValue()
        {
            Feature3 ft = new Feature3();
            Board bt = new Board();
            string record_file_name = "test_records.txt";

            List<Record> records = ReadRecordFile(record_file_name);
            int record_count = records.Count;

            const int input_num = 12;
            const int num_channels = 64;
            const int output_num = 2;
            const string model_file_name = "model_logreg0_v.pth";
            bool[] flg_color = { false, true };
            var d = torch.device(DeviceType.CUDA);
            //var input_layer = Conv2d(input_num, num_channels, 5, 1, 2, device: d);
            var input_layer = Linear(input_num * NSquare, num_channels, device: d);
            //ar output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            var output_layer = Linear(num_channels, 1, device: d);
            var seq = Sequential(("input_layer", input_layer), ("input_relu", ReLU()), ("output_layer", output_layer), ("output_sigmoid", Sigmoid()));

            seq = seq.cuda();
            seq.eval();

            seq.load(model_file_name);

            seq = seq.cuda();
            seq.eval();

            int move_count_total = 0;
            int acc_total = 0;

            //for (int i = 0; i < records.Count; i++)
            for (int i = 0; i < 40; i++)
            {
                bt.Init();
                Record record = records[i];

                Console.WriteLine("[ Record " + (i + 1).ToString() + "]");

                int acc_count = 0;

                short color = 0;
                for (int j = 0; j < record.str_moves.Length; j++)
                {
                    short move = record.moves[j];

                    /*List<short> move_list = new List<short>();
                    for (int k = 0; k < bt.pos_empty.Count; k++)
                    {
                        move_list.Add(bt.pos_empty[k]);
                    }

                    List<double> scores = new List<double>();
                    int teacher_index = 0;
                    double teacher_score = 0.0;
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        short m = move_list[k];
                        using var t = eval[0][m];
                        string s = t.ToSingle().ToString();
                        scores.Add(t.ToDouble());
                        if (move == m)
                        {
                            teacher_index = k;
                            teacher_score = t.ToDouble();
                        }
                    }

                    int teacher_move_rank = 1;
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        if (k == teacher_index)
                        {
                            continue;
                        }
                        if (scores[k] > teacher_score)
                        {
                            teacher_move_rank++;
                        }
                    }

                    if (teacher_move_rank == 1)
                    {
                        acc_count++;
                        acc_total++;
                    }*/

                    MakeMove.Do(ref bt, move, color, (short)j);

                    if (j == 189)
                    {
                        int sssssss = 0;
                    }

                    var x = torch.zeros(1, input_num, NSide * NSide, dtype: torch.float32, device: d);
                    x[0] = ft.MakeInputFeature(ref bt, flg_color[color]);
                    x = x.reshape(1, input_num * NSquare);
                    x.cuda();
                    var eval = seq.forward(x);
                    eval = eval.ToSingle();
                    float f = eval.ToSingle();
                    float f2 = 1 - f;

                    string str_ply = (j + 1).ToString();

                    if (color == 0)
                    {
                        Console.WriteLine("ply=" + str_ply + ", black_win_rate=" + f + ", white_win_rate=" + f2);
                    }
                    else
                    {
                        Console.WriteLine("ply=" + str_ply + ", black_win_rate=" + f2 + ", white_win_rate=" + f);
                    }



                    color ^= 1;
                }
                break;
                move_count_total += record.str_moves.Length;

                double acc_rate = ((double)acc_count / (double)record.str_moves.Length);
                Console.WriteLine(acc_count.ToString() + " / " + record.str_moves.Length.ToString());
                Console.WriteLine(acc_rate.ToString());
                Console.WriteLine();
            }

            Console.WriteLine("[ Total Accuracy ]");

            double acc_rate_total = ((double)acc_total / (double)move_count_total);
            Console.WriteLine(acc_total.ToString() + " / " + move_count_total.ToString());
            Console.WriteLine(acc_rate_total.ToString());
            Console.WriteLine();
        }

        public void PredictValue2()
        {
            Feature3 ft = new Feature3();
            Board bt = new Board();
            string record_file_name = "test_records.txt";

            List<Record> records = ReadRecordFile(record_file_name);
            int record_count = records.Count;

            const int input_num = 12;
            const int num_channels = 64;
            const int output_num = 2;
            const string model_file_name = "model_logreg1_v.pth";
            bool[] flg_color = { false, true };
            var d = torch.device(DeviceType.CUDA);
            //var input_layer = Conv2d(input_num, num_channels, 5, 1, 2, device: d);
            var input_layer = Linear(input_num * NSquare, num_channels, device: d);
            //ar output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            var output_layer = Linear(num_channels, 1, device: d);
            var seq = Sequential(("input_layer", input_layer), ("input_relu", ReLU()), ("output_layer", output_layer), ("output_sigmoid", Sigmoid()));

            seq = seq.cuda();
            seq.eval();

            seq.load(model_file_name);

            seq = seq.cuda();
            seq.eval();

            int move_count_total = 0;
            int acc_total = 0;

            //for (int i = 0; i < records.Count; i++)
            for (int i = 0; i < 1; i++)
            {
                bt.Init();
                Record record = records[i];

                Console.WriteLine("[ Record " + (i + 1).ToString() + "]");

                int acc_count = 0;

                short color = 0;
                for (int j = 0; j < record.str_moves.Length; j++)
                {
                    short move = record.moves[j];

                    List<short> move_list = new List<short>();
                    for (int k = 0; k < bt.pos_empty.Count; k++)
                    {
                        if (bt.IsMoveValid(bt, bt.pos_empty[k], color))
                            move_list.Add(bt.pos_empty[k]);
                    }
                    using var x0 = torch.zeros(move_list.Count, input_num, NSquare, dtype: torch.float32, device: d);
                    //using var t0 = torch.zeros(move_list.Count, 1, dtype: torch.float32, device: d);
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        short m = move_list[k];
                        MakeMove.Do(ref bt, m, color, (short)(j + 1));
                        x0[k] = ft.MakeInputFeature(ref bt, flg_color[color ^ 1]);
                        MakeMove.UnDo(ref bt, m, color, (short)(j + 1));
                    }

                    int teacher_index = 0;

                    List<double> scores = new List<double>();
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        short m = move_list[k];
                        if (m == move)
                        {
                            teacher_index = k;
                            break;
                        }
                    }
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        if (k == teacher_index)
                        {
                            //t0[k] = 1.0f;
                        }
                        else
                        {
                            //t0[k] = 0.0f;
                        }
                    }

                    var x = x0.reshape(move_list.Count, input_num * NSquare);
                    //var t = t0.reshape(move_list.Count, 1);
                    x.cuda();
                    //t.cuda();
                    seq.train();
                    using var eval = seq.forward(x);

                    int rank = 1;
                    float teacher_value = eval[teacher_index].ToSingle();
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        if (k == teacher_index)
                            continue;
                        float temp_value = eval[k].ToSingle();
                        if (temp_value > teacher_value)
                            rank++;
                    }

                    string str_out = "ply = " + (j + 1).ToString() + ", record_rank = " + rank.ToString();
                    Console.WriteLine(str_out);

                    MakeMove.Do(ref bt, move, color, (short)j);
                    color ^= 1;
                }
                move_count_total += record.str_moves.Length;

                double acc_rate = ((double)acc_count / (double)record.str_moves.Length);
                Console.WriteLine(acc_count.ToString() + " / " + record.str_moves.Length.ToString());
                Console.WriteLine(acc_rate.ToString());
                Console.WriteLine();
            }

            Console.WriteLine("[ Total Accuracy ]");

            double acc_rate_total = ((double)acc_total / (double)move_count_total);
            Console.WriteLine(acc_total.ToString() + " / " + move_count_total.ToString());
            Console.WriteLine(acc_rate_total.ToString());
            Console.WriteLine();
        }
        public void PredictValue3()
        {
            Feature3 ft = new Feature3();
            Board bt = new Board();
            string record_file_name = "test_records.txt";

            List<Record> records = ReadRecordFile(record_file_name);
            int record_count = records.Count;

            const int input_num = 12;
            const int num_channels = 64;
            const int output_num = 1;
            const double lr = 0.003;
            const double mt = 0.9;
            const double wd = 0.0001;
            const string model_file_name = "model_conv1d_v.pth";
            const string optimizer_file_name = "optimizer_conv1d_v.pth";
            const int console_out_threshold = 10;
            bool[] flg_color = { false, true };
            var d = torch.device(DeviceType.CUDA);
            //var input_layer = Conv2d(input_num, num_channels, 5, 1, 2, device: d);
            //var input_layer = Linear(input_num * NSquare, num_channels, device: d);
            var input_layer = Conv1d(input_num, num_channels, 3, 1, 1, device: d);
            var middle_layer = Conv1d(num_channels, num_channels, 3, 1, 1, device: d);
            //var output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            var flatten = Flatten();
            var output_layer = Linear(num_channels * NSquare, 1, device: d);
            var seq = Sequential(("input_layer", input_layer), ("input_relu", ReLU()), ("middle_layer", middle_layer), ("middle_relu", ReLU()), ("flatten", flatten), ("output_layer", output_layer), ("output_sigmoid", Sigmoid()));

            seq = seq.cuda();
            seq.eval();

            seq.load(model_file_name);

            seq = seq.cuda();
            seq.eval();

            int move_count_total = 0;
            int acc_total = 0;

            //for (int i = 0; i < records.Count; i++)
            for (int i = 0; i < 1; i++)
            {
                bt.Init();
                Record record = records[i];

                Console.WriteLine("[ Record " + (i + 1).ToString() + "]");

                int acc_count = 0;

                short color = 0;
                for (int j = 0; j < record.str_moves.Length; j++)
                {
                    short move = record.moves[j];

                    List<short> move_list = new List<short>();
                    for (int k = 0; k < bt.pos_empty.Count; k++)
                    {
                        if (bt.IsMoveValid(bt, bt.pos_empty[k], color))
                            move_list.Add(bt.pos_empty[k]);
                    }
                    using var x0 = torch.zeros(move_list.Count, input_num, NSquare, dtype: torch.float32, device: d);
                    //using var t0 = torch.zeros(move_list.Count, 1, dtype: torch.float32, device: d);
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        short m = move_list[k];
                        MakeMove.Do(ref bt, m, color, (short)(j + 1));
                        x0[k] = ft.MakeInputFeature(ref bt, flg_color[color ^ 1]);
                        MakeMove.UnDo(ref bt, m, color, (short)(j + 1));
                    }

                    int teacher_index = 0;

                    List<double> scores = new List<double>();
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        short m = move_list[k];
                        if (m == move)
                        {
                            teacher_index = k;
                            break;
                        }
                    }
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        if (k == teacher_index)
                        {
                            //t0[k] = 1.0f;
                        }
                        else
                        {
                            //t0[k] = 0.0f;
                        }
                    }

                    var x = x0.reshape(move_list.Count, input_num, NSquare);
                    //var t = t0.reshape(move_list.Count, 1);
                    x.cuda();
                    //t.cuda();
                    seq.eval();
                    using var eval = seq.forward(x);

                    int rank = 1;
                    float teacher_value = eval[teacher_index].ToSingle();
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        if (k == teacher_index)
                            continue;
                        float temp_value = eval[k].ToSingle();
                        if (temp_value > teacher_value)
                            rank++;
                    }

                    string str_out = "ply = " + (j + 1).ToString() + ", record_rank = " + rank.ToString();
                    Console.WriteLine(str_out);

                    MakeMove.Do(ref bt, move, color, (short)j);
                    color ^= 1;
                }
                move_count_total += record.str_moves.Length;

                double acc_rate = ((double)acc_count / (double)record.str_moves.Length);
                Console.WriteLine(acc_count.ToString() + " / " + record.str_moves.Length.ToString());
                Console.WriteLine(acc_rate.ToString());
                Console.WriteLine();
            }

            Console.WriteLine("[ Total Accuracy ]");

            double acc_rate_total = ((double)acc_total / (double)move_count_total);
            Console.WriteLine(acc_total.ToString() + " / " + move_count_total.ToString());
            Console.WriteLine(acc_rate_total.ToString());
            Console.WriteLine();
        }

        public void PredictValue4()
        {
            Feature3 ft = new Feature3();
            Board bt = new Board();
            string record_file_name = "test_records.txt";

            List<Record> records = ReadRecordFile(record_file_name);
            int record_count = records.Count;

            const int input_num = 12;
            const int num_channels = 64;
            const int output_num = 1;
            const double lr = 0.003;
            const double mt = 0.9;
            const double wd = 0.0001;
            const string model_file_name = "model_conv1d_v.pth";
            const string optimizer_file_name = "optimizer_conv1d_v.pth";
            const int console_out_threshold = 10;
            bool[] flg_color = { false, true };
            var d = torch.device(DeviceType.CUDA);
            //var input_layer = Conv2d(input_num, num_channels, 5, 1, 2, device: d);
            //var input_layer = Linear(input_num * NSquare, num_channels, device: d);
            var input_layer = Conv2d(input_num, num_channels, 3, 1, 1, device: d);
            var middle_layer = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            //var output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            var flatten = Flatten();
            var output_layer = Linear(num_channels * NSquare, 1, device: d);
            var seq = Sequential(("input_layer", input_layer), ("input_relu", ReLU()), ("middle_layer", middle_layer), ("middle_relu", ReLU()), ("flatten", flatten), ("output_layer", output_layer), ("output_sigmoid", Sigmoid()));

            seq = seq.cuda();
            seq.eval();

            seq.load(model_file_name);

            seq = seq.cuda();
            seq.eval();

            int move_count_total = 0;
            int acc_total = 0;
            int acc_count = 0;

            //for (int i = 0; i < records.Count; i++)
            for (int i = 0; i < 1; i++)
            {
                bt.Init();
                Record record = records[i];

                Console.WriteLine("[ Record " + (i + 1).ToString() + "]");

                acc_count = 0;

                short color = 0;
                for (int j = 0; j < record.str_moves.Length; j++)
                {
                    short move = record.moves[j];

                    List<short> move_list = new List<short>();
                    for (int k = 0; k < bt.pos_empty.Count; k++)
                    {
                        if (bt.IsMoveValid(bt, bt.pos_empty[k], color))
                            move_list.Add(bt.pos_empty[k]);
                    }
                    using var x0 = torch.zeros(move_list.Count, input_num, NSquare, dtype: torch.float32, device: d);
                    //using var t0 = torch.zeros(move_list.Count, 1, dtype: torch.float32, device: d);
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        short m = move_list[k];
                        MakeMove.Do(ref bt, m, color, (short)(j + 1));
                        x0[k] = ft.MakeInputFeature(ref bt, flg_color[color ^ 1]);
                        MakeMove.UnDo(ref bt, m, color, (short)(j + 1));
                    }

                    int teacher_index = 0;

                    List<double> scores = new List<double>();
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        short m = move_list[k];
                        if (m == move)
                        {
                            teacher_index = k;
                            break;
                        }
                    }
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        if (k == teacher_index)
                        {
                            //t0[k] = 1.0f;
                        }
                        else
                        {
                            //t0[k] = 0.0f;
                        }
                    }

                    var x = x0.reshape(move_list.Count, input_num, 19, 19);
                    //var t = t0.reshape(move_list.Count, 1);
                    x.cuda();
                    //t.cuda();
                    seq.eval();
                    using var eval = seq.forward(x);

                    int rank = 1;
                    float teacher_value = eval[teacher_index].ToSingle();
                    for (int k = 0; k < move_list.Count; k++)
                    {
                        if (k == teacher_index)
                            continue;
                        float temp_value = eval[k].ToSingle();
                        if (temp_value > teacher_value)
                            rank++;
                    }

                    if (rank == 1)
                        acc_count += 1;

                    string str_out = "ply = " + (j + 1).ToString() + ", record_rank = " + rank.ToString();
                    Console.WriteLine(str_out);

                    MakeMove.Do(ref bt, move, color, (short)j);
                    color ^= 1;
                }
                move_count_total += record.str_moves.Length;

                double acc_rate = ((double)acc_count / (double)record.str_moves.Length);
                Console.WriteLine(acc_count.ToString() + " / " + record.str_moves.Length.ToString());
                Console.WriteLine(acc_rate.ToString());
                Console.WriteLine();
            }

            Console.WriteLine("[ Total Accuracy ]");

            acc_total = acc_count;

            double acc_rate_total = ((double)acc_total / (double)move_count_total);
            Console.WriteLine(acc_total.ToString() + " / " + move_count_total.ToString());
            Console.WriteLine(acc_rate_total.ToString());
            Console.WriteLine();
        }

    }
}
