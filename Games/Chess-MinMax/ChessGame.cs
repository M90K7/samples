using System;
using ChessAI;

namespace ChessDemo
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Chess Game with Minimax AI and Alpha-Beta Pruning");
      Console.WriteLine("=================================================");
      Console.WriteLine("Options:");
      Console.WriteLine("1. Play against AI (you as White)");
      Console.WriteLine("2. Watch AI vs AI");
      Console.WriteLine("3. Run AI benchmark");
      Console.Write("Select an option (1-3): ");

      string option = Console.ReadLine();

      switch (option)
      {
        case "1":
          PlayAgainstAI();
          break;
        case "2":
          AIvsAI();
          break;
        case "3":
          RunBenchmark();
          break;
        default:
          Console.WriteLine("Invalid option. Exiting.");
          break;
      }
    }

    static void PlayAgainstAI()
    {
      ChessBoard board = new ChessBoard();
      ChessAI.ChessAI ai = new ChessAI.ChessAI(4); // Search depth of 4

      while (true)
      {
        board.PrintBoard();

        PieceColor winner;
        if (board.IsGameOver(out winner))
        {
          PrintGameResult(winner);
          break;
        }

        if (board.CurrentPlayer == PieceColor.White)
        {
          // Human player (White)
          ProcessHumanMove(board);
        }
        else
        {
          // AI player (Black)
          ProcessAIMove(board, ai);
        }
      }
    }

    static void AIvsAI()
    {
      ChessBoard board = new ChessBoard();
      ChessAI.ChessAI whiteAI = new ChessAI.ChessAI(3); // Lower depth for white to make it faster
      ChessAI.ChessAI blackAI = new ChessAI.ChessAI(4);

      int moveCount = 0;
      const int maxMoves = 100; // Prevent infinite games

      while (moveCount < maxMoves)
      {
        board.PrintBoard();
        Console.WriteLine($"Move: {moveCount + 1}");

        PieceColor winner;
        if (board.IsGameOver(out winner))
        {
          PrintGameResult(winner);
          break;
        }

        ChessAI.ChessAI currentAI = board.CurrentPlayer == PieceColor.White ? whiteAI : blackAI;
        string playerName = board.CurrentPlayer == PieceColor.White ? "White AI" : "Black AI";

        Console.WriteLine($"{playerName} is thinking...");
        Move bestMove = currentAI.FindBestMove(board);

        if (bestMove == null)
        {
          Console.WriteLine($"{playerName} couldn't find a valid move!");
          break;
        }

        Console.WriteLine($"{playerName} moves: {bestMove}");
        board.MakeMove(bestMove);
        moveCount++;

        // Add a small delay to make the game watchable
        System.Threading.Thread.Sleep(1000);
      }

      if (moveCount >= maxMoves)
      {
        Console.WriteLine("Game ended due to move limit. It's a draw!");
      }
    }

    static void RunBenchmark()
    {
      Console.WriteLine("Running AI benchmark at different depths...");
      ChessBoard board = new ChessBoard();

      for (int depth = 1; depth <= 5; depth++)
      {
        ChessAI.ChessAI ai = new ChessAI.ChessAI(depth);

        Console.WriteLine($"Testing depth {depth}...");
        DateTime start = DateTime.Now;

        Move bestMove = ai.FindBestMove(board);

        TimeSpan elapsed = DateTime.Now - start;
        Console.WriteLine($"Depth {depth}: Best move {bestMove}, Time: {elapsed.TotalSeconds:F2} seconds");
      }
    }

    static void ProcessHumanMove(ChessBoard board)
    {
      while (true)
      {
        Console.WriteLine("Your move (e.g., e2-e4) or 'quit' to exit: ");
        string input = Console.ReadLine();

        if (input.ToLower() == "quit" || input.ToLower() == "exit")
        {
          Environment.Exit(0);
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

          // Check for promotion
          PieceType promotionType = PieceType.None;
          ChessPiece piece = board.GetPiece(from);

          if (piece != null && piece.Type == PieceType.Pawn &&
              ((piece.Color == PieceColor.White && toRow == 0) ||
               (piece.Color == PieceColor.Black && toRow == 7)))
          {
            promotionType = GetPromotionPiece();
          }

          Move move = new Move(from, to, promotionType);

          if (board.MakeMove(move))
          {
            break; // Valid move, exit the loop
          }
          else
          {
            Console.WriteLine("Invalid move! Try again.");
          }
        }
        catch (Exception)
        {
          Console.WriteLine("Invalid input. Use format like 'e2-e4'");
        }
      }
    }

    static PieceType GetPromotionPiece()
    {
      while (true)
      {
        Console.WriteLine("Promote pawn to (Q)ueen, (R)ook, (B)ishop, or k(N)ight? [Q]: ");
        string input = Console.ReadLine().ToUpper();

        if (string.IsNullOrEmpty(input) || input == "Q")
          return PieceType.Queen;
        else if (input == "R")
          return PieceType.Rook;
        else if (input == "B")
          return PieceType.Bishop;
        else if (input == "N")
          return PieceType.Knight;
        else
          Console.WriteLine("Invalid choice. Try again.");
      }
    }

    static void ProcessAIMove(ChessBoard board, ChessAI.ChessAI ai)
    {
      Console.WriteLine("AI is thinking...");
      Move bestMove = ai.FindBestMove(board);

      if (bestMove == null)
      {
        Console.WriteLine("AI couldn't find a valid move!");
        return;
      }

      Console.WriteLine($"AI moves: {bestMove}");
      board.MakeMove(bestMove);
    }

    static void PrintGameResult(PieceColor winner)
    {
      if (winner == PieceColor.None)
      {
        Console.WriteLine("Game Over: Draw!");
      }
      else
      {
        Console.WriteLine($"Game Over: {winner} wins!");
      }
    }
  }
}
