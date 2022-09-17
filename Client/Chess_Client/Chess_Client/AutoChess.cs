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
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace Chess_Client
{
    public partial class AutoChess : Form
    {
        Bitmap[] allProfiles;
        Settings set;
        Socket mainServerSocket;
        Player myProfile;
        Panel[] allPanels;
        CancellationTokenSource cancelGame;
        TimeSpan gameTime = new TimeSpan(0, 30, 0);
        Move currentMove;
        bool drawOffer = false;
        bool gameOpMoveRec = false;
        public bool promotoeFlag = false;
        bool promotionFlagWait = false;
        public PieceType promotionPieceType;
        bool freindInvited;
        string recvLine;
        string testingLogPath = @"Logs";
        public string testingFile;
        ChessAI secondAi = null;
        string testingAILogPath = @"AILogs";
        ChessBoard currentGameBoard;
        public bool offlineGame;
        ChessAI offlineChessAI;
        public Config config_File;
        bool serverOnline;
        Random rand;
        AutoChessBoard boardClient;
        bool moveCandidate;

        public AutoChess()
        {
            InitializeComponent();
            //TBD open DB class
            boardClient = null;
            allPanels = new Panel[allPanelsTabControl.TabPages.Count];
            for(int i= 0; i< allPanelsTabControl.TabPages.Count; i++)
                allPanels[i] = (Panel)allPanelsTabControl.TabPages[i].Controls[0];
            allProfiles = new Bitmap[] { Properties.Resources.White_King_Piece,
                Properties.Resources.White_Queen_Piece,
                Properties.Resources.White_Rook_Piece,
                Properties.Resources.White_Bishop_Piece,
                Properties.Resources.White_Knight_Piece,
                Properties.Resources.White_Pawn_Piece,
                Properties.Resources.Black_King_Piece,
                Properties.Resources.Black_Queen_Piece,
                Properties.Resources.Black_Rook_Piece,
                Properties.Resources.Black_Bishop_Piece,
                Properties.Resources.Black_Knight_Piece,
                Properties.Resources.Black_Pawn_Piece};
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(Config));
            using (StreamReader reader = new StreamReader("Config.xml"))
                //x.Serialize(writer, config_File);
                config_File = (Config)x.Deserialize(reader);

            this.rand = new Random();

            HideAllPanels();
            DisplayOnePanel(connectingToServerPanel);

            //TBD
            /*
            Player op = new Player("AutoChess AI", "", "", "1", 1500, 300);
            /*
            if (serverOnline == false)
            {
                myProfile = new Player("My profile", "", "", "2", 1500, 300);
                UpdateProfileGUI();
            }

            offlineGame = true;
            int white;

            white = rand.NextDouble() >= 0.5 ? 1 : 0;

            if (config_File.Testing == true && config_File.Testing_AI && (config_File.Testing_AI_Side == 0 || config_File.Testing_AI_Side == 1))
                white = config_File.Testing_AI_Side;

            if (config_File.Testing == true && config_File.Testing_Ai_Vs_Ai == true)
                white = 0;

            AIDifficulty AIDifficultyForm = new AIDifficulty();
            AIDifficultyForm.ShowDialog();
            int chessAIDeifficulty = AIDifficultyForm.difficulty;
            */
            /*
            InitGame(op, 1);
            currentGameBoard.ExecuteGameMove(new Move("g1-h3"));
            currentGameBoard.ExecuteGameMove(new Move("b8-c6"));
            currentGameBoard.ExecuteGameMove(new Move("d2-d4"));
            currentGameBoard.ExecuteGameMove(new Move("c6-d4"));
            currentGameBoard.ExecuteGameMove(new Move("e2-e3"));
            currentGameBoard.ExecuteGameMove(new Move("d4-c2"));
            currentGameBoard.ExecuteGameMove(new Move("d1-c2"));
            currentGameBoard.ExecuteGameMove(new Move("g8-f6"));
            currentGameBoard.ExecuteGameMove(new Move("e3-e4"));
            currentGameBoard.ExecuteGameMove(new Move("f6-e4"));
            currentGameBoard.ExecuteGameMove(new Move("c2-e4"));
            currentGameBoard.ExecuteGameMove(new Move("d7-d5"));
            currentGameBoard.ExecuteGameMove(new Move("f1-b5"));
            currentGameBoard.ExecuteGameMove(new Move("c8-d7"));
            currentGameBoard.ExecuteGameMove(new Move("e4-a4"));
            List<List<Move>> allMovesToSend = PathFindingAlg.GetShortestPath(currentGameBoard.Copy(), new Move("b5-h1"), null, null);
            //TBD
            */
            if (config_File.Testing == false || config_File.TestingWithoutServer == false)
                ConnectServer();
            else
            {
                ConnectCallback(null);
            }
            //this.Show();
            /*
            Thread.Sleep(1000);
            if (ConnectServer())
                myProfile = new Player();
            else
            {
                logInButton.Enabled = false;
                registerCmd.Enabled = false;
            }
            DisplayOnePanel(logInPanel);
            set = null;
            offlineGame = false;
            if (testing)
            {
                testingFile = testingLogPath + @"\Chess_Client_Log" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".txt";
                File.Create(testingFile).Dispose();
                AIData.Visible = true;

                if(testingAI)
                {
                    testingAIMoves = new List<Move>();
                    /*
                    testingAIMoves.Add(new Move("d2-d3"));
                    testingAIMoves.Add(new Move("b8-c5"));
                    testingAIMoves.Add(new Move("b7-b6"));
                    testingAIMoves.Add(new Move("e2-b7"));
                    testingAIMoves.Add(new Move("h1-h2"));
                    testingAIMoves.Add(new Move("g1-b4"));
                    testingAIMoves.Add(new Move("a7-g2"));
                    testingAIMoves.Add(new Move("b7-g5"));
                    */
            //     }
            // }



            /*
            myProfile = new Player("","","");
            Player op = new Player("", "", "");
            InitGame(op, 1);

            //enPassantCol = 3;

            int first = currentGameBoard.GetHashCode();
            currentGameBoard.ExecuteGameMove(new Move("g1-f3"));
            int second = currentGameBoard.GetHashCode();
            MessageBox.Show((first == second).ToString() + " " + first.ToString() + " " + second.ToString());
            currentGameBoard.ExecuteGameMove(new Move("f3-g1"));
            int third = currentGameBoard.GetHashCode();
            MessageBox.Show((first == third).ToString() + " " + first.ToString() + " " + third.ToString());
            /*
            currentGameBoard.ExecuteGameMove(new Move("g1-f3"));

            currentGameBoard.ExecuteGameMove(new Move("f3-g1"));
            int third = currentGameBoard.GetHashCode();

            MessageBox.Show((first == third).ToString() + " " + first.ToString() + " " + third.ToString());
            //currentGameBoard.ExecuteGameMove(new Move("a1-d5"));
            //currentGameBoard.ExecuteGameMove(new Move("a7-a6"));
            //currentGameBoard.ExecuteGameMove(new Move("d8-f2"));
            //ExecuteGameMove("a2-b6");
            //ExecuteGameMove("a1-d1");
            //ExecuteGameMove("d2-f4");
            //ExecuteGameMove("c1-f5");
            //ExecuteGameMove("c7-b3");
            //ExecuteGameMove("f8-a5");
            //ExecuteGameMove("d1-a5");
            //ExecuteGameMove("b1-a5");
            //ExecuteGameMove("g1-a5");
            //ExecuteGameMove("f1-a5");
            //ExecuteGameMove("c2-c5");
            //ExecuteGameMove("d7-d5");
            //ExecuteGameMove("d8-c1");
            //ExecuteGameMove("f8-b6");*/

        }

        #region server communication functions 
        private void ConnectServer()
        {
            // Establish the remote endpoint for the socket.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = IPAddress.Parse("109.66.153.61"); //TBD from file
            //IPAddress ipAddress = IPAddress.Parse("109.66.153.61");
            IPAddress ipAddress = IPAddress.Parse(config_File.Ip_Address);
            //IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];


            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 3333);

            // Create a TCP/IP  socket.  
            mainServerSocket = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // mainServerSocket.Connect(remoteEP);

            mainServerSocket.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), mainServerSocket);
            return;
        }
        private int SendServer(string msgToServer)
        {
            byte[] msg;
            int bytesSent;

            // Encode the data string into a byte array.  
            msg = Encoding.ASCII.GetBytes(msgToServer);

            // Send the data through the socket.  
            bytesSent = mainServerSocket.Send(msg);
            if (config_File.Testing == true)
                using (StreamWriter writer = new StreamWriter(testingFile, true))
                {
                    writer.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + "send: " + msgToServer);
                }

            return bytesSent;
        }
        private int SendServerCommand(string cmd)
        {
            string line = string.Copy(cmd);
            for (int i = line.Length; i < 10; i++)
                line += "_";
            return SendServer(line);
        }
        private int SendServerPlayer(Player player)
        {
            string msg = "";
            msg += player.nickname + "/";
            msg += player.username + "/";
            msg += player.password + "/";
            msg += player.profilePic + "/";
            msg += player.ratingDelta.ToString() + "/";
            return SendServer(msg);
        }
        private void RecvServerPlayer()
        {
            string line = "";
            int counter = 0;
            int pos = -1;
            while (counter < 7)
            {
                line += RecvServer();
                do
                {
                    pos = line.IndexOf('/');
                    if (pos != -1)
                    {
                        switch (counter)
                        {
                            case 0:
                                myProfile.nickname = line.Substring(0, pos);
                                break;
                            case 1:
                                myProfile.username = line.Substring(0, pos);
                                break;
                            case 2:
                                myProfile.password = line.Substring(0, pos);
                                break;
                            case 3:
                                myProfile.profilePic = line.Substring(0, pos);
                                break;
                            case 4:
                                myProfile.rating = int.Parse(line.Substring(0, pos));
                                break;
                            case 5:
                                myProfile.ratingDelta = int.Parse(line.Substring(0, pos));
                                break;
                        }
                        counter++;
                        if (counter != 6)
                            line = line.Remove(0, pos + 1);
                    }
                } while (pos != -1);

            }
        }
        private Player RecvServerOpponent(out int white)
        {
            Player rentunPlayer = new Player();
            string line = "";
            int counter = 0;
            int pos = -1;
            white = 0;
            while (counter < 4)
            {
                line += RecvServer();
                do
                {
                    pos = line.IndexOf('/');
                    if (pos != -1)
                    {
                        switch (counter)
                        {
                            case 0:
                                white = int.Parse(line.Substring(0, pos));
                                break;
                            case 1:
                                rentunPlayer.nickname = line.Substring(0, pos);
                                break;
                            case 2:
                                rentunPlayer.rating = int.Parse(line.Substring(0, pos));
                                break;
                            case 3:
                                rentunPlayer.profilePic = line.Substring(0, pos);
                                break;
                        }
                        counter++;
                        if (counter != 4)
                            line = line.Remove(0, pos + 1);
                    }
                } while (pos != -1);

            }
            return rentunPlayer;
        }
        private string RecvServer()
        {
            return RecvServer(1024);
        }
        private string RecvServer(int size)
        {
            bool savedState = GetInviteTimeEnable();
            EnableDisableInviteTime(false);
            // Data buffer for incoming data.  
            byte[] bytes = new byte[size];
            int bytesRec;
            bytesRec = mainServerSocket.Receive(bytes);
            if (config_File.Testing == true)
                using (StreamWriter writer = new StreamWriter(testingFile, true))
                {
                    writer.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + "recv: " + Encoding.ASCII.GetString(bytes, 0, bytesRec));
                }
            EnableDisableInviteTime(savedState);
            string returnString = recvLine + Encoding.ASCII.GetString(bytes, 0, bytesRec);
            recvLine = "";
            return returnString;
        }
        private void DisconnectServer()
        {
            mainServerSocket.Shutdown(SocketShutdown.Both);
            mainServerSocket.Close();
        }
        private void UpdatePlayer()
        {
            SendServerCommand("get_user");
            RecvServerPlayer();
        }
        private void UpdateRate(bool expectChange)
        {
            int lastRate = myProfile.rating;
            do
            {
                SendServerCommand("get_rate");
                string responce = RecvServer();
                myProfile.rating = int.Parse(responce);
            } while (expectChange == true && myProfile.rating == lastRate);
        }
        #endregion

        #region GUI functions
        private void ConnectCallback(IAsyncResult ar)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<IAsyncResult>(ConnectCallback), new object[] { ar });
            else
            {
                try
                {
                    if (mainServerSocket!= null && mainServerSocket.Connected)
                    {
                        serverOnline = true;
                        mainServerSocket.EndConnect(ar);
                        myProfile = new Player();
                    }
                    else
                    {
                        serverOnline = false;
                        MessageBox.Show("Server is offline");
                        logInButton.Enabled = false;
                        registerCmd.Enabled = false;
                        usernameLogIn.Enabled = false;
                        passwordLogIn.Enabled = false;
                    }
                    DisplayOnePanel(logInPanel);
                    set = null;
                    offlineGame = false;
                    if (config_File.Testing)
                    {
                        testingFile = testingLogPath + @"\Chess_Client_Log" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".txt";
                        File.Create(testingFile).Dispose();
                        AIData.Visible = true;

                        if (config_File.Testing_AI)
                        {
                            /*
                            testingAIMoves.Add(new Move("d2-d3"));
                            testingAIMoves.Add(new Move("b8-c5"));
                            testingAIMoves.Add(new Move("b7-b6"));
                            testingAIMoves.Add(new Move("e2-b7"));
                            testingAIMoves.Add(new Move("h1-h2"));
                            testingAIMoves.Add(new Move("g1-b4"));
                            testingAIMoves.Add(new Move("a7-g2"));
                            testingAIMoves.Add(new Move("b7-g5"));
                            */
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }
        private void HideAllPanels()
        {
            for (int i = 0; i < allPanels.Length; i++)
                allPanels[i].Visible = false;
        }
        private void DisplayOnePanel(Panel panel)
        {
            HideAllPanels();
            SetPanelLocation(panel, 0, 0);
            this.Size = new Size(panel.Size.Width + 20, panel.Size.Height + 40);
        }
        private void SetPanelLocation(Panel panel, int x, int y)
        {
            for (int i = 0; i < allPanelsTabControl.TabPages.Count; i++)
            {
                if (allPanelsTabControl.TabPages[i].Controls.Contains(panel) == true)
                    allPanelsTabControl.TabPages[i].Controls.Remove(panel);
            }
            this.Controls.Add(panel);
            panel.Location = new Point(x, y);
            panel.Visible = true;
        }
        public void InitProfile(bool updateProfile)
        {
            if (updateProfile)
                UpdatePlayer();
            UpdateProfileGUI();
        }
        private void UpdateProfileGUI()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(UpdateProfileGUI));
            else
            {
                profilePic.Image = allProfiles[int.Parse(myProfile.profilePic)];
                nicknameLabel.Text = myProfile.nickname;
                ratingLabel.Text = myProfile.rating.ToString();
                nickNameWel.Text = myProfile.nickname;
            }
        }
        private async void DisplayMainMenu(bool updateProfile)
        {
            EnableDisableInviteTime(true);
            gameTimer.Enabled = false;
            await Task.Run(() => InitProfile(updateProfile));
            DisplayOnePanel(profilePanel);        
            SetPanelLocation(mainMenuPanel, profilePanel.Size.Width, 0);
            this.Size = new Size(profilePanel.Size.Width + mainMenuPanel.Size.Width + 20, mainMenuPanel.Size.Height + 40);
            nickNameWel.Text = myProfile.nickname;
        }
        public void EnableDisableInviteTime(bool enable)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<bool>(EnableDisableInviteTime), new object[] { enable });
            else
            {
                inviteTimer.Enabled = enable;
            }
        }
        private bool GetInviteTimeEnable()
        {
            if (this.InvokeRequired)
                return (bool)this.Invoke(new Func<bool>(GetInviteTimeEnable));
            else
            {
                return inviteTimer.Enabled;
            }
        }
        private void DisplayGame()
        {
            DisplayOnePanel(opProfilePanel);
            this.Size = new Size(this.Size.Width, this.Size.Height + timeOpPanel.Size.Height);
            SetPanelLocation(timeOpPanel, 0, opProfilePanel.Size.Height);
            this.Size = new Size(this.Size.Width + chessGamePanel.Size.Width, this.Size.Height);
            SetPanelLocation(chessGamePanel, opProfilePanel.Size.Width, 0);
            this.Size = new Size(this.Size.Width + profilePanel.Size.Width, this.Size.Height);
            SetPanelLocation(profilePanel, chessGamePanel.Location.X + chessGamePanel.Size.Width, 0);
            SetPanelLocation(timeMePanel, profilePanel.Location.X, profilePanel.Size.Height);
            SetPanelLocation(resDraPanel, profilePanel.Location.X, timeMePanel.Location.Y + timeMePanel.Size.Height);
        }
        public void UpdateAiProgress(string progress)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string>(UpdateAiProgress), new object[] { progress });
            else
            {
                AIProgress.Text = progress;
            }
        }
        #endregion

        #region Game Functions
        private bool WaitForGame(CancellationToken ct)
        {
            while (true)
            {
                if (mainServerSocket.Poll(0, SelectMode.SelectRead))
                {
                    return true;
                }
                if (ct.IsCancellationRequested)
                {
                    SendServerCommand("cancel");
                    return false;
                }
            }
        }
        private void SquareClickHandler(PictureBox sender)
        {
            if (boardClient != null && boardClient.Connected == true)
                return;
            int rowIndex;
            int colIndex;

            if (offlineGame == true && config_File.Testing == true && config_File.Testing_Ai_Vs_Ai == true)
                return;

            if (currentGameBoard.currentTurn != GameState.MyTurn)
                return;

            Panel[,] allSquaresPanels = new Panel[,] { { panel7, panel10, panel24, panel32, panel40, panel48, panel56, panel64 }
                                                , { panel3, panel11, panel25, panel33, panel41, panel49, panel57, panel65 }
                                                , { panel14, panel9, panel23, panel31, panel999, panel47, panel55, panel63 }
                                                , { panel15, panel8, panel22, panel30, panel38, panel46, panel54, panel62 }
                                                , { panel16, panel6, panel21, panel29, panel37, panel45, panel53, panel61 }
                                                , { panel17, panel5, panel20, panel28, panel36, panel44, panel52, panel60 }
                                                , { panel18, panel4, panel13, panel27, panel35, panel43, panel51, panel59 }
                                                , { panel19, panel2, panel12, panel26, panel34, panel42, panel50, panel58 } };


            GetLabelInternalBoardIndex(sender, out rowIndex, out colIndex);
            if (currentMove.sourceRowIndex == -1 && currentMove.sourceColIndex == -1 && currentGameBoard.board[rowIndex, colIndex].color != currentGameBoard.myColor)
                return;
            if (currentMove.sourceRowIndex == -1 && currentMove.sourceColIndex == -1)
            {
                currentMove = new Move();
                moveCandidate = false;
                currentGameBoard.PrintBoardPattern(currentMove);
                //get move source square and highlight possible moves
                allSquaresPanels[currentGameBoard.FlipRowIndex(rowIndex), currentGameBoard.FlipColIndex(colIndex)].BackColor = Color.Silver; //direct print - need to flip again
                currentGameBoard.HighlightPossibleMoves(rowIndex, colIndex);
                currentMove.sourceRowIndex = rowIndex;
                currentMove.sourceColIndex = colIndex;
            }
            else if (currentMove.sourceRowIndex == rowIndex && currentMove.sourceColIndex == colIndex)
            {
                //user selected the source cell
                currentMove = new Move();
                moveCandidate = false;
                currentGameBoard.PrintBoardPattern(currentMove);
                return;
            }
            else if (allSquaresPanels[currentGameBoard.FlipRowIndex(rowIndex), currentGameBoard.FlipColIndex(colIndex)].BackColor == Color.PaleGreen)
            {
                currentMove.destRowIndex = rowIndex;
                currentMove.destColIndex = colIndex;
                if (offlineGame == false)
                    SendServer(currentMove.ToString());
                else
                {
                    OfflineGameHandler(false);
                }
            }
        }
        private void GetLabelInternalBoardIndex(PictureBox sender, out int rowIndex, out int colIndex)
        {
            PictureBox[,] allSquares = new PictureBox[,] { { U0L0, U0L1, U0L2, U0L3, U0L4, U0L5, U0L6, U0L7 }
                                                , { U1L0, U1L1, U1L2, U1L3, U1L4, U1L5, U1L6, U1L7 }
                                                , { U2L0, U2L1, U2L2, U2L3, U2L4, U2L5, U2L6, U2L7 }
                                                , { U3L0, U3L1, U3L2, U3L3, U3L4, U3L5, U3L6, U3L7 }
                                                , { U4L0, U4L1, U4L2, U4L3, U4L4, U4L5, U4L6, U4L7 }
                                                , { U5L0, U5L1, U5L2, U5L3, U5L4, U5L5, U5L6, U5L7 }
                                                , { U6L0, U6L1, U6L2, U6L3, U6L4, U6L5, U6L6, U6L7 }
                                                , { U7L0, U7L1, U7L2, U7L3, U7L4, U7L5, U7L6, U7L7 } };
            rowIndex = -1;
            colIndex = -1;
            for (int i = 0; i < allSquares.GetLength(0); i++)
                for (int j = 0; j < allSquares.GetLength(1); j++)
                    if (allSquares[i, j] == sender)
                    {
                        rowIndex = i;
                        colIndex = j;
                        break;
                    }
            rowIndex = currentGameBoard.FlipRowIndex(rowIndex);
            colIndex = currentGameBoard.FlipColIndex(colIndex);
        }
        private async void OfflineGameHandler(bool dontExecuteMove)
        {
            if (dontExecuteMove == false)
            {
                if (config_File.Testing == true)
                    using (StreamWriter writer = new StreamWriter(testingFile, true))
                    {
                        writer.WriteLine("Execute player move: " + currentMove.ToString());
                    }
                currentGameBoard.ExecuteGameMove(currentMove);

                await Task.Run(() => Thread.Sleep(100));

                if ((currentGameBoard.myColor == PieceColor.White && currentMove.destRowIndex == 7
                    && currentGameBoard.board[currentMove.destRowIndex, currentMove.destColIndex].type == PieceType.Pawn) ||
                    (currentGameBoard.myColor == PieceColor.Black && currentMove.destRowIndex == 0
                    && currentGameBoard.board[currentMove.destRowIndex, currentMove.destColIndex].type == PieceType.Pawn))
                {
                    Settings set = new Settings(myProfile, mainServerSocket, this, true, currentGameBoard.myColor);
                    set.Show();
                    this.Hide();
                    return;
                }
            }

            currentMove = new Move();
            moveCandidate = false;
            bool found = false;
            found = CheckAvailableMove(currentGameBoard.opColor);

            if (found == false)
            {
                MessageBox.Show("You won, Congratulations :D");
                if (config_File.Testing == true)
                    using (StreamWriter writer = new StreamWriter(testingFile, true))
                    {
                        writer.WriteLine("Player won");
                    }
                currentGameBoard = null;
                offlineGame = false;
                gameTimer.Enabled = false;
                if (serverOnline == true)
                    DisplayMainMenu(false);
                else
                    DisplayOnePanel(logInPanel);
                return;
            }
            currentGameBoard.currentTurn = GameState.OpTurn;
            Move AIMove = await Task.Run(() => offlineChessAI.PlayMove());
            if (AIMove.ToString() == new Move().ToString())
            {
                MessageBox.Show("AI resigned congratulations!");
                if (config_File.Testing == true)
                    using (StreamWriter writer = new StreamWriter(testingFile, true))
                    {
                        writer.WriteLine("Player won");
                    }
                currentGameBoard = null;
                offlineGame = false;
                gameTimer.Enabled = false;
                if (serverOnline == true)
                    DisplayMainMenu(false);
                else
                    DisplayOnePanel(logInPanel);
                return;
            }

            ChessBoard tempBoard = currentGameBoard.Copy();
            currentGameBoard.ExecuteGameMove(AIMove);
            await Task.Run(() => Thread.Sleep(100));

            if (boardClient != null && boardClient.Connected == true)
            {
                boardClient.SendBoardMove(AIMove, tempBoard);
            }
            

            if (config_File.Testing == true)
                using (StreamWriter writer = new StreamWriter(testingFile, true))
                {
                    writer.WriteLine("Execute AI move: " + AIMove.ToString());
                }

            if ((currentGameBoard.opColor == PieceColor.White && AIMove.destRowIndex == 7
                && currentGameBoard.board[AIMove.destRowIndex, AIMove.destColIndex].type == PieceType.Pawn) ||
                (currentGameBoard.opColor == PieceColor.Black && AIMove.destRowIndex == 0
                && currentGameBoard.board[AIMove.destRowIndex, AIMove.destColIndex].type == PieceType.Pawn))
            {
                if (config_File.Testing == true)
                    using (StreamWriter writer = new StreamWriter(testingFile, true))
                    {
                        writer.WriteLine("Promote AI: Queen");
                    }
                if (boardClient != null && boardClient.Connected == true)
                {
                    boardClient.SendBoardPromo(new Piece(PieceType.Queen, currentGameBoard.opColor), AIMove, currentGameBoard.Copy());
                }
                currentGameBoard.PromoteOp(new Piece(PieceType.Queen, currentGameBoard.opColor));
            }
            currentGameBoard.currentTurn = GameState.MyTurn;
            found = CheckAvailableMove(currentGameBoard.myColor);

            if (found == false)
            {
                MessageBox.Show("You lost, sorry :(");
                if (config_File.Testing == true)
                    using (StreamWriter writer = new StreamWriter(testingFile, true))
                    {
                        writer.WriteLine("Player lost");
                    }
                currentGameBoard = null;
                offlineGame = false;
                gameTimer.Enabled = false;
                if (serverOnline == true)
                    DisplayMainMenu(false);
                else
                    DisplayOnePanel(logInPanel);
                return;
            }
            if (config_File.Testing == true && config_File.Testing_Ai_Vs_Ai == true)
            {
                Move secAIMove = await Task.Run(() => secondAi.PlayMove());
                if (secAIMove.ToString() == new Move().ToString())
                {
                    MessageBox.Show("second AI resigned");
                    return;
                }

                currentGameBoard.ExecuteGameMove(secAIMove);
                if (config_File.Testing == true)
                    using (StreamWriter writer = new StreamWriter(testingFile, true))
                    {
                        writer.WriteLine("Execute second AI move: " + secAIMove.ToString());
                    }

                if ((currentGameBoard.myColor == PieceColor.White && secAIMove.destRowIndex == 7
                    && currentGameBoard.board[secAIMove.destRowIndex, secAIMove.destColIndex].type == PieceType.Pawn) ||
                    (currentGameBoard.myColor == PieceColor.Black && secAIMove.destRowIndex == 0
                    && currentGameBoard.board[secAIMove.destRowIndex, secAIMove.destColIndex].type == PieceType.Pawn))
                {
                    currentGameBoard.PromoteMe(new Piece(PieceType.Queen, currentGameBoard.myColor));
                    if (config_File.Testing == true)
                        using (StreamWriter writer = new StreamWriter(testingFile, true))
                        {
                            writer.WriteLine("Promote second AI: Queen");
                        }
                }
                OfflineGameHandler(true);
            }

            if (boardClient != null && boardClient.Connected == true)
            {
                boardClient.StartUserTurn();
            }
        }
        private bool CheckAvailableMove(PieceColor color)
        {
            bool found = false;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (currentGameBoard.board[i, j].color == color)
                    {
                        List<Move> allMoves = new List<Move>();
                        currentGameBoard.GetAllPossibleMoves(i, j, true, color, allMoves);
                        if (allMoves.Count != 0)
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (found == true)
                    break;
            }
            return found;
        }
        #endregion
        private async void logInButton_Click(object sender, EventArgs e)
        {
            if (usernameLogIn.Text.Contains("/") || passwordLogIn.Text.Contains("/"))
            {
                MessageBox.Show("No '/' in username or password");
                return;
            }
            logInButton.Enabled = false;
            registerCmd.Enabled = false;
            myProfile.username = usernameLogIn.Text;
            myProfile.password = passwordLogIn.Text;
            SendServerCommand("log_in");
            SendServer(myProfile.username + "/" + myProfile.password);
            string responce = await Task.Run(() => RecvServer(5));
            if(responce == "Ack__")
            {
                await Task.Run(() => RecvServerPlayer());
                DisplayMainMenu(false);
            }
            else
            {
                if(responce == "Denid")
                    MessageBox.Show("Wrong username or password");
                else
                    MessageBox.Show("User already connected");
                logInButton.Enabled = true;
                registerCmd.Enabled = true;
            }
        }

        private void registerCmd_Click(object sender, EventArgs e)
        {
            DisplayOnePanel(registerPanel);
        }

        private async void register_Click(object sender, EventArgs e)
        {
            if(nicknameReg.Text.Contains("/") || usernameReg.Text.Contains("/") || passwordReg.Text.Contains("/"))
            {
                MessageBox.Show("No '/' in nichname, username or password");
                return;
            }
            register.Enabled = false;
            Player newPlayer = new Player(nicknameReg.Text, usernameReg.Text, passwordReg.Text);
            SendServerCommand("register");
            SendServerPlayer(newPlayer);
            string responce = await Task.Run(() => RecvServer(5));
            if (responce == "Ack__")
            {
                myProfile = newPlayer;
                DisplayMainMenu(false);
            }
            else
            {
                responce = await Task.Run(() => RecvServer(5));
                if (responce == "usern")
                    MessageBox.Show("username taken");
                else
                    MessageBox.Show("nickname taken");
                register.Enabled = true;
            }
        }

        private void settings_Click(object sender, EventArgs e)
        {
            set = new Settings(myProfile, mainServerSocket, this);
            set.Show();
            this.Hide();
        }

        private async void playOnline_Click(object sender, EventArgs e)
        {
            EnableDisableInviteTime(false);
            SendServerCommand("startgame");
            DisplayOnePanel(searchGamePanel);
            cancelGame = new CancellationTokenSource();
            bool gameStarted = await Task.Run(() => WaitForGame(cancelGame.Token));
            if(gameStarted == false)
            {
                DisplayMainMenu(false);
                return;
            }
            else
            {
                int white = 0;
                Player op = RecvServerOpponent(out white);

                offlineGame = false;
                InitGame(op, white);
            }
        }

        private void InitGame(Player op, int white)
        {
            Panel[,] allSquaresPanels = new Panel[,] { { panel7, panel10, panel24, panel32, panel40, panel48, panel56, panel64 }
                                                , { panel3, panel11, panel25, panel33, panel41, panel49, panel57, panel65 }
                                                , { panel14, panel9, panel23, panel31, panel999, panel47, panel55, panel63 }
                                                , { panel15, panel8, panel22, panel30, panel38, panel46, panel54, panel62 }
                                                , { panel16, panel6, panel21, panel29, panel37, panel45, panel53, panel61 }
                                                , { panel17, panel5, panel20, panel28, panel36, panel44, panel52, panel60 }
                                                , { panel18, panel4, panel13, panel27, panel35, panel43, panel51, panel59 }
                                                , { panel19, panel2, panel12, panel26, panel34, panel42, panel50, panel58 } };
            PictureBox[,] allSquaresLabels = new PictureBox[,] { { U0L0, U0L1, U0L2, U0L3, U0L4, U0L5, U0L6, U0L7 }
                                                , { U1L0, U1L1, U1L2, U1L3, U1L4, U1L5, U1L6, U1L7 }
                                                , { U2L0, U2L1, U2L2, U2L3, U2L4, U2L5, U2L6, U2L7 }
                                                , { U3L0, U3L1, U3L2, U3L3, U3L4, U3L5, U3L6, U3L7 }
                                                , { U4L0, U4L1, U4L2, U4L3, U4L4, U4L5, U4L6, U4L7 }
                                                , { U5L0, U5L1, U5L2, U5L3, U5L4, U5L5, U5L6, U5L7 }
                                                , { U6L0, U6L1, U6L2, U6L3, U6L4, U6L5, U6L6, U6L7 }
                                                , { U7L0, U7L1, U7L2, U7L3, U7L4, U7L5, U7L6, U7L7 } };
            Label[] LLabels = new Label[] { L0Label, L1Label, L2Label, L3Label, L4Label, L5Label, L6Label, L7Label };
            Label[] RLabels = new Label[] { R0Label, R1Label, R2Label, R3Label, R4Label, R5Label, R6Label, R7Label };
            Label[] ULabels = new Label[] { U0Label, U1Label, U2Label, U3Label, U4Label, U5Label, U6Label, U7Label };
            Label[] DLabels = new Label[] { D0Label, D1Label, D2Label, D3Label, D4Label, D5Label, D6Label, D7Label };

            currentGameBoard = new ChessBoard((PieceColor)white, gameTime, allSquaresPanels, allSquaresLabels, LLabels, RLabels, ULabels, DLabels, config_File.Testing ? boardScoreLabel : null);
            currentGameBoard.PrintBoard(new Move());

            timeMe.Text = gameTime.ToString();
            timeOp.Text = gameTime.ToString();
            opProfilePic.Image = allProfiles[int.Parse(op.profilePic)];
            opNicknameLabel.Text = op.nickname;
            opRatingLabel.Text = op.rating.ToString();

            if(offlineGame == true)
                drawBu.Enabled = false;
            else
                drawBu.Enabled = true;

            DisplayGame();
            currentMove = new Move();
            moveCandidate = false;
            gameTimer.Enabled = true;
            drawOffer = false;
            gameOpMoveRec = false;
            promotoeFlag = false;
            promotionFlagWait = false;

            if (boardClient != null && boardClient.Connected == true)
            {
                boardClient.GameInit(currentGameBoard.Copy());
            }
        }

        private void cancelSearch_Click(object sender, EventArgs e)
        {
            cancelGame.Cancel();
        }

        #region pieces click
        private void U0L0_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U0L2_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U7L0_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U7L3_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U0L1_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U0L3_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U0L4_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U0L5_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U0L6_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U0L7_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U1L7_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U1L6_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U1L5_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U1L4_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U1L3_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U1L2_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U1L1_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U1L0_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U2L0_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U2L1_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U2L2_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U2L3_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U2L4_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U2L5_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U2L6_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U2L7_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U3L7_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U3L6_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U3L5_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U3L4_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U3L3_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U3L2_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U3L1_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U3L0_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U4L0_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U4L1_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U4L2_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U4L3_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U4L4_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U4L5_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U4L6_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U4L7_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U5L7_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U5L6_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U5L5_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U5L4_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U5L3_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U5L2_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U5L1_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U5L0_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U6L0_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U6L1_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U6L2_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U6L3_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U6L4_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U6L5_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U6L6_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U6L7_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U7L7_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U7L6_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U7L5_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U7L4_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U7L2_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        private void U7L1_Click(object sender, EventArgs e)
        {
            SquareClickHandler((PictureBox)sender);
        }
        #endregion

        private async void gameTimer_Tick(object sender, EventArgs e)
        {
            //clock
            TimeSpan addedDelta = DateTime.Now - currentGameBoard.lastMoveTime;
            if(currentGameBoard.currentTurn == GameState.MyTurn)
            {
                timeMe.Text = (currentGameBoard.myTimer - addedDelta).ToString(@"mm\:ss");
                timeOp.Text = currentGameBoard.opTimer.ToString(@"mm\:ss");
            }
            else
            {
                timeMe.Text = currentGameBoard.myTimer.ToString(@"mm\:ss");
                timeOp.Text = (currentGameBoard.opTimer - addedDelta).ToString(@"mm\:ss");
            }

            if (boardClient != null && boardClient.Connected == true && boardClient.waitForUserTurn == true)
            {
                Panel[,] allSquaresPanels = new Panel[,] { { panel7, panel10, panel24, panel32, panel40, panel48, panel56, panel64 }
                                                , { panel3, panel11, panel25, panel33, panel41, panel49, panel57, panel65 }
                                                , { panel14, panel9, panel23, panel31, panel999, panel47, panel55, panel63 }
                                                , { panel15, panel8, panel22, panel30, panel38, panel46, panel54, panel62 }
                                                , { panel16, panel6, panel21, panel29, panel37, panel45, panel53, panel61 }
                                                , { panel17, panel5, panel20, panel28, panel36, panel44, panel52, panel60 }
                                                , { panel18, panel4, panel13, panel27, panel35, panel43, panel51, panel59 }
                                                , { panel19, panel2, panel12, panel26, panel34, panel42, panel50, panel58 } };

                while (boardClient.CheckMsgFromBoard())
                {
                    string line = boardClient.GetMsgFromBoard();
                    PieceToMove square = boardClient.FlipPieceToMove(new PieceToMove(line[3].ToString() + line[4].ToString()), currentGameBoard);
                    bool appear = (line.Substring(0, 3) == "apr");
                    if (currentMove.sourceRowIndex == -1 && currentMove.sourceColIndex == -1 && appear == true)
                    {
                           MessageBox.Show("square " + line[3].ToString() + line[4].ToString() + " magically appear, ignoring...");
                        return;
                    }
                    //if (currentMove.sourceRowIndex == -1 && currentMove.sourceColIndex == -1 && currentGameBoard.board[square.rowIndex, square.colIndex].color != currentGameBoard.myColor)
                    //    return; TBD
                    if (currentMove.sourceRowIndex == -1 && currentMove.sourceColIndex == -1 && appear == false
                        && currentGameBoard.board[square.rowIndex, square.colIndex].color == currentGameBoard.myColor)
                    {
                        currentMove = new Move();
                        moveCandidate = false;
                        currentGameBoard.PrintBoardPattern(currentMove);
                        //get move source square and highlight possible moves
                        allSquaresPanels[currentGameBoard.FlipRowIndex(square.rowIndex), currentGameBoard.FlipColIndex(square.colIndex)].BackColor = Color.Silver; //direct print - need to flip again
                        currentGameBoard.HighlightPossibleMoves(square.rowIndex, square.colIndex);
                        currentMove.sourceRowIndex = square.rowIndex;
                        currentMove.sourceColIndex = square.colIndex;
                    }
                    else if (currentMove.sourceRowIndex == square.rowIndex && currentMove.sourceColIndex == square.colIndex && appear == true)
                    {
                        currentMove = new Move();
                        moveCandidate = false;
                        currentGameBoard.PrintBoardPattern(currentMove);
                    }
                    else if (allSquaresPanels[currentGameBoard.FlipRowIndex(square.rowIndex), currentGameBoard.FlipColIndex(square.colIndex)].BackColor == Color.PaleGreen 
                        && appear == true)
                    {
                        currentMove.destRowIndex = square.rowIndex;
                        currentMove.destColIndex = square.colIndex;
                        if (offlineGame == false)
                            SendServer(currentMove.ToString());
                        else
                        {
                            OfflineGameHandler(false);
                        }
                        if (boardClient != null && boardClient.Connected == true)
                        {
                            boardClient.StopUserTurn();
                            boardClient.CleanMsgsFromBoard();
                        }
                    }
                }
            }

            if (offlineGame == true)
            {
                if (promotoeFlag == true && currentGameBoard.currentTurn == GameState.MyTurn)
                {
                    currentGameBoard.PromoteMe(new Piece(promotionPieceType, currentGameBoard.myColor));
                    if (config_File.Testing == true)
                        using (StreamWriter writer = new StreamWriter(testingFile, true))
                        {
                            writer.WriteLine("Promote player: " + promotionPieceType.ToString());
                        }
                    promotoeFlag = false;
                    OfflineGameHandler(true);
                }
                return;
            }

            if (promotoeFlag == true)
            {
                if (currentGameBoard.currentTurn == GameState.MyTurn)
                {
                    if (promotionPieceType == PieceType.Queen)
                        SendServer("Queen");
                    else if (promotionPieceType == PieceType.Rook)
                        SendServer("Rook_");
                    else if (promotionPieceType == PieceType.Bishop)
                        SendServer("Bisop");
                    else if (promotionPieceType == PieceType.Knight)
                        SendServer("Knigt");
                    promotionFlagWait = true;
                    promotoeFlag = false;
                }
            }

            if (mainServerSocket.Poll(0, SelectMode.SelectRead))
            {
                string line = RecvServer();
                //MessageBox.Show("Rec: " + line);
                while (line != "")
                {
                    string exactLine = GetNextFive(ref line);
                    if (drawOffer == true)
                    {
                        if (exactLine == "Yes__")
                        {
                            MessageBox.Show("It's a Draw!");
                            DisplayMainMenu(true);
                            return;
                        }
                        else if (exactLine == "No___")
                        {
                            MessageBox.Show("Opponent denied the draw request, Fight!");
                            currentMove = new Move();
                            moveCandidate = false;
                            currentGameBoard.PrintBoardPattern(currentMove);
                            currentGameBoard.currentTurn = GameState.MyTurn;
                            drawBu.Enabled = true;
                        }
                    }
                    else if (promotoeFlag == true)
                    {
                        if (currentGameBoard.currentTurn == GameState.OpTurn)
                        {
                            Piece promoPiece = null;
                            if (exactLine == "Queen")
                                promoPiece = new Piece(PieceType.Queen, currentGameBoard.opColor);
                            else if (exactLine == "Rook_")
                                promoPiece = new Piece(PieceType.Rook, currentGameBoard.opColor);
                            else if (exactLine == "Bisop")
                                promoPiece = new Piece(PieceType.Bishop, currentGameBoard.opColor);
                            else if (exactLine == "Knigt")
                                promoPiece = new Piece(PieceType.Knight, currentGameBoard.opColor);
                            if (boardClient != null && boardClient.Connected == true)
                            {
                                boardClient.SendBoardPromo(new Piece(PieceType.Queen, currentGameBoard.opColor), null, currentGameBoard.Copy());
                            }
                            currentGameBoard.PromoteOp(promoPiece);
                            if (boardClient != null && boardClient.Connected == true)
                            {
                                int rowIndex = -1;
                                int colIndex = -1;
                                if (currentGameBoard.opColor == PieceColor.White)
                                    rowIndex = 7;
                                else
                                    rowIndex = 0;
                                for (int i = 0; i < 8; i++)
                                {
                                    if (currentGameBoard.board[rowIndex, i].type == PieceType.Pawn)
                                    {
                                        colIndex = i;
                                        break;
                                    }
                                }
                            }
                        }
                        promotoeFlag = false;
                    }
                    else if (exactLine == "Ack__")
                    {
                        if (promotionFlagWait == false)
                            currentGameBoard.ExecuteGameMove(currentMove);
                        else
                        {
                            currentGameBoard.PromoteMe(new Piece(promotionPieceType, currentGameBoard.myColor));
                            promotionPieceType = PieceType.Null;
                            promotionFlagWait = false;
                        }
                        currentMove = new Move();
                        moveCandidate = false;
                        currentGameBoard.currentTurn = GameState.OpTurn;
                        drawBu.Enabled = false;
                    }
                    else if (exactLine == "Denid")
                    {
                        MessageBox.Show("Move denied");
                        currentMove = new Move();
                        moveCandidate = false;
                        currentGameBoard.PrintBoardPattern(currentMove);
                        currentGameBoard.currentTurn = GameState.MyTurn;
                        drawBu.Enabled = true;
                    }
                    else if (exactLine == "Resin")
                    {
                        MessageBox.Show("Opponent resigen, congratulations!!!");
                        currentGameBoard = null;
                        DisplayMainMenu(true);
                        return;
                    }
                    else if (exactLine == "Win__")
                    {
                        MessageBox.Show("You won, congratulations!!!");
                        currentGameBoard = null;
                        DisplayMainMenu(true);
                        return;
                    }
                    else if (exactLine == "Lose_")
                    {
                        MessageBox.Show("You lost, sorry :(");
                        currentGameBoard = null;
                        DisplayMainMenu(true);
                        return;
                    }
                    else if (exactLine == "Draw_")
                    {
                        DialogResult dialogResult = MessageBox.Show("Draw?", "Opponent offered a draw", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.Yes)
                        {
                            MessageBox.Show("It's a Draw!");
                            currentGameBoard = null;
                            SendServer("Yes__");
                            DisplayMainMenu(true);
                            return;
                        }
                        else if (dialogResult == DialogResult.No)
                        {
                            SendServer("No___");
                            currentMove = new Move();
                            moveCandidate = false;
                            currentGameBoard.PrintBoardPattern(currentMove);
                            currentGameBoard.currentTurn = GameState.OpTurn;
                            drawBu.Enabled = false;
                        }
                    }
                    else if (exactLine == "stlmt")
                    {
                        MessageBox.Show("It's a Stalemate!");
                        currentGameBoard = null;
                        DisplayMainMenu(true);
                        return;
                    }
                    else if (exactLine == "End__")
                    {
                        if (gameOpMoveRec)
                        {
                            currentGameBoard.currentTurn = GameState.MyTurn;
                            drawBu.Enabled = true;
                        }
                        else
                            MessageBox.Show("Error End__");
                    }
                    else if (exactLine == "otime")
                    {
                        MessageBox.Show("Opponent time is over, you won, Congratulations!!!");
                        currentGameBoard = null;
                        DisplayMainMenu(true);
                        return;
                    }
                    else if (exactLine == "ytime")
                    {
                        MessageBox.Show("Your's time is up!, you lost, sorry :(");
                        currentGameBoard = null;
                        DisplayMainMenu(true);
                        return;
                    }
                    else if (exactLine == "Promo")
                    {
                        if (currentGameBoard.currentTurn == GameState.MyTurn)
                        {
                            currentGameBoard.ExecuteGameMove(currentMove);
                            Settings set = new Settings(myProfile, mainServerSocket, this, true, currentGameBoard.myColor);
                            set.Show();
                            this.Hide();
                        }
                        else
                            promotoeFlag = true;
                    }
                    else
                    {
                        ChessBoard tempBoard = currentGameBoard.Copy();
                        currentGameBoard.ExecuteGameMove(new Move(exactLine));
                        await Task.Run(() => Thread.Sleep(100));

                        if (boardClient != null && boardClient.Connected == true)
                        {
                            boardClient.SendBoardMove(new Move(exactLine), tempBoard);
                        }
                        
                        currentMove = new Move();
                        moveCandidate = false;
                        gameOpMoveRec = true;
                    }
                }
            }
        }

        private string GetNextFive(ref string line)
        {
            string returnString = line.Substring(0, 5);
            line = line.Remove(0, 5);
            return returnString;
        }

        private void drawBu_Click(object sender, EventArgs e)
        {
            if (currentGameBoard.currentTurn == GameState.MyTurn)
            {
                currentMove = new Move();
                moveCandidate = false;
                currentGameBoard.PrintBoardPattern(currentMove);
                currentGameBoard.currentTurn = GameState.OpTurn;
                drawBu.Enabled = false;
                SendServer("Draw_");
                drawOffer = true;
            }
        }

        private void ReignBu_Click(object sender, EventArgs e)
        {
            if (offlineGame == false)
            {
                gameTimer.Enabled = false;
                SendServer("Resin");
                MessageBox.Show("You resigned, sorry :(");
                currentGameBoard = null;
                DisplayMainMenu(true);
            }
            else
            {
                MessageBox.Show("You resigned, sorry :(");
                currentGameBoard = null;
                offlineGame = false;
                gameTimer.Enabled = false;
                if (serverOnline == true)
                    DisplayMainMenu(false);
                else
                    DisplayOnePanel(logInPanel);
                return;
            }
        }

        private void AutoChess_FormClosing(object sender, FormClosingEventArgs e)
        {
            //defending the server from crashing when user is disconnected after asking for an information
            //Thread.Sleep(2000);
            //if (mainServerSocket.Poll(0, SelectMode.SelectRead))
            //{
            //    RecvServer();
            //}
        }

        private void inviteTimer_Tick(object sender, EventArgs e)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[5];
            int bytesRec;
            if (mainServerSocket.Poll(0, SelectMode.SelectRead) == true)
            {
                inviteTimer.Enabled = false;
                bytesRec = mainServerSocket.Receive(bytes);
                if (config_File.Testing == true)
                    using (StreamWriter writer = new StreamWriter(testingFile, true))
                    {
                        writer.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + "timer recv: " + Encoding.ASCII.GetString(bytes, 0, bytesRec));
                    }
                if (freindInvited == true)
                {
                    if(Encoding.ASCII.GetString(bytes, 0, bytesRec) == "game_")
                    {
                        MessageBox.Show("Game started !");
                        int white = 0;
                        Player op = RecvServerOpponent(out white);
                        InitGame(op, white);
                    }
                    else
                    {
                        MessageBox.Show("Friend denied the game request :(");
                        DisplayMainMenu(false);
                        inviteTimer.Enabled = true;
                    }
                }
                else
                {
                    if (Encoding.ASCII.GetString(bytes, 0, bytesRec) == "invit")
                    {
                        recvLine = "";
                        string recv = RecvServer();
                        DialogResult result = MessageBox.Show(recv +
                            " Invited you to a game, Accept?", "Game invite", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            if (mainServerSocket.Poll(0, SelectMode.SelectRead) == true)
                            {
                                recv = RecvServer();
                                if (recv == "Cancl")
                                    MessageBox.Show("Opponent canceled the request");
                                else
                                    MessageBox.Show("Unexpected: " + recv);
                            }
                            else
                            {
                                SendServerCommand("yes");
                                recv = RecvServer(5);
                                if (recv == "game_")
                                {
                                    if (set != null && set.IsDisposed == false && set.Visible == true)
                                    {
                                        set.Close();
                                        set = null;
                                    }
                                    this.Show();

                                    MessageBox.Show("Game started !");
                                    int white = 0;
                                    Player op = RecvServerOpponent(out white);

                                    InitGame(op, white);
                                }
                                else if (recv == "Cancl")
                                    MessageBox.Show("Opponent canceled the request");
                                else
                                    MessageBox.Show("Unexpected: " + recv);
                            }
                        }
                        else
                        {
                            SendServerCommand("no");
                            recv = RecvServer();
                            if (recv == "Ack__")
                            {
                                MessageBox.Show("Denied");
                                inviteTimer.Enabled = true;
                            }
                            else
                            {
                                MessageBox.Show("Unexpected: " + recv);
                            }
                        }
                    }
                    else
                    {
                        recvLine += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        inviteTimer.Enabled = true;
                    }
                }
            }
        }

        private void PlayFreind_Click(object sender, EventArgs e)
        {
            DisplayOnePanel(inviteFriendPanel);
        }

        private async void inviteFriend_Click(object sender, EventArgs e)
        {
            EnableDisableInviteTime(false);
            inviteFriend.Enabled = false;
            inviteFriendCancel.Enabled = false;
            string friendName = friendNameTextBox.Text;
            SendServerCommand("invite");
            SendServer(friendName);
            string recv = await Task.Run(() => RecvServer());

            if(recv == "nfond")
            {
                MessageBox.Show("Friend not found or not online at the moment :(, try later");
                inviteFriend.Enabled = true;
                inviteFriendCancel.Enabled = true;
                EnableDisableInviteTime(true);
            }
            else if(recv == "busy_")
            {
                MessageBox.Show("Friend is busy at the moment :(, try later");
                inviteFriend.Enabled = true;
                inviteFriendCancel.Enabled = true;
                EnableDisableInviteTime(true);
            }
            else
            {
                DisplayOnePanel(waitingForFriendPanel);
                freindInvited = true;
                EnableDisableInviteTime(true);
            }
        }

        private void inviteFriendCancel_Click(object sender, EventArgs e)
        {
            DisplayMainMenu(false);
        }

        private async void FreindRequestCancel_Click(object sender, EventArgs e)
        {
            freindRequestCancel.Enabled = false;
            EnableDisableInviteTime(false);
            SendServerCommand("cancel");
            string recv = await Task.Run(() => RecvServer(5));
            if (recv != "Ack__")
                MessageBox.Show("Unexpected responce: " + recv);
            freindInvited = false;
            DisplayMainMenu(false);
            EnableDisableInviteTime(true);
        }

        private void playOffline_Click(object sender, EventArgs e)
        {
            Player op = new Player("AutoChess AI", "", "", "1", 1500, 300);
            if(serverOnline == false)
            {
                myProfile = new Player("My profile", "", "", "2", 1500, 300);
                UpdateProfileGUI();
            }

            offlineGame = true;
            int white;

            white = rand.NextDouble() >= 0.5 ? 1 : 0;

            if (config_File.Testing == true && config_File.Testing_AI && (config_File.Testing_AI_Side == 0 || config_File.Testing_AI_Side == 1))
                white = config_File.Testing_AI_Side;

            if (config_File.Testing == true && config_File.Testing_Ai_Vs_Ai == true)
                white = 0;

            AIDifficulty AIDifficultyForm = new AIDifficulty();
            AIDifficultyForm.ShowDialog();
            int chessAIDeifficulty = AIDifficultyForm.difficulty;

            InitGame(op, white);

            if (config_File.Testing == true)
                using (StreamWriter writer = new StreamWriter(testingFile, true))
                {
                    writer.WriteLine("Start offline game");
                }

            if (config_File.Testing && config_File.Testing_AI)
            {
                foreach (Move move in config_File.Testing_AI_Moves)
                {
                    currentGameBoard.ExecuteGameMove(move);
                    if (config_File.Testing == true)
                        using (StreamWriter writer = new StreamWriter(testingFile, true))
                        {
                            writer.WriteLine("testing AI move: " + move.ToString());
                        }
                }
            }
            offlineChessAI = new ChessAI(currentGameBoard, chessAIDeifficulty, currentGameBoard.opColor, 
                this, config_File.Testing && config_File.Testing_AI, config_File.Testing && config_File.Testing_AI_Brain, testingAILogPath);

            if (config_File.Testing == true && config_File.Testing_Ai_Vs_Ai == true)
            {
                secondAi = new ChessAI(currentGameBoard, chessAIDeifficulty, currentGameBoard.myColor,
                null, config_File.Testing && config_File.Testing_AI, config_File.Testing && config_File.Testing_AI_Brain, testingAILogPath);
            }
            if (white == 0 || (config_File.Testing == true && config_File.Testing_Ai_Vs_Ai == true))
                OfflineGameHandler(true);
            else if (boardClient != null && boardClient.Connected == true)
                boardClient.StartUserTurn();
        }

        private void boardConnectLogInBtn_Click(object sender, EventArgs e)
        {
            if (boardClient == null || boardClient.Connected == false)
            {
                boardClient = new AutoChessBoard();
                if (boardClient.Connected == true)
                {
                    boardConnectLogInBtn.BackColor = Color.LimeGreen;
                    boardConnectMainMenuBtn.BackColor = Color.LimeGreen;
                }
                else
                {
                    boardConnectLogInBtn.BackColor = Color.Salmon;
                    boardConnectMainMenuBtn.BackColor = Color.Salmon;
                    boardClient.CloseBoard();
                    boardClient = null;
                }
            }
            else
            {
                boardConnectLogInBtn.BackColor = Color.Salmon;
                boardConnectMainMenuBtn.BackColor = Color.Salmon;
                boardClient.CloseBoard();
                boardClient = null;
            }
        }
    }
}