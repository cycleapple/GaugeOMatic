using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Bindings.ImGui;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.Trackers.Tracker;

namespace GaugeOMatic.GameData;

public abstract partial class ItemRef
{
    public virtual void DrawTooltip()
    {
        using (ImRaii.PushColor(ImGuiCol.PopupBg, new Vector4(0.03f, 0.03f, 0.03f, 1)))
        {
            using (var tt = ImRaii.Tooltip())
            {
                if (tt.Success) {
                    var startPos = ImGui.GetCursorPos();

                    DrawTooltipIcon(startPos);
                    using (var gr = ImRaii.Group())
                    {
                        if (gr.Success) TooltipHeaderText();
                    }

                    ImGui.SetCursorPosY(startPos.Y + (50 * GlobalScale));

                    WidgetBehaviorTable();

                    FooterContents();
                }
            }
        }
    }

    public void WidgetBehaviorTable()
    {
        ImGui.TextDisabled("元件行為");

        using var table = ImRaii.Table("BehaviorTable", 2);
        if (table.Success) {
            ImGui.TableSetupColumn("Widget");
            ImGui.TableSetupColumn("Value");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text("量條／計時器：");

            ImGui.TableNextColumn();
            PrintBarTimerDesc();

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text("計數器：");

            ImGui.TableNextColumn();
            if (UseCounterAsState())
                PrintStateDesc();
            else
                PrintCounterDesc();

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text("狀態指示器：");
            ImGui.TableNextColumn();
            PrintStateDesc();
        }
    }

    public IDalamudTextureWrap? GetIconTexture()
    {
        if (Icon == null) return null;
        try
        {
            return TextureProvider.GetFromGameIcon(new(Icon.Value))
                                  .TryGetWrap(out var texture, out _) ? texture : null;
        }
        catch
        {
            return null;
        }
    }

    public virtual void TooltipHeaderText() { }
    public virtual void DrawTooltipIcon(Vector2 startPos) { }
    public virtual bool UseCounterAsState() => false;
    public virtual void PrintBarTimerDesc() { }
    public virtual void PrintCounterDesc() { }
    public virtual void PrintStateDesc() { }
    public virtual void FooterContents() { }
    public abstract TrackerData GetTrackerData(float? preview);
}
