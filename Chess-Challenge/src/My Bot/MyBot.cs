using ChessChallenge.API;
using System;


public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        int finishTime = timer.MillisecondsRemaining - (timer.MillisecondsRemaining / 30);
        uint depth = 1;
        Move bestMove = moves[0];
        int bestEval = int.MinValue;
        while (timer.MillisecondsRemaining > finishTime)
        {
            Console.WriteLine("Searching depth: " + depth + " with " + timer.MillisecondsRemaining + " ms remaining");
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int move_eval = evalPosition(board, depth);
                board.UndoMove(move);
                if (move_eval > bestEval)
                {
                    bestMove = move;
                    bestEval = move_eval;
                }
            }
            Console.WriteLine("Finished depth: " + depth + " with " + timer.MillisecondsRemaining + " ms remaining");
            depth++;
        }
        return bestMove;
    }

    private int evalPosition(Board board, uint depth)
    {
        if (depth == 0)
        {
            return evalStatic(board);
        }
        if (board.IsInCheckmate())
        {
            // Shallow mate is slighty worse than very deep mate
            return -10000 - (int)depth;
        }
        if (board.IsDraw())
        {
            return 0;
        }
        Move[] moves = board.GetLegalMoves();
        int maxEval = int.MinValue;
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            // After we have made a move - want to know how good our oponents position is
            var eval = -evalPosition(board, depth - 1);
            board.UndoMove(move);
            if (eval > maxEval)
            {
                maxEval = eval;
            }
        }
        return maxEval;
    }

    private int evalStatic(Board board)
    {
        int score = getValueDiff(board, board.IsWhiteToMove);
        return score;
    }
    private int getValueDiff(Board board, bool white)
    {
        // Get value of whoever is going to make move
        return getPieceValues(board, white) - getPieceValues(board, !white);
    }
    private int getPieceValues(Board board, bool white)
    {
        return board.GetPieceList(PieceType.Pawn, white).Count * 1
        + board.GetPieceList(PieceType.Knight, white).Count * 3
        + board.GetPieceList(PieceType.Bishop, white).Count * 3
        + board.GetPieceList(PieceType.Queen, white).Count * 9
        + board.GetPieceList(PieceType.Rook, white).Count * 5;
    }
}
