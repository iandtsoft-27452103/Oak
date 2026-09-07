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
    internal class Prediction
    {
        public Sequential model;
        public Sequential model_value;
        public void InitPolicy()
        {
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
            var output_layer = Conv2d(num_channels, output_num, 3, 1, 1, device: d);
            model = Sequential(("input_layer", input_layer), ("input_norm", input_norm), ("input_relu", ReLU()),
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

            model.load(model_file_name);
            model = model.cuda();
            model.eval();
        }
        public void InitValue()
        {
            const int input_num = 44;
            const int num_channels = 256;
            const int output_num = 2;
            const int fcl = 256;
            const string model_file_name = "model_value.pth";
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
            model_value = Sequential(("input_layer", input_layer), ("input_norm", input_norm), ("input_relu", ReLU()),
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

            model_value.load(model_file_name);
            model_value = model_value.cuda();
            model_value.eval();
        }

        public torch.Tensor ExecPolicy(torch.Tensor x)
        {
            return model.forward(x);
        }
        public torch.Tensor ExecValue(torch.Tensor x)
        {
            return model_value.forward(x);
        }
    }
}
