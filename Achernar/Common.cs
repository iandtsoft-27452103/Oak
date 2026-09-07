using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Achernar
{
    internal class Common
    {
        public static Dictionary<char, short> FileStr2Short = new Dictionary<char, short>();
        public static Dictionary<char, short> RankStr2Short = new Dictionary<char, short>();
        public const short NSquare = 361;
        public const short NSide = 19;
        public const short Empty = 2;
        public static List<short>[] PosCrossTable = new List<short>[NSquare];
        public static short[] DirecCross = new short[4];
        public static short[] EdgeNorth = new short[NSide];
        public static short[] EdgeWest = new short[NSide];
        public static short[] EdgeEast = new short[NSide];
        public static short[] EdgeSouth = new short[NSide];
        public static short[] FileTable = new short[NSquare];
        public static short[] RankTable = new short[NSquare];
        public static string[] StrFile = {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s"};
        public static string[] StrRank = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s" };

        public static void Init()
        {
            FileStr2Short.Add('a', 0);
            FileStr2Short.Add('b', 1);
            FileStr2Short.Add('c', 2);
            FileStr2Short.Add('d', 3);
            FileStr2Short.Add('e', 4);
            FileStr2Short.Add('f', 5);
            FileStr2Short.Add('g', 6);
            FileStr2Short.Add('h', 7);
            FileStr2Short.Add('i', 8);
            FileStr2Short.Add('j', 9);
            FileStr2Short.Add('k', 10);
            FileStr2Short.Add('l', 11);
            FileStr2Short.Add('m', 12);
            FileStr2Short.Add('n', 13);
            FileStr2Short.Add('o', 14);
            FileStr2Short.Add('p', 15);
            FileStr2Short.Add('q', 16);
            FileStr2Short.Add('r', 17);
            FileStr2Short.Add('s', 18);
            RankStr2Short.Add('a', 0);
            RankStr2Short.Add('b', 19);
            RankStr2Short.Add('c', 38);
            RankStr2Short.Add('d', 57);
            RankStr2Short.Add('e', 76);
            RankStr2Short.Add('f', 95);
            RankStr2Short.Add('g', 114);
            RankStr2Short.Add('h', 133);
            RankStr2Short.Add('i', 152);
            RankStr2Short.Add('j', 171);
            RankStr2Short.Add('k', 190);
            RankStr2Short.Add('l', 209);
            RankStr2Short.Add('m', 228);
            RankStr2Short.Add('n', 247);
            RankStr2Short.Add('o', 266);
            RankStr2Short.Add('p', 285);
            RankStr2Short.Add('q', 304);
            RankStr2Short.Add('r', 323);
            RankStr2Short.Add('s', 342);

            DirecCross[0] = -19;
            DirecCross[1] = -1;
            DirecCross[2] = 1;
            DirecCross[3] = 19;

            for (short i = 0; i < NSide; i++)
            {
                EdgeNorth[i] = i;
                EdgeWest[i] = (short)(NSide * i);
                EdgeEast[i] = (short)(EdgeWest[i] + NSide - 1);
                EdgeSouth[i] = (short)(EdgeNorth[i] + NSquare - NSide);
            }

            short f = 0;
            short r = 0;
            for (short i = 0; i < NSquare; i++)
            {
                for (short j = 0; j < DirecCross.Length; j++)
                {
                    PosCrossTable[i] = new List<short>();
                    if (EdgeNorth.Contains(i))
                    {
                        if (EdgeWest.Contains(i))
                        {
                            PosCrossTable[i].Add((short)(i + DirecCross[2]));
                            PosCrossTable[i].Add((short)(i + DirecCross[3]));
                        }
                        else if (EdgeEast.Contains(i))
                        {
                            PosCrossTable[i].Add((short)(i + DirecCross[1]));
                            PosCrossTable[i].Add((short)(i + DirecCross[3]));
                        }
                        else
                        {
                            PosCrossTable[i].Add((short)(i + DirecCross[1]));
                            PosCrossTable[i].Add((short)(i + DirecCross[2]));
                            PosCrossTable[i].Add((short)(i + DirecCross[3]));
                        }
                    }
                    else if (EdgeSouth.Contains(i))
                    {
                        if (EdgeWest.Contains(i))
                        {
                            PosCrossTable[i].Add((short)(i + DirecCross[0]));
                            PosCrossTable[i].Add((short)(i + DirecCross[2]));
                        }
                        else if (EdgeEast.Contains(i))
                        {
                            PosCrossTable[i].Add((short)(i + DirecCross[0]));
                            PosCrossTable[i].Add((short)(i + DirecCross[1]));
                        }
                        else
                        {
                            PosCrossTable[i].Add((short)(i + DirecCross[0]));
                            PosCrossTable[i].Add((short)(i + DirecCross[1]));
                            PosCrossTable[i].Add((short)(i + DirecCross[2]));
                        }
                    }
                    else if (EdgeWest.Contains(i))
                    {
                        PosCrossTable[i].Add((short)(i + DirecCross[0]));
                        PosCrossTable[i].Add((short)(i + DirecCross[2]));
                        PosCrossTable[i].Add((short)(i + DirecCross[3]));
                    }
                    else if (EdgeEast.Contains(i))
                    {
                        PosCrossTable[i].Add((short)(i + DirecCross[0]));
                        PosCrossTable[i].Add((short)(i + DirecCross[1]));
                        PosCrossTable[i].Add((short)(i + DirecCross[3]));
                    }
                    else
                    {
                        PosCrossTable[i].Add((short)(i + DirecCross[0]));
                        PosCrossTable[i].Add((short)(i + DirecCross[1]));
                        PosCrossTable[i].Add((short)(i + DirecCross[2]));
                        PosCrossTable[i].Add((short)(i + DirecCross[3]));
                    }
                }

                FileTable[i] = f++;
                RankTable[i] = r;
                
                if (f == 19)
                {
                    f = 0;
                    r++;
                }                   
            }
        }

        public static string ShortToStr(short sq)
        {
            return StrFile[FileTable[sq]] + StrRank[RankTable[sq]];
        }
    }
}
