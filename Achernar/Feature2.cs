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

namespace Achernar
{
    // 簡易的なロジスティック回帰のPolicyの特徴。まったくうまく行っていない。
    internal class Feature2
    {
        const string str_file_name = "policy_shallow.bin";
        const int fv_length = NSquare * 6 + 2;
        public double[] fv = new double[fv_length];
        public void SaveFV()
        {
            BinaryWriter bw = new BinaryWriter(File.Open(str_file_name, FileMode.Create));
            for (int i = 0; i < fv_length; i++)
            {
                bw.Write(fv[i]);
                //Console.WriteLine(fv[i]);
            }
            bw.Close();
        }
        public void LoadFV()
        {
            double max_value = 0;
            BinaryReader br = new BinaryReader(File.Open(str_file_name, FileMode.Open));
            for (int i = 0; i < fv_length; i++)
            {
                fv[i] = br.ReadDouble();
                if (fv[i] > max_value)
                {
                    max_value = fv[i];
                }
            }
            br.Close();
            Console.WriteLine("max_value = " + max_value);
        }

        public  List<int> MakeInputFeature(Board bt, short color)
        {
            List<int> li = new List<int>();
            // 黒石、白石
            for (int i = 0; i < NSquare; i++)
            {
                switch (bt.board[i])
                {
                    case 0:
                        li.Add(i);
                        break;
                    case 1:
                        li.Add(i + NSquare);
                        break;
                }
            }
            int limit;
            if (bt.current_moves.Count <= 4)
            {
                limit = bt.current_moves.Count - 1;
            }
            else
            {
                limit = 4;
            }
            int counter = 0;
            int index = NSquare;
            while (counter < limit)
            {
                li.Add(index + bt.current_moves[bt.current_moves.Count - 1 - counter]);
                index += NSquare;
                counter++;
            }

            if (color == 0)
            {
                li.Add(fv_length - 2);
            }
            else
            {
                li.Add(fv_length - 1);
            }
            return li;
        }
        public double Calc(List<int> list)
        {
            double d = 0;
            for (int i = 0; i < list.Count; i++)
            {
                d += fv[list[i]];
            }
            return d;
        }
    }
}
