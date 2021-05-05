using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Combinations
{
    public class BackedQueue<T>
    {
        private readonly object _sync = new object();
        private readonly string _folderPath;
        private TextReader _fileReader;
        private int _fileCounter;

        private Queue<T> _primary;
        private readonly Queue<Queue<T>> _inbound;
        private readonly Queue<Queue<T>> _outbound;

        public BackedQueue(string folderPath)
        {
            _folderPath = folderPath;
            _primary = new Queue<T>();
            _inbound = new Queue<Queue<T>>();
            _outbound = new Queue<Queue<T>>();
        }

        public void Enqueue(T item)
        {
            _primary.Enqueue(item);

            if (_primary.Count < 1000000)
            {
                return;
            }

            lock (item)
            {
                _inbound.Enqueue(_primary);
                _primary = _outbound.Count > 0 
                    ? _outbound.Dequeue() 
                    : new Queue<T>();
            }
        }

        public bool TryDequeue(out T item)
        {
            if (_primary.Count == 0)
            {
                lock (_sync)
                {
                    if (_outbound.Count > 0)
                    {
                        _primary = _outbound.Dequeue();
                    }
                }
            }

            if (_primary.Count > 0)
            {
                item = _primary.Dequeue();
                return true;
            }

            item = default(T);
            return false;
        }

        private void InboundProcessor()
        {
            while (true)
            {
                if (_inbound.Count > 0)
                {
                    var queue = _inbound.Dequeue();
                    var fileCounter = Interlocked.Increment(ref _fileCounter);
                    var filePath = Path.Combine(_folderPath, $"Queue.{fileCounter}.txt");

                    using (var sw = new StreamWriter(filePath))
                    {
                        while (queue.Count > 0)
                        {
                            sw.WriteLine(queue.Dequeue());
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        private void OutboundProcessor()
        {
            while (true)
            {
                var queue = new Queue<T>();
                var line = _fileReader.ReadLine();
                while (line != null && queue.Count < 1000)
                {
                    var item = _parser.Parse(line);
                    queue.Enqueue(item);
                    line = _fileReader.ReadLine();
                }

                lock (_sync)
                {
                    _outbound.Enqueue(queue);
                }
            }
        }



    }

    public class MultiFileLineReader
    {
        private readonly string _folderPath;

        public MultiFileLineReader(string folderPath)
        {
            _folderPath = folderPath;
        }

        public IEnumerable<string> GetLine()
        {
            var folder = new DirectoryInfo(_folderPath);
            var fileList = new Queue<FileInfo>(folder.EnumerateFiles());

            while (fileList.Count > 0)
            {
                var fileInfo = fileList.Dequeue();
                using (var fileReader = fileInfo.OpenText())
                {
                    var line = fileReader.ReadLine();
                    while (line != null)
                    {
                        yield return line;
                        line = fileReader.ReadLine();
                    }
                }

                fileInfo.Delete();

            }




        }
    }
}
