---
AIGC:
    ContentProducer: Minimax Agent AI
    ContentPropagator: Minimax Agent AI
    Label: AIGC
    ProduceID: "00000000000000000000000000000000"
    PropagateID: "00000000000000000000000000000000"
    ReservedCode1: 3045022100c5d34c3e5aee23cc87491156df2f08e41370400c3952c68a347156cf8ce3a7e80220283ed6c6299c2b7390f5f2329ce714e42d6bace222eb5a7bc1943721ee71f0f7
    ReservedCode2: 304502205a15cbacae898a6a3377d920c7b53dffae90d835cf107a7673f087ccd895a4ba022100e711372851c50262b92263ac8271cba49bd25da50956e4ffbd61d1c46be16cb8
---

# 智能便签 - SmartNotes

一款集成了豆包大模型 AI 的 Windows 桌面便签应用，采用简洁圆润的 UI 设计。

## 功能特点

- **便签管理**：创建、编辑、删除、置顶便签
- **分类管理**：支持按分类筛选便签（默认、工作、生活、学习、其他）
- **搜索功能**：快速搜索便签标题和内容
- **AI 计划助手**：使用豆包大模型生成详细计划
- **本地存储**：便签数据自动保存到本地

## 系统要求

- Windows 10 或更高版本
- .NET 8.0 Runtime
- 豆包大模型 API 密钥

## 安装与运行

### 方法一：使用 Visual Studio（推荐）

1. 确保已安装 **Visual Studio 2022** 或更高版本
2. 安装 **.NET 桌面开发** 工作负载
3. 打开 `SmartNotes.sln` 解决方案文件
4. 按 `F5` 或点击"启动"按钮运行

### 方法二：使用命令行

```bash
cd SmartNotes
dotnet restore
dotnet run
```

### 发布为可执行文件

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

生成的 exe 文件位于 `bin/Release/net8.0-windows/win-x64/publish/` 目录。

## 获取豆包 API 密钥

1. 访问[火山引擎控制台](https://console.volcengine.com/)
2. 注册/登录账号
3. 开通**豆包大模型**服务
4. 在 API Key 管理页面创建新的 API Key
5. 复制 API Key 并在应用设置中配置

## 使用说明

### 基本操作

1. **新建便签**：点击左侧"新建便签"按钮
2. **编辑便签**：在右侧编辑器中修改标题和内容
3. **保存便签**：点击"保存"按钮或自动保存
4. **删除便签**：点击"删除"按钮确认删除
5. **置顶便签**：点击"置顶"按钮将便签置顶

### AI 计划助手

1. 点击左侧"AI 助手"按钮
2. 在弹窗中描述您的需求
3. 点击"生成计划"按钮
4. AI 将为您生成详细计划并保存为便签

### 分类管理

- 点击左侧分类标签筛选便签
- 在便签编辑器中更改分类

### 设置

1. 点击左下角"设置"按钮
2. 输入您的豆包 API 密钥
3. API 地址默认已填充，可按需修改
4. 点击"保存"保存设置

## 数据存储

便签数据存储在：
```
%APPDATA%\SmartNotes\notes.json
```

设置数据存储在：
```
%APPDATA%\SmartNotes\settings.json
```

## 技术栈

- **框架**：.NET 8.0 + WPF
- **MVVM**：CommunityToolkit.Mvvm
- **HTTP 客户端**：System.Net.Http
- **JSON 序列化**：System.Text.Json

## 项目结构

```
SmartNotes/
├── SmartNotes.csproj      # 项目文件
├── App.xaml               # 应用入口
├── App.xaml.cs
├── MainWindow.xaml        # 主窗口 UI
├── MainWindow.xaml.cs
├── Converters.cs          # 数据转换器
├── Models/
│   └── Note.cs           # 便签数据模型
├── Services/
│   ├── DoubaoApiService.cs    # 豆包 API 服务
│   └── NoteStorageService.cs  # 本地存储服务
├── ViewModels/
│   └── MainViewModel.cs   # 主窗口 ViewModel
└── README.md              # 说明文档
```

## 注意事项

- 请妥善保管您的 API 密钥，不要泄露给他人
- AI 生成的内容仅供参考，请根据实际情况判断使用
- 建议定期备份您的便签数据

## 许可证

MIT License
