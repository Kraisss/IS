using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace webtrades1
{
    class Classes
    {
    }
    class webcontext : DbContext//Класс контекста
    {
        public webcontext() : base("DefaultConnection")//Строка подключения из app.config
        { }
        public DbSet<Person> People { get; set; }//Создаем модель таблицы для каждой таблицы из базы данных
        public DbSet<Role> Roles { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<TradeOperation> TradeOperations { get; set; }
        //public DbSet<OperationType> OperationTypes { get; set; }
        // public DbSet<Level> Levels { get; set; }
        public DbSet<ItemPersonAccount> ItemPersonAccounts { get; set; }
        public DbSet<ExchangeRateHistory> ExchangeRateHistories { get; set; }
        


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // использование Fluent API
            //Вносим необходимые ограничения для некоторых столбцов таблиц
            modelBuilder.Entity<Person>().Property(p => p.Login).HasColumnType("nvarchar").HasMaxLength(20).IsRequired().IsUnicode(true);
            modelBuilder.Entity<Person>().Property(p => p.PasswordHash).HasColumnType("nvarchar").HasMaxLength(64).IsRequired().IsUnicode(true);
            modelBuilder.Entity<Person>().Property(p => p.PasswordSalt).HasColumnType("nvarchar").HasMaxLength(50).IsRequired().IsUnicode(true);
            modelBuilder.Entity<Person>().Ignore(p => p.Access);
            modelBuilder.Entity<Role>().Property(p => p.Name).HasColumnType("nvarchar").HasMaxLength(20).IsRequired().IsUnicode(true);
            modelBuilder.Entity<Item>().Property(p => p.Name).HasColumnType("nvarchar").HasMaxLength(20).IsRequired().IsUnicode(true);
            modelBuilder.Entity<TradeOperation>().Property(p => p.OperationType).HasColumnType("nvarchar").HasMaxLength(10).IsRequired().IsUnicode(true);
            //modelBuilder.Entity<Client>().Property(p => p.Patronymic).HasColumnType("nvarchar").HasMaxLength(15).IsUnicode(true);
            //modelBuilder.Entity<TypeOfPremise>().Property(p => p.Name).HasColumnType("nvarchar").IsRequired().HasMaxLength(20).IsUnicode(true);
            //modelBuilder.Entity<Premise>().Property(p => p.Name).HasColumnType("nvarchar").IsRequired().HasMaxLength(30).IsUnicode(true);
            //modelBuilder.Entity<AgeRestriction>().Property(p => p.Name).HasColumnType("nvarchar").IsRequired().HasMaxLength(30).IsUnicode(true);
            //modelBuilder.Entity<TypeOfLesson>().Property(p => p.Name).HasColumnType("nvarchar").IsRequired().HasMaxLength(20).IsUnicode(true);
            //modelBuilder.Entity<Lesson>().Property(p => p.Name).HasColumnType("nvarchar").IsRequired().HasMaxLength(20).IsUnicode(true);
            //modelBuilder.Entity<Status>().Property(p => p.Name).HasColumnType("nvarchar").IsRequired().HasMaxLength(30).IsUnicode(true);
            //modelBuilder.Entity<Client>().Ignore(p => p.FullName);
            //modelBuilder.Entity<Worker>().Ignore(p => p.FullName);
            base.OnModelCreating(modelBuilder);
        }
    }
    class MyContextInitializer : CreateDatabaseIfNotExists<webcontext>
    {
        protected override void Seed(webcontext context)
        {
            //if (!context.Items.Any())
            //{
                context.Items.Add(new Item
                {
                    Name = "BeepCoin",
                    ExchangeRate = 7400.00
                });
                context.Items.Add(new Item
                {
                    Name = "Dollar",
                    ExchangeRate = 100.00
                });
                context.Items.Add(new Item
                {
                    Name = "EuroCoin",
                    ExchangeRate = 150.00

                });
                context.Items.Add(new Item
                {
                    Name = "Rubol",
                    ExchangeRate = 10.00

                });
                context.Items.Add(new Item
                {
                    Name = "OilBarrel",
                    ExchangeRate = 20.00

                });

                context.SaveChanges();
            //}
            //if (!context.Roles.Any())
            //{
                context.Roles.AddRange(new List<Role> {
                    new Role
                    {
                        Name = "Admin"

                    },
                    new Role
                    {
                        Name = "User"
                    }

                });
                context.SaveChanges();
            //}
            //if (!context.ExchangeRateHistories.Any())
            //{
                List<Item> itemslist = context.Items.ToList();

                DateTime[] arr = new DateTime[5];
                arr[0] = new DateTime(2015, 5, 17, 18, 30, 25);
                arr[1] = new DateTime(2018, 1, 9, 16, 50, 20);
                arr[2] = new DateTime(2019, 10, 24, 18, 30, 25);
                arr[3] = new DateTime(2020, 1, 2, 13, 24, 25);
                arr[4] = new DateTime(2020, 4, 13, 2, 2, 25);
                foreach (Item i in itemslist)
                {
                    Random random = new Random();
                    double db = i.ExchangeRate;
                    for (int j = 0; j < 4; j++)
                    {

                        ExchangeRateHistory erh = new ExchangeRateHistory
                        {
                            Item = i,
                            ItemId = i.Id,
                            DateOfChange = arr[j],
                            ExchangeRateChange = Math.Round(random.NextDouble() * (db + (db / 5) - (db - (db / 5)) + (db - (db / 5))), 2)

                        };
                        context.ExchangeRateHistories.Add(erh);
                        i.RateHistory.Add(erh);
                        context.SaveChanges();

                    }
                    ExchangeRateHistory erhf = new ExchangeRateHistory
                    {
                        Item = i,
                        ItemId = i.Id,
                        DateOfChange = arr[4],
                        ExchangeRateChange = i.ExchangeRate
                    };
                    context.ExchangeRateHistories.Add(erhf);
                    i.RateHistory.Add(erhf);
                    context.SaveChanges();

                }

            //}
        }
    }
    public class ExchangeRateHistory
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public double ExchangeRateChange { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime DateOfChange { get; set; }

        [ForeignKey("Item")]
        public int ItemId { get; set; }
        public Item Item { get; set; }

    }
    public class Item
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public double ExchangeRate { get; set; }

        public ICollection<ExchangeRateHistory> RateHistory { get; set; }//Связь один ко многим 
        public Item()
        {
            RateHistory = new List<ExchangeRateHistory>();
        }
    }
    public class ItemPersonAccount
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public double ItemQuantity { get; set; }

        [ForeignKey("Person")]
        [Required]
        public int PersonId { get; set; }
        public Person Person { get; set; }

        [ForeignKey("Item")]
        [Required]
        public int ItemId { get; set; }
        public Item Item { get; set; }

    }
    public class PasswordHash
    {

        public static string CreateSalt()
        {

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[16];
            rng.GetBytes(buff);


            return Convert.ToBase64String(buff);
        }

        public static string GetHash(string input)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }
    }
    public class Person
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Login { get; set; }

        [NotMapped]
        public string Access { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string PasswordSalt { get; set; }

        [Required]
        public double PersonalAccount { get; set; }

        [ForeignKey("role")]
        [Required]
        public int RoleId { get; set; }
        public Role role { get; set; }

        [Required]
        [Range(0, 4, ErrorMessage = "{0} должен быть не менее {1} и не более {2}")]
        //[ForeignKey("level")]
        public int Level { get; set; }
        // public Level level { get; set; }

        public ICollection<ItemPersonAccount> Accounts { get; set; }//Связь один ко многим с таблицей TradeOperation
        public ICollection<TradeOperation> Operations { get; set; }//Связь один ко многим с таблицей TradeOperation
        public Person()
        {
            Operations = new List<TradeOperation>();
            Accounts = new List<ItemPersonAccount>();
        }
    }
    public class Role
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
    public class TradeOperation
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public double Profit { get; set; }

        [ForeignKey("Item")]
        [Required]
        public int ItemId { get; set; }
        public Item Item { get; set; }

        [ForeignKey("Person")]
        [Required]
        public int PersonId { get; set; }
        public Person Person { get; set; }

        [Required]
        public string OperationType { get; set; }
        [Required]
        [Column(TypeName = "datetime")]
        public DateTime DateOfOperation { get; set; }

        //[ForeignKey("operationType")]
        //[Required]
        //public int OperationTypeId { get; set; }
        //public OperationType operationType { get; set; }
    }


}
