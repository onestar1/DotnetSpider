using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.Downloader;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;
using DotnetSpider.Selector;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace DotnetSpider.Sample.samples;

public class BaseUsageSpider(
    IOptions<SpiderOptions> options,
    DependenceServices services,
    ILogger<Spider> logger)
    : Spider(options, services, logger)
{
    public static async Task RunAsync()
    {
        var builder = Builder.CreateDefaultBuilder<BaseUsageSpider>(x =>
        {
            x.Speed = 5;
        });
        builder.UseSerilog();
        // builder.UseDownloader<HttpClientDownloader>();
        builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
        await builder.Build().RunAsync();
    }

    class MyDataParser : DataParser
    {
        protected override Task ParseAsync(DataFlowContext context)
        {
            context.AddData("URL", context.Request.RequestUri);
            context.AddData("Title", context.Selectable.XPath(".//title")?.Value);
            return Task.CompletedTask;
        }

        public override Task InitializeAsync()
        {
            AddRequiredValidator("cnblogs\\.com");
            AddFollowRequestQuerier(Selectors.XPath("."));

            return Task.CompletedTask;
        }
    }

    protected override async Task InitializeAsync(CancellationToken stoppingToken = default)
    {
        await AddRequestsAsync(new Request("http://www.cnblogs.com/"));
        AddDataFlow<MyDataParser>();
        AddDataFlow<ConsoleStorage>();
    }

    protected override SpiderId GenerateSpiderId()
    {
        return new(ObjectId.CreateId().ToString(), "博客园");
    }
}
