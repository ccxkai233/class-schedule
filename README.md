# WPF 电子课程表 (WPF Timetable)

一个基于 C# / .NET 8 和 WPF 实现的高度可配置的电子课程表桌面应用。

## ✨ 功能特性

- **高度可配置**：所有课程、时间段、轮换规则均通过外部 `timetable.yaml` 文件配置，无需修改代码。
- **实时热加载**：修改并保存 `timetable.yaml` 后，课程表界面会自动刷新。
- **灵活的轮换系统**：支持按天轮换课程，并可自定义跳过任意星期（如只跳过周日）。
- **提醒功能**：在课程开始前、开始时和结束时，通过弹窗和语音进行提醒。
- **自定义界面**：实现了自定义的深色无边框窗口。
- **系统托盘**：支持关闭到系统托盘，并通过右键菜单彻底退出。

## 🚀 如何使用

### 1. 直接运行 (推荐)

1.  前往 `Timetable.App/bin/Release/net10.0-windows/win-x64/publish/` 目录。
2.  将整个 `publish` 文件夹复制到您希望的任何位置。
3.  双击运行 `Timetable.App.exe`。
4.  通过修改 `publish` 文件夹内的 `config/timetable.yaml` 文件来自定义您的课程表。

### 2. 从源码运行

1.  确保您已安装 [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 或更高版本。
2.  克隆本仓库。
3.  在项目根目录下打开终端，运行以下命令：
    ```bash
    dotnet run --project Timetable.App
    ```
4.  通过修改项目内的 `Timetable.App/config/timetable.yaml` 文件来自定义您的课程表。

## ⚙️ 如何配置

打开 `config/timetable.yaml` 文件，您可以修改以下部分：

-   `meta`: 全局设置，如提醒时间、是否在提醒时弹出主窗口等。
-   `subjects`: 定义所有课程的 `id` 和 `name`。
-   `slots`: 定义一天中所有可用时间段的 `id`、`start` 和 `end` 时间。
-   `rotations`: 定义课程轮换组。
    -   `subjects`: 轮换的课程 `id` 列表。
    -   `skip_days`: 一个列表，包含希望跳过的星期（`Sunday`, `Monday` 等）。
-   `weekly_plan`: 为周一到周日 (`1` 到 `7`) 的每个 `slot` 分配一个固定的 `subject` 或一个 `rotation`。
-   `date_overrides`: 用于临时覆盖某一天或某几个时间段的课程安排，优先级最高。

文件中有详细的中文注释，您可以根据需要进行调整。