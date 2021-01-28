using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XD_DBDW_Server.DataAlgorithm
{
    class RocMartix
    {
        public static byte[] ComputeRx(byte[] ar, byte[] ai, int Row, int Line)//标记方差矩阵处理的开始位置，由调用的函数负责统计
        {
            float[,] Ar_num = new float[Row, Line];//用二维数组表示实部
            float[,] Ai_num = new float[Row, Line];//二维数组表示虚部
            float[,] Ar_num_T = new float[Line, Row];//实部矩阵的转置
            float[,] Ai_num_T = new float[Line, Row];//虚部矩阵的转置

            float[,] Ar_Result = new float[Row, Row];//相乘之后实部的结果
            float[,] Ai_Result = new float[Row, Row];//相乘之后虚部的结果
            int pos = 0;//标记处理字节数组的位置
            for (int r = 0; r < Row; r++)//此步操作将函数输入的字节数组转化为int16存入两个二维数组
            {
                for (int l = 0; l < Line; l++)
                {
                    Ar_num[r, l] = BitConverter.ToInt16(ar, pos);
                    Ar_num_T[l, r] = BitConverter.ToInt16(ar, pos);
                    Ai_num[r, l] = BitConverter.ToInt16(ai, pos);
                    Ai_num_T[l, r] = BitConverter.ToInt16(ar, pos);
                    pos += 2;//有问题
                }
            }
            for (int r_r = 0; r_r < Row; r_r++)//表示原矩阵的行数
            {
                for (int l_r = 0; l_r < Row; l_r++)//表示转置矩阵的列数
                {
                    float temp_r = 0;
                    float temp_i = 0;
                    for (int l_pos = 0; l_pos < Line; l_pos++)//表示转置矩阵的行和原矩阵的列
                    {
                        temp_r += Ar_num[r_r, l_pos] * Ar_num_T[l_pos, l_r] - Ai_num[r_r, l_pos] * Ai_num_T[l_pos, l_r];
                        temp_i += Ar_num[r_r, l_pos] * Ai_num_T[l_pos, l_r] + Ai_num[r_r, l_pos] * Ar_num_T[l_pos, l_r];
                    }
                    Ar_Result[r_r, l_r] = temp_r;
                    Ai_Result[r_r, l_r] = temp_i;
                }

            }
            byte[] Res_byte = new byte[Row * Row * 4 * 2];//此处将实部和虚部拼接成一个字节数组，float=4byte所以*4
            int Byte_Pos = 0;
            //存入实部的数据
            for (int r = 0; r < Row; r++)
            {
                for (int l = 0; l < Row; l++)
                {
                    byte[] Temp_0 = BitConverter.GetBytes(Ar_Result[r, l]);//提取出当前float值
                    for (int num = 0; num < 4; num++)
                    {
                        Res_byte[Byte_Pos] = Temp_0[num];
                        Byte_Pos++;
                    }
                }
            }
            //存入虚部的数据
            for (int r = 0; r < Row; r++)
            {
                for (int l = 0; l < Row; l++)
                {
                    byte[] Temp_0 = BitConverter.GetBytes(Ai_Result[r, l]);//提取出当前float值
                    for (int num = 0; num < 4; num++)
                    {
                        Res_byte[Byte_Pos] = Temp_0[num];
                        Byte_Pos++;
                    }
                }
            }
            return Res_byte;  //返回方差矩阵的值                        
        }

        public static byte[] ComputeRxShort(short[] ar, short[] ai, int Row, int Line)//标记方差矩阵处理的开始位置，由调用的函数负责统计
        {
            double[,] Ar_num = new double[Row, Line];//用二维数组表示实部
            double[,] Ai_num = new double[Row, Line];//二维数组表示虚部
            double[,] Ar_num_T = new double[Line, Row];//实部矩阵的转置
            double[,] Ai_num_T = new double[Line, Row];//虚部矩阵的转置

            float[,] Ar_Result = new float[Row, Row];//相乘之后实部的结果
            float[,] Ai_Result = new float[Row, Row];//相乘之后虚部的结果
            int pos = 0;//标记处理字节数组的位置
            double temp_r = 0;
            double temp_i = 0;
            for (int r = 0; r < Row; r++)//此步操作将函数输入的字节数组转化为int16存入两个二维数组
            {
                for (int l = 0; l < Line; l++)
                {
                    Ar_num[r, l] = ar[pos];// BitConverter.ToInt16(ar, pos);
                    Ar_num_T[l, r] = ar[pos];// BitConverter.ToInt16(ar, pos);
                    Ai_num[r, l] = ai[pos];//BitConverter.ToInt16(ai, pos);
                    Ai_num_T[l, r] = ai[pos];//BitConverter.ToInt16(ai, pos);
                    pos = pos + 1;//有问题
                }
            }

            for (int r_r = 0; r_r < Row; r_r++)//表示原矩阵的行数
            {
                for (int l_r = r_r; l_r < Row; l_r++)//表示转置矩阵的列数,利用对称性
                {
                    temp_r = 0;
                    temp_i = 0;
                    for (int l_pos = 0; l_pos < Line; l_pos++)//表示转置矩阵的行和原矩阵的列,缺少共轭
                    {
                        temp_r += Ar_num[r_r, l_pos] * Ar_num_T[l_pos, l_r] + Ai_num[r_r, l_pos] * Ai_num_T[l_pos, l_r];
                        temp_i += Ai_num[r_r, l_pos] * Ar_num_T[l_pos, l_r] - Ar_num[r_r, l_pos] * Ai_num_T[l_pos, l_r];
                    }
                    Ar_Result[r_r, l_r] = (float)(temp_r / Line);
                    Ai_Result[r_r, l_r] = (float)(temp_i / Line);
                    Ar_Result[l_r, r_r] = Ar_Result[r_r, l_r];
                    Ai_Result[l_r, r_r] = -Ai_Result[r_r, l_r];
                }
            }
            byte[] Res_byte = new byte[Row * Row * 4 * 2];//此处将实部和虚部拼接成一个字节数组，float=4byte所以*4
            //存入实部的数据
            Buffer.BlockCopy(Ar_Result, 0, Res_byte, 0, Ar_Result.Length * 4);
            //存入虚部的数据
            Buffer.BlockCopy(Ai_Result, 0, Res_byte, Ar_Result.Length * 4, Ai_Result.Length * 4);
            return Res_byte;  //返回方差矩阵的值                        
        }
    }
}
