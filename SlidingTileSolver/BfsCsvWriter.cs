using System;
using System.IO;
using System.Text;

public sealed class BfsCsvWriter : IDisposable
{
    private readonly object Gate = new object();
    private readonly PuzzleInfo Info;
    private readonly string DirectoryPath;
    private readonly Encoding CsvEncoding;
    private readonly int NewLineBytes;
    private StreamWriter Writer;
    private long BytesInFile;
    private int FileIndex;
    private int CurrentLevel = -1;
    [ThreadStatic] private static StringBuilder ThreadBuffer;
    private const long MaxBytesPerFile = 8L * 1024 * 1024 * 1024;

    public BfsCsvWriter(PuzzleInfo info)
    {
        Info = info;
        DirectoryPath = Path.GetFullPath(PuzzleInfo.BFS_CSV_DIR, AppContext.BaseDirectory);
        CsvEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        NewLineBytes = CsvEncoding.GetByteCount(Environment.NewLine);
        Directory.CreateDirectory(DirectoryPath);
    }

    public void WriteIndex(long index, int depth)
    {
        Span<byte> board = stackalloc byte[16];
        PuzzleCodec.IndexToBoard(Info, index, board);

        var sb = ThreadBuffer ??= new StringBuilder(256);
        sb.Clear();
        AppendRow(sb, board, depth);
        WriteBuffer(sb, 1, depth);
    }

    public void WriteBatch(int segment, uint[] vals, int count, int depth)
    {
        if (count == 0) return;
        Span<byte> board = stackalloc byte[16];

        var sb = ThreadBuffer ??= new StringBuilder(256 * Math.Min(count, 1024));
        sb.Clear();
        for (int i = 0; i < count; i++)
        {
            long index = ((long)segment << PuzzleInfo.SEGMENT_SIZE_POW) | vals[i];
            PuzzleCodec.IndexToBoard(Info, index, board);
            AppendRow(sb, board, depth);
        }
        WriteBuffer(sb, count, depth);
    }

    private void AppendRow(StringBuilder sb, ReadOnlySpan<byte> board, int depth)
    {
        for (int i = 0; i < Info.Size; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append((int)board[i]);
        }
        sb.Append(',');
        sb.Append(depth);
        sb.AppendLine();
    }

    private void EnsureFileForDepth(int depth)
    {
        if (Writer != null && depth == CurrentLevel) return;
        CurrentLevel = depth;
        FileIndex = 0;
        OpenNextFile();
    }

    private void WriteBuffer(StringBuilder sb, int rows, int depth)
    {
        string text = sb.ToString();
        long bytes = CsvEncoding.GetByteCount(text);
        lock (Gate)
        {
            EnsureFileForDepth(depth);
            if (BytesInFile + bytes <= MaxBytesPerFile)
            {
                Writer.Write(text);
                BytesInFile += bytes;
                return;
            }
        }

        // Slow path if file boundary is crossed; write row-by-row.
        using var reader = new StringReader(text);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            long lineBytes = CsvEncoding.GetByteCount(line) + NewLineBytes;
            lock (Gate)
            {
                EnsureFileForDepth(depth);
                if (BytesInFile + lineBytes > MaxBytesPerFile)
                {
                    OpenNextFile();
                }
                Writer.WriteLine(line);
                BytesInFile += lineBytes;
            }
        }
    }

    private void OpenNextFile()
    {
        Writer?.Dispose();
        string path = Path.Combine(
            DirectoryPath,
            $"bfs_{CurrentLevel}_{FileIndex}.csv");
        Writer = new StreamWriter(
            new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read,
                1 << 20,
                FileOptions.SequentialScan),
            CsvEncoding,
            1 << 16);
        BytesInFile = 0;
        FileIndex++;
    }

    public void Dispose()
    {
        lock (Gate)
        {
            Writer?.Dispose();
            Writer = null;
        }
    }
}
