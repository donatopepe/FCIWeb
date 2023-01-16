using AreaCommonWeb.ModelView.Security;
using Elmah;
using FCIWeb.Areas.FCI.Models;
using FCIWeb.Areas.FCI.Uility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Threading.Tasks;


namespace FCIWeb.Areas.FCI.DAL
{

    public class FCIContext : DbContext
    {

        static FCIContext()
        {
            Database.SetInitializer<FCIContext>(null);
        }


        public FCIContext() : base("FCIContext")
        {
            base.Configuration.ValidateOnSaveEnabled = false;
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.AutoDetectChangesEnabled = false;
        }

        public DbSet<ParametriApp> ParametriApp { get; set; }
        public DbSet<EmailSettings> EmailSettings { get; set; }
        public DbSet<EmailTesti> EmailTesti { get; set; }
        public DbSet<LogEmail> LogEmail { get; set; }
        //public DbSet<Cartelle> Cartella { get; set; }
        public DbSet<Fogli> Fogli { get; set; }
        public DbSet<Righe> Righe { get; set; }
        public DbSet<Colonne> Colonne { get; set; }
        public DbSet<Stati> Stati { get; set; }
        public DbSet<LogStati> LogStati { get; set; }
        public DbSet<ProfiliWL> ProfiliWL { get; set; }
        public DbSet<Profili> Profili { get; set; }

        public void SetProperty(DbEntityEntry entityEntry,string CurrentValue,object obj)
        {
            try {
                var prop = entityEntry.Property(CurrentValue);
                if (prop != null)
                {
                    prop.CurrentValue = obj;
                }
            }
            catch { }

        }


