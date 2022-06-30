## AutoUpgrade

基于 .net 6.0 的winform程序自动更新程序，
包括1.服务端，AutoUpgrade
服务端中的wwwroot未作为静态目录使用，仅用于保存程序最新文件的文件夹，一个项目一个文件夹，文件夹中有两个文件必须，
> 1 .ignore  ，用于定义在自动更新中不同步服务端的文件，比如sqlite数据库，
> 2 version.txt，用于指定当前程序的最新版本，如果客户端的版本与当前最新版本不一致，将会在程序启动时触发自动更新

token信息保存在web项目的appsettings.json中，在安全方面，token保证了只有特定的用户才有权限去服务器去下载最新的程序文件，该配置在下列步骤中会被用到。

```
  "ProjectInfo": [
    {
      "Name": "SampleProject",
      "Password": "5C210AF8-6D4E-4D06-A9B7-F1E04BFE8AC3"
    }
  ]
  
 ```

2，客户端更新程序，只需要在目标程序的main函数中添加少许代码即可

```

            #if DEBUG
            args = new string[] { "false" };//发布时生成release版本的放到服务器，必须要这样
            #endif
            if (args.Length != 0 && args[0] == "false")
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                string currentVersion = "0.0.0.0";
                if (File.Exists("version.txt"))
                {
                    currentVersion = File.ReadAllText("version.txt");
                }
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "Upgrade.exe";//要启动的程序外部名称 
                info.Arguments = "Winform_App.exe " + currentVersion + " SampleProject 5C210AF8-6D4E-4D06-A9B7-F1E04BFE8AC3 http://localhost:56799/";
                info.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Process.Start(info);
                Application.Exit();
            }



```

自动更新的逻辑：
首先判断version和服务端是否一致，如果不一致触发自动更新
其次本地的更新程序会将Upgrade.exe所在目录下所有文件都计算hash值后提交到更新网站，通过对比hash值确定需要下载的文件，会忽略到.ignore中的文件，挨个去下载服务端提供的文件，下载完成后，退出自动更新程序，然后启动主程序。
增加更高一级的权限，解决用户将项目安装到C盘后，无法执行自动更新的问题
增加删除程序集目录冗余文件的删除操作


