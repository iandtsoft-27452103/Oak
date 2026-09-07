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

namespace Achernar
{
    internal class TrainDeep3
    {
        public void TrainPolicy(int file_number, bool is_load_model, bool is_load_optim, int batch_size)
        {
            Feature ft = new Feature();
            Board bt = new Board();
            string record_file_name = "records" + file_number.ToString() + ".txt";

            List<Record> records = ReadRecordFile(record_file_name);
            int record_count = records.Count;

            List<Position> positions = new List<Position>();
            Position obj_pos = new Position();
            positions = obj_pos.Alloc(records);

            Random rm = new Random();
            positions = positions.OrderBy(x => rm.Next()).ToList();

            const int input_num = 44;
            const int num_channels = 256;
            const int output_num = 2;
            const double lr = 0.003;
            const double mt = 0.9;
            const double wd = 0.0001;
            const string model_file_name = "model.pth";
            const string optimizer_file_name = "optimizer.pth";
            const int console_out_threshold = 10;
            bool[] flg_color = { false, true };
            var d = torch.device(DeviceType.CUDA);
            var input_layer = Conv2d(input_num, num_channels, 5, 1, 2, device: d);
            var input_norm = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer1 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm1 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer2 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm2 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer3 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm3 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer4 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm4 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer5 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm5 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer6 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm6 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer7 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm7 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer8 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm8 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer9 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm9 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer10 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm10 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer11 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm11 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer12 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm12 = BatchNorm2d(num_channels, 2e-05, device: d);
            //var resnet = ResNet.ResNetCustom(num_channels, device: d);
            var output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            var seq = Sequential(("input_layer", input_layer), ("input_norm", input_norm), ("input_relu", ReLU()), ("layer1", layer1), ("norm1", norm1), ("relu1", ReLU()), ("layer2", layer2), ("norm2", norm2), ("relu2", ReLU()), ("layer3", layer3), ("norm3", norm3), ("relu3", ReLU()),
                ("layer4", layer4), ("norm4", norm4), ("relu4", ReLU()), ("layer5", layer5), ("norm5", norm5), ("relu5", ReLU()), ("layer6", layer6), ("norm6", norm6), ("relu6", ReLU()), ("layer7", layer7), ("norm7", norm7), ("relu7", ReLU()),
                ("layer8", layer8), ("norm8", norm8), ("relu8", ReLU()), ("layer9", layer9), ("norm9", norm9), ("relu9", ReLU()), ("layer10", layer10), ("norm10", norm10), ("relu10", ReLU()),
                ("layer11", layer11), ("norm11", norm11), ("relu11", ReLU()), ("layer12", layer12), ("norm12", norm12), ("relu12", ReLU()), ("output_layer", output_layer));

            if (is_load_model)
            {
                seq.load(model_file_name);
            }

            seq = seq.cuda();
            var optimizer = torch.optim.SGD(seq.parameters(), learningRate: lr, momentum: mt, weight_decay: wd, nesterov: true);
            //var optimizer = torch.optim.Adagrad(seq.parameters(), weight_decay: wd, eps: 1e-10);

            if (is_load_optim)
            {
                optimizer.load_state_dict(optimizer_file_name);
            }

            int pos_index = 0;
            int iteration_num = positions.Count() / batch_size;
            iteration_num = 1500;// 後でコメント化する。
            double sum_loss = 0;
            int ith_out = 0;

            using var x0 = torch.zeros(batch_size, input_num, NSquare, dtype: torch.float32, device: d);
            using var t0 = torch.zeros(batch_size, output_num, NSquare, dtype: torch.float32, device: d);
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
                    var temp_t = torch.zeros(1, output_num, NSquare, dtype: torch.float32, device: d);
                    temp_t[0][1][record.moves[limit]] = 1.0f;
                    temp_t = temp_t.reshape(output_num, NSquare);
                    t0[i] = temp_t;
                }
                var x = x0.reshape(batch_size, input_num, NSide, NSide);
                var t = t0.reshape(batch_size, output_num, NSide, NSide);
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

