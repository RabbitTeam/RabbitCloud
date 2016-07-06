using System;
using System.Collections.Generic;

namespace Rabbit.Transport.Simple.Buffer
{
    public class GrowingByteBufferManager : IBufferManager
    {
        private Stack<byte[]> _bufferStack;
        private readonly object _sync = new object();
        private double _autoExpandedScale = 1.5;

        public GrowingByteBufferManager(int initialPooledBufferCount, int bufferSize)
        {
            BufferCount = initialPooledBufferCount;
            BufferSize = bufferSize;

            AutoExpanded = true;

            Initialize();
        }

        private void Initialize()
        {
            _bufferStack = new Stack<byte[]>(BufferCount);

            for (var i = 0; i < BufferCount; i++)
            {
                var buffer = new byte[BufferSize];
                _bufferStack.Push(buffer);
            }
        }

        public int BufferCount { get; private set; }

        public int BufferSize { get; }

        public bool AutoExpanded { get; set; }

        public double AutoExpandedScale
        {
            get { return _autoExpandedScale; }
            set
            {
                if (value <= 1)
                    throw new ArgumentException("Auto expanded scale must be greater than 1.");
                _autoExpandedScale = value;
            }
        }

        public int BufferRemaning
        {
            get
            {
                lock (_sync)
                {
                    return _bufferStack.Count;
                }
            }
        }

        private void ExpandBufferStack()
        {
            var currentBufferCount = BufferCount;
            BufferCount = (int)(BufferCount * AutoExpandedScale);

            for (var i = 0; i < BufferCount - currentBufferCount; i++)
            {
                var buffer = new byte[BufferSize];
                _bufferStack.Push(buffer);
            }
        }

        public byte[] BorrowBuffer()
        {
            lock (_sync)
            {
                if (_bufferStack.Count > 0)
                    return _bufferStack.Pop();

                if (AutoExpanded)
                    ExpandBufferStack();

                if (_bufferStack.Count == 0)
                    throw new IndexOutOfRangeException("No enough available buffers.");

                return _bufferStack.Pop();
            }
        }

        public void ReturnBuffer(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            lock (_sync)
            {
                _bufferStack.Push(buffer);
            }
        }

        public void ReturnBuffers(IEnumerable<byte[]> buffers)
        {
            if (buffers == null)
                return;
            foreach (var buffer in buffers)
            {
                ReturnBuffer(buffer);
            }
        }

        public void ReturnBuffers(params byte[][] buffers)
        {
            if (buffers == null)
                return;
            foreach (var buffer in buffers)
            {
                ReturnBuffer(buffer);
            }
        }
    }
}