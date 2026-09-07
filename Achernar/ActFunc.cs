using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Achernar
{
    public class ActFunc
    {
        const float FV_WINDOW = 256.0f;

        // Bonanza式の変形されたシグモイド関数
        public static float SigmoidBona(float x)
        {
            float delta = FV_WINDOW / 7.0f;
            if (x < -FV_WINDOW)
            { 
                x = -FV_WINDOW;
            }
            else if (x > FV_WINDOW) 
            { 
                x = FV_WINDOW;
            }
            return 1.0f / (1.0f + (float)Math.Exp(-x / delta));
        }

        // Bonanza式の変形されたシグモイド関数の導関数
        public static float dSigmoidBona(float x)
        {
            float delta = FV_WINDOW / 7.0f;
            float fd, fn, ftemp, fret;

            if (x <= -FV_WINDOW)
            {
                fret = 0.0f;
            }
            else if (x >= FV_WINDOW)
            {
                fret = 0.0f;
            }
            else
            {
                fn = (float)Math.Exp(-x / delta);
                ftemp = fn + 1.0f;
                fd = delta * ftemp * ftemp;
                fret = fn / fd;
            }
            return fret;
        }

        // 標準的なシグモイド関数
        public static float Sigmoid(float x)
        {
            return 1.0f / (1.0f + (float)Math.Exp(-x));
        }

        // 標準的なシグモイド関数の導関数
        public static float dSigmoid(float x)
        {
            return Sigmoid(x) * (1.0f - Sigmoid(x));
        }
    }
}