        public void TrainValue(int file_number, bool is_load_model, bool is_load_optim, int batch_size)
        {
            Feature ft = new Feature();
            Board bt = new Board();
            string record_file_name = "records" + file_number.ToString() + ".txt";

            List<Record> records = ReadRecordFile(record_file_name);
            int record_count = records.Count;

            List<Position> positions = new List<Position>();
            Position obj_pos = new Position();
            positions = obj_pos.Alloc(records);

            Random rm = new Random();
            positions = positions.OrderBy(x => rm.Next()).ToList();

            const int input_num = 44;
            const int num_channels = 256;
            const int output_num = 2;
            const double lr = 0.003;
            const double mt = 0.9;
            const double wd = 0.1;
            const int fcl = 256;
            const string model_file_name = "model_value.pth";
            const string optimizer_file_name = "optimizer_value.pth";
            const int console_out_threshold = 10;
            bool[] flg_color = { false, true };
            var d = torch.device(DeviceType.CUDA);
            var input_layer = Conv2d(input_num, num_channels, 5, 1, 2, device: d);
            var input_norm = BatchNorm2d(num_channels, 2e-05, device: d);
            var resnet = ResNet.ResNetCustom(num_channels, device: d);
            var middle_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            var middle_norm = BatchNorm2d(output_num, 2e-05, device: d);
            var value_layer0 = Linear(NSquare * output_num, fcl);
            var value_layer1 = Linear(fcl, 1);
            var seq = Sequential(("input_layer", input_layer), ("input_norm", input_norm), ("input_relu", ReLU()), ("resnet", resnet),
                ("middle_layer", middle_layer), ("middle_norm", middle_norm), ("middle_relu", ReLU()), ("flatten", Flatten()),
                ("value_layer0", value_layer0), ("value_relu", ReLU()), ("value_layer1", value_layer1), ("output_sigmoid", Sigmoid()));// ※bceを使っているのでSigmoidを入れてある。
            //var seq = Sequential(("input_layer", input_layer), ("input_norm", input_norm), ("input_relu", ReLU()), ("output_layer", output_layer));
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
            iteration_num = 300;// 後でコメント化する。
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
                var x = x0.reshape(batch_size, input_num, NSide, NSide);
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

        public void PredictPolicy()
        {
            Feature ft = new Feature();
            Board bt = new Board();
            string record_file_name = "test_records.txt";

            List<Record> records = ReadRecordFile(record_file_name);
            int record_count = records.Count;

            const int input_num = 44;
            const int num_channels = 256;
            const int output_num = 2;
            const string model_file_name = "model.pth";
            bool[] flg_color = { false, true };
            var d = torch.device(DeviceType.CUDA);
            var input_layer = Conv2d(input_num, num_channels, 5, 1, 2, device: d);
            var input_norm = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer1 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm1 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer2 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm2 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer3 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm3 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer4 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm4 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer5 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm5 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer6 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm6 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer7 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm7 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer8 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm8 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer9 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm9 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer10 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm10 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer11 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm11 = BatchNorm2d(num_channels, 2e-05, device: d);
            var layer12 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var norm12 = BatchNorm2d(num_channels, 2e-05, device: d);
            var resnet = ResNet.ResNetCustom(num_channels, device: d);
            var output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            var seq = Sequential(("input_layer", input_layer), ("input_norm", input_norm), ("input_relu", ReLU()), ("layer1", layer1), ("norm1", norm1), ("relu1", ReLU()), ("layer2", layer2), ("norm2", norm2), ("relu2", ReLU()), ("layer3", layer3), ("norm3", norm3), ("relu3", ReLU()),
                ("layer4", layer4), ("norm4", norm4), ("relu4", ReLU()), ("layer5", layer5), ("norm5", norm5), ("relu5", ReLU()), ("layer6", layer6), ("norm6", norm6), ("relu6", ReLU()), ("layer7", layer7), ("norm7", norm7), ("relu7", ReLU()),
                ("layer8", layer8), ("norm8", norm8), ("relu8", ReLU()), ("layer9", layer9), ("norm9", norm9), ("relu9", ReLU()), ("layer10", layer10), ("norm10", norm10), ("relu10", ReLU()),
                ("layer11", layer11), ("norm11", norm11), ("relu11", ReLU()), ("layer12", layer12), ("norm12", norm12), ("relu12", ReLU()), ("output_layer", output_layer));

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
                    x = x.reshape(1, 44, NSide, NSide);
                    x.cuda();

                    var eval = seq.forward(x);

                    eval = eval.reshape(1, output_num, NSquare);

                    List<short> move_list = new List<short>();
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
                        using var t = eval[0][1][m];
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
    }
}
