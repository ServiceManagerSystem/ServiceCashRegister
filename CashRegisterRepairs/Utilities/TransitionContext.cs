using CashRegisterRepairs.Model;
using System.Linq;

namespace CashRegisterRepairs.Utilities
{
    public static class TransitionContext
    {
        public static int selectedClientIndex;
        public static Site selectedSite;
        public static Device selectedDevice;
        public static DeviceModel selectedModel;

        private static bool canOpenSubview;

        public static void DisableSubviewOpen()
        {
            canOpenSubview = true;
        }

        public static void EnableSubviewOpen()
        {
            canOpenSubview = false;
        }

        public static bool CanOpenSubview()
        {
            return (!canOpenSubview);
        }

        // Use to avoid object stuck in cache
        public static void ConsumeObjectsAfterUse(params object[] usedObjects)
        {
            usedObjects.ToList().ForEach(usedObject => usedObject = null);
        }

    }

}
