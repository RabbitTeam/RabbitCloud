using Rabbit.Go;
using Rabbit.Go.Interceptors;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ConsoleApp
{
    /*    public class MyClass2:DecoderAttribute
        {
            #region Overrides of DecoderAttribute

            public override Task<object> DecodeAsync(HttpResponseMessage response, Type type)
            {
                return base.DecodeAsync(response, type);
            }

            #endregion Overrides of DecoderAttribute
        }
*/

    internal class MyClass : RequestInterceptorAttribute
    {
        #region Overrides of RequestInterceptorAttribute

        public override void OnRequestExecuting(RequestExecutingContext context)
        {
            Console.WriteLine("OnRequestExecuting");
        }

        public override void OnRequestExecuted(RequestExecutedContext context)
        {
            base.OnRequestExecuted(context);
        }

        #endregion Overrides of RequestInterceptorAttribute
    }

    [Go("http://192.168.100.150:7704/{userId}")]
    public interface ITestGoClient
    {
        [GoGet("/gift/count")]
        Task<long> TestAsync(long userId);
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var testGoClient= new GoBuilder()
//                .Codec(JsonCodec.Instance)
//                .Client(new HttpClient())
//                .KeyValueFormatterFactory(new KeyValueFormatterFactory())
                .Target<ITestGoClient>();

            
            
            Console.WriteLine(await testGoClient.TestAsync(1));
            var w = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                await testGoClient.TestAsync(1);
            }
            w.Stop();
            Console.WriteLine(w.ElapsedMilliseconds+"ms");
            return;
/*            var goOptions = Options.Create(new GoOptions());
            var provider = new DefaultGoModelProvider(goOptions);

            var descriptorProvider = new DefaultMethodDescriptorProvider(goOptions, new[] { typeof(ITestGoClient) }, new[] { provider });

            var collectionProvider = new MethodDescriptorCollectionProvider(new[] { descriptorProvider });

            var items = collectionProvider.Items;

            var descriptor = items.First();

            var factory =
                new MethodInvokerFactory(new MethodInvokerCache(new HttpClient(), new ServiceCollection().BuildServiceProvider()));

            var methodInvoker = factory.CreateInvoker(descriptor);

            var t = await methodInvoker.InvokeAsync(new object[] { 1 });

            //            var result=InterceptorFactory.GetAllInterceptors(descriptor, new ServiceCollection().BuildServiceProvider());
            //            var class1 = new DefaultMethodInvoker(new HttpClient(), descriptor, JsonCodec.Instance, result.Interceptors);

            //            var t = await class1.InvokeAsync(new object[] {1});

            Console.WriteLine(t);

            /*            var providerContext = new GoModelProviderContext(new[] {typeof(ITestGoClient)});

                        provider.OnProvidersExecuting(providerContext);#1#

            return;*/
            /*            var httpClient = new HttpClient(new RetryDelegatingHandler());
            //            await httpClient.GetStringAsync("http://192.168.100.150:7704/1/gift/count");
                        var rm = new HttpRequestMessage(HttpMethod.Get, "http://192.168.100.150:7704/1/gift/count")
                        {
                            Properties = {{"1", "1"}}
                        };
                        rm.Headers.Add("t","1");
                        rm.Headers.Add("t","2");
                        Console.WriteLine(string.Join(",", rm.Headers.GetValues("t")));
                        return;

                        var r=await httpClient.SendAsync(rm);

                        var headers = rm.GetContentHeaders();
                        foreach (var httpHeader in headers)
                        {
                            Console.WriteLine(httpHeader.Key, httpHeader.Value);
                        }

            //            Console.WriteLine(r.RequestMessage.Properties["1"]);
                        return;*/
            /*
                        var appServices = new ServiceCollection()
                            .AddOptions()
                            .AddLogging()
                            .AddConsulDiscovery(s =>
                            {
                                s.Address = "http://192.168.100.150:8500";
                                s.Prefix = "cs.";
                            })
                            .BuildServiceProvider();

                        var app = new RabbitApplicationBuilder(appServices)
                            .UseRabbitClient()
                            .UseRabbitHttpClient()
                            .Build();

                        var services = new ServiceCollection()
                            .AddOptions()
                            .Configure<GoOptions>(o =>
                            {;
                                //                    o.GlobalInterceptors.Add(new MyClass("1"));
                                //                    o.GlobalInterceptors.Add(new MyClass("2"));
                                //                        o.FormatterMappings.Set("application/json", dingTalkCodec, dingTalkCodec);
                            })
                            .AddGo()
            //                .AddSingleton<IGoClient>(new RabbitCloudGoClient(app))
                            .BuildServiceProvider();

                        var goFactory = services.GetService<IGoFactory>();

                        var testGoClient = goFactory.CreateInstance<ITestGoClient>();

                        var t = await testGoClient.TestAsync(1111);
                        Console.WriteLine(t);
                        return;

                        var watch = Stopwatch.StartNew();
                        for (int i = 0; i < 1000; i++)
                        {
            //                await httpClient.GetStringAsync("http://192.168.100.150:7704/1/gift/count");
                                            await testGoClient.TestAsync(1);
                        }
            //            Parallel.For(0, 1000, async i => { await testGoClient.TestAsync(1); });
                        watch.Stop();
                        Console.WriteLine(watch.ElapsedMilliseconds+"ms");
                        return;

                        var client = goFactory.CreateInstance<IAdvertiseGoClient>();

                        var queryable = client.GetAdvertisesAsync(new CommonFilter());

                        var tt = await queryable.Take(2).ExecuteAsync();
                        Console.WriteLine(JsonConvert.SerializeObject(tt));
                        return;*/

            /*            var testGoClient = goFactory.CreateInstance<ITestGoClient>();

                        var t = await testGoClient.TestAsync(1);

                        Console.WriteLine(t);
                        t = await testGoClient.TestAsync(1);
                        Console.WriteLine(t);
                        return;*/

            /*var dingTalkGoClient = goFactory.CreateInstance<IDingTalkGoClient>();

            await dingTalkGoClient.SendAsync(new TextMessage("test"));*/
            //            await dingTalkGoClient.SendAsync(new LinkMessage("title","text","http://www.baidu.com", "https://www.baidu.com/img/bd_logo1.png"));
            //            return;
            //            await dingTalkGoClient.SendAsync(new MarkdownMessage("杭州天气", "#### 杭州天气 @156xxxx8827") {IsAtAll = true});
            //            return;
            /*            await dingTalkGoClient.SendAsync(
                            new SingleActionCardMessage("title", "text", "sTitle", "https://open-doc.dingtalk.com/")
                            {
                                BtnOrientation = Orientation.Vertical,
                                HideAvatar = false
                            });*/
            /*

                        var message = new ActionCardMessage("乔布斯 20 年前想打造一间苹果咖啡厅，而它正是 Apple Store 的前身", @"![screenshot](@lADOpwk3K80C0M0FoA)
            ### 乔布斯 20 年前想打造的苹果咖啡厅
                        Apple Store 的设计正从原来满满的科技感走向生活化，而其生活化的走向其实可以追溯到 20 年前苹果一个建立咖啡馆的计划")
                        {
                            HideAvatar = true,
                            BtnOrientation = Orientation.Horizontal
                        };

                        message.Buttons.Add(new ActionCardMessage.ActionCardButton("内容不错", "https://www.dingtalk.com/"));
                        message.Buttons.Add(new ActionCardMessage.ActionCardButton("不感兴趣", "https://www.dingtalk.com/"));
            */

            /*var message = new FeedCardMessage();
            message.Items.Add(new FeedCardMessage.FeedCardItem("title1", "http://www.baidu.com", "https://www.baidu.com/img/bd_logo1.png"));
            message.Items.Add(new FeedCardMessage.FeedCardItem("title2", "http://www.baidu.com", "https://www.baidu.com/img/bd_logo1.png"));
            message.Items.Add(new FeedCardMessage.FeedCardItem("title3", "http://www.baidu.com", "https://www.baidu.com/img/bd_logo1.png"));

            await dingTalkGoClient.SendAsync(message);*/
        }
    }
}