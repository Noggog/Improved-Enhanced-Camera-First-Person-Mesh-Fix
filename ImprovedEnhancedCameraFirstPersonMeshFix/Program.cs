using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ImprovedEnhancedCameraFirstPersonMeshFix.Settings;
using System.Text.RegularExpressions;
using Mutagen.Bethesda.Plugins.Exceptions;

namespace ImprovedEnhancedCameraFirstPersonMeshFix;

public class Program
{
    static Lazy<PatcherSettings> Settings = null!;
    public static async Task<int> Main(string[] args)
    {
        return await SynthesisPipeline.Instance
            .SetAutogeneratedSettings(nickname: "Settings", path: "settings.json", out Settings)
            .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
            .SetTypicalOpen(GameRelease.SkyrimSE, "YourPatcher.esp")
            .Run(args);
    }

    public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        // set local variables
        bool maleFirstPerson = Settings.Value.MaleFirstPerson;
        bool femaleFirstPerson = Settings.Value.FemaleFirstPerson;
        var regexSources = Settings.Value.RegexSource.ToLower();
        var includedSources = Settings.Value.IncludedMods_Override.ToList();
        var excludedSources = Settings.Value.ExcludedMods_Override.ToList();

        foreach (var armorAddonGetterContext in state.LoadOrder.PriorityOrder.ArmorAddon().WinningContextOverrides())
        {
            try
            {
                var armorAddonGetter = armorAddonGetterContext.Record;
                //skip invalid
                string addon = armorAddonGetter.FormKey.ModKey.ToString().ToLower();
                addon = new Regex("[^a-z0-9]").Replace(addon, "");
                if (regexSources == "" || !Regex.IsMatch(addon, @"^" + regexSources + ".*"))
                {
                    continue;
                }

                if (excludedSources.Contains(armorAddonGetter.FormKey.ModKey))
                {
                    continue;
                }

                if (includedSources.Count > 0 && !includedSources.Contains(armorAddonGetter.FormKey.ModKey))
                {
                    continue;
                }

                if (armorAddonGetter.WorldModel == null) continue;
                if (armorAddonGetter.BodyTemplate == null) continue;

                // add or refactor for non-playable armor check (may need to loop through armor records first

                if (armorAddonGetter.BodyTemplate.FirstPersonFlags.HasFlag(IntToSlot(32)))
                {
                    var femaleWorldModel = armorAddonGetter.WorldModel.Female;
                    var maleWorldModel = armorAddonGetter.WorldModel.Male;
                    if ((femaleFirstPerson || maleFirstPerson) && (armorAddonGetter.FirstPersonModel == null ||
                                                                   armorAddonGetter.FirstPersonModel.Female == null || 
                                                                   armorAddonGetter.FirstPersonModel.Female.File.DataRelativePath.IsNull || 
                                                                   (femaleWorldModel != null && armorAddonGetter.FirstPersonModel.Female.File.DataRelativePath == femaleWorldModel.File.DataRelativePath) ||
                                                                   armorAddonGetter.FirstPersonModel.Male == null ||
                                                                   !armorAddonGetter.FirstPersonModel.Female.File.DataRelativePath.IsNull ||
                                                                   (maleWorldModel != null && armorAddonGetter.FirstPersonModel.Male.File.DataRelativePath == maleWorldModel.File.DataRelativePath)))
                    {
                        var aaNew = state.PatchMod.ArmorAddons.GetOrAddAsOverride(armorAddonGetter);
                        
                        if (aaNew.FirstPersonModel == null)
                        {
                            aaNew.FirstPersonModel = new GenderedItem<Model?>(
                                female: new Model()
                                {
                                    File = @"actors\character\character assets\1stpersonfemalebody_1.nif"
                                }, 
                                male: new Model()
                                {
                                    File = @"actors\character\character assets\1stpersonmalebody_1.nif"
                                });
                        }
                        if (femaleFirstPerson && 
                            (aaNew.FirstPersonModel.Female == null || aaNew.FirstPersonModel.Female.File.DataRelativePath.IsNull ||
                             (femaleWorldModel != null && aaNew.FirstPersonModel.Female.File.DataRelativePath == femaleWorldModel.File.DataRelativePath)))
                        {
                            aaNew.FirstPersonModel.Female = new Model()
                            {
                                File = @"actors\character\character assets\1stpersonfemalebody_1.nif", 
                                AlternateTextures = aaNew.FirstPersonModel.Female?.AlternateTextures
                            };
                        }
                        if (maleFirstPerson && 
                            (aaNew.FirstPersonModel.Male == null || !aaNew.FirstPersonModel.Male.File.DataRelativePath.IsNull || 
                             (maleWorldModel != null && aaNew.FirstPersonModel.Male.File.DataRelativePath == maleWorldModel.File.DataRelativePath)))
                        {
                            aaNew.FirstPersonModel.Male = new Model()
                            {
                                File = @"actors\character\character assets\1stpersonmalebody_1.nif", 
                                AlternateTextures = aaNew.FirstPersonModel.Male?.AlternateTextures
                            };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw RecordException.Enrich(e, armorAddonGetterContext);
            }
        }
    }

    public static BipedObjectFlag IntToSlot(int iFlag)
    {
        switch (iFlag)
        {
            case 30: return (BipedObjectFlag)0x00000001;
            case 31: return (BipedObjectFlag)0x00000002;
            case 32: return (BipedObjectFlag)0x00000004;
            case 33: return (BipedObjectFlag)0x00000008;
            case 34: return (BipedObjectFlag)0x00000010;
            case 35: return (BipedObjectFlag)0x00000020;
            case 36: return (BipedObjectFlag)0x00000040;
            case 37: return (BipedObjectFlag)0x00000080;
            case 38: return (BipedObjectFlag)0x00000100;
            case 39: return (BipedObjectFlag)0x00000200;
            case 40: return (BipedObjectFlag)0x00000400;
            case 41: return (BipedObjectFlag)0x00000800;
            case 42: return (BipedObjectFlag)0x00001000;
            case 43: return (BipedObjectFlag)0x00002000;
            case 44: return (BipedObjectFlag)0x00004000;
            case 45: return (BipedObjectFlag)0x00008000;
            case 46: return (BipedObjectFlag)0x00010000;
            case 47: return (BipedObjectFlag)0x00020000;
            case 48: return (BipedObjectFlag)0x00040000;
            case 49: return (BipedObjectFlag)0x00080000;
            case 50: return (BipedObjectFlag)0x00100000;
            case 51: return (BipedObjectFlag)0x00200000;
            case 52: return (BipedObjectFlag)0x00400000;
            case 53: return (BipedObjectFlag)0x00800000;
            case 54: return (BipedObjectFlag)0x01000000;
            case 55: return (BipedObjectFlag)0x02000000;
            case 56: return (BipedObjectFlag)0x04000000;
            case 57: return (BipedObjectFlag)0x08000000;
            case 58: return (BipedObjectFlag)0x10000000;
            case 59: return (BipedObjectFlag)0x20000000;
            case 60: return (BipedObjectFlag)0x40000000;
            case 61: return (BipedObjectFlag)0x80000000;
            default: throw new Exception(iFlag + " is not a valid armor slot.");
        }
    }
}