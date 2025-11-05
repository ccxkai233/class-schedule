# 目标
用 C# / .NET 8 + WPF + YAML 实现一个“电子课程表”，满足：
- 支持**按日期/星期**显示课程；
- 同一时间段可定义**轮换（如 4 天轮换 408 四门课）**；
- 左侧：当天课程表（高亮当前时段/课程）；右侧：当前课程 + 当前时间 + 倒计时；
- 到点**主动弹窗提醒 + 语音提醒**；
- 所有课程与时间段**通过 YAML 配置**（带中文注释），热加载；
- UI 美观（现代化暗/亮主题、圆角、阴影、颜色可定制）；
- 支持打包为单文件 exe。

---

# 技术栈 & 依赖
- 运行时：.NET 8（Windows）
- UI：WPF（MVVM）
- YAML：YamlDotNet
- MVVM 辅助：CommunityToolkit.Mvvm（简化 INotifyPropertyChanged、RelayCommand）
- 图标/样式：MaterialDesignInXaml（MaterialDesignThemes、MaterialDesignColors）或 ModernWpf（两者择一，下面以 MaterialDesign 为例）
- 语音：System.Speech.Synthesis（Windows 自带语音引擎）
- 弹窗：WPF 自定义提醒窗（默认）；可选 Windows Toast（Microsoft.Toolkit.Uwp.Notifications）
- 日志：内置简单日志（可选 Serilog）

NuGet（建议）
```
YamlDotNet
CommunityToolkit.Mvvm
MaterialDesignThemes
MaterialDesignColors
Microsoft.Toolkit.Uwp.Notifications   # 可选，用于 Win10+ Toast
```

csproj 关键设置（示例）
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

---

# 项目结构（建议）
```
Timetable.sln
└─ Timetable.App
   ├─ App.xaml / App.xaml.cs
   ├─ MainWindow.xaml / MainWindow.xaml.cs
   ├─ Views/
   │  ├─ TodayView.xaml
   │  ├─ ReminderWindow.xaml
   ├─ ViewModels/
   │  ├─ MainViewModel.cs
   │  ├─ TodayViewModel.cs
   │  └─ SettingsViewModel.cs (可选)
   ├─ Models/
   │  ├─ AppConfig.cs
   │  ├─ Subject.cs
   │  ├─ TimeSlot.cs
   │  ├─ Rotation.cs
   │  └─ DaySchedule.cs
   ├─ Services/
   │  ├─ ConfigService.cs            # 读/写/热加载 YAML
   │  ├─ ScheduleService.cs          # 计算指定日期的课程（含轮换）
   │  ├─ NotificationService.cs      # 弹窗 & 可选 Toast
   │  ├─ SpeechService.cs            # 语音播报
   │  ├─ TimerService.cs             # DispatcherTimer 每秒 Tick
   │  └─ DateOverrideService.cs      # 节假日/临时调整（可合并到 ScheduleService）
   ├─ Resources/
   │  ├─ Themes.xaml                 # 颜色/圆角等
   │  └─ icons.svg/png
   ├─ config/
   │  └─ timetable.yaml              # 主配置（带中文注释）
   └─ Helpers/
      └─ Extensions.cs
```

---

# YAML 配置规范（含中文注释示例）
> 建议文件路径：`config/timetable.yaml`；UTF-8；修改后自动热加载。

