using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilites;

namespace Database.DataTypes
{
    public class FollowerPropereties
    {
        public String FollowerID { get; set; }
        public String FollowerBrokerLogin { get; set; }
        public String FollowerBrokerPasswd { get; set; }
        public String FollowerEmail{get; set;}
        public Presets.SynchronizationMode FollowerSynchMode { get; set; }

        public FollowerPropereties()
        {
            
        }

        public FollowerPropereties(String followerID, String followerBrokerLogin, String followerBrokerPasswd, String followerEmail,
            Presets.SynchronizationMode followerSyncMode)
        {
            FollowerID           = followerID;
            FollowerBrokerLogin  = followerBrokerLogin;
            FollowerBrokerPasswd = followerBrokerPasswd;
            FollowerEmail        = followerEmail;
            FollowerSynchMode    = followerSyncMode;
        }
    }
}
