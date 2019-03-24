using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Uface
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UImage
    {
        public int Width;
        public int Height;
        public IntPtr pixels;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JudgeResult
    {
        public int code;

        public int xmin;
        public int xmax;
        public int ymin;
        public int ymax;

        public float pitch;
        public float yaw;
        public float roll;

        public float light;
        public float blur;
        public float yinyang;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public float[] landmark;
        public int numpts;
    }

    class PInvoke
    {
        const string dll_path = "D:/git/ImageCap/uface_quality_judge_c.dll";

        [DllImport(dll_path, EntryPoint = "new_QualityJudge", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr new_QualityJudge(int min_face_size);

        [DllImport(dll_path, EntryPoint = "delete_QualityJudge", CallingConvention = CallingConvention.Cdecl)]
        public static extern void delete_QualityJudge(IntPtr cptr);

        [DllImport(dll_path, EntryPoint = "QualityJudge_isQualified", CallingConvention = CallingConvention.Cdecl)]
        public static extern int QualityJudge_isQualified(IntPtr cptr,UImage uimage);

        [DllImport(dll_path, CallingConvention = CallingConvention.Cdecl)]
        public static extern int QualityJudge_getQualified(IntPtr cptr, UImage uimage, out JudgeResult result);
    }
}
