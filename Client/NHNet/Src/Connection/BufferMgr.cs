using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace nicehu.net
{
    public class BufferMgr
    {
        int totalBytesSize;//�ֽ��������ռ�
        int oneBufferSize;//һ��Event��Ҫ����Ŀռ�

        byte[] bigBufferData;//ΪN��Event�ṩ�ռ�Ĵ��ֽ�����
        int currentIndex;//��ǰ�ֽ�������ʹ�õ������λ��
        Stack<int> m_freeIndexPool;//�����յĿռ��λ��(���ظ�����)
        
       

        public BufferMgr()
        {
            totalBytesSize = NHNet.EVENTARGS_MAX_COUNT * NHNet.EVENTARGS_BUFFER_SIZE;
            oneBufferSize = NHNet.EVENTARGS_BUFFER_SIZE;

            bigBufferData = new byte[totalBytesSize];
            currentIndex = 0;
            m_freeIndexPool = new Stack<int>();
        }

        // allocate buffer space
        public bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (m_freeIndexPool.Count > 0)
            {
                args.SetBuffer(bigBufferData, m_freeIndexPool.Pop(), oneBufferSize);
            }
            else
            {
                if (currentIndex+ oneBufferSize> totalBytesSize)
                {
                    return false;
                }
                args.SetBuffer(bigBufferData, currentIndex, oneBufferSize);
                currentIndex += oneBufferSize;
            }
            return true;
        }

        //allocate buffer space and set data
        public bool SetBuffer(SocketAsyncEventArgs args, byte[] buffer, int offset, int count)
        {
            if (m_freeIndexPool.Count > 0)
            {
                int _offset = m_freeIndexPool.Pop();
                Buffer.BlockCopy(buffer, offset, bigBufferData, _offset, count);
                args.SetBuffer(bigBufferData, _offset, count);
            }
            else
            {
                if (currentIndex + oneBufferSize > totalBytesSize)
                {
                    return false;
                }
                Buffer.BlockCopy(buffer, offset, bigBufferData, currentIndex, count);
                args.SetBuffer(bigBufferData, currentIndex, count);
                currentIndex += oneBufferSize;
            }
            return true;
        }

        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            m_freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }


    }
}
