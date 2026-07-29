using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Components;
using Dalamud.Bindings.ImGui;
using static GaugeOMatic.Utility.ImGuiHelpy;

namespace GaugeOMatic.Windows;

public partial class ConfigWindow
{
    private static void DrawHelpTab()
    {
        ImGui.TextDisabled("GAUGE-O-MATIC 說明");

        using var tb = ImRaii.TabBar("HelpTabs");
        if (tb.Success)
        {
            AboutTab();
            HowToTab();
        }
    }

    public const string AboutText =
        "Gauge-O-Matic 可在職業量譜中加入各種樣式的額外計數器、量條與指示器，讓你自由自訂職業量譜。" +
        "本插件仍在開發中，可能會遇到錯誤或效能問題，未來更新也可能影響已儲存的設定。\n\n" +
        "目前部分職業已內建預設，其餘仍在製作中。這些預設會挑出原始量譜沒有顯示的實用資訊，" +
        "並以符合各職業風格的方式整合至現有量譜。\n\n" +
        "歡迎提供意見！若希望職業量譜加入某種資訊、對元件設計有想法，或想分享自己製作的預設，" +
        "請前往插件的 GitHub 倉庫回報。";

    private static void AboutTab()
    {
        using var ti = ImRaii.TabItem("關於插件##AboutPlugin");
        if (ti) ImGui.TextWrapped(AboutText);
    }

    private static void HowToTab()
    {
        using var ti = ImRaii.TabItem("使用方法##HowTo");
        if (ti)
        {
            ImGui.TextWrapped("有兩種方式可將元素加入職業量譜：手動設定，或載入預設。");

            ImGui.Spacing();
            ImGui.TextDisabled("手動新增元素");

            ImGui.Text("1）點擊");
            ImGui.SameLine();
            ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Plus,"新增##dummyAdd");

            ImGui.Spacing();
            ImGui.Text("2）選擇要使用的追蹤器，共分為三類：");

            ImGui.Indent(30f);
            WriteIcon(FontAwesomeIcon.Tags, null, 0x1c6e68ff);
            ImGui.Text("狀態效果－取得剩餘時間或層數。");

            WriteIcon(FontAwesomeIcon.FistRaised, null, 0xaa372dff);
            ImGui.Text("技能－取得復唱時間或可用次數。");

            WriteIcon(FontAwesomeIcon.Gauge, null, 0x2b455cff);
            ImGui.Text("職業量譜－取得各職業量譜特有的資料。");

            ImGui.Indent(-30f);

            ImGui.Spacing();
            ImGui.TextWrapped("3）選擇元件。元件分為以下類別：");

            ImGui.Indent(30f);
            ImGui.TextWrapped(
                              "計數器－顯示層數或可用次數\n" +
                              "量條與計時器－顯示時間或資源數值\n" +
                              "狀態指示器－在不同視覺狀態間切換（通常為開／關）\n" +
                              "多元件－一組可彼此疊加的元件，組合成完整設計。");

            ImGui.Indent(-30f);

            ImGui.Spacing();
            ImGui.TextWrapped("4）開始自訂！");
            ImGui.Indent(30f);

            ImGui.TextWrapped("每種元件都有專屬選項，可調整外觀與行為。你也可以選擇要將元件固定至哪個 HUD 元素，" +
                              "並控制元件彼此疊加的順序。");

            ImGui.Indent(-30f);

            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.TextDisabled("使用預設");
            ImGui.TextWrapped(
                "開啟預設視窗後會看到已安裝的預設清單。選擇任一預設即可查看內容。\n\n" +
                "你可以：");

            ImGui.Indent(30f);

            ImGui.Text("－個別加入預設中的元素");
            ImGui.SameLine();
            IconButton("", FontAwesomeIcon.Plus, 10f);

            ImGui.Text("－一次加入所有元素");
            ImGui.SameLine();
            ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Plus,"全部加入至……##dummyAddAll");

            ImGui.Text("－將設計複製到現有追蹤器");
            ImGui.SameLine();
            IconButton("", FontAwesomeIcon.Copy, 10f);

            ImGui.Text("－以預設內容取代目前設定");
            ImGui.SameLine();
            ImGuiComponents.IconButtonWithText(FontAwesomeIcon.PaintRoller,"覆寫目前設定##dummyOverwrite");


            ImGui.Indent(-30f);
            ImGui.TextWrapped("若預設包含不適用於所選職業的追蹤器，該項目會變灰，但仍可使用其元件設計。");


            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.TextDisabled("新增預設");
            ImGui.TextWrapped("若要將目前選項儲存為預設，輸入名稱後點擊");
            ImGui.SameLine();
            ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Save,"儲存##dummySave");

            ImGui.TextWrapped("若已從外部來源複製預設至剪貼簿，可點擊");
            ImGui.SameLine();
            ImGuiComponents.IconButtonWithText(FontAwesomeIcon.SignInAlt,"從剪貼簿匯入##dummyImport");
        }
    }
}
