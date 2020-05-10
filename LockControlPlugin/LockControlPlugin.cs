using Dalamud.Hooking;
using Dalamud.Plugin;
using ImGuiNET;
using System;

namespace LockControlPlugin {
	public class LockControlPlugin : IDalamudPlugin {

		public string Name => "LockControlPlugin";
		public DalamudPluginInterface PluginInterface { get; private set; }

		private LockAddressResolver Address;

		private bool started = false;
		private bool gotAddress = false;
		private bool manuallyUnlocked = false;

		public delegate ulong OnHotbarLock(IntPtr param1, uint param2, int param3);
		private Hook<OnHotbarLock> SetHotbarLock;
		private IntPtr param1Address;
		
		public int vkHotkey = 0x12;

		private int delayedStart = 0;

		public void Dispose() {
			SetHotbarLock.Disable();
		}

		private void Startup() {
			Address = new LockAddressResolver();
			Address.Setup(PluginInterface.TargetModuleScanner);
			PluginLog.Log($"Address Found: {Address.LockFunction.ToInt64():X}");
			SetHotbarLock = new Hook<OnHotbarLock>(Address.LockFunction, new OnHotbarLock(HandleOnHotbarLock));
			SetHotbarLock.Enable();
			PluginLog.Log("Hook Enabled");
			started = true;
		}

		private void Shutdown() {
			PluginLog.Log("Disabling Hook");
			started = false;
			Address = null;
			SetHotbarLock.Disable();
			SetHotbarLock = null;
		}

		public void Initialize(DalamudPluginInterface pluginInterface) {
			PluginLog.Log("Starting");
			this.PluginInterface = pluginInterface;
			pluginInterface.UiBuilder.OnBuildUi += this.BuildUI;
		}
		
		private ulong HandleOnHotbarLock(IntPtr param1, uint param2, int param3) {
			if (param2 == 0) {
				PluginLog.Log("Manual unlock triggered");
				manuallyUnlocked = true;
			} else if (param2 == 1){
				PluginLog.Log("Manual unlock released");
				manuallyUnlocked = false;
			} else {
				param1Address = param1;
				gotAddress = true;
			}
			
			return SetHotbarLock.Original(param1, param2, param3);
		}

		private void BuildUI() {
			try {

				if (started) {
					if (PluginInterface.ClientState.LocalPlayer == null) {
						Shutdown();
						return;
					}

					if (gotAddress && !manuallyUnlocked){
						if (PluginInterface.ClientState.KeyState[vkHotkey]) {
							SetHotbarLock.Original(IntPtr.Add(param1Address, 0x18A48), 0, 1);
							ImGui.BeginTooltip();
							ImGui.Text("Hotbar Unlocked!");
							ImGui.End();
						} else {
							SetHotbarLock.Original(IntPtr.Add(param1Address, 0x18A48), 1, 1);
						}
					}
				} else {
					if (PluginInterface.ClientState.LocalPlayer != null){
						if (delayedStart > 100) {
							Startup();
						} else {
							delayedStart += 1;
						}
					} else {
						delayedStart = 0;
					}
				}
			} catch (Exception ex) {
				PluginLog.LogError(ex.ToString());
			}

		}
	}
}
