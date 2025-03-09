using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessAI
{
  public enum PieceType
  {
    None,
    Pawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King
  }

  public enum PieceColor
  {
    White,
    Black
  }

  public class ChessPiece
  {
    public PieceType Type { get; }
    public PieceColor Color { get; }
    public bool HasMoved { get; set; }

    public ChessPiece(PieceType type, PieceColor color)
    {
      Type = type;
      Color = color;
      HasMoved = false;
    }

    public ChessPiece Clone()
    {
      var clone = new ChessPiece(Type, Color);
      clone.HasMoved = HasMoved;
      return clone;
    }

    public override string ToString()
    {
      char pieceChar = Type switch
      {
        PieceType.Pawn => 'P',
        PieceType.Knight => 'N',
        PieceType.Bishop => 'B',
        PieceType.Rook => 'R',
        PieceType.Queen => 'Q',
        PieceType.King => 'K',
        _ => '.'
      };

      return Color == PieceColor.White ? pieceChar.ToString() : pieceChar.ToString().ToLower();
    }
  }

  public class Position
  {
    public int Row { get; }
    public int Col { get; }

    public Position(int row, int col)
    {
      Row = row;
      Col = col;
    }

    public override bool Equals(object obj)
    {
      if (obj is Position other)
      {
        return Row == other.Row && Col == other.Col;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return Row * 8 + Col;
    }

    public override string ToString()
    {
      return $"{(char)('a' + Col)}{8 - Row}";
    }
  }

  public class Move
  {
    public Position From { get; }
    public Position To { get; }
    public PieceType PromotionType { get; }

    public Move(Position from, Position to, PieceType promotionType = PieceType.None)
    {
      From = from;
      To = to;
      PromotionType = promotionType;
    }

    public override string ToString()
    {
      string move = $"{From}-{To}";
      if (PromotionType != PieceType.None)
      {
        move += $"={PromotionType}";
      }
      return move;
    }
  }

  public class ChessBoard
  {
    private ChessPiece[,] board;
    public PieceColor CurrentPlayer { get; private set; }
    public List<Move> MoveHistory { get; }

    public ChessBoard()
    {
      board = new ChessPiece[8, 8];
      CurrentPlayer = PieceColor.White;
      MoveHistory = new List<Move>();
      InitializeBoard();
    }

    private ChessBoard(ChessPiece[,] boardState, PieceColor currentPlayer, List<Move> moveHistory)
    {
      board = boardState;
      CurrentPlayer = currentPlayer;
      MoveHistory = moveHistory;
    }

    private void InitializeBoard()
    {
      // Initialize empty board
      for (int row = 0; row < 8; row++)
      {
        for (int col = 0; col < 8; col++)
        {
          board[row, col] = null;
        }
      }

      // Set up pawns
      for (int col = 0; col < 8; col++)
      {
        board[1, col] = new ChessPiece(PieceType.Pawn, PieceColor.Black);
        board[6, col] = new ChessPiece(PieceType.Pawn, PieceColor.White);
      }

      // Set up other pieces
      SetupBackRank(0, PieceColor.Black);
      SetupBackRank(7, PieceColor.White);
    }

    private void SetupBackRank(int row, PieceColor color)
    {
      board[row, 0] = new ChessPiece(PieceType.Rook, color);
      board[row, 1] = new ChessPiece(PieceType.Knight, color);
      board[row, 2] = new ChessPiece(PieceType.Bishop, color);
      board[row, 3] = new ChessPiece(PieceType.Queen, color);
      board[row, 4] = new ChessPiece(PieceType.King, color);
      board[row, 5] = new ChessPiece(PieceType.Bishop, color);
      board[row, 6] = new ChessPiece(PieceType.Knight, color);
      board[row, 7] = new ChessPiece(PieceType.Rook, color);
    }

    public ChessPiece GetPiece(Position pos)
    {
      if (IsValidPosition(pos))
      {
        return board[pos.Row, pos.Col];
      }
      return null;
    }

    public bool IsValidPosition(Position pos)
    {
      return pos.Row >= 0 && pos.Row < 8 && pos.Col >= 0 && pos.Col < 8;
    }

    public ChessBoard Clone()
    {
      ChessPiece[,] newBoard = new ChessPiece[8, 8];
      for (int row = 0; row < 8; row++)
      {
        for (int col = 0; col < 8; col++)
        {
          if (board[row, col] != null)
          {
            newBoard[row, col] = board[row, col].Clone();
          }
        }
      }

      return new ChessBoard(newBoard, CurrentPlayer, MoveHistory.ToList());
    }

    public List<Move> GetLegalMoves()
    {
      List<Move> legalMoves = new List<Move>();

      for (int row = 0; row < 8; row++)
      {
        for (int col = 0; col < 8; col++)
        {
          ChessPiece piece = board[row, col];
          if (piece != null && piece.Color == CurrentPlayer)
          {
            Position from = new Position(row, col);
            legalMoves.AddRange(GetPieceLegalMoves(from));
          }
        }
      }

      return legalMoves;
    }

    private List<Move> GetPieceLegalMoves(Position from)
    {
      ChessPiece piece = GetPiece(from);
      if (piece == null)
      {
        return new List<Move>();
      }

      List<Move> moves = new List<Move>();

      switch (piece.Type)
      {
        case PieceType.Pawn:
          GetPawnMoves(from, moves);
          break;
        case PieceType.Knight:
          GetKnightMoves(from, moves);
          break;
        case PieceType.Bishop:
          GetBishopMoves(from, moves);
          break;
        case PieceType.Rook:
          GetRookMoves(from, moves);
          break;
        case PieceType.Queen:
          GetQueenMoves(from, moves);
          break;
        case PieceType.King:
          GetKingMoves(from, moves);
          break;
      }

      // Filter out moves that would leave the king in check
      return moves.Where(move => !WouldBeInCheckAfterMove(move)).ToList();
    }

    private void GetPawnMoves(Position from, List<Move> moves)
    {
      ChessPiece pawn = GetPiece(from);
      int direction = pawn.Color == PieceColor.White ? -1 : 1;
      int startRow = pawn.Color == PieceColor.White ? 6 : 1;
      int promotionRow = pawn.Color == PieceColor.White ? 0 : 7;

      // Forward move
      Position oneStep = new Position(from.Row + direction, from.Col);
      if (IsValidPosition(oneStep) && GetPiece(oneStep) == null)
      {
        if (oneStep.Row == promotionRow)
        {
          // Promotion
          AddPromotionMoves(from, oneStep, moves);
        }
        else
        {
          moves.Add(new Move(from, oneStep));
        }

        // Two steps forward from starting position
        if (from.Row == startRow)
        {
          Position twoStep = new Position(from.Row + 2 * direction, from.Col);
          if (IsValidPosition(twoStep) && GetPiece(twoStep) == null)
          {
            moves.Add(new Move(from, twoStep));
          }
        }
      }

      // Captures
      for (int colOffset = -1; colOffset <= 1; colOffset += 2)
      {
        Position capture = new Position(from.Row + direction, from.Col + colOffset);
        if (IsValidPosition(capture))
        {
          ChessPiece targetPiece = GetPiece(capture);
          if (targetPiece != null && targetPiece.Color != pawn.Color)
          {
            if (capture.Row == promotionRow)
            {
              // Promotion with capture
              AddPromotionMoves(from, capture, moves);
            }
            else
            {
              moves.Add(new Move(from, capture));
            }
          }
        }
      }

      // En passant (simplified implementation)
      if (MoveHistory.Count > 0)
      {
        Move lastMove = MoveHistory.Last();
        ChessPiece lastMovedPiece = GetPiece(lastMove.To);

        if (lastMovedPiece != null &&
            lastMovedPiece.Type == PieceType.Pawn &&
            Math.Abs(lastMove.From.Row - lastMove.To.Row) == 2 &&
            lastMove.To.Row == from.Row &&
            Math.Abs(lastMove.To.Col - from.Col) == 1)
        {
          Position enPassantTarget = new Position(from.Row + direction, lastMove.To.Col);
          moves.Add(new Move(from, enPassantTarget));
        }
      }
    }

    private void AddPromotionMoves(Position from, Position to, List<Move> moves)
    {
      moves.Add(new Move(from, to, PieceType.Queen));
      moves.Add(new Move(from, to, PieceType.Rook));
      moves.Add(new Move(from, to, PieceType.Bishop));
      moves.Add(new Move(from, to, PieceType.Knight));
    }

    private void GetKnightMoves(Position from, List<Move> moves)
    {
      int[] rowOffsets = { -2, -2, -1, -1, 1, 1, 2, 2 };
      int[] colOffsets = { -1, 1, -2, 2, -2, 2, -1, 1 };

      for (int i = 0; i < 8; i++)
      {
        Position to = new Position(from.Row + rowOffsets[i], from.Col + colOffsets[i]);
        if (IsValidPosition(to))
        {
          ChessPiece targetPiece = GetPiece(to);
          if (targetPiece == null || targetPiece.Color != CurrentPlayer)
          {
            moves.Add(new Move(from, to));
          }
        }
      }
    }

    private void GetBishopMoves(Position from, List<Move> moves)
    {
      int[] rowDirections = { -1, -1, 1, 1 };
      int[] colDirections = { -1, 1, -1, 1 };

      for (int i = 0; i < 4; i++)
      {
        int rowDir = rowDirections[i];
        int colDir = colDirections[i];

        for (int step = 1; step < 8; step++)
        {
          Position to = new Position(from.Row + step * rowDir, from.Col + step * colDir);
          if (!IsValidPosition(to))
          {
            break;
          }

          ChessPiece targetPiece = GetPiece(to);
          if (targetPiece == null)
          {
            moves.Add(new Move(from, to));
          }
          else
          {
            if (targetPiece.Color != CurrentPlayer)
            {
              moves.Add(new Move(from, to));
            }
            break;
          }
        }
      }
    }

    private void GetRookMoves(Position from, List<Move> moves)
    {
      int[] rowDirections = { -1, 0, 1, 0 };
      int[] colDirections = { 0, 1, 0, -1 };

      for (int i = 0; i < 4; i++)
      {
        int rowDir = rowDirections[i];
        int colDir = colDirections[i];

        for (int step = 1; step < 8; step++)
        {
          Position to = new Position(from.Row + step * rowDir, from.Col + step * colDir);
          if (!IsValidPosition(to))
          {
            break;
          }

          ChessPiece targetPiece = GetPiece(to);
          if (targetPiece == null)
          {
            moves.Add(new Move(from, to));
          }
          else
          {
            if (targetPiece.Color != CurrentPlayer)
            {
              moves.Add(new Move(from, to));
            }
            break;
          }
        }
      }
    }

    private void GetQueenMoves(Position from, List<Move> moves)
    {
      GetBishopMoves(from, moves);
      GetRookMoves(from, moves);
    }

    private void GetKingMoves(Position from, List<Move> moves)
    {
      for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
      {
        for (int colOffset = -1; colOffset <= 1; colOffset++)
        {
          if (rowOffset == 0 && colOffset == 0)
          {
            continue;
          }

          Position to = new Position(from.Row + rowOffset, from.Col + colOffset);
          if (IsValidPosition(to))
          {
            ChessPiece targetPiece = GetPiece(to);
            if (targetPiece == null || targetPiece.Color != CurrentPlayer)
            {
              moves.Add(new Move(from, to));
            }
          }
        }
      }

      // Castling
      if (!IsInCheck() && !GetPiece(from).HasMoved)
      {
        // Kingside castling
        TryCastling(from, 0, moves);
        // Queenside castling
        TryCastling(from, 7, moves);
      }
    }

    private void TryCastling(Position kingPos, int rookCol, List<Move> moves)
    {
      int row = kingPos.Row;
      Position rookPos = new Position(row, rookCol);
      ChessPiece rook = GetPiece(rookPos);

      if (rook == null || rook.Type != PieceType.Rook || rook.HasMoved)
      {
        return;
      }

      int direction = rookCol == 0 ? -1 : 1;
      int kingDestCol = kingPos.Col + 2 * direction;

      // Check if path is clear
      bool pathClear = true;
      for (int col = kingPos.Col + direction; col != rookCol; col += direction)
      {
        if (GetPiece(new Position(row, col)) != null)
        {
          pathClear = false;
          break;
        }
      }

      if (pathClear)
      {
        // Check if king passes through check
        bool passesThroughCheck = false;
        for (int col = kingPos.Col; col != kingDestCol + direction; col += direction)
        {
          if (IsSquareAttacked(new Position(row, col), CurrentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White))
          {
            passesThroughCheck = true;
            break;
          }
        }

        if (!passesThroughCheck)
        {
          moves.Add(new Move(kingPos, new Position(row, kingDestCol)));
        }
      }
    }

    public bool MakeMove(Move move)
    {
      if (!IsLegalMove(move))
      {
        return false;
      }

      ChessPiece movingPiece = GetPiece(move.From);
      ChessPiece capturedPiece = GetPiece(move.To);

      // Handle special moves
      if (movingPiece.Type == PieceType.King && Math.Abs(move.From.Col - move.To.Col) == 2)
      {
        // Castling
        int direction = move.To.Col > move.From.Col ? 1 : -1;
        int rookCol = direction == 1 ? 7 : 0;
        Position rookFrom = new Position(move.From.Row, rookCol);
        Position rookTo = new Position(move.From.Row, move.From.Col + direction);

        board[rookTo.Row, rookTo.Col] = board[rookFrom.Row, rookFrom.Col];
        board[rookFrom.Row, rookFrom.Col] = null;
        board[rookTo.Row, rookTo.Col].HasMoved = true;
      }
      else if (movingPiece.Type == PieceType.Pawn && move.From.Col != move.To.Col && capturedPiece == null)
      {
        // En passant
        Position capturedPawnPos = new Position(move.From.Row, move.To.Col);
        board[capturedPawnPos.Row, capturedPawnPos.Col] = null;
      }

      // Move the piece
      board[move.To.Row, move.To.Col] = movingPiece;
      board[move.From.Row, move.From.Col] = null;
      movingPiece.HasMoved = true;

      // Handle promotion
      if (move.PromotionType != PieceType.None)
      {
        board[move.To.Row, move.To.Col] = new ChessPiece(move.PromotionType, movingPiece.Color);
      }

      // Add to move history
      MoveHistory.Add(move);

      // Switch player
      CurrentPlayer = CurrentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;

      return true;
    }

    public bool IsLegalMove(Move move)
    {
      return GetLegalMoves().Any(m =>
          m.From.Row == move.From.Row &&
          m.From.Col == move.From.Col &&
          m.To.Row == move.To.Row &&
          m.To.Col == move.To.Col &&
          m.PromotionType == move.PromotionType);
    }

    private bool WouldBeInCheckAfterMove(Move move)
    {
      ChessBoard tempBoard = Clone();
      ChessPiece movingPiece = tempBoard.GetPiece(move.From);
      ChessPiece capturedPiece = tempBoard.GetPiece(move.To);

      // Make the move on the temporary board
      tempBoard.board[move.To.Row, move.To.Col] = movingPiece;
      tempBoard.board[move.From.Row, move.From.Col] = null;

      // Handle en passant
      if (movingPiece.Type == PieceType.Pawn && move.From.Col != move.To.Col && capturedPiece == null)
      {
        Position capturedPawnPos = new Position(move.From.Row, move.To.Col);
        tempBoard.board[capturedPawnPos.Row, capturedPawnPos.Col] = null;
      }

      // Handle promotion
      if (move.PromotionType != PieceType.None)
      {
        tempBoard.board[move.To.Row, move.To.Col] = new ChessPiece(move.PromotionType, movingPiece.Color);
      }

      // Check if the king is in check
      return tempBoard.IsInCheck(CurrentPlayer);
    }

    public bool IsInCheck(PieceColor color = PieceColor.None)
    {
      if (color == PieceColor.None)
      {
        color = CurrentPlayer;
      }

      Position kingPosition = FindKing(color);
      if (kingPosition == null)
      {
        return false;
      }

      return IsSquareAttacked(kingPosition, color == PieceColor.White ? PieceColor.Black : PieceColor.White);
    }

    private Position FindKing(PieceColor color)
    {
      for (int row = 0; row < 8; row++)
      {
        for (int col = 0; col < 8; col++)
        {
          ChessPiece piece = board[row, col];
          if (piece != null && piece.Type == PieceType.King && piece.Color == color)
          {
            return new Position(row, col);
          }
        }
      }
      return null;
    }

    private bool IsSquareAttacked(Position position, PieceColor attackerColor)
    {
      // Check for pawn attacks
      int pawnDirection = attackerColor == PieceColor.White ? -1 : 1;
      for (int colOffset = -1; colOffset <= 1; colOffset += 2)
      {
        Position pawnPos = new Position(position.Row - pawnDirection, position.Col + colOffset);
        if (IsValidPosition(pawnPos))
        {
          ChessPiece piece = GetPiece(pawnPos);
          if (piece != null && piece.Type == PieceType.Pawn && piece.Color == attackerColor)
          {
            return true;
          }
        }
      }

      // Check for knight attacks
      int[] knightRowOffsets = { -2, -2, -1, -1, 1, 1, 2, 2 };
      int[] knightColOffsets = { -1, 1, -2, 2, -2, 2, -1, 1 };
      for (int i = 0; i < 8; i++)
      {
        Position knightPos = new Position(position.Row + knightRowOffsets[i], position.Col + knightColOffsets[i]);
        if (IsValidPosition(knightPos))
        {
          ChessPiece piece = GetPiece(knightPos);
          if (piece != null && piece.Type == PieceType.Knight && piece.Color == attackerColor)
          {
            return true;
          }
        }
      }

      // Check for king attacks
      for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
      {
        for (int colOffset = -1; colOffset <= 1; colOffset++)
        {
          if (rowOffset == 0 && colOffset == 0)
          {
            continue;
          }

          Position kingPos = new Position(position.Row + rowOffset, position.Col + colOffset);
          if (IsValidPosition(kingPos))
          {
            ChessPiece piece = GetPiece(kingPos);
            if (piece != null && piece.Type == PieceType.King && piece.Color == attackerColor)
            {
              return true;
            }
          }
        }
      }

      // Check for sliding piece attacks (bishop, rook, queen)
      int[] bishopRowDirs = { -1, -1, 1, 1 };
      int[] bishopColDirs = { -1, 1, -1, 1 };
      int[] rookRowDirs = { -1, 0, 1, 0 };
      int[] rookColDirs = { 0, 1, 0, -1 };

      // Check bishop-like moves (bishop, queen)
      for (int i = 0; i < 4; i++)
      {
        for (int step = 1; step < 8; step++)
        {
          Position pos = new Position(position.Row + step * bishopRowDirs[i], position.Col + step * bishopColDirs[i]);
          if (!IsValidPosition(pos))
          {
            break;
          }

          ChessPiece piece = GetPiece(pos);
          if (piece != null)
          {
            if (piece.Color == attackerColor &&
                (piece.Type == PieceType.Bishop || piece.Type == PieceType.Queen))
            {
              return true;
            }
            break;
          }
        }
      }

      // Check rook-like moves (rook, queen)
      for (int i = 0; i < 4; i++)
      {
        for (int step = 1; step < 8; step++)
        {
          Position pos = new Position(position.Row + step * rookRowDirs[i], position.Col + step * rookColDirs[i]);
          if (!IsValidPosition(pos))
          {
            break;
          }

          ChessPiece piece = GetPiece(pos);
          if (piece != null)
          {
            if (piece.Color == attackerColor &&
                (piece.Type == PieceType.Rook || piece.Type == PieceType.Queen))
            {
              return true;
            }
            break;
          }
        }
      }

      return false;
    }

    public bool IsGameOver(out PieceColor winner)
    {
      winner = PieceColor.None;

      // Check for checkmate or stalemate
      if (GetLegalMoves().Count == 0)
      {
        if (IsInCheck())
        {
          winner = CurrentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;
        }
        return true;
      }

      // Check for insufficient material (simplified)
      int pieceCount = 0;
      bool hasOnlyKings = true;

      for (int row = 0; row < 8; row++)
      {
        for (int col = 0; col < 8; col++)
        {
          ChessPiece piece = board[row, col];
          if (piece != null)
          {
            pieceCount++;
            if (piece.Type != PieceType.King)
            {
              hasOnlyKings = false;
            }
          }
        }
      }

      if (hasOnlyKings || pieceCount <= 3)
      {
        return true;
      }

      return false;
    }

    public void PrintBoard()
    {
      Console.WriteLine("  a b c d e f g h");
      Console.WriteLine("  ---------------");
      for (int row = 0; row < 8; row++)
      {
        Console.Write($"{8 - row}|");
        for (int col = 0; col < 8; col++)
        {
          ChessPiece piece = board[row, col];
          char pieceChar = piece == null ? '.' : piece.ToString()[0];
          Console.Write($"{pieceChar} ");
        }
        Console.WriteLine($"|{8 - row}");
      }
      Console.WriteLine("  ---------------");
      Console.WriteLine("  a b c d e f g h");
      Console.WriteLine($"Current player: {CurrentPlayer}");
    }
  }

  public class ChessAI
  {
    private readonly int maxDepth;

    public ChessAI(int maxDepth = 4)
    {
      this.maxDepth = maxDepth;
    }

    public Move FindBestMove(ChessBoard board)
    {
      List<Move> legalMoves = board.GetLegalMoves();
      if (legalMoves.Count == 0)
      {
        return null;
      }

      Move bestMove = null;
      int bestValue = int.MinValue;
      int alpha = int.MinValue;
      int beta = int.MaxValue;

      foreach (Move move in legalMoves)
      {
        ChessBoard newBoard = board.Clone();
        newBoard.MakeMove(move);

        int value = MinimaxAlphaBeta(newBoard, maxDepth - 1, alpha, beta, false);

        if (value > bestValue)
        {
          bestValue = value;
          bestMove = move;
        }

        alpha = Math.Max(alpha, bestValue);
      }

      return bestMove;
    }

    private int MinimaxAlphaBeta(ChessBoard board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
      // Check for terminal state
      PieceColor winner;
      if (board.IsGameOver(out winner) || depth == 0)
      {
        return EvaluateBoard(board);
      }

      if (maximizingPlayer)
      {
        int maxEval = int.MinValue;
        foreach (Move move in board.GetLegalMoves())
        {
          ChessBoard newBoard = board.Clone();
          newBoard.MakeMove(move);

          int eval = MinimaxAlphaBeta(newBoard, depth - 1, alpha, beta, false);
          maxEval = Math.Max(maxEval, eval);

          alpha = Math.Max(alpha, eval);
          if (beta <= alpha)
          {
            break; // Beta cutoff
          }
        }
        return maxEval;
      }
      else
      {
        int minEval = int.MaxValue;
        foreach (Move move in board.GetLegalMoves())
        {
          ChessBoard newBoard = board.Clone();
          newBoard.MakeMove(move);

          int eval = MinimaxAlphaBeta(newBoard, depth - 1, alpha, beta, true);
          minEval = Math.Min(minEval, eval);

          beta = Math.Min(beta, eval);
          if (beta <= alpha)
          {
            break; // Alpha cutoff
          }
        }
        return minEval;
      }
    }

    private int EvaluateBoard(ChessBoard board)
    {
      // Simple evaluation function based on material and position
      int score = 0;

      // Check for checkmate
      PieceColor winner;
      if (board.IsGameOver(out winner))
      {
        if (winner == PieceColor.White)
        {
          return 10000; // White wins
        }
        else if (winner == PieceColor.Black)
        {
          return -10000; // Black wins
        }
        return 0; // Draw
      }

      // Check for check
      if (board.IsInCheck(PieceColor.Black))
      {
        score += 50; // Bonus for putting black in check
      }
      if (board.IsInCheck(PieceColor.White))
      {
        score -= 50; // Penalty for being in check as white
      }

      // Material values
      Dictionary<PieceType, int> pieceValues = new Dictionary<PieceType, int>
      {
        { PieceType.Pawn, 100 },
        { PieceType.Knight, 320 },
        { PieceType.Bishop, 330 },
        { PieceType.Rook, 500 },
        { PieceType.Queen, 900 },
        { PieceType.King, 20000 }
      };

      // Position evaluation tables (simplified)
      // Pawns are encouraged to advance
      int[,] pawnPositionWhite = new int[8, 8]
      {
        { 0,  0,  0,  0,  0,  0,  0,  0 },
        { 50, 50, 50, 50, 50, 50, 50, 50 },
        { 10, 10, 20, 30, 30, 20, 10, 10 },
        { 5,  5, 10, 25, 25, 10,  5,  5 },
        { 0,  0,  0, 20, 20,  0,  0,  0 },
        { 5, -5,-10,  0,  0,-10, -5,  5 },
        { 5, 10, 10,-20,-20, 10, 10,  5 },
        { 0,  0,  0,  0,  0,  0,  0,  0 }
      };

      // Knights and bishops are encouraged to control the center
      int[,] knightPosition = new int[8, 8]
      {
        { -50,-40,-30,-30,-30,-30,-40,-50 },
        { -40,-20,  0,  0,  0,  0,-20,-40 },
        { -30,  0, 10, 15, 15, 10,  0,-30 },
        { -30,  5, 15, 20, 20, 15,  5,-30 },
        { -30,  0, 15, 20, 20, 15,  0,-30 },
        { -30,  5, 10, 15, 15, 10,  5,-30 },
        { -40,-20,  0,  5,  5,  0,-20,-40 },
        { -50,-40,-30,-30,-30,-30,-40,-50 }
      };

      // Evaluate each piece on the board
      for (int row = 0; row < 8; row++)
      {
        for (int col = 0; col < 8; col++)
        {
          Position pos = new Position(row, col);
          ChessPiece piece = board.GetPiece(pos);

          if (piece != null)
          {
            // Material value
            int pieceValue = pieceValues[piece.Type];

            // Position value (simplified for pawns and knights)
            int positionValue = 0;
            if (piece.Type == PieceType.Pawn)
            {
              positionValue = piece.Color == PieceColor.White ?
                pawnPositionWhite[row, col] :
                pawnPositionWhite[7 - row, col]; // Flip for black
            }
            else if (piece.Type == PieceType.Knight)
            {
              positionValue = knightPosition[row, col];
            }

            // Add to score (positive for white, negative for black)
            int value = pieceValue + positionValue;
            score += piece.Color == PieceColor.White ? value : -value;
          }
        }
      }

      return score;
    }
  }

  public class Program
  {
    public static void Main(string[] args)
    {
      ChessBoard board = new ChessBoard();
      ChessAI ai = new ChessAI(4); // Search depth of 4

      Console.WriteLine("Chess Game with Minimax AI and Alpha-Beta Pruning");
      Console.WriteLine("=================================================");

      while (true)
      {
        board.PrintBoard();

        PieceColor winner;
        if (board.IsGameOver(out winner))
        {
          if (winner == PieceColor.None)
          {
            Console.WriteLine("Game Over: Draw!");
          }
          else
          {
            Console.WriteLine($"Game Over: {winner} wins!");
          }
          break;
        }

        if (board.CurrentPlayer == PieceColor.White)
        {
          // Human player (White)
          Console.WriteLine("Your move (e.g., e2-e4): ");
          string input = Console.ReadLine();

          if (input.ToLower() == "quit" || input.ToLower() == "exit")
          {
            break;
          }

          try
          {
            string[] parts = input.Split('-');
            if (parts.Length != 2)
            {
              Console.WriteLine("Invalid format. Use format like 'e2-e4'");
              continue;
            }

            string fromStr = parts[0].Trim();
            string toStr = parts[1].Trim();

            int fromCol = fromStr[0] - 'a';
            int fromRow = 8 - int.Parse(fromStr[1].ToString());
            int toCol = toStr[0] - 'a';
            int toRow = 8 - int.Parse(toStr[1].ToString());

            Position from = new Position(fromRow, fromCol);
            Position to = new Position(toRow, toCol);

            Move move = new Move(from, to);

            if (!board.MakeMove(move))
            {
              Console.WriteLine("Invalid move! Try again.");
            }
          }
          catch (Exception)
          {
            Console.WriteLine("Invalid input. Use format like 'e2-e4'");
          }
        }
        else
        {
          // AI player (Black)
          Console.WriteLine("AI is thinking...");
          Move bestMove = ai.FindBestMove(board);

          if (bestMove == null)
          {
            Console.WriteLine("AI couldn't find a valid move!");
            break;
          }

          Console.WriteLine($"AI moves: {bestMove}");
          board.MakeMove(bestMove);
        }
      }
    }
  }
