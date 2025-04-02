using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TrabalhoES2.Models;

public partial class projetoPraticoDbContext : IdentityDbContext<Utilizador, IdentityRole<int>, int>
{
    public projetoPraticoDbContext()
    {
    }

    public projetoPraticoDbContext(DbContextOptions<projetoPraticoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Ativofinanceiro> Ativofinanceiros { get; set; }

    public virtual DbSet<Banco> Bancos { get; set; }

    public virtual DbSet<Carteira> Carteiras { get; set; }

    public virtual DbSet<Depositoprazo> Depositoprazos { get; set; }

    public virtual DbSet<Fundoinvestimento> Fundoinvestimentos { get; set; }

    public virtual DbSet<Imovelarrendado> Imovelarrendados { get; set; }

    public virtual DbSet<Utilizador> Utilizadors { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=projetoPratico;Username=postgres;Password=1234;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder
            .HasPostgresEnum("tipoativofinanceiro", new[] { "DepositoPrazo", "ImovelArrendado", "FundoInvestimento" })
            .HasPostgresEnum("tipoutilizador", new[] { "Cliente", "Admin", "UserManager" });

        modelBuilder.Entity<Ativofinanceiro>(entity =>
        {
            entity.HasKey(e => e.AtivofinanceiroId).HasName("ativofinanceiro_pkey");

            entity.ToTable("ativofinanceiro");

            entity.Property(e => e.AtivofinanceiroId).HasColumnName("ativofinanceiro_id");
            entity.Property(e => e.CarteiraId).HasColumnName("carteira_id");
            entity.Property(e => e.Datainicio).HasColumnName("datainicio");
            entity.Property(e => e.Duracaomeses).HasColumnName("duracaomeses");
            entity.Property(e => e.Percimposto)
                .HasPrecision(5, 2)
                .HasColumnName("percimposto");

            entity.HasOne(d => d.Carteira).WithMany(p => p.Ativofinanceiros)
                .HasForeignKey(d => d.CarteiraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ativofinanceiro_carteira_id_fkey");
        });

        modelBuilder.Entity<Banco>(entity =>
        {
            entity.HasKey(e => e.BancoId).HasName("banco_pkey");

            entity.ToTable("banco");

            entity.Property(e => e.BancoId).HasColumnName("banco_id");
            entity.Property(e => e.Nome).HasColumnName("nome");
        });

        modelBuilder.Entity<Carteira>(entity =>
        {
            entity.HasKey(e => e.CarteiraId).HasName("carteira_pkey");

            entity.ToTable("carteira");

            entity.Property(e => e.CarteiraId).HasColumnName("carteira_id");
            entity.Property(e => e.Nome).HasColumnName("nome");
            entity.Property(e => e.UtilizadorId).HasColumnName("utilizador_id");

            entity.HasOne(d => d.Utilizador).WithMany(p => p.Carteiras)
                .HasForeignKey(d => d.UtilizadorId)
                .HasConstraintName("carteira_utilizador_id_fkey");
        });

        modelBuilder.Entity<Depositoprazo>(entity =>
        {
            entity.HasKey(e => e.DepositoprazoId).HasName("depositoprazo_pkey");

            entity.ToTable("depositoprazo");

            entity.HasIndex(e => e.AtivofinanceiroId, "depositoprazo_ativofinanceiro_id_key").IsUnique();

            entity.Property(e => e.DepositoprazoId).HasColumnName("depositoprazo_id");
            entity.Property(e => e.AtivofinanceiroId).HasColumnName("ativofinanceiro_id");
            entity.Property(e => e.BancoId).HasColumnName("banco_id");
            entity.Property(e => e.Nrconta)
                .HasMaxLength(50)
                .HasColumnName("nrconta");
            entity.Property(e => e.Taxajuroanual)
                .HasPrecision(5, 2)
                .HasColumnName("taxajuroanual");
            entity.Property(e => e.Titular).HasColumnName("titular");
            entity.Property(e => e.Valoratual)
                .HasPrecision(15, 2)
                .HasColumnName("valoratual");
            entity.Property(e => e.Valorinicial)
                .HasPrecision(15, 2)
                .HasColumnName("valorinicial");

            entity.HasOne(d => d.Ativofinanceiro).WithOne(p => p.Depositoprazo)
                .HasForeignKey<Depositoprazo>(d => d.AtivofinanceiroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("depositoprazo_ativofinanceiro_id_fkey");

            entity.HasOne(d => d.Banco).WithMany(p => p.Depositoprazos)
                .HasForeignKey(d => d.BancoId)
                .HasConstraintName("depositoprazo_banco_id_fkey");
        });

        modelBuilder.Entity<Fundoinvestimento>(entity =>
        {
            entity.HasKey(e => e.FundoinvestimentoId).HasName("fundoinvestimento_pkey");

            entity.ToTable("fundoinvestimento");

            entity.HasIndex(e => e.AtivofinanceiroId, "fundoinvestimento_ativofinanceiro_id_key").IsUnique();

            entity.Property(e => e.FundoinvestimentoId).HasColumnName("fundoinvestimento_id");
            entity.Property(e => e.AtivofinanceiroId).HasColumnName("ativofinanceiro_id");
            entity.Property(e => e.BancoId).HasColumnName("banco_id");
            entity.Property(e => e.Montanteinvestido)
                .HasPrecision(15, 2)
                .HasColumnName("montanteinvestido");
            entity.Property(e => e.Nome).HasColumnName("nome");
            entity.Property(e => e.Taxajuropdefeito)
                .HasPrecision(5, 2)
                .HasColumnName("taxajuropdefeito");

            entity.HasOne(d => d.Ativofinanceiro).WithOne(p => p.Fundoinvestimento)
                .HasForeignKey<Fundoinvestimento>(d => d.AtivofinanceiroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fundoinvestimento_ativofinanceiro_id_fkey");

            entity.HasOne(d => d.Banco).WithMany(p => p.Fundoinvestimentos)
                .HasForeignKey(d => d.BancoId)
                .HasConstraintName("fundoinvestimento_banco_id_fkey");
        });

        modelBuilder.Entity<Imovelarrendado>(entity =>
        {
            entity.HasKey(e => e.ImovelarrendadoId).HasName("imovelarrendado_pkey");

            entity.ToTable("imovelarrendado");

            entity.HasIndex(e => e.AtivofinanceiroId, "imovelarrendado_ativofinanceiro_id_key").IsUnique();

            entity.HasIndex(e => e.BancoId, "imovelarrendado_banco_id_key").IsUnique();

            entity.Property(e => e.ImovelarrendadoId).HasColumnName("imovelarrendado_id");
            entity.Property(e => e.AtivofinanceiroId).HasColumnName("ativofinanceiro_id");
            entity.Property(e => e.BancoId).HasColumnName("banco_id");
            entity.Property(e => e.Designacao).HasColumnName("designacao");
            entity.Property(e => e.Localizacao).HasColumnName("localizacao");
            entity.Property(e => e.Valoranualdespesas)
                .HasPrecision(15, 2)
                .HasColumnName("valoranualdespesas");
            entity.Property(e => e.Valorimovel)
                .HasPrecision(15, 2)
                .HasColumnName("valorimovel");
            entity.Property(e => e.Valormensalcondo)
                .HasPrecision(15, 2)
                .HasColumnName("valormensalcondo");
            entity.Property(e => e.Valorrenda)
                .HasPrecision(15, 2)
                .HasColumnName("valorrenda");

            entity.HasOne(d => d.Ativofinanceiro).WithOne(p => p.Imovelarrendado)
                .HasForeignKey<Imovelarrendado>(d => d.AtivofinanceiroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("imovelarrendado_ativofinanceiro_id_fkey");

            entity.HasOne(d => d.Banco).WithOne(p => p.Imovelarrendado)
                .HasForeignKey<Imovelarrendado>(d => d.BancoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("imovelarrendado_banco_id_fkey");
        });

        modelBuilder.Entity<Utilizador>(entity =>
        {
            entity.HasKey(e => e.Id); // A chave primária é o Id do IdentityUser<int>, que já é gerido pelo Identity

            entity.ToTable("utilizador");

            entity.HasIndex(e => e.Email).IsUnique(); // Garante que o email seja único

            entity.Property(e => e.Nome).HasColumnName("nome");
            entity.Property(e => e.TpUtilizador)
                .HasConversion(
                    v => v.ToString(),
                    v => (Utilizador.TipoUtilizador)Enum.Parse(typeof(Utilizador.TipoUtilizador), v));

            // Relacionamento com a tabela Carteira
            entity.HasMany(u => u.Carteiras)
                .WithOne(c => c.Utilizador)
                .HasForeignKey(c => c.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
