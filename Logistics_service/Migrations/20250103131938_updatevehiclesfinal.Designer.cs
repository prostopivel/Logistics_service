﻿// <auto-generated />
using System;
using Logistics_service.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Taxi_App.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250103131938_updatevehiclesfinal")]
    partial class updatevehiclesfinal
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Logistics_service.Models.Point", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ConnectedPointsIndexes")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "ConnectedPointsIndexes");

                    b.Property<string>("Distances")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Index")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PosX")
                        .HasColumnType("int");

                    b.Property<int>("PosY")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Points");
                });

            modelBuilder.Entity("Logistics_service.Models.Users.User", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int?>("Id"));

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(13)
                        .HasColumnType("nvarchar(13)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasDiscriminator().HasValue("User");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Logistics_service.Models.Vehicle", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int?>("Id"));

                    b.Property<int>("GarageId")
                        .HasColumnType("int");

                    b.Property<int>("Speed")
                        .HasColumnType("int");

                    b.Property<int?>("Status")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.HasKey("Id");

                    b.HasIndex("GarageId");

                    b.ToTable("Vehicles");
                });

            modelBuilder.Entity("Logistics_service.Models.Users.Administrator", b =>
                {
                    b.HasBaseType("Logistics_service.Models.Users.User");

                    b.HasDiscriminator().HasValue("Administrator");
                });

            modelBuilder.Entity("Logistics_service.Models.Users.Customer", b =>
                {
                    b.HasBaseType("Logistics_service.Models.Users.User");

                    b.Property<string>("OrdersId")
                        .HasColumnType("nvarchar(max)");

                    b.HasDiscriminator().HasValue("Customer");
                });

            modelBuilder.Entity("Logistics_service.Models.Users.Manager", b =>
                {
                    b.HasBaseType("Logistics_service.Models.Users.User");

                    b.HasDiscriminator().HasValue("Manager");
                });

            modelBuilder.Entity("Logistics_service.Models.Vehicle", b =>
                {
                    b.HasOne("Logistics_service.Models.Point", "Garage")
                        .WithMany()
                        .HasForeignKey("GarageId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Garage");
                });
#pragma warning restore 612, 618
        }
    }
}
