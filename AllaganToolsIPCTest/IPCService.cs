using System;
using System.Collections.Generic;
using System.Timers;
using Dalamud.Logging;
using Dalamud.Plugin.Ipc;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AllaganToolsIPCTest
{
    public class IPCService : IDisposable
    {
        private ICallGateSubscriber<bool>? _isInitialized;
        private ICallGateSubscriber<ulong>? _currentCharacter;
        private ICallGateSubscriber<uint, ulong?, uint>? _inventoryCountByType;
        private ICallGateSubscriber<string, bool>? _toggleBackgroundFilter;
        private ICallGateSubscriber<bool, bool> _atAvailable;
        private ICallGateSubscriber<uint, ulong, uint?, uint> _itemCount;
        
        private ICallGateSubscriber<(uint, InventoryItem.ItemFlags, ulong, uint), bool>? _itemAdded;
        private ICallGateSubscriber<(uint, InventoryItem.ItemFlags, ulong, uint), bool>? _itemRemoved;
        private bool _ATRegistered = false;

        public IPCService()
        {
            _isInitialized = Service.Interface.GetIpcSubscriber<bool>("AllaganTools.IsInitialized");
            _currentCharacter = Service.Interface.GetIpcSubscriber<ulong>("AllaganTools.CurrentCharacter");
            _inventoryCountByType = Service.Interface.GetIpcSubscriber<uint, ulong?, uint>("AllaganTools.InventoryCountByType");
            _toggleBackgroundFilter = Service.Interface.GetIpcSubscriber<string, bool>("AllaganTools.ToggleBackgroundFilter");
            _itemCount = Service.Interface.GetIpcSubscriber<uint, ulong, uint?, uint>("AllaganTools.ItemCount");
            _itemAdded = Service.Interface.GetIpcSubscriber<(uint, InventoryItem.ItemFlags, ulong, uint), bool>("AllaganTools.ItemAdded");
            _itemRemoved = Service.Interface.GetIpcSubscriber<(uint, InventoryItem.ItemFlags, ulong, uint), bool>("AllaganTools.ItemRemoved");

            try
            {
                var isInitialized = _isInitialized.InvokeFunc();
                if (isInitialized)
                {
                    PluginLog.Log("AT IPC registered.");
                    _ATRegistered = true;
                    Subscribe();
                }
                else
                {
                    PluginLog.Log("AT IPC could not be registered.");
                }
            }
            catch (Exception e)
            {
                PluginLog.Log("Exception while trying to register IPC.");
                PluginLog.Error(e.Message);
                _ATRegistered = false;
            }

            _atAvailable = Service.Interface.GetIpcSubscriber<bool, bool>("AllaganTools.Initialized");
            _atAvailable.Subscribe(LateInitilization);
        }

        private void Subscribe()
        {
            _itemAdded?.Unsubscribe(ItemAdded);
            _itemRemoved?.Unsubscribe(ItemRemoved);
            PluginLog.Log("Subscribing to Allagan Tool's events.");
            _itemAdded?.Subscribe(ItemAdded);
            _itemRemoved?.Subscribe(ItemRemoved);
        }

        private void ItemAdded((uint, InventoryItem.ItemFlags, ulong, uint) changedItem)
        {
            Service.Framework.RunOnFrameworkThread(() =>
            {
                PluginLog.Log("Item Received: " + changedItem.Item1 + " - " + changedItem.Item4 + " - " +
                              changedItem.Item3 + " - " + changedItem.Item3.ToString());
            });
        }

        private void ItemRemoved((uint, InventoryItem.ItemFlags, ulong, uint) changedItem)
        {
            Service.Framework.RunOnFrameworkThread(() =>
            {
                PluginLog.Log("Item Removed: " + changedItem.Item1 + " - " + changedItem.Item4 + " - " +
                              changedItem.Item3 + " - " + changedItem.Item3.ToString());
            });
        }

        private void LateInitilization(bool initalized)
        {
            Service.Framework.RunOnFrameworkThread(() =>
            {
                PluginLog.Log("AT IPC registered late.");
                _ATRegistered = initalized;
                if (initalized)
                {
                    Subscribe();
                }
            });
        }

        public uint GetItemCount(uint itemId, ulong? characterId = null)
        {
            if (!_ATRegistered)
            {
                return 0;
            }

            try
            {
                return _itemCount?.InvokeFunc(itemId, characterId ?? Service.ClientState.LocalContentId, 0) ?? 0;
            }
            catch(Exception e)
            {
                PluginLog.Log("Failed to call AllaganTools.GetItemCount.");
                PluginLog.Error(e.Message);
            }
            return 0;
        }

        public ulong GetCurrentCharacter()
        {
            if (!_ATRegistered)
            {
                return 0;
            }
            try
            {
                return _currentCharacter?.InvokeFunc() ?? 0;
            }
            catch(Exception e)
            {
                PluginLog.Log("Failed to call AllaganTools.CurrentCharacter.");
                PluginLog.Error(e.Message);
            }
            return 0;
        }

        public uint GetInventoryCountByType(uint inventoryType, ulong? characterId)
        {
            if (!_ATRegistered)
            {
                return 0;
            }
            
            try
            {
                return _inventoryCountByType?.InvokeFunc(inventoryType, characterId) ?? 0;
            }
            catch(Exception e)
            {
                PluginLog.Log("Failed to call AllaganTools.InventoryCountByType.");
                PluginLog.Error(e.Message);
            }
            return 0;
        }

        public bool ToggleBackgroundFilter(string filterNameOrKey)
        {
            if (!_ATRegistered)
            {
                return false;
            }
            
            try
            {
                return _toggleBackgroundFilter?.InvokeFunc(filterNameOrKey) ?? false;
            }
            catch(Exception e)
            {
                PluginLog.Log("Failed to call AllaganTools.ToggleBackgroundFilter.");
                PluginLog.Error(e.Message);
            }
            return false;
        }

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed && disposing)
            {
                PluginLog.Log("AT IPC unregistered.");
                _atAvailable.Unsubscribe(LateInitilization);
                _itemAdded?.Unsubscribe(ItemAdded);
                _itemRemoved?.Unsubscribe(ItemRemoved);
            }
            _disposed = true;         
        }
    }
}
