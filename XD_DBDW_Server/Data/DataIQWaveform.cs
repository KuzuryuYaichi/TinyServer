using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.ComponentCs;

namespace XD_DBDW_Server
{
    public class DataIQWaveform
    {
        public delegate void PassIQData(IQData t);
        public event PassIQData passIQData;
        public DataIQWaveform()
        {
            m_queue = new AsyncDataQueue<double[]>();
            m_queue.TEvent += new THandler<double[]>(m_queue_TEvent);
        }

        private AsyncDataQueue<double[]> m_queue;

        public void PushData(double[] Data)
        {
            m_queue.Enqueue(Data);
        }

        public void ClearQueue()
        {
            m_queue.ClearQueue();
        }

        void m_queue_TEvent(double[] t)
        {
            IQData nIQData = new IQData();
            nIQData.IData = new double[t.Length / 2];
            nIQData.QData = new double[t.Length / 2];
            for (int i = 0; i < t.Length / 2; i++)
            {
                nIQData.IData[i] = t[i * 2 + 1];
                nIQData.QData[i] = t[i * 2];
            }
            if (passIQData != null)
            {
                passIQData(nIQData);
            }
        }
    }
}