        public override int SaveChanges()
        {
            int controllo = 0;
            try
            {
                UserProfile myProfile = new UserProfileManager().GetCurrent();
                string myUocode = "";
                string flagUffRuolo = ""; //////non credo serva
                string filtroIst = "";
                int? istID = null;
                string matricola = "";
                if (myProfile != null)
                {
                    myUocode = UserProfileManager.GetFiltroSicurezza(myProfile, "codUnitRuolo");
                    flagUffRuolo = UserProfileManager.GetFiltroSicurezza(myProfile, "flagUffRuolo"); //////non credo serva
                    filtroIst = UserProfileManager.GetFiltroSicurezza(myProfile, "IST");
                    istID = !String.IsNullOrEmpty(filtroIst) ? Convert.ToInt32(filtroIst) : Convert.ToInt32(myProfile.UserInfo.CodIstituto);
                    matricola = myProfile.UserInfo.Matricola;
                }

                foreach (var entityEntry in ChangeTracker.Entries())
                {
                    if (entityEntry.State == EntityState.Added)
                    {
                        //entityEntry.Property("DataCreazione").CurrentValue = DateTime.Now;
                        SetProperty(entityEntry, "DataCreazione", DateTime.Now);
                        try
                        {
                            if (myProfile != null)
                            {
                                //entityEntry.Property("Matricola").CurrentValue = matricola;
                                SetProperty(entityEntry, "Matricola", matricola);
                                //entityEntry.Property("UOCode").CurrentValue = myUocode;
                                SetProperty(entityEntry, "UOCode", myUocode);
                                //entityEntry.Property("istID").CurrentValue = istID;
                                SetProperty(entityEntry, "istID", istID);
                                //entityEntry.Property("MatricolaUltimaModifica").CurrentValue = matricola;
                                SetProperty(entityEntry, "MatricolaUltimaModifica", matricola);
                                //entityEntry.Property("UOCodeUltimaModifica").CurrentValue = myUocode;
                                SetProperty(entityEntry, "UOCodeUltimaModifica", myUocode);
                                //entityEntry.Property("istIDUltimaModifica").CurrentValue = istID;
                                SetProperty(entityEntry, "istIDUltimaModifica", istID);
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            //ErrorSignal.FromCurrentContext().Raise(ex);
                        }
                        try
                        {
                            //entityEntry.Property("Guid").CurrentValue = Guid.NewGuid();
                            SetProperty(entityEntry, "Guid", Guid.NewGuid());
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            //ErrorSignal.FromCurrentContext().Raise(ex);
                        }
                    }
                    if (entityEntry.State == EntityState.Modified)
                    {
                        try
                        {
                            if (myProfile != null)
                            {
                                //entityEntry.Property("MatricolaUltimaModifica").CurrentValue = matricola;
                                SetProperty(entityEntry, "MatricolaUltimaModifica", matricola);
                                //entityEntry.Property("UOCodeUltimaModifica").CurrentValue = myUocode;
                                SetProperty(entityEntry, "UOCodeUltimaModifica", myUocode);
                                //entityEntry.Property("istIDUltimaModifica").CurrentValue = istID;
                                SetProperty(entityEntry, "istIDUltimaModifica", istID);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            //ErrorSignal.FromCurrentContext().Raise(ex);
                        }
                        try
                        {
                            var rowversion = entityEntry.Property("RowVersion");
                            if (rowversion != null)
                            {
                                var OriginalValue = (long)rowversion.OriginalValue;
                                var CurrentValue = (long)rowversion.CurrentValue;
                                var DatabaseValues = entityEntry.GetDatabaseValues();
                                var ActualValue = DatabaseValues.GetValue<long>("RowVersion");
                                if (OriginalValue != ActualValue || CurrentValue != ActualValue)
                                {
                                    entityEntry.Reload();
                                    controllo++;
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            //ErrorSignal.FromCurrentContext().Raise(ex);
                        }
                    }
                    if ((entityEntry.State == EntityState.Added) || (entityEntry.State == EntityState.Modified))
                    {
                        try
                        {
                            //entityEntry.Property("RowVersion").CurrentValue = DateTime.UtcNow.Ticks;
                            SetProperty(entityEntry, "RowVersion", DateTime.UtcNow.Ticks);
                            //entityEntry.Property("UltimaModifica").CurrentValue = DateTime.Now;
                            SetProperty(entityEntry, "UltimaModifica", DateTime.Now);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            //ErrorSignal.FromCurrentContext().Raise(ex);
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                //ErrorSignal.FromCurrentContext().Raise(ex);
            }
            base.SaveChanges();
            return controllo;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            ////modelBuilder.Entity<CondizioneStato>().HasMany(o => o.Richiesti);
            ////modelBuilder.Entity<CondizioneStato>().HasMany(o => o.Condizioni);
            //modelBuilder.Entity<Richieste>().HasMany(o => o.Allegati);
            //modelBuilder.Entity<Richieste>().HasMany(o => o.DocumentiRichiesti);
            //modelBuilder.Entity<Richieste>().Ignore(t => t.GiorniAttesaBanca);
            //modelBuilder.Entity<Richieste>().Ignore(t => t.GiorniAttesaCliente);
            //modelBuilder.Entity<Richieste>().Ignore(t => t.TotaleCosto);
            //modelBuilder.Entity<Richieste>().Ignore(t => t.Addetto);
            //modelBuilder.Entity<Richieste>().Ignore(t => t.Struttura);
            //modelBuilder.Entity<Richieste>().Ignore(t => t.DescrizioneUOCode);
            //modelBuilder.Entity<Richieste>().Ignore(t => t.DescrizioneFilialeConsegna);
            //modelBuilder.Entity<Cartelle>().Ignore(t => t.Addetto);
            //modelBuilder.Entity<Cartelle>().Ignore(t => t.Struttura);
            modelBuilder.Entity<Fogli>().Ignore(t => t.Addetto);
            modelBuilder.Entity<Fogli>().Ignore(t => t.Struttura);


            modelBuilder.Entity<Fogli>().Ignore(t => t.AddettoUltimaModifica);
            modelBuilder.Entity<Fogli>().Ignore(t => t.StrutturaUltimaModifica);
            modelBuilder.Entity<Righe>().Ignore(t => t.AddettoUltimaModifica);
            modelBuilder.Entity<Righe>().Ignore(t => t.StrutturaUltimaModifica);

            modelBuilder.Entity<LogStati>().Ignore(t => t.AddettoUltimaModifica);
            modelBuilder.Entity<LogStati>().Ignore(t => t.StrutturaUltimaModifica);

            modelBuilder.Entity<ProfiliWL>().Ignore(t => t.descrizione);

            //modelBuilder.Ignore<uocodeViewModel>();

            //modelBuilder.Ignore<GestioneViewModel>();

            //            modelBuilder.Entity<Richieste>().Property(a => a.Protocollo).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            // modelBuilder.Entity<Richieste>().HasKey(l => new { l.Id, l.Protocollo });



        }
    }
}
