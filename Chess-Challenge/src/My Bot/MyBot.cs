using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        board.MakeMove(moves[0]);
        var maxVal = Eval(board);
        board.UndoMove(moves[0]);
        var bestMove = moves[0];

        while (timer.MillisecondsRemaining > 10)
        {
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                var score = Eval(board);
                if (score > maxVal)
                {
                    bestMove = move;
                    maxVal = score;
                }
                board.UndoMove(move);
            }
            return bestMove;
        }
        return moves[0];
    }

    private int Eval(Board board)
    {
        int score = getValueDiff(board, board.IsWhiteToMove);
        return score;
    }
    private int getValueDiff(Board board, bool white)
    {
        // Eval after black has made move - so white is to move but want points from black perspective
        return getPieceValues(board, !white) - getPieceValues(board, white);
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
