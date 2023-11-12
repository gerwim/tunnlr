﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tunnlr.Client.Web.Persistence;

#nullable disable

namespace Tunnlr.Client.Web.Persistence.Migrations
{
    [DbContext(typeof(TunnlrDbContext))]
    partial class TunnlrDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.13");

            modelBuilder.Entity("Tunnlr.Client.Web.Persistence.Models.ReservedDomain", b =>
                {
                    b.Property<string>("Domain")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("TunnelId")
                        .HasColumnType("TEXT");

                    b.HasKey("Domain");

                    b.HasIndex("TunnelId")
                        .IsUnique();

                    b.ToTable("ReservedDomains");
                });

            modelBuilder.Entity("Tunnlr.Client.Web.Persistence.Models.Tunnel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("TargetUri")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Tunnels");
                });

            modelBuilder.Entity("Tunnlr.Client.Web.Persistence.Models.ReservedDomain", b =>
                {
                    b.HasOne("Tunnlr.Client.Web.Persistence.Models.Tunnel", "Tunnel")
                        .WithOne("ReservedDomain")
                        .HasForeignKey("Tunnlr.Client.Web.Persistence.Models.ReservedDomain", "TunnelId");

                    b.Navigation("Tunnel");
                });

            modelBuilder.Entity("Tunnlr.Client.Web.Persistence.Models.Tunnel", b =>
                {
                    b.Navigation("ReservedDomain");
                });
#pragma warning restore 612, 618
        }
    }
}
