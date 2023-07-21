using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ChessChallenge.API;
using Timer = ChessChallenge.API.Timer;

public class MyBot : IChessBot{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    
    // We could hardCode some openings in here???
    // prob need a check to see if board is a standard start
    private ulong bitBoard;
    private bool doingOpener;
    private int openingIndex = 0;
    private List<Move> superGoodStrongOpening = new();
    
    public Move Think(Board board, Timer timer)
    {
        // get the bitBoards
        if (board.IsWhiteToMove)
            bitBoard = board.WhitePiecesBitboard;
        else bitBoard = board.BlackPiecesBitboard;
        
        // if white or black board is a standard setup
        if (bitBoard == 18446462598732840960 || bitBoard == 18446462598732906495 || doingOpener)
        {
            doingOpener = true;
            openingIndex++;
            // make sure we only run through the opening and dont try to acces out of bounds
            if (openingIndex == superGoodStrongOpening.Count)
            {
                doingOpener = false;
            }
            // do some opening move based on index
            
        }
        
        return SingleDepthSearch(board, timer)[0];
    }
    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    List<Move> SingleDepthSearch(Board board, Timer timer)
    {
        Random rng = new();
        Dictionary<Move, int> evaluation = new();

        Move[] legalMoves = board.GetLegalMoves();
        List<int> moveVals = new();
        List<Move> moveList = new();
        

        foreach (Move legalMove in legalMoves)
        {
            // If the move is checkmate do the move
            if (MoveIsCheckmate(board, legalMove))
            {
                moveList.Add(legalMove);
                return moveList;
            }
            
            // else evaluate the move based on the captured piece
            Piece capturedPiece = board.GetPiece(legalMove.TargetSquare);
            int capturedPieceVal = pieceValues[(int)capturedPiece.PieceType];
            evaluation.Add(legalMove, capturedPieceVal);
            moveVals.Add(capturedPieceVal);
        }
        // Get the move with the highest value
        Move bestMove = evaluation.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
        if (evaluation[bestMove] == 0) moveList.Add(legalMoves[rng.Next(legalMoves.Length)]);
        else
        {
            double avg = moveVals.Count > 0 ? moveVals.Average() : 0.0;
            foreach (Move key in evaluation.Keys)
            {
                if (evaluation[key] >= avg) moveList.Add(key);
            }
        }
        return moveList;

    }
}