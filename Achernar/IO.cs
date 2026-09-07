using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Achernar.Common;

namespace Achernar
{
    internal class IO
    {
        public static List<Record> ReadRecordFile(string file_name)
        {
            string AppPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FilePath = AppPath + "\\" + file_name;
            List<Record> records = new List<Record>();
            string line;
            StreamReader sr = new StreamReader(FilePath, Encoding.UTF8);
            while ((line = sr.ReadLine()) != null)
            {
                Record record = new Record();
                string[] s = line.Split(',');
                record.players[0] = s[0];
                record.players[1] = s[1];
                if (s[2][0] == 'B')
                {
                    record.winner = 0;
                }
                else
                {
                    // 白の勝ち
                    record.winner = 1;
                }

                record.str_moves = new string[s.Length - 3];
                record.moves = new short[s.Length - 3];
                record.ply = s.Length - 3;

                for(int i = 3; i < s.Length; i++)
                {
                    record.str_moves[i - 3] = s[i];
                    record.moves[i - 3] = Str2Short(s[i]);
                }

                records.Add(record);
            }

            sr.Close();
            return records;
        }

        public static List<Book> ReadBookFile(string file_name, int limit)
        {
            string AppPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FilePath = AppPath + "\\" + file_name;
            List<Book> books = new List<Book>();
            string line;
            StreamReader sr = new StreamReader(FilePath, Encoding.UTF8);
            while ((line = sr.ReadLine()) != null)
            {
                Book book = new Book();
                book.moves = new short[limit];
                string[] s = line.Split(',');

                for (int i = 3; i < limit + 3; i++)
                    book.moves[i - 3] = Str2Short(s[i]);

                books.Add(book);
            }

            sr.Close();
            return books;
        }

        public static short Str2Short(string str_move)
        {
            short i, j;
            char chr_file = str_move[0];
            char chr_rank = str_move[1];
            FileStr2Short.TryGetValue(chr_file, out i);
            RankStr2Short.TryGetValue(chr_rank, out j);
            return (short)(i + j);
        }

        public static StreamWriter OpenStreamWriter(string file_name)
        {
            string AppPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FilePath = AppPath + "\\" + file_name;
            Encoding enc = new UTF8Encoding(false);// ここでEncodingを指定しないとBOMが入ってしまう。
            StreamWriter sw = new StreamWriter(FilePath, false, enc);
            return sw;
        }

        public static StreamWriter OpenStreamWriter(string file_name, bool is_append)
        {
            string AppPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FilePath = AppPath + "\\" + file_name;
            Encoding enc = new UTF8Encoding(false);// ここでEncodingを指定しないとBOMが入ってしまう。
            StreamWriter sw = new StreamWriter(FilePath, is_append, enc);
            return sw;
        }
    }
}
