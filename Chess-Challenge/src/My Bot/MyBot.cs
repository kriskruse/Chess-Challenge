using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using ChessChallenge.API;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Raylib_cs;
using Timer = ChessChallenge.API.Timer;

/// <summary>
/// Trying to cached visited boards, and the move that was deemed optimal by the model
/// </summary>
public struct cachedBoard
{
    public cachedBoard(Board b, Move m, int s)
    {
        board = b;
        move = m;
        score = s;
    }
    public Board board { get; }
    public Move move { get; }
    public int score { get; }
}



// TODO: We need a more elaborate evaluation function, one that takes into account the position on the board
// TODO: We need to take the next positions into account
// TODO: The evaluation of the moves need to be threaded to keep up to speed.
public class MyBot : IChessBot{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 320, 330, 500, 900, 20000 };
    
    // We could hardCode some openings in here???
    // prob need a check to see if board is a standard start
    private ulong myBoard;
    private ulong oppBoard;
    private ulong allBoard;
    private bool doingOpener;
    private int openingIndex = 0;
    private static readonly double _gamma = 0.9;
    
    // These two are the hardcoded Italian game openers
    //https://www.chess.com/openings/Italian-Game
    private List<String> _OpeningWhite = new List<string>{"e2e4", "g1f3","f1c4"};
    private List<String> _OpeningBlack = new List<string> { "e7e5", "b8c6", "f8c5"};
    
    // This is for caching moves
    private Dictionary<Board, cachedBoard> _cache = new();

    private Random rng = new();


    public Move Think(Board board, Timer timer)
    {
        bool isWhite = board.IsWhiteToMove;
        // get the bitBoards
        if (isWhite)
        {
            myBoard = board.WhitePiecesBitboard;
            oppBoard = board.BlackPiecesBitboard;
        }
        else
        {
            myBoard = board.BlackPiecesBitboard;
            oppBoard = board.WhitePiecesBitboard;
        }

        // if white or black board is a standard setup
        if (board.AllPiecesBitboard == 18446462598732906495 || myBoard == 18446462598732840960 || doingOpener)
        {
            doingOpener = true;
            if (openingIndex < _OpeningWhite.Count)
            {
                Move Open = isWhite
                    ? new Move(_OpeningWhite[openingIndex], board)
                    : new Move(_OpeningBlack[openingIndex], board);
                openingIndex++;
                return Open;
            }
            // when we are done with the opener, set the bool to false
            doingOpener = false;
        }
        
        // return the best move possible given the board
        return EvaluateBoard(board,10).Item1;
    }
    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    (Move, double) EvaluateBoard(Board board, int n)
    {
        // check if we have seen the board before, if so return the move we assumed best
        if (_cache.ContainsKey(board)) return (_cache[board].move, _cache[board].score);
        
        Move bestMoveOnBoard = new Move();
        double bestMoveScore = 0;
        double currentMoveScore = 0;
        Move[] legalMoves = board.GetLegalMoves();
        
        foreach (var legalmove in legalMoves)
        {
            // For every move evaluate the current worth
            currentMoveScore = MoveEvaluation(board, legalmove);
            if (currentMoveScore > bestMoveScore)
            {
                bestMoveOnBoard = legalmove;
                bestMoveScore = currentMoveScore;
            }
            // As well as see the board if the move was chosen
            // Then do the same evaluation and add it to the older moves score weighted by some Gamma
        }

        if (bestMoveScore == 0) bestMoveOnBoard = legalMoves[rng.Next(legalMoves.Length)];
        // to catch null moves 
        if (bestMoveOnBoard == Move.NullMove) throw new ArgumentException("We somehow made a null move");
        
        return (bestMoveOnBoard, bestMoveScore);
    }

    double MoveEvaluation(Board board, Move move)
    {
        double score = 0;
        // return a score for a move on the given board
        if (MoveIsCheckmate(board, move)) return double.PositiveInfinity; // Checkmate is perfect score
        
        // step one, check if we capture a piece
        // Find highest value capture
        Piece capturedPiece = board.GetPiece(move.TargetSquare);
        score += pieceValues[(int)capturedPiece.PieceType];

        return score;
    }
    
    
}