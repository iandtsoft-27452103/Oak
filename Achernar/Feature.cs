using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch.nn;
using static Achernar.Common;
using static Achernar.Board;
using static Tensorboard.CostGraphDef.Types;
using System.Numerics;
using System.Drawing;
using static TorchSharp.torch.distributions.constraints;

namespace Achernar
{
    internal class Feature
    {
        // 石 x 2
        // 空白 x 1
        // k手前の手 x 40
        // 手番 x 1
        // 合計 : 44個
        public torch.Tensor MakeInputFeature(ref Board bt, bool color)
        {
            const int limit = 42;
            var inputs = torch.zeros(limit + 2, NSquare);
            
            // 黒石、白石、空白
            for (int i = 0; i < NSquare; i++)
            {
                switch(bt.board[i])
                {
                    case 0:
                        inputs[0][i] = 1.0f;
                        break;
                    case 1:
                        inputs[1][i] = 1.0f;
                        break;
                    case 2:
                        inputs[2][i] = 1.0f;
                        break;
                }
            }

            // k手前の手
            int index = bt.current_moves.Count - 1;
            int index_t = 3;

            if (index >= 0)
            {
                while (true)
                {
                    inputs[index_t++, bt.current_moves[index--]] = 1.0f;
                    if (index_t == limit || index <= 0)
                        break;
                }
            }

            // 手番
            inputs[43] = color ? torch.zeros(NSquare) : torch.ones(NSquare);

            return inputs;
        }
    }
}
