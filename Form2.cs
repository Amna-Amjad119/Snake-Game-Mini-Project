using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SnakeGame;


namespace SnakeGame
{
    public partial class instScreen : Form
    {
        public instScreen()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            this.Hide();     // Instruction screen hide

            GameForm frm = new GameForm();

            frm.FormClosed += (s, args) => this.Show();  

            frm.Show();      
        }


        private void instScreen_Load(object sender, EventArgs e)
        {

        }
    }
}
