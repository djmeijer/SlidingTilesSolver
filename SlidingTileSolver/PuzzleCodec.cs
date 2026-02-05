using System;

public static class PuzzleCodec
{
    private const int OFFSET_ZERO = 15;

    public static void IndexToBoard(PuzzleInfo info, long index, Span<byte> board)
    {
        if (board.Length < info.Size) throw new ArgumentException("Board span too small.");
        Span<byte> arr = stackalloc byte[16];
        FromIndex(info, index, arr);
        Unpack(info, arr);

        int zeroPos = arr[OFFSET_ZERO];
        int listIndex = 0;
        for (int pos = 0; pos < info.Size; pos++)
        {
            if (pos == zeroPos)
            {
                board[pos] = 0;
            }
            else
            {
                board[pos] = (byte)(arr[listIndex] + 1);
                listIndex++;
            }
        }
    }

    private static void FromIndex(PuzzleInfo info, long index, Span<byte> arr)
    {
        long newIndex = index / 16;
        arr[OFFSET_ZERO] = (byte)(index - newIndex * 16);
        index = newIndex;

        int div = info.Size;
        for (int i = 0; i < info.Size - 3; i++)
        {
            div--;
            newIndex = index / div;
            arr[i] = (byte)(index - newIndex * div);
            index = newIndex;
        }
        arr[info.Size - 3] = 0;
        arr[info.Size - 2] = 0;
    }

    private static void Unpack(PuzzleInfo info, Span<byte> arr)
    {
        bool invEven = true;

        for (int i = info.Size - 2; i >= 0; i--)
        {
            for (int j = i + 1; j < info.Size - 1; j++)
            {
                if (arr[j] >= arr[i]) arr[j]++;
                else invEven = !invEven;
            }
        }

        byte zeroPos = arr[OFFSET_ZERO];
        bool rowEven = ((zeroPos / info.Width) & 1) == 0;
        bool widthIsEven = (info.Width & 1) == 0;
        bool swapLast = (widthIsEven && invEven == rowEven) || (!widthIsEven && invEven);
        if (swapLast)
        {
            byte tmp = arr[info.Size - 2];
            arr[info.Size - 2] = arr[info.Size - 3];
            arr[info.Size - 3] = tmp;
        }
    }
}
