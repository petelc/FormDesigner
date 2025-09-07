﻿using FormDesignerAPI.Core.ContributorAggregate;
using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.Infrastructure.Data;
using FormDesignerAPI.Infrastructure.Identity;

namespace FormDesignerAPI.IntegrationTests.Data;

public abstract class BaseEfRepoTestFixture
{
  protected AppDbContext _dbContext;

  protected BaseEfRepoTestFixture()
  {
    var options = CreateNewContextOptions();
    var _fakeEventDispatcher = Substitute.For<IDomainEventDispatcher>();

    _dbContext = new AppDbContext(options, _fakeEventDispatcher);
  }

  protected static DbContextOptions<AppDbContext> CreateNewContextOptions()
  {
    // Create a fresh service provider, and therefore a fresh
    // InMemory database instance.
    var serviceProvider = new ServiceCollection()
        .AddEntityFrameworkInMemoryDatabase()
        .BuildServiceProvider();

    // Create a new options instance telling the context to use an
    // InMemory database and the new service provider.
    var builder = new DbContextOptionsBuilder<AppDbContext>();
    builder.UseInMemoryDatabase("cleanarchitecture")
           .UseInternalServiceProvider(serviceProvider);

    return builder.Options;
  }

  protected EfRepository<Contributor> GetRepository()
  {
    return new EfRepository<Contributor>(_dbContext);
  }

  protected EfRepository<Form> GetFormRepository()
  {
    return new EfRepository<Form>(_dbContext);
  }

  // protected EfRepository<ApplicationUser> GetUserRepository()
  // {
  //   return new EfRepository<ApplicationUser>(_dbContext);
  // }
}
