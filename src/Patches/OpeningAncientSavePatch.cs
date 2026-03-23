using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace AncientSL.Patches;

[HarmonyPatch(typeof(EventRoom), "OnEventStateChanged")]
internal static class OpeningAncientSavePatch
{
    private static bool Prefix(EventRoom __instance, EventModel eventModel)
    {
        if (eventModel is not AncientEventModel)
        {
            return true;
        }

        foreach (EventModel item in RunManager.Instance.EventSynchronizer.Events)
        {
            if (!item.IsFinished)
            {
                // Preserve original short-circuit while ancient choices are still resolving.
                return false;
            }
        }

        __instance.MarkPreFinished();

        if (!ShouldSkipOpeningAncientSave())
        {
            TaskHelper.RunSafely(SaveManager.Instance.SaveRun(__instance));
        }

        return false;
    }

    private static bool ShouldSkipOpeningAncientSave()
    {
        RunState? state = RunManager.Instance.DebugOnlyGetState();
        if (state == null)
        {
            return false;
        }

        if (state.ActFloor != 1)
        {
            return false;
        }

        return state.CurrentMapPointHistoryEntry?.MapPointType == MapPointType.Ancient;
    }
}
