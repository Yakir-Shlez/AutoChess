using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Chess_Client
{
    public partial class Settings : Form
    {
        Socket mainServerSocket;
        Player myProfile;
        AutoChess mainForm;
        Button[] allButtons;
        string curentChange;
        bool promotion;

        public Settings(Player myProfile, Socket mainServerSocket, AutoChess mainForm)
            : this(myProfile, mainServerSocket, mainForm, false, PieceColor.Black)
        { }
        public Settings(Player myProfile, Socket mainServerSocket, AutoChess mainForm, bool promotion, PieceColor color)
        {
            InitializeComponent();
            this.myProfile = myProfile;
            this.mainServerSocket = mainServerSocket;
            this.mainForm = mainForm;
            this.allButtons = new Button[] { nicknameCh, PasswordCh, profileCh, ratingdeltaCh };
            this.promotion = promotion;
            if (promotion == true)
            {
                profileChangeTitle.Text = "Choose piece:";
                if(color == PieceColor.White)
                {
                    piecePicture1.Image = Properties.Resources.White_Queen_Piece;
                    piecePicture2.Image = Properties.Resources.White_Rook_Piece;
                    piecePicture3.Image = Properties.Resources.White_Bishop_Piece;
                    piecePicture4.Image = Properties.Resources.White_Knight_Piece;
                }
                else
                {
                    piecePicture1.Image = Properties.Resources.Black_Queen_Piece;
                    piecePicture2.Image = Properties.Resources.Black_Rook_Piece;
                    piecePicture3.Image = Properties.Resources.Black_Bishop_Piece;
                    piecePicture4.Image = Properties.Resources.Black_Knight_Piece;
                }
                piecePicture0.Image = null;
                piecePicture5.Image = null;
                piecePicture6.Image = null;
                piecePicture7.Image = null;
                piecePicture8.Image = null;
                piecePicture9.Image = null;
                piecePicture10.Image = null;
                piecePicture11.Image = null;
                DisplayOnePanel(profileChPanel);
            }
        }

        private void DisplayOnePanel(Panel panel)
        {
            for (int i = 0; i < allButtons.Length; i++)
                allButtons[i].Visible = false;
            panel.Location = new Point(0, 0);
            this.Size = new Size(panel.Size.Width + 20, panel.Size.Height + 40);
            panel.Visible = true;
        }
        private int SendServer(string msgToServer)
        {
            byte[] msg;
            int bytesSent;

            // Encode the data string into a byte array.  
            msg = Encoding.ASCII.GetBytes(msgToServer);

            // Send the data through the socket.  
            bytesSent = mainServerSocket.Send(msg);

            return bytesSent;
        }
        private int SendServerCommand(string cmd)
        {
            string line = string.Copy(cmd);
            for (int i = line.Length; i < 10; i++)
                line += "_";
            return SendServer(line);
        }
        private string RecvServer()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];
            int bytesRec;
            bytesRec = mainServerSocket.Receive(bytes);
            if (mainForm.config_File.Testing == true)
            {
                using (StreamWriter writer = new StreamWriter(mainForm.testingFile, true))
                {
                    writer.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + "recv: " + Encoding.ASCII.GetString(bytes, 0, bytesRec));
                }
            }
            return Encoding.ASCII.GetString(bytes, 0, bytesRec);
        }

        private void nicknameCh_Click(object sender, EventArgs e)
        {
            DisplayOnePanel(changePanel);
            changeTitle.Text = "Enter new nickname";
            curentChange = "nickname";
        }

        private async void change_Click(object sender, EventArgs e)
        {
            change.Enabled = false;
            if (curentChange == "ratingDelt" && int.TryParse(changeText.Text, out int value) == false)
            {
                MessageBox.Show("rating delta must be an integer");
                return;
            }
            if (changeText.Text.Contains("/"))
            {
                MessageBox.Show("No '/'");
                return;
            }
            mainForm.EnableDisableInviteTime(false);
            SendServerCommand("update");
            SendServerCommand(curentChange);
            SendServer(changeText.Text);
            string msg = await Task.Run(() => RecvServer());
            if (msg == "Ack__")
            {
                if (curentChange == "password")
                    myProfile.password = changeText.Text;
                await Task.Run(() => mainForm.InitProfile(true));
                mainForm.Show();
                mainForm.EnableDisableInviteTime(true);
                this.Close();
            }
            else
            {
                if (curentChange == "nickname")
                    MessageBox.Show("Nickname taken");
                else
                    MessageBox.Show("Change denied");
                change.Enabled = true;
            }
        }

        private void PasswordCh_Click(object sender, EventArgs e)
        {
            DisplayOnePanel(changePanel);
            changeTitle.Text = "Enter new password";
            curentChange = "password";
        }

        private void ratingdeltaCh_Click(object sender, EventArgs e)
        {
            DisplayOnePanel(changePanel);
            changeTitle.Text = "Enter new rating delta";
            curentChange = "ratingdelt";
        }

        private void profileCh_Click(object sender, EventArgs e)
        {
            piecePicture0.Image = Properties.Resources.White_King_Piece;
            piecePicture1.Image = Properties.Resources.White_Queen_Piece;
            piecePicture2.Image = Properties.Resources.White_Rook_Piece;
            piecePicture3.Image = Properties.Resources.White_Bishop_Piece;
            piecePicture4.Image = Properties.Resources.White_Knight_Piece;
            piecePicture5.Image = Properties.Resources.White_Pawn_Piece;
            piecePicture6.Image = Properties.Resources.Black_King_Piece;
            piecePicture7.Image = Properties.Resources.Black_Queen_Piece;
            piecePicture8.Image = Properties.Resources.Black_Rook_Piece;
            piecePicture9.Image = Properties.Resources.Black_Bishop_Piece;
            piecePicture10.Image = Properties.Resources.Black_Knight_Piece;
            piecePicture11.Image = Properties.Resources.Black_Pawn_Piece;
            DisplayOnePanel(profileChPanel);
        }

        private async void ChangeProfile(string pic)
        {
            this.Hide();
            mainForm.EnableDisableInviteTime(false);
            SendServerCommand("update");
            SendServerCommand("profile");
            SendServer(pic);
            string msg = await Task.Run(() => RecvServer());
            if (msg == "Ack__")
            {
                await Task.Run(() => mainForm.InitProfile(true));
                mainForm.Show();
                mainForm.EnableDisableInviteTime(true);
                this.Close();
            }
            else
                MessageBox.Show("Change denied");
        }

        private void piecePicture0_Click(object sender, EventArgs e)
        {
            ChangeProfile("0");
        }

        private void piecePicture1_Click(object sender, EventArgs e)
        {
            if (promotion == true)
            {
                mainForm.promotionPieceType = PieceType.Queen;
                mainForm.promotoeFlag = true;
                mainForm.Show();
                this.Close();
                return;
            }
            ChangeProfile("1");
        }

        private void piecePicture2_Click(object sender, EventArgs e)
        {
            if (promotion == true)
            {
                mainForm.promotionPieceType = PieceType.Rook;
                mainForm.promotoeFlag = true;
                mainForm.Show();
                this.Close();
                return;
            }
            ChangeProfile("2");
        }

        private void piecePicture3_Click(object sender, EventArgs e)
        {
            if (promotion == true)
            {
                mainForm.promotionPieceType = PieceType.Bishop;
                mainForm.promotoeFlag = true;
                mainForm.Show();
                this.Close();
                return;
            }
            ChangeProfile("3");
        }

        private void piecePicture4_Click(object sender, EventArgs e)
        {
            if (promotion == true)
            {
                mainForm.promotionPieceType = PieceType.Knight;
                mainForm.promotoeFlag = true;
                mainForm.Show();
                this.Close();
                return;
            }
            ChangeProfile("4");
        }

        private void piecePicture5_Click(object sender, EventArgs e)
        {
            ChangeProfile("5");
        }

        private void piecePicture6_Click(object sender, EventArgs e)
        {
            ChangeProfile("6");
        }

        private void piecePicture7_Click(object sender, EventArgs e)
        {
            ChangeProfile("7");
        }

        private void piecePicture8_Click(object sender, EventArgs e)
        {
            ChangeProfile("8");
        }

        private void piecePicture9_Click(object sender, EventArgs e)
        {
            ChangeProfile("9");
        }

        private void piecePicture10_Click(object sender, EventArgs e)
        {
            ChangeProfile("10");
        }

        private void piecePicture11_Click(object sender, EventArgs e)
        {
            ChangeProfile("11");
        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            mainForm.Show();
        }
    }
}
