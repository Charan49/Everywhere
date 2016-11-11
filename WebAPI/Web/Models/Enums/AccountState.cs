using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models.Enums
{    
    public enum AccountState : byte
    {
        //Less than 10 are Account Unavailable Codes i.e. Deleted, Disabled etc
        NotAvailable = 0,   //Not for use
        Deleted = 1,
        Disabled = 2,

        //10 - 20 are Account Restricted Codes i.e. Email Unconfirmed etc
        Restricted = 10, //Not for use
        UnconfirmedEmail = 11,

        //20 and above are Account in Good Standing
        Available = 20,   //Not for use
        Active = 21
    }
    
}