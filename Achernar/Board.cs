using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Achernar.Common;
using static Achernar.MakeMove;

namespace Achernar
{
    public class Board
    {
        public short[] board = new short[NSquare];
        public List<short> pos_empty = new List<short>();
        public short[] next_seq_num = new short[2];
        public short[,] seq_number_table = new short[2, NSquare];
        public const short seq_max = 256;
        public const short ply_max = 512;// 後で変えるかもしれない
        public List<short> current_moves = new List<short>();
        public List<short>[,] seq_sq = new List<short>[2, seq_max];
        public List<short>[,] dame_sq = new List<short>[2, seq_max];
        public byte[] agehama = new byte[2];
        public ulong hash_key;
        public List<ulong> hash_array = new List<ulong>();
        public byte[,] saved_agehama = new byte[2, ply_max];
        public bool[] connect_flag = new bool[ply_max];
        public bool[] tori_flag = new bool[ply_max];
        public short[,] saved_seq_num = new short[ply_max, 3];
        //public List<short>[,] saved_seq_sq = new List<short>[ply_max, 3];
        //public List<short>[,] saved_dame_sq = new List<short>[ply_max, 3];
        public List<short>[,] saved_seq_sq = new List<short>[ply_max, 3];
        public List<short>[,] saved_dame_sq = new List<short>[ply_max, 3];
        public short[] saved_base_seq_num = new short[ply_max];
        public List<short>[] saved_base_seq_sq = new List<short>[ply_max];
        public List<short>[] saved_base_dame_sq = new List<short>[ply_max];
        public short[,] saved_opp_seq_num = new short[ply_max, 4];
        //public List<short>[,] saved_opp_seq_sq = new List<short>[ply_max, 4];// 要らないかもしれない
        //public List<short>[,] saved_opp_seq_dame_sq = new List<short>[ply_max, 4];
        //public List<short>[,] saved_made_dame_square = new List<short>[ply_max, 128];
        //public short[,] removed_seq_num = new short[ply_max, 4];
        //public List<short>[,] removed_seq_sq = new List<short>[ply_max, 4];
        //public List<short>[] saved_opp_seq_sq = new List<short>[ply_max];// 要らないかもしれない
        public List<short>[,] saved_opp_seq_dame_sq = new List<short>[ply_max, 4];
        public List<short>[,] saved_made_dame_square = new List<short>[ply_max, 4];
        public short[,] removed_seq_num = new short[ply_max, 4];
        public List<short>[,] removed_seq_sq = new List<short>[ply_max, 4];
        public short kou = NSquare;
        public short[] kou_array = new short[ply_max];
        //public short root_color = 0;
        //public short root_ply = 1;
        //public ulong[] Hash = new ulong[ply_max];
        public ulong CurrentHash;
        public ulong PrevHash;
        public int ply;
        public short[] RootMoves;
        public short RootColor;
        // ToDo: Hashのデータを追加する

        public void Init()
        {
            pos_empty.Clear();
            hash_array.Clear();
            current_moves.Clear();

            for (short i = 0; i < board.Length; i++)
            {
                board[i] = 2;
                pos_empty.Add(i);
                seq_number_table[0, i] = seq_max;
                seq_number_table[1, i] = seq_max;
            }

            next_seq_num[0] = next_seq_num[1] = 0;
            agehama[0] = agehama[1] = 0;
            hash_key = 0;

            for (short i = 0; i < 2; i++)
            {
                for (short j = 0; j < ply_max; j++)
                {
                    saved_agehama[i, j] = 0;
                    if (j < seq_max)
                    {
                        seq_sq[i, j] = new List<short>();
                        dame_sq[i, j] = new List<short>();
                    }
                }
            }

            for (short i = 0; i < ply_max; i++)
            {
                for (short j = 0; j < 3; j++)
                {
                    saved_seq_num[i, j] = seq_max;
                }
            }

            for (short i = 0; i < ply_max; i++)
            {
                //saved_opp_seq_sq[i] = new List<short>(); // ※仮にコメントアウトしてある
                //saved_opp_seq_dame_sq[i] = new List<short>();
                //removed_seq_sq[i] = new List<short>();
                for (short j = 0; j < 4; j++)
                {
                    saved_opp_seq_dame_sq[i, j] = new List<short>();
                    saved_opp_seq_num[i, j] = seq_max;
                    saved_made_dame_square[i, j] = new List<short>();
                    //saved_opp_seq_sq[i, j] = new List<short>();
                    //saved_opp_seq_dame_sq[i, j] = new List<short>();
                    removed_seq_num[i, j] = seq_max;
                    removed_seq_sq[i, j] = new List<short>();
                }
            }

            for (short i = 0; i < ply_max; i++)
            {
                connect_flag[i] = false;
                tori_flag[i] = false;
                saved_base_seq_num[i] = seq_max;
                saved_base_seq_sq[i] = new List<short>();
                saved_base_dame_sq[i] = new List<short>();
                //saved_dame_sq[i] = new List<short>();
                for (short j = 0; j < 3; j++)
                {
                    //saved_seq_sq[i, j] = new List<short>();
                    //saved_dame_sq[i, j] = new List<short>();
                    saved_seq_num[i, j] = seq_max;
                    saved_seq_sq[i, j] = new List<short>();
                    saved_dame_sq[i, j] = new List<short>();
                }
            }

            kou_array = new short[ply_max];
            RootMoves = new short[pos_empty.Count];
            //root_color = 0;
            //Hash = new ulong[ply_max];
            CurrentHash = 0;
            PrevHash = 0;
            ply = 0;
        }

