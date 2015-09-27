using System.Collections.Generic;

namespace Assets.Source {

    /* Holds constants */
    public static class Constants {
        public const long InitialBoard = 124141717933596L;
        public const long GoalBoard = 16777216L;
        public const long ValidBoardCells = 124141734710812L; // equal "InitialBoard | GoalBoard"

        public static readonly int[] ValidBoardCellsList = {
            2, 3, 4, 9, 10, 11, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 37, 38, 39, 44, 45, 46
        };

        // holds all 76 moves that are possible
        // the inner array is structures as following:
        // - first entry holds the peg that is added by the move
        // - second entry holds the two pegs that are removed by the move
        // - third entry holds all three involved pegs
        public static readonly long[][] Moves = _InitMoves();

        // helper
        private static void _CreateMoves(int bit1, int bit2, int bit3, List<long[]> moves) {
            moves.Add(new[] {(1L << bit1), (1L << bit2) | (1L << bit3), (1L << bit1) | (1L << bit2) | (1L << bit3)});
            moves.Add(new[] {(1L << bit3), (1L << bit2) | (1L << bit1), (1L << bit1) | (1L << bit2) | (1L << bit3)});
        }

        // initialize all valid moves
        private static long[][] _InitMoves() {
            List<long[]> moves = new List<long[]>();
            int[] starts = {2, 9, 14, 15, 16, 17, 18, 21, 22, 23, 24, 25, 28, 29, 30, 31, 32, 37, 44};
            foreach (int start in starts) {
                _CreateMoves(start, start + 1, start + 2, moves);
                _CreateMoves((start%7)*7 + start/7, (start%7 + 1)*7 + start/7,
                    (start%7 + 2)*7 + start/7, moves);
            }
            return moves.ToArray();
        }

    }
}