﻿// <auto-generated />
using System;
using AngularAuthAPI.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AngularAuthAPI.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240613145822_CreateStudentResponsesTable")]
    partial class CreateStudentResponsesTable
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.29")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AngularAuthAPI.Models.StudentResponse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("ResponseDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Responses")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("StudentID")
                        .HasColumnType("integer");

                    b.Property<int>("TemplateID")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("StudentID");

                    b.HasIndex("TemplateID");

                    b.ToTable("student_responses", (string)null);
                });

            modelBuilder.Entity("AngularAuthAPI.Models.Template", b =>
                {
                    b.Property<int>("templateID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("templateID"));

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Fields")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Specializations")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("templateID");

                    b.ToTable("templates", (string)null);
                });

            modelBuilder.Entity("AngularAuthAPI.Models.User", b =>
                {
                    b.Property<int>("userID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("userID"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Facultate")
                        .HasColumnType("text");

                    b.Property<string>("Nume")
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("text");

                    b.Property<DateTime>("RefreshTokenExpiryTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Role")
                        .HasColumnType("text");

                    b.Property<string>("Specializare")
                        .HasColumnType("text");

                    b.Property<string>("Token")
                        .HasColumnType("text");

                    b.HasKey("userID");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("AngularAuthAPI.Models.StudentResponse", b =>
                {
                    b.HasOne("AngularAuthAPI.Models.User", "Student")
                        .WithMany()
                        .HasForeignKey("StudentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AngularAuthAPI.Models.Template", "Template")
                        .WithMany()
                        .HasForeignKey("TemplateID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Student");

                    b.Navigation("Template");
                });
#pragma warning restore 612, 618
        }
    }
}
