using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using GaugeOMatic.Config;
using GaugeOMatic.JobModules;
using GaugeOMatic.Trackers;
using GaugeOMatic.Trackers.Presets;
using GaugeOMatic.Utility;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static Dalamud.Interface.Components.ImGuiComponents;
using static Dalamud.Interface.FontAwesomeIcon;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.Windows.ConfigWindow;

namespace GaugeOMatic.Windows;

public class PresetWindow : Window, IDisposable
{
    public TrackerManager TrackerManager;
    public Configuration Configuration;

    public PresetWindow(TrackerManager trackerManager) : base("Gauge-O-Matic 預設")
    {
        TrackerManager = trackerManager;
        Configuration = TrackerManager.Configuration;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new(250f, 350f),
            MaximumSize = new(800f, 1100f)
        };

        UIData.PresetList = BuildPresetList();
    }

    internal static PresetUIData UIData = new();

    public List<Preset> BuildPresetList() =>
        [.. PluginPresets.Presets.Concat(Configuration.SavedPresets).OrderBy(static p => p.Name)];

    public override void Draw()
    {
        var module = GetModuleForTab(Configuration.JobTab, TrackerManager.JobModules);
        if (module != null) DrawPresetWindow(module);
    }

    public void Dispose() { }

    public void DrawPresetWindow(JobModule module)
    {
        PresetListUI(module);

        ImGui.Spacing();
        ImGui.Spacing();

        PresetAddUI(module);
    }

    public void SetUpRenamePopup(ref Preset preset)
    {
        using var pop = ImRaii.Popup("Rename");
        if (pop.Success)
        {
            var name = UIData.NewPresetName ?? preset.Name;
            ImGui.SetNextItemWidth(180f * GlobalScale);
            if (ImGui.InputText("##Name", ref name, 40))
                UIData.NewPresetName = string.IsNullOrWhiteSpace(name) ? preset.Name : name;

            ImGui.SameLine();
            if (ImGui.Button("重新命名##Save"))
            {
                Configuration.SavedPresets.Remove(preset);
                preset.Name = UIData.NewPresetName ?? preset.Name;
                Configuration.SavedPresets.Add(preset);
                UIData.PresetList = BuildPresetList();
                Configuration.Save();
                UIData.NewPresetName = null;
                ImGui.CloseCurrentPopup();
            }
        }
    }

    public void PresetListUI(JobModule module)
    {
        using var gr = ImRaii.Group();

        if (gr.Success)
        {
            var filter = Configuration.PresetFiltering;

            ImGui.SameLine();
            if (ImGui.RadioButton("全部##All", ref filter, 0))
            {
                Configuration.PresetFiltering = 0;
                Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGui.RadioButton("僅相同職能##RoleOnly", ref filter, 1))
            {
                Configuration.PresetFiltering = 1;
                Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGui.RadioButton("僅相同職業##JobOnly", ref filter, 2))
            {
                Configuration.PresetFiltering = 2;
                Configuration.Save();
            }

            var presetList = new List<Preset>(UIData.PresetList.Where(
                                                  p => filter == 0 || p.Trackers.Any(t => filter switch
                                                  {
                                                      1 => t.JobRoleMatch(module),
                                                      2 => t.JobMatch(module),
                                                      _ => true
                                                  })));
            var presetNames = presetList.Select(static p => p.Name).ToArray();
            var selectedIndex = Math.Clamp(UIData.PresetSelectedIndex, 0, Math.Max(0, presetList.Count - 1));
            ImGui.SetNextItemWidth(200f * GlobalScale);
            if (ImGui.ListBox("##Presets",ref selectedIndex,presetNames,presetList.Count))
            {
                UIData.PresetSelectedIndex = selectedIndex;
            }

            if (presetList.Count > 0)
            {
                var selectedPreset = presetList[selectedIndex];

                ImGui.SameLine();
                using var gr2 = ImRaii.Group();
                if (gr2.Success)
                {
                    var builtIn = selectedPreset.BuiltIn;

                    if (builtIn) ImGuiHelpy.IconButtonDisabled(Edit);
                    else
                    {
                        SetUpRenamePopup(ref selectedPreset);
                        if (IconButton(Edit)) ImGui.OpenPopup("Rename");
                    }

                    ImGui.SameLine();

                    if (builtIn) ImGuiHelpy.IconButtonDisabled(TrashAlt);
                    else if (IconButton(TrashAlt))
                    {
                        Configuration.SavedPresets.Remove(selectedPreset);
                        UIData.PresetList = BuildPresetList();
                        Configuration.Save();
                    }

                    ImGui.SameLine();
                    if (IconButtonWithText(SignOutAlt, "匯出至剪貼簿##ExportClipboard"))
                    {
                        ImGui.SetClipboardText(selectedPreset.ExportStr());
                    }

                    ImGui.Spacing();
                    DisplayPresetContents(module, selectedPreset);
                    ImGui.Spacing();
                    if (IconButtonWithText(Plus, $"全部加入至 {module.Abbr}##AddAll"))
                        ApplyPreset(module, selectedPreset.Clone());
                    ImGui.SameLine();
                    if (IconButtonWithText(PaintRoller, "覆寫目前設定##OverwriteCurrent"))
                        ApplyPreset(module, selectedPreset.Clone(), true);
                }
            }
        }
    }

    private static void DisplayPresetContents(JobModule module, Preset selectedPreset)
    {
        using var table = ImRaii.Table("PresetInfo", 2, ImGuiTableFlags.SizingFixedFit);
        if (table.Success)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(new Vector4(1, 1, 1, 0.6f), "追蹤器");

            ImGui.TableNextColumn();
            ImGui.TextColored(new Vector4(1, 1, 1, 0.6f), "元件");

            foreach (var trackerConfig in selectedPreset.Trackers)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                AddTrackerButton(trackerConfig);
                ImGui.SameLine();

                ImGuiHelpy.DrawGameIcon(trackerConfig.GetDisplayAttr().GameIcon, 22f);
                if (ImGui.IsItemHovered()) trackerConfig.DrawTooltip();

                ImGui.TextColored(trackerConfig.JobRoleMatch(module) ? new(1) : new Vector4(1, 1, 1, 0.3f),
                                  UiText.Translate(trackerConfig.GetDisplayAttr().Name));
                if (ImGui.IsItemHovered()) trackerConfig.DrawTooltip();

                ImGui.TableNextColumn();
                CopyWidgetButton(trackerConfig);

                if (trackerConfig.WidgetType != null)
                {
                    ImGui.SameLine();
                    ImGui.Text(UiText.Translate(trackerConfig.WidgetDisplayName));
                }
            }
        }

        return;

        static void CopyWidgetButton(TrackerConfig trackerConfig)
        {
            if (IconButton($"CopyWidget{trackerConfig.GetHashCode()}", Copy))
            {
                WidgetClipType = trackerConfig.WidgetType;
                WidgetClipboard = trackerConfig.WidgetConfig;
                ImGui.SetClipboardText(WidgetClipboard);
            }

            if (ImGui.IsItemHovered()) ImGui.SetTooltip("複製元件設定");
        }

        void AddTrackerButton(TrackerConfig trackerConfig)
        {
            if (IconButton($"##add{trackerConfig.GetHashCode()}", Plus))
            {
                var newConfig = trackerConfig.Clone();
                if (newConfig != null) module.AddTrackerConfig(newConfig);
                else module.AddBlankTracker();
            }

            if (ImGui.IsItemHovered()) ImGui.SetTooltip("新增 " + UiText.Translate(trackerConfig.GetDisplayAttr().Name));
        }
    }

    public static void ApplyPreset(JobModule module, Preset preset, bool replace = false)
    {
        var list = preset.Trackers;
        if (replace) module.TrackerConfigList = list;
        else
        {
            var currentLen = module.TrackerConfigList.Length;
            var newList = new TrackerConfig[currentLen + list.Length];

            module.TrackerConfigList.CopyTo(newList, 0);
            list.CopyTo(newList, currentLen);

            for (var i = 0; i < newList.Length; i++) newList[i].Index = i;

            module.TrackerConfigList = newList;
        }

        module.RebuildTrackerList();
        module.Save();
    }

    private void PresetAddUI(JobModule module)
    {
        using var gr = ImRaii.Group();
        if (gr.Success)
        {
            ImGui.TextColored(new Vector4(1, 1, 1, 0.6f), "新增預設");
            var saveName = UIData.SaveName;
            ImGui.Text($"將目前的 {module.Abbr} 追蹤器儲存為預設：");
            ImGui.SetNextItemWidth(200f * GlobalScale);
            if (ImGui.InputTextWithHint("##SaveName", "新預設名稱", ref saveName, (int)30u))
                UIData.SaveName = saveName;
            ImGui.SameLine();
            if (IconButtonWithText(Save, "儲存##SavePreset")) SaveNewPreset(module, saveName);

            ImGui.Spacing();
            ImGui.Text("從剪貼簿匯入預設：");
            if (IconButtonWithText(SignInAlt, "從剪貼簿匯入##ImportClipboard")) ImportNewPreset(ImGui.GetClipboardText());
        }
    }

    private void SaveNewPreset(JobModule module, string saveName)
    {
        var newPreset = new Preset(module.TrackerConfigList, saveName.Length == 0 ? "新預設" : saveName);
        var hash = newPreset.ExportStr().GetHashCode();

        var dup = false;
        foreach (var preset in UIData.PresetList)
        {
            if (preset.ExportStr().GetHashCode() != hash) continue;

            dup = true;
            break;
        }

        if (!dup)
        {
            Configuration.SavedPresets.Add(newPreset);
            UIData.PresetList = BuildPresetList();
            Configuration.Save();
        }
    }

    private void ImportNewPreset(string importString)
    {
        try
        {
            var hash = importString.GetHashCode();
            var dup = false;
            foreach (var preset in UIData.PresetList)
            {
                if (preset.ExportStr().GetHashCode() != hash) continue;

                UIData.ImportHintText = "此預設已存在！";
                dup = true;
                break;
            }

            if (!dup)
            {
                var importResult = new Preset(importString, true);
                if (importResult.Trackers.Length > 0)
                {
                    if (importResult.Name.Length == 0) importResult.Name = "新預設";
                    Configuration.SavedPresets.Add(importResult);
                    UIData.PresetList = BuildPresetList();
                    Configuration.Save();
                    UIData.ImportHintText = "匯入完成！";
                }
            }
        }
        catch (Exception ex)
        {
            UIData.ImportHintText = "輸入內容無效！";
            Log.Error(ex.Message);
        }

        UIData.ImportString = "";
    }

    public struct PresetUIData
    {
        public string? NewPresetName = null;
        public List<Preset> PresetList = [];
        public int PresetSelectedIndex = 0;
        public string ImportHintText { get; set; } = "貼到這裡";
        public string ImportString { get; set; } = "";
        public string SaveName = "";

        public PresetUIData() { }
    }
}
