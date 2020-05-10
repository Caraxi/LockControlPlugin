using Dalamud.Game;
using Dalamud.Game.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockControlPlugin {
	class LockAddressResolver : BaseAddressResolver {
		public IntPtr LockFunction { get; private set; }

		protected override void Setup64Bit(SigScanner sig)
        {
            this.LockFunction = sig.ScanText("?? ?? ?? ?? ?? ?? 49 04 44 8b 51 20 41 3b d1");
        }

	}
}
