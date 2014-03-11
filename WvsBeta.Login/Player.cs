using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Login
{

    public class Player
    {
        public string Username { get; set; }
        public int ID { get; set; }
        public byte Gender { get; set; }
        public bool Admin { get; set; }
        public bool LoggedOn { get; set; }
        public int State { get; set; }
        public byte World { get; set; }
        public byte Channel { get; set; }
        public ClientSocket Socket { get; set; }
        public string SessionHash { get; set; }

        public List<Character> Characters { get; set; }

        public bool ContainsCharacter(int id)
        {
            foreach (Character character in Characters) {
                if (character.mID == id) return true;
            }
            return false;
        }

        public Player()
        {
            Admin = false;
            LoggedOn = false;
            State = 0;
        }
    }
}
