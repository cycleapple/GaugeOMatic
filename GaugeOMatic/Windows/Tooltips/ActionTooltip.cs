using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Bindings.ImGui;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.GameData.ActionFlags;
using static GaugeOMatic.GameData.ActionRef.BarType;
using static GaugeOMatic.GaugeOMatic;
using static GaugeOMatic.Utility.ImGuiHelpy;

namespace GaugeOMatic.GameData;

public partial class ActionRef
{
    public Dictionary<ActionFlags, string> FlagNames = new()
    {
        { LongCooldown, "冷卻" },
        { HasCharges, "可用次數" },
        { ComboBonus, "連擊" },
        { Unassignable, "無法配置" },
        { RequiresStatus, "依狀態判定" },
        { CanGetAnts, "條件式" },
        { RoleAction, "職能技能" }
    };

    public static IDalamudTextureWrap? FrameTex => TextureProvider.GetFromFile(Path.Combine(PluginDirPath, @"TextureAssets\iconFrame.png"))
                                                                  .GetWrapOrDefault();

    public override void TooltipHeaderText()
    {
        MulticolorText((Plain, NameChain), (Disabled, $" [{ID}]"));
        ImGui.TextDisabled(string.Join(", ", FlagNames.Where(f => Flags.HasFlag(f.Key)).Select(static f => f.Value)));
    }

    public override void DrawTooltipIcon(Vector2 startPos)
    {
        var texture = GetAdjustedAction().GetIconTexture();

        var frameTex = FrameTex;
        if (texture != null && frameTex != null)
        {
            ImGui.Image(texture.Handle, new(40 * GlobalScale));
            ImGui.SetCursorPos(startPos - (new Vector2(4, 3) * GlobalScale));

            ImGui.Image(frameTex.Handle, new(48 * GlobalScale));
            ImGui.SameLine();
        }

        ImGui.SetCursorPos(new(startPos.X + (50 * GlobalScale),
                               startPos.Y + (2 * GlobalScale)));
    }

    public override bool UseCounterAsState() => !HasFlag(HasCharges);

    public override void PrintBarTimerDesc()
    {
        var barType = ((LongCooldown & Flags) != 0) switch
        {
            false when (Flags & RequiresStatus) != 0 && ReadyStatus != null => StatusTimer,
            false when (Flags & ComboBonus) != 0 => ComboTimer,
            _ => Cooldown
        };

        if (barType == StatusTimer)
        {
            ImGui.TextColored(Plain, "顯示");
            ImGui.SameLine(0,3);
            if (ReadyStatus?.Icon != null)
            {
                DrawGameIcon(ReadyStatus.Icon.Value, ImGui.GetFontSize() / GlobalScale);
                ImGui.SameLine(0,3);
            }
            ImGui.TextColored(Yellow, ReadyStatus?.Name ?? "?");
            ImGui.SameLine(0,3);
            ImGui.TextColored(Plain, "的計時器");
        }
        else if (barType == ComboTimer) ImGui.Text("顯示此技能的剩餘連擊時間");
        else ImGui.Text($"顯示剩餘復唱時間（{LastKnownCooldown} 秒）");
    }

    public override void PrintCounterDesc() => ImGui.Text($"顯示可用次數（{GetMaxCharges()}）");

    public override void PrintStateDesc() => ImGui.Text("顯示是否可用");

    public override void FooterContents()
    {
        var readyStatus = ReadyStatus?.Name ?? "?";
        ImGui.TextDisabled("可用條件");

        if (HasFlag(TransformedButton)) MulticolorText((Plain, "•"), (Orange, GetBaseAction().Name), (Plain, "???????"));
        if (HasFlag(RequiresStatus) && readyStatus.Length > 1)
        {

            ImGui.TextColored(Plain, "•");
            ImGui.SameLine(0,3);
            if (ReadyStatus?.Icon != null)
            {
                DrawGameIcon(ReadyStatus.Icon.Value, ImGui.GetFontSize() / GlobalScale);
                ImGui.SameLine(0,3);
            }
            MulticolorText((Yellow, readyStatus), (Plain, "???"));
        }

        if (HasFlag(ComboBonus)) ImGui.Text("• ????????????");
        else if (HasFlag(CanGetAnts)) ImGui.Text("• ?????????");

        if (HasFlag(HasCharges)) ImGui.Text("• ?????????");
        else if (HasFlag(LongCooldown)) ImGui.Text("• ????????");

        if (HasFlag(CostsMP)) ImGui.Text($"• ????????????{GetActionCost()}?");
    }
}
