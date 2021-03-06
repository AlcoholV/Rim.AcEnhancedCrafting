﻿using System.Linq;
using System.Reflection;
using AlcoholV.Detouring;
using Verse;

namespace AlcoholV
{
    [StaticConstructorOnStartup]
    public class AcEnhancedCrafting
    {
        private static readonly BindingFlags[] BindingFlagCombos =
        {
            BindingFlags.Instance | BindingFlags.Public, BindingFlags.Static | BindingFlags.Public,
            BindingFlags.Instance | BindingFlags.NonPublic, BindingFlags.Static | BindingFlags.NonPublic
        };


        static AcEnhancedCrafting()
        {
            LongEventHandler.QueueLongEvent(Inject, "Initializing", true, null);
        }

        public static Assembly Assembly => Assembly.GetAssembly(typeof(AcEnhancedCrafting));

        public static string AssemblyName => Assembly.FullName.Split(',').First();

        private static void Inject()
        {
            #region Automatic hookup

            // Loop through all detour attributes and try to hook them up
            foreach (var targetType in Assembly.GetTypes())
                foreach (var bindingFlags in BindingFlagCombos)
                    foreach (var targetMethod in targetType.GetMethods(bindingFlags))
                        foreach (DetourAttribute detour in targetMethod.GetCustomAttributes(typeof(DetourAttribute), true))
                        {
                            var flags = detour.bindingFlags != default(BindingFlags) ? detour.bindingFlags : bindingFlags;
                            var sourceMethod = detour.source.GetMethod(targetMethod.Name, flags);
                            if (sourceMethod == null)
                                Log.Error(string.Format(AssemblyName + " Can't find source method {0} with bindingflags {1}", targetMethod.Name, flags));
                            if (!Detours.TryDetourFromTo(sourceMethod, targetMethod)) Log.Message(AssemblyName + " Failed to get injected properly.");
                        }

            #endregion

            Log.Message(AssemblyName + " injected.");
        }
    }
}