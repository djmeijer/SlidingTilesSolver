using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public class MultislideFrontierCollector
{
    public readonly MultislideFrontier Frontier;
    public readonly PuzzleInfo Info;
    public BfsCsvWriter CsvWriter { get; set; }
    public int Depth { get; set; }
    public int Segment { get; set; } = -1;
    private readonly byte[] TempBuffer;
    private readonly uint[] Vals;
    private int BufferPosition;

    public MultislideFrontierCollector(MultislideFrontier frontier, PuzzleInfo info, byte[] tempBuffer, uint[] vals)
    {
        Frontier = frontier;
        Info = info;
        TempBuffer = tempBuffer;
        Vals = vals;
        BufferPosition = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Add(uint val)
    {
        Vals[BufferPosition++] = val;
        if (BufferPosition == Vals.Length)
        {
            Flush();
        }
    }

    private void Flush()
    {
        if (CsvWriter != null && BufferPosition > 0)
        {
            CsvWriter.WriteBatch(Segment, Vals, BufferPosition, Depth);
        }
        Frontier.Write(Segment, TempBuffer, Vals, BufferPosition);
        BufferPosition = 0;
    }

    public void Close() => Flush();
}
