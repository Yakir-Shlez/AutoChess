using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Chess_Client
{
    public class Player
    {
        public string nickname;
        public string username;
        public string password;
        public string profilePic;
        public int rating;
        public int ratingDelta;
        public Player() { }
        public Player(string nickname, string username, string password)
            : this(nickname, username, password, "0", 1500, 300) { }
        public Player(string nickname, string username, string password, string profilePic, int rating, int ratingDelta)
        {
            this.nickname = nickname;
            this.username = username;
            this.password = password;
            this.profilePic = profilePic;
            this.rating = rating;
            this.ratingDelta = ratingDelta;
        }
    }
}