```yaml
# 电子课程表配置（可直接修改本文件生效）
# 注意：YAML 缩进使用两个空格，不要使用 Tab。

meta:                           # 元信息
  timezone: "Asia/Shanghai"     # 时区，仅展示用
  theme: "dark"                 # dark / light
  accent_color: "#5C6BC0"       # 主色
  start_date: "2025-01-01"      # 轮换起始日期（用于取模计算）
  speech_voice: "ZH-CN"         # 语音引擎（系统默认即可）
  pre_alert_minutes: [5, 0]      # 开始前 5 分钟与开始时提醒

subjects:                       # 课程科目清单（id 用于引用）
  - { id: math,  name: "数学" }
  - { id: eng,   name: "英语" }
  - { id: pol,   name: "政治" }
  - { id: cs_ds, name: "408-数据结构" }
  - { id: cs_os, name: "408-操作系统" }
  - { id: cs_cn, name: "408-计算机网络" }
  - { id: cs_ca, name: "408-计算机组成原理" }

# 一天内的时间段定义（可自定义增删改）
# time 使用 24 小时制，闭开区间 [start, end)
slots:
  - { id: s1, start: "06:30", end: "08:00" }
  - { id: s2, start: "08:10", end: "09:40" }
  - { id: s3, start: "10:00", end: "11:30" }
  - { id: s4, start: "14:00", end: "15:30" }
  - { id: s5, start: "15:40", end: "17:10" }
  - { id: s6, start: "19:00", end: "20:30" }

# 轮换组：同一时间段按天轮换的课程顺序
# 例：四天轮换 408 四门课，顺序固定为 ds -> os -> cn -> ca
rotations:
  - name: cs_cycle
    subjects: [cs_ds, cs_os, cs_cn, cs_ca]  # 长度 = 周期
    start_date: "2025-01-01"                # 单独设置可覆盖 meta.start_date
    skip_weekend: true                      # true=遇周末不计入轮换（只在工作日推进）

# 基础周计划：按星期定义每个时间段默认课程（subject 或 rotation）
# 周一=1，周日=7
weekly_plan:
  1:   # 周一
    s1: eng
    s2: { rotation: cs_cycle }
    s3: math
    s4: pol
    s5: eng
    s6: { rotation: cs_cycle }
  2:   # 周二
    s1: math
    s2: { rotation: cs_cycle }
    s3: eng
    s4: pol
    s5: math
    s6: { rotation: cs_cycle }
  3:   # 周三
    s1: pol
    s2: { rotation: cs_cycle }
    s3: math
    s4: eng
    s5: pol
    s6: { rotation: cs_cycle }
  4:   # 周四
    s1: eng
    s2: { rotation: cs_cycle }
    s3: pol
    s4: math
    s5: eng
    s6: { rotation: cs_cycle }
  5:   # 周五
    s1: math
    s2: { rotation: cs_cycle }
    s3: eng
    s4: pol
    s5: math
    s6: { rotation: cs_cycle }
  6:   # 周六（可轻量安排或留空）
    s3: { rotation: cs_cycle }
  7:   # 周日（休息）

# 特定日期的临时覆盖（优先级最高），例如法定假日、模拟考试等
# date-only=全天；也可指定单个 slot
date_overrides:
  - date: "2025-05-01"                # 劳动节放假，全天无课
    all_day_off: true
  - date: "2025-06-15"
    slots:
      s2: math                         # 周计划本来是轮换，这天改为数学
      s6: { subject: pol, note: "晚间政治专项" }
```

---

# 业务规则（核心算法）
1. **当前时段判定**：按本地日期 + `slots` 闭开区间依次比对，命中即为当前时段；若未命中，则处于“未开始/已结束/空档”。
2. **轮换计算**：
   - 取 `rotation.start_date`（若未设置用 `meta.start_date`）。
   - 计算从起始到目标日期的“有效推进天数 N”。
     - 如果 `skip_weekend=true`：仅累计工作日（Mon-Fri）；
     - 否则：自然天数。
   - 索引 = `N % subjects.Count`，取出当天应上的科目；
   - 允许在 weekly_plan 中同一 `rotation` 出现在多个 slot（同一天该 rotation 结果一致）。
3. **优先级**：`date_overrides` > `weekly_plan`；当某 slot 未定义，则该 slot 为空。
4. **提醒**：根据 `pre_alert_minutes` 生成提醒点（如 T-5 分，T-0），对每个 slot 安排一次（同一 slot 当天只提醒一次）。

---

# 运行时流程（简化）
1. App 启动 → `ConfigService` 读取 YAML → `ScheduleService` 生成“今日课程表”。
2. `TimerService`（DispatcherTimer, 1s）每秒触发：
   - 刷新当前时间、倒计时；
   - 若跨 slot 边界 → 重算“今日课程表”与高亮；
   - 检查是否到达提醒点 → 调用 `NotificationService` 弹窗 + `SpeechService` 播报。
3. `ConfigService` 使用 `FileSystemWatcher` 监听 `timetable.yaml` 变化，触发热加载与 UI 刷新。

---

# UI 设计与交互
- **主窗体**：1280×800（可缩放），左右两栏。
  - 左栏（`TodayView`）：
    - 列表展示今日各 `slot`（时间 + 课程名 + 备注）；
    - “当前时段”卡片高亮（主色、进度条，左侧时间线）；
    - 空档段显示“自习/休息”。
  - 右栏（信息面板）：
    - 当前课程（大字标题）
    - 当前时间（HH:mm:ss）
    - 距离本段结束的倒计时（mm:ss）
    - 下一节预告（时间 + 课程）
    - 顶部显示当天日期与星期（yyyy-MM-dd EEEE）
