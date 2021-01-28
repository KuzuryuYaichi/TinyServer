#include <vector>

class COV_Martix
{
	static int ComputeRx(float ar[], float ai[], int Row, int Line, unsigned char *Res_byte)//��Ƿ��������Ŀ�ʼλ�ã��ɵ��õĺ�������ͳ��
	{
		std::vector<double*> Ar_num(Row), Ai_num(Row); //�ö�ά�����ʾʵ�鲿
		std::vector<double*> Ar_Result(Row), Ai_Result(Row); //���֮��ʵ�鲿�Ľ��

		for (int r = 0; r < Row; r++)//�˲�����������������ֽ�����ת��Ϊint16����������ά����
		{
			//Buffer.BlockCopy(ar, r * Line * 4, Ar_num, r * Line * 4, Line * 4); // ���п���
			//Buffer.BlockCopy(ai, r * Line * 4, Ai_num, r * Line * 4, Line * 4); // ���п���
			
			for (int l = 0; l < Line; l++)
			{
				Ar_num[r][l] = ar[r][l];
				Ai_num[r][l] = ai
			}
		}

		for (int r_r = 0; r_r < Row; r_r++)//��ʾԭ���������
		{
			for (int l_r = r_r; l_r < Row; l_r++)//��ʾת�þ��������,���öԳ���
			{
				double temp_r = 0;
				double temp_i = 0;
				for (int l_pos = 0; l_pos < Line; l_pos++)//��ʾת�þ�����к�ԭ�������,ȱ�ٹ���
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

		byte[] Res_byte = new byte[Row * Row * 4 * 2];//�˴���ʵ�����鲿ƴ�ӳ�һ���ֽ����飬float=4byte����*4
		Buffer.BlockCopy(Ar_Result, 0, Res_byte, 0, Ar_Result.Length * 4);
		Buffer.BlockCopy(Ai_Result, 0, Res_byte, Ar_Result.Length * 4, Ai_Result.Length * 4);
		return Res_byte;  //���ط�������ֵ                        
	}
};
