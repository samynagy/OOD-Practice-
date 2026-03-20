using System;

// 1. Core Enums & Simple Classes
public enum PlayerSymbol {
    X,
    O,
    Empty
}

public class Player {
    public PlayerSymbol Symbol { get; private set; }
    
    public Player(PlayerSymbol symbol) {
        Symbol = symbol;
    }
}

public class ScoreBoard {
    public int TotalGames { get; private set; }
    public int XWins { get; private set; }
    public int OWins { get; private set; }
    public int Draws { get; private set; }

    public void RecordWin(PlayerSymbol winner) {
        TotalGames++;
        if (winner == PlayerSymbol.X) XWins++;
        else if (winner == PlayerSymbol.O) OWins++;
    }

    public void RecordDraw() {
        TotalGames++;
        Draws++;
    }

    public void PrintScore() {
        Console.WriteLine($"\n--- SCOREBOARD ---");
        Console.WriteLine($"Games Played: {TotalGames} | X Wins: {XWins} | O Wins: {OWins} | Draws: {Draws}\n");
    }
}

public class Board {
    private PlayerSymbol[,] grid;
    private readonly int size = 3;
    public int MovesCount { get; private set; }

    public Board() {
        grid = new PlayerSymbol[size, size];
        Reset();
    }

    public void Reset() {
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                grid[i, j] = PlayerSymbol.Empty;
        MovesCount = 0;
    }

    public bool IsValidCell(int r, int c) {
        return r >= 0 && r < size && c >= 0 && c < size && grid[r, c] == PlayerSymbol.Empty;
    }

    public void PlaceSymbol(int r, int c, PlayerSymbol symbol) {
        grid[r, c] = symbol;
        MovesCount++;
    }

    public bool IsFull() {
        return MovesCount == (size * size);
    }

    public PlayerSymbol GetCell(int r, int c) {
        return grid[r, c];
    }
    
    // Optional: Simple method to visualize the board in console
    public void PrintBoard() {
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                char display = grid[i, j] == PlayerSymbol.Empty ? '-' : grid[i, j].ToString()[0];
                Console.Write(display + " ");
            }
            Console.WriteLine();
        }
    }
}

// 3. Strategy Pattern for Win Detection
public interface IWinningStrategy {
    bool CheckWin(Board board, PlayerSymbol symbol);
}

public class Classic3x3WinningStrategy : IWinningStrategy {
    public bool CheckWin(Board board, PlayerSymbol symbol) {
        // Check Rows & Columns
        for (int i = 0; i < 3; i++) {
            if ((board.GetCell(i, 0) == symbol && board.GetCell(i, 1) == symbol && board.GetCell(i, 2) == symbol) ||
                (board.GetCell(0, i) == symbol && board.GetCell(1, i) == symbol && board.GetCell(2, i) == symbol)) {
                return true;
            }
        }
        // Check Diagonals
        if ((board.GetCell(0, 0) == symbol && board.GetCell(1, 1) == symbol && board.GetCell(2, 2) == symbol) ||
            (board.GetCell(0, 2) == symbol && board.GetCell(1, 1) == symbol && board.GetCell(2, 0) == symbol)) {
            return true;
        }
        
        return false;
    }
}

// 4. State Pattern for Game Flow
public interface IGameState {
    void Play(int r, int c, PlayerSymbol symbol, Game game);
}

public class InProgressState : IGameState {
    public void Play(int r, int c, PlayerSymbol symbol, Game game) {
        if (!game.GameBoard.IsValidCell(r, c)) {
            Console.WriteLine("Invalid move! Try again.");
            return; // Reject move, state doesn't change, turn doesn't switch
        }

        // Apply move
        game.GameBoard.PlaceSymbol(r, c, symbol);
        game.GameBoard.PrintBoard();

        // Check for win using the injected strategy
        if (game.WinStrategy.CheckWin(game.GameBoard, symbol)) {
            Console.WriteLine($"\nPlayer {symbol} wins this game!");
            game.Score.RecordWin(symbol);
            game.ChangeState(new WonState());
        }
        // Check for draw
        else if (game.GameBoard.IsFull()) {
            Console.WriteLine("\nIt's a Draw!");
            game.Score.RecordDraw();
            game.ChangeState(new DrawState());
        }
        else {
            // Game continues, switch turn
            game.SwitchTurn();
        }
    }
}

public class WonState : IGameState {
    public void Play(int r, int c, PlayerSymbol symbol, Game game) {
        Console.WriteLine("Game is already finished. We have a winner! Start a new game.");
    }
}

public class DrawState : IGameState {
    public void Play(int r, int c, PlayerSymbol symbol, Game game) {
        Console.WriteLine("Game is already finished in a draw. Start a new game.");
    }
}

// 5. The Context (Game Class)
public class Game {
    public Board GameBoard { get; private set; }
    public ScoreBoard Score { get; private set; }
    public IWinningStrategy WinStrategy { get; private set; }
    
    private IGameState currentState;
    private Player playerX;
    private Player playerO;
    private Player currentPlayer;

    // Constructor utilizes Dependency Injection for the Strategy
    public Game(IWinningStrategy strategy) {
        WinStrategy = strategy;
        Score = new ScoreBoard();
        playerX = new Player(PlayerSymbol.X);
        playerO = new Player(PlayerSymbol.O);
        
        StartNewGame();
    }

    public void StartNewGame() {
        GameBoard = new Board();
        currentPlayer = playerX; // X always starts
        ChangeState(new InProgressState());
        Console.WriteLine("\n--- New Game Started ---");
    }

    public void ChangeState(IGameState newState) {
        currentState = newState;
    }

    public void SwitchTurn() {
        currentPlayer = (currentPlayer.Symbol == PlayerSymbol.X) ? playerO : playerX;
    }

    // Delegation to the Current State
    public void PlayMove(int r, int c) {
        Console.WriteLine($"\nPlayer {currentPlayer.Symbol} attempting move at ({r}, {c})");
        currentState.Play(r, c, currentPlayer.Symbol, this);
    }
}

// 6. Demo / Execution
class Program {
    static void Main(string[] args) {
        // Injecting the strategy
        IWinningStrategy classicRules = new Classic3x3WinningStrategy();
        Game ticTacToe = new Game(classicRules);

        // Simulate Game 1 (X Wins)
        ticTacToe.PlayMove(0, 0); // X
        ticTacToe.PlayMove(1, 0); // O
        ticTacToe.PlayMove(0, 1); // X
        ticTacToe.PlayMove(1, 1); // O
        ticTacToe.PlayMove(0, 2); // X Wins

        ticTacToe.Score.PrintScore();

        // Simulate attempting to play after game is won (State Pattern in action)
        ticTacToe.PlayMove(2, 2); 

        // Start Game 2
        ticTacToe.StartNewGame();
        ticTacToe.PlayMove(0, 0); // X
        ticTacToe.PlayMove(0, 0); // Invalid move by X
        // ... game continues
    }
}
