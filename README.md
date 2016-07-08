# Rabbit Rpc
一个轻量级跨平台的Rpc。
## 特性
1. Apache License 2.0协议开源
2. 支持客户端负载均衡（提供了轮询、随机算法的实现）
3. 支持ZooKeeper和文件共享形式的服务协调
4. 运行时客户端代理生成（基于Roslyn）
5. 预生成客户端代理
6. 客户端代理预生成（基于Roslyn）
7. 抽象的编解码器（提供了JSON、ProtoBuffer协议的实现）
8. 抽象的传输通道（提供了DotNetty与Cowboy.Sockets的移植实现）
9. 异常信息传递（服务端运行时的本地异常可以传递至客户端）
10. **NET Core项目架构**
11. **跨平台运行**

## 项目概况
![](http://images2015.cnblogs.com/blog/384997/201607/384997-20160708082111186-595090265.png)
### Rabbit.Rpc（支持跨平台）
1. Rpc核心类库，有如下功能：
2. 服务Id生成
3. 传输消息模型
4. 类型转换
5. 服务路由抽象
6. 序列化器抽象（默认提供JSON序列化器）
7. 传输抽象
8. 编解码器抽象（默认提供JSON的编解码器实现）
9. 客户端运行时（地址解析器、地址选择器，远程调用服务）
10. 服务端运行时（服务条目管理、服务执行器、服务发现抽象、RpcServiceAttribute标记服务发现实现）

### Rabbit.Rpc.ProxyGenerator（支持跨平台）
服务代理生成器，提供的功能：

1. 服务代理实现生成
2. 服务代理实例创建

### extensions（相关扩展）
#### Rabbit.Rpc.Codec.ProtoBuffer（支持跨平台）
ProtoBuffer协议的编解码器实现。

#### Rabbit.Rpc.Coordinate.Zookeeper（支持跨平台）
基于ZooKeeper的服务路由管理。

#### Rabbit.Transport.DotNetty（暂不支持跨平台）
基于DotNetty的传输实现。

_ps:官方以有将DotNetty支持NET Core的计划，大伙可以再等等，待官方支持后，会尽快进行适配。_

#### Rabbit.Transport.Simple（支持跨平台）
由于DotNetty不支持跨平台运行，为了让rpc能在其它平台上跑通，故移植了“Cowboy.Sockets”实现了一个简单的传输实现。

### tools
#### Rabbit.Rpc.Tests
单元测试项目。

#### Rabbit.Rpc.ClientGenerator（支持跨平台）
预生产服务代理的工具，提供了如下功能：

1. 生成服务代理实现代码文件
2. 生成服务代理实现程序集文件

## 性能测试
测试环境

OS | CPU | 内存 | 硬盘 | 网络环境 | 虚拟机
------------ | ------------- | ------------- | ------------- | ------------- | -------------
Windows 10 x64 | I7 3610QM | 16GB | SSD | 127.0.0.1 | 否
Ubuntu 16.04 x64 | I7 3610QM | 4GB | SSD | 127.0.0.1 | 是

### Windows10 + NETCoreApp1.0 + JSON协议 + Simple传输
![](http://images2015.cnblogs.com/blog/384997/201607/384997-20160708082114249-1409569407.png)
### Windows10 + NETCoreApp1.0 + ProtoBuffer协议 + Simple传输
![](http://images2015.cnblogs.com/blog/384997/201607/384997-20160708082117733-1064697075.png)
### Ubuntu16.04-x64 + NETCoreApp1.0 + JSON协议 + Simple传输
![](http://images2015.cnblogs.com/blog/384997/201607/384997-20160708082119405-1980756077.png)
### Windows10 + NETCoreApp1.0 + JSON协议 + Simple传输
![](http://images2015.cnblogs.com/blog/384997/201607/384997-20160708082123889-1516684603.png)

## 相关文章
* [拥抱.NET Core，跨平台的轻量级RPC：Rabbit.Rpc](http://www.cnblogs.com/ants/p/5652132.html)
* [.NET轻量级RPC框架：Rabbit.Rpc](http://www.cnblogs.com/ants/p/5605754.html)

## 交流方式
* [QQ群：384413261（RabbitHub）](http://jq.qq.com/?_wv=1027&k=29DzAfj)
* [Email：majian159@live.com](mailto:majian159@live.com)
