using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using static Achernar.Common;
using static Achernar.Board;
using System.Linq.Expressions;

namespace Achernar
{
    internal class MakeMove
    {

        //ToDo: PlayOut内ではsave系のテーブルとハッシュの更新を入れない方が良い
        public static void Do(ref Board bt, short sq, short color, short ply)
        {
            Dictionary<short, short> v1 = new Dictionary<short, short>();
            Dictionary<short, short> v2 = new Dictionary<short, short>();
            Dictionary<short, short> v3 = new Dictionary<short, short>();
            short i, j;
            short BaseNumber;
            short AddDameCount = 0;
            short count = 0;

            bt.board[sq] = color;
            bt.current_moves.Add(sq);
            bt.pos_empty.Remove(sq);
            bt.PrevHash = bt.CurrentHash;
            bt.hash_array.Add(bt.hash_key);
            bt.hash_key ^= Hash.StoneRand[color, sq];
            bt.CurrentHash = bt.hash_key;
            bt.saved_agehama[color, ply] = bt.agehama[color];
            bt.saved_agehama[color ^ 1, ply] = bt.agehama[color ^ 1];
            bt.kou = bt.kou_array[ply] = NSquare;

            List<short> li = PosCrossTable[sq].ToList();

            short connect_count = 0;
            Dictionary<short, short> connect_sq = new Dictionary<short, short>();
            Dictionary<short, short> empty_sq = new Dictionary<short, short>();
            for (i = 0; i < li.Count; i++)
            {
                if (bt.board[li[i]] == color)
                {
                    connect_sq.TryAdd(connect_count++, li[i]);
                }
                else if (bt.board[li[i]] == Empty)
                {
                    empty_sq.TryAdd(AddDameCount++, li[i]);
                }
            }

            if (connect_count == 0)
            {
                // 自分の石と連絡しない手の場合
                bt.connect_flag[ply] = false;
                bt.seq_number_table[color, sq] = bt.next_seq_num[color];
                bt.seq_sq[color, bt.next_seq_num[color]].Add(sq);
                for (i = 0; i < empty_sq.Count; i++)
                    bt.dame_sq[color, bt.next_seq_num[color]].Add(empty_sq[i]);
                bt.next_seq_num[color]++;
                v1.TryAdd(count, sq);
                v2.TryAdd(count, bt.seq_number_table[color, sq]);
            }
            else
            {
                // 自分の石と連絡している場合の手（ノビ, サガリ, ツギ他）
                bt.connect_flag[ply] = true;
                for (i = 0; i < connect_sq.Count; i++)
                {
                    v1.TryAdd(count, connect_sq[i]);
                    if (!v2.ContainsValue(bt.seq_number_table[color, connect_sq[i]]))
                        v2.TryAdd(count++, bt.seq_number_table[color, connect_sq[i]]);
                    //m++;
                }

                BaseNumber = v2[0];
                bt.seq_number_table[color, sq] = BaseNumber;
                bt.saved_base_seq_num[ply] = BaseNumber;
                for (i = 0; i < bt.seq_sq[color, BaseNumber].Count; i++)
                    bt.saved_base_seq_sq[ply].Add(bt.seq_sq[color, BaseNumber][i]);
                for (i = 0; i < bt.dame_sq[color, BaseNumber].Count; i++)
                    bt.saved_base_dame_sq[ply].Add(bt.dame_sq[color, BaseNumber][i]);
                bt.seq_sq[color, BaseNumber].Add(sq);
                for (i = 0; i < empty_sq.Count; i++)
                {
                    if (!bt.dame_sq[color, BaseNumber].Contains(empty_sq[i]))
                        bt.dame_sq[color, BaseNumber].Add(empty_sq[i]);
                }

                for (i = 1; i < count; i++)
                {
                    bt.saved_seq_num[ply, i - 1] = v2[i];
                    bt.saved_seq_sq[ply, i - 1] = bt.seq_sq[color, v2[i]].ToList();
                    for (j = 0; j < bt.dame_sq[color, v2[i]].Count; j++)
                    {
                        bt.saved_dame_sq[ply, i - 1].Add(bt.dame_sq[color, v2[i]][j]);
                    }

                    for (j = 0; j < bt.seq_sq[color, v2[i]].Count; j++)
                        bt.seq_sq[color, BaseNumber].Add(bt.seq_sq[color, v2[i]][j]);
                    for (j = 0; j < bt.dame_sq[color, v2[i]].Count; j++)
                    {
                        if (!bt.dame_sq[color, BaseNumber].Contains(bt.dame_sq[color, v2[i]][j]))
                            bt.dame_sq[color, BaseNumber].Add(bt.dame_sq[color, v2[i]][j]);
                    }
                    for (j = 0; j < bt.seq_sq[color, v2[i]].Count; j++)
                        bt.seq_number_table[color, bt.seq_sq[color, v2[i]][j]] = BaseNumber;
                    bt.seq_sq[color, v2[i]].Clear();
                    bt.dame_sq[color, v2[i]].Clear();
                }
                bt.dame_sq[color, BaseNumber].Remove(sq);
            }

            // 4つの連をツグ手の場合、相手の駄目を詰めないので終了
            if (connect_count == 4)
                goto end;

            // 自分の石と連絡せず、相手の駄目も詰めない手の場合終了
            if (AddDameCount == 4)
                goto end;

            if (AddDameCount == PosCrossTable[sq].Count)
                goto end;

            short count2 = 0;
            short tori_cnt = 0;
            short l = 0;
            short m = 0;
            short x = 0;

            for (i = 0; i < li.Count; i++)
            {
                if (bt.board[li[i]] == (color ^ 1))
                    v3.TryAdd(x++, bt.seq_number_table[color ^ 1, li[i]]);
            }

            for (i = 0; i < x; i++)
            {
                if(bt.dame_sq[color ^ 1, v3[i]].Contains(sq))
                {
                    // 駄目を詰める
                    bt.saved_opp_seq_num[ply, count2] = v3[i];
                    for (j = 0; j < bt.dame_sq[color ^ 1, v3[i]].Count; j++)
                        bt.saved_opp_seq_dame_sq[ply, count2].Add(bt.dame_sq[color ^ 1, v3[i]][j]);
                    bt.dame_sq[color ^ 1, v3[i]].Remove(sq);
                    count2++;
                    if (bt.dame_sq[color ^ 1, v3[i]].Count == 0)
                    {
                        // トリの手の場合
                        short k = (short)bt.seq_sq[color ^ 1, v3[i]].Count;
                        bt.tori_flag[ply] = true;
                        bt.agehama[color] += (byte)k;
                        bt.saved_agehama[color, ply] = bt.agehama[color];

                        // 取った相手の石の連・碁盤・空白の情報を更新する
                        for (j = 0; j < k; j++)
                        {
                            //bt.seq_number_table[color, bt.seq_sq[color ^ 1, v3[i]][j]] = NSquare;
                            bt.seq_number_table[color, bt.seq_sq[color ^ 1, v3[i]][j]] = seq_max;
                            bt.pos_empty.Add(bt.seq_sq[color ^ 1, v3[i]][j]);
                            bt.board[bt.seq_sq[color ^ 1, v3[i]][j]] = 2;
                            bt.hash_key ^= Hash.StoneRand[color ^ 1, bt.seq_sq[color ^ 1, v3[i]][j]];
                        }

                        for (j = 0; j < bt.seq_sq[color ^ 1, v3[i]].Count; j++)
                            bt.saved_made_dame_square[ply, i].Add(bt.seq_sq[color ^ 1, v3[i]][j]);                       

                        // 自分の駄目を作る
                        for (l = 0; l < bt.saved_made_dame_square[ply, i].Count; l++)
                        {
                            for (j = 0; j < PosCrossTable[bt.saved_made_dame_square[ply, i][l]].Count; j++)
                            {
                                if (bt.board[PosCrossTable[bt.saved_made_dame_square[ply, i][l]][j]] == color)
                                {
                                    if (!bt.dame_sq[color, bt.seq_number_table[color, PosCrossTable[bt.saved_made_dame_square[ply, i][l]][j]]].Contains(bt.saved_made_dame_square[ply, i][l]))
                                    {
                                        bt.dame_sq[color, bt.seq_number_table[color, PosCrossTable[bt.saved_made_dame_square[ply, i][l]][j]]].Add(bt.saved_made_dame_square[ply, i][l]);
                                    }
                                }
                            }
                        }

                        // 所属する連番号を初期化する
                        for (l = 0; l < bt.seq_sq[color ^ 1, v3[i]].Count; l++)
                            bt.seq_number_table[color ^ 1, bt.seq_sq[color ^ 1, v3[i]][l]] = seq_max;

                        // 取った連番号と位置を保存する
                        bt.removed_seq_num[ply, tori_cnt++] = v3[i];
                        for (l = 0; l < k; l++)
                            bt.removed_seq_sq[ply, m].Add(bt.seq_sq[color ^ 1, v3[i]][l]);

                        bt.seq_sq[color ^ 1, v3[i]].Clear();
                        bt.dame_sq[color ^ 1, v3[i]].Clear();
                        m++;
                    }
                }
            }

            if (AddDameCount == 0 && connect_count == 0 &&bt.tori_flag[ply] && m == 1)
            {
                for (i = 0; i < x; i++)
                {
                    if(bt.saved_made_dame_square[ply, i].Count == 1)
                    {
                        bt.kou = bt.kou_array[ply] = bt.saved_made_dame_square[ply, i][0];
                        break;
                    }
                }             
            }

        end:

            ++bt.ply;
        }

