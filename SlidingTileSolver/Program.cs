using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        /*
        var info = new PuzzleInfo(8, 2, 0);
        PuzzleSolver.Solve(info);
        */

        var cpuThreads = 14;
        PuzzleInfo.THREADS = cpuThreads;
        PuzzleInfo.WRITE_BFS_CSV = true;
        var info = new PuzzleInfo(4, 4, 15, false);
        PuzzleSolver.Solve(info);

    }
}
