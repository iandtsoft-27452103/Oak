using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Achernar.Board;
using static Achernar.Common;
using static Achernar.Evaluate;
using static Achernar.Sort;
using static Achernar.MakeMove;

namespace Achernar
{
    public class AlphaBeta
    {
        public const int ply_inc = 8;
        Evaluate2 eval = new Evaluate2();

        public struct AlphaBetaTree
        {
            public List<short>[] moves;
            public Board bt;
            public TT2 tt;
            public int task_number;
            public short[] current_move;
            public int[] eval;
            public List<short> pv;
            public int pv_length;
            public uint size_tt;
            public Sort.MoveAndScore[,] mas_before;
            public Sort.MoveAndScore[,] mas_after;
            public int root_move_num;
            public bool is_abort;
            public bool is_finished;
            public long num_node_searched;
            public long null_move_cut;
            public long delta_cut;
            public long razor_cut;
            public long futility_cut;
            public int BestValue;
            public int[] EvalArray;
            public AlphaBetaTree()
            {
                moves = new List<short>[ply_max];
                for (int i = 0; i < ply_max; i++)
                {
                    moves[i] = new List<short>();
                }
                bt = new Board();
                tt = new TT2();
                task_number = 0;
                current_move = new short[ply_max];
                eval = new int[ply_max];
                pv = new List<short>();
                pv_length = 0;
                size_tt = 0x10000000; // 256MB
                mas_before = new Sort.MoveAndScore[ply_max, NSquare];
                mas_after = new Sort.MoveAndScore[ply_max, NSquare];
                root_move_num = 0;
                is_abort = false;
                is_finished = false;
                num_node_searched = 0;
                null_move_cut = 0;
                delta_cut = 0;
                razor_cut = 0;
                futility_cut = 0;
                BestValue = 0;
                EvalArray = new int[ply_max];
            }

            public enum NodeState : uint
            {
                node_pv = 1,
                node_do_null_move = 2,
                node_do_delta = 4,
                node_do_razoring = 8,
                node_do_futility = 16,
                node_do_probcut = 32,
                node_mate_threat = 128,
            };
        }
        public  void EvalInit()
        {
            eval.LoadFV();
        }

        public int Search(ref AlphaBetaTree abt, int color, int alpha, int beta, int depth, int ply, uint state_node, int prev_value)
        {
            int value, iret, temp_value, move_count, ifrom, ito, icap_pc, alpha_old;
            uint state_node_new;
            //Direction idirec;
            List<short> moves = abt.moves[ply];
            Sort.MoveAndScore[] mb = new Sort.MoveAndScore[NSquare];
            Sort.MoveAndScore[] ma = new Sort.MoveAndScore[NSquare];
            List<short> li_b = new List<short>();
            List<short> li_w = new List<short>();
            short m = new short();
            abt.num_node_searched++;
            if (depth < ply_inc)
            {
                value = eval.EvaluateDiff(abt.bt, (short)color, abt.current_move[ply - 1], (short)prev_value, ref li_b, ref li_w);
                abt.tt.Store(abt.bt.CurrentHash, value, (short)color, abt.current_move[ply - 1]);
            }

            alpha_old = alpha;

            // #トランスポジションテーブルを調べる
            if (abt.tt.value.ContainsKey(abt.bt.CurrentHash) && abt.tt.color.ContainsKey(abt.bt.CurrentHash))
            {
                int c = (int)abt.tt.color[abt.bt.CurrentHash];
                if (c == color)
                {
                    // ハッシュ値と手番が一致した場合
                    value = abt.tt.value[abt.bt.CurrentHash];
                    abt.eval[ply] = value;
                    return value;
                }
            }

            if (abt.current_move[ply - 1] == NSquare)
            {
                temp_value = -abt.EvalArray[ply - 1];
            }
            else
            {
                temp_value = prev_value;
            }

            // Delta Pruningを実行する。
            // 【参考】
            // https://www.chessprogramming.org/Delta_Pruning
            // 1536は1024 + 512とか2のn乗の値を2つ足しただけで、科学的な根拠はない。
            if (2 * ply_inc <= depth && (state_node & (uint)AlphaBetaTree.NodeState.node_do_delta) != 0)
            {
                if (temp_value >= beta)
                {
                    abt.delta_cut++;
                    return beta;
                }

                if (temp_value < alpha - 1536)
                {
                    abt.delta_cut++;
                    return alpha;
                }

                if (alpha < temp_value)
                    alpha = temp_value;
            }

            List<short> legal_move_list = new List<short>();
            for (short i = 0; i < abt.bt.pos_empty.Count; i++)
            {
                if (abt.bt.IsMoveValid(abt.bt, i, (short)color))
                    legal_move_list.Add(i);
            }

            for (short i = 0; i < legal_move_list.Count; i++)
            {
                Do(ref abt.bt, moves[i], (short)color, (short)ply);
                li_b.Clear();
                li_w.Clear();
                //int v = (color == 0) ? EvalWrapper(abt.bt, color, moves[i], false) : -EvalWrapper(abt.bt, color, moves[i], false);
                int v = (color == 0) ? eval.EvaluateDiff(abt.bt, (short)color, moves[i], (short)prev_value, ref li_b, ref li_w) : -eval.EvaluateDiff(abt.bt, (short)color, moves[i], (short)prev_value, ref li_b, ref li_w);
                Sort.MoveAndScore temp_mas = new Sort.MoveAndScore();
                temp_mas.move = moves[i];
                temp_mas.score = v;
                mb[i] = temp_mas;
                UnDo(ref abt.bt, moves[i], (short)color, (short)ply);
            }

            return 0;// ※後で変更する。
        }
    }
}
