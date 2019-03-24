using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace Uface
{
    public class QualityJudge
    {
        private IntPtr cptr;
        public QualityJudge(int min_face_size)
        {
            cptr = PInvoke.new_QualityJudge(min_face_size);

            Console.WriteLine("p 1");
        }

        ~QualityJudge()
        {
            PInvoke.delete_QualityJudge(cptr);

            Console.WriteLine("p 2");
        }

  
        public int isQualified(UImage uimage)
        {
            int res = -100;
            int s = Marshal.ReadByte(uimage.pixels);
            Console.WriteLine("uiamge = " + uimage.Height);
            Console.WriteLine("uiamge pixel = " + s);
            try
            {
                res = PInvoke.QualityJudge_isQualified(cptr, uimage);//
            }            
            catch (AccessViolationException e) {
                System.Console.WriteLine(e.Message);
                return 0;
            }

            return res;
        }

        public JudgeResult getQualified(UImage uimage)
        {
            JudgeResult res = new JudgeResult();
            PInvoke.QualityJudge_getQualified(cptr, uimage, out res);
            return res;
        }
    }
}