        public Board DeepCopy(Board bt, bool flag)
        {
            Board bt_base = new Board();
            bt.board.CopyTo(bt_base.board, 0);
            bt_base.pos_empty = bt.pos_empty.ToList();
            bt.next_seq_num.CopyTo(bt_base.next_seq_num, 0);
            bt.agehama.CopyTo(bt_base.agehama, 0);
            bt.connect_flag.CopyTo(bt_base.connect_flag, 0);
            bt.tori_flag.CopyTo(bt_base.tori_flag, 0);
            bt_base.hash_key = bt.hash_key;
            bt_base.hash_array = bt.hash_array.ToList();
            bt_base.saved_base_seq_num = bt.saved_base_seq_num.ToArray();
            //bt_base.root_color = bt.root_color;
            bt_base.RootColor = bt.RootColor;
            bt_base.CurrentHash = bt.CurrentHash;
            bt_base.PrevHash = bt.PrevHash;
            bt_base.ply = bt.ply;
            bt_base.current_moves = bt.current_moves.ToList();
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < ply_max; j++)
                {
                    if (j < NSquare)
                    {
                        bt_base.seq_number_table[i, j] = bt.seq_number_table[i, j];
                    }                    
                    if (j < seq_max)
                    {
                        bt_base.seq_sq[i, j] = bt.seq_sq[i, j].ToList();
                        bt_base.dame_sq[i, j] = bt.dame_sq[i, j].ToList();
                    }
                    bt_base.saved_agehama[i, j] = bt.saved_agehama[i, j];
                    bt_base.saved_base_seq_sq[j] = bt.saved_base_seq_sq[j].ToList();
                    bt_base.saved_base_dame_sq[j] = bt.saved_base_dame_sq[j].ToList();
                    bt_base.kou_array = bt.kou_array.ToArray();
                    for (int k = 0; k < 4; k++)
                    {
                        bt_base.saved_opp_seq_dame_sq[j, k] = bt.saved_opp_seq_dame_sq[j, k].ToList();
                        bt_base.saved_opp_seq_num[j, k] = bt.saved_opp_seq_num[j, k];
                        bt_base.saved_made_dame_square[j, k] = bt.saved_made_dame_square[j, k].ToList();
                        bt_base.removed_seq_num[j, k] = bt.removed_seq_num[j, k];
                        bt_base.removed_seq_sq[j, k] = bt.removed_seq_sq[j, k].ToList();
                        if (k < 3)
                        {
                            bt_base.saved_seq_num[j, k] = bt.saved_seq_num[j, k];
                            bt_base.saved_seq_sq[j, k] = bt.saved_seq_sq[j, k].ToList();
                            bt_base.saved_dame_sq[j, k] = bt.saved_dame_sq[j, k].ToList();
                        }

                    }
                }
            }

            if (flag)
                bt_base.RootMoves = bt.RootMoves.ToArray();

