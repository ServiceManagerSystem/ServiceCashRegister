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
    
    public partial class Client
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Client()
        {
            this.Sites = new HashSet<Site>();
        }
    
        public int ID { get; set; }
        public string EGN { get; set; }
        public string NAME { get; set; }
        public string BULSTAT { get; set; }
        public string ADDRESS { get; set; }
        public string TDD { get; set; }
        public string COMMENT { get; set; }
        public int MANAGER_ID { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Site> Sites { get; set; }
        public virtual Manager Manager { get; set; }
    }
}
