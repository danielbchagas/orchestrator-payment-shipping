﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Samples.Orchestrator.Core.Infrastructure.Database;

#nullable disable

namespace Samples.Orchestrator.Core.Migrations
{
    [DbContext(typeof(OrderStateDbContext))]
    [Migration("20250728095627_Update-3")]
    partial class Update3
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Samples.Orchestrator.Core.Infrastructure.StateMachine.OrderState", b =>
                {
                    b.Property<Guid>("CorrelationId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("DATE");

                    b.Property<string>("CurrentState")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("VARCHAR");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("VARCHAR");

                    b.Property<int>("RetryCount")
                        .HasColumnType("INT");

                    b.HasKey("CorrelationId");

                    b.ToTable("OrderStates");
                });
#pragma warning restore 612, 618
        }
    }
}
