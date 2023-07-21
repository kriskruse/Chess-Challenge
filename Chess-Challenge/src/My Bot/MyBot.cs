using System;
using System.Collections.Generic;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    
    public Move Think(Board board, Timer timer)
    {
        Move[] legalMoves = board.GetLegalMoves();
        Dictionary<Move, int> evaluation = new Dictionary<Move, int>();
        
        foreach (Move legalMove in legalMoves)
        {
            evaluation.Add(legalMove, Math.Max());
        }


        return 
    }
}