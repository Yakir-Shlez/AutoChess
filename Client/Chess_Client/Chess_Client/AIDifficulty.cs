using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chess_Client
{
    public partial class AIDifficulty : Form
    {
        public int difficulty;
        public AIDifficulty()
        {
            InitializeComponent();
        }

        private void confirmButton_Click(object sender, EventArgs e)
        {
            difficulty = (int)difficultyUpDown.Value;
            this.Close();
        }
    }
}
