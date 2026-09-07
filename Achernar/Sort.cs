using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Achernar
{
    public static class Sort
    {
        public struct MoveAndScore
        {
            public short move;
            public int score;
            public MoveAndScore()
            {
                move = new short();
                score = 0;
            }
        }

        public static void Merge(ref MoveAndScore[] A, ref MoveAndScore[] B, int left, int mid, int right)
        {
            int i = left;
            int j = mid;
            int k = 0;
            int l;
            while (i < mid && j < right)
            {
                if (A[i].score <= A[j].score)
                {
                    B[k++] = A[i++];
                }
                else
                {
                    B[k++] = A[j++];
                }
            }
            if (i == mid)
            { /* i側のAをBに移動し尽くしたので、j側も順番にBに入れていく */
                while (j < right)
                {
                    B[k++] = A[j++];
                }
            }
            else
            {
                while (i < mid)
                { /* j側のAをBに移動し尽くしたので、i側も順番にBに入れていく */
                    B[k++] = A[i++];
                }
            }
            for (l = 0; l < k; l++)
            {
                A[left + l] = B[l];
            }
        }

        public static void MergeSort(ref MoveAndScore[] A, ref MoveAndScore[] B, int left, int right)
        {
            int mid;
            if (left == right || left == right - 1) { return; }
            mid = (left + right) / 2;
            MergeSort(ref A, ref B, left, mid);
            MergeSort(ref A, ref B, mid, right);
            Merge(ref A, ref B, left, mid, right);
        }
    }

}
