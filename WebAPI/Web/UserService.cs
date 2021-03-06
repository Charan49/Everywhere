//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Web
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserService
    {
        public System.Guid ServiceGUID { get; set; }
        public System.Guid UserGUID { get; set; }
        public System.Guid AccessID { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string AccessToken { get; set; }
        public string TokenExpiration { get; set; }
        public string StreamID { get; set; }
        public string StreamURL { get; set; }
        public string StreamKey { get; set; }
        public Nullable<System.DateTime> StreamDate { get; set; }
        public string PictureURL { get; set; }
        public string fbUserID { get; set; }
        public string LongToken { get; set; }
    
        public virtual Service Service { get; set; }
        public virtual User User { get; set; }
    }
}
