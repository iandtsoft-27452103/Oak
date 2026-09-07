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
    internal class TrainDeep
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
            const double lr = 0.0001;
            const double mt = 0.9;
            const double wd = 0.0001;
            const string model_file_name = "model.pth";
            const string optimizer_file_name = "optimizer.pth";
            const int console_out_threshold = 10;
            bool[] flg_color = { false, true };
            var d = torch.device(DeviceType.CUDA);
            var input_layer = Conv2d(input_num, num_channels, 5, 1, 2, device: d);
            var input_norm = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer1 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm1 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer2 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm2 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer3 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm3 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer4 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm4 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer5 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm5 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer6 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm6 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer7 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm7 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer8 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm8 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer9 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm9 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer10 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm10 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer11 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm11 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer12 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm12 = BatchNorm2d(num_channels, 2e-05, device: d);
            //var resnet = ResNet.ResNetCustom(num_channels, device: d);
            var output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            //var seq = Sequential(("input_layer", input_layer), ("input_norm", input_norm), ("input_relu", ReLU()), ("resnet", resnet), ("output_layer", output_layer));
            //var seq = Sequential(("input_layer", input_layer), ("input_norm", input_norm), ("input_relu", ReLU()), ("output_layer", output_layer));
            var seq = Sequential(("input_layer", input_layer), ("input_norm", input_norm), ("input_relu", ReLU()),
                ("cnn_layer1", cnn_layer1), ("cnn_norm1", cnn_norm1), ("cnn_relu1", ReLU()),
                ("cnn_layer2", cnn_layer2), ("cnn_norm2", cnn_norm2), ("cnn_relu2", ReLU()),
                ("cnn_layer3", cnn_layer3), ("cnn_norm3", cnn_norm3), ("cnn_relu3", ReLU()),
                ("cnn_layer4", cnn_layer4), ("cnn_norm4", cnn_norm4), ("cnn_relu4", ReLU()),
                ("cnn_layer5", cnn_layer5), ("cnn_norm5", cnn_norm5), ("cnn_relu5", ReLU()),
                ("cnn_layer6", cnn_layer6), ("cnn_norm6", cnn_norm6), ("cnn_relu6", ReLU()),
                ("cnn_layer7", cnn_layer7), ("cnn_norm7", cnn_norm7), ("cnn_relu7", ReLU()),
                ("cnn_layer8", cnn_layer8), ("cnn_norm8", cnn_norm8), ("cnn_relu8", ReLU()),
                ("cnn_layer9", cnn_layer9), ("cnn_norm9", cnn_norm9), ("cnn_relu9", ReLU()),
                ("cnn_layer10", cnn_layer10), ("cnn_norm10", cnn_norm10), ("cnn_relu10", ReLU()),
                ("cnn_layer11", cnn_layer11), ("cnn_norm11", cnn_norm11), ("cnn_relu11", ReLU()),
                ("cnn_layer12", cnn_layer12), ("cnn_norm12", cnn_norm12), ("cnn_relu12", ReLU()),
                ("output_layer", output_layer));

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
            iteration_num = 9000;// 後でコメント化する。
            double sum_loss = 0;
            int ith_out = 0;

            using var x0 = torch.zeros(batch_size, input_num, NSquare, dtype: torch.float32, device: d);
            //using var t0 = torch.zeros(batch_size, output_num, NSquare, dtype: torch.float32, device: d);
            using var t0 = torch.zeros(batch_size, NSquare, dtype: torch.@long, device: d);
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
                    //var temp_t = torch.zeros(1, output_num, NSquare, dtype: torch.float32, device: d);
                    var temp_t = torch.zeros(NSquare, dtype: torch.@long, device: d);
                    temp_t[record.moves[limit]] = 1.0f;
                    //temp_t = temp_t.reshape(output_num, NSquare);
                    t0[i] = temp_t;
                }
                var x = x0.reshape(batch_size, input_num, NSide, NSide);
                //var t = t0.reshape(batch_size, output_num, NSide, NSide);
                var t = t0.reshape(batch_size, NSide, NSide);
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
            const double lr = 0.0002;
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
            var cnn_layer1 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm1 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer2 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm2 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer3 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm3 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer4 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm4 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer5 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm5 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer6 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm6 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer7 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm7 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer8 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm8 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer9 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm9 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer10 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm10 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer11 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm11 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer12 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm12 = BatchNorm2d(num_channels, 2e-05, device: d);
            var output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            var value_layer0 = Linear(NSquare * output_num, fcl);
            var value_layer1 = Linear(fcl, 1);
            //var resnet = ResNet.ResNetCustom(num_channels, device: d);
            //var middle_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            //var middle_norm = BatchNorm2d(output_num, 2e-05, device: d);
            var seq = Sequential(("input_layer", input_layer), ("input_norm", input_norm), ("input_relu", ReLU()),
                                ("cnn_layer1", cnn_layer1), ("cnn_norm1", cnn_norm1), ("cnn_relu1", ReLU()),
                                ("cnn_layer2", cnn_layer2), ("cnn_norm2", cnn_norm2), ("cnn_relu2", ReLU()),
                                ("cnn_layer3", cnn_layer3), ("cnn_norm3", cnn_norm3), ("cnn_relu3", ReLU()),
                                ("cnn_layer4", cnn_layer4), ("cnn_norm4", cnn_norm4), ("cnn_relu4", ReLU()),
                                ("cnn_layer5", cnn_layer5), ("cnn_norm5", cnn_norm5), ("cnn_relu5", ReLU()),
                                ("cnn_layer6", cnn_layer6), ("cnn_norm6", cnn_norm6), ("cnn_relu6", ReLU()),
                                ("cnn_layer7", cnn_layer7), ("cnn_norm7", cnn_norm7), ("cnn_relu7", ReLU()),
                                ("cnn_layer8", cnn_layer8), ("cnn_norm8", cnn_norm8), ("cnn_relu8", ReLU()),
                                ("cnn_layer9", cnn_layer9), ("cnn_norm9", cnn_norm9), ("cnn_relu9", ReLU()),
                                ("cnn_layer10", cnn_layer10), ("cnn_norm10", cnn_norm10), ("cnn_relu10", ReLU()),
                                ("cnn_layer11", cnn_layer11), ("cnn_norm11", cnn_norm11), ("cnn_relu11", ReLU()),
                                ("cnn_layer12", cnn_layer12), ("cnn_norm12", cnn_norm12), ("cnn_relu12", ReLU()),
                                ("output_layer", output_layer), ("flatten", Flatten()), ("value_layer0", value_layer0), ("value_relu", ReLU()), ("value_layer1", value_layer1), ("output_sigmoid", Sigmoid()));
            //var seq = Sequential(("input_layer", input_layer), ("input_norm", input_norm), ("input_relu", ReLU()), ("resnet", resnet),
            //    ("middle_layer", middle_layer), ("middle_norm", middle_norm), ("middle_relu", ReLU()), ("flatten", Flatten()),
            //    ("value_layer0", value_layer0), ("value_relu", ReLU()), ("value_layer1", value_layer1), ("output_sigmoid", Sigmoid()));// ※bceを使っているのでSigmoidを入れてある。
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
            iteration_num = 9000;// 後でコメント化する。
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
            var cnn_layer1 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm1 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer2 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm2 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer3 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm3 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer4 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm4 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer5 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm5 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer6 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm6 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer7 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm7 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer8 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm8 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer9 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm9 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer10 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm10 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer11 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm11 = BatchNorm2d(num_channels, 2e-05, device: d);
            var cnn_layer12 = Conv2d(num_channels, num_channels, 3, 1, 1, device: d);
            var cnn_norm12 = BatchNorm2d(num_channels, 2e-05, device: d);
            //var resnet = ResNet.ResNetCustom(num_channels, device: d);
            var output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            //var seq = Sequential(("input_layer", input_layer), ("input_norm", input_norm), ("input_relu", ReLU()), ("resnet", resnet), ("output_layer", output_layer));
            //var seq = Sequential(("input_layer", input_layer), ("input_norm", input_norm), ("input_relu", ReLU()), ("output_layer", output_layer));
            var seq = Sequential(("input_layer", input_layer), ("input_norm", input_norm), ("input_relu", ReLU()),
                ("cnn_layer1", cnn_layer1), ("cnn_norm1", cnn_norm1), ("cnn_relu1", ReLU()),
                ("cnn_layer2", cnn_layer2), ("cnn_norm2", cnn_norm2), ("cnn_relu2", ReLU()),
                ("cnn_layer3", cnn_layer3), ("cnn_norm3", cnn_norm3), ("cnn_relu3", ReLU()),
                ("cnn_layer4", cnn_layer4), ("cnn_norm4", cnn_norm4), ("cnn_relu4", ReLU()),
                ("cnn_layer5", cnn_layer5), ("cnn_norm5", cnn_norm5), ("cnn_relu5", ReLU()),
                ("cnn_layer6", cnn_layer6), ("cnn_norm6", cnn_norm6), ("cnn_relu6", ReLU()),
                ("cnn_layer7", cnn_layer7), ("cnn_norm7", cnn_norm7), ("cnn_relu7", ReLU()),
                ("cnn_layer8", cnn_layer8), ("cnn_norm8", cnn_norm8), ("cnn_relu8", ReLU()),
                ("cnn_layer9", cnn_layer9), ("cnn_norm9", cnn_norm9), ("cnn_relu9", ReLU()),
                ("cnn_layer10", cnn_layer10), ("cnn_norm10", cnn_norm10), ("cnn_relu10", ReLU()),
                ("cnn_layer11", cnn_layer11), ("cnn_norm11", cnn_norm11), ("cnn_relu11", ReLU()),
                ("cnn_layer12", cnn_layer12), ("cnn_norm12", cnn_norm12), ("cnn_relu12", ReLU()),
                ("output_layer", output_layer));


            //seq = seq.cuda();
            //seq.eval();

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
                    var x = torch.zeros(1, input_num, NSide*NSide, dtype: torch.float32, device: d);
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
