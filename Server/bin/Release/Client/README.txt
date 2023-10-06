这个DLL文件不能用来注入（还没写好）
只能用来内置到程序中，如果你编写了一个程序
可以引入这个DLL
它有一个NovaRATClient的类
其中有一个Start(string ip,int port)的函数，用来连接服务器

ClientLib.exe需要提供两个参数
使用cmd命令
ClientLib.exe 127.0.0.1 12345连接到服务器
并且内置了自启动命令
使用请加壳

本地IP：loca.novapp.icu
服务器开启默认端口12345