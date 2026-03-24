using System;

// 1. Core Enums
public enum CellType {
    Empty,
    Blocked
}

// 2. Shape Class (Holds its own local state)
public class Shape {
    public int Row { get; private set; }
    public int Col { get; private set; }
    public CellType[][] Matrix { get; private set; }

    public Shape(CellType[][] matrix) {
        Matrix = matrix;
        // Start at the top center of the board
        Row = 0;
        Col = 4; 
    }

    public void MoveDown() { Row++; }
    public void MoveLeft() { Col--; }
    public void MoveRight() { Col++; }
    
}

// 3. Factory Pattern for Shapes
public class ShapeGenerator {
    private Random random = new Random();

    public Shape Generate() {
        // Example: Generating a 'T' shape matrix
        CellType[][] tShapeMatrix = new CellType[][] {
            new CellType[] { CellType.Empty, CellType.Blocked, CellType.Empty },
            new CellType[] { CellType.Blocked, CellType.Blocked, CellType.Blocked },
            new CellType[] { CellType.Empty, CellType.Empty, CellType.Empty }
        };

        // In a real game, randomly select between I, J, L, O, S, T, Z matrices
        return new Shape(tShapeMatrix);
    }
}

// 4. Grid Class (Information Expert on collisions and locked blocks)
public class Grid {
    public const int Rows = 20;
    public const int Cols = 10;
    private CellType[][] board;

    public Grid() {
        board = new CellType[Rows][];
        for (int i = 0; i < Rows; i++) {
            board[i] = new CellType[Cols];
            for (int j = 0; j < Cols; j++) {
                board[i][j] = CellType.Empty;
            }
        }
    }

    // The Magic Method: Collision Detection
    public bool IsValidPosition(Shape shape, int targetRow, int targetCol) {
        int rowsSize = shape.Matrix.Length;
        int colsSize = shape.Matrix[0].Length;

        for (int i = 0; i < rowsSize; i++) {
            for (int j = 0; j < colsSize; j++) {
                
                // Only check solid blocks of the shape
                if (shape.Matrix[i][j] == CellType.Blocked) {
                    
                    int actualRow = targetRow + i;
                    int actualCol = targetCol + j;

                    // 1. Boundary Check (Walls and Floor)
                    if (actualRow >= Rows || actualCol < 0 || actualCol >= Cols) {
                        return false;
                    }

                    // 2. Collision with locked blocks (Avoid out of bounds exception by doing this second)
                    if (actualRow >= 0 && board[actualRow][actualCol] != CellType.Empty) {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    // Lock the shape into the grid when it can't move down
    public void LockShape(Shape shape) {
        int rowsSize = shape.Matrix.Length;
        int colsSize = shape.Matrix[0].Length;

        for (int i = 0; i < rowsSize; i++) {
            for (int j = 0; j < colsSize; j++) {
                if (shape.Matrix[i][j] == CellType.Blocked) {
                    board[shape.Row + i][shape.Col + j] = CellType.Blocked;
                }
            }
        }
    }

    // Check for full lines, clear them, move rows down, and return score
    public int CollectScore() {
        int linesCleared = 0;
        
        for (int r = Rows - 1; r >= 0; r--) {
            bool isFull = true;
            for (int c = 0; c < Cols; c++) {
                if (board[r][c] == CellType.Empty) {
                    isFull = false;
                    break;
                }
            }

            if (isFull) {
                linesCleared++;
                // Shift all rows above this one down
                for (int moveRow = r; moveRow > 0; moveRow--) {
                    for (int c = 0; c < Cols; c++) {
                        board[moveRow][c] = board[moveRow - 1][c];
                    }
                }
                // Clear the top row
                for (int c = 0; c < Cols; c++) {
                    board[0][c] = CellType.Empty;
                }
                r++; // Check the same row index again because a new row dropped into it
            }
        }

        // Standard Tetris scoring
        switch (linesCleared) {
            case 1: return 100;
            case 2: return 300;
            case 3: return 500;
            case 4: return 800;
            default: return 0;
        }
    }
}

// 5. Game Class (The Controller)
public class Game {
    private Grid grid;
    private Shape activeShape;
    private ShapeGenerator generator;
    
    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }

    public Game() {
        grid = new Grid();
        generator = new ShapeGenerator();
        activeShape = generator.Generate();
        Score = 0;
        IsGameOver = false;
    }

    // Handle downward movement (Tick or User Input)
    public void MoveDown() {
        if (IsGameOver) return;

        // Check if we can move down
        if (grid.IsValidPosition(activeShape, activeShape.Row + 1, activeShape.Col)) {
            activeShape.MoveDown();
        } 
        else {
            // Cannot move down -> Lock shape
            grid.LockShape(activeShape);
            Score += grid.CollectScore();
            
            // Spawn new shape
            activeShape = generator.Generate();

            // Check Game Over condition (if the new shape immediately overlaps something)
            if (!grid.IsValidPosition(activeShape, activeShape.Row, activeShape.Col)) {
                IsGameOver = true;
                Console.WriteLine("GAME OVER! Final Score: " + Score);
            }
        }
    }

    // Handle Left Input
    public void MoveLeft() {
        if (!IsGameOver && grid.IsValidPosition(activeShape, activeShape.Row, activeShape.Col - 1)) {
            activeShape.MoveLeft();
        }
    }

    // Handle Right Input
    public void MoveRight() {
        if (!IsGameOver && grid.IsValidPosition(activeShape, activeShape.Row, activeShape.Col + 1)) {
            activeShape.MoveRight();
        }
    }
}
