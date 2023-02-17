using Azure;
using Azure.Storage.Queues;
using System;
using System.Threading.Tasks;

namespace StorageQueueReader
{
    class Program
    {
        static string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=psqueueueueueues;AccountKey=SI0VBVPB4vyfQyHAK8V6u5gBojOlakubd/DN57hC1HbFa+ldbMQIEoDeKmedl4tCv8bdmj7onJCu+ASt5SiFKA==;EndpointSuffix=core.windows.net";
        static string QueueName = "kassa";
        static string sasToken = "sv=2021-06-08&ss=q&srt=so&sp=rwdlup&se=2023-02-17T16:43:54Z&st=2023-02-17T08:43:54Z&spr=https&sig=mZnaDAcy4H5l3SfI55tKx%2B6Riuk7Gn2FS1vttAtPVaQ%3D";
        static Uri queueUri = new Uri("https://pshubs.queue.core.windows.net/myqueue");

        static async Task Main(string[] args)
        {
            var t1 = ReadFromQueueAsync(true);
            var t2 = ReadFromQueueAsync();
            await Task.WhenAll(t1, t2);

            Console.WriteLine("Press Enter to Quit");
            Console.ReadLine();
        }

        private static async Task ReadFromQueueAsync(bool fout = false)
        {
            var cnt = 0;
            var creep = new AzureSasCredential(sasToken);
            var client = new QueueClient(queueUri, creep);
            //var client = new QueueClient(ConnectionString, QueueName);
            do
            {
                // 10 seconds "lease" time
                try
                {
                    var response = await client.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
                   
                    if (response.Value == null)
                    {
                        await Task.Delay(100);
                        continue;
                    }
                   // await Task.Delay(200);
                    var msg = response.Value;
                    Console.WriteLine($"[{++cnt}] {msg.Body}");

                    // We need more time to process
                    //await client.UpdateMessageAsync(msg.MessageId, msg.PopReceipt, msg.Body, TimeSpan.FromSeconds(30));
                    // Don't forget to remove
                    if (fout) throw new Exception("oops");
                    await client.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            while (true);
        }
    }
}
