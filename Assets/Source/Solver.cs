using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets {
    /* Solver utility for a given board */

    internal class Solver {
        // caches all found solutions (and their paths)
        private static readonly Dictionary<long, long[]> SeenSolutions = new Dictionary<long, long[]>();
        // Recursive helper method to find a solution
        private static bool _Search(long board, HashSet<long> seenBoards, List<long> solution) {
            foreach (long[] move in Constants.Moves) {
                if ((move[1] & board) != 0L || (move[0] & board) == 0L) continue;
                long newBoard = board ^ move[2];
                if (seenBoards.Contains(newBoard)) continue;
                seenBoards.Add(newBoard);
                if (newBoard != Constants.InitialBoard && !_Search(newBoard, seenBoards, solution)) continue;
                solution.Add(board);
                return true;
            }
            return false;
        }

        // Find a solution for a given board
        public static long[] Solve(long board) {
            if (!SeenSolutions.ContainsKey(board)) {
                List<long> solution = new List<long>();
                // invert for more performant search
                _Search(board ^ Constants.ValidBoardCells, new HashSet<long>(), solution);
                long[] path = new long[0];
                if (solution.Count != 0) {
                    solution.Reverse();
                    solution.Add(Constants.InitialBoard);
                    path = solution.ToArray();
                    // reverse inversion
                    for (var i = 0; i < path.Length; i++) {
                        path[i] = path[i] ^ Constants.ValidBoardCells;
                    }
                }
                // store solution
                if (path.Length == 0) {
                    // no soltion found
                    SeenSolutions.Add(board, path);
                } else {
                    Debug.Assert(board == path[0]);
                    // store solution and all parts of it
                    for (int i = 0; i < path.Length - 1; i++) {
                        if (!SeenSolutions.ContainsKey(path[i])) {
                            SeenSolutions.Add(path[i], path.Skip(i).ToArray());
                        }
                    }
                }
            }
            return SeenSolutions[board];
        }

        // Extract next move as "from" and "to" for a given solution
        public static int[] GetNextMove(long[] solution) {
            if (solution.Length < 2) return null;
            long move = solution[0] ^ solution[1];
            int[] cells = Constants.ValidBoardCellsList.Where(cell => (move & (1L << cell)) != 0).ToArray();
            int v1 = cells.Min();
            int v2 = cells.Max();
            return (solution[0] & (move & (1L << v1))) == 0L ? new[] {v2, v1} : new[] {v1, v2};
        }

    }
}