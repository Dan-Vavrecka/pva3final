using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

//Daniel Vavrečka, 3.C, PVA, Kostky

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // UI controls
        // Designer-initialized controls are declared in Form1.Designer.cs
        // private PictureBox[] pbDice; // removed
        private PictureBox[]? pbDice1;
        private PictureBox[]? pbDice2;

        // Game state
        private Random rnd = new Random();
        private int currentPlayer = 0;
        // playersCount will be total players (always 2 internally); isSinglePlayer signals CPU mode
        private int playersCount = 2;
        private bool isSinglePlayer = true;
        private int diceCount = 6;
        private int limitKol = 5;
        private int maxSkore = 10000;
        private int aktualniKolo = 0;
        private int[] celkoveSkore = new int[2];
        private int aktualniPocetBodu = 0;
        private int[] perPlayerAccumulated = new int[2];
        private bool startHry = false;

        public Form1()
        {
            InitializeComponent();
            // default values
            cbPlayers.SelectedIndex = 0;
            nudDiceCount.Value = 6;
            rbRounds.Checked = true;
            UpdateStatus("Připravit hru");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // create picture boxes only at runtime (designer cannot execute this code)
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                if (pbDice1 == null) pbDice1 = new PictureBox[6];
                if (pbDice2 == null) pbDice2 = new PictureBox[6];
                for (int i = 0; i < 6; i++)
                {
                    var pb = new PictureBox();
                    pb.BorderStyle = BorderStyle.FixedSingle;
                    pb.Location = new Point(300 + i * 90, 40);
                    pb.Size = new Size(80, 80);
                    pb.SizeMode = PictureBoxSizeMode.CenterImage;
                    pb.Name = "pbDiceP1" + i;
                    pb.Paint += pbDice_Paint;
                    this.Controls.Add(pb);
                    pbDice1[i] = pb;
                }
                for (int i = 0; i < 6; i++)
                {
                    var pb = new PictureBox();
                    pb.BorderStyle = BorderStyle.FixedSingle;
                    pb.Location = new Point(300 + i * 90, 140);
                    pb.Size = new Size(80, 80);
                    pb.SizeMode = PictureBoxSizeMode.CenterImage;
                    pb.Name = "pbDiceP2" + i;
                    pb.Paint += pbDice_Paint;
                    this.Controls.Add(pb);
                    pbDice2[i] = pb;
                }
            }
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            // okamžité ukončení hry a vynulování skóre
            startHry = false;
            btnRoll.Enabled = false;
            btnStart.Enabled = true;
            celkoveSkore = new int[2];
            perPlayerAccumulated = new int[2];
            aktualniPocetBodu = 0;
            aktualniKolo = 0;
            currentPlayer = 0;
            UpdateScoresLabel();
            UpdateStatus("Hra ukončena. Skóre vynulováno.");
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            isSinglePlayer = cbPlayers.SelectedIndex == 0;
            playersCount = 2; // vždy interně dva hráči (druhý je CPU pokud isSinglePlayer)
            diceCount = (int)nudDiceCount.Value;
            limitKol = (int)nudRounds.Value;
            maxSkore = (int)nudTarget.Value;
            currentPlayer = 0;
            aktualniKolo = 1;
            celkoveSkore = new int[2];
            aktualniPocetBodu = 0;
            startHry = true;
            btnRoll.Enabled = true;
            btnStart.Enabled = false;
            UpdateScoresLabel();
            UpdateStatus($"Hraje hráč {currentPlayer + 1}");
            RefreshDiceDisplayForPlayer(0, new int[diceCount]);
        }



        private void btnRoll_Click(object sender, EventArgs e)
        {
            if (!startHry) return;
            // proveď hod kostkami - jeden hod = konec tahu
            int[] rolls = new int[diceCount];
            for (int i = 0; i < diceCount; i++) rolls[i] = rnd.Next(1, 7);
            RefreshDiceDisplayForPlayer(currentPlayer, rolls);
            int score = CalculateScore(rolls);
            aktualniPocetBodu = score; // jeden hod
            if (score == 0)
            {
                UpdateStatus($"Hráč {currentPlayer + 1} hodil bez bodů.");
            }
            else
            {
                UpdateStatus($"Hráč {currentPlayer + 1} získal {score} bodů.");
            }
            // automaticky ukončit tah a přepnout hráče
            EndTurn();
            UpdateScoresLabel();
        }

        private void EndTurn()
        {
            // add accumulated points from this turn
            celkoveSkore[currentPlayer] += aktualniPocetBodu;
            aktualniPocetBodu = 0;

            // determine next player
            int nextPlayer = (currentPlayer + 1) % 2;

            // Check end conditions
            if (rbTarget.Checked)
            {
                if (celkoveSkore[currentPlayer] >= maxSkore)
                {
                    int winner = GetWinner();
                    string scoreText = $"Hráč 1: {celkoveSkore[0]}\nHráč 2: {celkoveSkore[1]}";
                    MessageBox.Show($"Konec hry. Vítěz: Hráč {winner + 1}\n\n{scoreText}", "Konec hry", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                    return;
                }
            }
            else
            {
                // rounds mode: if we just finished the last player's turn of the final round, end game
                if (nextPlayer == 0 && aktualniKolo >= limitKol)
                {
                    int winner = GetWinner();
                    string scoreText = $"Hráč 1: {celkoveSkore[0]}\nHráč 2: {celkoveSkore[1]}";
                    MessageBox.Show($"Konec hry. Vítěz: Hráč {winner + 1}\n\n{scoreText}", "Konec hry", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                    return;
                }
            }

            // switch to next player
            currentPlayer = nextPlayer;
            // if we've wrapped back to player 0, advance round
            if (currentPlayer == 0) aktualniKolo++;

            UpdateStatus($"Hraje hráč {currentPlayer + 1}");
            UpdateScoresLabel();

            // manage buttons
            btnRoll.Enabled = true;

            // if CPU's turn in single-player, perform CPU turn automatically
            if (isSinglePlayer && currentPlayer == 1)
            {
                DoCpuTurn();
            }
        }

        private void DoCpuTurn()
        {
            if (!startHry) return;
            int[] rolls = new int[diceCount];
            for (int i = 0; i < diceCount; i++) rolls[i] = rnd.Next(1, 7);
            int score = CalculateScore(rolls);
            aktualniPocetBodu = score;
            if (score == 0)
            {
                UpdateStatus($"CPU hodil bez bodů.");
            }
            else
            {
                UpdateStatus($"CPU získal {score} bodů.");
            }
            EndTurn();
        }

        private int GetWinner()
        {
            int best = 0;
            for (int i = 1; i < celkoveSkore.Length; i++) if (celkoveSkore[i] > celkoveSkore[best]) best = i;
            return best;
        }

        private void UpdateScoresLabel()
        {
            string s = "";
            for (int i = 0; i < playersCount; i++) s += $"Hráč {i + 1}: {celkoveSkore[i]}  ";
            lblScores.Text = s + $"| Kolo: {aktualniKolo} | Stav kola: {aktualniPocetBodu}";
        }

        private void UpdateStatus(string text)
        {
            lblStatus.Text = text;
        }

        private void RefreshDiceDisplayForPlayer(int playerIndex, int[] rolls)
        {
            var pb = playerIndex == 0 ? pbDice1 : pbDice2;
            var other = playerIndex == 0 ? pbDice2 : pbDice1;
            if (pb == null || other == null) return; // designer/runtime safety
            for (int i = 0; i < other.Length; i++) { if (other[i] != null) { other[i].Tag = null; other[i].Visible = false; other[i].Invalidate(); } }
            for (int i = 0; i < pb.Length; i++)
            {
                if (pb[i] == null) continue;
                pb[i].Visible = i < rolls.Length;
                pb[i].Tag = i < rolls.Length ? (object)rolls[i] : null;
                pb[i].Invalidate();
            }
        }

        private void pbDice_Paint(object sender, PaintEventArgs e)
        {
            var pb = sender as PictureBox;
            if (pb == null) return;
            e.Graphics.Clear(pb.BackColor);
            if (pb.Tag == null) return;
            int v = (int)pb.Tag;
            // kreslení teček
            var g = e.Graphics;
            var rect = new Rectangle(0, 0, pb.Width, pb.Height);
            g.FillRectangle(Brushes.White, rect);
            g.DrawRectangle(Pens.Black, 0, 0, pb.Width - 1, pb.Height - 1);
            var center = new Point(pb.Width / 2, pb.Height / 2);
            var r = 12;
            Brush pip = Brushes.Black;
            // pozice
            Point[] pts = new Point[] {
                new Point(center.X - 20, center.Y - 20),
                new Point(center.X + 20, center.Y - 20),
                new Point(center.X - 20, center.Y),
                new Point(center.X + 20, center.Y),
                new Point(center.X - 20, center.Y + 20),
                new Point(center.X + 20, center.Y + 20),
                center
            };
            switch (v)
            {
                case 1:
                    g.FillEllipse(pip, pts[6].X - r/2, pts[6].Y - r/2, r, r);
                    break;
                case 2:
                    g.FillEllipse(pip, pts[0].X - r/2, pts[0].Y - r/2, r, r);
                    g.FillEllipse(pip, pts[5].X - r/2, pts[5].Y - r/2, r, r);
                    break;
                case 3:
                    g.FillEllipse(pip, pts[0].X - r/2, pts[0].Y - r/2, r, r);
                    g.FillEllipse(pip, pts[6].X - r/2, pts[6].Y - r/2, r, r);
                    g.FillEllipse(pip, pts[5].X - r/2, pts[5].Y - r/2, r, r);
                    break;
                case 4:
                    g.FillEllipse(pip, pts[0].X - r/2, pts[0].Y - r/2, r, r);
                    g.FillEllipse(pip, pts[1].X - r/2, pts[1].Y - r/2, r, r);
                    g.FillEllipse(pip, pts[4].X - r/2, pts[4].Y - r/2, r, r);
                    g.FillEllipse(pip, pts[5].X - r/2, pts[5].Y - r/2, r, r);
                    break;
                case 5:
                    g.FillEllipse(pip, pts[0].X - r/2, pts[0].Y - r/2, r, r);
                    g.FillEllipse(pip, pts[1].X - r/2, pts[1].Y - r/2, r, r);
                    g.FillEllipse(pip, pts[6].X - r/2, pts[6].Y - r/2, r, r);
                    g.FillEllipse(pip, pts[4].X - r/2, pts[4].Y - r/2, r, r);
                    g.FillEllipse(pip, pts[5].X - r/2, pts[5].Y - r/2, r, r);
                    break;
                case 6:
                    g.FillEllipse(pip, pts[0].X - r/2, pts[0].Y - r/2, r, r);
                    g.FillEllipse(pip, pts[1].X - r/2, pts[1].Y - r/2, r, r);
                    g.FillEllipse(pip, pts[2].X - r/2, pts[2].Y - r/2, r, r);
                    g.FillEllipse(pip, pts[3].X - r/2, pts[3].Y - r/2, r, r);
                    g.FillEllipse(pip, pts[4].X - r/2, pts[4].Y - r/2, r, r);
                    g.FillEllipse(pip, pts[5].X - r/2, pts[5].Y - r/2, r, r);
                    break;
            }
        }

        private int CalculateScore(int[] rolls)
        {
            // pravidla
            // counts
            var counts = new int[7];
            foreach (var r in rolls) counts[r]++;
            int dice = rolls.Length;

            // straight 1-6
            bool straight = true;
            for (int i = 1; i <= 6; i++) if (counts[i] != 1) straight = false;
            if (straight && dice == 6) return 1500;

            // pairs? any three pairs not required, user said Dvojice e.g. 22,44,55 = 1000
            int pairs = 0;
            for (int i = 1; i <= 6; i++) if (counts[i] == 2) pairs++;
            if (pairs >= 3) return 1000;

            int score = 0;
            // six of a kind etc. user rules: for triple of number n: 200,300,400,500,600 for 3x (2x values for 2..6?),
            // Interpretace: three of 1s = 1000; three of 2-6 = value: 200..600
            // then increasing multiples for 4,5,6 counts: each additional same die doubles previous set
            for (int v = 1; v <= 6; v++)
            {
                int c = counts[v];
                if (c >= 3)
                {
                    if (v == 1)
                    {
                        // base 1000 for three 1s
                        int baseVal = 1000;
                        int multiplier = 1 << (c - 3); // each extra die doubles
                        score += baseVal * multiplier;
                    }
                    else
                    {
                        int baseVal = v * 100; // 2->200, 3->300 etc.
                        int multiplier = 1 << (c - 3);
                        score += baseVal * multiplier;
                    }
                }
                else
                {
                    // single 1s and 5s
                    if (v == 1) score += counts[1] * 100;
                    if (v == 5) score += counts[5] * 50;
                }
            }

            return score;
        }

        // CPU automatic play removed. CPU will only play when user clicks "Hodit kostkami" even in single-player mode.
    }
}
