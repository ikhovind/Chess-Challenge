using ChessChallenge.API;
using System;
using System.Collections.Generic;


public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        int finishTime = timer.MillisecondsRemaining - (timer.MillisecondsRemaining / 30);
        int depth = 1;
        Move bestMove = moves[0];
        int bestEval = int.MinValue;
        while (timer.MillisecondsRemaining > finishTime)
        {
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int move_eval = -evalPosition(board, depth, int.MinValue, int.MaxValue, false);
                board.UndoMove(move);
                if (move_eval > bestEval)
                {
                    bestMove = move;
                    bestEval = move_eval;
                    Console.WriteLine("Found new best move with evalution: " + move_eval);
                }
            }
            Console.WriteLine("Finished depth: " + depth + " with " + timer.MillisecondsRemaining + " ms remaining");
            depth++;
        }
        return bestMove;
    }

    private Move[] getOrderedMoves(Board board, bool captures)
    {
        Move[] moves = board.GetLegalMoves(captures);
        if (!captures)
        {
            Array.Sort(moves, new MoveOrderingClass());
        }
        return moves;
    }

    private int evalPosition(Board board, int depth, int alpha, int beta, bool quiesence)
    {
        if (depth == 0 && !quiesence)
        {
            // quiesence uses too much time currently so not currently implemented
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
        // if quiesence then we only care about captures
        Move[] moves = getOrderedMoves(board, quiesence);
        // For when quiesence search has run out of legal captures to examine
        if (moves.Length == 0)
        {
            Console.WriteLine("Reached end of quiesence depth: " + depth);
            return evalStatic(board);
        }
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            // After we have made a move - a high score is good for our oponent therefore reversese
            var eval = -evalPosition(board, depth - 1, -beta, -alpha, quiesence);
            board.UndoMove(move);
            if (eval >= beta)
            {
                return beta;
            }
            if (eval >= alpha)
            {
                alpha = eval;
            }
        }
        return alpha;
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

    public class MoveOrderingClass : Comparer<Move>
    {
        public override int Compare(Move a, Move b)
        {
            if (a.IsPromotion) return 1;
            if (b.IsPromotion) return -1;
            if (a.IsCapture)
            {
                if (b.IsCapture)
                {
                    // Order capture of strong piece with weak piece first
                    return ((int)a.CapturePieceType - (int)a.MovePieceType) - ((int)b.CapturePieceType - (int)b.MovePieceType);
                }
                // Order captures before non captures
                return 1;
            }
            if (b.IsCapture) return -1;
            if (a.MovePieceType == PieceType.Pawn)
            {
                return 1;
            }
            else if (b.MovePieceType == PieceType.Pawn)
            {
                return -1;
            }
            return 0;
        }
    }
}
