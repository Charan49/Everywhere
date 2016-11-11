using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models.Redis
{
    public class RApiUser
    {        
        public Guid SubjectID;      //Username GUID, Video Switch GUID etc
        public Guid DeviceID;       //UUID of the Device - in case of App

        public string ClientType;   //User, Admin or VideoSwitch
        public string Email;
        public string Name;         //First-Name
        public string LastName;
        public string GeoLocation;  //Location

        public Guid SessionID;      //Unique Session ID per Login (E.g. unique for each web and desktop/mobile client login)
        public string TokenHash;    //Hash part of the active JSON Token
    }
}