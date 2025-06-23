using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace MountCameraFix
{
    [HarmonyPatch]
    public class MountCameraFixModSystem : ModSystem
    {
        public static ICoreAPI api;
        public Harmony harmony;

        public override void Start(ICoreAPI api)
        {
            MountCameraFixModSystem.api = api;
            // The mod is started once for the server and once for the client.
            // Prevent the patches from being applied by both in the same process.
            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                harmony = new Harmony(Mod.Info.ModID);
                harmony.PatchAll(); // Applies all harmony patches
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EntityAgent), "doMount")]
        public static bool doMount(EntityAgent __instance, IMountableSeat mountable)
        {
            api.Logger.Event("Mounting {0} unmounting {0}", mountable != null, __instance.MountedOn != null);
            if (mountable == null || mountable.Entity == null || mountable.Passenger == null) return true;
            if (__instance.MountedOn == null || __instance.MountedOn.Entity == null || __instance.MountedOn.Passenger == null) return true;
            api.Logger.Event("Mounting {0} while on {0}", mountable.Entity.EntityId, __instance.MountedOn.Entity.EntityId);
            return mountable != __instance.MountedOn;
        }


        [HarmonyTranspiler]
        [HarmonyPatch(typeof(SyncedTreeAttribute), "MarkPathDirty")]
        public static IEnumerable<CodeInstruction> markPathDirty(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions, generator)
                .Advance(39)
                .SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_S, 100))
                .InstructionEnumeration();
        }

        public override void Dispose()
        {
            harmony?.UnpatchAll(Mod.Info.ModID);
        }
    }
}
