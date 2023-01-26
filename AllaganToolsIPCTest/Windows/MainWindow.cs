using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using ImGuiScene;

namespace AllaganToolsIPCTest.Windows;

public class MainWindow : Window, IDisposable
{
    private TextureWrap GoatImage;
    private Plugin Plugin;

    public MainWindow(Plugin plugin, TextureWrap goatImage) : base(
        "My Amazing Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.GoatImage = goatImage;
        this.Plugin = plugin;
    }

    public void Dispose()
    {
        this.GoatImage.Dispose();
    }

    private ulong _characterId = 0;
    private int _inventoryType = 0;
    private int _itemId = 0;
    private string _filterName = "";
    
    public override void Draw()
    {
        ImGui.Text($"The random config bool is {this.Plugin.Configuration.SomePropertyToBeSavedAndWithADefault}");

        if (ImGui.Button("Show Settings"))
        {
            this.Plugin.DrawConfigUI();
        }

        ImGui.Spacing();

        if (ImGui.Button("Get Current Character"))
        {
            PluginLog.Log(Plugin.IpcService.GetCurrentCharacter().ToString());
        }

        string characterId = _characterId.ToString();
        ImGui.Text("Character ID: ");
        if (ImGui.InputText("Character ID", ref characterId, 50))
        {
            ulong.TryParse(characterId, out _characterId);
        }

        int inventoryType = _inventoryType;
        
        ImGui.Text("Inventory Type: ");
        if (ImGui.InputInt("Inventory Type", ref inventoryType))
        {
            if (inventoryType != _inventoryType)
            {
                _inventoryType = inventoryType;
            }
        }

        if (ImGui.Button("Get Inventory Count"))
        {
            PluginLog.Log(Plugin.IpcService.GetInventoryCountByType((uint)_inventoryType, _characterId).ToString());
        }

        var filterName = _filterName;
        ImGui.Text("Filter Name: ");
        if (ImGui.InputText("Filter Name", ref filterName, 100))
        {
            if (filterName != _filterName)
            {
                _filterName = filterName;
            }
        }
        if (ImGui.Button("Toggle Filter"))
        {
            PluginLog.Log(Plugin.IpcService.ToggleBackgroundFilter(_filterName).ToString());
        }
        
        int itemId = _itemId;

        ImGui.Text("Item ID: ");
        if (ImGui.InputInt("Item ID", ref itemId))
        {
            if (itemId != _itemId)
            {
                _itemId = itemId;
            }
        }

        if (ImGui.Button("Get Item Count"))
        {
            PluginLog.Log(Plugin.IpcService.GetItemCount((uint)_itemId).ToString());
        }
    }
}
