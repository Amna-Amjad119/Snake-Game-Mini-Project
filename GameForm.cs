using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SnakeGame
{
    public enum Direction { Up, Down, Left, Right, None }

    public class GameForm : Form
    {
        // Grid & cell size
        private readonly int cols = 20;     // minimum 20x20 
        private readonly int rows = 20;
        private readonly int cellSize = 20; // pixels per cell

        // Game state
        private List<Point> snake;
        private Point food;
        private Direction currentDirection = Direction.None;
        private Direction nextDirection = Direction.None;
        private Timer gameTimer;
        private Random rnd = new Random();

        // UI
        private Panel gamePanel;
        private Label lblScore;
        private Label lblHighScore;
        private Button btnStart;
        private Button btnPause;
        private Button btnRestart;
        private Label lblState;

        private int score = 0;
        private bool isRunning = false;
        private bool isPaused = false;


        public GameForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Invalidate();
            this.Refresh();


            // FOCUS FIX (MOST IMPORTANT)
            this.Load += (s, e) => this.ActiveControl = null;

            // Panel ko focus mat lene do
            gamePanel.TabStop = false;

            // Buttons ko bhi mat lene do
            btnStart.TabStop = false;
            btnPause.TabStop = false;
            btnRestart.TabStop = false;

            this.KeyPreview = true;
            this.KeyDown += GameForm_KeyDown;

            InitGame();
        }


        private void InitializeComponent()
        {
            // Form
            this.Text = "Snake Game - C# WinForms";
            this.ClientSize = new Size(cols * cellSize + 250, rows * cellSize + 20);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 240); // Soft light gray

            // Game panel
            gamePanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(cols * cellSize, rows * cellSize),
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };
            gamePanel.Paint += GamePanel_Paint;
            this.Controls.Add(gamePanel);

            // Labels (black text)
            lblScore = new Label
            {
                Location = new Point(gamePanel.Right + 20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.Black,
                Text = "Score: 0"
            };

            lblHighScore = new Label
            {
                Location = new Point(gamePanel.Right + 20, 55),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.Black,
                Text = "High Score: 0"
            };

            lblState = new Label
            {
                Location = new Point(gamePanel.Right + 20, 90),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.Black,
                Text = "State: Ready"
            };

            // Buttons with rounded corners, colored, white text
            btnStart = CreateSimpleButton("Start", Color.FromArgb(60, 179, 113), new Point(gamePanel.Right + 20, 130));
            btnPause = CreateSimpleButton("Pause", Color.FromArgb(255, 165, 0), new Point(gamePanel.Right + 20, 180), false);
            btnRestart = CreateSimpleButton("Restart", Color.FromArgb(220, 20, 60), new Point(gamePanel.Right + 20, 230), false);

            btnStart.Click += BtnStart_Click;
            btnPause.Click += BtnPause_Click;
            btnRestart.Click += BtnRestart_Click;

            this.Controls.Add(lblScore);
            this.Controls.Add(lblHighScore);
            this.Controls.Add(lblState);
            this.Controls.Add(btnStart);
            this.Controls.Add(btnPause);
            this.Controls.Add(btnRestart);

            // Timer
            gameTimer = new Timer();
            gameTimer.Interval = 120;
            gameTimer.Tick += GameTimer_Tick;

            // Key events
            this.KeyPreview = true;
            this.KeyDown += GameForm_KeyDown;
        }

        // Helper: simple rounded button with white text
        private Button CreateSimpleButton(string text, Color bgColor, Point location, bool enabled = true)
        {
            Button btn = new Button
            {
                Text = text,
                Location = location,
                Width = 150,
                Height = 40,
                Enabled = enabled,
                BackColor = bgColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Padding = new Padding(5)
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bgColor);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(bgColor);

            // Rounded corners
            btn.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 15, 15));

            return btn;
        }

        // Rounded rectangle helper
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse
        );


        private void InitGame()
        {
            // Initial snake: 3 segments in center going right
            snake = new List<Point>();
            int startX = cols / 2;
            int startY = rows / 2;
            snake.Add(new Point(startX, startY));
            snake.Add(new Point(startX - 1, startY));
            snake.Add(new Point(startX - 2, startY));

            currentDirection = Direction.Right;
            nextDirection = Direction.Right;
            score = 0;
            isRunning = false;
            isPaused = false;

            SpawnFood();
            UpdateLabels();
            gamePanel.Invalidate();
            lblState.Text = "State: Ready (Press Start)";
        }

        private void StartGame()
        {
            isRunning = true;
            isPaused = false;
            btnStart.Enabled = false;
            btnPause.Enabled = true;
            btnRestart.Enabled = true;
            currentDirection = nextDirection = Direction.Right;
            gameTimer.Start();
            lblState.Text = "State: Running";
        }

        private void PauseGame()
        {
            if (!isRunning) return;
            isPaused = !isPaused;
            if (isPaused)
            {
                gameTimer.Stop();
                lblState.Text = "State: Paused";
                btnPause.Text = "Resume";
            }
            else
            {
                gameTimer.Start();
                lblState.Text = "State: Running";
                btnPause.Text = "Pause";
            }
        }

        private void EndGame()
        {
            gameTimer.Stop();
            isRunning = false;
            btnStart.Enabled = true;
            btnPause.Enabled = false;
            btnRestart.Enabled = true;
            lblState.Text = "State: Game Over";
            // update session high score
            if (score > Program.HighScore) Program.HighScore = score;
            MessageBox.Show($"Game Over!\nYour score: {score}", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
            UpdateLabels();
        }

        private void SpawnFood()
        {
            // find empty cell randomly
            Point p;
            do
            {
                p = new Point(rnd.Next(0, cols), rnd.Next(0, rows));
            } while (snake.Contains(p));
            food = p;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // update direction first (prevents instant reversal)
            if (IsReverse(nextDirection, currentDirection))
            {
                // ignore reverse command
                nextDirection = currentDirection;
            }
            currentDirection = nextDirection;

            MoveSnake();
            gamePanel.Invalidate();
        }

        private bool IsReverse(Direction a, Direction b)
        {
            return (a == Direction.Up && b == Direction.Down) ||
                   (a == Direction.Down && b == Direction.Up) ||
                   (a == Direction.Left && b == Direction.Right) ||
                   (a == Direction.Right && b == Direction.Left);
        }

        private void MoveSnake()
        {
            if (currentDirection == Direction.None) return;

            Point head = snake[0];
            Point newHead = head;
            switch (currentDirection)
            {
                case Direction.Up: newHead = new Point(head.X, head.Y - 1); break;
                case Direction.Down: newHead = new Point(head.X, head.Y + 1); break;
                case Direction.Left: newHead = new Point(head.X - 1, head.Y); break;
                case Direction.Right: newHead = new Point(head.X + 1, head.Y); break;
            }

            // Collision with walls
            if (newHead.X < 0 || newHead.X >= cols || newHead.Y < 0 || newHead.Y >= rows)
            {
                EndGame();
                return;
            }

            // Collision with self (if newHead equals any segment)
            if (snake.Contains(newHead))
            {
                EndGame();
                return;
            }

            // Move (if food eaten, grow; else move by removing tail)
            snake.Insert(0, newHead);
            if (newHead == food)
            {
                score += 10;
                SpawnFood();
                UpdateLabels();
            }
            else
            {
                // remove tail
                snake.RemoveAt(snake.Count - 1);
            }
        }

        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            // draw grid (optional thin lines)
            using (Pen gridPen = new Pen(Color.FromArgb(40, 40, 40)))
            {
                for (int x = 0; x <= cols; x++)
                    g.DrawLine(gridPen, x * cellSize, 0, x * cellSize, rows * cellSize);
                for (int y = 0; y <= rows; y++)
                    g.DrawLine(gridPen, 0, y * cellSize, cols * cellSize, y * cellSize);
            }

            // draw snake
            for (int i = 0; i < snake.Count; i++)
            {
                Rectangle rect = new Rectangle(snake[i].X * cellSize + 1, snake[i].Y * cellSize + 1, cellSize - 2, cellSize - 2);
                if (i == 0)
                    g.FillRectangle(Brushes.GreenYellow, rect); // head
                else
                    g.FillRectangle(Brushes.Green, rect); // body
            }

            // draw food
            Rectangle foodRect = new Rectangle(food.X * cellSize + 2, food.Y * cellSize + 2, cellSize - 4, cellSize - 4);
            g.FillEllipse(Brushes.Red, foodRect);
        }

        private void UpdateLabels()
        {
            lblScore.Text = $"Score: {score}";
            lblHighScore.Text = $"High Score (session): {Program.HighScore}";
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;           // <--- ADD THIS
    e.SuppressKeyPress = true;  // <--- ADD THIS

            // arrow keys control direction
            if (!isRunning && e.KeyCode != Keys.Enter) return;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (currentDirection != Direction.Down) nextDirection = Direction.Up;
                    break;
                case Keys.Down:
                    if (currentDirection != Direction.Up) nextDirection = Direction.Down;
                    break;
                case Keys.Left:
                    if (currentDirection != Direction.Right) nextDirection = Direction.Left;
                    break;
                case Keys.Right:
                    if (currentDirection != Direction.Left) nextDirection = Direction.Right;
                    break;
                case Keys.P: // quick pause
                    if (isRunning) PauseGame();
                    break;
                case Keys.Enter:
                    if (!isRunning) StartGame();
                    break;
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (!isRunning) StartGame();
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            PauseGame();
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            gameTimer.Stop();
            InitGame();
            btnStart.Enabled = true;
            btnPause.Enabled = false;
            btnRestart.Enabled = false;
        }
    }
}
