﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CashRegisterServiceContext : DbContext
    {
        public CashRegisterServiceContext()
            : base("name=CashRegisterServiceContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<DeviceModel> DeviceModels { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<Manager> Managers { get; set; }
        public virtual DbSet<Site> Sites { get; set; }
        public virtual DbSet<Template> Templates { get; set; }
    }
}
