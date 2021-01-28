#include <vector>

class COV_Martix
{
	static int ComputeRx(float ar[], float ai[], int Row, int Line, unsigned char *Res_byte)//标记方差矩阵处理的开始位置，由调用的函数负责统计
	{
		std::vector<double*> Ar_num(Row), Ai_num(Row); //用二维数组表示实虚部
		std::vector<double*> Ar_Result(Row), Ai_Result(Row); //相乘之后实虚部的结果

		for (int r = 0; r < Row; r++)//此步操作将函数输入的字节数组转化为int16存入两个二维数组
		{
			//Buffer.BlockCopy(ar, r * Line * 4, Ar_num, r * Line * 4, Line * 4); // 按行拷贝
			//Buffer.BlockCopy(ai, r * Line * 4, Ai_num, r * Line * 4, Line * 4); // 按行拷贝
			
			for (int l = 0; l < Line; l++)
			{
				Ar_num[r][l] = ar[r][l];
				Ai_num[r][l] = ai
			}
		}

		for (int r_r = 0; r_r < Row; r_r++)//表示原矩阵的行数
		{
			for (int l_r = r_r; l_r < Row; l_r++)//表示转置矩阵的列数,利用对称性
			{
				double temp_r = 0;
				double temp_i = 0;
				for (int l_pos = 0; l_pos < Line; l_pos++)//表示转置矩阵的行和原矩阵的列,缺少共轭
				{
					temp_r += Ar_num[r_r, l_pos] * Ar_num[l_r, l_pos] + Ai_num[r_r, l_pos] * Ai_num[l_r, l_pos];
					temp_i += Ai_num[r_r, l_pos] * Ar_num[l_r, l_pos] - Ar_num[r_r, l_pos] * Ai_num[l_r, l_pos];
				}
				Ar_Result[r_r, l_r] = (float)(temp_r / Line);
				Ai_Result[r_r, l_r] = (float)(temp_i / Line);
				Ar_Result[l_r, r_r] = Ar_Result[r_r, l_r];
				Ai_Result[l_r, r_r] = -Ai_Result[r_r, l_r];
			}
		}

		byte[] Res_byte = new byte[Row * Row * 4 * 2];//此处将实部和虚部拼接成一个字节数组，float=4byte所以*4
		Buffer.BlockCopy(Ar_Result, 0, Res_byte, 0, Ar_Result.Length * 4);
		Buffer.BlockCopy(Ai_Result, 0, Res_byte, Ar_Result.Length * 4, Ai_Result.Length * 4);
		return Res_byte;  //返回方差矩阵的值                        
	}
};