            return bt_base;
        }

        public static string BoardToString(Board board, string type, short color)
        {
            int index, cnt;
            const string hyphen = "-";
            string str_board = "";
            for (int i = 0; i < board.board.Length; i++)
                str_board += board.board[i].ToString();
            str_board += hyphen;
            int length = board.current_moves.Count;
            if (length > 40)
                length = 40;
            int null_move_length = 40 - length;
            index = board.current_moves.Count - 1;
            cnt = 0;
            while(cnt < length)
            {
                str_board += board.current_moves[index--] + hyphen;
                cnt++;
            }                
            for (int i = 0; i < null_move_length; i++)
                str_board += NSquare.ToString() + hyphen;
            if (board.kou != NSquare)
            {
                str_board += board.kou.ToString();
            }
            else
            {
                str_board += "None";
            }
            if (color == 0)
            {
                str_board += hyphen + "b";
            }
            else
            {
                str_board += hyphen + "w";
            }
            return str_board;
        }

        public void SetRootPos(ref Board bt, string str_response, short color, ref float[] policy_output)
        {
            string[] s0 = str_response.Split(',');
            //bt.RootMoves = new Move[s0.Length];
            List<short> moves = new List<short>();
            policy_output = new float[s0.Length];
            for (int i = 0; i < s0.Length; i++)
            {
                string[] s1 = s0[i].Split(" ");
                //bt.RootMoves[i] = CSA.CSA2Move(bt, s1[0]);
                short temp_move = short.Parse(s1[0]);

                if (!IsMoveValid(bt, temp_move, color))
                    continue;

                //Do(ref bt, temp_move, color, 1);

                moves.Add(temp_move);

                //UnDo(ref bt, temp_move, color, 1);

                policy_output[i] = float.Parse(s1[1]);
            }

            bt.RootMoves = new short[moves.Count];
            for (int i = 0; i < moves.Count; i++)
                bt.RootMoves[i] = moves[i];

            bt.RootColor = color;
        }

        public bool IsMoveValid(Board bt, short move, short color)
        {
            int empty_count = 0;
            int opponent_count = 0;
            List<short> my_near_seq = new List<short>();
            List<short> opp_near_seq = new List<short>();
            List<short> li = PosCrossTable[move].ToList();

            // moveの位置が空白でなかったら非合法手
            if (bt.board[move] != 2)
                return false;

            // 直前のコウの取り返しは非合法手
            if (move == kou)
                return false;

            for (int i = 0; i < li.Count; i++)
            {
                if (bt.board[li[i]] == color)
                {
                    my_near_seq.Add(bt.seq_number_table[color, li[i]]);
                }
                else if (bt.board[li[i]] == (color ^ 1))
                {
                    opp_near_seq.Add(bt.seq_number_table[color ^ 1, li[i]]);
                    opponent_count++;
                }
                else if (bt.board[li[i]] == 2)
                {
                    empty_count++;
                }
            }

            for (int i = 0;i < my_near_seq.Count; i++)
            {
                int count = bt.dame_sq[color, my_near_seq[i]].Count;

                // 自分の駄目を埋める手かつ駄目の数が2以上だったら合法手
                if (count >= 2)
                {
                    return true;
                }
                else if (count == 1)
                {
                    // 石を打つ位置の周囲4マスに空白がある場合は駄目が増えるので合法手
                    if (empty_count > 0)
                        return true;
                }               
            }

            // 相手の一眼を埋める手でなければ合法手
            //if (opponent_count <= 3)
            if (opponent_count <= (li.Count - 1))// 2024.3.17 修正 4隅のli.Countの値は2, 端のli.Countの値は3であった。
            {
                return true;
            }
            //else if (opponent_count == 4)// 2024.3.17 修正 4隅のli.Countの値は2, 端のli.Countの値は3であった。
            else if (opponent_count == li.Count)
            {
                // 相手の一眼を埋める場合、相手の連の駄目がその一眼のみだったら合法手
                for (int i = 0; i < opp_near_seq.Count; i++)
                {
                    int count = bt.dame_sq[color ^ 1, opp_near_seq[i]].Count;
                    if (count == 1)
                        return true;
                }
            }

            // ここに到達する場合は相手の一眼を埋める場合かつ相手の連の駄目が2以上、すなわち自殺手である。
            return false;
        }
    }
}
