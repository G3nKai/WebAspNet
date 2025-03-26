﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using backendRetake.Models;

#nullable disable

namespace backendRetake.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250112210158_CampusCourseDetails")]
    partial class CampusCourseDetails
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0-rc.2.24474.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("backendRetake.Models.CampusCourseModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Annotation")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("CampusGroupId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("MainTeacherId")
                        .HasColumnType("uuid");

                    b.Property<int>("MaximumStudentsCount")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("RemainingSlotsCount")
                        .HasColumnType("integer");

                    b.Property<string>("Requirements")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Semester")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("StartYear")
                        .HasColumnType("integer");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CampusGroupId");

                    b.ToTable("CampusCourse");
                });

            modelBuilder.Entity("backendRetake.Models.CampusCourseNotificationModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("isImportant")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("Notification");
                });

            modelBuilder.Entity("backendRetake.Models.CampusCourseUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CampusCourseId")
                        .HasColumnType("uuid");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("CampusCourseId");

                    b.HasIndex("UserId");

                    b.ToTable("CampusCourseUser");
                });

            modelBuilder.Entity("backendRetake.Models.CampusGroupModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("CampusGroup");
                });

            modelBuilder.Entity("backendRetake.Models.LogoutToken", b =>
                {
                    b.Property<string>("Token")
                        .HasColumnType("text");

                    b.HasKey("Token");

                    b.ToTable("TokenBlackListed");
                });

            modelBuilder.Entity("backendRetake.Models.UserModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("backendRetake.Models.CampusCourseModel", b =>
                {
                    b.HasOne("backendRetake.Models.CampusGroupModel", "CampusGroup")
                        .WithMany("CampusCourses")
                        .HasForeignKey("CampusGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CampusGroup");
                });

            modelBuilder.Entity("backendRetake.Models.CampusCourseUser", b =>
                {
                    b.HasOne("backendRetake.Models.CampusCourseModel", "CampusCourse")
                        .WithMany("CampusCourseUsers")
                        .HasForeignKey("CampusCourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("backendRetake.Models.UserModel", "User")
                        .WithMany("CampusCourseUsers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CampusCourse");

                    b.Navigation("User");
                });

            modelBuilder.Entity("backendRetake.Models.CampusCourseModel", b =>
                {
                    b.Navigation("CampusCourseUsers");
                });

            modelBuilder.Entity("backendRetake.Models.CampusGroupModel", b =>
                {
                    b.Navigation("CampusCourses");
                });

            modelBuilder.Entity("backendRetake.Models.UserModel", b =>
                {
                    b.Navigation("CampusCourseUsers");
                });
#pragma warning restore 612, 618
        }
    }
}
