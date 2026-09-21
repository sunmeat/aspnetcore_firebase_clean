using Soccer.Application.DependencyInjection;
using Soccer.Infrastructure.DependencyInjection;
using Soccer.Infrastructure.Persistence;

// View > Terminal:
// cd react.client
// npm install

// cd ..
// dotnet add .\Soccer.Infrastructure\ package Google.Cloud.Firestore
// dotnet list .\Soccer.Infrastructure\ package

// =================================================================
// у Firebase Console:
// Settings > Service accounts > Generate new private key > Download the JSON file
// файл кладемо в Soccer.Infrastructure\Firebase\firebase.json (з внесенням в .gitignore!)

// =================================================================

// Infrastructure > Persistence > FirestoreSeeder.cs
// Infrastructure > DependencyInjection > InfrastructureServiceExtensions.cs
// Program.cs (Soccer.WebAPI) - додано код для підключення FirestoreSeeder

// знесено файли IUnitOfWork.cs, EFUnitOfWork.cs, SoccerContext.cs
// внесено значні зміни в PlayerRepository.cs, TeamRepository.cs, PlayerService.cs, TeamService.cs та їх інтерфейси

var builder = WebApplication.CreateBuilder(args);

string firebasePath = Path.GetFullPath(
    Path.Combine(
        builder.Environment.ContentRootPath,
        "..",
        "Soccer.Infrastructure",
        "firebase.json"));

builder.Services.AddInfrastructure(firebasePath); // !!!
builder.Services.AddApplication();

builder.Services.AddControllers();

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    FirestoreSeeder seeder =
        scope.ServiceProvider.GetRequiredService<FirestoreSeeder>();

    await seeder.SeedAsync();
}

app.MapControllers();

app.Run();