        public static void UnDo(ref Board bt, short sq, short color, short ply)
        {
            short i, k, l;

            bt.board[sq] = 2;
            bt.current_moves.RemoveAt(bt.current_moves.Count - 1);
            bt.pos_empty.Add(sq);
            bt.CurrentHash = bt.hash_key = bt.PrevHash;
            bt.PrevHash = bt.hash_array[bt.hash_array.Count - 1];
            bt.hash_array.RemoveAt(bt.hash_array.Count - 1);
            //bt.hash_key ^= Hash.StoneRand[color, sq];
            bt.agehama[color] = bt.saved_agehama[color, ply - 1];
            bt.agehama[color ^ 1] = bt.saved_agehama[color ^ 1, ply - 1];
            bt.kou = NSquare;
            bt.kou_array[ply] = 0;
            --bt.ply;

            if (bt.connect_flag[ply] == false)
            {
                // 自分の石と連絡していない手
                //bt.seq_number_table[color, sq] = NSquare;
                bt.seq_number_table[color, sq] = seq_max;
                bt.seq_sq[color, bt.next_seq_num[color] - 1].Clear();
                bt.dame_sq[color, bt.next_seq_num[color] - 1].Clear();
                bt.next_seq_num[color]--;
            }
            else
            {
                // 自分の石と連絡している場合の手（ノビ, サガリ, ツギ他）
                // 基になる連番号の情報を戻す
                IEnumerable<short> v = bt.saved_base_seq_sq[ply].Intersect(bt.seq_sq[color, bt.saved_base_seq_num[ply]]);
                bt.seq_sq[color, bt.saved_base_seq_num[ply]] = v.ToList();
                v = bt.saved_base_dame_sq[ply].Intersect(bt.dame_sq[color, bt.saved_base_seq_num[ply]]);
                bt.dame_sq[color, bt.saved_base_seq_num[ply]] = v.ToList();
                bt.dame_sq[color, bt.saved_base_seq_num[ply]].Add(sq);
                bt.connect_flag[ply] = false;
                bt.saved_base_seq_num[ply] = seq_max;
                bt.saved_base_seq_sq[ply].Clear();
                bt.saved_base_dame_sq[ply].Clear();

                for (i = 0; i < 3; i++)
                {
                    if (bt.saved_seq_num[ply, i] == seq_max)
                        break;

                    //v = bt.saved_seq_sq[ply, i].Intersect(bt.seq_sq[color, bt.saved_seq_num[ply, i]]);
                    //bt.seq_sq[color, bt.saved_seq_num[ply, i]] = v.ToList();
                    bt.seq_sq[color, bt.saved_seq_num[ply, i]] = bt.saved_seq_sq[ply, i].ToList();
                    foreach (short j in bt.seq_sq[color, bt.saved_seq_num[ply, i]])
                        bt.seq_number_table[color, j] = bt.saved_seq_num[ply, i];
                    //v = bt.saved_dame_sq[ply, i].Intersect(bt.dame_sq[color, bt.saved_seq_num[ply, i]]);
                    //bt.dame_sq[color, bt.saved_seq_num[ply, i]] = v.ToList();
                    bt.dame_sq[color, bt.saved_seq_num[ply, i]] = bt.saved_dame_sq[ply, i].ToList();
                    bt.saved_seq_num[ply, i] = seq_max;
                    bt.saved_seq_sq[ply, i].Clear();
                    bt.saved_dame_sq[ply, i].Clear();
                }
            }

            for (i = 0; i < 4; i++)
            {
                if (bt.saved_opp_seq_num[ply, i] == seq_max)
                    break;
                bt.dame_sq[color ^ 1, bt.saved_opp_seq_num[ply, i]] = bt.saved_opp_seq_dame_sq[ply, i].ToList();
                bt.saved_opp_seq_num[ply, i] = seq_max;
                bt.saved_opp_seq_dame_sq[ply, i].Clear();
                //bt.saved_opp_seq_sq
            }

            if (bt.tori_flag[ply] == true)
            {
                for (i = 0; i < 4; i++)
                {
                    if (bt.removed_seq_num[ply, i] == seq_max)
                        break;
                    bt.seq_sq[color ^ 1, bt.removed_seq_num[ply, i]] = bt.removed_seq_sq[ply, i].ToList();
                    foreach(short j in bt.removed_seq_sq[ply, i])
                    {
                        bt.seq_number_table[color ^ 1, j] = bt.removed_seq_num[ply, i];
                        bt.board[j] = (short)(color ^ 1);
                        bt.pos_empty.Remove(j);
                    }                       
                    bt.removed_seq_num[ply, i] = seq_max;
                    bt.removed_seq_sq[ply, i].Clear();
                }

                for (i = 0; i < 4; i++)
                {
                    if (bt.saved_made_dame_square[ply, i].Count == 0)
                        continue;

                    // トリによって増えた自分の駄目を戻す
                    k = l = 0;
                    for (l = 0; l < bt.saved_made_dame_square[ply, i].Count; l++)
                    {
                        for (k = 0; k < PosCrossTable[bt.saved_made_dame_square[ply, i][l]].Count; k++)
                        {
                            if (bt.board[PosCrossTable[bt.saved_made_dame_square[ply, i][l]][k]] == color)
                            {
                                //var x = bt.seq_number_table[color, PosCrossTable[bt.saved_made_dame_square[ply, i][l]][k]];
                                if (bt.dame_sq[color, bt.seq_number_table[color, PosCrossTable[bt.saved_made_dame_square[ply, i][l]][k]]].Contains(bt.saved_made_dame_square[ply, i][l]))
                                {
                                    //bt.dame_sq[color, bt.seq_number_table[color, PosCrossTable[bt.saved_made_dame_square[ply, i][l]][k]]].Add(bt.saved_made_dame_square[ply, i][l]);
                                    bt.dame_sq[color, bt.seq_number_table[color, PosCrossTable[bt.saved_made_dame_square[ply, i][l]][k]]].Remove(bt.saved_made_dame_square[ply, i][l]);
                                }
                            }
                        }
                    }

                    bt.saved_made_dame_square[ply, i].Clear();
                }
            }
        }
    }
}
