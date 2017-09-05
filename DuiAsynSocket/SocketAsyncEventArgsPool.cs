using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace DuiAsynSocket
{
    /// <summary>
    /// Based on example from http://msdn2.microsoft.com/en-us/library/system.net.sockets.socketasynceventargs.socketasynceventargs.aspx
    /// Represents a collection of reusable SocketAsyncEventArgs objects.  
    /// </summary>
    internal sealed class SocketAsyncEventArgsPool
    {
        /// <summary>
        /// Pool of SocketAsyncEventArgs.
        /// </summary>
        Stack<SocketAsyncEventArgs> pool;

        internal int Count
        {
            get
            {
                if (pool == null)
                    return 0;
                return pool.Count;
            }
        }
        /// <summary>
        /// Initializes the object pool to the specified size.
        /// </summary>
        /// <param name="capacity">Maximum number of SocketAsyncEventArgs objects the pool can hold.</param>
        internal SocketAsyncEventArgsPool(Int32 capacity)
        {
            this.pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        /// <summary>
        /// Removes a SocketAsyncEventArgs instance from the pool.
        /// </summary>
        /// <returns>SocketAsyncEventArgs removed from the pool.</returns>
        internal SocketAsyncEventArgs Pop()
        {
            lock (this.pool)
            {
                if (this.pool.Count > 0)
                {
                    return this.pool.Pop();
                }
                else
                {
                    return new SocketAsyncEventArgs();
                }
            }
        }

        /// <summary>
        /// Add a SocketAsyncEventArg instance to the pool. 
        /// </summary>
        /// <param name="item">SocketAsyncEventArgs instance to add to the pool.</param>
        internal void Push(SocketAsyncEventArgs item)
        {
            lock (this.pool)
            {
                this.pool.Push(item);
            }
        }

        internal void Clear()
        {
            lock (this.pool)
            {
                while (true)
                {
                    if (pool == null || pool.Count <= 0)
                        break;
                    var args = pool.Pop();
                    if (args != null)
                    {
                        args.SetBuffer(null, 0, 0);
                        args = null;
                    }
                }
            }
        }
    }
}
