using BugSearch.Shared.Enums;
using BugSearch.Shared.Models;
using BugSearch.Crawler.Services;
using BugSearch.Shared.Singletons;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TaskJobs>(TaskJobs.GetInstance());
builder.Services.AddSingleton<CrawlerRequest>(new CrawlerRequest(new List<string>(), new CrawlerProperties(false, false, 100, 0)));
builder.Services.AddSingleton<CrawlerJob>(new CrawlerJob(JobStatus.None, new List<string>(), new CancellationTokenSource())); // Registrar o enum JobStatus
builder.Services.AddHostedService<CrawlerService>(new Func<IServiceProvider, CrawlerService> (serviceProvider => {
    var crawlerReq = serviceProvider.GetService<CrawlerRequest>();
    var crawlerJob = serviceProvider.GetService<CrawlerJob>();

    if (crawlerJob is null)
    {
        throw new ArgumentNullException(nameof(crawlerJob));
    }

    if (crawlerReq is null)
    {
        throw new ArgumentNullException(nameof(crawlerReq));
    }

    if (crawlerJob.Url is null)
    {
        throw new ArgumentNullException(nameof(crawlerJob.Url));
    }
    

    return new CrawlerService(crawlerReq, crawlerJob);
}));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseRouting();

app.Run();
