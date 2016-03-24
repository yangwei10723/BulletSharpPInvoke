﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BulletSharpGen
{
    [Flags]
    public enum WriteTo
    {
        None = 0,
        WriterFlags = 1,
        Header = 2,
        Source = 4,
        CS = 8,
        Buffer = 16,
        CMake = 32,
    }

    public abstract class WrapperWriter
    {
        private const int TabWidth = 4;

        public WrapperProject Project { get; }
        public WriteTo To { get; set; }

        private readonly Dictionary<WriteTo, FileStream> _streams = new Dictionary<WriteTo, FileStream>();
        private readonly Dictionary<WriteTo, StreamWriter> _writers = new Dictionary<WriteTo, StreamWriter>();
        public Dictionary<WriteTo, int> LineLengths { get; } = new Dictionary<WriteTo, int>();
        private StringBuilder _bufferBuilder;
        protected bool hasHeaderWhiteSpace;
        protected bool hasSourceWhiteSpace;
        protected bool hasCSWhiteSpace;

        protected WrapperWriter(WrapperProject project)
        {
            Project = project;
        }

        protected void OpenFile(string filename, WriteTo to)
        {
            if (to == WriteTo.Buffer)
            {
                _bufferBuilder = new StringBuilder();
            }
            else
            {
                _streams[to] = new FileStream(filename, FileMode.Create, FileAccess.Write);
                _writers[to] = new StreamWriter(_streams[to]);
            }
            LineLengths[to] = 0;

            To = to;
        }

        protected void CloseFile(WriteTo to)
        {
            if (to == WriteTo.Buffer) return;

            _writers[to].Dispose();
            _streams[to].Dispose();
        }

        protected void ClearBuffer()
        {
            _bufferBuilder.Clear();
        }

        protected string GetBufferString()
        {
            return _bufferBuilder.ToString();
        }

        public abstract void Output();

        private static IEnumerable<WriteTo> GetIndividualFlags(WriteTo to)
        {
            if ((to & WriteTo.Header) != 0)
            {
                yield return WriteTo.Header;
            }
            if ((to & WriteTo.Source) != 0)
            {
                yield return WriteTo.Source;
            }
            if ((to & WriteTo.CS) != 0)
            {
                yield return WriteTo.CS;
            }
            if ((to & WriteTo.Buffer) != 0)
            {
                yield return WriteTo.Buffer;
            }
            if ((to & WriteTo.CMake) != 0)
            {
                yield return WriteTo.CMake;
            }
        }

        protected void Write(string s, WriteTo to = WriteTo.WriterFlags)
        {
            if (to == WriteTo.WriterFlags) to = To;

            int sLength = s.Length + s.Count(c => c == '\t') * (TabWidth - 1);

            foreach (var toFlag in GetIndividualFlags(to))
            {
                if (toFlag == WriteTo.Buffer)
                {
                    _bufferBuilder.Append(s);
                }
                else
                {
                    _writers[toFlag].Write(s);
                }

                LineLengths[toFlag] += sLength;
            }
        }

        protected void Write(char c, WriteTo to = WriteTo.WriterFlags)
        {
            Write(c.ToString(), to);
        }

        protected void WriteLine(string s, WriteTo to = WriteTo.WriterFlags)
        {
            Write(s, to);
            WriteLine(to);
        }

        protected void WriteLine(char c, WriteTo to = WriteTo.WriterFlags)
        {
            Write(c, to);
            WriteLine(to);
        }

        protected void WriteLine(WriteTo to = WriteTo.WriterFlags)
        {
            if (to == WriteTo.WriterFlags) to = To;
            Write("\r\n", to);

            foreach (var toFlag in GetIndividualFlags(to))
            {
                LineLengths[toFlag] = 0;
            }
        }

        protected void WriteTabs(int n, WriteTo to = WriteTo.WriterFlags)
        {
            for (int i = 0; i < n; i++)
            {
                Write('\t', to);
            }
        }

        protected void EnsureWhiteSpace(WriteTo to = WriteTo.WriterFlags)
        {
            if (to == WriteTo.WriterFlags) to = To;

            if ((to & WriteTo.Source) != 0)
            {
                if (!hasSourceWhiteSpace)
                {
                    WriteLine(WriteTo.Source);
                    hasSourceWhiteSpace = true;
                }
            }
            if ((to & WriteTo.CS) != 0)
            {
                if (!hasCSWhiteSpace)
                {
                    WriteLine(WriteTo.CS);
                    hasCSWhiteSpace = true;
                }
            }
        }
    }
}
