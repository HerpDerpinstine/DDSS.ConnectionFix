using System.Reflection;
using MelonLoader;

[assembly: AssemblyTitle(DDSS_ConnectionFix.Properties.BuildInfo.Description)]
[assembly: AssemblyDescription(DDSS_ConnectionFix.Properties.BuildInfo.Description)]
[assembly: AssemblyCompany(DDSS_ConnectionFix.Properties.BuildInfo.Company)]
[assembly: AssemblyProduct(DDSS_ConnectionFix.Properties.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + DDSS_ConnectionFix.Properties.BuildInfo.Author)]
[assembly: AssemblyTrademark(DDSS_ConnectionFix.Properties.BuildInfo.Company)]
[assembly: AssemblyVersion(DDSS_ConnectionFix.Properties.BuildInfo.Version)]
[assembly: AssemblyFileVersion(DDSS_ConnectionFix.Properties.BuildInfo.Version)]
[assembly: MelonInfo(typeof(DDSS_ConnectionFix.MelonMain), 
    DDSS_ConnectionFix.Properties.BuildInfo.Name, 
    DDSS_ConnectionFix.Properties.BuildInfo.Version,
    DDSS_ConnectionFix.Properties.BuildInfo.Author,
    DDSS_ConnectionFix.Properties.BuildInfo.DownloadLink)]
[assembly: MelonGame("StripedPandaStudios", "DDSS")]
[assembly: VerifyLoaderVersion("0.6.5", true)]
[assembly: HarmonyDontPatchAll]