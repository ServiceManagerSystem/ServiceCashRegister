//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CashRegisterRepairs.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class Document
    {
        public Nullable<System.DateTime> START_DATE { get; set; }
        public Nullable<System.DateTime> END_DATE { get; set; }
        public string DOC { get; set; }
        public int CLIENT_ID { get; set; }
        public int TEMPLATE_ID { get; set; }
    
        public virtual Client Client { get; set; }
        public virtual Template Template { get; set; }
    }
}
