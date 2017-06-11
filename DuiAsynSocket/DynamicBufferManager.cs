using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuiAsynSocket
{
    /// <summary>
    /// 动态buff管理，主要用于解决socket的粘包与断包问题(断包在socket buff不够时分多次接收导致）
    /// 原理:开辟好
    /// </summary>
    public class DynamicBufferManager
    {
        private byte[] _buff = null;
        /// <summary>
        /// 记录原始大小，重新开辟内存用完后需要还原到原始大小(默认10k)
        /// </summary>
        public static int Size { get; set; } = 1024 * 10;
        private DynamicBufferManager()
        {
            _buff = new byte[Size];
            _offset = 0;
            _lenght = 0;
        }
        /// <summary>
        /// 指向有效数据起始位置
        /// </summary>
        private int _offset;
        /// <summary>
        /// 指向有效数据长度
        /// </summary>
        private int _lenght;
        /// <summary>
        /// 返回buff剩余可用长度
        /// </summary>
        private int ReserveLenght { get { return _buff.Length - _offset - _lenght; } }

        private readonly object _objLock = new object();
        /// <summary>
        /// 写入数据到动态缓存中
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        private void WriteBuffer(byte[] buffer, int offset, int count)
        {
            lock (_objLock)
            {
                if (ReserveLenght >= count) //缓冲区空间够，不需要申请
                {
                    Array.Copy(buffer, offset, _buff, _offset + _lenght, count); //追加写入的数据
                }
                else //缓冲区空间不够，需要申请更大的内存，并进行移位
                {
                    int totalSize = _buff.Length * 10; //新的大小=原来的基础上*10
                    //若总大小还是不够则需要重新设置大小
                    if (totalSize < _lenght + count)
                        totalSize = _lenght + count;

                    byte[] tmpBuffer = new byte[totalSize];
                    Array.Copy(_buff, _offset, tmpBuffer, 0, _lenght); //复制以前的数据
                    Array.Copy(buffer, offset, tmpBuffer, _lenght, count); //复制新写入的数据
                    _offset = 0;
                    _buff = tmpBuffer; //替换
                }
                _lenght += count;
            }
        }
        /// <summary>
        /// 写入数据到动态缓存中
        /// </summary>
        /// <param name="buffer"></param>
        private void WriteBuffer(byte[] buffer)
        {
            WriteBuffer(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 1G长度为错误长度
        /// </summary>
        private static int ErrorLengh = 1024 * 1024 * 1024;

        /// <summary>
        /// 是否把网络字节顺序转为本地字节顺序
        /// </summary>
        public static bool NetByteOrder { get; set; }
        /// <summary>
        /// 控制最近几次的包长度若都小于或等于初始大小则使用完后需要重置动态buff的大小，免得浪费内存
        /// </summary>
        private int _lowInitSizeTime = 5;

        /// <summary>
        /// 弹出所有已组装好的数据
        /// </summary>
        /// <returns></returns>
        private List<byte[]> PopPackets()
        {
            lock (_objLock)
            {
                var list = new List<byte[]>();

                while (true)
                {
                    if (_lenght <= 3)
                        return list;

                    var packLenght = BitConverter.ToInt32(_buff, _offset);
                    if (NetByteOrder)
                        packLenght = System.Net.IPAddress.NetworkToHostOrder(packLenght); //把网络字节顺序转为本地字节顺序

                    //检查是否有异常数据
                    if (packLenght >= ErrorLengh)
                    {
                        //重置有效数据游标
                        _offset = 0;
                        _lenght = 0;
                        return list;
                    }

                    if (packLenght <= _lenght - 4)
                    {
                        var data = new byte[packLenght];
                        Array.Copy(_buff, _offset + 4, data, 0, packLenght);

                        //过滤长度为0的错误数据
                        if (data.Length > 0)
                        {
                            list.Add(data);

                            //控制最近几次的包长度若都小于或等于初始大小则使用完后需要重置动态buff的大小，免得浪费内存
                            if (packLenght > Size)
                                _lowInitSizeTime = 5;
                            else
                            {
                                //防止无限自减
                                if (_lowInitSizeTime > -10)
                                    _lowInitSizeTime--;
                            }
                        }

                        _offset += packLenght + 4;
                        _lenght -= (packLenght + 4);
                        //数据有效长度为0后需要重置有效数据的索引值
                        if (_lenght <= 0)
                        {
                            _offset = 0;
                            //若重新分配过空间，需要还原到初始大小,防止内存浪费
                            if (_lowInitSizeTime < 0 && Size != _buff.Length)
                                _buff = new byte[Size];
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// 保存session与动态缓存哈希表(string,DynamicBufferManager)
        /// </summary>
        private static Hashtable _hashTable = new Hashtable();

        public static void Remove(string sessionId)
        {
            if (_hashTable.ContainsKey(sessionId))
            {
                _hashTable.Remove(sessionId);
            }
        }
        /// <summary>
        /// 写入数据到动态缓存中
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public static void WriteBuffer(string sessionId, byte[] buffer, int offset, int count)
        {
            DynamicBufferManager buffManager = null;
            if (_hashTable.ContainsKey(sessionId) == false)
            {
                buffManager = new DynamicBufferManager();
                _hashTable.Add(sessionId, buffManager);
            }
            else
                buffManager = _hashTable[sessionId] as DynamicBufferManager;
            buffManager.WriteBuffer(buffer, offset, count);
        }
        /// <summary>
        /// 写入数据到动态缓存中
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="buffer"></param>
        public static void WriteBuffer(string sessionId, byte[] buffer)
        {
            WriteBuffer(sessionId, buffer, 0, buffer.Length);
        }
        /// <summary>
        /// 弹出所有已组装好的数据
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static List<byte[]> PopPackets(string sessionId)
        {
            if (_hashTable.ContainsKey(sessionId))
            {
                var buffManager = _hashTable[sessionId] as DynamicBufferManager;
                return buffManager.PopPackets();
            }
            return new List<byte[]>();
        }
    }
}