- **提醒窗**（`ReminderWindow`）：小弹窗，显示“课程名 + 时段 + 操作按钮（知道了/稍后提醒）”，默认 8 秒自动淡出；伴随 TTS 播报。
- **主题/颜色**：读取 `meta.theme` / `meta.accent_color`；MaterialDesign 的 Palette 统一管理。

---

# 数据模型（简化示例）
```csharp
public record Subject(string Id, string Name);
public record TimeSlot(string Id, TimeSpan Start, TimeSpan End);
public record Rotation(string Name, List<string> Subjects, DateOnly StartDate, bool SkipWeekend);

public class AppConfig {
  public Meta Meta { get; set; } = new();
  public List<Subject> Subjects { get; set; } = new();
  public List<TimeSlotConfig> Slots { get; set; } = new();
  public List<RotationConfig> Rotations { get; set; } = new();
  public Dictionary<int, Dictionary<string, PlanEntry>> WeeklyPlan { get; set; } = new();
  public List<DateOverride> DateOverrides { get; set; } = new();
}

public class PlanEntry { public string? Subject { get; set; } public string? Rotation { get; set; } }
public class DateOverride { public DateOnly Date { get; set; } public bool AllDayOff { get; set; } public Dictionary<string, PlanEntry>? Slots { get; set; } }
```

---

# 服务设计（关键接口）
```csharp
public interface IConfigService {
  AppConfig Config { get; }
  event EventHandler? ConfigReloaded;  // 热加载
  void Load(string path);
}

public interface IScheduleService {
  DaySchedule BuildSchedule(DateOnly date); // 结合 weekly + overrides + rotation
  PlanEntry? ResolveEntry(DateOnly date, string slotId);
}

public interface INotificationService {
  void ShowInAppReminder(string title, string message);
  void ShowToast(string title, string message); // 可选
}

public interface ISpeechService { void SpeakAsync(string text); }
```

---

# 关键代码片段
**1) 计算轮换索引**
```csharp
static int CountBusinessDays(DateOnly from, DateOnly to) {
  int days = 0; for (var d = from; d < to; d = d.AddDays(1)) {
    var dow = d.DayOfWeek; if (dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday) days++;
  } return days;
}

string ResolveRotationSubject(Rotation rot, DateOnly target) {
  var start = rot.StartDate;
  int n = rot.SkipWeekend ? CountBusinessDays(start, target) : target.DayNumber - start.DayNumber;
  if (n < 0) n = 0; // 目标在起始日前则取首科目
  return rot.Subjects[n % rot.Subjects.Count];
}
```

**2) 读取 YAML + 热加载**
```csharp
var deserializer = new DeserializerBuilder()
  .WithNamingConvention(UnderscoredNamingConvention.Instance)
  .Build();
Config = deserializer.Deserialize<AppConfig>(File.ReadAllText(path));

var watcher = new FileSystemWatcher(Path.GetDirectoryName(path)!, Path.GetFileName(path)!)
{ NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size };
watcher.Changed += (_, __) => { Thread.Sleep(100); Reload(); };
watcher.EnableRaisingEvents = true;
```

**3) 语音提醒**（System.Speech）
```csharp
using System.Speech.Synthesis;
class SpeechService : ISpeechService {
  private readonly SpeechSynthesizer _s = new();
  public SpeechService(string? voice) { if (!string.IsNullOrWhiteSpace(voice)) _s.SelectVoiceByHints(VoiceGender.NotSet, VoiceAge.NotSet, 0, new System.Globalization.CultureInfo(voice)); }
  public void SpeakAsync(string text) => _s.SpeakAsync(text);
}
```

