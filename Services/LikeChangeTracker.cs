using System;
using System.Threading.Tasks;

namespace Culinary_Assistant.Services
{
    public static class LikeChangeTracker
    {
        public static int Changes { get; private set; } = 0;

        public static event Func<Task> OnThresholdReached;

        public static void RegisterChange()
        {
            Changes++;

            if (Changes >= 5)
            {
                Changes = 0;
                Trigger();
            }
        }

        private static async void Trigger()
        {
            if (OnThresholdReached != null)
                await OnThresholdReached.Invoke();
        }
    }
}
