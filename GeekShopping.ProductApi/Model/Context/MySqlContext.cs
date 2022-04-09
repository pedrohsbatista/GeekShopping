using Microsoft.EntityFrameworkCore;

namespace GeekShopping.ProductApi.Model.Context
{
    public class MySqlContext : DbContext
    {
        public MySqlContext()
        {
        }

        public MySqlContext(DbContextOptions<MySqlContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 1,
                Name = "Produto A",
                Price = new decimal(10.10),
                Description = "Descrição A",
                CategoryName = "Categoria A",
                ImageUrl = "https://assets.pokemon.com/assets/cms2/img/pokedex/full/004.png"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 2,
                Name = "Produto B",
                Price = new decimal(20.20),
                Description = "Descrição B",
                CategoryName = "Categoria B",
                ImageUrl = "https://assets.pokemon.com/assets/cms2/img/pokedex/full/001.png"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 3,
                Name = "Produto C",
                Price = new decimal(30.30),
                Description = "Descrição C",
                CategoryName = "Categoria C",
                ImageUrl = "https://assets.pokemon.com/assets/cms2/img/pokedex/full/007.png"
            });
        }
    }
}