**4) WPF 布局（MainWindow.xaml 摘要）**
```xml
<materialDesign:PaletteHelper />
<Grid Margin="16" ColumnDefinitions="3*,2*">
  <!-- 左：今日课程表 -->
  <ListBox Grid.Column="0" ItemsSource="{Binding TodaySlots}" SelectedItem="{Binding CurrentSlot}">
    <ListBox.ItemTemplate>
      <DataTemplate>
        <Border CornerRadius="12" Padding="12" Margin="0,8"
                Background="{Binding IsCurrent, Converter={StaticResource BoolToAccentBrush}}">
          <StackPanel>
            <TextBlock Text="{Binding TimeRange}" FontSize="14"/>
            <TextBlock Text="{Binding SubjectName}" FontSize="18" FontWeight="SemiBold"/>
            <ProgressBar Value="{Binding ProgressPercent}" Height="4"/>
          </StackPanel>
        </Border>
      </DataTemplate>
    </ListBox.ItemTemplate>
  </ListBox>

  <!-- 右：当前课程/时间/倒计时 -->
  <StackPanel Grid.Column="1" Margin="24">
    <TextBlock Text="{Binding NowDateText}" FontSize="20"/>
    <TextBlock Text="{Binding NowTimeText}" FontSize="48" FontWeight="Bold"/>
    <TextBlock Text="{Binding CurrentSubjectName}" FontSize="28"/>
    <TextBlock Text="{Binding CountdownText}" FontSize="24"/>
    <Separator Margin="0,12"/>
    <TextBlock Text="{Binding NextSubjectText}" FontSize="18"/>
  </StackPanel>
</Grid>
```

**5) 倒计时与跨段切换（伪代码）**
```csharp
void OnTick() {
  Now = DateTime.Now;
  var slot = FindCurrentSlot(Now.TimeOfDay);
  if (slot != CurrentSlot) { CurrentSlot = slot; RebuildToday(); }
  UpdateCountdown(slot);
  CheckAndFireReminders(Now);
}
```

**6) 弹窗提醒（示例）**
```csharp
public void ShowInAppReminder(string title, string message) {
  var w = new ReminderWindow { DataContext = new ReminderVM(title, message) };
  w.Show(); w.Activate();
}
```

---

# 开发步骤（从零到可运行）
1. `dotnet new wpf -n Timetable.App -f net8.0-windows10.0.19041.0`
2. 添加 NuGet：YamlDotNet、CommunityToolkit.Mvvm、MaterialDesignThemes/Colors（可视化主题）。
3. 落地 Models & Services（先实现 `ConfigService`、`ScheduleService`、`SpeechService`、`NotificationService`）。
4. 写 `timetable.yaml`（复制上面的示例），启动时加载；实现热加载。
5. ViewModels 绑定：`TodayViewModel` 生成 `TodaySlots`（包含 IsCurrent、ProgressPercent、Countdown）。
6. XAML 框架 + 样式：主题色、字体、圆角、阴影、ItemTemplate。
7. 定时器逻辑与提醒落地；语音播报“现在开始 {课程名}，时间 {HH:mm} 到 {HH:mm}”。
8. 可选：Windows Toast（Toolkit Desktop 示例）；
9. 完成测试用例与边界条件（详见下）。

---

# 测试用例（要点）
- **轮换**：设置 `start_date` 为周五，`skip_weekend=true`，验证跨周末后顺序正确；
- **跨午夜**：23:00-00:30 时间段（如自习）能正确计算当前段与倒计时；
- **覆盖**：`date_overrides` 对特定 slot 生效，且 `all_day_off` 时当天为空；
- **提醒去重**：同一 slot 只提醒一次；修改 YAML 后重新计算提醒队列；
- **热加载**：保存 YAML 后 UI 在 200ms 内刷新（watcher 防抖处理）。

---

# 打包为 exe
**单文件自包含（推荐）**
```
dotnet publish -c Release -r win-x64 \
  /p:PublishSingleFile=true \
  /p:IncludeNativeLibrariesForSelfExtract=true \
  /p:PublishTrimmed=false \
  --self-contained true
```
输出目录：`bin/Release/net8.0-windows10.0.19041.0/win-x64/publish/Timetable.App.exe`

**可选安装包**
- MSIX（签名更方便系统集成/Toast）；
- Squirrel / WiX Toolset（制作安装向导、开机自启、桌面快捷方式）。

---

# 可扩展点
- 多配置文件合并（工作日/周末/假期方案）；
- UI 日期切换器：可查看“任意日期”的课程；
- 倒计时完成自动切换休息番茄钟；
- 导出/导入配置；
- 云端同步（如 WebDAV/GitHub Gist）。

---

# 里程碑（建议）
- M0（0.5 天）：项目脚手架 + NuGet + YAML 读写
- M1（1 天）：轮换算法 + Weekly/Override 生效 + 今日列表
- M2（1 天）：提醒/语音 + 热加载 + 倒计时
- M3（0.5 天）：主题与视觉优化
- M4（0.5 天）：打包与文档

> 以上规划已包含可直接落地的模型、配置、算法与代码骨架。按步骤推进即可完成最小可用版本（MVP），随后按扩展点逐步增强。

