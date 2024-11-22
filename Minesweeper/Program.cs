using System;
using System.Drawing;
using System.Windows.Forms;

public class MinesweeperForm : Form
{
    private const int GRID_SIZE = 10;
    private const int BOMB_COUNT = 10;
    private const int BUTTON_SIZE = 30;

    private Button[,] buttons;
    private bool[,] bombs;
    private int[,] numbers;
    private bool firstClick = true;

    public MinesweeperForm()
    {
        InitializeComponent();
        InitializeGame();
    }


    // Този метод отговаря за първоначалната настройка на формата и създаването на всички бутони.
    private void InitializeComponent()
    {
        this.Text = "Minesweeper";
        this.Size = new Size(GRID_SIZE * BUTTON_SIZE + 50, GRID_SIZE * BUTTON_SIZE + 80);

        buttons = new Button[GRID_SIZE, GRID_SIZE];
        bombs = new bool[GRID_SIZE, GRID_SIZE];
        numbers = new int[GRID_SIZE, GRID_SIZE];

        // Създаваме мрежа от бутони
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                Button button = new Button
                {
                    Location = new Point(j * BUTTON_SIZE + 10, i * BUTTON_SIZE + 10),
                    Size = new Size(BUTTON_SIZE, BUTTON_SIZE),
                    BackColor = Color.Aquamarine,
                    UseVisualStyleBackColor = false
                };

                // Създаваме метод за обработка на клик и използваме lambda израз за да предадем точните координати
                int row = i;
                int col = j;
                button.MouseUp += (sender, e) => Button_Click(sender, e, row, col);

                // Добавяме бутона в масива
                buttons[i, j] = button;
                this.Controls.Add(button);
            }
        }
    }


    // Mетод за "ресетване" на играта, който подготвя всичко за начало на нова игра.
    private void InitializeGame()
    {
        // Нулираме масивите за бомби и числа
        Array.Clear(bombs, 0, bombs.Length);
        Array.Clear(numbers, 0, numbers.Length);

        firstClick = true;

        // Връщаме всички бутони към началното им състояние
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                buttons[i, j].Text = "";
                buttons[i, j].Enabled = true;
                buttons[i, j].BackColor = SystemColors.Control;
            }
        }
    }


    // Този метод подрежда мините и подготвя играта за началото. Гарантира, че първият клик няма да е върху мина
    // и изчислява за всяка клетка колко съседни мини има.
    private void PlaceBombs(int firstX, int firstY)
    {
        Random rand = new Random();

        int bombsPlaced = 0;

        while (bombsPlaced < BOMB_COUNT)
        {
            int x = rand.Next(GRID_SIZE);
            int y = rand.Next(GRID_SIZE);

            // Проверяваме дали вече има мина на тази позиция и дали това не е пъроначално кликнатата клетка.
            // Ако са изпълнение двете горни условия, маркираме клетката като съдържаща мина.
            if (!bombs[x, y] && (x != firstX || y != firstY))
            {
                bombs[x, y] = true;
                bombsPlaced++;
            }
        }

        // След поставяне на мините, този вложен цикъл изчислява числата на съседните клетки
        // или по-точно казано колко мини има в съседство на клетка без мина
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                if (!bombs[i, j])
                {
                    numbers[i, j] = CountAdjacentBombs(i, j);
                }
            }
        }
    }


    // Ето я и самата логика, която се грижи да преброи колко мини има около дадена клетка
    private int CountAdjacentBombs(int x, int y)
    {
        int count = 0;

        // Обхождаме всички съседни клетки, като използваме методите Min и Max на класа Math, за да си гарантираме,
        // че няма да излезем от границите на масива
        for (int i = Math.Max(0, x - 1); i <= Math.Min(GRID_SIZE - 1, x + 1); i++)
        {
            for (int j = Math.Max(0, y - 1); j <= Math.Min(GRID_SIZE - 1, y + 1); j++)
            {
                // ако има мина, увеличаваме брояча
                if (bombs[i, j]) count++;
            }
        }

        // Връщаме броя на съседните мини
        return count;
    }


    // Този метод се грижи за разкриването на избрана клетка, както и за автоматичното разкриване 
    // на съседните клетки, ако текущата е празна (няма съседни мини)
    private void RevealCell(int x, int y)
    {
        // Провераваме дали клетката е валидна (да е в рамките на масива и да не е вече разкрита)
        if (x < 0 || x >= GRID_SIZE || y < 0 || y >= GRID_SIZE || !buttons[x, y].Enabled)
            return;

        buttons[x, y].Enabled = false;
        buttons[x, y].BackColor = Color.White;

        buttons[x, y].Enabled = false;

        // Ако текущата клетка е мина извикваме метода GameOver за прекратяване на играта като загуба
        if (bombs[x, y])
        {
            buttons[x, y].Text = "💣";
            GameOver(false);
            return;
        }
        
        // Ако текущата клетка има съседни мини, показва техния брой (1, 2, и т.н.)
        if (numbers[x, y] > 0)
        {
            buttons[x, y].Text = numbers[x, y].ToString();
        }
        else
        {
            // Използваме рекурсия за разкриване на празните съседни клетки
            for (int i = Math.Max(0, x - 1); i <= Math.Min(GRID_SIZE - 1, x + 1); i++)
            {
                for (int j = Math.Max(0, y - 1); j <= Math.Min(GRID_SIZE - 1, y + 1); j++)
                {
                    RevealCell(i, j);
                }
            }
        }

        // Проверка за победа
        CheckWin();
    }


    // Методът Button_Click е обработчик за събития, който се изпълнява, когато потребителят кликне върху даден бутон от мрежата.
    // Той се грижи за разпознаването на типа на клик (ляв или десен бутон на мишката) и извършва съответното действие:
    // разкриване на клетка или маркиране с флаг.
    private void Button_Click(object sender, MouseEventArgs e, int x, int y)
    {
        // Добавяме проверка дали координатите са в границите на масива
        if (x < 0 || x >= GRID_SIZE || y < 0 || y >= GRID_SIZE)
            return;

        Button clickedButton = sender as Button;
        if (clickedButton == null)
            return;

        // Проверяваме дали е натиснат десния бутон на мишката
        if (e.Button == MouseButtons.Right)
        {
            // Ако бутона е все още активен маркира или премахва флаг.
            if (buttons[x, y].Enabled)
            {
                if (buttons[x, y].Text == "🏴‍☠️")
                    buttons[x, y].Text = "";
                else
                    buttons[x, y].Text = "🏴‍☠️";
            }
            return;
        }

        if (buttons[x, y].Text == "🏴‍☠️")
            return;

        if (firstClick)
        {
            PlaceBombs(x, y);
            firstClick = false;
        }

        RevealCell(x, y);
    }


    // 
    private void GameOver(bool won)
    {
        // Разкрива всички мини
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                if (bombs[i, j])
                {
                    buttons[i, j].Text = "💣";
                }
            }
        }

        // Показва съобщение според резултата - победа или загуба
        string message = won 
            ? "Поздравления! Вие спечелихте!" 
            : "Играта свърши! Опитайте отново!";

        DialogResult result = MessageBox.Show(message + "\nИскате ли нова игра?", "Край на играта", MessageBoxButtons.YesNo);

        // Обработва отговора на играча
        if (result == DialogResult.Yes)
        {
            InitializeGame();
        }
        else
        {
            this.Close();
        }
    }

    // Проверка за победа
    private void CheckWin()
    {
        bool won = true;
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                if (!bombs[i, j] && buttons[i, j].Enabled)
                {
                    won = false;
                    break;
                }
            }
        }

        if (won)
        {
            GameOver(true);
        }
    }
}

public class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MinesweeperForm());
    }
}
