using System.Diagnostics;
using System.Text;
using ShellProgressBar;

namespace TraverseFolderEncoded;

internal abstract class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Welcome to use TraverseFolderEncoded!");
        if (args.Length != 2)   // 判断启动参数是否正确
        {
            Console.WriteLine("Incorrect startup parameters.");
            Console.WriteLine("Example: TraverseFolderEncoded.exe [Encoded(utf-8/gbk/gb2312/65001...)] [File/Folder Path]");
            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }
        else
        {
            string path = args[1];  // 指定路径
            string targetEncodingStringName = args[0];  // 指定目标编码名称
            int.TryParse(args[0], out var targetEncodingIntName);   // 判断是否为数字，如果是则使用数字作为目标编码
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);  // 注册编码
            Encoding targetEncoding;    // 目标编码
            if (targetEncodingIntName != 0) // 如果是数字则使用数字作为目标编码
                targetEncoding = Encoding.GetEncoding(targetEncodingIntName);   
            else   // 如果不是数字则使用名称作为目标编码
                targetEncoding = Encoding.GetEncoding(targetEncodingStringName);
            Stopwatch stopwatch = new Stopwatch();  // 计时器
            
            if (File.Exists(path))  // 判断路径是否为文件
            {
                stopwatch.Start();
                // 读取文件内容
                byte[] bytes = File.ReadAllBytes(path);
                // 将文件内容从原编码转换为目标编码
                Encoding sourceEncoding = Encoding.Default; // 自动检测原编码
                byte[] targetBytes = Encoding.Convert(sourceEncoding, targetEncoding, bytes);
                // 将转换后的内容写入文件
                File.WriteAllBytes(path, targetBytes);
                Console.WriteLine("File {0} have been converted to {1} encoding, taking time {2}",
                    path, targetEncoding.EncodingName, stopwatch.Elapsed);
                stopwatch.Stop();
            }
            else if (Directory.Exists(path))    // 判断路径是否为文件夹
            {
                // 获取文件夹中的所有文件
                string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                
                int totalTicks = files.Length;
                var options = new ProgressBarOptions
                    {
                        // 设置前景色
                        ForegroundColor = ConsoleColor.Yellow,
                        // 设置完成时的前景色
                        ForegroundColorDone = ConsoleColor.DarkGreen,
                        // 设置背景色
                        BackgroundColor = ConsoleColor.DarkGray,
                        // 设置背景字符
                        BackgroundCharacter = '\u2593'
                    };
                using (var pbar = new ProgressBar(totalTicks, "Processing", options))
                {
                    //double percent = 100.00 / files.Length;
                    stopwatch.Start();
                    // 遍历所有文件并修改编码
                    foreach (string file in files)
                    {
                        pbar.Message = $"Start {Array.IndexOf(files, file) + 1} of {files.Length}: {file}";
                        // 读取文件内容
                        byte[] bytes = File.ReadAllBytes(file);
                        // 将文件内容从原编码转换为目标编码
                        Encoding sourceEncoding = Encoding.Default; // 自动检测原编码
                        byte[] targetBytes = Encoding.Convert(sourceEncoding, targetEncoding, bytes);
                        await Task.Delay(1);
                        // 将转换后的内容写入文件
                        File.WriteAllBytes(file, targetBytes);
                        pbar.Tick($"End {Array.IndexOf(files, file) + 1} of {files.Length}: {file}");
                    }
                    stopwatch.Stop();
                }

                Console.WriteLine("All files have been converted to {0} encoding, taking time {1}",
                    targetEncoding.EncodingName, stopwatch.Elapsed);
            }
            else   // 路径无效
            {
                Console.WriteLine("The path is invalid.");
            }

            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }
    }
}