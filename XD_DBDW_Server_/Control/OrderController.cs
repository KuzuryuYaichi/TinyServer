using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace XD_DBDW_Server
{
    class OrderController
    {
        const int CONTROL_HEAD = 0x44444444;
        const int CONTROL_TAIL = 0x55555555;

        public OrderController()
        {
            s_control_instruction.iPackHead = CONTROL_HEAD;
            s_control_instruction.iDeviceType = 1;
            s_control_instruction.iPackEnd = CONTROL_TAIL;
        }

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct S_Control_Instruction
        {
            public uint iPackHead;
            public uint iDeviceType;
            public uint iChannelNo;
            public uint iInstruction;
            public uint iControlContext;
            public uint cReserved1;
            public uint cReserved2;
            public uint iPackEnd;
        }

        public S_Control_Instruction s_control_instruction;

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct TestStruct
        {
            public int c;
            //字符串，SizeConst为字符串的最大长度
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string str;
            //int数组，SizeConst表示数组的个数，在转换成
            //byte数组前必须先初始化数组，再使用，初始化
            //的数组长度必须和SizeConst一致，例test = new int[6];
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public int[] test;
        }

        public byte[] StructToBytes()
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(s_control_instruction);
            //创建byte数组
            byte[] bytes = new byte[size];
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷到分配好的内存空间
            Marshal.StructureToPtr(s_control_instruction, structPtr, false);
            //从内存空间拷到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回byte数组
            return bytes;
        }

        //// <summary>
        /// 结构体转byte数组
        /// </summary>
        /// <param name="structObj">要转换的结构体</param>
        /// <returns>转换后的byte数组</returns>
        public static byte[] StructToBytes(object structObj)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(structObj);
            //创建byte数组
            byte[] bytes = new byte[size];
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷到分配好的内存空间
            Marshal.StructureToPtr(structObj, structPtr, false);
            //从内存空间拷到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回byte数组
            return bytes;
        }

        /// <summary>
        /// byte数组转结构体
        /// </summary>
        /// <param name="bytes">byte数组</param>
        /// <param name="type">结构体类型</param>
        /// <returns>转换后的结构体</returns>
        public object BytesToStuct(byte[] bytes)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(this);
            //byte数组长度小于结构体的大小
            if (size > bytes.Length)
            {
                //返回空
                return null;
            }
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将byte数组拷到分配好的内存空间
            Marshal.Copy(bytes, 0, structPtr, size);
            //将内存空间转换为目标结构体
            object obj = Marshal.PtrToStructure(structPtr, typeof(OrderController));
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回结构体
            return obj;
        }

        public byte[] pack(uint ichannel, uint iinstruction, uint MGC_AGC_Switch, uint MGC_Val)
        {
            s_control_instruction.iChannelNo = ichannel;
            s_control_instruction.iInstruction = iinstruction;
            s_control_instruction.iControlContext = MGC_AGC_Switch;
            s_control_instruction.cReserved1 = MGC_Val;
            return StructToBytes();
        }

        public byte[] pack(uint ichannel, uint iinstruction, uint iControlContext)
        {
            s_control_instruction.iChannelNo = ichannel;
            s_control_instruction.iInstruction = iinstruction;
            s_control_instruction.iControlContext = iControlContext;
            return StructToBytes();
        }
    }
}
